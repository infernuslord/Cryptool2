/*                              
   Copyright 2011 Nils Kopal

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
using WorkspaceManager.Model;
using System.Windows;

namespace WorkspaceManagerModel.Model.Operations
{

    /// <summary>
    /// Abstract basic class representing a single operation which modifies the model
    /// </summary>
    public abstract class Operation
    {
        public Operation(VisualElementModel model){
            Model = model;
        }

        public int Identifier { get; protected set; }
        public VisualElementModel Model { get; internal set; }
        internal abstract object Execute(WorkspaceModel workspaceModel);
        internal abstract void Undo(WorkspaceModel workspaceModel);
        internal bool SavedHere;
    }

    /// <summary>
    /// Creates a new PluginModel
    /// </summary>
    public sealed class NewPluginModelOperation : Operation
    {
        private Point Position = new Point(0,0);
        private double Width = 0; 
        private double Height = 0; 
        private Type PluginType = null;

        public NewPluginModelOperation(Point position, double width, double height, Type pluginType)
            : base(null)
        {
            this.Position = position;
            this.Width = width;
            this.Height = height;
            this.PluginType = pluginType;
            this.Identifier = 0;
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel)
        {
            if (this.Model == null)
            {
                this.Model = workspaceModel.newPluginModel(this.Position,
                    this.Width,
                    this.Height,
                    this.PluginType);
            }
            else
            {
                workspaceModel.addPluginModel((PluginModel)Model);
            }
            return this.Model;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            workspaceModel.deletePluginModel((PluginModel)this.Model);
        }

        #endregion
    }

    /// <summary>
    /// Deletes an existing PluginModel
    /// </summary>
    public sealed class DeletePluginModelOperation : Operation
    {
        private Point Position = new Point(0, 0);
        private double Width = 0;
        private double Height = 0;
        private Type PluginType = null;

        public DeletePluginModelOperation(PluginModel model)
            : base(model)
        {
            this.Position = model.GetPosition();
            this.Width = model.GetWidth();
            this.Height = model.GetHeight();
            this.PluginType = model.PluginType;
            this.Identifier = model.GetHashCode();
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel)
        {
            workspaceModel.deletePluginModel((PluginModel)this.Model);
            return true;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            workspaceModel.addPluginModel((PluginModel)Model);
        }

        #endregion
    }

    /// <summary>
    /// Creates a new ConnectionModel
    /// </summary>
    public sealed class NewConnectionModelOperation : Operation
    {
        private ConnectorModel From = null;
        private ConnectorModel To = null;
        private Type ConnectionType = null;

        public NewConnectionModelOperation(ConnectorModel from, ConnectorModel to, Type connectionType) :
            base(null)
        {
            this.From = from;
            this.To = to;
            this.ConnectionType = connectionType;
            this.Identifier = 0;
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel)
        {
            if (this.Model == null)
            {
                this.Model = workspaceModel.newConnectionModel(From, To, ConnectionType);
            }
            else
            {
                workspaceModel.addConnectionModel((ConnectionModel)this.Model);
            }
            return this.Model;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            workspaceModel.deleteConnectionModel((ConnectionModel)this.Model);
        }

        #endregion
    }

    /// <summary>
    /// Deletes a ConnectionModel
    /// </summary>
    public class DeleteConnectionModelOperation : Operation
    {
        public DeleteConnectionModelOperation(ConnectionModel model) :
            base(model)
        {
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel)
        {
            workspaceModel.deleteConnectionModel((ConnectionModel)Model);
            return true;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            workspaceModel.addConnectionModel((ConnectionModel)this.Model);
        }

        #endregion
    }

    /// <summary>
    /// Creates a new ImageModel
    /// </summary>
    public sealed class NewImageModelOperation : Operation
    {
        private Uri ImgUri;

        public NewImageModelOperation(Uri imgUri)
            : base(null)
        {
            this.ImgUri = imgUri;
            this.Identifier = GetHashCode();
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel)
        {
            if (this.Model == null)
            {
                this.Model = workspaceModel.newImageModel(ImgUri);
            }
            else
            {
                workspaceModel.addImageModel((ImageModel)Model);
            }
            return this.Model;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            workspaceModel.deleteImageModel((ImageModel)this.Model);
        }

        #endregion
    }

    /// <summary>
    /// Deletes an ImageModel
    /// </summary>
    public sealed class DeleteImageModelOperation : Operation
    {
        public DeleteImageModelOperation(ImageModel model) :
            base(model)
        {
            this.Identifier = model.GetHashCode();
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel)
        {
            workspaceModel.deleteImageModel((ImageModel)Model);
            return true;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            workspaceModel.addImageModel((ImageModel)this.Model);
        }

        #endregion
    }

    /// <summary>
    /// Creates a new TextModel
    /// </summary>
    public sealed class NewTextModelOperation : Operation
    {
        public NewTextModelOperation()
            : base(null)
        {
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel)
        {
            if (this.Model == null)
            {
                this.Model = workspaceModel.newTextModel();
            }
            else
            {
                workspaceModel.addTextModel((TextModel)Model);
            }
            return this.Model;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            workspaceModel.deleteTextModel((TextModel)this.Model);
        }

        #endregion
    }

    /// <summary>
    /// Deletes a TextModel
    /// </summary>
    public sealed class DeleteTextModelOperation : Operation
    {
        public DeleteTextModelOperation(TextModel model) :
            base(model)
        {
            this.Identifier = model.GetHashCode();
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel)
        {
            workspaceModel.deleteTextModel((TextModel)Model);
            return true;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            workspaceModel.addTextModel((TextModel)this.Model);
        }

        #endregion
    }


    /// <summary>
    /// Moves the Position of an existing VisualElementModel
    /// </summary>
    public sealed class MoveModelElementOperation : Operation
    {
        private Point OldPosition = new Point(0, 0);
        private Point NewPosition = new Point(0, 0);

        public MoveModelElementOperation(VisualElementModel model, Point newPosition)
            : base(model)
        {
            this.OldPosition = model.GetPosition();
            this.NewPosition = newPosition;
            this.Identifier = model.GetHashCode();
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel)
        {
            if(OldPosition.Equals(NewPosition))
            {
                return false;
            }
            Model.Position = NewPosition;
            workspaceModel.OnChildPositionChanged(Model, OldPosition, NewPosition);
            return true;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            Model.Position = OldPosition;
            workspaceModel.OnChildPositionChanged(Model, NewPosition, OldPosition);
        }

        #endregion
    }

    /// <summary>
    /// Resizes an existing VisualElementModel
    /// </summary>
    public sealed class ResizeModelElementOperation : Operation
    {
        private double OldWidth = 0;
        private double OldHeight = 0;
        private double NewWidth = 0;
        private double NewHeight = 0;

        public ResizeModelElementOperation(VisualElementModel model, double newWidth, double newHeight)
            : base(model)
        {
            this.OldWidth = model.GetWidth();
            this.OldHeight = model.GetHeight();
            this.NewWidth = newWidth;
            this.NewHeight = newHeight;
            this.Identifier = model.GetHashCode();
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel)
        {
            if (OldWidth.Equals(NewWidth) && OldHeight.Equals(NewHeight))
            {
                return false;
            }
            Model.Width = NewWidth;
            Model.Height = NewHeight;
            workspaceModel.OnChildSizeChanged(Model, this.OldWidth, this.NewWidth, this.OldHeight, this.NewHeight);
            return true;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            Model.Width = OldWidth;
            Model.Height = OldHeight;
            workspaceModel.OnChildSizeChanged(Model, this.NewWidth, this.OldWidth, this.NewHeight, this.OldHeight);
        }

        #endregion
    }

    /// <summary>
    /// Rename a model element
    /// </summary>
    public sealed class RenameModelElementOperation : Operation
    {
        private string OldName = null;
        private string NewName = null;

        public RenameModelElementOperation(VisualElementModel model, string newName)
            : base(model)
        {
            this.OldName = model.Name;
            this.NewName = newName;
            this.Identifier = model.GetHashCode();
        }

        #region Operation Members

        internal override object Execute(WorkspaceModel workspaceModel)
        {
            if(OldName.Equals(NewName))
            {
                return false;
            }
            Model.Name = NewName;
            workspaceModel.OnRenamedChildElement(this.Model, OldName, NewName);
            return true;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            Model.Name = OldName;
            workspaceModel.OnRenamedChildElement(this.Model, NewName, OldName);
        }

        #endregion
    }

    /// <summary>
    /// Wrapper around n Operations which will operate as one single operation
    /// </summary>
    public sealed class MultiOperation : Operation
    {
        private List<Operation> _operations = null;
        
        public MultiOperation(List<Operation> operations) :
            base(null)
        {
            if (operations == null)
            {
                throw new ArgumentNullException("operations");
            }
            this._operations = operations;
            foreach (var operation in operations)
            {
                Identifier += operation.Identifier;
            }
        }

        internal override object Execute(WorkspaceModel workspaceModel)
        {
            foreach (Operation op in _operations)
            {
                op.Execute(workspaceModel);
            }
            return true;
        }

        internal override void Undo(WorkspaceModel workspaceModel)
        {
            foreach (Operation op in _operations)
            {
                op.Undo(workspaceModel);
            }
        }
    }
}
