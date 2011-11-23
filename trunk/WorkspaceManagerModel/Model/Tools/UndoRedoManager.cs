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
using WorkspaceManagerModel.Model.Operations;

namespace WorkspaceManager.Model.Tools
{
    public class UndoRedoManager
    {
        private WorkspaceModel WorkspaceModel = null;

        private bool CurrentlyWorking { get; set; }

        internal UndoRedoManager(WorkspaceModel workspaceModel)
        {
            WorkspaceModel = workspaceModel;
        }

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
            if (!CanUndo())
            {
                return;
            }
            
            this.CurrentlyWorking = true;
            try
            {
                Operation op = UndoStack.Pop();
                op.Undo(WorkspaceModel);
                RedoStack.Push(op);

                while (UndoStack.Count > 0 &&
                    op.GetType().Equals(UndoStack.Peek().GetType()) &&
                    UndoStack.Peek().Identifier ==  op.Identifier &&
                    (UndoStack.Peek() is MoveModelElementOperation ||
                    UndoStack.Peek() is ResizeModelElementOperation ||
                    UndoStack.Peek() is MultiOperation))
                {
                    op = UndoStack.Pop();
                    op.Undo(WorkspaceModel);
                    RedoStack.Push(op);
                }
            }
            finally
            {
                this.CurrentlyWorking = false;
            }
        }

        /// <summary>
        /// Do redo now
        /// </summary>
        public void Redo()
        {
            if (!CanRedo())
            {
                return;
            }

            this.CurrentlyWorking = true;
            try
            {
                Operation op = RedoStack.Pop();
                op.Execute(WorkspaceModel);
                UndoStack.Push(op);

                while (RedoStack.Count > 0 &&
                    op.GetType().Equals(RedoStack.Peek().GetType()) &&
                    RedoStack.Peek().Identifier == op.Identifier &&
                    (RedoStack.Peek() is MoveModelElementOperation ||
                    RedoStack.Peek() is ResizeModelElementOperation ||
                    RedoStack.Peek() is MultiOperation))
                {
                    op = RedoStack.Pop();
                    op.Execute(WorkspaceModel);
                    UndoStack.Push(op);
                }
            }
            finally
            {
                this.CurrentlyWorking = false;
            }
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
            //we do not notice any operation if we are currently working 
            //(means we undo or redo at this moment)
            if (CurrentlyWorking)
            {
                return;
            }

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

        internal bool SavedHere
        {
            set
            {
                foreach (var operation in UndoStack)
                {
                    operation.SavedHere = false;
                }
                foreach (var operation in RedoStack)
                {
                    operation.SavedHere = false;
                }
                if (UndoStack.Count > 0)
                {
                    UndoStack.Peek().SavedHere = value;
                }
            }
            get 
            { 
                return UndoStack.Peek().SavedHere;
            }
        }

        internal bool HasUnsavedChanges()
        {
            if (CanUndo() && SavedHere == false)
            {
                return true;
            }

            if(CanRedo())
            {
                return RedoStack.Any(operation => operation.SavedHere);
            }

            return false;
        }
    }
}
