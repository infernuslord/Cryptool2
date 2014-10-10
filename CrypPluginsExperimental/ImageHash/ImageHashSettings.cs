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
        private int presentationStep = 5;
        private static String stepName = "Step 4: Black and White";

        #endregion

        #region TaskPane Settings

        [TaskPane("Size (standard: 16x16)", "Enter a value of horizontal and vertical pixels, from 4 to 128, only powers of two.", null, 1, false, ControlType.TextBox, ValidationType.RangeInteger, 0, 5000)]
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
                    if (size > 128)
                    {
                        size = 128;
                    }
                    else
                    {
                        size = value;
                    }
                    OnPropertyChanged("size");
                }
            }
        }

        [TaskPane("OutputFileFormatCaption", "OutputFileFormatTooltip", null, 2, true, ControlType.ComboBox, new string[] { 
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
        [TaskPane("", "StepNameTooltip", "SliderGroup", 3, false, ControlType.TextBoxReadOnly)]
        public String StepName
        {
            get { return stepName; }
            set
            {
                if ((value) != stepName)
                {
                    stepName = value;
                    OnPropertyChanged("StepName");
                }
            }
        }

        [TaskPane("", "PresentationStepTooltip", "SliderGroup", 4, true, ControlType.Slider, 1, 5)]
        public int PresentationStep
        {
            get { return (int)presentationStep; }
            set
            {
                if ((value) != presentationStep)
                {
                    presentationStep = value;
                    switch (presentationStep)
                    {
                        case 1:
                            StepName = "Original Image";
                            break;
                        case 2:
                            StepName = "Step 1: Gray scale";
                            break;
                        case 3:
                            StepName = "Step 2: Resize";
                            break;
                        case 4:
                            StepName = "Step 3: Flip";
                            break;
                        case 5:
                            StepName = "Step 4: Black and White";
                            break;
                    }
                    OnPropertyChanged("StepName");
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
