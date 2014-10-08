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

namespace Cryptool.Plugins.ImageHash
{
    public class ImageHashSettings : ISettings
    {
        #region Private Variables

        private int outputFileFormat = 0;
        private int size = 16;
        private int Presentation_Step = 5;

        #endregion

        #region TaskPane Settings

        [TaskPane("Size (standard: 16x16)", "Enter a value of horizontal and vertical pixels, from 4 to 64", null, 1, false, ControlType.TextBox, ValidationType.RangeInteger, 0, 5000)]
        public int Size
        {
            get
            {
                return size;
            }
            set
            {
                if (size != value)
                {
                    if (size > 64)
                    {
                        size = 64;
                    }
                    else
                    {
                        size = value;
                    }
                    OnPropertyChanged("size");
                }
            }
        }

        [TaskPane("OutputFileFormatCaption", "OutputFileFormatTooltip", null, 1, true, ControlType.ComboBox, new string[] { 
            "Bmp", 
            "Png", 
            "Tiff" })]
        public int OutputFileFormat
        {
            get
            {
                return this.outputFileFormat;
            }
            set
            {
                if (value != outputFileFormat)
                {
                    this.outputFileFormat = value;
                    OnPropertyChanged("OutputFileFormat");
                }
            }
        }


        [TaskPane("PresentationStepCaption", "PresentationStepTooltip", "PresentationGroup", 71, true, ControlType.Slider, 1, 5)]
        public int PresentationStep
        {
            get { return (int)Presentation_Step; }
            set
            {
                if ((value) != Presentation_Step)
                {
                    Presentation_Step = value;
                    OnPropertyChanged("PresentationStep");
                }
            }
        }

        #endregion

        #region Events

        public void Initialize()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
