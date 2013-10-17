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
using System.Collections.ObjectModel;
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Sigaba.Properties;

namespace Sigaba
{
    // HOWTO: rename class (click name, press F2)
    public class SigabaSettings : ISettings
    {
        #region Private Variables

        private ObservableCollection<string> _typeStrings = new ObservableCollection<string>();
        private ObservableCollection<string> _actionStrings = new ObservableCollection<string>();
        private ObservableCollection<string> _cipherControlRotorStrings = new ObservableCollection<string>();
        private ObservableCollection<string> _indexRotorStrings = new ObservableCollection<string>();

        public string _cipherKey = "OOOOO";
        public string _controlKey = "OOOOO";
        public string _indexKey = "00000";

        public int _type = 0;

        private int _presentationSpeed = 100;

        private int _unknownSymbolHandling = 0;
        private int _caseHandling = 0;

        private int _cipherRotor1 = 1;
        private int _cipherRotor2 = 2;
        private int _cipherRotor3 = 3;
        private int _cipherRotor4 = 4;
        private int _cipherRotor5 = 5;

        private int _controlRotor1 = 6;
        private int _controlRotor2 = 7;
        private int _controlRotor3 = 8;
        private int _controlRotor4 = 9;
        private int _controlRotor5 = 10;

        private int _indexRotor1 = 1;
        private int _indexRotor2 = 2;
        private int _indexRotor3 = 3;
        private int _indexRotor4 = 4;
        private int _indexRotor5 = 5;

        private Boolean _cipherRotor1Reverse = false;
        private Boolean _cipherRotor2Reverse = false;
        private Boolean _cipherRotor3Reverse = false;
        private Boolean _cipherRotor4Reverse = false;
        private Boolean _cipherRotor5Reverse = false;

        private Boolean _controlRotor1Reverse = false;
        private Boolean _controlRotor2Reverse = false;
        private Boolean _controlRotor3Reverse = false;
        private Boolean _controlRotor4Reverse = false;
        private Boolean _controlRotor5Reverse = false;

        private Boolean _indexRotor1Reverse = false;
        private Boolean _indexRotor2Reverse = false;
        private Boolean _indexRotor3Reverse = false;
        private Boolean _indexRotor4Reverse = false;
        private Boolean _indexRotor5Reverse = false;

        private string _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private int _action = 0; // we will need crypt, decrypt, zerozize, Plaintext, O for off and Reset Position

        #endregion

        #region Initialisation / Contructor
        public SigabaSettings()
        {
            SetList(_typeStrings,"CSP888/889","CSP2800");
            SetList(_actionStrings, "SigabaSettings_SigabaSettings_Cipher", Resources.SigabaSettings_SigabaSettings_Decipher);
            SetList(_cipherControlRotorStrings, "TestRotor", "RotorA1", "RotorA2", "RotorA3", "RotorA4", "RotorA5", "RotorA6", "RotorA7", "RotorA8", "RotorA9","RotorA10");
            SetList(_indexRotorStrings, "TestRotor","RotorB1", "RotorB2", "RotorB3", "RotorB4", "RotorB5");
         
        }
        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        
         private void SetList(ObservableCollection<string> coll, params string[] keys)
        {
            coll.Clear();
            foreach (string key in keys)
                coll.Add(typeof(Sigaba).GetPluginStringResource(key));
        }

         [TaskPane("TypeCaption", "TypeTooltip",
             null, 0, false, ControlType.DynamicComboBox, new string[] { "TypeStrings" })]
         public int Type
         {
             get { return this._type; }
             set
             {
                 if (((int)value) != _type)
                 {
                     this._type = (int)value;
                     OnPropertyChanged("Type");
                 }
             }
         }

         [TaskPane("ActionCaption", "ActionTooltip",
            null, 0, false, ControlType.DynamicComboBox, new string[] { "ActionStrings" })]
         public int Action
         {
             get { return this._action; }
             set
             {
                 if (((int)value) != _action)
                 {
                     this._action = (int)value;
                     OnPropertyChanged("Action");
                 }
             }
         }

