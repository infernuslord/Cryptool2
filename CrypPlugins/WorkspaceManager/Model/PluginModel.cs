/*                              
   Copyright 2010 Nils Kopal, Viktor M.

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
using Cryptool.PluginBase;
using System.Threading;
using System.Windows.Controls;
using Gears4Net;
using WorkspaceManager.Execution;
using System.Windows.Threading;
using Cryptool.PluginBase.IO;
using System.Reflection;
using System.ComponentModel;

namespace WorkspaceManager.Model
{
    /// <summary>
    /// Class to represent and wrap a IPlugin in our model graph
    /// </summary>
    [Serializable]
    public class PluginModel : VisualElementModel
    {
        #region private members

        [NonSerialized]
        private PluginProtocol pluginProtocol;
        [NonSerialized]
        private IPlugin plugin;         
        private int imageIndex = 0;
        [NonSerialized]
        private PluginModelState state = PluginModelState.Normal;
        private string PluginTypeName = null;
        private string PluginTypeAssemblyName = null;
        
        #endregion

        #region public members

        /// <summary>
        /// State of the Plugin
        /// </summary>
        
        public PluginModelState State {
            get { return state; } 
            set { state = value; }
        }

        /// <summary>
        /// All ingoing connectors of this PluginModel
        /// </summary>
        public List<ConnectorModel> InputConnectors = null;

        /// <summary>
        /// All outgoing connectors of this PluginModel
        /// </summary>
        public List<ConnectorModel> OutputConnectors = null;

        /// <summary>
        /// The wrapped IPlugin of this PluginModel
        /// if there is currently no plugin instance it
        /// will automatically create one. Otherwise
        /// this acts as singleton and returns the created
        /// instance
        /// </summary>        
        public IPlugin Plugin{
            get { 
                if(plugin==null && PluginType != null){
                    plugin = PluginType.CreateObject();                    
                }
                return plugin;
            }

            private set
            {
                plugin = value;
            }
        }

        /// <summary>
        /// The Type of the Wrapped IPlugin of this PluginModel
        /// Depending on this the Plugin of this PluginModel will be instanciated
        /// </summary>        
        public Type PluginType{
            get{
                if (this.PluginTypeName != null){
                    Assembly assembly = Assembly.Load(PluginTypeAssemblyName);
                    Type t = assembly.GetType(PluginTypeName);
                    return t;
                }
                else
                {
                    return null;
                }
            } 
            set{
                this.PluginTypeName = value.FullName;
                this.PluginTypeAssemblyName = value.Assembly.FullName;
            }
        }

        /// <summary>
        /// Should this plugin may be startet again when it
        /// is startable?
        /// </summary>
        public bool RepeatStart;

        /// <summary>
        /// Is the wrapped plugin startable
        /// </summary>
        public bool Startable;
        
        /// <summary>
        /// Is the Plugin actually minimized?
        /// </summary>
        public bool Minimized { get; set; }

        /// <summary>
        /// The execution state of the progress of the wrapped plugin 
        /// </summary>
        public double PercentageFinished { get; set; }

        /// <summary>
        /// Create a new PluginModel
        /// </summary>
        public PluginModel()
        {
            this.InputConnectors = new List<ConnectorModel>();
            this.OutputConnectors = new List<ConnectorModel>();
        }
      
        /// <summary>
        /// The WorkspaceModel of this PluginModel
        /// </summary>
        public WorkspaceModel WorkspaceModel { get; set; }

        /// <summary>
        /// Generates all Connectors of this Plugin.
        /// </summary>
        public void generateConnectors()
        {

            if (Plugin != null)
            {   
                foreach (PropertyInfoAttribute propertyInfoAttribute in Plugin.GetProperties())
                {
                    if (propertyInfoAttribute.Direction.Equals(Direction.InputData))
                    {
                        ConnectorModel connectorModel = new ConnectorModel();
                        connectorModel.ConnectorType = propertyInfoAttribute.PropertyInfo.PropertyType;
                        connectorModel.WorkspaceModel = WorkspaceModel;
                        connectorModel.PluginModel = this;
                        connectorModel.IsMandatory = propertyInfoAttribute.Mandatory;
                        connectorModel.PropertyName = propertyInfoAttribute.PropertyName;
                        connectorModel.Name = propertyInfoAttribute.PropertyName;
                        connectorModel.ToolTip = propertyInfoAttribute.ToolTip;
                        connectorModel.IControl = false;
                        InputConnectors.Add(connectorModel);
                        WorkspaceModel.AllConnectorModels.Add(connectorModel);
                    }
                    else if (propertyInfoAttribute.Direction.Equals(Direction.ControlSlave))
                    {
                        ConnectorModel connectorModel = new ConnectorModel();
                        connectorModel.ConnectorType = propertyInfoAttribute.PropertyInfo.PropertyType;
                        connectorModel.WorkspaceModel = WorkspaceModel;
                        connectorModel.PluginModel = this;
                        connectorModel.IsMandatory = propertyInfoAttribute.Mandatory;
                        connectorModel.PropertyName = propertyInfoAttribute.PropertyName;
                        connectorModel.Name = propertyInfoAttribute.PropertyName;
                        connectorModel.ToolTip = propertyInfoAttribute.ToolTip;
                        connectorModel.IControl = true;
                        InputConnectors.Add(connectorModel);
                        WorkspaceModel.AllConnectorModels.Add(connectorModel);
                    }
                    else if (propertyInfoAttribute.Direction.Equals(Direction.OutputData))
                    {
                        ConnectorModel connectorModel = new ConnectorModel();
                        connectorModel.ConnectorType = propertyInfoAttribute.PropertyInfo.PropertyType;
                        connectorModel.WorkspaceModel = WorkspaceModel;
                        connectorModel.PluginModel = this;
                        connectorModel.IsMandatory = propertyInfoAttribute.Mandatory;
                        connectorModel.PropertyName = propertyInfoAttribute.PropertyName;
                        connectorModel.Name = propertyInfoAttribute.PropertyName;
                        connectorModel.ToolTip = propertyInfoAttribute.ToolTip;
                        connectorModel.Outgoing = true;
                        connectorModel.IControl = false;
                        Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
                        OutputConnectors.Add(connectorModel);
                        WorkspaceModel.AllConnectorModels.Add(connectorModel);
                    }
                    else if (propertyInfoAttribute.Direction.Equals(Direction.ControlMaster))
                    {
                        ConnectorModel connectorModel = new ConnectorModel();
                        connectorModel.ConnectorType = propertyInfoAttribute.PropertyInfo.PropertyType;
                        connectorModel.WorkspaceModel = WorkspaceModel;
                        connectorModel.PluginModel = this;
                        connectorModel.IsMandatory = propertyInfoAttribute.Mandatory;
                        connectorModel.PropertyName = propertyInfoAttribute.PropertyName;
                        connectorModel.Name = propertyInfoAttribute.PropertyName;
                        connectorModel.ToolTip = propertyInfoAttribute.ToolTip;
                        connectorModel.Outgoing = true;
                        connectorModel.IControl = true;
                        Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
                        OutputConnectors.Add(connectorModel);
                        WorkspaceModel.AllConnectorModels.Add(connectorModel);
                    }
                }

                Dictionary<string, DynamicProperty> dictionary = Plugin.GetDynamicPropertyList();
                if (dictionary != null)
                {
                    DynamicPropertyInfoAttribute dynamicPropertyInfoAttribute = Plugin.GetDynamicPropertyInfo();
                    foreach (DynamicProperty dynamicProperty in dictionary.Values)
                    {

                        if (dynamicProperty.PInfo.Direction.Equals(Direction.InputData))
                        {
                            ConnectorModel connectorModel = new ConnectorModel();
                            connectorModel.ConnectorType = dynamicProperty.Type;
                            connectorModel.WorkspaceModel = WorkspaceModel;
                            connectorModel.PluginModel = this;
                            connectorModel.IsMandatory = dynamicProperty.PInfo.Mandatory;
                            connectorModel.PropertyName = dynamicProperty.Name;
                            connectorModel.Name = dynamicProperty.Name;
                            connectorModel.ToolTip = dynamicProperty.PInfo.ToolTip;
                           EventInfo eventinfo = Plugin.GetType().GetEvent(dynamicPropertyInfoAttribute.UpdateDynamicPropertiesEvent);
                            connectorModel.IsDynamic = true;
                            connectorModel.DynamicGetterName = dynamicPropertyInfoAttribute.MethodGetValue;
                            connectorModel.DynamicSetterName = dynamicPropertyInfoAttribute.MethodSetValue;
                            eventinfo.AddEventHandler(Plugin, new DynamicPropertiesChanged(connectorModel.PropertyTypeChangedOnPlugin));
                            InputConnectors.Add(connectorModel);
                            WorkspaceModel.AllConnectorModels.Add(connectorModel);
                        }
                        else if (dynamicProperty.PInfo.Direction.Equals(Direction.OutputData))
                        {
                            ConnectorModel connectorModel = new ConnectorModel();
                            connectorModel.ConnectorType = dynamicProperty.Type;
                            connectorModel.WorkspaceModel = WorkspaceModel;
                            connectorModel.PluginModel = this;
                            connectorModel.IsMandatory = dynamicProperty.PInfo.Mandatory;
                            connectorModel.PropertyName = dynamicProperty.Name;
                            connectorModel.Name = dynamicProperty.Name;
                            connectorModel.ToolTip = dynamicProperty.PInfo.ToolTip;
                             EventInfo eventinfo = Plugin.GetType().GetEvent(dynamicPropertyInfoAttribute.UpdateDynamicPropertiesEvent);
                            eventinfo.AddEventHandler(Plugin, new DynamicPropertiesChanged(connectorModel.PropertyTypeChangedOnPlugin));
                            connectorModel.IsDynamic = true;
                            connectorModel.DynamicGetterName = dynamicPropertyInfoAttribute.MethodGetValue;
                            connectorModel.DynamicSetterName = dynamicPropertyInfoAttribute.MethodSetValue;
                            connectorModel.Outgoing = true;
                            Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
                            OutputConnectors.Add(connectorModel);
                            WorkspaceModel.AllConnectorModels.Add(connectorModel);

                        }
                    }
                }
            }
        }
        /// <summary>
        /// Get the Image of the Plugin
        /// </summary>
        /// <returns></returns>
        public Image getImage()
        {
            return Plugin.GetImage(imageIndex);
        }

        /// <summary>
        /// Returns the Presentation of the wrapped IPlugin
        /// </summary>
        public UserControl PluginPresentation
        {
            get
            {
                if(this.Plugin.Presentation != null){
                    return this.Plugin.Presentation;
                }else{
                    return this.Plugin.QuickWatchPresentation;
                }
            }
        }
             
        /// <summary>
        /// Should be called by the UI-Thread to paint changes of the PluginModel
        /// </summary>
        public void paint()
        {
            //Enter some Code which calls the paint method of the IPlugin
        }       

        /// <summary>
        /// Progress of the plugin changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void PluginProgressChanged(IPlugin sender, PluginProgressEventArgs args)
        {
            //Calculate % of the plugins process
            this.PercentageFinished = args.Value / args.Max;
            //Tell the ExecutionEngine that this plugin needs a gui update
            this.GuiNeedsUpdate = true;
        }

        /// <summary>
        /// Status of the plugin changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void PluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (args.StatusChangedMode == StatusChangedMode.ImageUpdate)
            {
                this.imageIndex = args.ImageIndex;
            }
                
            if (this.WorkspaceModel.WorkspaceManagerEditor.isExecuting())
            {
                this.GuiNeedsUpdate = true;
            }
            else
            {
                this.WorkspaceModel.WorkspaceManagerEditor.Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.UpdateableView.update();
                }, null);
            }            
        }

        /// <summary>
        /// The pluginProtocol of the current ExecutionEngine run to set/get
        /// </summary>
        public PluginProtocol PluginProtocol {
            get { return pluginProtocol; }
            set { pluginProtocol = value;}
        }

        /// <summary>
        /// All occured log events of this plugin
        /// </summary>
        [NonSerialized]
        public List<GuiLogEventArgs> GuiLogEvents = new List<GuiLogEventArgs>();

        /// <summary>
        /// GuiLogNotificationOccured
        /// saves the plugins log events and tells the gui that it needs
        /// an update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void GuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
        {
            if (sender == this.plugin)
            {
                this.GuiLogEvents.Add(args);
                this.GuiNeedsUpdate = true;
            }
        }

        /// <summary>
        /// Called if a Setting of a Plugin is changed and notifies the Editor that
        /// there is a change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        public void SettingsPropertyChanged(Object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            this.WorkspaceModel.WorkspaceManagerEditor.HasChanges = true;
        }

        #endregion
    }

    /// <summary>
    /// The internal state of a Plugin Model
    /// </summary>
    public enum PluginModelState{
        Normal,
        Warning,
        Error
    }
}
