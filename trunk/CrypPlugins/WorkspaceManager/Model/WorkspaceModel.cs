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
using System.Windows;
using Cryptool.PluginBase;

namespace WorkspaceManager.Model
{
    /// <summary>
    /// Class to represent our Workspace
    /// </summary>
    [Serializable]
    public class WorkspaceModel 
    {

        /// <summary>
        /// The surrounding WorkspaceManagerEditor
        /// </summary>
        public WorkspaceManager WorkspaceManagerEditor { get; set; }

        /// <summary>
        /// All PluginModels of our Workspace Model
        /// </summary>
        public List<PluginModel> AllPluginModels;

        /// <summary>
        /// All Connector Models of our Workspace Model
        /// </summary>
        public List<ConnectorModel> AllConnectorModels;

        /// <summary>
        /// All ConnectionModels of our Workspace Model
        /// </summary>
        public List<ConnectionModel> AllConnectionModels;

        /// <summary>
        /// Creates a new Workspace Model
        /// </summary>
        public WorkspaceModel()
        { 
            this.AllPluginModels = new List<PluginModel>();
            this.AllConnectionModels = new List<ConnectionModel>();
            this.AllConnectorModels = new List<ConnectorModel>();            
        }

        /// <summary>
        /// Creates a new PluginModel belonging to this WorkspaceModel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pluginType"></param>
        /// <returns></returns>
        public PluginModel newPluginModel(Point position, double width, double height, Type pluginType)
        {
            PluginModel pluginModel = new PluginModel();
            pluginModel.WorkspaceModel = this;
            pluginModel.Position = position;
            pluginModel.Width = width;
            pluginModel.Height = height;
            pluginModel.PluginType = pluginType;
            pluginModel.generateConnectors();
            pluginModel.Name = pluginType.Name;
            pluginModel.Plugin.OnGuiLogNotificationOccured += this.WorkspaceManagerEditor.GuiLogNotificationOccured;
            pluginModel.Plugin.OnPluginProgressChanged += pluginModel.PluginProgressChanged;
            this.AllPluginModels.Add(pluginModel);
            return pluginModel;
        }

        /// <summary>
        /// Creates a new PluginModel belonging to this WorkspaceModel
        /// Position and Dimension are (x,y,width,height) = (0,0,0,0)
        /// </summary>
        /// <param name="pluginType"></param>
        /// <returns></returns>
        public PluginModel newPluginModel(Type pluginType)
        {
            return newPluginModel(new Point(0, 0), 0, 0, pluginType);
        }       

        /// <summary>
        /// Creates a new Connection starting at "from"-Connector going to "to"-Connector with
        /// the given connectionType
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="connectionType"></param>
        /// <returns></returns>
        public ConnectionModel newConnectionModel(ConnectorModel from, ConnectorModel to, Type connectionType)
        {
            ConnectionModel connectionModel = new ConnectionModel();
            connectionModel.WorkspaceModel = this;
            connectionModel.From = from;
            connectionModel.To = to;
            from.OutputConnections.Add(connectionModel);
            to.InputConnection = connectionModel;
            connectionModel.ConnectionType = connectionType;
            return connectionModel;
        }

        /// <summary>
        /// Deletes the pluginModel and all of its Connectors and the connected Connections
        /// from our WorkspaceModel
        /// </summary>
        /// <param name="pluginModel"></param>
        /// <returns></returns>
        public bool deletePluginModel(PluginModel pluginModel)
        {
            //we can only delete PluginModels which are part of our WorkspaceModel
            if (this.AllPluginModels.Contains(pluginModel))
            {
                // remove all InputConnectors belonging to this pluginModel from our WorkspaceModel
                foreach (ConnectorModel inputConnector in pluginModel.InputConnectors)
                {
                    deleteConnectorModel(inputConnector);
                }

                // remove all OutputConnectors belonging to this pluginModel from our WorkspaceModel
                foreach (ConnectorModel outputConnector in pluginModel.OutputConnectors)
                {
                    deleteConnectorModel(outputConnector);
                }
                pluginModel.Plugin.Dispose();
                return this.AllPluginModels.Remove(pluginModel);
            }            
            return false;
        }

