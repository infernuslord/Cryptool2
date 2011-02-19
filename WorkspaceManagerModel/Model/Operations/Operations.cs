using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkspaceManager.Model;
using System.Windows;

namespace WorkspaceManagerModel.Model.Operations
{
    /// <summary>
    /// abstract basic class representing a single operation which modifies the model
    /// </summary>
    public abstract class Operation
    {
        public Operation(VisualElementModel model){
            Model = model;
        }
        public VisualElementModel Model { get; internal set; }
        public abstract void Execute();
        public abstract void Undo();        
    }

    public class NewModelElementOperation : Operation
    {
        public NewModelElementOperation(VisualElementModel model)
            : base(model)
        {

        }

        #region Operation Members
        
        public override void Execute()
        {
            if (Model is PluginModel)
            {

            }
            else if (Model is ConnectorModel)
            {

            }
            else if (Model is ConnectionModel)
            {

            }
            else if (Model is TextModel)
            {
            }
            else if (Model is ImageModel)
            {

            }
            else
            {
                throw new NotImplementedException("Can not execute NewModelElementOperation: " + Model.GetType().Name);
            }
        }

        public override void Undo()
        {
            if (Model is PluginModel)
            {

            }
            else if (Model is ConnectorModel)
            {

            }
            else if (Model is ConnectionModel)
            {

            }
            else if (Model is TextModel)
            {

            }
            else if (Model is ImageModel)
            {

            }
            else
            {
                throw new NotImplementedException("Can not undo NewModelElementOperation: " + Model.GetType().Name);
            }
        }
        #endregion
    }

    public class DeleteModelElementOperation : Operation
    {

        public DeleteModelElementOperation(VisualElementModel model)
            : base(model)
        {

        }

        #region Operation Members

        public override void Execute()
        {
            if (Model is PluginModel)
            {

            }
            else if (Model is ConnectorModel)
            {

            }
            else if (Model is ConnectionModel)
            {

            }
            else if (Model is TextModel)
            {

            }
            else if (Model is ImageModel)
            {

            }
            else
            {
                throw new NotImplementedException("Can not execute DeleteModelElementOperation: " + Model.GetType().Name);
            }
        }

        public override void Undo()
        {
            if (Model is PluginModel)
            {

            }
            else if (Model is ConnectorModel)
            {

            }
            else if (Model is ConnectionModel)
            {

            }
            else if (Model is TextModel)
            {

            }
            else if (Model is ImageModel)
            {

            }
            else
            {
                throw new NotImplementedException("Can not undo DeleteModelElementOperation: " + Model.GetType().Name);
            }
        }

        #endregion
    }

    public class MoveModelElementOperation : Operation
    {

        public MoveModelElementOperation(VisualElementModel model)
            : base(model)
        {

        }

        #region Operation Members

        public override void Execute()
        {
            if (Model is PluginModel)
            {

            }
            else if (Model is ConnectorModel)
            {

            }
            else if (Model is ConnectionModel)
            {

            }
            else if (Model is TextModel)
            {

            }
            else if (Model is ImageModel)
            {

            }
            else
            {
                throw new NotImplementedException("Can not execute MoveModelElementOperation: " + Model.GetType().Name);
            }
        }

        public override void Undo()
        {
            if (Model is PluginModel)
            {

            }
            else if (Model is ConnectorModel)
            {

            }
            else if (Model is ConnectionModel)
            {

            }
            else if (Model is TextModel)
            {

            }
            else if (Model is ImageModel)
            {

            }
            else
            {
                throw new NotImplementedException("Can not undo MoveModelElementOperation: " + Model.GetType().Name);
            }
        }

        public Point OldPosition
        {
            get;
            set;
        }

        public Point NewPosition
        {
            get;
            set;
        }

        #endregion
    }
}
