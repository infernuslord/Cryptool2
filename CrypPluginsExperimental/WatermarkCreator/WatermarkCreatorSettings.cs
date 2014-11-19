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
using WatermarkCreator.Properties;

namespace Cryptool.Plugins.WatermarkCreator
{
    // HOWTO: rename class (click name, press F2)
    public class WatermarkCreatorSettings : ISettings
    {
        #region Private Variables

        private int _watermarkAlgorithm = 0;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("ModificationType", "What kind of Watermark should be added?", null, 1, true, ControlType.ComboBox, new string[] { "Vis", "Invis", "Extr" })]
        public int ModificationType
        {
            get
            {
                return _watermarkAlgorithm;
            }
            set
            {
                if (_watermarkAlgorithm != value)
                {
                    _watermarkAlgorithm = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("ModificationType");
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

        public void Initialize()
        {

        }

        #endregion
    }
}
