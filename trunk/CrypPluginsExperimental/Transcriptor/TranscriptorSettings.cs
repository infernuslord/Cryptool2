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
using System.Windows;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.Transcriptor
{
    // HOWTO: rename class (click name, press F2)
    public class TranscriptorSettings : ISettings
    {
        #region Private Variables

        private int color;
        private int stroke;
        private int mode = 1;
        private int method = 1;
        private int threshold = 75;
 
        #endregion

        #region TaskPane Settings
        
        [TaskPane("Color", "ColorTooltip", null, 1, false, ControlType.ComboBox, new String[] { "Black", "White", "Red" })]
        public int Color
        {
            get
            {
                return color;
            }
            set
            {
                if (color != value)
                {
                    color = value;
                    OnPropertyChanged("Color");
                }
            }
        }

        [TaskPane("Stroke", "StrokeTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 3)]
        public int Stroke
        {
            get
            {
                return stroke;
            }
            set
            {
                if (stroke != value)
                {
                    stroke = value;
                    OnPropertyChanged("Stroke");
                }
            }
        }

        [TaskPane("MatchTemplate", "TemplateTooltip", "MatchTemplate", 3, false, ControlType.ComboBox, new String[] { "Off", "On" })]
        public int Mode
        {
            get
            {
                return mode;
            }
            set
            {
                if (value != mode)
                {
                    mode = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("Mode");
                }
            }
        }

        [TaskPane("ComparisionMethods", "ComparisonMethodsTooltip", "MatchTemplate", 4, false, ControlType.ComboBox, new String[] { "CCOEFF", "CCOEFF_NORMED",
            "CCORR", "CCORR_NORMED", "SQDIFF", "SQDIFF_NORMED" })]
        public int Method
        {
            get
            {
                return method;
            }
            set
            {
                if (value != method)
                {
                    method = value;
                    OnPropertyChanged("Method");
                }
            }
        }
        
        [TaskPane("Threshold", "ThresholdTooltip", "MatchTemplate", 5, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int Threshold
        {
            get
            {
                return threshold;
            }
            set
            {
                if (threshold != value)
                {
                    threshold = value;
                    OnPropertyChanged("Threshold");
                }
            }
        }

        #endregion

        #region Events

        private void UpdateTaskPaneVisibility()
        {
            if (Mode == 1)
            {
                settingChanged("Method", Visibility.Visible);
                settingChanged("Threshold", Visibility.Visible);
            }
            else
            {
                settingChanged("Method", Visibility.Collapsed);
                settingChanged("Threshold", Visibility.Collapsed);
            }
        }

        private void settingChanged(string setting, Visibility visibility)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, visibility)));
            }
        }

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
