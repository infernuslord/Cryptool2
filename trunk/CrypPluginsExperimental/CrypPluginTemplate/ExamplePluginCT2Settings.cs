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

namespace Cryptool.Plugins.ExamplePluginCT2
{
    // HOWTO: rename class (click name, press F2)
    public class ExamplePluginCT2Settings : ISettings
    {
        #region Private Variables

        private bool hasChanges = false;
        private int someParameter = 0;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("SomeParameter", "This is a parameter tooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int SomeParameter
        {
            get
            {
                return someParameter;
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (someParameter != value)
                {
                    someParameter = value;
                    hasChanges = true;
                }
            }
        }

        #endregion

        #region ISettings Members

        /// <summary>
        /// HOWTO: This flags indicates whether some setting has been changed since the last save.
        /// Unfortunately you have to handle this manually.
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
