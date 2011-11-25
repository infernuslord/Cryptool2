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

        private int Presentation_Speed = 1;

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
                char newChar = alphabet[newIndex];
                //int newCharIndex = plugBoard.ToString().IndexOf(newChar);
                char currentChar = plugBoard[letterPos];
                int currentIndex = alphabet.IndexOf(currentChar);
                int preconnect = alphabet.IndexOf(this.plugBoard[newIndex]);




                if (this.plugBoard[preconnect] != alphabet[preconnect])
                {
                    this.plugBoard[preconnect] = alphabet[preconnect];
                    OnPropertyChanged("PlugBoard" + alphabet[preconnect]);
                    OnPropertyChanged("ForPresentation" + alphabet[newIndex] + alphabet[preconnect] + "if1");
                }


                OnPropertyChanged("ForPresentation" + alphabet[letterPos] + alphabet[newIndex] + "else");


                this.plugBoard[newIndex] = alphabet[letterPos];
                OnPropertyChanged("PlugBoard" + alphabet[newIndex]);



                if (this.plugBoard[letterPos] != alphabet[letterPos])
                {
                    this.plugBoard[currentIndex] = alphabet[currentIndex];
                    OnPropertyChanged("PlugBoard" + alphabet[currentIndex]);
                    OnPropertyChanged("ForPresentation" + alphabet[newIndex] + alphabet[currentIndex] + "if2");
                }


                this.plugBoard[letterPos] = newChar;
                OnPropertyChanged("PlugBoard" + alphabet[letterPos]);
                OnPropertyChanged("PlugBoardDisplay");
                
            }
        }

        private void setSettingsVisibility()
        {
            switch (this.model)
            {
                case 3: // Enigma M3 - curently analysis is supported only for this model
                    switch (this.action)
                    {
                        case 0: // Encrypt/Decrypt
                            // hide all options related to analysis
                            hideSettingsElement("AnalyzeKey");
                            hideSettingsElement("AnalyzeRotors");
                            hideSettingsElement("AnalysisUseRotorI");
                            hideSettingsElement("AnalysisUseRotorII");
                            hideSettingsElement("AnalysisUseRotorIII");
                            hideSettingsElement("AnalysisUseRotorIV");
                            hideSettingsElement("AnalysisUseRotorV");
                            hideSettingsElement("AnalysisUseRotorVI");
                            hideSettingsElement("AnalysisUseRotorVII");
                            hideSettingsElement("AnalysisUseRotorVIII");
                            hideSettingsElement("AnalyzeRings");
                            hideSettingsElement("KeySearchMethod");
                            hideSettingsElement("AnalyzePlugs");
                            hideSettingsElement("MaxSearchedPlugs");
                            hideSettingsElement("PlugSearchMethod");

                            // make sure, that everything is visible
                            showSettingsElement("Key");
                            showSettingsElement("Rotor1");
                            showSettingsElement("Rotor2");
                            showSettingsElement("Rotor3");
                            showSettingsElement("Ring1");
                            showSettingsElement("Ring2");
                            showSettingsElement("Ring3");
                            showPlugBoard();
                            break;
                        case 1: // Analyze
                            showSettingsElement("AnalyzeKey");
                            showSettingsElement("AnalyzeRotors");
                            showSettingsElement("AnalysisUseRotorI");
                            showSettingsElement("AnalysisUseRotorII");
                            showSettingsElement("AnalysisUseRotorIII");
                            showSettingsElement("AnalysisUseRotorIV");
                            showSettingsElement("AnalysisUseRotorV");
                            showSettingsElement("AnalysisUseRotorVI");
                            showSettingsElement("AnalysisUseRotorVII");
                            showSettingsElement("AnalysisUseRotorVIII");
                            showSettingsElement("AnalyzeRings");
                            showSettingsElement("KeySearchMethod");
                            showSettingsElement("AnalyzePlugs");
                            showSettingsElement("MaxSearchedPlugs");
                            showSettingsElement("PlugSearchMethod");

                            // now check which analysis options are active and hide those settings which are automatically determined
                            if (this.analyzeKey)
                            {
                                hideSettingsElement("Key");
                            }
                            else
                            {
                                showSettingsElement("Key");
                            }

                            if (this.analyzeRotors)
                            {
                                hideSettingsElement("Rotor1");
                                hideSettingsElement("Rotor2");
                                hideSettingsElement("Rotor3");
                            }
                            else
                            {
                                showSettingsElement("Rotor1");
                                showSettingsElement("Rotor2");
                                showSettingsElement("Rotor3");
                            }

                            if (this.analyzeRings)
                            {
                                hideSettingsElement("Ring1");
                                hideSettingsElement("Ring2");
                                hideSettingsElement("Ring3");
                            }
                            else
                            {
                                showSettingsElement("Ring1");
                                showSettingsElement("Ring2");
                                showSettingsElement("Ring3");
                            }

                            if (this.analyzePlugs)
                            {
                                hidePlugBoard();
                            }
                            else
                            {
                                showPlugBoard();
                            }
                            break;
                    }
                    break;
                default:
                    // hide all options related to analysis
                    hideSettingsElement("AnalyzeKey");
                    hideSettingsElement("AnalyzeRotors");
                    hideSettingsElement("AnalysisUseRotorI");
                    hideSettingsElement("AnalysisUseRotorII");
                    hideSettingsElement("AnalysisUseRotorIII");
                    hideSettingsElement("AnalysisUseRotorIV");
                    hideSettingsElement("AnalysisUseRotorV");
                    hideSettingsElement("AnalysisUseRotorVI");
                    hideSettingsElement("AnalysisUseRotorVII");
                    hideSettingsElement("AnalysisUseRotorVIII");
                    hideSettingsElement("AnalyzeRings");
                    hideSettingsElement("KeySearchMethod");
                    hideSettingsElement("AnalyzePlugs");
                    hideSettingsElement("MaxSearchedPlugs");
                    hideSettingsElement("PlugSearchMethod");
                    break;
            }
        }

        private void hidePlugBoard()
        {
            foreach (char c in this.alphabet)
            {
                hideSettingsElement("PlugBoard" + c);
            }
            hideSettingsElement("PlugBoard");
            hideSettingsElement("ResetPlugboard");
        }

        private void showPlugBoard()
        {
            foreach (char c in this.alphabet)
            {
                showSettingsElement("PlugBoard" + c);
            }
            showSettingsElement("PlugBoard");
            showSettingsElement("ResetPlugboard");
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
        }

        public void Initialize()
        {
            hideSettingsElement("Rotor4"); hideSettingsElement("Ring4");
            setSettingsVisibility();
        }

        #endregion

        #region Public properties

        public string Alphabet
        {
            get { return alphabet; }
            set { alphabet = value; }
        }

        #endregion

        #region Taskpane settings

        [ContextMenu( "ModelCaption", "ModelTooltip",
            0, ContextMenuControlType.ComboBox, null,
            new string[] { "ModelList1", "ModelList2", "ModelList3", "ModelList4", "ModelList5", "ModelList6", "ModelList7" })]
        [TaskPane( "ModelTPCaption", "ModelTPTooltip",
            null, 0, false, ControlType.ComboBox,
            new string[] { "ModelList1", "ModelList2", "ModelList3", "ModelList4", "ModelList5", "ModelList6", "ModelList7" })]
        [PropertySaveOrder(1)]
        public int Model
        {
            get { return this.model; }
            set
            {
                if (value == model)
                    return;

                this.model = value;
                OnPropertyChanged("Model");
                switch (this.model)
                {
                    case 0: // Enigma A/B
                        actionStrings.Clear(); actionStrings.Add("Encrypt"); actionStrings.Add("Decrypt"); action = 0; OnPropertyChanged("Action");
                        if (key.Length > 3) key = key.Remove(0, 1); OnPropertyChanged("Key");
                        rotorAStrings.Clear(); rotorAStrings.Add("I (C) -- since 1924"); rotorAStrings.Add("II (C) -- since 1924"); rotorAStrings.Add("III (C) -- since 1924");
                        rotor1 = 0; rotor2 = 1; rotor3 = 2; OnPropertyChanged("Rotor1"); OnPropertyChanged("Rotor2"); OnPropertyChanged("Rotor3");
                        rotorBStrings.Clear(); rotorBStrings.Add("Not available for this model."); rotor4 = 0; OnPropertyChanged("Rotor4");
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


        [TaskPane( "KeyCaption", "KeyTooltip",
            null, 1, false, ControlType.TextBox, ValidationType.RegEx, "^[A-Za-z]{3,4}$")]
        public string Key
        {
            get { return this.key; }
            set
            {
                if (value != key)
                {
                    this.key = value;
                    OnPropertyChanged("Key");   
                }
            }
        }


        [TaskPane( "ActionCaption", "ActionTooltip",
            null, 2, false, ControlType.DynamicComboBox, new string[] { "ActionStrings" })]
        [PropertySaveOrder(9)]
        public int Action
        {
            get { return this.action; }
            set
            {
                if (((int)value) != action)
                {
                    this.action = (int)value;
                    OnPropertyChanged("Action");
                    setSettingsVisibility();   
                }
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
                    OnPropertyChanged("ActionStrings");
                }
            }
        }

        #region Text options

        [ContextMenu( "UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip",
            3, ContextMenuControlType.ComboBox, null,
            new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        [TaskPane( "UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip",
            "TextOptionsGroup", 3, false, ControlType.ComboBox,
            new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public int UnknownSymbolHandling
        {
            get { return this.unknownSymbolHandling; }
            set
            {
                if ((int)value != unknownSymbolHandling)
                {
                    this.unknownSymbolHandling = (int)value;
                    OnPropertyChanged("UnknownSymbolHandling");   
                }
            }
        }

        [ContextMenu( "CaseHandlingCaption", "CaseHandlingTooltip",
            4, ContextMenuControlType.ComboBox, null,
            new string[] { "CaseHandlingList1", "CaseHandlingList2", "CaseHandlingList3" })]
        [TaskPane( "CaseHandlingCaption", "CaseHandlingTooltip",
            "TextOptionsGroup", 4, false, ControlType.ComboBox,
            new string[] { "CaseHandlingList1", "CaseHandlingList2", "CaseHandlingList3" })]
        public int CaseHandling
        {
            get { return this.caseHandling; }
            set
            {
                if ((int)value != caseHandling)
                {
                    this.caseHandling = (int)value;
                    OnPropertyChanged("CaseHandling");   
                }
            }
        }

        #endregion

        #region Analysis options

        [TaskPane( "AnalyzeKeyCaption", "AnalyzeKeyTooltip",
            "AnalysisOptionsGroup", 6, false, ControlType.CheckBox, "", null)]
        public bool AnalyzeKey
        {
            get { return analyzeKey; }
            set
            {
                if (value != analyzeKey)
                {
                    analyzeKey = value;
                    OnPropertyChanged("AnalyzeKey");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane( "AnalyzeRotorsCaption", "AnalyzeRotorsTooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalyzeRotors
        {
            get { return analyzeRotors; }
            set
            {
                if (value != analyzeRotors)
                {
                    analyzeRotors = value;
                    OnPropertyChanged("AnalyzeRotors");
                    setSettingsVisibility();
                }
            }
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane( "AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorI
        {
            get { return analysisUseRotorI; }
            set
            {
                if (value != analysisUseRotorI)
                {
                    analysisUseRotorI = value;
                    OnPropertyChanged("AnalysisUseRotorI");
                }
            }
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane( "AnalysisUseRotorIICaption", "AnalysisUseRotorIITooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorII
        {
            get { return analysisUseRotorII; }
            set
            {
                if (value != analysisUseRotorII)
                {
                    analysisUseRotorII = value;
                    OnPropertyChanged("AnalysisUseRotorII");
                }
            }
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane( "AnalysisUseRotorIIICaption", "AnalysisUseRotorIIITooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorIII
        {
            get { return analysisUseRotorIII; }
            set
            {
                if (value != analysisUseRotorIII)
                {
                    analysisUseRotorIII = value;
                    OnPropertyChanged("AnalysisUseRotorIII");
                }
            }
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane( "AnalysisUseRotorIVCaption", "AnalysisUseRotorIVTooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorIV
        {
            get { return analysisUseRotorIV; }
            set
            {
                if (value != analysisUseRotorIV)
                {
                    analysisUseRotorIV = value;
                    OnPropertyChanged("AnalysisUseRotorIV");
                }
            }
        }


        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane( "AnalysisUseRotorVCaption", "AnalysisUseRotorVTooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorV
        {
            get { return analysisUseRotorV; }
            set
            {
                if (value != analysisUseRotorV)
                {
                    analysisUseRotorV = value;
                    OnPropertyChanged("AnalysisUseRotorV");
                }
            }
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane( "AnalysisUseRotorVICaption", "AnalysisUseRotorVITooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorVI
        {
            get { return analysisUseRotorVI; }
            set
            {
                if (value != analysisUseRotorVI)
                {
                    analysisUseRotorVI = value;
                    OnPropertyChanged("AnalysisUseRotorVI");
                }
            }
        }


        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane( "AnalysisUseRotorVIICaption", "AnalysisUseRotorVIITooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorVII
        {
            get { return analysisUseRotorVII; }
            set
            {
                if (value != analysisUseRotorVII)
                {
                    analysisUseRotorVII = value;
                    OnPropertyChanged("AnalysisUseRotorVII");
                }
            }
        }


        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane( "AnalysisUseRotorVIIICaption", "AnalysisUseRotorVIIITooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorVIII
        {
            get { return analysisUseRotorVIII; }
            set
            {
                if (value != analysisUseRotorVIII)
                {
                    analysisUseRotorVIII = value;
                    OnPropertyChanged("AnalysisUseRotorVIII");
                }
            }
        }


        [TaskPane( "AnalyzeRingsCaption", "AnalyzeRingsTooltip",
            "AnalysisOptionsGroup", 8, false, ControlType.CheckBox, "", null)]
        public bool AnalyzeRings
        {
            get { return analyzeRings; }
            set
            {
                if (value != analyzeRings)
                {
                    analyzeRings = value;
                    OnPropertyChanged("AnalyzeRings");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane("KeySearchMethodCaption", "KeySearchMethodTooltip", "AnalysisOptionsGroup", 8, false, ControlType.ComboBox, new string[] { "KeySearchMethodList1", "KeySearchMethodList2", "KeySearchMethodList3", "KeySearchMethodList4", "KeySearchMethodList5", "KeySearchMethodList6" })]
        public int KeySearchMethod
        {
            get { return this.keySearchMethod; }
            set
            {
                if (value != keySearchMethod)
                {
                    keySearchMethod = value;
                    OnPropertyChanged("KeySearchMethod");
                }
            }
        }

        [TaskPane( "AnalyzePlugsCaption", "AnalyzePlugsTooltip",
            "AnalysisOptionsGroup", 9, false, ControlType.CheckBox, "", null)]
        public bool AnalyzePlugs
        {
            get { return analyzePlugs; }
            set
            {
                if (value != analyzePlugs)
                {
                    analyzePlugs = value;
                    OnPropertyChanged("AnalyzePlugs");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane( "MaxSearchedPlugsCaption", "MaxSearchedPlugsTooltip",
            "AnalysisOptionsGroup", 9, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int MaxSearchedPlugs
        {
            get { return this.maxSearchedPlugs; }
            set
            {
                if (value != maxSearchedPlugs)
                {
                    maxSearchedPlugs = value;
                    OnPropertyChanged("MaxSearchedPlugs");
                }
            }
        }

        [TaskPane("PlugSearchMethodCaption", "PlugSearchMethodTooltip", "AnalysisOptionsGroup", 9, false, ControlType.ComboBox, new string[] { "KeySearchMethodList1", "KeySearchMethodList2", "KeySearchMethodList3", "KeySearchMethodList4", "KeySearchMethodList5", "KeySearchMethodList6" })]
        public int PlugSearchMethod
        {
            get { return this.plugSearchMethod; }
            set
            {
                if (value != plugSearchMethod)
                {
                    plugSearchMethod = value;
                    OnPropertyChanged("PlugSearchMethod");
                }
            }
        }

        #endregion

        #region Used rotor settings

        [TaskPane( "Rotor1Caption", "Rotor1Tooltip",
            "UsedRotorsGroup", 10, false, ControlType.DynamicComboBox, new string[] { "RotorAStrings" })]
        public int Rotor1
        {
            get { return this.rotor1; }
            set
            {
                if (((int)value) != rotor1)
                {
                    checkRotorChange(1, this.rotor1, value);
                    this.rotor1 = value;
                    OnPropertyChanged("Rotor1");   
                }
            }
        }

        [TaskPane( "Rotor2Caption", "Rotor2Tooltip",
            "UsedRotorsGroup", 11, false, ControlType.DynamicComboBox, new string[] { "RotorAStrings" })]
        public int Rotor2
        {
            get { return this.rotor2; }
            set
            {
                if (((int)value) != rotor2)
                {
                    checkRotorChange(2, this.rotor2, value);
                    this.rotor2 = (int)value;
                    OnPropertyChanged("Rotor2");   
                }
            }
        }

        [TaskPane( "Rotor3Caption", "Rotor3Tooltip",
            "UsedRotorsGroup", 12, false, ControlType.DynamicComboBox, new string[] { "RotorAStrings" })]
        public int Rotor3
        {
            get { return this.rotor3; }
            set
            {
                if (((int)value) != rotor3)
                {
                    checkRotorChange(3, this.rotor3, value);
                    this.rotor3 = (int)value;
                    OnPropertyChanged("Rotor3");   
                }
            }
        }

        [TaskPane( "Rotor4Caption", "Rotor4Tooltip",
            "UsedRotorsGroup", 13, false, ControlType.DynamicComboBox, new string[] { "RotorBStrings" })]
        public int Rotor4
        {
            get { return this.rotor4; }
            set
            {
                if (((int)value) != rotor4)
                {
                    this.rotor4 = (int)value;
                    OnPropertyChanged("Rotor4");   
                }
            }
        }


        [TaskPane( "ReflectorCaption", "ReflectorTooltip",
            "UsedRotorsGroup", 14, false, ControlType.DynamicComboBox, new string[] { "ReflectorStrings" })]
        public int Reflector
        {
            get { return this.reflector; }
            set
            {
                if (((int)value) != reflector)
                {
                    this.reflector = (int)value;
                    OnPropertyChanged("Reflector");   
                }
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
                    OnPropertyChanged("RotorAStrings");
                }
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
                    OnPropertyChanged("RotorBStrings");
                }
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
                    OnPropertyChanged("ReflectorStrings");
                }
            }
        }

        #endregion

        #region Used ring settings

        [TaskPane( "Ring1Caption", "Ring1Tooltip", "RingSettingsGroup", 20, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int Ring1
        {
            get { return ring1; }
            set
            {
                if (value < ring1)
                {
                    if (value + 1 == ring1 && false) // TODO: always false
                    {
                        ring1 = value;
                        OnPropertyChanged("Ring1down");
                    }
                    else
                    {
                        ring1 = value;
                        OnPropertyChanged("Ring1NewValue");
                    }
                }
                if (value > ring1)
                {
                    if (value == ring1 + 1 && false) // TODO: always false
                    {
                        ring1 = value;
                        OnPropertyChanged("Ring1up");
                    }
                    else
                    {
                        ring1 = value;
                        OnPropertyChanged("Ring1NewValue");
                    }
                }
                OnPropertyChanged("Ring1");
            }
        }

        [TaskPane( "Ring2Caption", "Ring2Tooltip", "RingSettingsGroup", 21, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int Ring2
        {
            get { return ring2; }
            set
            {
                if (value < ring2)
                {
                    if (value + 1 == ring2 && false) // TODO: always false
                    {
                        ring2 = value;
                        OnPropertyChanged("Ring2down");
                    }
                    else
                    {
                        ring2 = value;
                        OnPropertyChanged("Ring2NewValue");

                    }

                }
                if (value > ring2)
                {
                    if (value == ring2 + 1 && false) // TODO: always false
                    {
                        ring2 = value;
                        OnPropertyChanged("Ring2up");
                    }
                    else
                    {
                        ring2 = value;
                        OnPropertyChanged("Ring2NewValue");
                    }
                }
                OnPropertyChanged("Ring2");
            }
        }

        [TaskPane( "Ring3Caption", "Ring3Tooltip", "RingSettingsGroup", 22, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int Ring3
        {
            get { return ring3; }
            set
            {
                if (value < ring3)
                {
                    if (value + 1 == ring3 && false) // TODO: always false
                    {
                        ring3 = value;
                        OnPropertyChanged("Ring3down");
                    }
                    else
                    {
                        ring3 = value;
                        OnPropertyChanged("Ring3NewValue");
                    }
                }
                if (value > ring3)
                {
                    if (value == ring3 + 1 && false) // TODO: always false
                    {
                        ring3 = value;
                        OnPropertyChanged("Ring3up");
                    }
                    else
                    {
                        ring3 = value;
                        OnPropertyChanged("Ring3NewValue");
                    }
                }
                OnPropertyChanged("Ring3");
            }
        }

        [TaskPane( "Ring4Caption", "Ring4Tooltip", "RingSettingsGroup", 23, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int Ring4
        {
            get { return ring4; }
            set
            {
                if (value < ring4)
                {
                    ring4 = value;
                    OnPropertyChanged("Ring4down");
                }
                if (value > ring4)
                {
                    ring4 = value;
                    OnPropertyChanged("Ring4up");
                }
            }
        }

        #endregion

        #region Plugboard settings

        [TaskPane( "PlugBoardCaption", "PlugBoardTooltip", "PlugboardGroup", 30, false, ControlType.TextBoxReadOnly)]
        public string PlugBoard
        {
            get { return plugBoard.ToString(); }
            set { }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane( "PlugBoardACaption", "PlugBoardATooltip", "PlugboardGroup", 40, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardA
        {
            get { return alphabet.IndexOf(this.plugBoard[0]); }
            set { setPlugBoard(0, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane( "PlugBoardBCaption", "PlugBoardBTooltip", "PlugboardGroup", 41, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardB
        {
            get { return alphabet.IndexOf(this.plugBoard[1]); }
            set { setPlugBoard(1, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane( "PlugBoardCCaption", "PlugBoardCTooltip", "PlugboardGroup", 42, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardC
        {
            get { return alphabet.IndexOf(this.plugBoard[2]); }
            set { setPlugBoard(2, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane( "PlugBoardDCaption", "PlugBoardDTooltip", "PlugboardGroup", 43, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardD
        {
            get { return alphabet.IndexOf(this.plugBoard[3]); }
            set { setPlugBoard(3, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane( "PlugBoardECaption", "PlugBoardETooltip", "PlugboardGroup", 44, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardE
        {
            get { return alphabet.IndexOf(this.plugBoard[4]); }
            set { setPlugBoard(4, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane( "PlugBoardFCaption", "PlugBoardFTooltip", "PlugboardGroup", 45, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardF
        {
            get { return alphabet.IndexOf(this.plugBoard[5]); }
            set { setPlugBoard(5, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane( "PlugBoardGCaption", "PlugBoardGTooltip", "PlugboardGroup", 46, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardG
        {
            get { return alphabet.IndexOf(this.plugBoard[6]); }
            set { setPlugBoard(6, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane( "PlugBoardHCaption", "PlugBoardHTooltip", "PlugboardGroup", 47, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardH
        {
            get { return alphabet.IndexOf(this.plugBoard[7]); }
            set { setPlugBoard(7, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane( "PlugBoardICaption", "PlugBoardITooltip", "PlugboardGroup", 48, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardI
        {
            get { return alphabet.IndexOf(this.plugBoard[8]); }
            set { setPlugBoard(8, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane( "PlugBoardJCaption", "PlugBoardJTooltip", "PlugboardGroup", 49, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardJ
        {
            get { return alphabet.IndexOf(this.plugBoard[9]); }
            set { setPlugBoard(9, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane( "PlugBoardKCaption", "PlugBoardKTooltip", "PlugboardGroup", 50, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardK
        {
            get { return alphabet.IndexOf(this.plugBoard[10]); }
            set { setPlugBoard(10, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane( "PlugBoardLCaption", "PlugBoardLTooltip", "PlugboardGroup", 51, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardL
        {
            get { return alphabet.IndexOf(this.plugBoard[11]); }
            set { setPlugBoard(11, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane( "PlugBoardMCaption", "PlugBoardMTooltip", "PlugboardGroup", 52, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardM
        {
            get { return alphabet.IndexOf(this.plugBoard[12]); }
            set { setPlugBoard(12, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane( "PlugBoardNCaption", "PlugBoardNTooltip", "PlugboardGroup", 53, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardN
        {
            get { return alphabet.IndexOf(this.plugBoard[13]); }
            set { setPlugBoard(13, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane( "PlugBoardOCaption", "PlugBoardOTooltip", "PlugboardGroup", 54, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardO
        {
            get { return alphabet.IndexOf(this.plugBoard[14]); }
            set { setPlugBoard(14, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane( "PlugBoardPCaption", "PlugBoardPTooltip", "PlugboardGroup", 55, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardP
        {
            get { return alphabet.IndexOf(this.plugBoard[15]); }
            set { setPlugBoard(15, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane( "PlugBoardQCaption", "PlugBoardQTooltip", "PlugboardGroup", 56, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardQ
        {
            get { return alphabet.IndexOf(this.plugBoard[16]); }
            set { setPlugBoard(16, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane( "PlugBoardRCaption", "PlugBoardRTooltip", "PlugboardGroup", 57, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardR
        {
            get { return alphabet.IndexOf(this.plugBoard[17]); }
            set { setPlugBoard(17, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane( "PlugBoardSCaption", "PlugBoardSTooltip", "PlugboardGroup", 58, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardS
        {
            get { return alphabet.IndexOf(this.plugBoard[18]); }
            set { setPlugBoard(18, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane( "PlugBoardTCaption", "PlugBoardTTooltip", "PlugboardGroup", 59, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardT
        {
            get { return alphabet.IndexOf(this.plugBoard[19]); }
            set { setPlugBoard(19, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane( "PlugBoardUCaption", "PlugBoardUTooltip", "PlugboardGroup", 60, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardU
        {
            get { return alphabet.IndexOf(this.plugBoard[20]); }
            set { setPlugBoard(20, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane( "PlugBoardVCaption", "PlugBoardVTooltip", "PlugboardGroup", 61, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardV
        {
            get { return alphabet.IndexOf(this.plugBoard[21]); }
            set { setPlugBoard(21, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane( "PlugBoardWCaption", "PlugBoardWTooltip", "PlugboardGroup", 62, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardW
        {
            get { return alphabet.IndexOf(this.plugBoard[22]); }
            set { setPlugBoard(22, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane( "PlugBoardXCaption", "PlugBoardXTooltip", "PlugboardGroup", 63, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardX
        {
            get { return alphabet.IndexOf(this.plugBoard[23]); }
            set { setPlugBoard(23, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Neun")]
        [TaskPane( "PlugBoardYCaption", "PlugBoardYTooltip", "PlugboardGroup", 64, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardY
        {
            get { return alphabet.IndexOf(this.plugBoard[24]); }
            set { setPlugBoard(24, value); }
        }

        [SettingsFormat(1, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Neun")]
        [TaskPane( "PlugBoardZCaption", "PlugBoardZTooltip", "PlugboardGroup", 65, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardZ
        {
            get { return alphabet.IndexOf(this.plugBoard[25]); }
            set { setPlugBoard(25, value); }
        }


        [TaskPane( "ResetPlugboardCaption", "ResetPlugboardTooltip", "PlugboardGroup", 70, false, ControlType.Button)]
        public void ResetPlugboard()
        {
            plugBoard = new StringBuilder("ABCDEFGHIJKLMNOPQRSTUVWXYZ");            
            foreach (char c in this.alphabet)
            {
                OnPropertyChanged("PlugBoard" + c);
            }

            // Are the following needed? For the presentation?
            OnPropertyChanged("PlugBoardDisplay");
            OnPropertyChanged("Remove all Plugs");
        }

        #endregion

        [TaskPane( "PresentationSpeedCaption", "PresentationSpeedTooltip", "PresentationGroup", 71, true, ControlType.Slider, 2, 25)]
        public int PresentationSpeed
        {
            get { return (int)Presentation_Speed; }
            set
            {
                if ((value) != Presentation_Speed)
                {
                    Presentation_Speed = value;
                    OnPropertyChanged("PresentationSpeed");   
                }
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