        /// <summary>
        /// Deletes the connectorModel and the connected Connections
        /// from our WorkspaceModel
        /// </summary>
        /// <param name="connectorModel"></param>
        /// <returns></returns>
        private bool deleteConnectorModel(ConnectorModel connectorModel)
        {
            //we can only delete ConnectorModels which are part of our WorkspaceModel
            if(this.AllConnectorModels.Contains(connectorModel)){

                //remove all output ConnectionModels belonging to this Connector from our WorkspaceModel
                foreach (ConnectionModel outputConnection in connectorModel.OutputConnections)
                {
                    deleteConnectionModel(outputConnection);
                }

                //remove the input ConnectionModel belonging to this Connector from our WorkspaceModel
                if (connectorModel.InputConnection != null)
                {
                    deleteConnectionModel(connectorModel.InputConnection);
                }
                return this.AllConnectorModels.Remove(connectorModel);
            }
            return false;
        }

        /// <summary>
        /// Removes the connectionModel from our Workspace Model and removes it from all Connectors
        /// </summary>
        /// <param name="connectionModel"></param>
        /// <returns></returns>
        public bool deleteConnectionModel(ConnectionModel connectionModel)
        {            
            connectionModel.From.OutputConnections.Remove(connectionModel);
            connectionModel.To.InputConnection = null;
            return this.AllConnectionModels.Remove(connectionModel);
        }

        /// <summary>
        /// Sets all Connections and Connectors to state nonActive/noData
        /// </summary>
        public void resetStates()
        {
            foreach (PluginModel pluginModel in this.AllPluginModels)
            {
                pluginModel.ExecutionState = PluginModelState.Undefined;
            }
            foreach (ConnectionModel connection in this.AllConnectionModels)
            {
                connection.Active = false;
            }
            foreach (ConnectorModel connector in this.AllConnectorModels)
            {
                connector.HasData = false;
                connector.Data = null;
            }
        }

        /// <summary>
        /// Reconnects a Connection with an other Connector
        /// </summary>
        /// <param name="connectionModel"></param>
        /// <param name="connectorModel"></param>
        /// <returns></returns>
        public bool reconnectConnection(ConnectionModel connectionModel, ConnectorModel connectorModel)
        {
            if (connectionModel.To != null)
            {
                connectionModel.To.InputConnection = null;
            }
            connectionModel.To = connectorModel;
            connectorModel.InputConnection = connectionModel;
            return true;
        }

        /// <summary>
        /// Checks wether a Connector and a Connector are compatible to be connected
        /// They are compatible if their types are equal or the base type of the Connector
        /// is equal to the type of the other Connector
        /// </summary>
        /// <param name="connectorModelA"></param>
        /// <param name="connectorModelB"></param>
        /// <returns></returns>
        public static bool compatibleConnectors(ConnectorModel connectorModelA, ConnectorModel connectorModelB)
        {
            if (connectorModelA.ConnectorType.Equals(connectorModelB.ConnectorType)
                || connectorModelA.ConnectorType.BaseType.Equals(connectorModelB.ConnectorType))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks wether a Connection and a Connector are compatible to be connected
        /// They are compatible if their types are equal or the base type of the connection
        /// is equal to the type of the connector
        /// </summary>
        /// <param name="connectionModel"></param>
        /// <param name="connectorModel"></param>
        /// <returns></returns>
        public static bool compatibleConnectors(ConnectionModel connectionModel, ConnectorModel connectorModel)
        {
            if (connectionModel.ConnectionType.Equals(connectorModel.ConnectorType)
                || connectionModel.ConnectionType.BaseType.Equals(connectorModel.ConnectorType))
            {
                return true;
            }
            else 
            { 
                return false; 
            }
        }

        /// <summary>
        /// Checks wether a Connection and a Connector are compatible to be connected
        /// They are compatible if their types are equal or the base type of the connection
        /// is equal to the type of the connector
        /// </summary>
        /// <param name="connectorModel"></param>
        /// <param name="connectionModel"></param>
        /// <returns></returns>
        public static bool compatibleConnectors(ConnectorModel connectorModel, ConnectionModel connectionModel)
        {
            return compatibleConnectors(connectionModel, connectorModel);
        }
    }
}