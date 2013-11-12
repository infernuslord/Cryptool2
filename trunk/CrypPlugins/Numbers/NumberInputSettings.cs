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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace Cryptool.Plugins.Numbers
{
    class NumberInputSettings : ISettings
    {
        #region Number
        private String number = "";
        //[TaskPane("NumberCaption", "NumberTooltip", null, 1, false, ControlType.TextBox)]
        public String Number
        {
            get 
            {
                return number;
            }
            set
            {
                number = value;
                OnPropertyChanged("Number");
            }
        }
        #endregion

        #region ShowDigits
        private bool showDigits = true;
        [TaskPane("ShowDigitsCaption", "ShowDigitsTooltip", "ShowDigitsGroup", 1, true, ControlType.CheckBox, "", null)]
        public bool ShowDigits
        {
            get { return showDigits; }
            set
            {
                if (value != showDigits)
                {
                    showDigits = value;
                    OnPropertyChanged("ShowDigits");
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
        }

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(p));
        }

        #endregion
    }
}
