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
using System.Windows;

namespace Cryptool.Plugins.CostFunction
{
    class CostFunctionSettings : ISettings
    {
        #region private variables
        private bool hasChanges = false;
        private int functionType;
        private String bytesToUse = "256";
        private int bytesToUseInteger = 256;
        #endregion
        
        [TaskPane("FunctionType", "Select the type of function", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Index of coincidence", "Entropy", "Bigrams: log 2", "Bigrams: Sinkov", "Bigrams: Percentaged", "Regular Expression", "Weighted Bigrams/Trigrams"})]
        public int FunctionType
        {
            get { return this.functionType; }
            set
            {
                this.functionType = (int)value;
                UpdateTaskPaneVisibility();
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
                bytesToUseInteger = int.Parse(value);
                OnPropertyChanged("BytesToUse");
            }
        }

        public int BytesToUseInteger
        {
            get { return bytesToUseInteger; }
        }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            if (functionType.Equals(5))
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BytesToUse", Visibility.Visible)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RegEx", Visibility.Visible)));
            }
            else
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BytesToUse", Visibility.Visible)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RegEx", Visibility.Collapsed)));
            }

        }


        private string regEx;
        [TaskPane("Regular Expression:", "Regular Expression match", null, 5, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String RegEx
        {
            get
            {
                return regEx;
            }
            set
            {
                regEx = value;
                OnPropertyChanged("RegEx");
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
