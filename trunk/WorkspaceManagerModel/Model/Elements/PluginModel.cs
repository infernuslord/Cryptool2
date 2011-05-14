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
using Cryptool.PluginBase;
using System.Threading;
using System.Windows.Controls;
using WorkspaceManager.Execution;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace WorkspaceManager.Model
{

    /// <summary>
    /// Class to represent and wrap a IPlugin in our model graph
    /// </summary>
    [Serializable]
    public class PluginModel : VisualElementModel
    {
        internal PluginModel()
        {
            this.InputConnectors = new List<ConnectorModel>();
            this.OutputConnectors = new List<ConnectorModel>();
        }
              
        #region private members

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
        internal List<ConnectorModel> InputConnectors = null;

        /// <summary>
        /// Get all ingoing connectors of this PluginModel
        /// </summary>
        public ReadOnlyCollection<ConnectorModel> GetInputConnectors()
        {
            return InputConnectors.AsReadOnly();
        }

        /// <summary>
        /// All outgoing connectors of this PluginModel
        /// </summary>
        internal List<ConnectorModel> OutputConnectors = null;

        /// <summary>
        /// Get all outgoing connectors of this PluginModel
        /// </summary>
        public ReadOnlyCollection<ConnectorModel> GetOutputConnectors()
        {
            return OutputConnectors.AsReadOnly();
        }

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
            internal set
            {
                this.PluginTypeName = value.FullName;
                this.PluginTypeAssemblyName = value.Assembly.GetName().Name;
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
        public bool Minimized { get; internal set; }

        /// <summary>
        /// The execution state of the progress of the wrapped plugin 
        /// </summary>
        public double PercentageFinished { get; internal set; }
       
        /// <summary>
        /// The WorkspaceModel of this PluginModel
        /// </summary>
        public WorkspaceModel WorkspaceModel { get; internal set; }

        /// <summary>
        /// Current View state
        /// </summary>
        public PluginViewState ViewState { get; set; }

        /// <summary>
        /// Generates all Connectors of this Plugin.
        /// </summary>
        internal void generateConnectors()
        {
            InputConnectors.Clear();
            OutputConnectors.Clear();

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
                        connectorModel.PluginModel.Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
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
                        connectorModel.PluginModel.Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
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
                        connectorModel.PluginModel.Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
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
                        connectorModel.PluginModel.Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
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
                            connectorModel.PluginModel.Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
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
                            connectorModel.PluginModel.Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
                            connectorModel.Outgoing = true;
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
        }

        /// <summary>
        /// GuiLogNotificationOccured
        /// saves the plugins log events and tells the gui that it needs
        /// an update. If the Workspace is not executing an event is invoked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /*public void GuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
        {
            switch (((WorkspaceManagerSettings)this.WorkspaceModel.Editor.Settings).LogLevel)
            {
                case 3://Error
                    if (args.NotificationLevel == NotificationLevel.Debug ||
                        args.NotificationLevel == NotificationLevel.Info ||
                        args.NotificationLevel == NotificationLevel.Warning)
                    {
                        return;
                    }
                    break;

                case 2://Warning
                    if (args.NotificationLevel == NotificationLevel.Debug ||
                        args.NotificationLevel == NotificationLevel.Info)
                    {
                        return;
                    }
                    break;

                case 1://Info
                    if (args.NotificationLevel == NotificationLevel.Debug)
                    {
                        return;
                    }
                    break;
            }
            if (sender == this.plugin)
            {
                this.GuiLogEvents.Add(args);
                this.GuiNeedsUpdate = true;
            }

            if (this.LogUpdated != null)
            {
                if(!this.WorkspaceModel.Editor.isExecuting())
                    this.LogUpdated.Invoke(this, new LogUpdated {});
            }
        }*/

        
        /// <summary>
        /// Called if a Setting of a Plugin is changed and notifies the Editor that
        /// there is a change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        public void SettingsPropertyChanged(Object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {            
            this.WorkspaceModel.HasChanges = true;
        }

        /// <summary>
        /// Returns true if one of this PluginModel inputs is an IControl
        /// </summary>
        /// <returns></returns>
        public bool HasIControlInputs()
        {
            foreach(ConnectorModel connectorModel in OutputConnectors)
            {
                if(connectorModel.IControl)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        [NonSerialized]
        private bool stopped = false;
        internal bool Stop { get { return stopped; } set { stopped = value; } }
        
        [NonSerialized]
        internal ManualResetEvent resetEvent = new ManualResetEvent(true);

        /// <summary>
        /// Called by the execution engine threads to execute the internal plugin
        /// </summary>
        /// <param name="o"></param>
        internal void Execute(Object o)
        {
            var executionEngine = (ExecutionEngine) o;
            Stop = false;
            
            plugin.PreExecution();
            bool firstrun = true;

            while(true)
            {
                resetEvent.WaitOne(1000);
                resetEvent.Reset();

                //Check if we want to stop
                if (Stop)
                {
                    break;
                }

                // ################
                // 0. If this is our first run and we are startable we start
                // ################

                if (firstrun && Startable)
                {
                    firstrun = false;
                    try
                    {
                        Plugin.Execute();
                        executionEngine.ExecutionCounter++;
                    }
                    catch (Exception ex)
                    {
                        executionEngine.GuiLogMessage("An error occured while executing  \"" + Name + "\": " + ex.Message, NotificationLevel.Error);
                        State = PluginModelState.Error;
                        GuiNeedsUpdate = true;
                    }
                    continue;
                }

                if (Startable && !RepeatStart && InputConnectors.Count == 0)
                {
                    continue;
                }

                var breakit = false;

                // ################
                // 1. Check if we may execute
                // ################

                //Check if all necessary inputs are set                
                foreach (ConnectorModel connectorModel in InputConnectors)
                {
                    if (!connectorModel.IControl &&
                        (connectorModel.IsMandatory || connectorModel.InputConnections.Count > 0) && !connectorModel.HasData)
                    {
                        breakit = true;
                        continue;
                    }
                }
                if(breakit)
                {
                    continue;
                }

                //Check if all outputs are free
                foreach (ConnectorModel connectorModel in OutputConnectors)
                {
                    if (!connectorModel.IControl)
                    {
                        List<ConnectionModel> outputConnections = connectorModel.OutputConnections;
                        foreach (ConnectionModel connectionModel in outputConnections)
                        {
                            if (connectionModel.To.HasData)
                            {
                                breakit = true;
                                continue;
                            }
                        }
                    }
                }
                if (breakit)
                {
                    continue;
                }

                // ################
                //2. Fill all Inputs of the plugin, if this fails break the loop run
                // ################
                foreach (ConnectorModel connectorModel in InputConnectors)
                {
                    try
                    {
                        if (connectorModel.HasData && connectorModel.Data != null)
                        {
                            if (connectorModel.IsDynamic)
                            {

                                if (connectorModel.method == null)
                                {
                                    connectorModel.method = Plugin.GetType().GetMethod(connectorModel.DynamicSetterName);
                                }
                                connectorModel.method.Invoke(Plugin, new object[] { connectorModel.PropertyName, connectorModel.Data });
                            }
                            else
                            {
                                if (connectorModel.property == null)
                                {
                                    connectorModel.property = Plugin.GetType().GetProperty(connectorModel.PropertyName);
                                }
                                connectorModel.property.SetValue(Plugin, connectorModel.Data, null);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        executionEngine.GuiLogMessage("An error occured while setting value of connector \"" + connectorModel.Name + "\" of \"" + Name + "\": " + ex.Message, NotificationLevel.Error);
                        State = PluginModelState.Error;
                        GuiNeedsUpdate = true;
                    }
                   
                }

                // ################
                //3. Execute
                // ################
                try
                {
                    if (executionEngine.SleepTime > 0)
                    {
                        Thread.Sleep(executionEngine.SleepTime);
                    }
                    Plugin.Execute();
                    executionEngine.ExecutionCounter++;                    
                }
                catch (Exception ex)
                {
                    executionEngine.GuiLogMessage("An error occured while executing  \"" + Name + "\": " + ex.Message, NotificationLevel.Error);
                    State = PluginModelState.Error;
                    GuiNeedsUpdate = true;
                }


                // ################
                //4. "Consume" all inputs
                // ################
                foreach (ConnectorModel connectorModel in InputConnectors)
                {
                    try
                    {
                        if (connectorModel.HasData && connectorModel.Data != null)
                        {
                            connectorModel.HasData = false;
                            connectorModel.Data = null; 
                            foreach(ConnectionModel connectionModel in connectorModel.InputConnections)
                            {
                                connectionModel.Active = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        executionEngine.GuiLogMessage("An error occured while 'consuming' value of connector \"" + connectorModel.Name + "\" of \"" + Name + "\": " + ex.Message, NotificationLevel.Error);
                        State = PluginModelState.Error;
                        GuiNeedsUpdate = true;
                    }
                }

                // ################
                //4. let all plugins before this check if it may execute
                // ################
                foreach (ConnectorModel connectorModel in InputConnectors)
                {
                    foreach (ConnectionModel connectionModel in connectorModel.InputConnections)
                    {
                        connectionModel.From.PluginModel.resetEvent.Set();
                    }
                }
            }
            plugin.PostExecution();
        }        
    }

    /// <summary>
    /// The internal state of a Plugin Model
    /// </summary>
    public enum PluginModelState{
        Normal,
        Warning,
        Error
    };

    public enum BinComponentState
    {
        Min,
        Presentation,
        Data,
        Log,
        Setting,
        Description,
    };

    public enum PluginViewState
    {
        Min,
        Presentation,
        Data,
        Log,
        Setting,
        Description,
        Fullscreen,
    };
}
