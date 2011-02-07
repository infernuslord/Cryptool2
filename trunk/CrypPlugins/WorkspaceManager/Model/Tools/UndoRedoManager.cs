using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using WorkspaceManager.View.VisualComponents;
using WorkspaceManager.View.Container;
using System.Windows.Controls;
using System.Windows.Input;
using System.Drawing;

namespace WorkspaceManager.Model.Tools
{
    public class UndoRedoManager
    {
        private Stack<Operation> UndoStack = new Stack<Operation>();
        private Stack<Operation> RedoStack = new Stack<Operation>();

        /// <summary>
        /// Is an undo-operation possible?
        /// </summary>
        /// <returns></returns>
        public bool CanUndo()
        {
            return UndoStack.Count > 0;
        }

        /// <summary>
        /// Is a redo-operation possible?
        /// </summary>
        /// <returns></returns>
        public bool CanRedo()
        {
            return RedoStack.Count > 0;
        }

        /// <summary>
        /// Do undo now
        /// </summary>
        public void Undo()
        {
            Working = true;
            if (UndoStack.Count > 0)
            {
                Operation op = UndoStack.Pop();
                op.Undo();
                RedoStack.Push(op);

                if (UndoStack.Count > 0 && !UndoStack.First().SingleOperation)
                {
                    do
                    {
                        op = UndoStack.Pop();
                        op.Undo();
                        RedoStack.Push(op);
                    } while (UndoStack.Count > 0 && !UndoStack.First().SingleOperation);
                }
            }
            Working = false;
        }

        /// <summary>
        /// Do redo now
        /// </summary>
        public void Redo()
        {
            Working = true;
            if (RedoStack.Count > 0)
            {
                Operation op = null;
                do
                {
                    op = RedoStack.Pop();
                    op.Redo();
                    UndoStack.Push(op);                    
                } while (RedoStack.Count > 0 && !op.SingleOperation);

            }
            Working = false;
        }

        /// <summary>
        /// Clears undo and redo stacks
        /// </summary>
        public void ClearStacks()
        {
            UndoStack.Clear();
            RedoStack.Clear();
        }

        /// <summary>
        /// Tell the UndoRedoManager that an operation occured
        /// </summary>
        /// <param name="op"></param>
        public void DidOperation(Operation op)
        {
            if (RedoStack.Count > 0)
            {
                RedoStack.Clear();
            }
            this.UndoStack.Push(op);
        }

        public bool Working
        {
            get;
            private set;
        }
    }

    public interface Operation
    {
        void Undo();
        void Redo();
        bool SingleOperation { get; set; }
    }

    public class NewModelElementOperation : Operation{

        VisualElementModel model = null;

        public NewModelElementOperation(VisualElementModel model)
        {
            this.model = model;
        }

        #region Operation Members

        public void Undo()
        {
            if (model is PluginModel)
            {
                PluginModel pluginModel = (PluginModel)model;
                pluginModel.WorkspaceModel.deletePluginModel(pluginModel);
                if (pluginModel.UpdateableView is PluginContainerView)
                {
                    PluginContainerView pluginContainerView = (PluginContainerView)pluginModel.UpdateableView;
                    WorkSpaceEditorView workSpaceEditorView = (WorkSpaceEditorView)pluginModel.WorkspaceModel.UpdateableView;
                    workSpaceEditorView.root.Children.Remove(pluginContainerView);
                }
            }
            else if (model is ConnectorModel)
            {
                ConnectorModel connectorModel = (ConnectorModel)model;
                connectorModel.WorkspaceModel.deleteConnectorModel(connectorModel);
            }
            else if (model is ConnectionModel)
            {
                ConnectionModel connectionModel = (ConnectionModel)model;
                connectionModel.WorkspaceModel.deleteConnectionModel(connectionModel);
            }
            else if (model is TextModel)
            {
                TextModel textModel = (TextModel)model;
                textModel.WorkspaceModel.deleteTextModel(textModel);
            }
            else if (model is ImageModel)
            {
                ImageModel imageModel = (ImageModel)model;
                imageModel.WorkspaceModel.deleteImageModel(imageModel);
            }
            else
            {
                throw new NotImplementedException("Can not undo NewModelElement: " + model.GetType().Name);
            }
        }

