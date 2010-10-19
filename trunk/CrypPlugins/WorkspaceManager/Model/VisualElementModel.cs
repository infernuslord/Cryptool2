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
using System.Linq;
using System.Text;
using System.Windows;
using WorkspaceManager.View.Interface;

namespace WorkspaceManager.Model
{
    /// <summary>
    /// Abstract super class for all Model Elements which will have
    /// a representation by a View class. It is needed to represent 
    /// Coordinates and Dimensions
    /// </summary>
    [Serializable]
    public abstract class VisualElementModel
    {
        /// <summary>
        /// Called when this object is deleted
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<EventArgs> OnDelete;

        /// <summary>
        /// Triggers event OnDelete
        /// </summary>
        public void onDelete(){
            if(OnDelete != null){
                EventArgs args = new EventArgs();         
                OnDelete.Invoke(this, args);
            }
        }

        /// <summary>
        /// Current Position of this VisualElementModel
        /// </summary>
        public Point Position;

        /// <summary>
        /// Minimum Width of this VisualElementModel
        /// </summary>
        public double MinWidth = 250;
        
        /// <summary>
        /// Minimum Height of this VisualElementModel
        /// </summary>      
        public double MinHeight = 200;

        /// <summary>
        /// Width of this VisualElementModel
        /// </summary>
        public double Width = 400;

        /// <summary>
        /// Height of this VisualElementModel
        /// </summary>
        public double Height = 300;

        /// <summary>
        /// Name of this VisualElementModel
        /// </summary>
        public string Name;

        /// <summary>
        /// Indicates that this Model Element needs a GUI Update
        /// </summary>
        public Boolean GuiNeedsUpdate { get; set; }

        /// <summary>
        /// View Element of this VisualElement
        /// </summary>
        [NonSerialized]
        public IUpdateableView UpdateableView = null;   
    }
}
