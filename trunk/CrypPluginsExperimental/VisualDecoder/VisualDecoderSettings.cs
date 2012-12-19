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

namespace Cryptool.Plugins.VisualDecoder
{
    public class VisualDecoderSettings : ISettings
    {
        public enum DimCodeType { AUTO, EAN13, EAN8, Code39, Code128, QRCode, DataMatrix, PDF417 };
        private DimCodeType decodingType;
        private bool stopOnSuccess;

        #region TaskPane Settings
        
        [TaskPane("CodeTypeCaption", "CodeTypeTooltip", null, 1, true, ControlType.ComboBox, new[] { "AUTO","EAN13", "EAN8", "Code39", "Code128", "QRCode", "DataMatrix", "PDF417" })]
        public DimCodeType DecodingType
        {
            get { return decodingType; }
            set
            {
                if (value != decodingType)
                {
                    decodingType = value;
                    OnPropertyChanged("DecodingType");
                }
            }
        }

        [TaskPane("StopOnSuccessCaption", "StopOnSuccessCaptionTooltip", null, 2, true, ControlType.CheckBox)]
        public bool StopOnSuccess
        {
            get
            {
                return stopOnSuccess;
            }
            set
            {
                if (stopOnSuccess != value)
                {
                    stopOnSuccess = value;
                    OnPropertyChanged("StopOnSuccess");
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

        #endregion
    }
}