        public void Redo()
        {
            if (model is PluginModel)
            {
                PluginModel pluginModel = (PluginModel)model;
                pluginModel.WorkspaceModel.AllPluginModels.Add(pluginModel);
                if (pluginModel.UpdateableView is PluginContainerView)
                {
                    WorkSpaceEditorView workSpaceEditorView = (WorkSpaceEditorView)pluginModel.WorkspaceModel.UpdateableView;
                    workSpaceEditorView.root.Children.Add((PluginContainerView)pluginModel.UpdateableView);
                    pluginModel.WorkspaceModel.WorkspaceManagerEditor.HasChanges = true;
                }
            }
            else if (model is ConnectorModel)
            {
                ConnectorModel connectorModel = (ConnectorModel)model;
                connectorModel.WorkspaceModel.AllConnectorModels.Add(connectorModel);
            }
            else if (model is ConnectionModel)
            {
                ConnectionModel connectionModel = (ConnectionModel)model;
                connectionModel.WorkspaceModel.AllConnectionModels.Add(connectionModel);
                connectionModel.From.OutputConnections.Add(connectionModel);
                connectionModel.To.InputConnections.Add(connectionModel);
                if (connectionModel.UpdateableView is CryptoLineView)
                {
                    CryptoLineView cryptoLineView = (CryptoLineView)connectionModel.UpdateableView;

                    WorkSpaceEditorView workSpaceEditorView = (WorkSpaceEditorView)connectionModel.WorkspaceModel.UpdateableView;
                    if (connectionModel.From.UpdateableView is ConnectorView &&
                        connectionModel.To.UpdateableView is ConnectorView)
                    {
                        ConnectorView source = (ConnectorView)connectionModel.From.UpdateableView;
                        ConnectorView target = (ConnectorView)connectionModel.To.UpdateableView;
                        CryptoLineView conn = new CryptoLineView(connectionModel, source, target);
                        conn.StartPointSource = source;
                        conn.EndPointSource = target;
                        connectionModel.UpdateableView = conn;
                        connectionModel.OnDelete += workSpaceEditorView.DeleteConnection;
                        conn.SetBinding(CryptoLineView.StartPointProperty, workSpaceEditorView.CreateConnectorBinding(source));
                        conn.SetBinding(CryptoLineView.EndPointProperty, workSpaceEditorView.CreateConnectorBinding(target));
                        workSpaceEditorView.root.Children.Add(conn);
                        workSpaceEditorView.ConnectionList.Add(conn);
                        Canvas.SetZIndex(conn, 0);
                    }                    
                }
                connectionModel.WorkspaceModel.WorkspaceManagerEditor.HasChanges = true;
            }
            else if (model is TextModel)
            {
                TextModel textModel = (TextModel)model;
                textModel.WorkspaceModel.AllTextModels.Add(textModel);
            }
            else if (model is ImageModel)
            {
                ImageModel imageModel = (ImageModel)model;
                imageModel.WorkspaceModel.AllImageModels.Add(imageModel);
            }
            else
            {
                throw new NotImplementedException("Can not redo NewModelElement: " + model.GetType().Name);
            }
        }

        public bool SingleOperation
        {
            get;
            set;
        }
        #endregion       
    }

    public class DeleteModelElementOperation : Operation
    {

        VisualElementModel model = null;

        public DeleteModelElementOperation(VisualElementModel model)
        {
            this.model = model;
        }

        #region Operation Members

