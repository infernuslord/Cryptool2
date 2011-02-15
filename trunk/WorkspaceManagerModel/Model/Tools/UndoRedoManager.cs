using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

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
                object lastModelElement = null;
                Operation op = UndoStack.Pop();
                lastModelElement = op.Element;
                op.Undo();
                RedoStack.Push(op);

                if (UndoStack.Count > 0 && (!UndoStack.First().SingleOperation || 
                    (UndoStack.First() is MoveModelElementOperation &&
                    UndoStack.First().Element == lastModelElement)))
                {
                    do
                    {
                        lastModelElement = op.Element;
                        op = UndoStack.Pop();
                        op.Undo();
                        RedoStack.Push(op);
                    } while (UndoStack.Count > 0 && (!UndoStack.First().SingleOperation || (UndoStack.First() is MoveModelElementOperation &&
                    UndoStack.First().Element == lastModelElement)));
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
                object lastModelElement = null;
                do
                {
                    op = RedoStack.Pop();                    
                    lastModelElement = op.Element;
                    op.Redo();
                    UndoStack.Push(op);
                } while (RedoStack.Count > 0 && (!RedoStack.First().SingleOperation || (RedoStack.First() is MoveModelElementOperation &&
                    RedoStack.First().Element == lastModelElement)));

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
        object Element{get;}
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

        public object Element { get { return this.model; } }

        public void Undo()
        {
            if (model is PluginModel)
            {
                
            }
            else if (model is ConnectorModel)
            {
                
            }
            else if (model is ConnectionModel)
            {
                
            }
            else if (model is TextModel)
            {
               
            }
            else if (model is ImageModel)
            {

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
              
            }
            else if (model is ConnectorModel)
            {
                
            }
            else if (model is ConnectionModel)
            {
                
            }
            else if (model is TextModel)
            {
            }
            else if (model is ImageModel)
            {                

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

        public object Element { get { return this.model; } }

        public void Undo()
        {
            if (model is PluginModel)
            {
                
            }
            else if (model is ConnectorModel)
            {
               
            }
            else if (model is ConnectionModel)
            {
                
            }
            else if (model is TextModel)
            {
               
            }
            else if (model is ImageModel)
            {
               
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
               
            }
            else if (model is ConnectorModel)
            {
                
            }
            else if (model is ConnectionModel)
            {
                
            }
            else if (model is TextModel)
            {
                
            }
            else if (model is ImageModel)
            {
              
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

    public class MoveModelElementOperation : Operation
    {

        VisualElementModel model = null;

        public MoveModelElementOperation(VisualElementModel model)
        {
            this.model = model;
        }

        #region Operation Members

        public object Element { get { return this.model; } }

        public void Undo()
        {
            if (model is PluginModel)
            {
               
            }
            else if (model is ConnectorModel)
            {
                
            }
            else if (model is ConnectionModel)
            {
                
            }
            else if (model is TextModel)
            {
                
            }
            else if (model is ImageModel)
            {
               
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
                
            }
            else if (model is ConnectorModel)
            {

            }
            else if (model is ConnectionModel)
            {

            }
            else if (model is TextModel)
            {

            }
            else if (model is ImageModel)
            {

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
