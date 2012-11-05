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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.DimCodeEncoder
{
    public class DimCodeEncoderSettings : ISettings
    {
        #region Variables

        private readonly List<string> inputList = new List<string>();  
        private readonly Dictionary<DimCodeType, List<string>> inputVisibility = new Dictionary<DimCodeType,List<string>>();
        private readonly DimCodeEncoder caller;
        #region input Variables

        private bool appendICV = true;
        private DimCodeType encodingType;
        private bool pdf417CompactMode;

        #endregion

        public enum DimCodeType { EAN13, EAN8, Code39, Code128, QRCode, DataMatrix, PDF417 };

        #endregion

        public DimCodeEncoderSettings(DimCodeEncoder caller)
        {
            this.caller = caller;
            //init for each enum inputVisibilityLists 
            foreach (DimCodeType name in Enum.GetValues(typeof(DimCodeType)))
            {
                inputVisibility[name] = new List<string>();
            }

            //add all inputs
            inputList.Add("AppendICV");

            //add input for each codetype if it should be visible
            inputVisibility[DimCodeType.EAN8].Add("AppendICV");
            inputVisibility[DimCodeType.EAN13].Add("AppendICV");
            inputVisibility[DimCodeType.Code39].Add("AppendICV");
            UpdateTaskPaneVisibility();
        }



        #region TaskPane Settings

        [TaskPane("EncodingCaption", "EncodingTooltip", null, 1, true, ControlType.ComboBox, new[] { "EAN13", "EAN8", "Code39", "Code128", "QRCode", "DataMatrix", "PDF417" })]
        public DimCodeType EncodingType
        {
            get { return encodingType; }
            set
            {
                if (value != encodingType)
                {
                    encodingType = value;
                    caller.Execute();
                    OnPropertyChanged("EncodingType");
                    UpdateTaskPaneVisibility();
                    
                }
            }
        }


        [TaskPane("AppendICVCaption", "AppendICVCaptionTooltip", "BarcodeSection", 2, true, ControlType.CheckBox)]
        public bool AppendICV
        {
            get
            {
                return appendICV;
            }
            set
            {
                if (appendICV != value)
                {
                    appendICV = value;
                    OnPropertyChanged("AppendICV");
                }
            }
        }

       
        #endregion

        #region Visualisation updates

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            foreach (var tpac in inputList.Select(input => new TaskPaneAttribteContainer(input, (inputVisibility[EncodingType].Contains(input)) ? Visibility.Visible : Visibility.Collapsed)))
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tpac));
            }
        }

        #endregion

        #region Events

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event PropertyChangedEventHandler PropertyChanged;
        

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }
    
        #endregion
    }
}
