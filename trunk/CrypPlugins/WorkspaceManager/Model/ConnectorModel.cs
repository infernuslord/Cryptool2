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
using System.ComponentModel;
using System.Threading;

namespace WorkspaceManager.Model
{
    

    /// <summary>
    /// Class to represent the Connection between two Connector Models
    /// </summary>
    [Serializable]
    public class ConnectorModel : VisualElementModel
    {
        [NonSerialized]
        private Mutex mutex = new Mutex();

        [NonSerialized]
        private bool hasData = false;

        [NonSerialized]
        private object data;

        /// <summary>
        /// The PluginModel this Connector belongs to
        /// </summary>
        public PluginModel PluginModel { get; set; }

        /// <summary>
        /// The data type of this ConnectorModel
        /// </summary>
        public Type ConnectorType { get; set; }

        /// <summary>
        /// Is this Connector Outgoing?
        /// </summary>
        public bool Outgoing { get; set; }
        
        /// <summary>
        /// The InputConnection of this ConnectorModel
        /// </summary>
        public ConnectionModel InputConnection { get; set; }

        /// <summary>
        /// The OutputConnections of this ConnectorModel
        /// </summary>
        public List<ConnectionModel> OutputConnections;

        /// <summary>
        /// Creates a new ConnectorModel
        /// </summary>
        public ConnectorModel()
        {
            this.OutputConnections = new List<ConnectionModel>();
        }

        /// <summary>
        /// The WorkspaceModel of this PluginModel
        /// </summary>
        public WorkspaceModel WorkspaceModel { get; set; }

        /// <summary>
        /// Is this Connectors Data mandatory?
        /// </summary>
        /// <returns></returns>
        public bool IsMandatory
        {
            get;
            set;
        }

        /// <summary>
        /// Does this Connector currently provides Data?
        /// </summary>
        /// <returns></returns>
        public bool HasData
        {
            get { 
                    return hasData; 
            }

            set 
            {   
                mutex.WaitOne(); 
                hasData = value; 
                mutex.ReleaseMutex(); 
            }
        }

        /// <summary>
        /// Data of this Connector
        /// </summary>
        public object Data
        {
            get
            {
                return data;
            }

            set
            {
                mutex.WaitOne();
                data = value;
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Name of the represented Property of the IPlugin of this ConnectorModel
        /// </summary>
        public string PropertyName{get;set;}

        /// <summary>
        /// ToolTip of this Connector
        /// </summary>
        public string ToolTip { get; set; }

        /// <summary>
        /// Plugin informs the Connector that a PropertyChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        public void PropertyChangedOnPlugin(Object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if(propertyChangedEventArgs.PropertyName.Equals(PropertyName) && Outgoing){
                foreach (ConnectionModel connectionModel in this.OutputConnections)
                {
                    while (connectionModel.To.HasData && WorkspaceModel.WorkspaceManagerEditor.isExecuting())
                    {
                        Thread.Sleep(5); 
                    }
                    connectionModel.To.Data = sender.GetType().GetProperty(propertyChangedEventArgs.PropertyName).GetValue(sender, null);
                    connectionModel.To.HasData = true;
                    connectionModel.Active = true;
                    this.WorkspaceModel.WorkspaceManagerEditor.GuiLogMessage("PropertyChanged: " + sender.GetType().GetProperty(propertyChangedEventArgs.PropertyName),Cryptool.PluginBase.NotificationLevel.Debug);

                    //We changed an input on the PluginModel where "To" is belonging to so
                    //we have to check if this is executable now
                    connectionModel.To.PluginModel.checkExecutable(PluginModel.PluginProtocol);
                }
            }                       
        }

        /// <summary>
        /// Orientation of this Connecor
        /// </summary>
        public ConnectorOrientation ConnectorOrientation { get; set; }

    }

    /// <summary>
    /// Enumeration for connector orientation:
    /// 
    ///        North
    ///       --------
    ///       |      |
    /// West  |      |  East
    ///       |      |
    ///       --------
    ///        South
    /// </summary>
    public enum ConnectorOrientation
    {
        North,
        East,
        South,
        West
    }
}
