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

namespace Cryptool.Plugins.CypherMatrix
{
    // HOWTO: rename class (click name, press F2)
    public class CypherMatrixSettings : ISettings
    {
        public enum CypherMatrixMode { Encrypt = 0, Decrypt = 1 , Hash = 2};

        #region Private variables and public constructor

        //private int someParameter = 0;
        private CypherMatrixMode selectedAction = CypherMatrixMode.Encrypt;
        private int code = 1;   // 1 bis 99, Standardwert: 1, individueller Anwender-Code
        private int basis = 77; // 35 bis 96, Standardwert: 77?, Zahlensystem für Expansionsfunktion
        private int matrixKeyLen = 42;  // 36 bis 64 Bytes, Standardwert: 44, Länge des Matrix-Schlüssels
        private int blockKeyLen = 63;   //35 bis 96 Bytes, Standardwert: 63, Länge des Block-Schlüssels
        private int hashBlockLen = 64;  //32-96 Bytes, Standardwert: 64?, Länge der >Hash-Sequenz<
        private bool debug = false;

        public CypherMatrixSettings()
        {
        }

        public void Initialize()
        {
            setSettingsVisibility();
        }

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        //[TaskPane("SomeParameter", "This is a parameter tooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        //public int SomeParameter
        //{
        //    get
        //    {
        //        return someParameter;
        //    }
        //    set
        //    {
        //        if (someParameter != value)
        //        {
        //            someParameter = value;
        //            // HOWTO: MUST be called every time a property value changes with correct parameter name
        //            OnPropertyChanged("SomeParameter");
        //        }
        //    }
        //}

        [TaskPane("Action", "Select the Algorithm action", null, 1, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt", "Hash" })]
        public CypherMatrixMode Action
        {
            get
            {
                return this.selectedAction;
            }
            set
            {
                if (value != selectedAction)
                {
                    
                    this.selectedAction = value;
                    OnPropertyChanged("Action");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane("User Code", "Choose an individual number to customize the calculations", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 99)]
        public int Code
        {
            get
            {
                return code;
            }
            set
            {
                if (code != value)
                {
                    code = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("Code");
                }
            }
        }

        [TaskPane("Expansion Base", "Choose an individual number to customize the expansion function", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 35, 96)]
        public int Basis
        {
            get
            {
                return basis;
            }
            set
            {
                if (basis != value)
                {
                    basis = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("Basis");
                }
            }
        }

        [TaskPane("write debug log", "Choose whether the internal parameters should be written in a logfile or not", null, 4, false, ControlType.CheckBox)]
        public bool Debug
        {
            get
            {
                return debug;
            }
            set
            {
                if (debug != value)
                {
                    debug = value;
                    OnPropertyChanged("Debug");
                }
            }
        }

        [TaskPane("Matrix Key Lenght", "Choose the lenght of the Matrix Key", "Cipher Options", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 36, 64)]
        public int MatrixKeyLen
        {
            get
            {
                return matrixKeyLen;
            }
            set
            {
                if (matrixKeyLen != value)
                {
                    matrixKeyLen = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("MatrixKeyLen");
                }
            }
        }

        [TaskPane("Block Key Lenght", "Choose the lenght of the Block Key", "Cipher Options", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 35, 96)]
        public int BlockKeyLen
        {
            get
            {
                return blockKeyLen;
            }
            set
            {
                if (blockKeyLen != value)
                {
                    blockKeyLen = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("BlockKeyLen");
                }
            }
        }

        [TaskPane("Hash Block Lenght", "Choose the lenght of the Hash Block", "Hash Options", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 32, 96)]
        public int HashBlockLen
        {
            get
            {
                return hashBlockLen;
            }
            set
            {
                if (hashBlockLen != value)
                {
                    hashBlockLen = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("HashBlockLen");
                }
            }
        }

        #endregion

        #region private Methods

        private void setSettingsVisibility()
        {
            switch (selectedAction)
            {
                case CypherMatrixMode.Encrypt:
                case CypherMatrixMode.Decrypt:
                    {
                        showSettingsElement("MatrixKeyLen");
                        showSettingsElement("BlockKeyLen");
                        hideSettingsElement("HashBlockLen");
                        break;
                    }
                case CypherMatrixMode.Hash:
                    {
                        showSettingsElement("HashBlockLen");
                        hideSettingsElement("MatrixKeyLen");
                        hideSettingsElement("BlockKeyLen");
                        break;
                    }
                default:
                    {
                        goto case CypherMatrixMode.Decrypt;
                    }
            }
        }

        private void showSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

        private void hideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// This event is needed in order to render settings elements visible/invisible
        /// </summary>
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
