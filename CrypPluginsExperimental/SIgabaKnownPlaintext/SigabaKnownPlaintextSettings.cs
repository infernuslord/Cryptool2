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

namespace SigabaKnownPlaintext
{
    // HOWTO: rename class (click name, press F2)
    public class SigabaKnownPlaintextSettings : ISettings
    {
        #region Private Variables

        public bool action = false;

        #endregion

        public void Initialize()
        {

        }

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Verfahren", "Verfahren", "", 2, false, ControlType.ComboBox,
              new String[] { "Verfahren zur Schlüsselsuche", "Verfahren zur schnellen Dechiffrierung"})]
        public bool Action
        {
            get { return action; }
            set
            {
                action = value;
                OnPropertyChanged("Action");
               
               

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
