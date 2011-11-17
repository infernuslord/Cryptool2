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

        private int algorithm = 0;

        #endregion

        #region events

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region public      

        /// <summary>
        /// Getter/Setter for the algorithm. Currently disabled in UI as Index Calculus is not fully working.
        /// </summary>
        //[ContextMenu( "AlgorithmCaption", "AlgorithmTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "AlgorithmList1", "AlgorithmList2")]
        //[TaskPane( "AlgorithmCaption", "AlgorithmTooltip", null, 1, false, ControlType.ComboBox, new string[] { "AlgorithmList1", "AlgorithmList2" })]
        public int Algorithm
        {
            get { return this.algorithm; }
            set
            {
                algorithm = value;
                OnPropertyChanged("Algorithm");
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
