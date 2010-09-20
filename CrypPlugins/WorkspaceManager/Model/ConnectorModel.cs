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
using System.ComponentModel;
using System.Windows.Media;
using Cryptool.PluginBase;
using WorkspaceManager.View.Container;
using System.Reflection;
using WorkspaceManager.Execution;
using System.Threading;

namespace WorkspaceManager.Model
{
    
    /// <summary>
    /// Class to represent the Connection between two Connector Models
    /// </summary>
    [Serializable]
    public class ConnectorModel : VisualElementModel
    {
        #region private members

        /// <summary>
        /// Does this Connector Model has actually data?
        /// </summary>
        [NonSerialized]
        private volatile bool hasData = false;

        /// <summary>
        /// Name of the Connector type
        /// </summary>
        private string ConnectorTypeName = null;
        /// <summary>
        /// Name of the Connector assembly
        /// </summary>
        private string ConnectorTypeAssemblyName = null;

        #endregion

        #region public members

        /// <summary>
        /// The PluginModel this Connector belongs to
        /// </summary>
        public PluginModel PluginModel { get; set; }

        /// <summary>
        /// The Type of the Connector Model
        /// </summary>        
        public Type ConnectorType
        {
            get
            {
                if (this.ConnectorTypeName != null)
                {
                    if (ConnectorTypeName.Equals("System.Numerics.BigInteger"))
                    {
                        return typeof(System.Numerics.BigInteger);
                    }
                    if (ConnectorTypeName.Equals("System.Numerics.BigInteger[]"))
                    {
                        return typeof(System.Numerics.BigInteger[]);
                    }
                    Assembly assembly = Assembly.Load(ConnectorTypeAssemblyName);
                    Type t = assembly.GetType(ConnectorTypeName);
                    return t;
                                        
                }
                else
                {
                    return null;
                }
            }
            set
            {
                this.ConnectorTypeName = value.FullName;
                this.ConnectorTypeAssemblyName = value.Assembly.GetName().Name;
            }
        }

        /// <summary>
        /// Is this Connector Outgoing?
        /// </summary>
        public bool Outgoing { get; set; }

        /// <summary>
        /// Is this Connector Outgoing?
        /// </summary>
        public bool IControl { get; set; }

        /// <summary>
        /// The InputConnections of this ConnectorModel
        /// </summary>
        public List<ConnectionModel> InputConnections;

        /// <summary>
        /// The OutputConnections of this ConnectorModel
        /// </summary>
        public List<ConnectionModel> OutputConnections;

        /// <summary>
        /// The Orientation of this ConnectorModel
        /// </summary>
        public ConnectorOrientation Orientation = ConnectorOrientation.Unset;

        /// <summary>
        /// Creates a new ConnectorModel
        /// </summary>
        public ConnectorModel()
        {
            this.InputConnections = new List<ConnectionModel>();
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
            get
            {
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
        [NonSerialized]
        public object Data = null;

        /// <summary>
        /// Name of the represented Property of the IPlugin of this ConnectorModel
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// ToolTip of this Connector
        /// </summary>
        public string ToolTip { get; set; }

        /// Plugin informs the Connector that a PropertyChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        public void PropertyChangedOnPlugin(Object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (!this.PluginModel.WorkspaceModel.WorkspaceManagerEditor.isExecuting() ||
                !(sender == this.PluginModel.Plugin) ||
                !propertyChangedEventArgs.PropertyName.Equals(PropertyName))
            {
                return;
            }

            if (Outgoing)
            {
                if (this.OutputConnections.Count == 0)
                {
                    return;
                }
                object data = null;
                if (IsDynamic)
                {
                    data = sender.GetType().GetMethod(DynamicGetterName).Invoke(sender, new object[] { this.PropertyName });
                }
                else
                {
                    data = sender.GetType().GetProperty(propertyChangedEventArgs.PropertyName).GetValue(sender, null);
                }

                if (data == null)
                {
                    return;
                }

                this.Data = data;

                List<ConnectionModel> outputConnections = this.OutputConnections;
                foreach (ConnectionModel connectionModel in outputConnections)
                {
                    connectionModel.To.Data = data;
                    connectionModel.To.HasData = true;
                    connectionModel.Active = true;
                    connectionModel.GuiNeedsUpdate = true;
                }

                //We changed an input on the PluginModels where "To"s are belonging to so
                //we have to check if there are executable now
                foreach (ConnectionModel connectionModel in outputConnections)
                {
                    if (connectionModel.To.PluginModel.PluginProtocol.QueueLength == 0)
                    {
                        connectionModel.To.PluginModel.PluginProtocol.BroadcastMessage(connectionModel.To.PluginModel.MessageExecution);
                    }
                }
            }
            else
            {
                this.Data = null;
                this.hasData = false;
                this.GuiNeedsUpdate = true;

                List<ConnectionModel> inputConnections = this.InputConnections;
                foreach (ConnectionModel connectionModel in inputConnections)
                {
                    connectionModel.Active = false;
                    connectionModel.GuiNeedsUpdate = true;
                }
                foreach (ConnectionModel connectionModel in inputConnections)
                {
                    if (!connectionModel.From.PluginModel.Startable ||
                        (connectionModel.From.PluginModel.Startable && connectionModel.From.PluginModel.RepeatStart))
                    {
                        if (connectionModel.From.PluginModel.PluginProtocol.QueueLength == 0)
                        {
                            connectionModel.From.PluginModel.PluginProtocol.BroadcastMessage(connectionModel.From.PluginModel.MessageExecution);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// The data type of the wrapped property changes
        /// </summary>        
        public void PropertyTypeChangedOnPlugin(IPlugin plugin)
        {
            Dictionary<string, DynamicProperty> dictionary = plugin.GetDynamicPropertyList();
            DynamicPropertyInfoAttribute dynamicPropertyInfoAttribute = plugin.GetDynamicPropertyInfo();
            foreach (DynamicProperty dynamicProperty in dictionary.Values)
            {

                if (this.PropertyName == dynamicProperty.Name)
                {
                    foreach (ConnectionModel connectionModel in new List<ConnectionModel>(InputConnections))
                    {
                        this.WorkspaceModel.deleteConnectionModel(connectionModel);
                    }
                    foreach (ConnectionModel connectionModel in new List<ConnectionModel>(this.OutputConnections))
                    {
                        this.WorkspaceModel.deleteConnectionModel(connectionModel);
                    }
                    this.ConnectorType = dynamicProperty.Type;
                    if (this.UpdateableView != null)
                    {
                        ((ConnectorView)this.UpdateableView).ConnectorRep.Fill = new SolidColorBrush(ColorHelper.GetColor(this.ConnectorType));
                    }
                }
            }
        }

        #endregion
                
    }
}
