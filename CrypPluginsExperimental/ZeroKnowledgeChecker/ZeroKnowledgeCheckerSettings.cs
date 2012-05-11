/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Numerics;
namespace Cryptool.Plugins.ZeroKnowledgeChecker
{
   
    public class ZeroKnowledgeCheckerSettings : ISettings
    {
        #region Private Variables

        private int ammountOfAttempts = 0;

        private int ammountOfOptions = 1;


        #endregion

        #region TaskPane Settings

        
        [TaskPane("AmmountOfAttempts", "Ammount of attempts", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int AmmountOfAttempts
        {
            get
            {
                return ammountOfAttempts;
            }
            set
            {
                if (ammountOfAttempts != value)
                {
                    if (value < 0)
                    {
                        ammountOfAttempts = 0;
                    }
                    else
                    {
                        ammountOfAttempts = value;
                    }
                  
                    OnPropertyChanged("AmmountOfAttempts");
                }
            }
        }


        [TaskPane("AmmountOfOptions", "Ammount of options", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int AmmountOfOptions
        {
            get
            {
                return ammountOfOptions;
            }
            set
            {
                if (ammountOfOptions != value)
                {
                    if (value < 1)
                    {
                        ammountOfOptions = 1;
                    }
                    else
                    {
                        ammountOfOptions = value;
                    }
                   
                    OnPropertyChanged("AmmountOfOptions");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
