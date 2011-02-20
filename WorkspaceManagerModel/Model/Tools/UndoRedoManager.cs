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
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using WorkspaceManagerModel.Model.Operations;

namespace WorkspaceManager.Model.Tools
{
    public class UndoRedoManager
    {
        internal UndoRedoManager()
        {

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
            
        }

        /// <summary>
        /// Do redo now
        /// </summary>
        public void Redo()
        {
            
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
}
