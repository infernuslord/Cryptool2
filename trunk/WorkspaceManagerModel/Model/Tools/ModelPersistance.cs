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
    public class ModelPersistance
    {
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        
        /// <summary>
        /// Deserializes a model from the given file with the given filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public WorkspaceModel loadModel(string filename)
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
            //if not generate new ConnectorModels
            foreach (PluginModel pluginModel in workspacemodel.AllPluginModels)
            {
                var connectorModels = new List<ConnectorModel>();
                connectorModels.AddRange(pluginModel.InputConnectors);
                connectorModels.AddRange(pluginModel.OutputConnectors);
                //Check if a property of a ConnectorModel was deleted or its type changed => delete the ConnectorModel););
                foreach (ConnectorModel connectorModel in new List<ConnectorModel>(connectorModels))
                {
                    var propertyInfo = connectorModel.PluginModel.Plugin.GetType().GetProperty(connectorModel.PropertyName);
                    if (propertyInfo == null ||
                        !connectorModel.ConnectorType.Equals(propertyInfo.PropertyType))
                    {
                        //the property belonging to this ConnectorModel was not found
                        //or the type of the saved property differs to the real one
                        //so we delete the connector
                        pluginModel.WorkspaceModel.deleteConnectorModel(connectorModel);
                        connectorModels.Remove(connectorModel);
                        GuiLogMessage(string.Format("A property with name '{0}' of type '{1}' does not exist in '{2}:{3}' but a ConnectorModel exists in the PluginModel. Delete the ConnectorModel now.", connectorModel.PropertyName,connectorModel.ConnectorType.Name, pluginModel.PluginType, pluginModel.Name),
                                      NotificationLevel.Warning);
                    }
                }
                //Check if there are properties which have no own ConnectorModel
                foreach(PropertyInfoAttribute propertyInfoAttribute in pluginModel.Plugin.GetProperties())
                {
                    var query = from c in connectorModels
                                where c.PropertyName.Equals(propertyInfoAttribute.PropertyName)
                                select c;
                    if(query.Count()==0)
                    {
                        //we found a property which has no ConnectorModel, so we create a new one
                        pluginModel.generateConnector(propertyInfoAttribute);
                        GuiLogMessage(string.Format("A ConnectorModel for the plugins property '{0}' of type '{1}' does not exist in the PluginModel of '{2}:{3}'. Create a ConnectorModel now.", propertyInfoAttribute.PropertyName,propertyInfoAttribute.PropertyInfo.PropertyType.Name, pluginModel.PluginType, pluginModel.Name),
                                      NotificationLevel.Warning);
                    }
                }                        
            }                        

            //initialize the plugins
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

            workspacemodel.UndoRedoManager.ClearStacks();
            return workspacemodel;          
        }

        /// <summary>
        /// Serializes the given model to a file with the given filename
        /// </summary>
        /// <param name="workspaceModel"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public void saveModel(WorkspaceModel workspaceModel, string filename)
        {
            PersistantModel persistantModel = new PersistantModel();
            persistantModel.WorkspaceModel = workspaceModel;            

            //Save all Settings of each Plugin
            foreach (PluginModel pluginModel in workspaceModel.AllPluginModels){

                if (pluginModel.Plugin.Settings != null)
                {
                    pluginModel.SettingesHaveChanges = false;
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
            workspaceModel.UndoRedoManager.SavedHere = true;
        }

        /// <summary>
        /// Loggs a gui message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        internal void GuiLogMessage(string message, NotificationLevel level)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                var args = new GuiLogEventArgs(message, null, level);
                args.Title = "-";
                OnGuiLogNotificationOccured(null, args);
            }
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
