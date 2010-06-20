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
using Cryptool.PluginBase;
using WorkspaceManager.View.Container;
using System.Windows.Media;

namespace WorkspaceManager.Model
{
    

    /// <summary>
    /// Class to represent the Connection between two Connector Models
    /// </summary>
    [Serializable]
    public class ConnectorModel : VisualElementModel
    {        
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
        /// Is this a dynamic connector?
        /// </summary>
        public bool IsDynamic
        {
            get;
            set;
        }

        /// <summary>
        /// DynamicGetterName
        /// </summary>
        public string DynamicGetterName
        {
            get;
            set;
        }

        /// <summary>
        /// DynamicSetterName
        /// </summary>
        public string DynamicSetterName
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
                hasData = value; 
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
                data = value;
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
            if(sender == this.PluginModel.Plugin && 
                propertyChangedEventArgs.PropertyName.Equals(PropertyName) && 
                Outgoing){
                
                foreach (ConnectionModel connectionModel in this.OutputConnections)
                {
                    if (IsDynamic)
                    {
                        connectionModel.To.Data = sender.GetType().GetMethod(DynamicGetterName).Invoke(sender, new object[] { this.PropertyName });
                    }
                    else
                    {
                        connectionModel.To.Data = sender.GetType().GetProperty(propertyChangedEventArgs.PropertyName).GetValue(sender, null);
                    }
                    connectionModel.To.HasData = true;
                    connectionModel.Active = true;
                    
                    //We changed an input on the PluginModel where "To" is belonging to so
                    //we have to check if this is executable now
                    connectionModel.To.PluginModel.checkExecutable(connectionModel.To.PluginModel.PluginProtocol);
                }
            }                       
        }

        /// <summary>
        /// Orientation of this Connecor
        /// </summary>
        public ConnectorOrientation ConnectorOrientation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public void PropertyTypeChangedOnPlugin(IPlugin plugin)
        {
            Dictionary<string, DynamicProperty> dictionary = plugin.GetDynamicPropertyList();
            DynamicPropertyInfoAttribute dynamicPropertyInfoAttribute = plugin.GetDynamicPropertyInfo();
            foreach (DynamicProperty dynamicProperty in dictionary.Values)
            {
                
                if (this.PropertyName == dynamicProperty.Name)
                {
                    if(this.InputConnection != null){
                        this.WorkspaceModel.deleteConnectionModel(this.InputConnection);
                    }
                    foreach(ConnectionModel connectionModel in new List<ConnectionModel>(this.OutputConnections))
                    {
                        this.WorkspaceModel.deleteConnectionModel(connectionModel);
                    }
                    this.ConnectorType = dynamicProperty.Type;
                    if (this.UpdateableView != null)
                    {
                        ((ConnectorView)this.UpdateableView).Ellipse.Fill = new SolidColorBrush(ColorHelper.GetColor(this.ConnectorType));
                    }
                }
            }
        }
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
