/*                              
   Copyright 2010 Nils Kopal

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cryptool.PluginBase;

namespace WorkspaceManager.Model
{
    /// <summary>
    /// Class with static methods for loading and saving of WorkspaceModels
    /// </summary>
    public static class ModelPersistance
    {
        /// <summary>
        /// Deserializes a model from the given file with the given filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static WorkspaceModel loadModel(string filename)
        {
            PersistantModel persistantModel = (PersistantModel)XMLSerialization.XMLSerialization.Deserialize(filename,true);
            WorkspaceModel workspacemodel = persistantModel.WorkspaceModel;
            
            //restore all settings of each plugin
            foreach (PersistantPlugin persistantPlugin in persistantModel.PersistantPluginList)
            {
                if (persistantPlugin.PluginModel.Plugin.Settings == null)
                    continue; // do not attempt deserialization if plugin type has no settings
                    

                foreach (PersistantSetting persistantSetting in persistantPlugin.PersistantSettingsList)
                {

                    PropertyInfo[] arrpInfo = persistantPlugin.PluginModel.Plugin.Settings.GetType().GetProperties();
                    foreach (PropertyInfo pInfo in arrpInfo)
                    {
                        try
                        {
                            DontSaveAttribute[] dontSave =
                                (DontSaveAttribute[]) pInfo.GetCustomAttributes(typeof (DontSaveAttribute), false);
                            if (dontSave.Length == 0)
                            {
                                if (pInfo.Name.Equals(persistantSetting.Name))
                                {
                                    if (persistantSetting.Type.Equals("System.String"))
                                    {
                                        pInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings,
                                                       (String) persistantSetting.Value, null);
                                    }
                                    else if (persistantSetting.Type.Equals("System.Int16"))
                                    {
                                        pInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings,
                                                       System.Int16.Parse((String) persistantSetting.Value), null);
                                    }
                                    else if (persistantSetting.Type.Equals("System.Int32"))
                                    {
                                        pInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings,
                                                       System.Int32.Parse((String) persistantSetting.Value), null);
                                    }
                                    else if (persistantSetting.Type.Equals("System.Int64"))
                                    {
                                        pInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings,
                                                       System.Int64.Parse((String) persistantSetting.Value), null);
                                    }
                                    else if (persistantSetting.Type.Equals("System.Double"))
                                    {
                                        pInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings,
                                                       System.Double.Parse((String) persistantSetting.Value), null);
                                    }
                                    else if (persistantSetting.Type.Equals("System.Boolean"))
                                    {
                                        pInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings,
                                                       System.Boolean.Parse((String) persistantSetting.Value), null);
                                    }
                                    else if (pInfo.PropertyType.IsEnum)
                                    {
                                        Int32 result = 0;
                                        System.Int32.TryParse((String) persistantSetting.Value, out result);
                                        object newEnumValue = Enum.ToObject(pInfo.PropertyType, result);
                                        pInfo.SetValue(persistantPlugin.PluginModel.Plugin.Settings, newEnumValue, null);
                                    }
                                }
                            }
                        }catch(Exception ex)
                        {
                            throw new Exception("Could not restore the setting \"" + persistantSetting.Name + "\" of plugin \"" + persistantPlugin.PluginModel.Name + "\"", ex);
                        }
                    }
                }
            }

            //check if all properties belonging to its ConnectorModels really exist and if each property has a ConnectorModel
            //if not generate new connector models
            foreach (PluginModel pluginModel in workspacemodel.AllPluginModels)
            {
                bool refreshConnectorModels = false;
                IEnumerable<ConnectorModel> connectorModels = (new List<ConnectorModel>(pluginModel.OutputConnectors)).Concat(pluginModel.InputConnectors);
                foreach (ConnectorModel connectorModel in connectorModels)
                {
                    if (connectorModel.PluginModel.Plugin.GetType().GetProperty(connectorModel.PropertyName) == null)
                    {
                        //A connector does not exist, so we reset all connectors of this pluginmodel
                        refreshConnectorModels = true;
                        break;
                    }
                }
                foreach(PropertyInfoAttribute propertyInfoAttribute in pluginModel.Plugin.GetProperties())
                {
                    var query = from c in connectorModels
                                where c.PropertyName.Equals(propertyInfoAttribute.PropertyName)
                                select c;
                    if(query.Count()==0)
                    {
                        refreshConnectorModels = true;
                        break;
                    }
                }
                if(refreshConnectorModels)
                {                    
                    foreach (ConnectorModel connectorModel in connectorModels)
                    {                        
                        workspacemodel.deleteConnectorModel(connectorModel);
                    }
                    pluginModel.generateConnectors();
                }                
            }

            if(workspacemodel.UndoRedoManager.CanUndo())
            {
                workspacemodel.UndoRedoManager.ClearStacks();
            }

            //connect all listener for plugins/plugin models            
            foreach (PluginModel pluginModel in workspacemodel.AllPluginModels)
            {
                try
                {
                    pluginModel.Plugin.Initialize();
                    pluginModel.PercentageFinished = 0;
                }
                catch(Exception ex)
                {
                    throw new Exception("Error while initializing \"" + pluginModel.Name + "\".", ex);
                }
                pluginModel.Plugin.OnGuiLogNotificationOccured += workspacemodel.GuiLogMessage;
                pluginModel.Plugin.OnPluginProgressChanged += pluginModel.PluginProgressChanged;                
                pluginModel.Plugin.OnPluginStatusChanged += pluginModel.PluginStatusChanged;                
                if (pluginModel.Plugin.Settings != null)
                {
                    pluginModel.Plugin.Settings.PropertyChanged += pluginModel.SettingsPropertyChanged;
                }
            }                
            
            foreach (ConnectorModel connectorModel in workspacemodel.AllConnectorModels)
            {
                //refresh language stuff
                foreach (var property in connectorModel.PluginModel.Plugin.GetProperties())
                {
                    if (property.PropertyName.Equals(connectorModel.PropertyName))
                    {
                        connectorModel.ToolTip = property.ToolTip;
                        connectorModel.Caption = property.Caption;
                        break;
                    }
                }           
                connectorModel.PluginModel.Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
            }

            //restore all IControls
            foreach (ConnectionModel connectionModel in workspacemodel.AllConnectionModels) 
            {
                ConnectorModel from = connectionModel.From;
                ConnectorModel to = connectionModel.To;
                try
                {
                    if (from.IControl && to.IControl)
                    {
                        object data = null;
                        //Get IControl data from "to"                       
                        data = to.PluginModel.Plugin.GetType().GetProperty(to.PropertyName).GetValue(to.PluginModel.Plugin, null);                                                
                        PropertyInfo propertyInfo = from.PluginModel.Plugin.GetType().GetProperty(from.PropertyName);
                        propertyInfo.SetValue(from.PluginModel.Plugin, data, null);

                    }
                }
                catch(Exception ex)
                {
                    throw new Exception("Error while restoring IControl Connection between \"" + from.PluginModel.Name + "\" to \"" + to.PluginModel.Name + "\". Workspace surely will not work well.",ex);
                }
            }

            workspacemodel.HasChanges = false;
            return workspacemodel;          
        }

        /// <summary>
        /// Serializes the given model to a file with the given filename
        /// </summary>
        /// <param name="workspaceModel"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static void saveModel(WorkspaceModel workspaceModel, string filename)
        {
            PersistantModel persistantModel = new PersistantModel();
            persistantModel.WorkspaceModel = workspaceModel;            

            //Save all Settings of each Plugin
            foreach (PluginModel pluginModel in workspaceModel.AllPluginModels){

                if (pluginModel.Plugin.Settings != null)
                {
                    PropertyInfo[] arrpInfo = pluginModel.Plugin.Settings.GetType().GetProperties();

                    PersistantPlugin persistantPlugin = new PersistantPlugin();
                    persistantPlugin.PluginModel = pluginModel;

                    foreach (PropertyInfo pInfo in arrpInfo)
                    {
                        DontSaveAttribute[] dontSave = (DontSaveAttribute[])pInfo.GetCustomAttributes(typeof(DontSaveAttribute), false);
                        if (pInfo.CanWrite && dontSave.Length == 0)
                        {
                            PersistantSetting persistantSetting = new PersistantSetting();
                            if (pInfo.PropertyType.IsEnum)
                            {
                                persistantSetting.Value = "" + pInfo.GetValue(pluginModel.Plugin.Settings, null).GetHashCode();
                            }
                            else
                            {
                                persistantSetting.Value = "" + pInfo.GetValue(pluginModel.Plugin.Settings, null);
                            }
                            persistantSetting.Name = pInfo.Name;
                            persistantSetting.Type = pInfo.PropertyType.FullName;
                            persistantPlugin.PersistantSettingsList.Add(persistantSetting);
                        }

                    }
                    persistantModel.PersistantPluginList.Add(persistantPlugin);
                }
            }
            XMLSerialization.XMLSerialization.Serialize(persistantModel, filename,true);
            workspaceModel.HasChanges = false;
        }
    }

    /// <summary>
    /// Class for persisting a workspace model
    /// stores the model and a list of persistant plugin models
    /// </summary>
    [Serializable]
    public class PersistantModel{
        public WorkspaceModel WorkspaceModel{get;set;}
        public List<PersistantPlugin> PersistantPluginList = new List<PersistantPlugin>();
    }

    /// <summary>
    /// Class for persisting a plugin model
    /// stores the plugin model and a list of settings
    /// </summary>
    [Serializable]
    public class PersistantPlugin
    {
        public PluginModel PluginModel { get; set; }
        public List<PersistantSetting> PersistantSettingsList = new List<PersistantSetting>();
    }

    /// <summary>
    /// Class for persisting settings
    /// stores the name, the type and the value of the setting
    /// </summary>
    [Serializable]
    public class PersistantSetting
    {
        public string Name {get;set;}
        public string Type { get; set; }
        public string Value { get; set; }
    }

}
