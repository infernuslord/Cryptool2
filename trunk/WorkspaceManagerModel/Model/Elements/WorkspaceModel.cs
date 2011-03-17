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

        /// <summary>
        /// The executing editor
        /// </summary>
        [NonSerialized]
        private IEditor myEditor;
        public IEditor MyEditor
        {
            get { return myEditor; }
            set { myEditor = value; }

        }

        /// <summary>
        /// Create a new WorkspaceModel
        /// </summary>
        public WorkspaceModel()
        {
            this.AllPluginModels = new List<PluginModel>();
            this.AllConnectionModels = new List<ConnectionModel>();
            this.AllConnectorModels = new List<ConnectorModel>();
            this.AllImageModels = new List<ImageModel>();
            this.AllTextModels = new List<TextModel>();
            this.UndoRedoManager = new UndoRedoManager(this);
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
            internal set
            {
                hasChanges = value;
            }
        }

        /// <summary>
        /// Tell this model if its executed or not
        /// </summary>
        [NonSerialized]
        private bool beingExecuted = false;
        internal bool IsBeingExecuted
        {
            get { return beingExecuted; }
            set { beingExecuted = value; }
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
            pluginModel.Plugin.OnGuiLogNotificationOccured += this.GuiLogMessage;
            pluginModel.Plugin.OnPluginProgressChanged += pluginModel.PluginProgressChanged;
            pluginModel.Plugin.OnPluginStatusChanged += pluginModel.PluginStatusChanged;
            if (pluginModel.Plugin.Settings != null)
            {
                pluginModel.Plugin.Settings.PropertyChanged += pluginModel.SettingsPropertyChanged;
            }            
            this.AllPluginModels.Add(pluginModel);
            this.HasChanges = true;
            this.OnNewChildElement(pluginModel);
            return pluginModel;
        }

        /// <summary>
        /// Add an existing PluginModel to this WorkspaceModel
        /// </summary>
        /// <param name="pluginModel"></param>
        /// <returns></returns>
        internal void addPluginModel(PluginModel pluginModel)
        {            
            this.AllPluginModels.Add(pluginModel);
            foreach (ConnectorModel connectorModel in pluginModel.InputConnectors)
            {
                this.AllConnectorModels.Add(connectorModel);
            }
            foreach (ConnectorModel connectorModel in pluginModel.OutputConnectors)
            {
                this.AllConnectorModels.Add(connectorModel);
            }
            this.HasChanges = true;
            this.OnNewChildElement(pluginModel);
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
            this.OnNewChildElement(connectionModel);
            return connectionModel;
        }

        /// <summary>
        /// Add an existing ConnectionModel to this WorkspaceModel
        /// </summary>
        /// <param name="connectionModel"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        internal void addConnectionModel(ConnectionModel connectionModel)
        {
            ConnectorModel from = connectionModel.From;
            ConnectorModel to = connectionModel.To;
            from.OutputConnections.Add(connectionModel);
            to.InputConnections.Add(connectionModel);
           
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
            this.OnNewChildElement(connectionModel);
        }

        /// <summary>
        /// Creates a new ImageModel containing the under imgUri stored Image
        /// </summary>
        /// <param name="imgUri"></param>
        /// <returns></returns>
        internal ImageModel newImageModel(Uri imgUri)
        {
            ImageModel imageModel = new ImageModel(imgUri);
            imageModel.WorkspaceModel = this;
            this.AllImageModels.Add(imageModel);
            this.HasChanges = true;
            this.OnNewChildElement(imageModel);
            return imageModel;
        }

        /// <summary>
        /// Add ImageModel containing the under imgUri stored Image
        /// </summary>
        /// <param name="imgUri"></param>
        /// <returns></returns>
        internal void addImageModel(ImageModel imageModel)
        {
            this.AllImageModels.Add(imageModel);
            this.HasChanges = true;
            this.OnNewChildElement(imageModel);
        }

        /// <summary>
        /// Creates a new TextModel
        /// </summary>
        /// <param name="imgUri"></param>
        /// <returns></returns>
        internal TextModel newTextModel()
        {
            TextModel textModel = new TextModel();
            textModel.WorkspaceModel = this;
            this.AllTextModels.Add(textModel);
            this.HasChanges = true;
            this.OnNewChildElement(textModel);
            return textModel;
        }

        /// <summary>
        /// Add a TextModel to this WorkspaceModel
        /// </summary>
        /// <param name="imgUri"></param>
        /// <returns></returns>
        internal void addTextModel(TextModel textModel)
        {
            this.AllTextModels.Add(textModel);
            this.HasChanges = true;
            this.OnNewChildElement(textModel);
        }

        /// <summary>
        /// Deletes the given ImageModel
        /// </summary>
        /// <param name="imgUri"></param>
        /// <returns></returns>
        internal bool deleteImageModel(ImageModel imageModel)
        {
            this.OnDeletedChildElement(imageModel);
            return this.AllImageModels.Remove(imageModel);            
        }

        /// <summary>
        /// Deletes the given TextModel
        /// </summary>
        /// <param name="imgUri"></param>
        /// <returns></returns>
        internal bool deleteTextModel(TextModel textModel)
        {
            this.OnDeletedChildElement(textModel);
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
                this.OnDeletedChildElement(pluginModel);
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
                    //deleteConnectionModel(connectionModel);
                    this.ModifyModel(new DeleteConnectionModelOperation(connectionModel));
                }

                //remove all output ConnectionModels belonging to this Connector from our WorkspaceModel
                foreach (ConnectionModel outputConnection in new List<ConnectionModel>(connectorModel.OutputConnections))
                {
                    //deleteConnectionModel(outputConnection);
                    this.ModifyModel(new DeleteConnectionModelOperation(outputConnection));
                }
                this.HasChanges = true;
                this.OnDeletedChildElement(connectorModel);
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
            this.OnDeletedChildElement(connectionModel);
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
        /// Modify the current WorkspaceModel by using an operation
        /// returns the created object or true if its not a 'new' operation
        /// </summary>
        /// <param name="operation"></param>
        public object ModifyModel(Operation operation){

            try
            {
                HasChanges = true;
                return operation.Execute(this);                
            }
            finally
            {
                this.UndoRedoManager.DidOperation(operation);
            }

        }

        /// <summary>
        /// "Something" logged
        /// </summary>
        [field: NonSerialized]
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        /// <summary>
        /// A childs position of this WorkspaceModel changed
        /// </summary>     
        [field:NonSerialized]
        public event EventHandler<PositionArgs> ChildPositionChanged;

        /// <summary>
        /// A childs size of this WorkspaceModel changed
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<SizeArgs> ChildSizeChanged;

        /// <summary>
        /// A child of this WorkspaceModel is created
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<ModelArgs> NewChildElement;

        /// <summary>
        /// A child of this WorkspaceModel is deleted
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<ModelArgs> DeletedChildElement;

        /// <summary>
        /// A child of this WorkspaceModel is deleted
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<NameArgs> ChildNameChanged;

        /// <summary>
        /// Loggs a gui message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        internal void GuiLogMessage(IPlugin sender, GuiLogEventArgs args)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(sender, args);
            }
        }


        /// <summary>
        /// Call this to tell the environment that a childs position changed
        /// </summary>
        /// <param name="effectedModelElement"></param>
        /// <param name="oldPosition"></param>
        /// <param name="newPosition"></param>
        internal void OnChildPositionChanged(VisualElementModel effectedModelElement, Point oldPosition, Point newPosition)
        {
            if (ChildPositionChanged != null)
            {
                ChildPositionChanged(this, new PositionArgs(effectedModelElement, oldPosition, newPosition));
            }
        }

        /// <summary>
        ///  Call this to tell the environment that a childs size changed
        /// </summary>
        /// <param name="effectedModelElement"></param>
        /// <param name="oldWidth"></param>
        /// <param name="newWidth"></param>
        /// <param name="oldHeight"></param>
        /// <param name="newHeight"></param>
        internal void OnChildSizeChanged(VisualElementModel effectedModelElement, double oldWidth, double newWidth, double oldHeight, double newHeight)
        {
            if (ChildSizeChanged != null)
            {
                ChildSizeChanged(this, new SizeArgs(effectedModelElement, oldWidth, newWidth, oldHeight, newHeight));
            }
        }

        /// <summary>
        /// Call this to tell the environment that we created a new child
        /// </summary>
        /// <param name="effectedModelElement"></param>
        internal void OnNewChildElement(VisualElementModel effectedModelElement)
        {
            if (NewChildElement != null)
            {
                NewChildElement(this, new ModelArgs(effectedModelElement));
            }
        }

        /// <summary>
        /// Call this to tell the environment that we deleted a child
        /// </summary>
        /// <param name="effectedModelElement"></param>
        internal void OnDeletedChildElement(VisualElementModel effectedModelElement)
        {
            if (DeletedChildElement != null)
            {
                DeletedChildElement(this, new ModelArgs(effectedModelElement));
            }
        }

        /// <summary>
        /// Call this to tell the environment that we renamed a child
        /// </summary>
        /// <param name="effectedModelElement"></param>
        internal void OnRenamedChildElement(VisualElementModel effectedModelElement, string oldname, string newname)
        {
            if (ChildNameChanged != null)
            {
                ChildNameChanged(this, new NameArgs(effectedModelElement, oldname, newname));
            }
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
            if (connectorModelA == null)
            {
                throw new ArgumentNullException("connectorModelA may not be null");
            }
            else if (connectorModelB == null)
            {
                throw new ArgumentNullException("connectorModelB may not be null");
            }


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

    /// <summary>
    /// Event args which "knows" the effected model
    /// </summary>
    public class ModelArgs : EventArgs
    {
        public VisualElementModel EffectedModelElement { get; private set; }

        public ModelArgs(VisualElementModel effectedModelElement)
        {
            this.EffectedModelElement = effectedModelElement;
        }
    }

    /// <summary>
    /// Event args which also "knows" old and new positions
    /// </summary>
    public class PositionArgs : ModelArgs
    {
        public Point OldPosition { get; internal set; }
        public Point NewPosition { get; internal set; }
        internal PositionArgs(VisualElementModel model, Point oldPosition, Point newPosition) :
            base(model)
        {
            this.OldPosition = oldPosition;
            this.NewPosition = newPosition;
        }
    }

    /// <summary>
    /// Event args which also "knows" old and new size (Width, Height)
    /// </summary>
    public class SizeArgs : ModelArgs
    {
        public double OldWidth { get; internal set; }
        public double NewWidth { get; internal set; }
        public double OldHeight { get; internal set; }
        public double NewHeight { get; internal set; }

        internal SizeArgs(VisualElementModel model, double oldWidth, double newWidth, double oldHeight, double newHeight) :
            base(model)
        {
            this.OldWidth = oldWidth;
            this.NewWidth = newWidth;
            this.OldHeight = oldHeight;
            this.NewHeight = newHeight;
        }
    }

    /// <summary>
    /// Event args which "knows" old and new name of the model element
    /// </summary>
    public class NameArgs : ModelArgs
    {
        public string Oldname { get; internal set; }
        public string NewName { get; internal set; }

        internal NameArgs(VisualElementModel model, string oldname, string newname) :
            base(model)
        {
            this.Oldname = oldname;
            this.NewName = newname;
        }
    }
}