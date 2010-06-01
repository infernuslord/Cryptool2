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

namespace WorkspaceManager.Model
{
    /// <summary>
    /// Class to represent and wrap a IPlugin in our model graph
    /// </summary>
    [Serializable]
    public class PluginModel : VisualElementModel
    {
        [NonSerialized]
        private Mutex mutex = new Mutex();

        [NonSerialized]
        private IPlugin plugin;

        [NonSerialized]
        private PluginModelState executionstate = PluginModelState.Undefined;

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
                        connectorModel.ConnectorType = propertyInfoAttribute.PropertyInfo.DeclaringType;
                        connectorModel.WorkspaceModel = WorkspaceModel;
                        connectorModel.PluginModel = this;
                        connectorModel.IsMandatory = propertyInfoAttribute.Mandatory;
                        connectorModel.PropertyName = propertyInfoAttribute.PropertyName;
                        connectorModel.ConnectorOrientation = ConnectorOrientation.West;
                        InputConnectors.Add(connectorModel);
                        WorkspaceModel.AllConnectorModels.Add(connectorModel);
                    }
                    else if (propertyInfoAttribute.Direction.Equals(Direction.OutputData))
                    {
                        ConnectorModel connectorModel = new ConnectorModel();
                        connectorModel.ConnectorType = propertyInfoAttribute.PropertyInfo.DeclaringType;
                        connectorModel.WorkspaceModel = WorkspaceModel;
                        connectorModel.PluginModel = this;
                        connectorModel.IsMandatory = propertyInfoAttribute.Mandatory;
                        connectorModel.PropertyName = propertyInfoAttribute.PropertyName;
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
        /// Returns the Presentation of the wrapped IPlugin
        /// </summary>
        public System.Windows.Controls.UserControl PluginPresentation
        {
            get
            {
                return this.Plugin.Presentation;
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
        /// The current ExecutionState of this PluginModel
        /// </summary>
        public PluginModelState ExecutionState{
            get
            {
                return this.executionstate;
            }
            set
            {
                this.executionstate = value;
            }
        }

        /// <summary>
        /// Checks wether this PluginModel is executable or not and sets the isExecutable bool
        /// </summary>
        /// <returns></returns>
        public void checkExecutable()
        {
            if(ExecutionState == PluginModelState.Undefined){

                mutex.WaitOne();
                //First test if every mandatory Connector has Data
                foreach (ConnectorModel connectorModel in this.InputConnectors)
                {
                    if (connectorModel.IsMandatory && !connectorModel.HasData)
                    {
                        mutex.ReleaseMutex();
                        return;
                    }

                }

                //Next test if every connceted Connection to each Connection is not active
                foreach (ConnectorModel connectorModel in this.OutputConnectors)
                {
                    foreach (ConnectionModel connection in connectorModel.OutputConnections)
                    {
                        if (connection.Active)
                        {                            
                            mutex.ReleaseMutex();
                            return;
                        }                        
                    }
                }

                ExecutionState = PluginModelState.Executable;
                mutex.ReleaseMutex();
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
            this.PercentageFinished = args.Value / args.Max;
        }
    }

    /// <summary>
    /// Execution States of a PluginModel
    /// </summary>
    public enum PluginModelState
    {
        Undefined,
        Executable,
        PreExecuting,
        Executing,
        PostExecuting,
        Terminated,
        Error
    }
}
