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
using System.Windows;

namespace Cryptool.Plugins.Keccak
{
    public class KeccakSettings : ISettings
    {
        #region variables

        private struct KeccakFunction
        {
            public keccakFunctionName name;
            public int outputLength, rate, capacity;

            public KeccakFunction(keccakFunctionName name, int outputLength, int rate, int capacity)
            {
                this.name = name;
                this.outputLength = outputLength;
                this.rate = rate;
                this.capacity = capacity;
            }
        }

        /* define Keccak SHA3 versions */
        private static KeccakFunction Keccak = new KeccakFunction(keccakFunctionName.Keccak, 256, 1024, 576);       // default Keccak with variable output length
        private static KeccakFunction Keccak224 = new KeccakFunction(keccakFunctionName.Keccak224, 224, 1152, 448); // proposal for SHA3-224
        private static KeccakFunction Keccak256 = new KeccakFunction(keccakFunctionName.Keccak256, 256, 1088, 512); // proposal for SHA3-256
        private static KeccakFunction Keccak384 = new KeccakFunction(keccakFunctionName.Keccak384, 384, 832, 768);  // proposal for SHA3-384
        private static KeccakFunction Keccak512 = new KeccakFunction(keccakFunctionName.Keccak512, 512, 576, 1024); // proposal for SHA3-512
        
        /* order must be the same like in the TaskPane ComboBox */
        private enum keccakFunctionName { Keccak, Keccak224, Keccak256, Keccak384, Keccak512 };
        private KeccakFunction[] KeccakFunctions = new KeccakFunction[] { Keccak, Keccak224, Keccak256, Keccak384, Keccak512 };

        /* define default settings */
        private keccakFunctionName selectedKeccakFunction = Keccak256.name;
        private int outputLength = 256, rate = 1088, capacity = 512, stateSize = 1600;        

        private bool manualSettings = false;

        private enum stateSizeName { bits25, bits50, bits100, bits200, bits400, bits800, bits1600 };
        private stateSizeName selectedStateSize = stateSizeName.bits1600;

        #endregion
        
        [TaskPane("KECCAKFunctionCaption", "KECCAKFunctionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "KeccakFunctionList1", "KeccakFunctionList2", "KeccakFunctionList3", "KeccakFunctionList4", "KeccakFunctionList5" })]
        public int KECCAKFunction
        {
            get { return (int)this.selectedKeccakFunction; }
            set
            {
                this.selectedKeccakFunction = KeccakFunctions[value].name;
                OnPropertyChanged("KECCAKFunction");

                if (KeccakFunctions[value].name == keccakFunctionName.Keccak)
                {
                    if (!manualSettings)
                    {
                        ManualSettings = true;
                    }

                    SelectedStateSize = (int)stateSizeName.bits1600;
                }
                else
                {
                    ManualSettings = false;
                }

                OutputLength = KeccakFunctions[value].outputLength;
                Rate = KeccakFunctions[value].rate;
                Capacity = KeccakFunctions[value].capacity;
                stateSize = KeccakFunctions[value].rate + KeccakFunctions[value].capacity;
            }
        }

        [TaskPane("ManualSettingsCaption", "ManualSettingsTooltip", "ParametersCaption", 0, false, ControlType.CheckBox)]
        public bool ManualSettings
        {
            get { return this.manualSettings; }
            set
            {
                if (manualSettings != value)
                {
                    manualSettings = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("ManualSettings");

                    if (manualSettings)
                    {
                        KECCAKFunction = (int)keccakFunctionName.Keccak;
                    }
                    else
                    {
                        KECCAKFunction = (int)keccakFunctionName.Keccak256;
                    }
                }
            }
        }  

        #region variable Task Pane settings

        [TaskPane("SelectedStateSizeCaption", "SelectedStateSizeTooltip", "ParametersCaption", 1, false, ControlType.ComboBox, new string[] { "StateSize0", "StateSize1", "StateSize2", "StateSize3", "StateSize4", "StateSize5", "StateSize6" })]
        public int SelectedStateSize
        {
            get { return (int)this.selectedStateSize; }
            set
            {
                this.selectedStateSize = (stateSizeName)value;
                OnPropertyChanged("SelectedStateSize");

                /* set default values for rate and capacity */
                switch (value)
                {
                    case 0: this.stateSize = 25; Rate = 16; Capacity = 9; break;
                    case 1: this.stateSize = 50; Rate = 32; Capacity = 18; break;
                    case 2: this.stateSize = 100; Rate = 64; Capacity = 36; break;
                    case 3: this.stateSize = 200; Rate = 128; Capacity = 72; break;
                    case 4: this.stateSize = 400; Rate = 256; Capacity = 144; break;
                    case 5: this.stateSize = 800; Rate = 512; Capacity = 288; break;
                    case 6: this.stateSize = 1600; Rate = 1024; Capacity = 576; break;
                    default: break;
                }
            }
        }

        [TaskPane("OutputLengthCaption", "OutputLengthTooltip", "ParametersCaption", 2, false, ControlType.TextBox)]
        public int OutputLength
        {
            get { return this.outputLength; }
            set
            {
                this.outputLength = (int)value;
                OnPropertyChanged("OutputLength");
            }
        }



        [TaskPane("RateCaption", "RateTooltip", "ParametersCaption", 3, false, ControlType.TextBox)]
        public int Rate
        {
            get { return this.rate; }
            set
            {
                this.rate = (int)value;
                OnPropertyChanged("Rate");
            }
        }

        [TaskPane("CapacityCaption", "CapacityTooltip", "ParametersCaption", 4, false, ControlType.TextBox)]
        public int Capacity
        {
            get { return this.capacity; }
            set
            {
                this.capacity = (int)value;
                OnPropertyChanged("Capacity");
            }
        }

        #endregion

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        /*  used for verification of rate and capacity size in Keccak.PreExecution() */
        public int GetStateSize()
        {
            return this.stateSize;
            
        }

        private void settingChanged(string setting, Visibility vis)
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
        }

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            if (ManualSettings)
            {
                settingChanged("SelectedStateSize", Visibility.Visible);
                settingChanged("OutputLength", Visibility.Visible);
                settingChanged("Rate", Visibility.Visible);
                settingChanged("Capacity", Visibility.Visible);
            }
            else
            {
                settingChanged("SelectedStateSize", Visibility.Collapsed);
                settingChanged("OutputLength", Visibility.Collapsed);
                settingChanged("Rate", Visibility.Collapsed);
                settingChanged("Capacity", Visibility.Collapsed);
            }
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
