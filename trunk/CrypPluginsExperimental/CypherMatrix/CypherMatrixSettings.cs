﻿/*
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
    public class CypherMatrixSettings : ISettings
    {
        public enum CypherMatrixMode { Encrypt = 0, Decrypt = 1 , Hash = 2};
        //public enum Permutation { A = 0, B = 1, C = 2, D = 3 }; // Variante A ist nicht für die Implementierung vorgesehen; bei Aktivierung die Funktion Perm ändern!
        public enum Permutation { B = 0, C = 1, D = 2 };
        public enum CypherMatrixHashMode { SMX = 0, FMX = 1, LCX = 2 , Mini = 3};

        #region Private variables and public constructor

        private CypherMatrixMode selectedAction = CypherMatrixMode.Encrypt;
        private Permutation selectedPerm = Permutation.B;
        private CypherMatrixHashMode selectedHash = CypherMatrixHashMode.FMX;
        private int code = 1;   // 1 bis 99, Standardwert: 1, individueller Anwender-Code
        private int basis = 77; // 35 bis 96, Standardwert: 77?, Zahlensystem für Expansionsfunktion
        private int matrixKeyLen = 42;  // 36 bis 64 Bytes, Standardwert: 44, Länge des Matrix-Schlüssels
        private int blockKeyLen = 63;   //35 bis 96 Bytes, Standardwert: 63, Länge des Block-Schlüssels
        private int hashBlockLen = 64;  //32-96 Bytes, Standardwert: 64?, Länge der >Hash-Sequenz<
        private bool debug = false;

        public CypherMatrixSettings()
        {
            Initialize();
        }

        public void Initialize()
        {
            setSettingsVisibility();
        }

        #endregion

        #region TaskPane Settings

        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "CypherMatrixMode0", "CypherMatrixMode1", "CypherMatrixMode2" })]
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

        [TaskPane("UserCodeCaption", "UserCodeTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 99)]
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
                    OnPropertyChanged("Code");
                }
            }
        }

        [TaskPane("ExpansionBaseCaption", "ExpansionBaseTooltip", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 35, 96)]
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
                    OnPropertyChanged("Basis");
                }
            }
        }

        [TaskPane("PermCaption", "PermTooltip", null, 4, false, ControlType.ComboBox, new string[] { "PermOption1", "PermOption2", "PermOption3" })]
        public Permutation Perm
        {
            get
            {
                return this.selectedPerm;
            }
            set
            {
                if (value != selectedPerm)
                {

                    this.selectedPerm = value;
                    OnPropertyChanged("Perm");
                }
            }
        }

        [TaskPane("WriteDebugLogCaption", "WriteDebugLogTooltip", null, 5, false, ControlType.CheckBox)]
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

        [TaskPane("MatrixKeySizeCaption", "MatrixKeySizeTooltip", "CipherOptionsGroup", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 36, 64)]
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
                    OnPropertyChanged("MatrixKeyLen");
                }
            }
        }

        [TaskPane("BlockSizeCaption", "BlockSizeTooltip", "CipherOptionsGroup", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 35, 96)]
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
                    OnPropertyChanged("BlockKeyLen");
                }
            }
        }

        [TaskPane("HashBlockSizeCaption", "HashBlockSizeTooltip", "HashOptionsGroup", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 32, 96)]
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
                    OnPropertyChanged("HashBlockLen");
                }
            }
        }

        [TaskPane("HashModeCaption", "HashModeTooltip", "HashOptionsGroup", 2, false, ControlType.ComboBox, new string[] { "HashModeSMX", "HashModeFMX", "HashModeLCX", "HashModeMini" })]
        public CypherMatrixHashMode HashMode
        {
            get
            {
                return this.selectedHash;
            }
            set
            {
                if (value != selectedHash)
                {

                    this.selectedHash = value;
                    OnPropertyChanged("HashMode");
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
                        hideSettingsElement("HashMode");
                        break;
                    }
                case CypherMatrixMode.Hash:
                    {
                        showSettingsElement("HashBlockLen");
                        showSettingsElement("HashMode");
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
