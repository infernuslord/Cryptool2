/*
   Copyright 2008-2009, Dr. Arno Wacker, University of Duisburg-Essen

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
using System.Linq;
using System.Text;

//additionally needed libs
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;

// Cryptool 2.0 specific includes
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;


namespace Cryptool.Enigma
{
    public class EnigmaSettings : ISettings
    {
        #region Private variables

        private bool hasChanges = false;
        private int action = 0; //0=encrypt, 1=decrypt
        private ObservableCollection<string> actionStrings = new ObservableCollection<string>();
        private ObservableCollection<string> rotorAStrings = new ObservableCollection<string>();
        private ObservableCollection<string> rotorBStrings = new ObservableCollection<string>();
        private ObservableCollection<string> reflectorStrings = new ObservableCollection<string>();
        private int model = 3;
        private int unknownSymbolHandling = 0; // 0=ignore, leave unmodified
        private int caseHandling = 0; // 0=preserve, 1, convert all to upper, 2= convert all to lower
        private string key = "AAA";
        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private bool analyzeRotors = true;
        private bool analyzeKey = true;
        private bool analyzeRings = true;
        private bool analyzePlugs = true;
        private bool analysisUseRotorI = true;
        private bool analysisUseRotorII = true;
        private bool analysisUseRotorIII = true;
        private bool analysisUseRotorIV = true;
        private bool analysisUseRotorV = true;
        private bool analysisUseRotorVI = true;
        private bool analysisUseRotorVII = true;
        private bool analysisUseRotorVIII = true;

        private int keySearchMethod = 0;
        private int maxSearchedPlugs = 10;
        private int plugSearchMethod = 2;

        private int rotor1 = 0;
        private int rotor2 = 1;
        private int rotor3 = 2;
        private int rotor4 = 0;

        private int ring1 = 1; // 01 = A, 02 = B ...
        private int ring2 = 1; 
        private int ring3 = 1;
        private int ring4 = 1;

        private int reflector = 1;

        private bool involutoricPlugBoard = true;
        private StringBuilder plugBoard = new StringBuilder("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

        #endregion

        #region Private methods

        private void checkRotorChange(int rotor, int was, int becomes)
        {
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
        }

        private void setPlugBoard(int letterPos, int newIndex)
        {
            if (newIndex != alphabet.IndexOf(plugBoard[letterPos]))
            {
                hasChanges = true;
                char newChar = alphabet[newIndex];
                //int newCharIndex = plugBoard.ToString().IndexOf(newChar);
                char currentChar = plugBoard[letterPos];
                int currentIndex = alphabet.IndexOf(currentChar);

                if (this.involutoricPlugBoard)
                {
                    this.plugBoard[newIndex] = currentChar;
                    OnPropertyChanged("PlugBoard" + alphabet[newIndex]);

                    if (newChar == this.alphabet[letterPos])
                    {
                        // we removed a plug
                        this.plugBoard[currentIndex] = this.alphabet[currentIndex];
                        OnPropertyChanged("PlugBoard" + alphabet[currentIndex]);
                    }

                }

                this.plugBoard[letterPos] = newChar;
                OnPropertyChanged("PlugBoard" + alphabet[letterPos]);
                OnPropertyChanged("PlugBoardDisplay");
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

        #region Public Events and Methods

        /// <summary>
        /// This event is needed in order to render settings elements visible/invisible
        /// </summary>
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        /// <summary>
        /// Return the expected length of n-grams statistics for the given search method.
        /// </summary>
        /// <returns></returns>
        public int GetGramLength(int searchMethod)
        {
            switch (searchMethod)
            {
                case 0:
                    return 1;
                case 1:
                    return 2;
                case 2:
                    return 3;
                case 3:
                    return 1;
                case 4:
                    return 2;
                case 5:
                    return 1;
                default:
                    throw new NotSupportedException("Search method with index " + searchMethod + " is unknown");
            }
        }

        public int AlphabetIndexOf(char c)
        {
            return c - 'A';
        }

        #endregion

        #region Initialisation / Contructor

        public EnigmaSettings()
        {
            actionStrings.Add("Encrypt/Decrypt"); actionStrings.Add("Analyze");
            reflectorStrings.Add("UKW A"); reflectorStrings.Add("UKW B (2. November 1937)"); reflectorStrings.Add("UKW C (since 1940/41)");
            rotorAStrings.Add("I (since 1930)"); rotorAStrings.Add("II (since 1930)"); rotorAStrings.Add("III (since 1930)");
            rotorAStrings.Add("IV (since 1938, M3 \"Heer\")"); rotorAStrings.Add("V (since 1938, M3 \"Heer\")"); rotorAStrings.Add("VI (since 1939, M3/M4)");
            rotorAStrings.Add("VII (since 1939, M3/M4)"); rotorAStrings.Add("VIII (since 1939, M3/M4)");
            rotorBStrings.Add("Not available for this model.");
            hideSettingsElement("Rotor4"); hideSettingsElement("Ring4"); //FIXME: They still show initially... ? Probably, the event is not registered yet..
        }

        #endregion

        #region Public properties

        public string Alphabet
        { 
            get {return alphabet; }
            set {alphabet = value; } 
        }

        #endregion

        #region Taskpane settings

        [ContextMenu("Enigma model", "Please select which Enigma model you want to use. This settings influences available rotors and their inner cabling.", 
            0, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, 
            new string[] { "Commercial Enigma A/B - since 1924", "Commercial Enigma D", "Reichsbahn (Rocket) - since 1941", "Enigma I / M3", "M4 (Shark)", "K-Model", "G (Defense model)" })]
        [TaskPane("Enigma model", "Please select which Enigma model you want to use. This settings influences the available rotors and their inner cabling.", 
            null, 0, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new string[] { "Enigma A/B - since 1924", "Enigma D", "Reichsbahn (Rocket) - since 1941", "Enigma I / M3", "M4 (Shark)", "K-Model", "G (Defense model)" })]
        [PropertySaveOrder(1)]
        public int Model
        {
            get { return this.model; }
            set
            {
                if (value != model) hasChanges = true;
                this.model = value;
                OnPropertyChanged("Model");
                switch (this.model)
                {
                    case 0: // Enigma A/B
                        actionStrings.Clear(); actionStrings.Add("Encrypt"); actionStrings.Add("Decrypt"); action = 0; OnPropertyChanged("Action");
                        if (key.Length > 3) key = key.Remove(0,1); OnPropertyChanged("Key");
                        rotorAStrings.Clear();  rotorAStrings.Add("I (C) -- since 1924"); rotorAStrings.Add("II (C) -- since 1924"); rotorAStrings.Add("III (C) -- since 1924");
                        rotor1 = 0; rotor2 = 1; rotor3 = 2; OnPropertyChanged("Rotor1"); OnPropertyChanged("Rotor2"); OnPropertyChanged("Rotor3");
                        rotorBStrings.Clear(); rotorBStrings.Add("Not available for this model."); rotor4 = 0;  OnPropertyChanged("Rotor4");
                        reflectorStrings.Clear(); reflectorStrings.Add("Not available for this model."); reflector = 0; OnPropertyChanged("Reflector");
                        hideSettingsElement("Rotor4"); hideSettingsElement("Ring4"); hideSettingsElement("Reflector");
                        break;
                    case 1: // Enigma D
                        actionStrings.Clear(); actionStrings.Add("Encrypt / Decrypt"); action = 0; OnPropertyChanged("Action");
                        if (key.Length > 3) key = key.Remove(0, 1); OnPropertyChanged("Key");
                        rotorAStrings.Clear(); rotorAStrings.Add("I (D)"); rotorAStrings.Add("II (D)"); rotorAStrings.Add("III (D)");
                        rotor1 = 0; rotor2 = 1; rotor3 = 2; OnPropertyChanged("Rotor1"); OnPropertyChanged("Rotor2"); OnPropertyChanged("Rotor3");
                        rotorBStrings.Clear(); rotorBStrings.Add("Not available for this model."); rotor4 = 0; OnPropertyChanged("Rotor4");
                        reflectorStrings.Clear(); reflectorStrings.Add("UKW (D)"); reflector = 0; OnPropertyChanged("Reflector");
                        hideSettingsElement("Rotor4"); hideSettingsElement("Ring4");
                        showSettingsElement("Reflector");
                        break;
                    case 2: // Reichsbahn (Rocket)
                        actionStrings.Clear(); actionStrings.Add("Encrypt / Decrypt"); action = 0; OnPropertyChanged("Action");
                        if (key.Length > 3) key = key.Remove(0, 1); OnPropertyChanged("Key");
                        rotorAStrings.Clear(); rotorAStrings.Add("I -- since 7th Feb. 1941"); rotorAStrings.Add("II -- since 7th Feb. 1941"); rotorAStrings.Add("III -- since 7th Feb. 1941");
                        rotor1 = 0; rotor2 = 1; rotor3 = 2; OnPropertyChanged("Rotor1"); OnPropertyChanged("Rotor2"); OnPropertyChanged("Rotor3");
                        rotorBStrings.Clear(); rotorBStrings.Add("Not available for this model."); rotor4 = 0; OnPropertyChanged("Rotor4");
                        reflectorStrings.Clear(); reflectorStrings.Add("UKW -- since 7th Feb. 1941"); reflector = 0; OnPropertyChanged("Reflector");
                        hideSettingsElement("Rotor4"); hideSettingsElement("Ring4");
                        showSettingsElement("Reflector");
                        break;
                    case 3: // Enigma I / M3
                        actionStrings.Clear(); actionStrings.Add("Encrypt / Decrypt"); actionStrings.Add("Analyze"); action = 0; OnPropertyChanged("Action");
                        if (key.Length > 3) key = key.Remove(0, 1); OnPropertyChanged("Key");
                        rotorAStrings.Clear(); rotorAStrings.Add("I (since 1930)"); rotorAStrings.Add("II (since 1930)"); rotorAStrings.Add("III (since 1930)");
                        rotorAStrings.Add("IV (since 1938, M3 \"Heer\")"); rotorAStrings.Add("V (since 1938, M3 \"Heer\")"); rotorAStrings.Add("VI (since 1939, M3/M4)");
                        rotorAStrings.Add("VII (since 1939, M3/M4)"); rotorAStrings.Add("VIII (since 1939, M3/M4)");
                        rotor1 = 0; rotor2 = 1; rotor3 = 2; OnPropertyChanged("Rotor1"); OnPropertyChanged("Rotor2"); OnPropertyChanged("Rotor3");
                        rotorBStrings.Clear(); rotorBStrings.Add("Not available for this model."); rotor4 = 0; OnPropertyChanged("Rotor4");
                        reflectorStrings.Clear(); reflectorStrings.Add("UKW A"); reflectorStrings.Add("UKW B (2. November 1937)");
                        reflectorStrings.Add("UKW C (since 1940/41)"); reflector = 1; OnPropertyChanged("Reflector");
                        hideSettingsElement("Rotor4"); hideSettingsElement("Ring4"); 
                        showSettingsElement("Reflector");
                        break;
                    case 4: // Enigma M4 "Shark"
                        actionStrings.Clear(); actionStrings.Add("Encrypt / Decrypt"); action = 0; OnPropertyChanged("Action");
                        if (key.Length < 4) key = "A" + key; OnPropertyChanged("Key");
                        rotorAStrings.Clear(); rotorAStrings.Add("I (since 1930)"); rotorAStrings.Add("II (since 1930)"); rotorAStrings.Add("III (since 1930)");
                        rotorAStrings.Add("IV (since 1938, M3 \"Heer\")"); rotorAStrings.Add("V (since 1938, M3 \"Heer\")"); rotorAStrings.Add("VI (since 1939, M3/M4)");
                        rotorAStrings.Add("VII (since 1939, M3/M4)"); rotorAStrings.Add("VIII (since 1939, M3/M4)");
                        rotor1 = 0; rotor2 = 1; rotor3 = 2; OnPropertyChanged("Rotor1"); OnPropertyChanged("Rotor2"); OnPropertyChanged("Rotor3");
                        rotorBStrings.Clear(); rotorBStrings.Add("Beta -- since 1st Feb. 1942"); rotorBStrings.Add("Gamma -- since 1st July 1943"); rotor4 = 0; OnPropertyChanged("Rotor4");
                        reflectorStrings.Clear(); reflectorStrings.Add("UKW B \"thin\" -- since 1st Feb. 1942"); reflectorStrings.Add("UKW C \"thin\" -- since 1st July 1943");
                        reflector = 0; OnPropertyChanged("Reflector");
                        showSettingsElement("Rotor4"); showSettingsElement("Ring4"); showSettingsElement("Reflector");
                        break;
                    case 5: // Enigma K-Model
                        actionStrings.Clear(); actionStrings.Add("Encrypt / Decrypt"); action = 0; OnPropertyChanged("Action");
                        if (key.Length > 3) key = key.Remove(0, 1); OnPropertyChanged("Key");
                        rotorAStrings.Clear(); rotorAStrings.Add("I (K) -- since Feb. 1939"); rotorAStrings.Add("II (K) -- since Feb. 1939"); rotorAStrings.Add("III (K) -- since Feb. 1939");
                        rotor1 = 0; rotor2 = 1; rotor3 = 2; OnPropertyChanged("Rotor1"); OnPropertyChanged("Rotor2"); OnPropertyChanged("Rotor3");
                        rotorBStrings.Clear(); rotorBStrings.Add("Not available for this model."); rotor4 = 0; OnPropertyChanged("Rotor4");
                        reflectorStrings.Clear(); reflectorStrings.Add("UKW (K) -- since Feb. 1939"); reflector = 0; OnPropertyChanged("Reflector");
                        hideSettingsElement("Rotor4"); hideSettingsElement("Ring4");
                        showSettingsElement("Reflector");
                        break;
                    case 6: // Enigam G / Abwehr
                        actionStrings.Clear(); actionStrings.Add("Encrypt / Decrypt"); action = 0; OnPropertyChanged("Action");
                        if (key.Length > 3) key = key.Remove(0, 1); OnPropertyChanged("Key");
                        rotorAStrings.Clear(); rotorAStrings.Add("I (G)"); rotorAStrings.Add("II (G)"); rotorAStrings.Add("III (G)");
                        rotor1 = 0; rotor2 = 1; rotor3 = 2; OnPropertyChanged("Rotor1"); OnPropertyChanged("Rotor2"); OnPropertyChanged("Rotor3");
                        rotorBStrings.Clear(); rotorBStrings.Add("Not available for this model."); rotor4 = 0; OnPropertyChanged("Rotor4");
                        reflectorStrings.Clear(); reflectorStrings.Add("UKW (G)"); reflector = 0; OnPropertyChanged("Reflector");
                        hideSettingsElement("Rotor4"); hideSettingsElement("Ring4");
                        showSettingsElement("Reflector");
                        break;
                }
            }
        }


        [TaskPane("Key (Initial rotor setting)", "Please provide the initial rotor setting for each rotor, e.g. ABCD. Note that the slowest rotor is on the left and the fastest rotor is on the right. Only the Enigma M4 \"Shark\" uses four rotors.",
            null, 1, false, DisplayLevel.Beginner, ControlType.TextBox, ValidationType.RegEx, "^([A-Z]|[a-z]){3,4}$")]
        public string Key
        {
            get { return this.key; }
            set
            {
                if (value != key) hasChanges = true;
                this.key = value;
                OnPropertyChanged("Key");
            }
        }


        [TaskPane("Operation mode", "Select the mode of operation for this Enigma simulator. Note that all Enigmas since Enigma D are working with a reflector and therefore there is not difference between encrypting an decrypting.",
            null, 2, false, DisplayLevel.Beginner, ControlType.DynamicComboBox, new string[] { "ActionStrings" })]
        [PropertySaveOrder(9)]
        public int Action
        {
            get { return this.action; }
            set
            {
                if (((int)value) != action) hasChanges = true;
                this.action = (int)value;
                OnPropertyChanged("Action");
            }
        }

        /// <summary>
        /// This collection contains the values for the Action combobox.
        /// </summary>
        [PropertySaveOrder(9)]
        public ObservableCollection<string> ActionStrings
        {
            get { return actionStrings; }
            set
            {
                if (value != actionStrings)
                {
                    actionStrings = value;
                }
                OnPropertyChanged("ActionStrings");
            }
        }

        #region Text options

        [ContextMenu("Unknown symbol handling", "What should be done with encountered characters at the input which are not in the alphabet?", 
            3, DisplayLevel.Expert, ContextMenuControlType.ComboBox, null, 
            new string[] { "Ignore (leave unmodified)", "Remove", "Replace with \'X\'" })]
        [TaskPane("Unknown symbol handling", "What should be done with encountered characters at the input which are not in the alphabet?", 
            "Text options", 3, false, DisplayLevel.Expert, ControlType.ComboBox, 
            new string[] { "Ignore (leave unmodified)", "Remove", "Replace with \'X\'" })]
        public int UnknownSymbolHandling
        {
            get { return this.unknownSymbolHandling; }
            set
            {
                if ((int)value != unknownSymbolHandling) HasChanges = true;
                this.unknownSymbolHandling = (int)value;
                OnPropertyChanged("UnknownSymbolHandling");
            }
        }

        [ContextMenu("Case handling", "What should be done with upper and lower case letters?", 
            4, DisplayLevel.Expert, ContextMenuControlType.ComboBox, null, 
            new string[] { "Preserve case", "Convert to upper", "Convert to lower" })]
        [TaskPane("Case handling", "What should be done with upper and lower case letters?", 
            "Text options", 4, false, DisplayLevel.Expert, ControlType.ComboBox, 
            new string[] { "Preserve case", "Convert to upper", "Convert to lower" })]
        public int CaseHandling
        {
            get { return this.caseHandling; }
            set
            {
                if ((int)value != caseHandling) HasChanges = true;
                this.caseHandling = (int)value;
                OnPropertyChanged("CaseHandling");
            }
        }

        #endregion

        #region Analysis options

        [TaskPane("Analyze key (initial rotor pos.)", "If checked, the analysis tries to detect the correct key. If unchecked, the value from \"Key\" is used.",
            "Analysis options", 6, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool AnalyzeKey
        {
            get { return analyzeKey; }
            set
            {
                if (value != analyzeKey)
                {
                    analyzeKey = value;
                    hasChanges = true;
                    OnPropertyChanged("AnalyzeKey");
                }
            }
        }

        [TaskPane("Analyze used rotors", "If checked, the analysis tries to detect the correct rotors and their positions. If unchecked, the values from the rotor settings are used.",
            "Analysis options", 7, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool AnalyzeRotors
        {
            get { return analyzeRotors; }
            set
            {
                if (value != analyzeRotors)
                {
                    analyzeRotors = value;
                    hasChanges = true;
                    OnPropertyChanged("AnalyzeRotors");
                }
            }
        }

        [SettingsFormat(1,"Normal","Normal")]
        [TaskPane("Include rotor I", "Check if rotor I should be included when analyzing rotors.",
            "Analysis options", 7, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorI
        {
            get { return analysisUseRotorI; }
            set
            {
                if (value != analysisUseRotorI)
                {
                    analysisUseRotorI = value;
                    hasChanges = true;
                    OnPropertyChanged("AnalysisUseRotorI");
                }
            }
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("Include rotor II", "Check if rotor II should be included when analyzing rotors.",
            "Analysis options", 7, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorII
        {
            get { return analysisUseRotorII; }
            set
            {
                if (value != analysisUseRotorII)
                {
                    analysisUseRotorII = value;
                    hasChanges = true;
                    OnPropertyChanged("AnalysisUseRotorII");
                }
            }
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("Include rotor III", "Check if rotor III should be included when analyzing rotors.",
            "Analysis options", 7, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorIII
        {
            get { return analysisUseRotorIII; }
            set
            {
                if (value != analysisUseRotorIII)
                {
                    analysisUseRotorIII = value;
                    hasChanges = true;
                    OnPropertyChanged("AnalysisUseRotorIII");
                }
            }
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("Include rotor IV", "Check if rotor IV should be included when analyzing rotors.",
            "Analysis options", 7, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorIV
        {
            get { return analysisUseRotorIV; }
            set
            {
                if (value != analysisUseRotorIV)
                {
                    analysisUseRotorIV = value;
                    hasChanges = true;
                    OnPropertyChanged("AnalysisUseRotorIV");
                }
            }
        }


        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("Include rotor V", "Check if rotor V should be included when analyzing rotors.",
            "Analysis options", 7, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorV
        {
            get { return analysisUseRotorV; }
            set
            {
                if (value != analysisUseRotorV)
                {
                    analysisUseRotorV = value;
                    hasChanges = true;
                    OnPropertyChanged("AnalysisUseRotorV");
                }
            }
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("Include rotor VI", "Check if rotor VI should be included when analyzing rotors.",
            "Analysis options", 7, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorVI
        {
            get { return analysisUseRotorVI; }
            set
            {
                if (value != analysisUseRotorVI)
                {
                    analysisUseRotorVI = value;
                    hasChanges = true;
                    OnPropertyChanged("AnalysisUseRotorVI");
                }
            }
        }


        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("Include rotor VII", "Check if rotor VII should be included when analyzing rotors.",
            "Analysis options", 7, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorVII
        {
            get { return analysisUseRotorVII; }
            set
            {
                if (value != analysisUseRotorVII)
                {
                    analysisUseRotorVII = value;
                    hasChanges = true;
                    OnPropertyChanged("AnalysisUseRotorVII");
                }
            }
        }


        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("Include rotor VIII", "Check if rotor VIII should be included when analyzing rotors.",
            "Analysis options", 7, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorVIII
        {
            get { return analysisUseRotorVIII; }
            set
            {
                if (value != analysisUseRotorVIII)
                {
                    analysisUseRotorVIII = value;
                    hasChanges = true;
                    OnPropertyChanged("AnalysisUseRotorVIII");
                }
            }
        }


        [TaskPane("Analyze rings settings", "If checked, the analysis tries to detect the correct rings settings. If unchecked, the values from the ring settings are used.",
            "Analysis options", 8, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool AnalyzeRings
        {
            get { return analyzeRings; }
            set
            {
                if (value != analyzeRings)
                {
                    analyzeRings = value;
                    hasChanges = true;
                    OnPropertyChanged("AnalyzeRings");
                }
            }
        }

        [TaskPane("Rotor/Ring/Key search method", "Which method should be used to assess the best rotor configuration?", "Analysis options", 8, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Index of coincidence", "log2-bigram", "log2-trigram", "Sinkov unigram", "Sinkov bigram", "Unigram entropy" })]
        public int KeySearchMethod
        {
            get { return this.keySearchMethod; }
            set
            {
                if (value != keySearchMethod)
                {
                    hasChanges = true;
                    keySearchMethod = value;
                    OnPropertyChanged("KeySearchMethod");
                }
            }
        }

        [TaskPane("Analyze plugs", "If checked, the analysis tries to detect the correct plug settings. If unchecked, the values from the plugboard are used.",
            "Analysis options", 9, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool AnalyzePlugs
        {
            get { return analyzePlugs; }
            set
            {
                if (value != analyzePlugs)
                {
                    analyzePlugs = value;
                    hasChanges = true;
                    OnPropertyChanged("AnalyzePlugs");
                }
            }
        }
       
        [TaskPane("Max. plugs searched", "Select how many plugs should be searched at most. Note that the search algorithm might return less plugs - this number is just an upper limit", 
            "Analysis options", 9, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int MaxSearchedPlugs
        {
            get { return this.maxSearchedPlugs; }
            set
            {
                if (value != maxSearchedPlugs)
                {
                    hasChanges = true;
                    maxSearchedPlugs = value;
                    OnPropertyChanged("MaxSearchedPlugs");
                }
            }
        }

        [TaskPane("Plug search method", "Which method should be used to assess the best plugboard configuration?", "Analysis options", 9, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Index of coincidence", "log2-bigram", "log2-trigram", "Sinkov unigram", "Sinkov bigram", "Unigram entropy" })]
        public int PlugSearchMethod
        {
            get { return this.plugSearchMethod; }
            set
            {
                if (value != plugSearchMethod)
                {
                    hasChanges = true;
                    plugSearchMethod = value;
                    OnPropertyChanged("PlugSearchMethod");
                }
            }
        }

        #endregion

        #region Used rotor settings

        [TaskPane("Rotor 1 (fastest/right)", "Select the used rotor on the right (fastest) position.",
            "Used rotors", 10, false, DisplayLevel.Beginner, ControlType.DynamicComboBox, new string[] { "RotorAStrings" })]
        public int Rotor1
        {
            get { return this.rotor1; }
            set
            {
                if (((int)value) != rotor1) hasChanges = true;
                checkRotorChange(1, this.rotor1, value);
                this.rotor1 = value;
                OnPropertyChanged("Rotor1");
            }
        }

        [TaskPane("Rotor 2", "Select the used rotor on the second position.",
            "Used rotors", 11, false, DisplayLevel.Beginner, ControlType.DynamicComboBox, new string[] { "RotorAStrings" })]
        public int Rotor2
        {
            get { return this.rotor2; }
            set
            {
                if (((int)value) != rotor2) hasChanges = true;
                checkRotorChange(2, this.rotor2, value);
                this.rotor2 = (int)value;
                OnPropertyChanged("Rotor2");
            }
        }

        [TaskPane("Rotor 3", "Select the used rotor on the third position.",
            "Used rotors", 12, false, DisplayLevel.Beginner, ControlType.DynamicComboBox, new string[] { "RotorAStrings" })]
        public int Rotor3
        {
            get { return this.rotor3; }
            set
            {
                if (((int)value) != rotor3) hasChanges = true;
                checkRotorChange(3, this.rotor3, value);
                this.rotor3 = (int)value;
                OnPropertyChanged("Rotor3");
            }
        }

        [TaskPane("Rotor 4 (slowest/left)", "Select the used rotor on the forth position. If you want to use only three rotors, select none here.",
            "Used rotors", 13, false, DisplayLevel.Beginner, ControlType.DynamicComboBox, new string[] { "RotorBStrings" })]
        public int Rotor4
        {
            get { return this.rotor4; }
            set
            {
                if (((int)value) != rotor4) hasChanges = true;
                this.rotor4 = (int)value;
                OnPropertyChanged("Rotor4");
            }
        }


        [TaskPane("Reflector", "Select the used reflector. If you don't want to use a reflector, select none here.",
            "Used rotors", 14, false, DisplayLevel.Beginner, ControlType.DynamicComboBox, new string[] { "ReflectorStrings" })]
        public int Reflector
        {
            get { return this.reflector; }
            set
            {
                if (((int)value) != reflector) hasChanges = true;
                this.reflector = (int)value;
                OnPropertyChanged("Reflector");
            }
        }

        /// <summary>
        /// This collection contains the values for the Rotor 1-3 comboboxes.
        /// </summary>
        public ObservableCollection<string> RotorAStrings
        {
            get { return rotorAStrings; }
            set
            {
                if (value != rotorAStrings)
                {
                    rotorAStrings = value;
                }
                OnPropertyChanged("RotorAStrings");
            }
        }

        /// <summary>
        /// This collection contains the values for the Rotor 4 combobox.
        /// </summary>
        public ObservableCollection<string> RotorBStrings
        {
            get { return rotorBStrings; }
            set
            {
                if (value != rotorBStrings)
                {
                    rotorBStrings = value;
                }
                OnPropertyChanged("RotorBStrings");
            }
        }

        /// <summary>
        /// This collection contains the values for the Rotor 1-3 comboboxes.
        /// </summary>
        public ObservableCollection<string> ReflectorStrings
        {
            get { return reflectorStrings; }
            set
            {
                if (value != reflectorStrings)
                {
                    reflectorStrings = value;
                }
                OnPropertyChanged("ReflectorStrings");
            }
        }

        #endregion

        #region Used ring settings

        [TaskPane("Ring 1 (right)", "Select the the offset for ring 1", "Ring settings", 20, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger,1,26)]
        public int Ring1
        {
            get { return ring1; }
            set
            {
                if (value != ring1)
                {
                    hasChanges = true;
                    ring1 = value;
                    OnPropertyChanged("Ring1");
                }
            }
        }

        [TaskPane("Ring 2", "Select the the offset for ring 2", "Ring settings", 21, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int Ring2
        {
            get { return ring2; }
            set
            {
                if (value != ring2)
                {
                    hasChanges = true;
                    ring2 = value;
                    OnPropertyChanged("Ring2");
                }
            }
        }

        [TaskPane("Ring 3", "Select the the offset for ring 3", "Ring settings", 22, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int Ring3
        {
            get { return ring3; }
            set
            {
                if (value != ring3)
                {
                    hasChanges = true;
                    ring3 = value;
                    OnPropertyChanged("Ring3");
                }
            }
        }

        [TaskPane("Ring 4 (left)", "Select the the offset for ring 4", "Ring settings", 23, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int Ring4
        {
            get { return ring4; }
            set
            {
                if (value != ring4)
                {
                    hasChanges = true;
                    ring4 = value;
                    OnPropertyChanged("Ring4");
                }
            }
        }

        #endregion

        #region Plugboard settings

        [TaskPane("Plugboard substitution", "Displays the current substitution", "Plugboard", 30, false, DisplayLevel.Beginner, ControlType.TextBoxReadOnly)]
        public string PlugBoard
        {
            get { return plugBoard.ToString(); }
            set { }
        }

        [TaskPane("Involutoric", "The mapping and the inverse mapping are the same. As an example if A mapps to X, also X is mapped to A", "Plugboard", 31, false, DisplayLevel.Expert, ControlType.CheckBox, "", null)]
        public bool Involutoric
        {
            get { return involutoricPlugBoard; }
            set
            {
                if (value != involutoricPlugBoard)
                {
                    involutoricPlugBoard = value;
                    hasChanges = true;
                    OnPropertyChanged("Involutoric");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("A=", "Select the letter for connecting this plug.", "Plugboard", 40, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardA
        {
            get { return alphabet.IndexOf(this.plugBoard[0]); }
            set { setPlugBoard(0, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("B=", "Select the letter for connecting this plug.", "Plugboard", 41, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardB
        {
            get { return alphabet.IndexOf(this.plugBoard[1]); }
            set { setPlugBoard(1, value);  }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("C=", "Select the letter for connecting this plug.", "Plugboard", 42, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardC
        {
            get { return alphabet.IndexOf(this.plugBoard[2]); }
            set { setPlugBoard(2, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("D=", "Select the letter for connecting this plug.", "Plugboard", 43, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardD
        {
            get { return alphabet.IndexOf(this.plugBoard[3]); }
            set { setPlugBoard(3, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("E=", "Select the letter for connecting this plug.", "Plugboard", 44, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardE
        {
            get { return alphabet.IndexOf(this.plugBoard[4]); }
            set { setPlugBoard(4, value);  }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("F=", "Select the letter for connecting this plug.", "Plugboard", 45, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardF
        {
            get { return alphabet.IndexOf(this.plugBoard[5]); }
            set { setPlugBoard(5, value);  }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane("G=", "Select the letter for connecting this plug.", "Plugboard", 46, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardG
        {
            get { return alphabet.IndexOf(this.plugBoard[6]); }
            set { setPlugBoard(6, value);  }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane("H=", "Select the letter for connecting this plug.", "Plugboard", 47, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardH
        {
            get { return alphabet.IndexOf(this.plugBoard[7]); }
            set { setPlugBoard(7, value);  }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane("I=", "Select the letter for connecting this plug.", "Plugboard", 48, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardI
        {
            get { return alphabet.IndexOf(this.plugBoard[8]); }
            set { setPlugBoard(8, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane("J=", "Select the letter for connecting this plug.", "Plugboard", 49, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardJ
        {
            get { return alphabet.IndexOf(this.plugBoard[9]); }
            set { setPlugBoard(9, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane("K=", "Select the letter for connecting this plug.", "Plugboard", 50, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardK
        {
            get { return alphabet.IndexOf(this.plugBoard[10]); }
            set { setPlugBoard(10, value);  }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane("L=", "Select the letter for connecting this plug.", "Plugboard", 51, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardL
        {
            get { return alphabet.IndexOf(this.plugBoard[11]); }
            set { setPlugBoard(11, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane("M=", "Select the letter for connecting this plug.", "Plugboard", 52, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardM
        {
            get { return alphabet.IndexOf(this.plugBoard[12]); }
            set { setPlugBoard(12, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane("N=", "Select the letter for connecting this plug.", "Plugboard", 53, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardN
        {
            get { return alphabet.IndexOf(this.plugBoard[13]); }
            set { setPlugBoard(13, value);  }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane("O=", "Select the letter for connecting this plug.", "Plugboard", 54, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardO
        {
            get { return alphabet.IndexOf(this.plugBoard[14]); }
            set { setPlugBoard(14, value);  }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane("P=", "Select the letter for connecting this plug.", "Plugboard", 55, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardP
        {
            get { return alphabet.IndexOf(this.plugBoard[15]); }
            set { setPlugBoard(15, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane("Q=", "Select the letter for connecting this plug.", "Plugboard", 56, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardQ
        {
            get { return alphabet.IndexOf(this.plugBoard[16]); }
            set { setPlugBoard(16, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane("R=", "Select the letter for connecting this plug.", "Plugboard", 57, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardR
        {
            get { return alphabet.IndexOf(this.plugBoard[17]); }
            set { setPlugBoard(17, value);  }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane("S=", "Select the letter for connecting this plug.", "Plugboard", 58, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardS
        {
            get { return alphabet.IndexOf(this.plugBoard[18]); }
            set { setPlugBoard(18, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane("T=", "Select the letter for connecting this plug.", "Plugboard", 59, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardT
        {
            get { return alphabet.IndexOf(this.plugBoard[19]); }
            set { setPlugBoard(19, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane("U=", "Select the letter for connecting this plug.", "Plugboard", 60, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardU
        {
            get { return alphabet.IndexOf(this.plugBoard[20]); }
            set { setPlugBoard(20, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane("V=", "Select the letter for connecting this plug.", "Plugboard", 61, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardV
        {
            get { return alphabet.IndexOf(this.plugBoard[21]); }
            set { setPlugBoard(21, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto","*", "Acht")]
        [TaskPane("W=", "Select the letter for connecting this plug.", "Plugboard", 62, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardW
        {
            get { return alphabet.IndexOf(this.plugBoard[22]); }
            set { setPlugBoard(22, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal,"Auto","*" ,"Acht")]
        [TaskPane("X=", "Select the letter for connecting this plug.", "Plugboard", 63, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardX
        {
            get { return alphabet.IndexOf(this.plugBoard[23]); }
            set { setPlugBoard(23, value);  }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Neun")]
        [TaskPane("Y=", "Select the letter for connecting this plug.", "Plugboard", 64, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardY
        {
            get { return alphabet.IndexOf(this.plugBoard[24]); }
            set { setPlugBoard(24, value); }
        }

        [SettingsFormat(1, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Neun")]
        [TaskPane("Z=", "Select the letter for connecting this plug.", "Plugboard", 65, false, DisplayLevel.Beginner, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardZ
        {
            get { return alphabet.IndexOf(this.plugBoard[25]); }
            set { setPlugBoard(25, value); }
        }


        [TaskPane("Remove all plugs", "By pressing this button the plugboard is reset to its empty state, i.e. all plugs are removed", "Plugboard", 70, false, DisplayLevel.Beginner, ControlType.Button)]
        public void ResetPlugboard()
        {
            plugBoard = new StringBuilder("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            OnPropertyChanged("PlugBoardDisplay");
            for (int i = 0; i < alphabet.Length; i++)
            {
                OnPropertyChanged("PlugBoard" + alphabet[i]);
            }
        }

        #endregion

        #endregion

        #region ISettings Member

        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                hasChanges = value;
            }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
