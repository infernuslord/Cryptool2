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
using System.Windows.Forms;

namespace Cryptool.Plugins.Transcriptor
{
    // HOWTO: rename class (click name, press F2)
    public class TranscriptorSettings : ISettings
    {
        #region Private Variables

        private String fileName;
        private int color;
        private int stroke;
        private String alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        #endregion

        #region TaskPane Settings
        [TaskPane("Alphabet", "AlphabetTooltip", null, 1, false, ControlType.TextBox, null)]
        public String Alphabet
        {
            get
            {
                return alphabet;
            }
            set
            {
                if (alphabet != value)
                {
                    alphabet = value;
                        OnPropertyChanged("Alphabet");
                }
            }
        }

        [TaskPane("Load File", "LoadFileTooltip", null, 3, false, ControlType.OpenFileDialog, FileExtension = "All Files (*.*)|*.*")]
        public String FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                if (fileName != value)
                {
                    fileName = value;
                    OnPropertyChanged("FileName");
                }
            }
        }

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

        #endregion

        public void Initialize()
        {

        }

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