        [TaskPane("ControlKeyCaption", "ControlKeyTooltip",
            null, 2, false, ControlType.TextBox, ValidationType.RegEx, "^[A-Z]{5}$")]
        public string ControlKey
        {
            get { return this._controlKey; }
            set
            {
                if (value != _controlKey)
                {
                    this._controlKey = value;
                    OnPropertyChanged("ControlKey");
                }
            }
        }

        [TaskPane("CipherKeyCaption", "ControlKeyTooltip",
            null, 0, false, ControlType.TextBox, ValidationType.RegEx, "^[A-Z]{5}$")]
        public string CipherKey
        {
            get { return this._cipherKey; }
            set
            {
                if (value != _cipherKey)
                {
                    this._cipherKey = value;
                    OnPropertyChanged("CipherKey");
                }
            }
        }

        [TaskPane("IndexKeyCaption", "IndexKeyTooltip",
            null, 1, false, ControlType.TextBox, ValidationType.RegEx, "^[0-9]{5}$")]
        public string IndexKey
        {
            get { return this._indexKey; }
            set
            {
                if (value != _indexKey)
                {
                    this._indexKey = value;
                    OnPropertyChanged("IndexKey");
                }
            }
        }

        [ContextMenu("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip",
          3, ContextMenuControlType.ComboBox, null,
          new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip",
            "TextOptionsGroup", 3, false, ControlType.ComboBox,
            new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public int UnknownSymbolHandling
        {
            get { return _unknownSymbolHandling; }
            set
            {
                if ((int)value != _unknownSymbolHandling)
                {
                    _unknownSymbolHandling = (int)value;
                    OnPropertyChanged("UnknownSymbolHandling");
                }
            }
        }

        [ContextMenu("CaseHandlingCaption", "CaseHandlingTooltip",
            4, ContextMenuControlType.ComboBox, null,
            new string[] { "CaseHandlingList1", "CaseHandlingList2", "CaseHandlingList3" })]
        [TaskPane("CaseHandlingCaption", "CaseHandlingTooltip",
            "TextOptionsGroup", 4, false, ControlType.ComboBox,
            new string[] { "CaseHandlingList1", "CaseHandlingList2", "CaseHandlingList3" })]
        public int CaseHandling
        {
            get { return _caseHandling; }
            set
            {
                if ((int)value != _caseHandling)
                {
                    _caseHandling = (int)value;
                    OnPropertyChanged("CaseHandling");
                }
            }
        }


        [PropertySaveOrder(11)]
        public ObservableCollection<string> TypeStrings
        {
            get { return _typeStrings; }
            set
            {
                if (value != _typeStrings)
                {
                    _typeStrings = value;
                    OnPropertyChanged("TypeStrings");
                }
            }
        }

        [PropertySaveOrder(9)]
        public ObservableCollection<string> ActionStrings
        {
            get { return _actionStrings; }
            set
            {
                if (value != _actionStrings)
                {
                    _actionStrings = value;
                    OnPropertyChanged("ActionStrings");
                }
            }
        }

        [PropertySaveOrder(10)]
        [TaskPane("PresentationSpeedCaption", "PresentationSpeedTooltip", "PresentationGroup", 6, true, ControlType.Slider, 1, 40)]
        public int PresentationSpeed
        {
            get { return (int)_presentationSpeed; }
            set
            {
                if ((value) != _presentationSpeed)
                {
                    this._presentationSpeed = value;
                    OnPropertyChanged("PresentationSpeed");
                }
            }
        }

        #endregion

        #region Used rotor settings
        
        [TaskPane("CipherRotor1Caption", "CipherRotor1Tooltip",
            "CipherGroup", 5, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int CipherRotor1
        {
            get { return _cipherRotor1; }
            set
            {
                if (((int)value) != _cipherRotor1)
                {
                    CheckRotorChange(1, _cipherRotor1, value);
                    _cipherRotor1 = value;
                    OnPropertyChanged("CipherRotor1");
                }
            }
        }

        [TaskPane("CipherRotor1ReverseCaption", "CipherRotor2ReverseTooltip",
            "CipherGroup", 6, false, ControlType.CheckBox, "Reverse")]
        public Boolean CipherRotor1Reverse
        {
            get { return _cipherRotor1Reverse; }
            set
            {
                if (((Boolean)value) != _cipherRotor1Reverse)
                {
                    _cipherRotor1Reverse = value;
                    OnPropertyChanged("CipherRotor1Reverse");
                }
            }
        }

        [TaskPane("CipherRotor2Caption", "CipherRotor2Tooltip",
            "CipherGroup", 7, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int CipherRotor2
        {
            get { return _cipherRotor2; }
            set
            {
                if (((int)value) != _cipherRotor2)
                {
                    CheckRotorChange(2, _cipherRotor2, value);
                    _cipherRotor2 = (int)value;
                    OnPropertyChanged("CipherRotor2");
                }
            }
        }

        [TaskPane("CipherRotor1ReverseCaption", "CipherRotor2ReverseTooltip",
            "CipherGroup", 8, false, ControlType.CheckBox, "Reverse")]
        public Boolean CipherRotor2Reverse
        {
            get { return _cipherRotor2Reverse; }
            set
            {
                if (((Boolean)value) != _cipherRotor2Reverse)
                {
                    _cipherRotor2Reverse = value;
                    OnPropertyChanged("CipherRotor2Reverse");
                }
            }
        }

        [TaskPane("CipherRotor3Caption", "CipherRotor3Tooltip",
            "CipherGroup", 9, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int CipherRotor3
        {
            get { return _cipherRotor3; }
            set
            {
                if (((int)value) != _cipherRotor3)
                {
                    CheckRotorChange(3, _cipherRotor3, value);
                    _cipherRotor3 = (int)value;
                    OnPropertyChanged("CipherRotor3");
                }
            }
        }

        [TaskPane("CipherRotor1ReverseCaption", "CipherRotor2ReverseTooltip",
            "CipherGroup", 10, false, ControlType.CheckBox, "Reverse")]
        public Boolean CipherRotor3Reverse
        {
            get { return _cipherRotor3Reverse; }
            set
            {
                if (((Boolean)value) != _cipherRotor3Reverse)
                {
                    _cipherRotor3Reverse = value;
                    OnPropertyChanged("CipherRotor3Reverse");
                }
            }
        }

        [TaskPane("CipherRotor4Caption", "CipherRotor4Tooltip",
            "CipherGroup", 11, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int CipherRotor4
        {
            get { return _cipherRotor4; }
            set
            {
                if (((int)value) != _cipherRotor4)
                {
                    _cipherRotor4 = (int)value;
                    OnPropertyChanged("CipherRotor4");
                }
            }
        }

        [TaskPane("CipherRotor1ReverseCaption", "CipherRotor2ReverseTooltip",
            "CipherGroup", 12, false, ControlType.CheckBox, "Reverse")]
        public Boolean CipherRotor4Reverse
        {
            get { return _cipherRotor4Reverse; }
            set
            {
                if (((Boolean)value) != _cipherRotor4Reverse)
                {
                    _cipherRotor4Reverse = value;
                    OnPropertyChanged("CipherRotor4Reverse");
                }
            }
        }

        [TaskPane("CipherRotor5Caption", "CipherRotor5Tooltip",
            "CipherGroup", 13, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int CipherRotor5
        {
            get { return _cipherRotor5; }
            set
            {
                if (((int)value) != _cipherRotor5)
                {
                    _cipherRotor5 = (int)value;
                    OnPropertyChanged("CipherRotor5");
                }
            }
        }

        [TaskPane("CipherRotor1ReverseCaption", "CipherRotor2ReverseTooltip",
            "CipherGroup", 14, false, ControlType.CheckBox, "Reverse")]
        public Boolean CipherRotor5Reverse
        {
            get { return _cipherRotor5Reverse; }
            set
            {
                if (((Boolean)value) != _cipherRotor5Reverse)
                {
                    _cipherRotor5Reverse = value;
                    OnPropertyChanged("CipherRotor5Reverse");
                }
            }
        }

        [TaskPane("ControlRotor1Caption", "ControlRotor1Tooltip",
          "ControlGroup", 15, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int ControlRotor1
        {
            get { return _controlRotor1; }
            set
            {
                if (((int)value) != _controlRotor1)
                {
                    CheckRotorChange(1, _controlRotor1, value);
                    _controlRotor1 = value;
                    OnPropertyChanged("ControlRotor1");
                }
            }
        }

        [TaskPane("CipherRotor1ReverseCaption", "CipherRotor2ReverseTooltip",
        "ControlGroup", 16, false, ControlType.CheckBox, "Reverse")]
        public Boolean ControlRotor1Reverse
        {
            get { return _controlRotor1Reverse; }
            set
            {
                if (((Boolean)value) != _controlRotor1Reverse)
                {
                    _controlRotor1Reverse = value;
                    OnPropertyChanged("ControlRotor1Reverse");
                }
            }
        }


        [TaskPane("ControlRotor2Caption", "ControlRotor2Tooltip",
            "ControlGroup", 17, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int ControlRotor2
        {
            get { return _controlRotor2; }
            set
            {
                if (((int)value) != _controlRotor2)
                {
                    CheckRotorChange(2, _controlRotor2, value);
                    _controlRotor2 = (int)value;
                    OnPropertyChanged("ControlRotor2");
                }
            }
        }

        [TaskPane("CipherRotor1ReverseCaption", "CipherRotor2ReverseTooltip",
        "ControlGroup", 18, false, ControlType.CheckBox, "Reverse")]
        public Boolean ControlRotor2Reverse
        {
            get { return _controlRotor2Reverse; }
            set
            {
                if (((Boolean)value) != _controlRotor2Reverse)
                {
                    _controlRotor2Reverse = value;
                    OnPropertyChanged("ControlRotor2Reverse");
                }
            }
        }


        [TaskPane("ControlRotor3Caption", "ControlRotor3Tooltip",
            "ControlGroup", 19, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int ControlRotor3
        {
            get { return _controlRotor3; }
            set
            {
                if (((int)value) != _controlRotor3)
                {
                    CheckRotorChange(3, _controlRotor3, value);
                    _controlRotor3 = (int)value;
                    OnPropertyChanged("ControlRotor3");
                }
            }
        }

        [TaskPane("CipherRotor1ReverseCaption", "CipherRotor2ReverseTooltip",
        "ControlGroup", 20, false, ControlType.CheckBox, "Reverse")]
        public Boolean ControlRotor3Reverse
        {
            get { return _controlRotor3Reverse; }
            set
            {
                if (((Boolean)value) != _controlRotor3Reverse)
                {
                    _controlRotor3Reverse = value;
                    OnPropertyChanged("ControlRotor3Reverse");
                }
            }
        }

        [TaskPane("ControlRotor4Caption", "ControlRotor4Tooltip",
            "ControlGroup", 21, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int ControlRotor4
        {
            get { return _controlRotor4; }
            set
            {
                if (((int)value) != _controlRotor4)
                {
                    _controlRotor4 = (int)value;
                    OnPropertyChanged("ControlRotor4");
                }
            }
        }

        [TaskPane("CipherRotor1ReverseCaption", "CipherRotor2ReverseTooltip",
        "ControlGroup", 22, false, ControlType.CheckBox, "Reverse")]
        public Boolean ControlRotor4Reverse
        {
            get { return _controlRotor4Reverse; }
            set
            {
                if (((Boolean)value) != _controlRotor4Reverse)
                {
                    _controlRotor4Reverse = value;
                    OnPropertyChanged("ControlRotor4Reverse");
                }
            }
        }

        [TaskPane("ControlRotor5Caption", "ControlRotor5Tooltip",
            "ControlGroup", 23, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int ControlRotor5
        {
            get { return _controlRotor5; }
            set
            {
                if (((int)value) != _controlRotor5)
                {
                    _controlRotor5 = (int)value;
                    OnPropertyChanged("ControlRotor5");
                }
            }
        }

        [TaskPane("CipherRotor1ReverseCaption", "CipherRotor2ReverseTooltip",
        "ControlGroup", 24, false, ControlType.CheckBox, "Reverse")]
        public Boolean ControlRotor5Reverse
        {
            get { return _controlRotor5Reverse; }
            set
            {
                if (((Boolean)value) != _controlRotor5Reverse)
                {
                    _controlRotor5Reverse = value;
                    OnPropertyChanged("ControlRotor5Reverse");
                }
            }
        }
        
        [TaskPane("IndexRotor1Caption", "IndexRotor1Tooltip",
          "IndexGroup", 25, false, ControlType.DynamicComboBox, new string[] { "IndexRotorStrings" })]
        public int IndexRotor1
        {
            get { return _indexRotor1; }
            set
            {
                if (((int)value) != _indexRotor1)
                {
                    CheckRotorChange(1, _indexRotor1, value);
                    _indexRotor1 = value;
                    OnPropertyChanged("IndexRotor1");
                }
            }
        }

        [TaskPane("CipherRotor1ReverseCaption", "CipherRotor2ReverseTooltip",
        "IndexGroup", 26, false, ControlType.CheckBox, "Reverse")]
        public Boolean IndexRotor1Reverse
        {
            get { return _indexRotor1Reverse; }
            set
            {
                if (((Boolean)value) != _indexRotor1Reverse)
                {
                    _indexRotor1Reverse = value;
                    OnPropertyChanged("ControlRotor1Reverse");
                }
            }
        }

        [TaskPane("IndexRotor2Caption", "Rotor2Tooltip",
          "IndexGroup", 27, false, ControlType.DynamicComboBox, new string[] { "IndexRotorStrings" })]
        public int IndexRotor2
        {
            get { return _indexRotor2; }
            set
            {
                if (((int)value) != _indexRotor2)
                {
                    CheckRotorChange(1, _indexRotor2, value);
                    _indexRotor2 = value;
                    OnPropertyChanged("IndexRotor2");
                }
            }
        }

        [TaskPane("CipherRotor1ReverseCaption", "CipherRotor2ReverseTooltip",
        "IndexGroup", 28, false, ControlType.CheckBox, "Reverse")]
        public Boolean IndexRotor2Reverse
        {
            get { return _indexRotor2Reverse; }
            set
            {
                if (((Boolean)value) != _indexRotor2Reverse)
                {
                    _indexRotor2Reverse = value;
                    OnPropertyChanged("ControlRotor2Reverse");
                }
            }
        }

        [TaskPane("IndexRotor3Caption", "IndexRotor3Tooltip",
          "IndexGroup", 29, false, ControlType.DynamicComboBox, new string[] { "IndexRotorStrings" })]
        public int IndexRotor3
        {
            get { return _indexRotor3; }
            set
            {
                if (((int)value) != _indexRotor3)
                {
                    CheckRotorChange(1, _indexRotor3, value);
                    _indexRotor3 = value;
                    OnPropertyChanged("IndexRotor1");
                }
            }
        }

        [TaskPane("CipherRotor1ReverseCaption", "CipherRotor2ReverseTooltip",
        "IndexGroup", 30, false, ControlType.CheckBox, "Reverse")]
        public Boolean IndexRotor3Reverse
        {
            get { return _indexRotor3Reverse; }
            set
            {
                if (((Boolean)value) != _indexRotor3Reverse)
                {
                    _indexRotor3Reverse = value;
                    OnPropertyChanged("ControlRotor1Reverse");
                }
            }
        }

        [TaskPane("IndexRotor4Caption", "IndexRotor4Tooltip",
          "IndexGroup", 31, false, ControlType.DynamicComboBox, new string[] { "IndexRotorStrings" })]
        public int IndexRotor4
        {
            get { return _indexRotor4; }
            set
            {
                if (((int)value) != _indexRotor4)
                {
                    CheckRotorChange(1, _indexRotor4, value);
                    _indexRotor4 = value;
                    OnPropertyChanged("IndexRotor4");
                }
            }
        }

        [TaskPane("CipherRotor1ReverseCaption", "CipherRotor2ReverseTooltip",
        "IndexGroup", 32, false, ControlType.CheckBox, "Reverse")]
        public Boolean IndexRotor4Reverse
        {
            get { return _indexRotor4Reverse; }
            set
            {
                if (((Boolean)value) != _indexRotor4Reverse)
                {
                    _indexRotor4Reverse = value;
                    OnPropertyChanged("ControlRotor4Reverse");
                }
            }
        }

        [TaskPane("IndexRotor5Caption", "IndexRotor5Tooltip",
          "IndexGroup", 33, false, ControlType.DynamicComboBox, new string[] { "IndexRotorStrings" })]
        public int IndexRotor5
        {
            get { return _indexRotor5; }
            set
            {
                if (((int)value) != _indexRotor5)
                {
                    CheckRotorChange(1, _indexRotor5, value);
                    _indexRotor5 = value;
                    OnPropertyChanged("IndexRotor5");
                }
            }
        }

        [TaskPane("CipherRotor1ReverseCaption", "CipherRotor2ReverseTooltip",
        "IndexGroup", 34, false, ControlType.CheckBox, "Reverse")]
        public Boolean IndexRotor5Reverse
        {
            get { return _indexRotor5Reverse; }
            set
            {
                if (((Boolean)value) != _indexRotor5Reverse)
                {
                    _indexRotor5Reverse = value;
                    OnPropertyChanged("ControlRotor5Reverse");
                }
            }
        }

        private void CheckRotorChange(int rotor, int was, int becomes)
        {/*
            switch (rotor)
            {
                case 1:
                    if (rotor2 == becomes) { rotor2 = was; OnPropertyChanged("Rotor2"); }
                    if (rotor3 == becomes) { rotor3 = was; OnPropertyChanged("Rotor3"); }
                    break;
                case 2:
                    if (rotor1 == becomes) { rotor1 = was; OnPropertyChanged("Rotor1"); }
                    if (rotor3 == becomes) { rotor3 = was; OnPropertyChanged("Rotor3"); }
                    break;
                case 3:
                    if (rotor1 == becomes) { rotor1 = was; OnPropertyChanged("Rotor1"); }
                    if (rotor2 == becomes) { rotor2 = was; OnPropertyChanged("Rotor2"); }
                    break;
            }
          */
        }
        

        /// <summary>
        /// This collection contains the values for the Rotor 1-3 comboboxes.
        /// </summary>
        public ObservableCollection<string> CipherControlRotorStrings
        {
            get { return _cipherControlRotorStrings; }
            set
            {
                if (value != _cipherControlRotorStrings)
                {
                    _cipherControlRotorStrings = value;
                    OnPropertyChanged("RotorAStrings");
                }
            }
        }

        /// <summary>
        /// This collection contains the values for the Rotor 4 combobox.
        /// </summary>
        public ObservableCollection<string> IndexRotorStrings
        {
            get { return _indexRotorStrings; }
            set
            {
                if (value != _indexRotorStrings)
                {
                    _indexRotorStrings = value;
                    OnPropertyChanged("RotorBStrings");
                }
            }
        }

       

        #endregion

        #region Public properties

        public string Alphabet
        {
            get { return _alphabet; }
            set { _alphabet = value; }
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