        public void Undo()
        {
            if (model is PluginModel)
            {
                PluginModel pluginModel = (PluginModel)model;
                pluginModel.WorkspaceModel.AllPluginModels.Add(pluginModel);
                if (pluginModel.UpdateableView is PluginContainerView)
                {
                    WorkSpaceEditorView workSpaceEditorView = (WorkSpaceEditorView)pluginModel.WorkspaceModel.UpdateableView;
                    workSpaceEditorView.root.Children.Add((PluginContainerView)pluginModel.UpdateableView);
                    pluginModel.WorkspaceModel.WorkspaceManagerEditor.HasChanges = true;
                }
            }
            else if (model is ConnectorModel)
            {
                ConnectorModel connectorModel = (ConnectorModel)model;
                connectorModel.WorkspaceModel.AllConnectorModels.Add(connectorModel);
            }
            else if (model is ConnectionModel)
            {
                ConnectionModel connectionModel = (ConnectionModel)model;
                connectionModel.WorkspaceModel.AllConnectionModels.Add(connectionModel);
                connectionModel.From.OutputConnections.Add(connectionModel);
                connectionModel.To.InputConnections.Add(connectionModel);
                if (connectionModel.UpdateableView is CryptoLineView)
                {
                    CryptoLineView cryptoLineView = (CryptoLineView)connectionModel.UpdateableView;

                    WorkSpaceEditorView workSpaceEditorView = (WorkSpaceEditorView)connectionModel.WorkspaceModel.UpdateableView;
                    if (connectionModel.From.UpdateableView is ConnectorView &&
                        connectionModel.To.UpdateableView is ConnectorView)
                    {
                        ConnectorView source = (ConnectorView)connectionModel.From.UpdateableView;
                        ConnectorView target = (ConnectorView)connectionModel.To.UpdateableView;
                        CryptoLineView conn = new CryptoLineView(connectionModel, source, target);
                        conn.StartPointSource = source;
                        conn.EndPointSource = target;
                        connectionModel.UpdateableView = conn;
                        connectionModel.OnDelete += workSpaceEditorView.DeleteConnection;
                        conn.SetBinding(CryptoLineView.StartPointProperty, workSpaceEditorView.CreateConnectorBinding(source));
                        conn.SetBinding(CryptoLineView.EndPointProperty, workSpaceEditorView.CreateConnectorBinding(target));
                        workSpaceEditorView.root.Children.Add(conn);
                        workSpaceEditorView.ConnectionList.Add(conn);
                        Canvas.SetZIndex(conn, 0);
                    }
                }
                connectionModel.WorkspaceModel.WorkspaceManagerEditor.HasChanges = true;
            }
            else if (model is TextModel)
            {
                TextModel textModel = (TextModel)model;
                textModel.WorkspaceModel.AllTextModels.Add(textModel);
            }
            else if (model is ImageModel)
            {
                ImageModel imageModel = (ImageModel)model;
                imageModel.WorkspaceModel.AllImageModels.Add(imageModel);
            }
            else
            {
                throw new NotImplementedException("Can not undo DeleteModelElement: " + model.GetType().Name);
            }
        }

        public void Redo()
        {
            if (model is PluginModel)
            {
                PluginModel pluginModel = (PluginModel)model;
                pluginModel.WorkspaceModel.deletePluginModel(pluginModel);
                if (pluginModel.UpdateableView is PluginContainerView)
                {
                    PluginContainerView pluginContainerView = (PluginContainerView)pluginModel.UpdateableView;
                    WorkSpaceEditorView workSpaceEditorView = (WorkSpaceEditorView)pluginModel.WorkspaceModel.UpdateableView;
                    workSpaceEditorView.root.Children.Remove(pluginContainerView);
                }
            }
            else if (model is ConnectorModel)
            {
                ConnectorModel connectorModel = (ConnectorModel)model;
                connectorModel.WorkspaceModel.deleteConnectorModel(connectorModel);
            }
            else if (model is ConnectionModel)
            {
                ConnectionModel connectionModel = (ConnectionModel)model;
                connectionModel.WorkspaceModel.deleteConnectionModel(connectionModel);
            }
            else if (model is TextModel)
            {
                TextModel textModel = (TextModel)model;
                textModel.WorkspaceModel.deleteTextModel(textModel);
            }
            else if (model is ImageModel)
            {
                ImageModel imageModel = (ImageModel)model;
                imageModel.WorkspaceModel.deleteImageModel(imageModel);
            }
            else
            {
                throw new NotImplementedException("Can not redo DeleteModelElement: " + model.GetType().Name);
            }
        }

        public bool SingleOperation
        {
            get;
            set;
        }

        #endregion
    }
}
