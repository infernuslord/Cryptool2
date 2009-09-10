/*                              
   Copyright 2009 Team Cryptool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using System.ComponentModel;

namespace Cryptool.Plugins.Substring
{
    class SubstringSettings : ISettings
    {
        bool hasChanges = false;

        #region taskPane

        private int integerLengthValue;
        [TaskPane("Length value", "Integer value for the length.", null, 0, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int IntegerLengthValue
        {
            get { return this.integerLengthValue; }
            set
            {
                if (value != this.integerLengthValue)
                {
                    this.integerLengthValue = value;
                    OnPropertyChanged("IntegerLengthValue");
                    HasChanges = true;
                }
            }
        }

        private int integerStartValue;
        [TaskPane("Start value", "Integer value to start from.", null, 0, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int IntegerStartValue
        {
            get { return this.integerStartValue; }
            set
            {
                if (value != this.integerStartValue)
                {
                    this.integerStartValue = value;
                    OnPropertyChanged("IntegerStartValue");
                    HasChanges = true;
                }
            }
        }


        #endregion taskPane



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
