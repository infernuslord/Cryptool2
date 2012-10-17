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
    // HOWTO: rename class (click name, press F2)
    public class DimCodeEncoderSettings : ISettings
    {
        #region Variables

        private List<string> inputList = new List<string>();  
        private Dictionary<DimCodeType, List<string>> inputVisibility = new Dictionary<DimCodeType,List<string>>();
          
        #region input Variables

        private int integerInput;
        private DimCodeType encodingType = DimCodeType.QRCode;

        #endregion

        public enum DimCodeType { QRCode, EAN8 };
        
        #endregion

        public DimCodeEncoderSettings()
        {
            //init for each enum inputVisibilityLists 
            foreach (DimCodeType name in Enum.GetValues(typeof(DimCodeType)))
            {
                inputVisibility[name] = new List<string>();
            }

            //add all inputs
            inputList.Add("IntegerInput");
            //add input for each codetype if it should be visible
            inputVisibility[DimCodeType.QRCode].Add("IntegerInput");

           
        }



        #region TaskPane Settings


        // To add more codetypes you will have to add your codeType to the stringArray below and to the DimCodeType enum.
        // if you want to have unique settings for your code type f.e. YourSettings:
        // 1. add in constructor to inputlist : inputList.Add("YourSettings");
        // 2. add your input to the inputVisibilitylist of your code type: inputVisibility[DimCodeType.QRCode].Add("YourSettings");
        [TaskPane("EncodingCaption", "EncodingTooltip", null, 1, true, ControlType.ComboBox, new string[] { "QRCode", "EAN8" })]
        public DimCodeType EncodingType
        {
            get { return this.encodingType; }
            set
            {
                if (value != this.encodingType)
                {
                    this.encodingType = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("EncodingType");

                    
                  //  UpdatePresentationTab(value);
                }
            }
        }

       
        [TaskPane("IntegerInputCaption", "IntegerInputTooltip", "CodeTypeSettings", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 65535)]
        public int IntegerInput
        {
            get { return this.integerInput; }
            set
            {
                if (value != this.integerInput)
                {
                    this.integerInput = value;
                    OnPropertyChanged("IntegerInput");
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
