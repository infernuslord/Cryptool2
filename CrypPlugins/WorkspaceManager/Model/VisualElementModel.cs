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
        /// I will be deleted
        /// </summary>
        public event EventHandler<EventArgs> Delete;

        /// <summary>
        /// X-Coordinate of this VisualElementModel
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y-Coordinate of this VisualElementModel
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Minimum Width of this VisualElementModel
        /// </summary>
        public double MinWidth { get; set; }

        /// <summary>
        /// Minimum Height of this VisualElementModel
        /// </summary>
        public double MinHeight { get; set; }

        /// <summary>
        /// Width of this VisualElementModel
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Height of this VisualElementModel
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Name of this VisualElementModel
        /// </summary>
        public string Name { get; set; }
    }

}
