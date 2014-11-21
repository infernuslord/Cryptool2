/*
   Copyright 2014 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Windows;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.WatermarkCreator
{
    // HOWTO: rename class (click name, press F2)
    public class WatermarkCreatorSettings : ISettings
    {
        #region Private Variables

        private int _watermarkAlgorithm = 0;
        private int _textSize = 0;
        private string _font = "arial";
        private int _location = 1;
        private double _opacity = 1.0;
        private int _boxSize = 10;
        private int _errorCorrection = 0;
        private long _s1 = 19;
        private long _s2 = 24;
        private int _locPercentage = 0;
        private int _advanced = 0;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("ModificationTypeCap", "ModificationTypeDes", null, 0, false, ControlType.ComboBox, new string[] { "WatermarkCreatorSettings_ModificationType_EmbedText", "WatermarkCreatorSettings_ModificationType_EmbedInvisibleText", "WatermarkCreatorSettings_ModificationType_ExtractText" })]
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
                    OnPropertyChanged("ModificationType");
                }
            }
        }


        [TaskPane("TextSizeMaxCap", "TextSizeMaxDes", null, 1, false, ControlType.TextBox)]
        public int TextSizeMax
        {
            get
            {
                return _textSize;
            }
            set
            {
                if (_textSize != value)
                {
                    _textSize = value;
                    OnPropertyChanged("TextSizeMax");
                }
            }
        }

        [TaskPane("FontTypeCap", "FontTypeDes", null, 1, false, ControlType.ComboBox, new string[] { "arial", "other" })]
        public string FontType
        {
            get
            {
                return _font;
            }
            set
            {
                if (_font != value)
                {
                    _font = value;
                    OnPropertyChanged("FontType");
                }
            }
        }

        [TaskPane("WatermarkLocationCap", "WatermarkLocationDes", null, 1, false, ControlType.ComboBox, new string[] { "TopLoc", "BotLoc", "OtherLoc" })]
        public int WatermarkLocation
        {
            get
            {
                return _location;
            }
            set
            {
                if (_location != value)
                {
                    _location = value;
                    OnPropertyChanged("WatermarkLocation");
                }
            }
        }

        [TaskPane("LocationPercentageCap", "LocationPercentageDes", null, 2, false, ControlType.Slider, 5, 95)]
        public int LocationPercentage
        {
            get
            {
                return _locPercentage;
            }
            set
            {
                if (_locPercentage != value)
                {
                    _locPercentage = value;
                    OnPropertyChanged("LocationPercentage");
                }
            }
        }

        [TaskPane("OpacityCap", "OpacityDes", null, 2, false, ControlType.Slider, 0, 1000)]
        public double Opacity
        {
            get
            {
                return _opacity;
            }
            set
            {
                if (_opacity != value)
                {
                    _opacity = value;
                    OnPropertyChanged("Opacity");
                }
            }
        }

        [TaskPane("BoxSizeCap", "BoxSizeDes", null, 1, false, ControlType.TextBox)]
        public int BoxSize
        {
            get
            {
                return _boxSize;
            }
            set
            {
                if (_boxSize != value)
                {
                    _boxSize = value;
                    OnPropertyChanged("BoxSize");
                }
            }
        }

        [TaskPane("ErrorCorrectionCap", "ErrorCorrectionDes", null, 1, false, ControlType.TextBox)]
        public int ErrorCorrection
        {
            get
            {
                return _errorCorrection;
            }
            set
            {
                if (_errorCorrection != value)
                {
                    _errorCorrection = value;
                    OnPropertyChanged("ErrorCorrection");
                }
            }
        }

        [TaskPane("Seed1", "Seed", null, 1, false, ControlType.TextBox)]
        public long Seed1
        {
            get
            {
                return _s1;
            }
            set
            {
                if (_s1 != value)
                {
                    _s1 = value;
                    OnPropertyChanged("Seed1");
                }
            }
        }

        [TaskPane("Seed2", "Seed", null, 1, false, ControlType.TextBox)]
        public long Seed2
        {
            get
            {
                return _s2;
            }
            set
            {
                if (_s2 != value)
                {
                    _s2 = value;
                    OnPropertyChanged("Seed2");
                }
            }
        }

        [TaskPane("AdvancedModeCap", "AdvancedModeDes", null, 1, false, ControlType.ComboBox, new string[] { "No", "Yes" })]
        public int AdvancedMode
        {
            get
            {
                return _advanced;
            }
            set
            {
                if (_advanced != value)
                {
                    _advanced = value;
                    OnPropertyChanged("AdvancedMode");
                }
            }
        }


        public void UpdateTaskPaneVisibility()
        {
            SettingChanged("ModificationType", Visibility.Visible);
            SettingChanged("TextSizeMax", Visibility.Collapsed);
            SettingChanged("FontType", Visibility.Collapsed);
            SettingChanged("WatermarkLocation", Visibility.Collapsed);
            SettingChanged("Opacity", Visibility.Collapsed);
            SettingChanged("BoxSize", Visibility.Collapsed);
            SettingChanged("ErrorCorrection", Visibility.Collapsed);
            SettingChanged("Seed1", Visibility.Collapsed);
            SettingChanged("Seed2", Visibility.Collapsed);
            SettingChanged("LocationPercentage", Visibility.Collapsed);

            switch (ModificationType)
            {
                case 0: //Visible Watermark (embedding)
                    SettingChanged("TextSizeMax", Visibility.Visible);
                    SettingChanged("FontType", Visibility.Visible);
                    SettingChanged("WatermarkLocation", Visibility.Visible);
                    break;
                case 1: //Invisible Watermark (embedding)
                    SettingChanged("AdvancedMode", Visibility.Visible);
                    break;
                case 2: //Invisible Watermark (extracting)
                    SettingChanged("Opacity", Visibility.Visible);
                    SettingChanged("BoxSize", Visibility.Visible);
                    SettingChanged("ErrorCorrection", Visibility.Visible);
                    SettingChanged("Seed1", Visibility.Visible);
                    SettingChanged("Seed2", Visibility.Visible);
                    break;
            }
        }

        public void UpdateSlider()
        {
            switch (WatermarkLocation)
            {
                case 2:
                    SettingChanged("LocationPercentage", Visibility.Visible);
                    break;
                default:
                    SettingChanged("LocationPercentage", Visibility.Collapsed);
                    break;
            }
        }

        public void UpdateAdvanced()
        {
            switch (AdvancedMode)
            {
                case 1:
                    SettingChanged("Opacity", Visibility.Visible);
                    SettingChanged("BoxSize", Visibility.Visible);
                    SettingChanged("ErrorCorrection", Visibility.Visible);
                    SettingChanged("Seed1", Visibility.Visible);
                    SettingChanged("Seed2", Visibility.Visible);
                    break;
                case 0:
                    SettingChanged("Opacity", Visibility.Collapsed);
                    SettingChanged("BoxSize", Visibility.Collapsed);
                    SettingChanged("ErrorCorrection", Visibility.Collapsed);
                    SettingChanged("Seed1", Visibility.Collapsed);
                    SettingChanged("Seed2", Visibility.Collapsed);
                    break;
            }
        }
        private void SettingChanged(string setting, Visibility vis)
        {
            if (TaskPaneAttributeChanged != null)
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

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
