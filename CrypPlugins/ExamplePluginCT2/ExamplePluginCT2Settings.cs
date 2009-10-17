/*
   Copyright 2009 Matthäus Wander, Universität Duisburg-Essen

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

namespace Cryptool.Plugin.ExamplePluginCT2
{
    public class ExamplePluginCT2Settings : ISettings
    {
        #region Private Variables

        private bool hasChanges = false;
        private int subtrahend = 0;

        #endregion

        #region TaskPane Settings

        #region ISettings Members

        [TaskPane("Subtrahend", "Amount to subtract from input number", null, 1, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int Subtrahend
        {
            get
            {
                return subtrahend;
            }
            set
            {
                if (subtrahend != value)
                {
                    subtrahend = value;
                    hasChanges = true;
                }
            }
        }

        #endregion

        /// <summary>
        /// need to store the change status of the plugin
        /// if a property was changed -> hasChangess = true
        /// hence CrypTool will ask automatically if you want to save your changes
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

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
