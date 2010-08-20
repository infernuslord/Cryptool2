/*                              
   Copyright 2010 Team CrypTool (Sven Rech), Uni Duisburg-Essen

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
using System.ComponentModel;
using Cryptool.PluginBase;
using System.Collections.ObjectModel;

namespace Cryptool.Plugins.DiscreteLogarithm
{
    /// <summary>
    /// Settings class for the Discrete Logarithm plugin
    /// </summary>
    class DiscreteLogarithmSettings : ISettings
    {
        #region private members

        private bool hasChanges = false;
        private int algorithm;

        #endregion

        #region events

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region public

        /// <summary>
        /// Constructs a new DiscreteLogarithmSettings
        /// </summary>
        public DiscreteLogarithmSettings()
        {
        }        

        /// <summary>
        /// Getter/Setter for the algorithm
        /// </summary>
        [ContextMenu("Algorithm", "Which algorithm do you want to use?", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "Enumeration", "Index-Calculus")]
        [TaskPane("Algorithm", "Which algorithm do you want to use?", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Enumeration", "Index-Calculus" })]
        public int Algorithm
        {
            get { return this.algorithm; }
            set
            {
                algorithm = value;
                OnPropertyChanged("Algorithm");
            }
        }

        /// <summary>
        /// Did anything change on the settigns?
        /// </summary>
        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                hasChanges = value;
            }
        }

        #endregion

        #region private

        /// <summary>
        /// The property p has changes
        /// </summary>
        /// <param name="p">p</param>
        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion

    }//end DiscreteLogarithmSettings

}//end Cryptool.Plugins.DiscreteLogarithm
