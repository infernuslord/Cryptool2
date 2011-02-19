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
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using Cryptool.PluginBase;
using XMLSerialization;
using Cryptool.PluginBase.Editor;

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
        public static WorkspaceModel loadModel(string filename, IEditor workspaceManagerEditor)
        {
            PersistantModel persistantModel = (PersistantModel)XMLSerialization.XMLSerialization.Deserialize(filename,true);
            WorkspaceModel workspacemodel = persistantModel.WorkspaceModel;
            workspacemodel.Editor = workspaceManagerEditor;            

            //restore all settings of each plugin
            foreach (PersistantPlugin persistantPlugin in persistantModel.PersistantPluginList)
            {
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
                            //workspaceManagerEditor.GuiLogMessage("Could not restore the setting \"" + persistantSetting.Name + "\" of plugin \"" + persistantPlugin.PluginModel.Name + "\" because of:" + ex.Message,NotificationLevel.Warning);
                        }
                    }
                }
            }

            //connect all listener for plugins/plugin models
            foreach (PluginModel pluginModel in workspacemodel.AllPluginModels)
            {
                try
                {
                    pluginModel.Plugin.Initialize();
                }
                catch(Exception ex)
                {
                    //workspaceManagerEditor.GuiLogMessage("Error while initializing \"" + pluginModel.Name + "\". Surely plugin will not work well. Error was:" + ex.Message,NotificationLevel.Error);
                }
                //pluginModel.Plugin.OnGuiLogNotificationOccured += workspaceManagerEditor.GuiLogNotificationOccured;
                //pluginModel.Plugin.OnGuiLogNotificationOccured += pluginModel.GuiLogNotificationOccured;
                pluginModel.Plugin.OnPluginProgressChanged += pluginModel.PluginProgressChanged;                
                pluginModel.Plugin.OnPluginStatusChanged += pluginModel.PluginStatusChanged;                
                if (pluginModel.Plugin.Settings != null)
                {
                    //pluginModel.Plugin.Settings.PropertyChanged += pluginModel.SettingsPropertyChanged;
                }
            }
                
            //connect all listeners for connectors
            foreach (ConnectorModel connectorModel in workspacemodel.AllConnectorModels)
            {
                if (connectorModel.IsDynamic == true)
                {
                    DynamicPropertyInfoAttribute dynamicPropertyInfoAttribute = connectorModel.PluginModel.Plugin.GetDynamicPropertyInfo();
                    EventInfo eventinfo = connectorModel.PluginModel.PluginType.GetEvent(dynamicPropertyInfoAttribute.UpdateDynamicPropertiesEvent);
                    eventinfo.AddEventHandler(connectorModel.PluginModel.Plugin, new DynamicPropertiesChanged(connectorModel.PropertyTypeChangedOnPlugin));
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
                        if (to.IsDynamic)
                        {
                            data =
                                to.PluginModel.Plugin.GetType().GetMethod(to.DynamicGetterName).Invoke(
                                    to.PluginModel.Plugin, new object[] {to.PropertyName});
                        }
                        else
                        {
                            data =
                                to.PluginModel.Plugin.GetType().GetProperty(to.PropertyName).GetValue(
                                    to.PluginModel.Plugin, null);
                        }

                        //Set IControl data
                        if (from.IsDynamic)
                        {
                            MethodInfo propertyInfo = from.PluginModel.Plugin.GetType().GetMethod(from.DynamicSetterName);
                            propertyInfo.Invoke(from.PluginModel.Plugin, new object[] {from.PropertyName, data});
                        }
                        else
                        {
                            PropertyInfo propertyInfo = from.PluginModel.Plugin.GetType().GetProperty(from.PropertyName);
                            propertyInfo.SetValue(from.PluginModel.Plugin, data, null);
                        }
                    }
                }catch(Exception ex)
                {
                    //workspaceManagerEditor.GuiLogMessage("Error while restoring IControl Connection between \"" + from.PluginModel.Name + "\" to \"" + to.PluginModel.Name + "\". Workspace surely will not work well. Error was:" + ex.Message, NotificationLevel.Error);
                }
            }

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
