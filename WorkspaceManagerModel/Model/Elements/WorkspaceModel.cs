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
using System.Windows;
using Cryptool.PluginBase;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Threading;
using WorkspaceManager.Model.Tools;
using Cryptool.PluginBase.Editor;
using WorkspaceManagerModel.Model.Operations;

namespace WorkspaceManager.Model
{
    /// <summary>
    /// Class to represent our Workspace
    /// </summary>
    [Serializable]
    public class WorkspaceModel : VisualElementModel
    {

        internal WorkspaceModel()
        {
            this.AllPluginModels = new List<PluginModel>();
            this.AllConnectionModels = new List<ConnectionModel>();
            this.AllConnectorModels = new List<ConnectorModel>();
            this.AllImageModels = new List<ImageModel>();
            this.AllTextModels = new List<TextModel>();
        }

        [NonSerialized]
        private IEditor workspaceManagerEditor;

        /// <summary>
        /// The surrounding WorkspaceManagerEditor
        /// </summary> 
        public IEditor Editor
        {
            get
            {
                return workspaceManagerEditor;
            }
            set
            {
                workspaceManagerEditor = value;
            }
        }

        [NonSerialized]
        public UndoRedoManager UndoRedoManager;

        [NonSerialized]
        private bool hasChanges;
        public bool HasChanges 
        {
            get
            {
                return hasChanges;
            }
            private set
            {
                hasChanges = value;
            }
        }

        /// <summary>
        /// All PluginModels of our Workspace Model
        /// </summary>
        internal List<PluginModel> AllPluginModels;

        /// <summary>
        /// Get all PluginModels of our Workspace Model
        /// </summary>
        public ReadOnlyCollection<PluginModel> GetAllPluginModels()
        {
            return AllPluginModels.AsReadOnly();
        }

        /// <summary>
        /// All Connector Models of our Workspace Model
        /// </summary>
        internal List<ConnectorModel> AllConnectorModels;

        /// <summary>
        /// Get all Connector Models of our Workspace Model
        /// </summary>
        public ReadOnlyCollection<ConnectorModel> GetAllConnectorModels()
        {
            return AllConnectorModels.AsReadOnly();
        }

        /// <summary>
        /// All ConnectionModels of our Workspace Model
        /// </summary>
        internal List<ConnectionModel> AllConnectionModels;

        /// <summary>
        /// Get all ConnectionModels of our Workspace Model
        /// </summary>
        public ReadOnlyCollection<ConnectionModel> GetAllConnectionModels()
        {
            return AllConnectionModels.AsReadOnly();
        }

        /// <summary>
        /// All ImageModels of our Workspace Model
        /// </summary>
        internal List<ImageModel> AllImageModels;

        /// <summary>
        /// Get all ImageModels of our Workspace Model
        /// </summary>
        public ReadOnlyCollection<ImageModel> GetAllImageModels()
        {
            return AllImageModels.AsReadOnly();
        }

        /// <summary>
        /// All TextModels of our Workspace Model
        /// </summary>
        internal List<TextModel> AllTextModels;

