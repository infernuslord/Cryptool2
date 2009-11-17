/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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

namespace Cryptool.Plugins.CostFunction
{
    class CostFunctionSettings : ISettings
    {
        #region private variables
        private bool hasChanges = false;
        private int functionType=0;
        private String bytesToUse = "256";
        #endregion
        
        [TaskPane("FunctionType", "Select the type of function", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Index of coincidence", "Entropy", "Bigrams: log 2", "Bigrams: Sinkov", "Bigrams: Percentaged", "Relative Bigram Frequency" })]
        public int FunctionType
        {
            get { return this.functionType; }
            set
            {
                functionType = value;
                OnPropertyChanged("FunctionType");
            }
        }

        [TaskPane("Bytes to use", "Which amount of bytes should be used for calculating?", null, 4, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String BytesToUse
        {
            get
            {
                return bytesToUse;
            }
            set
            {
                bytesToUse = value;
                OnPropertyChanged("bytesToUse");
            }
        }

        #region ISettings Members

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

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion
    }
    
    
}
