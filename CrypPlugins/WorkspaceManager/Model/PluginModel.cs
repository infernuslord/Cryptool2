﻿/*                              
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

namespace WorkspaceManager.Model
{
    /// <summary>
    /// Class to represent and wrap a IPlugin in our model graph
    /// </summary>
    [Serializable]
    public class PluginModel : VisualElementModel
    {       
        [NonSerialized]
        private IPlugin plugin;
        
        /// <summary>
        /// This is the current index of the image which should be shown
        /// on the plugins view
        /// </summary>
        private int imageIndex = 0;
        
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
        public Type PluginType = null;
        
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
        /// Warning: Before generation all "old" Connectors will be deleted
        /// </summary>
        public void generateConnectors(){
            
            if (Plugin != null)
            {
                this.InputConnectors.Clear();
                this.OutputConnectors.Clear();

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
                        connectorModel.ToolTip = propertyInfoAttribute.ToolTip;
                        connectorModel.ConnectorOrientation = ConnectorOrientation.West;
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
                        connectorModel.ToolTip = propertyInfoAttribute.ToolTip;
                        connectorModel.ConnectorOrientation = ConnectorOrientation.East;
                        connectorModel.Outgoing = true;
                        Plugin.PropertyChanged += connectorModel.PropertyChangedOnPlugin;
                        OutputConnectors.Add(connectorModel);
                        WorkspaceModel.AllConnectorModels.Add(connectorModel);
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
        /// Checks wether this PluginModel is executable or not and sets the isExecutable bool
        /// 
        /// There are 3 ways in that a plugin can be executable:
        ///     1. All mandatory inputs are set + all outputs are "free"
        ///     2. There are no mandatory inputs and at least one non-mandatory input is set + all outputs are "free"
        ///     3. There are no inputs + all outputs are "free"
        /// </summary>
        public void checkExecutable(ProtocolBase protocolBase)
        {                            
            bool AtLeastOneInputSet = false;
            //First test if every mandatory Connector has data
            //or one non-mandatory input has data
            foreach (ConnectorModel connectorModel in this.InputConnectors)
            {
                if (connectorModel.IsMandatory && !connectorModel.HasData)
                {
                    return;
                }
                else if (connectorModel.HasData)
                {
                    AtLeastOneInputSet = true;
                }

            }

            //Next test if every connected output Connection is not active
            foreach (ConnectorModel connectorModel in this.OutputConnectors)
            {
                foreach (ConnectionModel connection in connectorModel.OutputConnections)
                {
                    if (connection.Active)
                    {                            
                       return;
                    }                        
                }
            }

            if (AtLeastOneInputSet || this.InputConnectors.Count == 0)
            {
                MessageExecution msg = new MessageExecution();
                msg.PluginModel = this;
                
                //protocolBase is set at Startup of the ExecutionEngine
                //but it could be that we have an event before setting
                //of the protocl base (triggered by user clicking on
                //a plugins presentation (button or so))
                if (protocolBase != null)
                {
                    protocolBase.BroadcastMessageReliably(msg);
                }
            }
            return;
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
        public PluginProtocol PluginProtocol { get; set; }
    }
}