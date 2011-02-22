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
using WorkspaceManagerModel.Model.Interfaces;

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
        internal VisualElementModel()
        {

        }

        /// <summary>
        /// Current Position of this VisualElementModel
        /// </summary>
        internal Point Position;
        
        /// <summary>
        /// Minimum Width of this VisualElementModel
        /// </summary>
        internal double MinWidth = 250;
        
        /// <summary>
        /// Minimum Height of this VisualElementModel
        /// </summary>      
        internal double MinHeight = 200;

        /// <summary>
        /// Width of this VisualElementModel
        /// </summary>
        internal double Width = 400;

        /// <summary>
        /// Height of this VisualElementModel
        /// </summary>
        internal double Height = 300;

        /// <summary>
        /// Name of this VisualElementModel
        /// </summary>
        internal string Name;

        /// <summary>
        /// Indicates that this Model Element needs a GUI Update
        /// </summary>
        public Boolean GuiNeedsUpdate { get; set; }

        /// <summary>
        /// View Element of this VisualElement
        /// </summary>
        [NonSerialized]
        public UpdateableView UpdateableView = null;

        /// <summary>
        /// Get the current Position of this VisualElementModel
        /// </summary>
        /// <returns></returns>
        public Point GetPosition()
        {
            return Position;
        }

        /// <summary>
        /// Get the minimum width of this VisualElementModel
        /// </summary>
        /// <returns></returns>        
        public double GetMinWidth()
        {
            return MinWidth;
        }

        /// <summary>
        /// Get the minimum height of this VisualElementModel
        /// </summary>
        /// <returns></returns>        
        public double GetMinHeight()
        {
            return MinHeight;
        }

        /// <summary>
        /// Get the width of this VisualElementModel
        /// </summary>
        /// <returns></returns>
        public double GetWidth()
        {
            return Width;
        }

        /// <summary>
        /// Get the current height of this VisualElementModel
        /// </summary>
        /// <returns></returns>
        public double GetHeight()
        {
            return Height;
        }

        /// <summary>
        /// Get the name of thisVisualElementModel
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return Name;
        }

        [field: NonSerialized]
        public event EventHandler<PositionArgs> PositionChanged;

        [field: NonSerialized]
        public event EventHandler<SizeArgs> SizeChanged;

        [field: NonSerialized]
        public event EventHandler<ModelArgs> NewChildElement;

        [field: NonSerialized]
        public event EventHandler<ModelArgs> DeletedChildElement;

        internal void OnPositionChanged(Point oldPosition, Point newPosition)
        {
            PositionChanged.Invoke(this, new PositionArgs(this,oldPosition, newPosition));
        }

        internal void OnSizeChanged(double oldWidth, double newWidth, double oldHeight, double newHeight)
        {
            SizeChanged.Invoke(this, new SizeArgs(this, oldWidth, newWidth, oldHeight, newHeight));
        }

        internal void OnNewChildElement(VisualElementModel effectedModelElement)
        {
            NewChildElement.Invoke(this, new ModelArgs(effectedModelElement));
        }

        internal void OnDeletedChildElement(VisualElementModel effectedModelElement)
        {
            DeletedChildElement.Invoke(this, new ModelArgs(effectedModelElement));
        }
    }

    public class ModelArgs : EventArgs
    {
        public VisualElementModel EffectedModelElement { get; private set; }

        public ModelArgs(VisualElementModel effectedModelElement)
        {
            this.EffectedModelElement = effectedModelElement;
        }
    }

    public class PositionArgs : ModelArgs
    {
        public Point OldPosition{get;internal set;}
        public Point NewPosition { get; internal set; }
        internal PositionArgs(VisualElementModel model, Point oldPosition, Point newPosition):
            base(model)
        {
            this.OldPosition = oldPosition;
            this.NewPosition = newPosition;
        }
    }

    public class SizeArgs : ModelArgs
    {
        public double OldWidth{get;internal set;}
        public double NewWidth { get; internal set; }
        public double OldHeight { get; internal set; }
        public double NewHeight { get; internal set; }

        internal SizeArgs(VisualElementModel model, double oldWidth, double newWidth, double oldHeight, double newHeight):
            base(model)
        {
            this.OldWidth = oldWidth;
            this.NewWidth = newWidth;
            this.OldHeight = oldHeight;
            this.NewHeight = newHeight;
        }
    }

    
}
