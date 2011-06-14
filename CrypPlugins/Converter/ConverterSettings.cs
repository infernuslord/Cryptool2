/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using Cryptool.PluginBase;

namespace Cryptool.Plugins.Converter
{
    public class ConverterSettings : ISettings
    {
        #region private variables
        private int converter = 0; // 0 = String, 1 = int, 2 = short, 3 = byte, 4 = double, 5 = bigInteger, 6= Int[] , 7=Byte[], 8=CryptoolStream
        private bool hasChanges;
        private bool numeric = false;
        private bool formatAmer = false;
        public enum EncodingTypes { Default = 0, Unicode = 1, UTF7 = 2, UTF8 = 3, UTF32 = 4, ASCII = 5, BigEndianUnicode = 6 };
        public enum PresentationFormat { Text, Hex, Base64 }
        private EncodingTypes encoding = EncodingTypes.Default;
        private PresentationFormat presentation = PresentationFormat.Text;

        public PresentationFormat Presentation
        {
            get { return this.presentation; }
            set
            {
                if (this.presentation != value) hasChanges = true;
                this.presentation = value;
                OnPropertyChanged("Presentation");
            }
        }
        #endregion

        #region taskpane

        [ContextMenu("EncodingSettingCaption", "EncodingSettingTooltip", 1, ContextMenuControlType.ComboBox, null, new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7" })]
        [TaskPane("EncodingSettingCaption", "EncodingSettingTooltip", "", 1, false, ControlType.RadioButton, new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7" })]
        public int EncodingSetting
        {
            get
            {
                return (int)this.encoding;
            }
            set
            {
                if (this.Encoding != (EncodingTypes)value)
                {
                    hasChanges = true;
                    this.Encoding = (EncodingTypes)value;
                    OnPropertyChanged("EncodingSetting");
                    HasChanges = true;
                }
            }
        }

        [TaskPane( "ConverterCaption", "ConverterTooltip", null, 1, false, ControlType.ComboBox, new string[] { "string", "int", "short", "byte", "double", "BigInteger", "int[]", "byte[]", "Cryptoolstream" })]
        public int Converter
        {
            get { return this.converter; }
            set
            {
                if (value != this.converter)
                {
                    this.converter = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("Converter");
                    HasChanges = true;

                    UpdateIcon();
                }
            }
        }

        private void settingChanged(string setting, Visibility vis)
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
        }

        internal void UpdateIcon()
        {
            ChangePluginIcon(converter + 1);
        }

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            switch (Converter)
            {
                case 0:
                    {

                        settingChanged("Numeric", Visibility.Collapsed);
                        settingChanged("EncodingSetting", Visibility.Collapsed);
                        settingChanged("PresentationFormatSetting", Visibility.Collapsed);
                        //settingChanged("Format", Visibility.Collapsed)));
                        break;
                    }
                case 1:
                    {
                        settingChanged("Numeric", Visibility.Collapsed);
                        settingChanged("EncodingSetting", Visibility.Collapsed);
                        settingChanged("Format", Visibility.Collapsed);
                        settingChanged("PresentationFormatSetting", Visibility.Collapsed);
                        break;
                    }
                case 2:
                    {
                        settingChanged("PresentationFormatSetting", Visibility.Collapsed);
                        settingChanged("EncodingSetting", Visibility.Collapsed);
                        settingChanged("Numeric", Visibility.Collapsed);
                        settingChanged("Format", Visibility.Collapsed);
                        break;
                    }
                case 3:
                    {
                        settingChanged("PresentationFormatSetting", Visibility.Collapsed);
                        settingChanged("EncodingSetting", Visibility.Collapsed);
                        settingChanged("Numeric", Visibility.Collapsed);
                        settingChanged("Format", Visibility.Collapsed);
                        break;
                    }
                case 4:
                    {
                        settingChanged("PresentationFormatSetting", Visibility.Collapsed);
                        settingChanged("EncodingSetting", Visibility.Collapsed);
                        settingChanged("Numeric", Visibility.Collapsed);
                        settingChanged("Format", Visibility.Collapsed);
                        break;
                    }
                case 5:
                    {
                        settingChanged("PresentationFormatSetting", Visibility.Collapsed);
                        settingChanged("EncodingSetting", Visibility.Collapsed);
                        settingChanged("Numeric", Visibility.Collapsed);
                        settingChanged("Format", Visibility.Collapsed);
                        break;
                    }
                case 6:
                    {
                        settingChanged("PresentationFormatSetting", Visibility.Collapsed);
                        settingChanged("EncodingSetting", Visibility.Collapsed);
                        settingChanged("Numeric", Visibility.Collapsed);
                        //settingChanged("Format", Visibility.Collapsed);
                        break;
                    }
                case 7:
                    {
                        settingChanged("PresentationFormatSetting", Visibility.Visible);
                        settingChanged("Numeric", Visibility.Visible);
                        settingChanged("EncodingSetting", Visibility.Visible);
                        //settingChanged("Format", Visibility.Collapsed);
                        break;
                    }
                case 8:
                    {
                        settingChanged("PresentationFormatSetting", Visibility.Collapsed);
                        settingChanged("EncodingSetting", Visibility.Collapsed);
                        settingChanged("Numeric", Visibility.Collapsed);
                        //settingChanged("Format", Visibility.Collapsed);
                        break;
                    }
            }
        }

        [TaskPane("NumericCaption", "NumericTooltip", null, 1, false, ControlType.ComboBox, new string[] { "NumericList1", "NumericList2" })]
        public bool Numeric
        {
            get { return this.numeric; }
            set
            {
                if (value != this.numeric)
                {
                    this.numeric = value;
                    OnPropertyChanged("Numeric");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("FormatAmerCaption", "FormatAmerTooltip", null, 1, false, ControlType.ComboBox, new string[] { "FormatAmerList1", "FormatAmerList2" })]
        public bool FormatAmer
        {
            get { return this.formatAmer; }
            set
            {
                if (value != this.formatAmer)
                {
                    this.formatAmer = value;
                    OnPropertyChanged("Format");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("PresentationFormatSettingCaption", "PresentationFormatSettingTooltip", null, 1, false, ControlType.RadioButton, new string[] { "PresentationFormatSettingList1", "PresentationFormatSettingList2", "PresentationFormatSettingList3" })]
        public int PresentationFormatSetting
        {
            get
            {
                return (int)this.presentation;
            }
            set
            {
                if (this.presentation != (PresentationFormat)value) HasChanges = true;
                this.presentation = (PresentationFormat)value;
                OnPropertyChanged("PresentationFormatSetting");
            }
        }
     
        #endregion

        #region ISettings Member

        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }

        }
        public EncodingTypes Encoding
        {
            get { return this.encoding; }
            set
            {
                if (this.encoding != value)
                {
                    hasChanges = true;
                    this.encoding = value;
                    OnPropertyChanged("EncodingSetting");
                }
            }
        }


        #endregion

        #region INotifyPropertyChanged Member

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {

        }
        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIcon(int iconIndex)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, iconIndex));
        }
        #endregion
    }
}
