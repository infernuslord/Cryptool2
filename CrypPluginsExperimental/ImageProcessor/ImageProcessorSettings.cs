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

namespace Cryptool.Plugins.ImageProcessor
{

    public enum ActionType { flip, gray, smooth, resize, rotate, invert, create, and, or };

    public class ImageProcessorSettings : ISettings
    {
        #region Private Variables

        private ActionType action = ActionType.flip;
        private int flipType = 0;
        private int smooth = 99;
        private int sizeX = 50;
        private int sizeY = 50;
        private int degrees = 90;
        private int outputFileFormat = 0;
        private static String[] comboAction = new string[] { "FLip Image", 
                                      "Gray Scale",
                                      "Create Image"};
        
        #endregion

        #region TaskPane Settings
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, true, ControlType.ComboBox, new String[] { 
            "FLip Image", 
            "Gray Scale",
            "Smooth Image",
            "Resize Image",
            "Rotate Image",
            "Invert Image",
            "Create Image" })]
        public ActionType Action
        {
            get
            {
                return this.action;
            }
            set
            {
                if (value != action)
                {
                    this.action = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("Action");
                }
            }
        }

        [TaskPane("FlipTypeCaption", "FlipTypeTooltip", null, 1, true, ControlType.ComboBox, new string[] {
            "Horizontal", 
            "Vertical" })]
        public int FlipType
        {
            get
            {
                return this.flipType;
            }
            set
            {
                if (value != flipType)
                {
                    this.flipType = value;
                    OnPropertyChanged("FlipType");
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

        [TaskPane("Smooth", "Enter the smooth value", null, 1, false, ControlType.TextBox, ValidationType.RangeInteger, 0, 5000)]
        public int Smooth
        {
            get
            {
                return smooth;
            }
            set
            {
                if (smooth != value)
                {
                    smooth = value;
                    OnPropertyChanged("smooth");
                }
            }
        }

        [TaskPane("SizeX", "Enter the value of horizontal pixels", null, 1, false, ControlType.TextBox, ValidationType.RangeInteger, 0, 5000)]
        public int SizeX
        {
            get
            {
                return sizeX;
            }
            set
            {
                if (sizeX != value)
                {
                    sizeX = value;
                    OnPropertyChanged("sizeX");
                }
            }
        }

        [TaskPane("SizeY", "Enter the value of vertical pixels", null, 1, false, ControlType.TextBox, ValidationType.RangeInteger, 0, 5000)]
        public int SizeY
        {
            get
            {
                return sizeY;
            }
            set
            {
                if (sizeY != value)
                {
                    sizeY = value;
                    OnPropertyChanged("sizeY");
                }
            }
        }

        [TaskPane("Degrees", "Enter the degrees for the rotation", null, 1, false, ControlType.TextBox, ValidationType.RangeInteger, 0, 360)]
        public int Degrees
        {
            get
            {
                return degrees;
            }
            set
            {
                if (degrees != value)
                {
                    degrees = value;
                    OnPropertyChanged("degrees");
                }
            }
        }

        internal void UpdateTaskPaneVisibility()
        {
            settingChanged("OutputFileFormat", Visibility.Visible);
            settingChanged("FlipType", Visibility.Collapsed);
            settingChanged("Smooth", Visibility.Collapsed);
            settingChanged("SizeX", Visibility.Collapsed);
            settingChanged("SizeY", Visibility.Collapsed);
            settingChanged("Degrees", Visibility.Collapsed);
            settingChanged("CustomizeRegions", Visibility.Visible);
            settingChanged("ShowRegions", Visibility.Collapsed);
            switch (Action)
            {
                case ActionType.flip: // Fliping
                    settingChanged("FlipType", Visibility.Visible);
                    break;
                case ActionType.smooth: // Smooth Image
                    settingChanged("Smooth", Visibility.Visible);
                    break;
                case ActionType.resize: // Resize Image
                    settingChanged("SizeX", Visibility.Visible);
                    settingChanged("SizeY", Visibility.Visible);
                    break;
                case ActionType.rotate: // Rotate Image
                    settingChanged("Degrees", Visibility.Visible);
                    break;
                case ActionType.create: // Create Image
                    settingChanged("SizeX", Visibility.Visible);
                    settingChanged("SizeY", Visibility.Visible);
                    break;
            }
        }

        private void settingChanged(string setting, Visibility vis)
        {
            if (TaskPaneAttributeChanged != null)
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
        }

        #endregion

        #region Events


        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

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
