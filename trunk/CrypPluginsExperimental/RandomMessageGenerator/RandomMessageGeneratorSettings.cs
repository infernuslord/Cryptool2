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
using System.Numerics;
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.RandomMessageGenerator
{
    
    public class RandomMessageGeneratorSettings : ISettings
    {
        #region Private Variables

        private int messageLimit = 1;

        #endregion

        #region TaskPane Settings

       
        [TaskPane("MessageLimit", "Maximum limit of message", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int MessageLimit
        {
            get
            {
                return messageLimit;
            }
            set
            {
                if (messageLimit != value)
                {
                    if (value < 1)
                    {
                        messageLimit = 1;
                    }
                    else
                    {
                        messageLimit = value;
                    }
                  
                    OnPropertyChanged("MessageLimit");
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
