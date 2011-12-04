﻿/*                              
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
using System.Windows.Threading;
using Cryptool.PluginBase;
using System.Threading;
using System.Windows.Controls;
using WorkspaceManager.Execution;
using System.Reflection;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text;

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
        [NonSerialized] 
        internal bool SettingesHaveChanges = false;
        #endregion

        #region public members

        /// <summary>
        /// State of the Plugin
        /// </summary>

        public PluginModelState State
        {
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
        public IPlugin Plugin
        {
            get
            {
                if (plugin == null && PluginType != null)
                {
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
        public Type PluginType
        {
            get
            {
                if (this.PluginTypeName != null)
                {
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
        /// Not used at all anymore
        /// </summary>
        [Obsolete("Startable flag is not used anymore")]
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
                    generateConnector(propertyInfoAttribute);
                }                
            }
        }

        /// <summary>
        /// Generate a single Connector of this Plugin.
        /// </summary>
        /// <param name="propertyInfoAttribute"></param>
        internal void generateConnector(PropertyInfoAttribute propertyInfoAttribute)
        {
            if (propertyInfoAttribute.Direction.Equals(Direction.InputData))
            {
                ConnectorModel connectorModel = new ConnectorModel();

                connectorModel.Caption = propertyInfoAttribute.Caption;
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
                connectorModel.Caption = propertyInfoAttribute.Caption;
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
                connectorModel.Caption = propertyInfoAttribute.Caption;
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
                connectorModel.Caption = propertyInfoAttribute.Caption;
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
                return this.Plugin.Presentation;
            }
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
                imageIndex = args.ImageIndex;
                if (WorkspaceModel.ExecutionEngine == null || !WorkspaceModel.ExecutionEngine.IsRunning())
                {
                    if (WorkspaceModel.MyEditor != null && WorkspaceModel.MyEditor.Presentation != null && UpdateableView != null)
                    {
                        WorkspaceModel.MyEditor.Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            UpdateableView.update();
                        }, null);
                    }
                }
            }
        }        

        /// <summary>
        /// Called if a Setting of a Plugin is changed 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        public void SettingsPropertyChanged(Object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if(sender == plugin.Settings)
            {
                SettingesHaveChanges = true;
            }
        }

        /// <summary>
        /// Returns true if one of this PluginModel inputs is an IControl
        /// </summary>
        /// <returns></returns>
        public bool HasIControlInputs()
        {
            foreach (ConnectorModel connectorModel in OutputConnectors)
            {
                if (connectorModel.IControl)
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
            var executionEngine = (ExecutionEngine)o;
            try
            {
                Stop = false;

                plugin.PreExecution();
                bool firstrun = true;

                while (true)
                {
                    resetEvent.WaitOne(10);
                    resetEvent.Reset();

                    //Check if we want to stop
                    if (Stop)
                    {
                        break;
                    }

                    // ################
                    // 0. If this is our first run and we are startable we start
                    // ################

                    //we are startable if we have NO input connectors
                    //Startable flag is deprecated now
                    if (firstrun && InputConnectors.Count == 0)
                    {
                        firstrun = false;
                        try
                        {
                            PercentageFinished = 0;
                            GuiNeedsUpdate = true;

                            Plugin.Execute();
                            executionEngine.ExecutionCounter++;

                            PercentageFinished = 1;
                            GuiNeedsUpdate = true;
                        }
                        catch (Exception ex)
                        {
                            executionEngine.GuiLogMessage(
                                "An error occured while executing  \"" + Name + "\": " + ex.Message,
                                NotificationLevel.Error);
                            State = PluginModelState.Error;
                            GuiNeedsUpdate = true;
                        }
                        continue;
                    }

                    var breakit = false;
                    var atLeastOneNew = false;

                    // ################
                    // 1. Check if we may execute
                    // ################
                    
                    //Check if all necessary inputs are set                
                    foreach (ConnectorModel connectorModel in InputConnectors)
                    {                        
                        if (!connectorModel.IControl &&
                            (connectorModel.IsMandatory || connectorModel.InputConnections.Count > 0))
                        {
                            if(connectorModel.DataQueue.Count == 0 && connectorModel.LastData == null)
                            {
                                breakit = true;
                                continue;
                            }
                            if(connectorModel.DataQueue.Count > 0)
                            {
                                atLeastOneNew = true;
                            }
                        }                        
                    }

                    //Check if all outputs are free         
                    foreach (ConnectorModel connectorModel in OutputConnectors)
                    {
                        if (!connectorModel.IControl)
                        {
                            foreach(ConnectionModel connectionModel in connectorModel.OutputConnections)
                            {
                                if(connectionModel.To.DataQueue.Count>0)
                                {
                                    breakit = true;
                                }
                            }
                        }
                    }

                    //Gate is a special case: here we need all new data
                    if (PluginType.FullName.Equals("Gate.Gate"))
                    {
                        foreach (ConnectorModel connectorModel in InputConnectors)
                        {
                            if (connectorModel.InputConnections.Count > 0 && connectorModel.DataQueue.Count == 0)
                            {
                                breakit = true;
                            }
                        }
                    }

                    if (breakit || !atLeastOneNew)
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
                            if(connectorModel.DataQueue.Count == 0 && connectorModel.LastData == null)
                            {
                                continue;
                            }

                            object data;
                            
                            if (connectorModel.DataQueue.Count > 0)
                            {
                                data = connectorModel.DataQueue.Dequeue();                                
                            }
                            else
                            {
                                continue;
                            }

                            if (data == null)
                            {
                                continue;
                            }
                            //Implicit conversions:

                            //Cast from BigInteger -> Integer
                            if ((connectorModel.ConnectorType.FullName == "System.Int32" ||
                                 connectorModel.ConnectorType.FullName == "System.Int64") &&
                                data.GetType().FullName == "System.Numerics.BigInteger")
                            {
                                try
                                {
                                    data = (int)((BigInteger)data);                                    
                                }
                                catch (OverflowException)
                                {
                                    State = PluginModelState.Error;
                                    WorkspaceModel.ExecutionEngine.GuiLogMessage(String.Format("Number of {0} too big for {1}: {2}", connectorModel.Name, Name, data), NotificationLevel.Error);
                                }
                            }
                            //Cast from Integer -> BigInteger
                            else if (connectorModel.ConnectorType.FullName == "System.Numerics.BigInteger" &&
                               (data.GetType().FullName == "System.Int32" || data.GetType().FullName == "System.Int64"))
                            {
                                data = new BigInteger((int)data);
                            }
                            //Cast from System.Byte[] -> System.String (UTF8)
                            else if (connectorModel.ConnectorType.FullName == "System.String" && data.GetType().FullName == "System.Byte[]")
                            {
                                var encoding = new UTF8Encoding();
                                data = encoding.GetString((byte[])data);
                            }
                            //Cast from System.String (UTF8) -> System.Byte[]
                            else if (connectorModel.ConnectorType.FullName == "System.Byte[]" && data.GetType().FullName == "System.String")
                            {
                                var encoding = new UTF8Encoding();
                                data = encoding.GetBytes((string)data);
                            }
                            
                            //now set the data                           
                            if (connectorModel.property == null)
                            {
                                connectorModel.property =
                                    Plugin.GetType().GetProperty(connectorModel.PropertyName);
                            }
                            connectorModel.property.SetValue(Plugin, data, null);                                                     
                        }
                        catch (Exception ex)
                        {
                            executionEngine.GuiLogMessage(
                                "An error occured while setting value of connector \"" + connectorModel.Name +
                                "\" of \"" + Name + "\": " + ex.Message, NotificationLevel.Error);
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
                        
                        PercentageFinished = 0;
                        GuiNeedsUpdate = true;

                        Plugin.Execute();
                        executionEngine.ExecutionCounter++;

                        PercentageFinished = 1;
                        GuiNeedsUpdate = true;
                    }
                    catch (Exception ex)
                    {
                        executionEngine.GuiLogMessage(
                            "An error occured while executing  \"" + Name + "\": " + ex.Message, NotificationLevel.Error);
                        State = PluginModelState.Error;
                        GuiNeedsUpdate = true;
                    }                    
                    
                    // ################
                    // 4. Set all connectorModels belonging to this pluginModel to inactive
                    // ################
                    foreach (ConnectorModel connectorModel in InputConnectors)
                    {
                        foreach (var connectionModel in connectorModel.InputConnections)
                        {
                            connectionModel.Active = false;
                            connectionModel.GuiNeedsUpdate = true;
                        }
                    }

                    // ################
                    // 5. let all plugins before this check if it may execute
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
            catch (Exception ex)
            {
                executionEngine.GuiLogMessage(
                               "An error occured while executing  \"" + Name + "\": " + ex.Message,
                               NotificationLevel.Error);
                State = PluginModelState.Error;
            }
        }

        //public int ZIndex { get; set; }
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