        /// <summary>
        /// Get all TextModels of our Workspace Model
        /// </summary>
        public ReadOnlyCollection<TextModel> GetAllTextModels()
        {
            return AllTextModels.AsReadOnly();
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
        internal PluginModel newPluginModel(Point position, double width, double height, Type pluginType)
        {
            PluginModel pluginModel = new PluginModel();
            pluginModel.WorkspaceModel = this;            
            pluginModel.Startable = pluginType.GetPluginInfoAttribute().Startable;
            pluginModel.Position = position;
            pluginModel.PluginType = pluginType;            
            pluginModel.Name = pluginType.Name;
            pluginModel.RepeatStart = false;
            pluginModel.generateConnectors();
            pluginModel.Plugin.Initialize();
            //pluginModel.Plugin.OnGuiLogNotificationOccured += pluginModel.GuiLogNotificationOccured;
            pluginModel.Plugin.OnPluginProgressChanged += pluginModel.PluginProgressChanged;
            pluginModel.Plugin.OnPluginStatusChanged += pluginModel.PluginStatusChanged;
            if (pluginModel.Plugin.Settings != null)
            {
                //pluginModel.Plugin.Settings.PropertyChanged += pluginModel.SettingsPropertyChanged;
            }            
            this.AllPluginModels.Add(pluginModel);
            this.HasChanges = true;
            if (!this.UndoRedoManager.Working)
            {
                this.UndoRedoManager.DidOperation(new NewModelElementOperation(pluginModel));
            }
            return pluginModel;
        }

        /// <summary>
        /// Creates a new PluginModel belonging to this WorkspaceModel
        /// Position and Dimension are (x,y,width,height) = (0,0,0,0)
        /// </summary>
        /// <param name="pluginType"></param>
        /// <returns></returns>
        internal PluginModel newPluginModel(Type pluginType)
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
        internal ConnectionModel newConnectionModel(ConnectorModel from, ConnectorModel to, Type connectionType)
        {
            ConnectionModel connectionModel = new ConnectionModel();
            connectionModel.WorkspaceModel = this;
            connectionModel.From = from;
            connectionModel.To = to;
            from.OutputConnections.Add(connectionModel);
            to.InputConnections.Add(connectionModel);
            connectionModel.ConnectionType = connectionType;

            //If we connect two IControls we have to set data directly:
            if (from.IControl && to.IControl)
            {
                object data = null;
                //Get IControl data from "to"
                if (to.IsDynamic)
                {
                    data = to.PluginModel.Plugin.GetType().GetMethod(to.DynamicGetterName).Invoke(to.PluginModel.Plugin, new object[] { to.PropertyName });
                }
                else
                {
                    data = to.PluginModel.Plugin.GetType().GetProperty(to.PropertyName).GetValue(to.PluginModel.Plugin, null);
                }

                //Set IControl data
                if (from.IsDynamic)
                {
                    MethodInfo propertyInfo = from.PluginModel.Plugin.GetType().GetMethod(from.DynamicSetterName);
                    propertyInfo.Invoke(from.PluginModel.Plugin, new object[] { from.PropertyName, data });
                }
                else
                {
                    PropertyInfo propertyInfo = from.PluginModel.Plugin.GetType().GetProperty(from.PropertyName);
                    propertyInfo.SetValue(from.PluginModel.Plugin, data, null);
                }
            }

            this.AllConnectionModels.Add(connectionModel);
            this.HasChanges = true;
            if (!this.UndoRedoManager.Working)
            {
                this.UndoRedoManager.DidOperation(new NewModelElementOperation(connectionModel));
            }
            return connectionModel;
        }

        /// <summary>
        /// Creates a new ImageModel containing the under imgUri stored Image
        /// </summary>
        /// <param name="imgUri"></param>
        /// <returns></returns>
        internal ImageModel newImageModel(Uri imgUri)
        {
            ImageModel imageModel = new ImageModel(imgUri);
            this.AllImageModels.Add(imageModel);
            this.HasChanges = true;
            if (!this.UndoRedoManager.Working)
            {
                this.UndoRedoManager.DidOperation(new NewModelElementOperation(imageModel));
            }
            return imageModel;
        }

        /// <summary>
        /// Creates a new TextModel
        /// </summary>
        /// <param name="imgUri"></param>
        /// <returns></returns>
        internal TextModel newTextModel()
        {
            TextModel textModel = new TextModel();
            this.AllTextModels.Add(textModel);
            this.HasChanges = true;
            if (!this.UndoRedoManager.Working)
            {
                this.UndoRedoManager.DidOperation(new NewModelElementOperation(textModel));
            }
            return textModel;
        }

        /// <summary>
        /// Deletes the given ImageModel
        /// </summary>
        /// <param name="imgUri"></param>
        /// <returns></returns>
        internal bool deleteImageModel(ImageModel imageModel)
        {
            if (!this.UndoRedoManager.Working)
            {
                this.UndoRedoManager.DidOperation(new DeleteModelElementOperation(imageModel));
            }
            return this.AllImageModels.Remove(imageModel);
        }

        /// <summary>
        /// Deletes the given TextModel
        /// </summary>
        /// <param name="imgUri"></param>
        /// <returns></returns>
        internal bool deleteTextModel(TextModel textModel)
        {
            if (!this.UndoRedoManager.Working)
            {
                this.UndoRedoManager.DidOperation(new DeleteModelElementOperation(textModel));
            }
            return this.AllTextModels.Remove(textModel);
        }

        /// <summary>
        /// Deletes the pluginModel and all of its Connectors and the connected Connections
        /// from our WorkspaceModel
        /// </summary>
        /// <param name="pluginModel"></param>
        /// <returns></returns>
        internal bool deletePluginModel(PluginModel pluginModel)
        {
            //we can only delete PluginModels which are part of our WorkspaceModel
            if (this.AllPluginModels.Contains(pluginModel))
            {
                // remove all InputConnectors belonging to this pluginModel from our WorkspaceModel
                foreach (ConnectorModel inputConnector in new List<ConnectorModel>(pluginModel.InputConnectors))
                {
                    deleteConnectorModel(inputConnector);
                }

                // remove all OutputConnectors belonging to this pluginModel from our WorkspaceModel
                foreach (ConnectorModel outputConnector in new List<ConnectorModel>(pluginModel.OutputConnectors))
                {
                    deleteConnectorModel(outputConnector);
                }
                pluginModel.Plugin.Dispose();                
                this.HasChanges = true;
                if (!this.UndoRedoManager.Working)
                {
                    this.UndoRedoManager.DidOperation(new DeleteModelElementOperation(pluginModel));
                }

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
        internal bool deleteConnectorModel(ConnectorModel connectorModel)
        {
            //we can only delete ConnectorModels which are part of our WorkspaceModel
            if(this.AllConnectorModels.Contains(connectorModel)){

                //remove all input ConnectionModels belonging to this Connector from our WorkspaceModel
                foreach (ConnectionModel connectionModel in new List<ConnectionModel>(connectorModel.InputConnections))
                {
                    deleteConnectionModel(connectionModel);
                }

                //remove all output ConnectionModels belonging to this Connector from our WorkspaceModel
                foreach (ConnectionModel outputConnection in new List<ConnectionModel>(connectorModel.OutputConnections))
                {
                    deleteConnectionModel(outputConnection);
                }
                this.HasChanges = true;
                if (!this.UndoRedoManager.Working)
                {
                    this.UndoRedoManager.DidOperation(new DeleteModelElementOperation(connectorModel));
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
        internal bool deleteConnectionModel(ConnectionModel connectionModel)
        {
            if (connectionModel == null)
                return false;

            connectionModel.To.InputConnections.Remove(connectionModel);
            connectionModel.From.OutputConnections.Remove(connectionModel);            
            this.HasChanges = true;
            if (!this.UndoRedoManager.Working)
            {
                this.UndoRedoManager.DidOperation(new DeleteModelElementOperation(connectionModel));
            }

            return this.AllConnectionModels.Remove(connectionModel);
        }

        /// <summary>
        /// Sets all Connections and Connectors to state nonActive/noData
        ///      all plugins to state Normal
        ///      deletes all stored log events
        /// </summary>
        internal void resetStates()
        {
            foreach (PluginModel pluginModel in this.AllPluginModels)
            {
                pluginModel.State = PluginModelState.Normal;
                pluginModel.GuiLogEvents.Clear();
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
        internal bool reconnectConnection(ConnectionModel connectionModel, ConnectorModel connectorModel)
        {
            if (connectionModel.To != null)
            {
                connectionModel.To.InputConnections.Remove(connectionModel);
            }
            connectionModel.To = connectorModel;
            connectorModel.InputConnections.Add(connectionModel);
            this.HasChanges = true;
            return true;
        }

        /// <summary>
        /// Checks wether a Connector and a Connector are compatible to be connected
        /// They are compatible if their types are equal or the base type of the Connector
        /// is equal to the type of the other Connector
        /// It is false if already exists a ConnectionModel between both given ConnectorModels
        /// </summary>
        /// <param name="connectorModelA"></param>
        /// <param name="connectorModelB"></param>
        /// <returns></returns>
        public static bool compatibleConnectors(ConnectorModel connectorModelA, ConnectorModel connectorModelB)
        {
            if (!connectorModelA.Outgoing || connectorModelB.Outgoing || connectorModelA.PluginModel == connectorModelB.PluginModel)
            {
                return false;
            }

            foreach(ConnectionModel connectionModel in connectorModelA.WorkspaceModel.AllConnectionModels)
            {
                if ((connectionModel.From == connectorModelA && connectionModel.To == connectorModelB) ||
                   (connectionModel.From == connectorModelB && connectionModel.To == connectorModelA))
                {
                    return false;
                }
            }
               
            if (connectorModelA.ConnectorType.Equals(connectorModelB.ConnectorType)
                || connectorModelA.ConnectorType.FullName == "System.Object"
                || connectorModelB.ConnectorType.FullName == "System.Object"
                || connectorModelA.ConnectorType.IsSubclassOf(connectorModelB.ConnectorType)
                || connectorModelA.ConnectorType.GetInterfaces().Contains(connectorModelB.ConnectorType))              
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}