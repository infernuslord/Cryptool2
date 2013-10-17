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
using System.Windows;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace SigabaBruteforce
{
    // HOWTO: rename class (click name, press F2)
    public class SigabaBruteforceSettings : ISettings
    {
       

        #region Private Variables

        private int someParameter = 0;

        private int cipherRotor1From = 0;
        private int cipherRotor1To = 25;

        private int cipherRotor2From = 0;
        private int cipherRotor2To = 25;

        private int cipherRotor3From = 0;
        private int cipherRotor3To = 25;

        private int cipherRotor4From = 0;
        private int cipherRotor4To = 25;

        private int cipherRotor5From = 0;
        private int cipherRotor5To = 25;

        private int controlRotor1From = 0;
        private int controlRotor1To = 25;

        private int controlRotor2From = 0;
        private int controlRotor2To = 25;

        private int controlRotor3From = 0;
        private int controlRotor3To = 25;

        private int controlRotor4From = 0;
        private int controlRotor4To = 25;

        private int controlRotor5From = 0;
        private int controlRotor5To = 25;

        private int indexRotor1From = 0;
        private int indexRotor1To = 9;

        private int indexRotor2From = 0;
        private int indexRotor2To = 9;

        private int indexRotor3From = 0;
        private int indexRotor3To = 9;

        private int indexRotor4From = 0;
        private int indexRotor4To = 9;

        private int indexRotor5From = 0;
        private int indexRotor5To = 9;

        private bool cipherRotor1Rotors = true;
        private bool cipherRotor2Rotors = true;
        private bool cipherRotor3Rotors = true;
        private bool cipherRotor4Rotors = true;
        private bool cipherRotor5Rotors = true;

        private bool controlRotor1Rotors = true;
        private bool controlRotor2Rotors = true;
        private bool controlRotor3Rotors = true;
        private bool controlRotor4Rotors = true;
        private bool controlRotor5Rotors = true;

        private bool indexRotor1Rotors = true;
        private bool indexRotor2Rotors = true;
        private bool indexRotor3Rotors = true;
        private bool indexRotor4Rotors = true;
        private bool indexRotor5Rotors = true;


        private bool cipher1AnalysisUseRotor1 = true;
        private bool cipher1AnalysisUseRotor2 = true;
        private bool cipher1AnalysisUseRotor3 = true;
        private bool cipher1AnalysisUseRotor4 = true;
        private bool cipher1AnalysisUseRotor5 = true;
        private bool cipher1AnalysisUseRotor6 = true;
        private bool cipher1AnalysisUseRotor7 = true;
        private bool cipher1AnalysisUseRotor8 = true;
        private bool cipher1AnalysisUseRotor9 = true;
        private bool cipher1AnalysisUseRotor0 = true;

        private bool cipher2AnalysisUseRotor1 = true;
        private bool cipher2AnalysisUseRotor2 = true;
        private bool cipher2AnalysisUseRotor3 = true;
        private bool cipher2AnalysisUseRotor4 = true;
        private bool cipher2AnalysisUseRotor5 = true;
        private bool cipher2AnalysisUseRotor6 = true;
        private bool cipher2AnalysisUseRotor7 = true;
        private bool cipher2AnalysisUseRotor8 = true;
        private bool cipher2AnalysisUseRotor9 = true;
        private bool cipher2AnalysisUseRotor0 = true;

        private bool cipher3AnalysisUseRotor1 = true;
        private bool cipher3AnalysisUseRotor2 = true;
        private bool cipher3AnalysisUseRotor3 = true;
        private bool cipher3AnalysisUseRotor4 = true;
        private bool cipher3AnalysisUseRotor5 = true;
        private bool cipher3AnalysisUseRotor6 = true;
        private bool cipher3AnalysisUseRotor7 = true;
        private bool cipher3AnalysisUseRotor8 = true;
        private bool cipher3AnalysisUseRotor9 = true;
        private bool cipher3AnalysisUseRotor0 = true;

        private bool cipher4AnalysisUseRotor1 = true;
        private bool cipher4AnalysisUseRotor2 = true;
        private bool cipher4AnalysisUseRotor3 = true;
        private bool cipher4AnalysisUseRotor4 = true;
        private bool cipher4AnalysisUseRotor5 = true;
        private bool cipher4AnalysisUseRotor6 = true;
        private bool cipher4AnalysisUseRotor7 = true;
        private bool cipher4AnalysisUseRotor8 = true;
        private bool cipher4AnalysisUseRotor9 = true;
        private bool cipher4AnalysisUseRotor0 = true;

        private bool cipher5AnalysisUseRotor1 = true;
        private bool cipher5AnalysisUseRotor2 = true;
        private bool cipher5AnalysisUseRotor3 = true;
        private bool cipher5AnalysisUseRotor4 = true;
        private bool cipher5AnalysisUseRotor5 = true;
        private bool cipher5AnalysisUseRotor6 = true;
        private bool cipher5AnalysisUseRotor7 = true;
        private bool cipher5AnalysisUseRotor8 = true;
        private bool cipher5AnalysisUseRotor9 = true;
        private bool cipher5AnalysisUseRotor0 = true;



        private bool control1AnalysisUseRotor1 = true;
        private bool control1AnalysisUseRotor2 = true;
        private bool control1AnalysisUseRotor3 = true;
        private bool control1AnalysisUseRotor4 = true;
        private bool control1AnalysisUseRotor5 = true;
        private bool control1AnalysisUseRotor6 = true;
        private bool control1AnalysisUseRotor7 = true;
        private bool control1AnalysisUseRotor8 = true;
        private bool control1AnalysisUseRotor9 = true;
        private bool control1AnalysisUseRotor0 = true;

        private bool control2AnalysisUseRotor1 = true;
        private bool control2AnalysisUseRotor2 = true;
        private bool control2AnalysisUseRotor3 = true;
        private bool control2AnalysisUseRotor4 = true;
        private bool control2AnalysisUseRotor5 = true;
        private bool control2AnalysisUseRotor6 = true;
        private bool control2AnalysisUseRotor7 = true;
        private bool control2AnalysisUseRotor8 = true;
        private bool control2AnalysisUseRotor9 = true;
        private bool control2AnalysisUseRotor0 = true;

        private bool control3AnalysisUseRotor1 = true;
        private bool control3AnalysisUseRotor2 = true;
        private bool control3AnalysisUseRotor3 = true;
        private bool control3AnalysisUseRotor4 = true;
        private bool control3AnalysisUseRotor5 = true;
        private bool control3AnalysisUseRotor6 = true;
        private bool control3AnalysisUseRotor7 = true;
        private bool control3AnalysisUseRotor8 = true;
        private bool control3AnalysisUseRotor9 = true;
        private bool control3AnalysisUseRotor0 = true;

        private bool control4AnalysisUseRotor1 = true;
        private bool control4AnalysisUseRotor2 = true;
        private bool control4AnalysisUseRotor3 = true;
        private bool control4AnalysisUseRotor4 = true;
        private bool control4AnalysisUseRotor5 = true;
        private bool control4AnalysisUseRotor6 = true;
        private bool control4AnalysisUseRotor7 = true;
        private bool control4AnalysisUseRotor8 = true;
        private bool control4AnalysisUseRotor9 = true;
        private bool control4AnalysisUseRotor0 = true;

        private bool control5AnalysisUseRotor1 = true;
        private bool control5AnalysisUseRotor2 = true;
        private bool control5AnalysisUseRotor3 = true;
        private bool control5AnalysisUseRotor4 = true;
        private bool control5AnalysisUseRotor5 = true;
        private bool control5AnalysisUseRotor6 = true;
        private bool control5AnalysisUseRotor7 = true;
        private bool control5AnalysisUseRotor8 = true;
        private bool control5AnalysisUseRotor9 = true;
        private bool control5AnalysisUseRotor0 = true;

        private bool index1AnalysisUseRotor1 = true;
        private bool index1AnalysisUseRotor2 = true;
        private bool index1AnalysisUseRotor3 = true;
        private bool index1AnalysisUseRotor4 = true;
        private bool index1AnalysisUseRotor5 = true;
        
        private bool index2AnalysisUseRotor1 = true;
        private bool index2AnalysisUseRotor2 = true;
        private bool index2AnalysisUseRotor3 = true;
        private bool index2AnalysisUseRotor4 = true;
        private bool index2AnalysisUseRotor5 = true;
        
        private bool index3AnalysisUseRotor1 = true;
        private bool index3AnalysisUseRotor2 = true;
        private bool index3AnalysisUseRotor3 = true;
        private bool index3AnalysisUseRotor4 = true;
        private bool index3AnalysisUseRotor5 = true;
        
        private bool index4AnalysisUseRotor1 = true;
        private bool index4AnalysisUseRotor2 = true;
        private bool index4AnalysisUseRotor3 = true;
        private bool index4AnalysisUseRotor4 = true;
        private bool index4AnalysisUseRotor5 = true;
        
        private bool index5AnalysisUseRotor1 = true;
        private bool index5AnalysisUseRotor2 = true;
        private bool index5AnalysisUseRotor3 = true;
        private bool index5AnalysisUseRotor4 = true;
        private bool index5AnalysisUseRotor5 = true;
        

        #endregion

        public SigabaBruteforceSettings()
        {
            setSettingsVisibility();
        }
        public void Initialize()
        {
            setSettingsVisibility();
        }

        #region TaskPane Settings

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("Cipher Rotor 1: From", "PlugBoardATooltip", "PositionOptionsGroup", 1, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor1From
        {
            get
            {
                return this.cipherRotor1From;
            }
            set
            {

                this.cipherRotor1From = (int)value;
                OnPropertyChanged("CipherRotor1From");
                setSettingsVisibility();

                if (value > cipherRotor1To)
                {
                    CipherRotor1To = value;


                }
            }
        }

        #region on/off cipherrotor

        [TaskPane("AnalyzeCipherRotors1Caption", "AnalyzeRotorsTooltip",
            "Rotor Analysis", 6, false, ControlType.CheckBox, "", null)]
        public bool CipherRotor1Rotors
        {
            get { return cipherRotor1Rotors; }
            set
            {
                if (value != cipherRotor1Rotors)
                {
                    cipherRotor1Rotors = value;
                    OnPropertyChanged("CipherRotor1Rotors");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane("AnalyzeCipherrotors2caption", "AnalyzeRotorsTooltip",
           "Rotor Analysis", 6, false, ControlType.CheckBox, "", null)]
        public bool CipherRotor2Rotors
        {
            get { return cipherRotor2Rotors; }
            set
            {
                if (value != cipherRotor2Rotors)
                {
                    cipherRotor2Rotors = value;
                    OnPropertyChanged("CipherRotor2Rotors");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane("AnalyzeCipherRotors3Caption", "AnalyzeRotorsTooltip",
           "Rotor Analysis", 6, false, ControlType.CheckBox, "", null)]
        public bool CipherRotor3Rotors
        {
            get { return cipherRotor3Rotors; }
            set
            {
                if (value != cipherRotor3Rotors)
                {
                    cipherRotor3Rotors = value;
                    OnPropertyChanged("CipherRotor3Rotors");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane("AnalyzeCipherRotors4Caption", "AnalyzeRotorsTooltip",
           "Rotor Analysis", 6, false, ControlType.CheckBox, "", null)]
        public bool CipherRotor4Rotors
        {
            get { return cipherRotor4Rotors; }
            set
            {
                if (value != cipherRotor4Rotors)
                {
                    cipherRotor4Rotors = value;
                    OnPropertyChanged("CipherRotor4Rotors");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane("AnalyzeCipherrotors5caption", "AnalyzeRotorsTooltip",
           "Rotor Analysis", 6, false, ControlType.CheckBox, "", null)]
        public bool CipherRotor5Rotors
        {
            get { return cipherRotor5Rotors; }
            set
            {
                if (value != cipherRotor5Rotors)
                {
                    cipherRotor5Rotors = value;
                    OnPropertyChanged("CipherRotor5Rotors");
                    setSettingsVisibility();
                }
            }
        }

        #endregion

        #region on/off controlrotor

        [TaskPane("AnalyzeControlRotors1Caption", "AnalyzeRotorsTooltip",
            "Rotor Analysis", 6, false, ControlType.CheckBox, "", null)]
        public bool ControlRotor1Rotors
        {
            get { return controlRotor1Rotors; }
            set
            {
                if (value != controlRotor1Rotors)
                {
                    controlRotor1Rotors = value;
                    OnPropertyChanged("ControlRotor1Rotors");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane("AnalyzeControlrotors2caption", "AnalyzeRotorsTooltip",
           "Rotor Analysis", 6, false, ControlType.CheckBox, "", null)]
        public bool ControlRotor2Rotors
        {
            get { return controlRotor2Rotors; }
            set
            {
                if (value != controlRotor2Rotors)
                {
                    controlRotor2Rotors = value;
                    OnPropertyChanged("ControlRotor2Rotors");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane("AnalyzeControlRotors3Caption", "AnalyzeRotorsTooltip",
           "Rotor Analysis", 6, false, ControlType.CheckBox, "", null)]
        public bool ControlRotor3Rotors
        {
            get { return controlRotor3Rotors; }
            set
            {
                if (value != controlRotor3Rotors)
                {
                    controlRotor3Rotors = value;
                    OnPropertyChanged("ControlRotor3Rotors");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane("AnalyzeControlRotors4Caption", "AnalyzeRotorsTooltip",
           "Rotor Analysis", 6, false, ControlType.CheckBox, "", null)]
        public bool ControlRotor4Rotors
        {
            get { return controlRotor4Rotors; }
            set
            {
                if (value != controlRotor4Rotors)
                {
                    controlRotor4Rotors = value;
                    OnPropertyChanged("ControlRotor4Rotors");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane("AnalyzeControlrotors5caption", "AnalyzeRotorsTooltip",
           "Rotor Analysis", 6, false, ControlType.CheckBox, "", null)]
        public bool ControlRotor5Rotors
        {
            get { return controlRotor5Rotors; }
            set
            {
                if (value != controlRotor5Rotors)
                {
                    controlRotor5Rotors = value;
                    OnPropertyChanged("ControlRotor5Rotors");
                    setSettingsVisibility();
                }
            }
        }

        #endregion

        #region on/off indexrotor

        [TaskPane("AnalyzeIndexRotors1Caption", "AnalyzeRotorsTooltip",
            "Rotor Analysis", 6, false, ControlType.CheckBox, "", null)]
        public bool IndexRotor1Rotors
        {
            get { return indexRotor1Rotors; }
            set
            {
                if (value != indexRotor1Rotors)
                {
                    indexRotor1Rotors = value;
                    OnPropertyChanged("IndexRotor1Rotors");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane("AnalyzeIndexrotors2caption", "AnalyzeRotorsTooltip",
           "Rotor Analysis", 6, false, ControlType.CheckBox, "", null)]
        public bool IndexRotor2Rotors
        {
            get { return indexRotor2Rotors; }
            set
            {
                if (value != indexRotor2Rotors)
                {
                    indexRotor2Rotors = value;
                    OnPropertyChanged("IndexRotor2Rotors");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane("AnalyzeIndexRotors3Caption", "AnalyzeRotorsTooltip",
           "Rotor Analysis", 6, false, ControlType.CheckBox, "", null)]
        public bool IndexRotor3Rotors
        {
            get { return indexRotor3Rotors; }
            set
            {
                if (value != indexRotor3Rotors)
                {
                    indexRotor3Rotors = value;
                    OnPropertyChanged("IndexRotor3Rotors");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane("AnalyzeIndexRotors4Caption", "AnalyzeRotorsTooltip",
           "Rotor Analysis", 6, false, ControlType.CheckBox, "", null)]
        public bool IndexRotor4Rotors
        {
            get { return indexRotor4Rotors; }
            set
            {
                if (value != indexRotor4Rotors)
                {
                    indexRotor4Rotors = value;
                    OnPropertyChanged("IndexRotor4Rotors");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane("AnalyzeIndexrotors5caption", "AnalyzeRotorsTooltip",
           "Rotor Analysis", 6, false, ControlType.CheckBox, "", null)]
        public bool IndexRotor5Rotors
        {
            get { return indexRotor5Rotors; }
            set
            {
                if (value != indexRotor5Rotors)
                {
                    indexRotor5Rotors = value;
                    OnPropertyChanged("IndexRotor5Rotors");
                    setSettingsVisibility();
                }
            }
        }

        #endregion


        #region Cipher Bank

        

        

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("To", "PlugBoardBTooltip", "PositionOptionsGroup", 41, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor1To
        {
            get { return cipherRotor1To; }
            set
            {
                
                    cipherRotor1To = value;
                    OnPropertyChanged("CipherRotor1To");
                    setSettingsVisibility();
                if (value < cipherRotor1From)
                {
                    CipherRotor1From = value;
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor1
        {
            get { return cipher1AnalysisUseRotor1; }
            set
            {
                if (value != cipher1AnalysisUseRotor1)
                {
                    cipher1AnalysisUseRotor1 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor1");
                }
            }
        }

        
        [TaskPane("AnalysisUseRotorIICaption", "AnalysisUseRotorIITooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor2
        {
            get { return cipher1AnalysisUseRotor2 ; }
            set
            {
                if (value != cipher1AnalysisUseRotor2 )
                {
                    cipher1AnalysisUseRotor2  = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor2");
                }
            }
        }

        
        [TaskPane("AnalysisUseRotorIIICaption", "AnalysisUseRotorIIITooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor3
        {
            get { return cipher1AnalysisUseRotor3; }
            set
            {
                if (value != cipher1AnalysisUseRotor3)
                {
                    cipher1AnalysisUseRotor3 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor3");
                }
            }
        }

        
        [TaskPane("AnalysisUseRotorIVCaption", "AnalysisUseRotorIVTooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor4
        {
            get { return cipher1AnalysisUseRotor4; }
            set
            {
                if (value != cipher1AnalysisUseRotor4)
                {
                    cipher1AnalysisUseRotor4 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor4");
                }
            }
        }


        
        [TaskPane("AnalysisUseRotorVCaption", "AnalysisUseRotorVTooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor5
        {
            get { return cipher1AnalysisUseRotor5; }
            set
            {
                if (value != cipher1AnalysisUseRotor5)
                {
                    cipher1AnalysisUseRotor5 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor5");
                }
            }
        }

        
        [TaskPane("AnalysisUseRotorVICaption", "AnalysisUseRotorVITooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor6
        {
            get { return cipher1AnalysisUseRotor6; }
            set
            {
                if (value != cipher1AnalysisUseRotor6)
                {
                    cipher1AnalysisUseRotor6 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor6");
                }
            }
        }
        
        [TaskPane("AnalysisUseRotorVIICaption", "AnalysisUseRotorVIITooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor7
        {
            get { return cipher1AnalysisUseRotor7; }
            set
            {
                if (value != cipher1AnalysisUseRotor7)
                {
                    cipher1AnalysisUseRotor7 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor7");
                }
            }
        }
        
        [TaskPane("AnalysisUseRotor8Caption", "AnalysisUseRotorVIIITooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor8
        {
            get { return cipher1AnalysisUseRotor8; }
            set
            {
                if (value != cipher1AnalysisUseRotor8)
                {
                    cipher1AnalysisUseRotor8 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor8");
                }
            }
        }

        [TaskPane("AnalysisUseRotor9Caption", "AnalysisUseRotorVIIITooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor9
        {
            get { return cipher1AnalysisUseRotor9; }
            set
            {
                if (value != cipher1AnalysisUseRotor9)
                {
                    cipher1AnalysisUseRotor9 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor9");
                }
            }
        }
        [TaskPane("AnalysisUseRotor10Caption", "AnalysisUseRotorVIIITooltip",
            "Cipher1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher1AnalysisUseRotor0
        {
            get { return cipher1AnalysisUseRotor0; }
            set
            {
                if (value != cipher1AnalysisUseRotor0)
                {
                    cipher1AnalysisUseRotor0 = value;
                    OnPropertyChanged("Cipher1AnalysisUseRotor0");
                }
            }
        }
        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("Cipher Rotor 2: From", "PlugBoardATooltip", "PositionOptionsGroup", 40, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor2From
        {
            get { return cipherRotor2From; }
            set
            {
                cipherRotor2From = value;
                OnPropertyChanged("CipherRotor2From");
                setSettingsVisibility();

                if (value > cipherRotor2To)
                {
                    CipherRotor2To = value;


                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("To", "PlugBoardBTooltip", "PositionOptionsGroup", 41, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor2To
        {
            get { return cipherRotor2To; }
            set
            {

                cipherRotor2To = value;
                OnPropertyChanged("CipherRotor2To");
                setSettingsVisibility();
                if (value < cipherRotor2From)
                {
                    CipherRotor2From = value;
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor1
        {
            get { return cipher2AnalysisUseRotor1; }
            set
            {
                if (value != cipher2AnalysisUseRotor1)
                {
                    cipher2AnalysisUseRotor1 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("Cipher2AnalysisUseRotorIICaption", "Cipher2AnalysisUseRotorIITooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor2
        {
            get { return cipher2AnalysisUseRotor2; }
            set
            {
                if (value != cipher2AnalysisUseRotor2)
                {
                    cipher2AnalysisUseRotor2 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("Cipher2AnalysisUseRotorIIICaption", "Cipher2AnalysisUseRotorIIITooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor3
        {
            get { return cipher2AnalysisUseRotor3; }
            set
            {
                if (value != cipher2AnalysisUseRotor3)
                {
                    cipher2AnalysisUseRotor3 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("Cipher2AnalysisUseRotorIVCaption", "Cipher2AnalysisUseRotorIVTooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor4
        {
            get { return cipher2AnalysisUseRotor4; }
            set
            {
                if (value != cipher2AnalysisUseRotor4)
                {
                    cipher2AnalysisUseRotor4 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("Cipher2AnalysisUseRotorVCaption", "Cipher2AnalysisUseRotorVTooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor5
        {
            get { return cipher2AnalysisUseRotor5; }
            set
            {
                if (value != cipher2AnalysisUseRotor5)
                {
                    cipher2AnalysisUseRotor5 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor5");
                }
            }
        }


        [TaskPane("Cipher2AnalysisUseRotorVICaption", "Cipher2AnalysisUseRotorVITooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor6
        {
            get { return cipher2AnalysisUseRotor6; }
            set
            {
                if (value != cipher2AnalysisUseRotor6)
                {
                    cipher2AnalysisUseRotor6 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor6");
                }
            }
        }



        [TaskPane("Cipher2AnalysisUseRotorVIICaption", "Cipher2AnalysisUseRotorVIITooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor7
        {
            get { return cipher2AnalysisUseRotor7; }
            set
            {
                if (value != cipher2AnalysisUseRotor7)
                {
                    cipher2AnalysisUseRotor7 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor7");
                }
            }
        }



        [TaskPane("Cipher2AnalysisUseRotor8Caption", "Cipher2AnalysisUseRotorVIIITooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor8
        {
            get { return cipher2AnalysisUseRotor8; }
            set
            {
                if (value != cipher2AnalysisUseRotor8)
                {
                    cipher2AnalysisUseRotor8 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor8");
                }
            }
        }

        [TaskPane("Cipher2AnalysisUseRotor9Caption", "Cipher2AnalysisUseRotorVIIITooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor9
        {
            get { return cipher2AnalysisUseRotor9; }
            set
            {
                if (value != cipher2AnalysisUseRotor9)
                {
                    cipher2AnalysisUseRotor9 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor9");
                }
            }
        }
        [TaskPane("Cipher2AnalysisUseRotor10Caption", "Cipher2AnalysisUseRotorVIIITooltip",
            "Cipher2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher2AnalysisUseRotor0
        {
            get { return cipher2AnalysisUseRotor0; }
            set
            {
                if (value != cipher2AnalysisUseRotor0)
                {
                    cipher2AnalysisUseRotor0 = value;
                    OnPropertyChanged("Cipher2AnalysisUseRotor0");
                }
            }
        }
        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane("Cipher Rotor 3: From", "PlugBoardATooltip", "PositionOptionsGroup", 40, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor3From
        {
            get { return cipherRotor3From; }
            set
            {
                cipherRotor3From = value;
                OnPropertyChanged("CipherRotor3From");
                setSettingsVisibility();

                if (value > cipherRotor3To)
                {
                    CipherRotor3To = value;


                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane("To", "PlugBoardBTooltip", "PositionOptionsGroup", 41, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor3To
        {
            get { return cipherRotor3To; }
            set
            {

                cipherRotor3To = value;
                OnPropertyChanged("CipherRotor3To");
                setSettingsVisibility();
                if (value < cipherRotor3From)
                {
                    CipherRotor3From = value;
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor1
        {
            get { return cipher3AnalysisUseRotor1; }
            set
            {
                if (value != cipher3AnalysisUseRotor1)
                {
                    cipher3AnalysisUseRotor1 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("Cipher3AnalysisUseRotorIICaption", "Cipher3AnalysisUseRotorIITooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor2
        {
            get { return cipher3AnalysisUseRotor2; }
            set
            {
                if (value != cipher3AnalysisUseRotor2)
                {
                    cipher3AnalysisUseRotor2 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("Cipher3AnalysisUseRotorIIICaption", "Cipher3AnalysisUseRotorIIITooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor3
        {
            get { return cipher3AnalysisUseRotor3; }
            set
            {
                if (value != cipher3AnalysisUseRotor3)
                {
                    cipher3AnalysisUseRotor3 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("Cipher3AnalysisUseRotorIVCaption", "Cipher3AnalysisUseRotorIVTooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor4
        {
            get { return cipher3AnalysisUseRotor4; }
            set
            {
                if (value != cipher3AnalysisUseRotor4)
                {
                    cipher3AnalysisUseRotor4 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("Cipher3AnalysisUseRotorVCaption", "Cipher3AnalysisUseRotorVTooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor5
        {
            get { return cipher3AnalysisUseRotor5; }
            set
            {
                if (value != cipher3AnalysisUseRotor5)
                {
                    cipher3AnalysisUseRotor5 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor5");
                }
            }
        }


        [TaskPane("Cipher3AnalysisUseRotorVICaption", "Cipher3AnalysisUseRotorVITooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor6
        {
            get { return cipher3AnalysisUseRotor6; }
            set
            {
                if (value != cipher3AnalysisUseRotor6)
                {
                    cipher3AnalysisUseRotor6 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor6");
                }
            }
        }



        [TaskPane("Cipher3AnalysisUseRotorVIICaption", "Cipher3AnalysisUseRotorVIITooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor7
        {
            get { return cipher3AnalysisUseRotor7; }
            set
            {
                if (value != cipher3AnalysisUseRotor7)
                {
                    cipher3AnalysisUseRotor7 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor7");
                }
            }
        }



        [TaskPane("Cipher3AnalysisUseRotor8Caption", "Cipher3AnalysisUseRotorVIIITooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor8
        {
            get { return cipher3AnalysisUseRotor8; }
            set
            {
                if (value != cipher3AnalysisUseRotor8)
                {
                    cipher3AnalysisUseRotor8 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor8");
                }
            }
        }

        [TaskPane("Cipher3AnalysisUseRotor9Caption", "Cipher3AnalysisUseRotorVIIITooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor9
        {
            get { return cipher3AnalysisUseRotor9; }
            set
            {
                if (value != cipher3AnalysisUseRotor9)
                {
                    cipher3AnalysisUseRotor9 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor9");
                }
            }
        }
        [TaskPane("Cipher3AnalysisUseRotor10Caption", "Cipher3AnalysisUseRotorVIIITooltip",
            "Cipher3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher3AnalysisUseRotor0
        {
            get { return cipher3AnalysisUseRotor0; }
            set
            {
                if (value != cipher3AnalysisUseRotor0)
                {
                    cipher3AnalysisUseRotor0 = value;
                    OnPropertyChanged("Cipher3AnalysisUseRotor0");
                }
            }
        }
        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane("Cipher Rotor 4: From", "PlugBoardATooltip", "PositionOptionsGroup", 40, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor4From
        {
            get { return cipherRotor4From; }
            set
            {
                cipherRotor4From = value;
                OnPropertyChanged("CipherRotor4From");
                setSettingsVisibility();

                if (value > cipherRotor4To)
                {
                    CipherRotor4To = value;


                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane("To", "PlugBoardBTooltip", "PositionOptionsGroup", 41, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor4To
        {
            get { return cipherRotor4To; }
            set
            {

                cipherRotor4To = value;
                OnPropertyChanged("CipherRotor4To");
                setSettingsVisibility();
                if (value < cipherRotor4From)
                {
                    CipherRotor4From = value;
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor1
        {
            get { return cipher4AnalysisUseRotor1; }
            set
            {
                if (value != cipher4AnalysisUseRotor1)
                {
                    cipher4AnalysisUseRotor1 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("Cipher4AnalysisUseRotorIICaption", "Cipher4AnalysisUseRotorIITooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor2
        {
            get { return cipher4AnalysisUseRotor2; }
            set
            {
                if (value != cipher4AnalysisUseRotor2)
                {
                    cipher4AnalysisUseRotor2 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("Cipher4AnalysisUseRotorIIICaption", "Cipher4AnalysisUseRotorIIITooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor3
        {
            get { return cipher4AnalysisUseRotor3; }
            set
            {
                if (value != cipher4AnalysisUseRotor3)
                {
                    cipher4AnalysisUseRotor3 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("Cipher4AnalysisUseRotorIVCaption", "Cipher4AnalysisUseRotorIVTooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor4
        {
            get { return cipher4AnalysisUseRotor4; }
            set
            {
                if (value != cipher4AnalysisUseRotor4)
                {
                    cipher4AnalysisUseRotor4 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("Cipher4AnalysisUseRotorVCaption", "Cipher4AnalysisUseRotorVTooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor5
        {
            get { return cipher4AnalysisUseRotor5; }
            set
            {
                if (value != cipher4AnalysisUseRotor5)
                {
                    cipher4AnalysisUseRotor5 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor5");
                }
            }
        }


        [TaskPane("Cipher4AnalysisUseRotorVICaption", "Cipher4AnalysisUseRotorVITooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor6
        {
            get { return cipher4AnalysisUseRotor6; }
            set
            {
                if (value != cipher4AnalysisUseRotor6)
                {
                    cipher4AnalysisUseRotor6 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor6");
                }
            }
        }



        [TaskPane("Cipher4AnalysisUseRotorVIICaption", "Cipher4AnalysisUseRotorVIITooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor7
        {
            get { return cipher4AnalysisUseRotor7; }
            set
            {
                if (value != cipher4AnalysisUseRotor7)
                {
                    cipher4AnalysisUseRotor7 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor7");
                }
            }
        }



        [TaskPane("Cipher4AnalysisUseRotor8Caption", "Cipher4AnalysisUseRotorVIIITooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor8
        {
            get { return cipher4AnalysisUseRotor8; }
            set
            {
                if (value != cipher4AnalysisUseRotor8)
                {
                    cipher4AnalysisUseRotor8 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor8");
                }
            }
        }

        [TaskPane("Cipher4AnalysisUseRotor9Caption", "Cipher4AnalysisUseRotorVIIITooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor9
        {
            get { return cipher4AnalysisUseRotor9; }
            set
            {
                if (value != cipher4AnalysisUseRotor9)
                {
                    cipher4AnalysisUseRotor9 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor9");
                }
            }
        }
        [TaskPane("Cipher4AnalysisUseRotor10Caption", "Cipher4AnalysisUseRotorVIIITooltip",
            "Cipher4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher4AnalysisUseRotor0
        {
            get { return cipher4AnalysisUseRotor0; }
            set
            {
                if (value != cipher4AnalysisUseRotor0)
                {
                    cipher4AnalysisUseRotor0 = value;
                    OnPropertyChanged("Cipher4AnalysisUseRotor0");
                }
            }
        }
        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fünf")]
        [TaskPane("Cipher Rotor 5: From", "PlugBoardATooltip", "PositionOptionsGroup", 40, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor5From
        {
            get { return cipherRotor5From; }
            set
            {
                cipherRotor5From = value;
                OnPropertyChanged("CipherRotor5From");
                setSettingsVisibility();

                if (value > cipherRotor5To)
                {
                    CipherRotor5To = value;


                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fünf")]
        [TaskPane("To", "PlugBoardBTooltip", "PositionOptionsGroup", 41, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int CipherRotor5To
        {
            get { return cipherRotor5To; }
            set
            {

                cipherRotor5To = value;
                OnPropertyChanged("CipherRotor5To");
                setSettingsVisibility();
                if (value < cipherRotor5From)
                {
                    CipherRotor5From = value;
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor1
        {
            get { return cipher5AnalysisUseRotor1; }
            set
            {
                if (value != cipher5AnalysisUseRotor1)
                {
                    cipher5AnalysisUseRotor1 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("Cipher5AnalysisUseRotorIICaption", "Cipher5AnalysisUseRotorIITooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor2
        {
            get { return cipher5AnalysisUseRotor2; }
            set
            {
                if (value != cipher5AnalysisUseRotor2)
                {
                    cipher5AnalysisUseRotor2 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("Cipher5AnalysisUseRotorIIICaption", "Cipher5AnalysisUseRotorIIITooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor3
        {
            get { return cipher5AnalysisUseRotor3; }
            set
            {
                if (value != cipher5AnalysisUseRotor3)
                {
                    cipher5AnalysisUseRotor3 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("Cipher5AnalysisUseRotorIVCaption", "Cipher5AnalysisUseRotorIVTooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor4
        {
            get { return cipher5AnalysisUseRotor4; }
            set
            {
                if (value != cipher5AnalysisUseRotor4)
                {
                    cipher5AnalysisUseRotor4 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("Cipher5AnalysisUseRotorVCaption", "Cipher5AnalysisUseRotorVTooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor5
        {
            get { return cipher5AnalysisUseRotor5; }
            set
            {
                if (value != cipher5AnalysisUseRotor5)
                {
                    cipher5AnalysisUseRotor5 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor5");
                }
            }
        }


        [TaskPane("Cipher5AnalysisUseRotorVICaption", "Cipher5AnalysisUseRotorVITooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor6
        {
            get { return cipher5AnalysisUseRotor6; }
            set
            {
                if (value != cipher5AnalysisUseRotor6)
                {
                    cipher5AnalysisUseRotor6 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor6");
                }
            }
        }



        [TaskPane("Cipher5AnalysisUseRotorVIICaption", "Cipher5AnalysisUseRotorVIITooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor7
        {
            get { return cipher5AnalysisUseRotor7; }
            set
            {
                if (value != cipher5AnalysisUseRotor7)
                {
                    cipher5AnalysisUseRotor7 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor7");
                }
            }
        }



        [TaskPane("Cipher5AnalysisUseRotor8Caption", "Cipher5AnalysisUseRotorVIIITooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor8
        {
            get { return cipher5AnalysisUseRotor8; }
            set
            {
                if (value != cipher5AnalysisUseRotor8)
                {
                    cipher5AnalysisUseRotor8 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor8");
                }
            }
        }

        [TaskPane("Cipher5AnalysisUseRotor9Caption", "Cipher5AnalysisUseRotorVIIITooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor9
        {
            get { return cipher5AnalysisUseRotor9; }
            set
            {
                if (value != cipher5AnalysisUseRotor9)
                {
                    cipher5AnalysisUseRotor9 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor9");
                }
            }
        }
        [TaskPane("Cipher5AnalysisUseRotor10Caption", "Cipher5AnalysisUseRotorVIIITooltip",
            "Cipher5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Cipher5AnalysisUseRotor0
        {
            get { return cipher5AnalysisUseRotor0; }
            set
            {
                if (value != cipher5AnalysisUseRotor0)
                {
                    cipher5AnalysisUseRotor0 = value;
                    OnPropertyChanged("Cipher5AnalysisUseRotor0");
                }
            }
        }
        #endregion

        #endregion

        #region Control Bank


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane("Control Rotor 1: From", "PlugBoardATooltip", "PositionOptionsGroup", 40, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int ControlRotor1From
        {
            get { return controlRotor1From; }
            set
            {

                controlRotor1From = value;
                OnPropertyChanged("ControlRotor1From");
                setSettingsVisibility();

                if (value > controlRotor1To)
                {
                    ControlRotor1To = value;


                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane("To", "PlugBoardBTooltip", "PositionOptionsGroup", 41, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int ControlRotor1To
        {
            get { return controlRotor1To; }
            set
            {

                controlRotor1To = value;
                OnPropertyChanged("ControlRotor1To");
                setSettingsVisibility();
                if (value < controlRotor1From)
                {
                    ControlRotor1From = value;
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Control1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor1
        {
            get { return control1AnalysisUseRotor1; }
            set
            {
                if (value != control1AnalysisUseRotor1)
                {
                    control1AnalysisUseRotor1 = value;
                    OnPropertyChanged("Control1AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIICaption", "AnalysisUseRotorIITooltip",
            "Control1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor2
        {
            get { return control1AnalysisUseRotor2; }
            set
            {
                if (value != control1AnalysisUseRotor2)
                {
                    control1AnalysisUseRotor2 = value;
                    OnPropertyChanged("Control1AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIIICaption", "AnalysisUseRotorIIITooltip",
            "Control1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor3
        {
            get { return control1AnalysisUseRotor3; }
            set
            {
                if (value != control1AnalysisUseRotor3)
                {
                    control1AnalysisUseRotor3 = value;
                    OnPropertyChanged("Control1AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIVCaption", "AnalysisUseRotorIVTooltip",
            "Control1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor4
        {
            get { return control1AnalysisUseRotor4; }
            set
            {
                if (value != control1AnalysisUseRotor4)
                {
                    control1AnalysisUseRotor4 = value;
                    OnPropertyChanged("Control1AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("AnalysisUseRotorVCaption", "AnalysisUseRotorVTooltip",
            "Control1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor5
        {
            get { return control1AnalysisUseRotor5; }
            set
            {
                if (value != control1AnalysisUseRotor5)
                {
                    control1AnalysisUseRotor5 = value;
                    OnPropertyChanged("Control1AnalysisUseRotor5");
                }
            }
        }


        [TaskPane("AnalysisUseRotorVICaption", "AnalysisUseRotorVITooltip",
            "Control1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor6
        {
            get { return control1AnalysisUseRotor6; }
            set
            {
                if (value != control1AnalysisUseRotor6)
                {
                    control1AnalysisUseRotor6 = value;
                    OnPropertyChanged("Control1AnalysisUseRotor6");
                }
            }
        }



        [TaskPane("AnalysisUseRotorVIICaption", "AnalysisUseRotorVIITooltip",
            "Control1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor7
        {
            get { return control1AnalysisUseRotor7; }
            set
            {
                if (value != control1AnalysisUseRotor7)
                {
                    control1AnalysisUseRotor7 = value;
                    OnPropertyChanged("Control1AnalysisUseRotor7");
                }
            }
        }



        [TaskPane("AnalysisUseRotor8Caption", "AnalysisUseRotorVIIITooltip",
            "Control1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor8
        {
            get { return control1AnalysisUseRotor8; }
            set
            {
                if (value != control1AnalysisUseRotor8)
                {
                    control1AnalysisUseRotor8 = value;
                    OnPropertyChanged("Control1AnalysisUseRotor8");
                }
            }
        }

        [TaskPane("AnalysisUseRotor9Caption", "AnalysisUseRotorVIIITooltip",
            "Control1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor9
        {
            get { return control1AnalysisUseRotor9; }
            set
            {
                if (value != control1AnalysisUseRotor9)
                {
                    control1AnalysisUseRotor9 = value;
                    OnPropertyChanged("Control1AnalysisUseRotor9");
                }
            }
        }
        [TaskPane("AnalysisUseRotor10Caption", "AnalysisUseRotorVIIITooltip",
            "Control1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control1AnalysisUseRotor0
        {
            get { return control1AnalysisUseRotor0; }
            set
            {
                if (value != control1AnalysisUseRotor0)
                {
                    control1AnalysisUseRotor0 = value;
                    OnPropertyChanged("Control1AnalysisUseRotor0");
                }
            }
        }
        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane("Control Rotor 2: From", "PlugBoardATooltip", "PositionOptionsGroup", 40, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int ControlRotor2From
        {
            get { return controlRotor2From; }
            set
            {
                controlRotor2From = value;
                OnPropertyChanged("ControlRotor2From");
                setSettingsVisibility();

                if (value > controlRotor2To)
                {
                    ControlRotor2To = value;


                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane("To", "PlugBoardBTooltip", "PositionOptionsGroup", 41, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int ControlRotor2To
        {
            get { return controlRotor2To; }
            set
            {

                controlRotor2To = value;
                OnPropertyChanged("ControlRotor2To");
                setSettingsVisibility();
                if (value < controlRotor2From)
                {
                    ControlRotor2From = value;
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Control2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor1
        {
            get { return control2AnalysisUseRotor1; }
            set
            {
                if (value != control2AnalysisUseRotor1)
                {
                    control2AnalysisUseRotor1 = value;
                    OnPropertyChanged("Control2AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("Control2AnalysisUseRotorIICaption", "Control2AnalysisUseRotorIITooltip",
            "Control2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor2
        {
            get { return control2AnalysisUseRotor2; }
            set
            {
                if (value != control2AnalysisUseRotor2)
                {
                    control2AnalysisUseRotor2 = value;
                    OnPropertyChanged("Control2AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("Control2AnalysisUseRotorIIICaption", "Control2AnalysisUseRotorIIITooltip",
            "Control2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor3
        {
            get { return control2AnalysisUseRotor3; }
            set
            {
                if (value != control2AnalysisUseRotor3)
                {
                    control2AnalysisUseRotor3 = value;
                    OnPropertyChanged("Control2AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("Control2AnalysisUseRotorIVCaption", "Control2AnalysisUseRotorIVTooltip",
            "Control2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor4
        {
            get { return control2AnalysisUseRotor4; }
            set
            {
                if (value != control2AnalysisUseRotor4)
                {
                    control2AnalysisUseRotor4 = value;
                    OnPropertyChanged("Control2AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("Control2AnalysisUseRotorVCaption", "Control2AnalysisUseRotorVTooltip",
            "Control2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor5
        {
            get { return control2AnalysisUseRotor5; }
            set
            {
                if (value != control2AnalysisUseRotor5)
                {
                    control2AnalysisUseRotor5 = value;
                    OnPropertyChanged("Control2AnalysisUseRotor5");
                }
            }
        }


        [TaskPane("Control2AnalysisUseRotorVICaption", "Control2AnalysisUseRotorVITooltip",
            "Control2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor6
        {
            get { return control2AnalysisUseRotor6; }
            set
            {
                if (value != control2AnalysisUseRotor6)
                {
                    control2AnalysisUseRotor6 = value;
                    OnPropertyChanged("Control2AnalysisUseRotor6");
                }
            }
        }



        [TaskPane("Control2AnalysisUseRotorVIICaption", "Control2AnalysisUseRotorVIITooltip",
            "Control2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor7
        {
            get { return control2AnalysisUseRotor7; }
            set
            {
                if (value != control2AnalysisUseRotor7)
                {
                    control2AnalysisUseRotor7 = value;
                    OnPropertyChanged("Control2AnalysisUseRotor7");
                }
            }
        }



        [TaskPane("Control2AnalysisUseRotor8Caption", "Control2AnalysisUseRotorVIIITooltip",
            "Control2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor8
        {
            get { return control2AnalysisUseRotor8; }
            set
            {
                if (value != control2AnalysisUseRotor8)
                {
                    control2AnalysisUseRotor8 = value;
                    OnPropertyChanged("Control2AnalysisUseRotor8");
                }
            }
        }

        [TaskPane("Control2AnalysisUseRotor9Caption", "Control2AnalysisUseRotorVIIITooltip",
            "Control2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor9
        {
            get { return control2AnalysisUseRotor9; }
            set
            {
                if (value != control2AnalysisUseRotor9)
                {
                    control2AnalysisUseRotor9 = value;
                    OnPropertyChanged("Control2AnalysisUseRotor9");
                }
            }
        }
        [TaskPane("Control2AnalysisUseRotor10Caption", "Control2AnalysisUseRotorVIIITooltip",
            "Control2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control2AnalysisUseRotor0
        {
            get { return control2AnalysisUseRotor0; }
            set
            {
                if (value != control2AnalysisUseRotor0)
                {
                    control2AnalysisUseRotor0 = value;
                    OnPropertyChanged("Control2AnalysisUseRotor0");
                }
            }
        }
        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane("Control Rotor 3: From", "PlugBoardATooltip", "PositionOptionsGroup", 40, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int ControlRotor3From
        {
            get { return controlRotor3From; }
            set
            {
                controlRotor3From = value;
                OnPropertyChanged("ControlRotor3From");
                setSettingsVisibility();

                if (value > controlRotor3To)
                {
                    ControlRotor3To = value;


                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane("To", "PlugBoardBTooltip", "PositionOptionsGroup", 41, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int ControlRotor3To
        {
            get { return controlRotor3To; }
            set
            {

                controlRotor3To = value;
                OnPropertyChanged("ControlRotor3To");
                setSettingsVisibility();
                if (value < controlRotor3From)
                {
                    ControlRotor3From = value;
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Control3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor1
        {
            get { return control3AnalysisUseRotor1; }
            set
            {
                if (value != control3AnalysisUseRotor1)
                {
                    control3AnalysisUseRotor1 = value;
                    OnPropertyChanged("Control3AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("Control3AnalysisUseRotorIICaption", "Control3AnalysisUseRotorIITooltip",
            "Control3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor2
        {
            get { return control3AnalysisUseRotor2; }
            set
            {
                if (value != control3AnalysisUseRotor2)
                {
                    control3AnalysisUseRotor2 = value;
                    OnPropertyChanged("Control3AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("Control3AnalysisUseRotorIIICaption", "Control3AnalysisUseRotorIIITooltip",
            "Control3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor3
        {
            get { return control3AnalysisUseRotor3; }
            set
            {
                if (value != control3AnalysisUseRotor3)
                {
                    control3AnalysisUseRotor3 = value;
                    OnPropertyChanged("Control3AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("Control3AnalysisUseRotorIVCaption", "Control3AnalysisUseRotorIVTooltip",
            "Control3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor4
        {
            get { return control3AnalysisUseRotor4; }
            set
            {
                if (value != control3AnalysisUseRotor4)
                {
                    control3AnalysisUseRotor4 = value;
                    OnPropertyChanged("Control3AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("Control3AnalysisUseRotorVCaption", "Control3AnalysisUseRotorVTooltip",
            "Control3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor5
        {
            get { return control3AnalysisUseRotor5; }
            set
            {
                if (value != control3AnalysisUseRotor5)
                {
                    control3AnalysisUseRotor5 = value;
                    OnPropertyChanged("Control3AnalysisUseRotor5");
                }
            }
        }


        [TaskPane("Control3AnalysisUseRotorVICaption", "Control3AnalysisUseRotorVITooltip",
            "Control3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor6
        {
            get { return control3AnalysisUseRotor6; }
            set
            {
                if (value != control3AnalysisUseRotor6)
                {
                    control3AnalysisUseRotor6 = value;
                    OnPropertyChanged("Control3AnalysisUseRotor6");
                }
            }
        }



        [TaskPane("Control3AnalysisUseRotorVIICaption", "Control3AnalysisUseRotorVIITooltip",
            "Control3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor7
        {
            get { return control3AnalysisUseRotor7; }
            set
            {
                if (value != control3AnalysisUseRotor7)
                {
                    control3AnalysisUseRotor7 = value;
                    OnPropertyChanged("Control3AnalysisUseRotor7");
                }
            }
        }



        [TaskPane("Control3AnalysisUseRotor8Caption", "Control3AnalysisUseRotorVIIITooltip",
            "Control3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor8
        {
            get { return control3AnalysisUseRotor8; }
            set
            {
                if (value != control3AnalysisUseRotor8)
                {
                    control3AnalysisUseRotor8 = value;
                    OnPropertyChanged("Control3AnalysisUseRotor8");
                }
            }
        }

        [TaskPane("Control3AnalysisUseRotor9Caption", "Control3AnalysisUseRotorVIIITooltip",
            "Control3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor9
        {
            get { return control3AnalysisUseRotor9; }
            set
            {
                if (value != control3AnalysisUseRotor9)
                {
                    control3AnalysisUseRotor9 = value;
                    OnPropertyChanged("Control3AnalysisUseRotor9");
                }
            }
        }
        [TaskPane("Control3AnalysisUseRotor10Caption", "Control3AnalysisUseRotorVIIITooltip",
            "Control3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control3AnalysisUseRotor0
        {
            get { return control3AnalysisUseRotor0; }
            set
            {
                if (value != control3AnalysisUseRotor0)
                {
                    control3AnalysisUseRotor0 = value;
                    OnPropertyChanged("Control3AnalysisUseRotor0");
                }
            }
        }
        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Neun")]
        [TaskPane("Control Rotor 4: From", "PlugBoardATooltip", "PositionOptionsGroup", 40, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int ControlRotor4From
        {
            get { return controlRotor4From; }
            set
            {
                controlRotor4From = value;
                OnPropertyChanged("ControlRotor4From");
                setSettingsVisibility();

                if (value > controlRotor4To)
                {
                    ControlRotor4To = value;


                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Neun")]
        [TaskPane("To", "PlugBoardBTooltip", "PositionOptionsGroup", 41, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int ControlRotor4To
        {
            get { return controlRotor4To; }
            set
            {

                controlRotor4To = value;
                OnPropertyChanged("ControlRotor4To");
                setSettingsVisibility();
                if (value < controlRotor4From)
                {
                    ControlRotor4From = value;
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Control4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor1
        {
            get { return control4AnalysisUseRotor1; }
            set
            {
                if (value != control4AnalysisUseRotor1)
                {
                    control4AnalysisUseRotor1 = value;
                    OnPropertyChanged("Control4AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("Control4AnalysisUseRotorIICaption", "Control4AnalysisUseRotorIITooltip",
            "Control4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor2
        {
            get { return control4AnalysisUseRotor2; }
            set
            {
                if (value != control4AnalysisUseRotor2)
                {
                    control4AnalysisUseRotor2 = value;
                    OnPropertyChanged("Control4AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("Control4AnalysisUseRotorIIICaption", "Control4AnalysisUseRotorIIITooltip",
            "Control4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor3
        {
            get { return control4AnalysisUseRotor3; }
            set
            {
                if (value != control4AnalysisUseRotor3)
                {
                    control4AnalysisUseRotor3 = value;
                    OnPropertyChanged("Control4AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("Control4AnalysisUseRotorIVCaption", "Control4AnalysisUseRotorIVTooltip",
            "Control4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor4
        {
            get { return control4AnalysisUseRotor4; }
            set
            {
                if (value != control4AnalysisUseRotor4)
                {
                    control4AnalysisUseRotor4 = value;
                    OnPropertyChanged("Control4AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("Control4AnalysisUseRotorVCaption", "Control4AnalysisUseRotorVTooltip",
            "Control4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor5
        {
            get { return control4AnalysisUseRotor5; }
            set
            {
                if (value != control4AnalysisUseRotor5)
                {
                    control4AnalysisUseRotor5 = value;
                    OnPropertyChanged("Control4AnalysisUseRotor5");
                }
            }
        }


        [TaskPane("Control4AnalysisUseRotorVICaption", "Control4AnalysisUseRotorVITooltip",
            "Control4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor6
        {
            get { return control4AnalysisUseRotor6; }
            set
            {
                if (value != control4AnalysisUseRotor6)
                {
                    control4AnalysisUseRotor6 = value;
                    OnPropertyChanged("Control4AnalysisUseRotor6");
                }
            }
        }



        [TaskPane("Control4AnalysisUseRotorVIICaption", "Control4AnalysisUseRotorVIITooltip",
            "Control4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor7
        {
            get { return control4AnalysisUseRotor7; }
            set
            {
                if (value != control4AnalysisUseRotor7)
                {
                    control4AnalysisUseRotor7 = value;
                    OnPropertyChanged("Control4AnalysisUseRotor7");
                }
            }
        }



        [TaskPane("Control4AnalysisUseRotor8Caption", "Control4AnalysisUseRotorVIIITooltip",
            "Control4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor8
        {
            get { return control4AnalysisUseRotor8; }
            set
            {
                if (value != control4AnalysisUseRotor8)
                {
                    control4AnalysisUseRotor8 = value;
                    OnPropertyChanged("Control4AnalysisUseRotor8");
                }
            }
        }

        [TaskPane("Control4AnalysisUseRotor9Caption", "Control4AnalysisUseRotorVIIITooltip",
            "Control4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor9
        {
            get { return control4AnalysisUseRotor9; }
            set
            {
                if (value != control4AnalysisUseRotor9)
                {
                    control4AnalysisUseRotor9 = value;
                    OnPropertyChanged("Control4AnalysisUseRotor9");
                }
            }
        }
        [TaskPane("Control4AnalysisUseRotor10Caption", "Control4AnalysisUseRotorVIIITooltip",
            "Control4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control4AnalysisUseRotor0
        {
            get { return control4AnalysisUseRotor0; }
            set
            {
                if (value != control4AnalysisUseRotor0)
                {
                    control4AnalysisUseRotor0 = value;
                    OnPropertyChanged("Control4AnalysisUseRotor0");
                }
            }
        }
        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zehn")]
        [TaskPane("Control Rotor 5: From", "PlugBoardATooltip", "PositionOptionsGroup", 40, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int ControlRotor5From
        {
            get { return controlRotor5From; }
            set
            {
                controlRotor5From = value;
                OnPropertyChanged("ControlRotor5From");
                setSettingsVisibility();

                if (value > controlRotor5To)
                {
                    ControlRotor5To = value;


                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zehn")]
        [TaskPane("To", "PlugBoardBTooltip", "PositionOptionsGroup", 41, false, ControlType.ComboBox,
            new String[] { "LetterA", "LetterB", "LetterC", "LetterD", "LetterE", "LetterF", "LetterG", "LetterH", "LetterI", "LetterJ", "LetterK", "LetterL", "LetterM", "LetterN", "LetterO", "LetterP", "LetterQ", "LetterR", "LetterS", "LetterT", "LetterU", "LetterV", "LetterW", "LetterX", "LetterY", "LetterZ" })]
        public int ControlRotor5To
        {
            get { return controlRotor5To; }
            set
            {

                controlRotor5To = value;
                OnPropertyChanged("ControlRotor5To");
                setSettingsVisibility();
                if (value < controlRotor5From)
                {
                    ControlRotor5From = value;
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Control5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor1
        {
            get { return control5AnalysisUseRotor1; }
            set
            {
                if (value != control5AnalysisUseRotor1)
                {
                    control5AnalysisUseRotor1 = value;
                    OnPropertyChanged("Control5AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("Control5AnalysisUseRotorIICaption", "Control5AnalysisUseRotorIITooltip",
            "Control5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor2
        {
            get { return control5AnalysisUseRotor2; }
            set
            {
                if (value != control5AnalysisUseRotor2)
                {
                    control5AnalysisUseRotor2 = value;
                    OnPropertyChanged("Control5AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("Control5AnalysisUseRotorIIICaption", "Control5AnalysisUseRotorIIITooltip",
            "Control5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor3
        {
            get { return control5AnalysisUseRotor3; }
            set
            {
                if (value != control5AnalysisUseRotor3)
                {
                    control5AnalysisUseRotor3 = value;
                    OnPropertyChanged("Control5AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("Control5AnalysisUseRotorIVCaption", "Control5AnalysisUseRotorIVTooltip",
            "Control5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor4
        {
            get { return control5AnalysisUseRotor4; }
            set
            {
                if (value != control5AnalysisUseRotor4)
                {
                    control5AnalysisUseRotor4 = value;
                    OnPropertyChanged("Control5AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("Control5AnalysisUseRotorVCaption", "Control5AnalysisUseRotorVTooltip",
            "Control5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor5
        {
            get { return control5AnalysisUseRotor5; }
            set
            {
                if (value != control5AnalysisUseRotor5)
                {
                    control5AnalysisUseRotor5 = value;
                    OnPropertyChanged("Control5AnalysisUseRotor5");
                }
            }
        }


        [TaskPane("Control5AnalysisUseRotorVICaption", "Control5AnalysisUseRotorVITooltip",
            "Control5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor6
        {
            get { return control5AnalysisUseRotor6; }
            set
            {
                if (value != control5AnalysisUseRotor6)
                {
                    control5AnalysisUseRotor6 = value;
                    OnPropertyChanged("Control5AnalysisUseRotor6");
                }
            }
        }



        [TaskPane("Control5AnalysisUseRotorVIICaption", "Control5AnalysisUseRotorVIITooltip",
            "Control5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor7
        {
            get { return control5AnalysisUseRotor7; }
            set
            {
                if (value != control5AnalysisUseRotor7)
                {
                    control5AnalysisUseRotor7 = value;
                    OnPropertyChanged("Control5AnalysisUseRotor7");
                }
            }
        }



        [TaskPane("Control5AnalysisUseRotor8Caption", "Control5AnalysisUseRotorVIIITooltip",
            "Control5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor8
        {
            get { return control5AnalysisUseRotor8; }
            set
            {
                if (value != control5AnalysisUseRotor8)
                {
                    control5AnalysisUseRotor8 = value;
                    OnPropertyChanged("Control5AnalysisUseRotor8");
                }
            }
        }

        [TaskPane("Control5AnalysisUseRotor9Caption", "Control5AnalysisUseRotorVIIITooltip",
            "Control5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor9
        {
            get { return control5AnalysisUseRotor9; }
            set
            {
                if (value != control5AnalysisUseRotor9)
                {
                    control5AnalysisUseRotor9 = value;
                    OnPropertyChanged("Control5AnalysisUseRotor9");
                }
            }
        }
        [TaskPane("Control5AnalysisUseRotor10Caption", "Control5AnalysisUseRotorVIIITooltip",
            "Control5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Control5AnalysisUseRotor0
        {
            get { return control5AnalysisUseRotor0; }
            set
            {
                if (value != control5AnalysisUseRotor0)
                {
                    control5AnalysisUseRotor0 = value;
                    OnPropertyChanged("Control5AnalysisUseRotor0");
                }
            }
        }
        #endregion

        
        #endregion

        #region Index Bank

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Elf")]
        [TaskPane("Index Rotor 1: From", "PlugBoardATooltip", "PositionOptionsGroup", 40, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 9)]
        public int IndexRotor1From
        {
            get { return indexRotor1From; }
            set
            {

                indexRotor1From = value;
                OnPropertyChanged("IndexRotor1From");
                setSettingsVisibility();

                if (value > indexRotor1To)
                {
                    IndexRotor1To = value;
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Elf")]
        [TaskPane("To", "PlugBoardBTooltip", "PositionOptionsGroup", 41, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 9)]
        public int IndexRotor1To
        {
            get { return indexRotor1To; }
            set
            {

                indexRotor1To = value;
                OnPropertyChanged("IndexRotor1To");
                setSettingsVisibility();
                if (value < indexRotor1From)
                {
                    IndexRotor1From = value;
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Index1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index1AnalysisUseRotor1
        {
            get { return index1AnalysisUseRotor1; }
            set
            {
                if (value != index1AnalysisUseRotor1)
                {
                    index1AnalysisUseRotor1 = value;
                    OnPropertyChanged("Index1AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIICaption", "AnalysisUseRotorIITooltip",
            "Index1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index1AnalysisUseRotor2
        {
            get { return index1AnalysisUseRotor2; }
            set
            {
                if (value != index1AnalysisUseRotor2)
                {
                    index1AnalysisUseRotor2 = value;
                    OnPropertyChanged("Index1AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIIICaption", "AnalysisUseRotorIIITooltip",
            "Index1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index1AnalysisUseRotor3
        {
            get { return index1AnalysisUseRotor3; }
            set
            {
                if (value != index1AnalysisUseRotor3)
                {
                    index1AnalysisUseRotor3 = value;
                    OnPropertyChanged("Index1AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("AnalysisUseRotorIVCaption", "AnalysisUseRotorIVTooltip",
            "Index1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index1AnalysisUseRotor4
        {
            get { return index1AnalysisUseRotor4; }
            set
            {
                if (value != index1AnalysisUseRotor4)
                {
                    index1AnalysisUseRotor4 = value;
                    OnPropertyChanged("Index1AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("AnalysisUseRotorVCaption", "AnalysisUseRotorVTooltip",
            "Index1AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index1AnalysisUseRotor5
        {
            get { return index1AnalysisUseRotor5; }
            set
            {
                if (value != index1AnalysisUseRotor5)
                {
                    index1AnalysisUseRotor5 = value;
                    OnPropertyChanged("Index1AnalysisUseRotor5");
                }
            }
        }

        #endregion


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwölf")]
        [TaskPane("Index Rotor 2: From", "PlugBoardATooltip", "PositionOptionsGroup", 40, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 9)]
        public int IndexRotor2From
        {
            get { return indexRotor2From; }
            set
            {
                indexRotor2From = value;
                OnPropertyChanged("IndexRotor2From");
                setSettingsVisibility();

                if (value > indexRotor2To)
                {
                    IndexRotor2To = value;


                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwölf")]
        [TaskPane("To", "PlugBoardBTooltip", "PositionOptionsGroup", 41, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 9)]
        public int IndexRotor2To
        {
            get { return indexRotor2To; }
            set
            {

                indexRotor2To = value;
                OnPropertyChanged("IndexRotor2To");
                setSettingsVisibility();
                if (value < indexRotor2From)
                {
                    IndexRotor2From = value;
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Index2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index2AnalysisUseRotor1
        {
            get { return index2AnalysisUseRotor1; }
            set
            {
                if (value != index2AnalysisUseRotor1)
                {
                    index2AnalysisUseRotor1 = value;
                    OnPropertyChanged("Index2AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("Index2AnalysisUseRotorIICaption", "Index2AnalysisUseRotorIITooltip",
            "Index2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index2AnalysisUseRotor2
        {
            get { return index2AnalysisUseRotor2; }
            set
            {
                if (value != index2AnalysisUseRotor2)
                {
                    index2AnalysisUseRotor2 = value;
                    OnPropertyChanged("Index2AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("Index2AnalysisUseRotorIIICaption", "Index2AnalysisUseRotorIIITooltip",
            "Index2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index2AnalysisUseRotor3
        {
            get { return index2AnalysisUseRotor3; }
            set
            {
                if (value != index2AnalysisUseRotor3)
                {
                    index2AnalysisUseRotor3 = value;
                    OnPropertyChanged("Index2AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("Index2AnalysisUseRotorIVCaption", "Index2AnalysisUseRotorIVTooltip",
            "Index2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index2AnalysisUseRotor4
        {
            get { return index2AnalysisUseRotor4; }
            set
            {
                if (value != index2AnalysisUseRotor4)
                {
                    index2AnalysisUseRotor4 = value;
                    OnPropertyChanged("Index2AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("Index2AnalysisUseRotorVCaption", "Index2AnalysisUseRotorVTooltip",
            "Index2AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index2AnalysisUseRotor5
        {
            get { return index2AnalysisUseRotor5; }
            set
            {
                if (value != index2AnalysisUseRotor5)
                {
                    index2AnalysisUseRotor5 = value;
                    OnPropertyChanged("Index2AnalysisUseRotor5");
                }
            }
        }
        #endregion
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "DreiZehn")]
        [TaskPane("Index Rotor 3: From", "PlugBoardATooltip", "PositionOptionsGroup", 40, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 9)]
        public int IndexRotor3From
        {
            get { return indexRotor3From; }
            set
            {
                indexRotor3From = value;
                OnPropertyChanged("IndexRotor3From");
                setSettingsVisibility();

                if (value > indexRotor3To)
                {
                    IndexRotor3To = value;


                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "DreiZehn")]
        [TaskPane("To", "PlugBoardBTooltip", "PositionOptionsGroup", 41, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 9)]
        public int IndexRotor3To
        {
            get { return indexRotor3To; }
            set
            {

                indexRotor3To = value;
                OnPropertyChanged("IndexRotor3To");
                setSettingsVisibility();
                if (value < indexRotor3From)
                {
                    IndexRotor3From = value;
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Index3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index3AnalysisUseRotor1
        {
            get { return index3AnalysisUseRotor1; }
            set
            {
                if (value != index3AnalysisUseRotor1)
                {
                    index3AnalysisUseRotor1 = value;
                    OnPropertyChanged("Index3AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("Index3AnalysisUseRotorIICaption", "Index3AnalysisUseRotorIITooltip",
            "Index3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index3AnalysisUseRotor2
        {
            get { return index3AnalysisUseRotor2; }
            set
            {
                if (value != index3AnalysisUseRotor2)
                {
                    index3AnalysisUseRotor2 = value;
                    OnPropertyChanged("Index3AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("Index3AnalysisUseRotorIIICaption", "Index3AnalysisUseRotorIIITooltip",
            "Index3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index3AnalysisUseRotor3
        {
            get { return index3AnalysisUseRotor3; }
            set
            {
                if (value != index3AnalysisUseRotor3)
                {
                    index3AnalysisUseRotor3 = value;
                    OnPropertyChanged("Index3AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("Index3AnalysisUseRotorIVCaption", "Index3AnalysisUseRotorIVTooltip",
            "Index3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index3AnalysisUseRotor4
        {
            get { return index3AnalysisUseRotor4; }
            set
            {
                if (value != index3AnalysisUseRotor4)
                {
                    index3AnalysisUseRotor4 = value;
                    OnPropertyChanged("Index3AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("Index3AnalysisUseRotorVCaption", "Index3AnalysisUseRotorVTooltip",
            "Index3AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index3AnalysisUseRotor5
        {
            get { return index3AnalysisUseRotor5; }
            set
            {
                if (value != index3AnalysisUseRotor5)
                {
                    index3AnalysisUseRotor5 = value;
                    OnPropertyChanged("Index3AnalysisUseRotor5");
                }
            }
        }

        #endregion

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "VierZehn")]
        [TaskPane("Index Rotor 4: From", "PlugBoardATooltip", "PositionOptionsGroup", 40, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 9)]
        public int IndexRotor4From
        {
            get { return indexRotor4From; }
            set
            {
                indexRotor4From = value;
                OnPropertyChanged("IndexRotor4From");
                setSettingsVisibility();

                if (value > indexRotor4To)
                {
                    IndexRotor4To = value;


                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "VierZehn")]
        [TaskPane("To", "PlugBoardBTooltip", "PositionOptionsGroup", 41, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 9)]
        public int IndexRotor4To
        {
            get { return indexRotor4To; }
            set
            {

                indexRotor4To = value;
                OnPropertyChanged("IndexRotor4To");
                setSettingsVisibility();
                if (value < indexRotor4From)
                {
                    IndexRotor4From = value;
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Index4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index4AnalysisUseRotor1
        {
            get { return index4AnalysisUseRotor1; }
            set
            {
                if (value != index4AnalysisUseRotor1)
                {
                    index4AnalysisUseRotor1 = value;
                    OnPropertyChanged("Index4AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("Index4AnalysisUseRotorIICaption", "Index4AnalysisUseRotorIITooltip",
            "Index4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index4AnalysisUseRotor2
        {
            get { return index4AnalysisUseRotor2; }
            set
            {
                if (value != index4AnalysisUseRotor2)
                {
                    index4AnalysisUseRotor2 = value;
                    OnPropertyChanged("Index4AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("Index4AnalysisUseRotorIIICaption", "Index4AnalysisUseRotorIIITooltip",
            "Index4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index4AnalysisUseRotor3
        {
            get { return index4AnalysisUseRotor3; }
            set
            {
                if (value != index4AnalysisUseRotor3)
                {
                    index4AnalysisUseRotor3 = value;
                    OnPropertyChanged("Index4AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("Index4AnalysisUseRotorIVCaption", "Index4AnalysisUseRotorIVTooltip",
            "Index4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index4AnalysisUseRotor4
        {
            get { return index4AnalysisUseRotor4; }
            set
            {
                if (value != index4AnalysisUseRotor4)
                {
                    index4AnalysisUseRotor4 = value;
                    OnPropertyChanged("Index4AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("Index4AnalysisUseRotorVCaption", "Index4AnalysisUseRotorVTooltip",
            "Index4AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index4AnalysisUseRotor5
        {
            get { return index4AnalysisUseRotor5; }
            set
            {
                if (value != index4AnalysisUseRotor5)
                {
                    index4AnalysisUseRotor5 = value;
                    OnPropertyChanged("Index4AnalysisUseRotor5");
                }
            }
        }

        #endregion


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "FünfZehn")]
        [TaskPane("Index Rotor 5: From", "PlugBoardATooltip", "PositionOptionsGroup", 40, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 9)]
        public int IndexRotor5From
        {
            get { return indexRotor5From; }
            set
            {
                indexRotor5From = value;
                OnPropertyChanged("IndexRotor5From");
                setSettingsVisibility();

                if (value > indexRotor5To)
                {
                    IndexRotor5To = value;


                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "FünfZehn")]
        [TaskPane("To", "PlugBoardBTooltip", "PositionOptionsGroup", 41, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 9)]
        public int IndexRotor5To
        {
            get { return indexRotor5To; }
            set
            {

                indexRotor5To = value;
                OnPropertyChanged("IndexRotor5To");
                setSettingsVisibility();
                if (value < indexRotor5From)
                {
                    IndexRotor5From = value;
                }

            }
        }

        #region Rotor choice

        [TaskPane("AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "Index5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index5AnalysisUseRotor1
        {
            get { return index5AnalysisUseRotor1; }
            set
            {
                if (value != index5AnalysisUseRotor1)
                {
                    index5AnalysisUseRotor1 = value;
                    OnPropertyChanged("Index5AnalysisUseRotor1");
                }
            }
        }


        [TaskPane("Index5AnalysisUseRotorIICaption", "Index5AnalysisUseRotorIITooltip",
            "Index5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index5AnalysisUseRotor2
        {
            get { return index5AnalysisUseRotor2; }
            set
            {
                if (value != index5AnalysisUseRotor2)
                {
                    index5AnalysisUseRotor2 = value;
                    OnPropertyChanged("Index5AnalysisUseRotor2");
                }
            }
        }


        [TaskPane("Index5AnalysisUseRotorIIICaption", "Index5AnalysisUseRotorIIITooltip",
            "Index5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index5AnalysisUseRotor3
        {
            get { return index5AnalysisUseRotor3; }
            set
            {
                if (value != index5AnalysisUseRotor3)
                {
                    index5AnalysisUseRotor3 = value;
                    OnPropertyChanged("Index5AnalysisUseRotor3");
                }
            }
        }


        [TaskPane("Index5AnalysisUseRotorIVCaption", "Index5AnalysisUseRotorIVTooltip",
            "Index5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index5AnalysisUseRotor4
        {
            get { return index5AnalysisUseRotor4; }
            set
            {
                if (value != index5AnalysisUseRotor4)
                {
                    index5AnalysisUseRotor4 = value;
                    OnPropertyChanged("Index5AnalysisUseRotor4");
                }
            }
        }



        [TaskPane("Index5AnalysisUseRotorVCaption", "Index5AnalysisUseRotorVTooltip",
            "Index5AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool Index5AnalysisUseRotor5
        {
            get { return index5AnalysisUseRotor5; }
            set
            {
                if (value != index5AnalysisUseRotor5)
                {
                    index5AnalysisUseRotor5 = value;
                    OnPropertyChanged("Index5AnalysisUseRotor5");
                }
            }
        }

        #endregion

        #endregion

        #endregion

        private void setSettingsVisibility()
        {
            #region Cipher
            if (!cipherRotor1Rotors)
            {
                hideSettingsElement("Cipher1AnalysisUseRotor1");
                hideSettingsElement("Cipher1AnalysisUseRotor2");
                hideSettingsElement("Cipher1AnalysisUseRotor3");
                hideSettingsElement("Cipher1AnalysisUseRotor4");
                hideSettingsElement("Cipher1AnalysisUseRotor5");
                hideSettingsElement("Cipher1AnalysisUseRotor6");
                hideSettingsElement("Cipher1AnalysisUseRotor7");
                hideSettingsElement("Cipher1AnalysisUseRotor8");
                hideSettingsElement("Cipher1AnalysisUseRotor9");
                hideSettingsElement("Cipher1AnalysisUseRotor0");
                hideSettingsElement("CipherRotor1From");
                hideSettingsElement("CipherRotor1To");
            }
            else
            {
                showSettingsElement("Cipher1AnalysisUseRotor1");
                showSettingsElement("Cipher1AnalysisUseRotor2");
                showSettingsElement("Cipher1AnalysisUseRotor3");
                showSettingsElement("Cipher1AnalysisUseRotor4");
                showSettingsElement("Cipher1AnalysisUseRotor5");
                showSettingsElement("Cipher1AnalysisUseRotor6");
                showSettingsElement("Cipher1AnalysisUseRotor7");
                showSettingsElement("Cipher1AnalysisUseRotor8");
                showSettingsElement("Cipher1AnalysisUseRotor9");
                showSettingsElement("Cipher1AnalysisUseRotor0");
                showSettingsElement("CipherRotor1From");
                showSettingsElement("CipherRotor1To");
            }

            if (!cipherRotor2Rotors)
            {
                hideSettingsElement("Cipher2AnalysisUseRotor1");
                hideSettingsElement("Cipher2AnalysisUseRotor2");
                hideSettingsElement("Cipher2AnalysisUseRotor3");
                hideSettingsElement("Cipher2AnalysisUseRotor4");
                hideSettingsElement("Cipher2AnalysisUseRotor5");
                hideSettingsElement("Cipher2AnalysisUseRotor6");
                hideSettingsElement("Cipher2AnalysisUseRotor7");
                hideSettingsElement("Cipher2AnalysisUseRotor8");
                hideSettingsElement("Cipher2AnalysisUseRotor9");
                hideSettingsElement("Cipher2AnalysisUseRotor0");
                hideSettingsElement("CipherRotor2From");
                hideSettingsElement("CipherRotor2To");
            }
            else
            {
                showSettingsElement("Cipher2AnalysisUseRotor1");
                showSettingsElement("Cipher2AnalysisUseRotor2");
                showSettingsElement("Cipher2AnalysisUseRotor3");
                showSettingsElement("Cipher2AnalysisUseRotor4");
                showSettingsElement("Cipher2AnalysisUseRotor5");
                showSettingsElement("Cipher2AnalysisUseRotor6");
                showSettingsElement("Cipher2AnalysisUseRotor7");
                showSettingsElement("Cipher2AnalysisUseRotor8");
                showSettingsElement("Cipher2AnalysisUseRotor9");
                showSettingsElement("Cipher2AnalysisUseRotor0");
                showSettingsElement("CipherRotor2From");
                showSettingsElement("CipherRotor2To");
            }


            if (!cipherRotor3Rotors)
            {
                hideSettingsElement("Cipher3AnalysisUseRotor1");
                hideSettingsElement("Cipher3AnalysisUseRotor2");
                hideSettingsElement("Cipher3AnalysisUseRotor3");
                hideSettingsElement("Cipher3AnalysisUseRotor4");
                hideSettingsElement("Cipher3AnalysisUseRotor5");
                hideSettingsElement("Cipher3AnalysisUseRotor6");
                hideSettingsElement("Cipher3AnalysisUseRotor7");
                hideSettingsElement("Cipher3AnalysisUseRotor8");
                hideSettingsElement("Cipher3AnalysisUseRotor9");
                hideSettingsElement("Cipher3AnalysisUseRotor0");
                hideSettingsElement("CipherRotor3From");
                hideSettingsElement("CipherRotor3To");
            }
            else
            {
                showSettingsElement("Cipher3AnalysisUseRotor1");
                showSettingsElement("Cipher3AnalysisUseRotor2");
                showSettingsElement("Cipher3AnalysisUseRotor3");
                showSettingsElement("Cipher3AnalysisUseRotor4");
                showSettingsElement("Cipher3AnalysisUseRotor5");
                showSettingsElement("Cipher3AnalysisUseRotor6");
                showSettingsElement("Cipher3AnalysisUseRotor7");
                showSettingsElement("Cipher3AnalysisUseRotor8");
                showSettingsElement("Cipher3AnalysisUseRotor9");
                showSettingsElement("Cipher3AnalysisUseRotor0");
                showSettingsElement("CipherRotor3From");
                showSettingsElement("CipherRotor3To");
            }

            if (!cipherRotor4Rotors)
            {
                hideSettingsElement("Cipher4AnalysisUseRotor1");
                hideSettingsElement("Cipher4AnalysisUseRotor2");
                hideSettingsElement("Cipher4AnalysisUseRotor3");
                hideSettingsElement("Cipher4AnalysisUseRotor4");
                hideSettingsElement("Cipher4AnalysisUseRotor5");
                hideSettingsElement("Cipher4AnalysisUseRotor6");
                hideSettingsElement("Cipher4AnalysisUseRotor7");
                hideSettingsElement("Cipher4AnalysisUseRotor8");
                hideSettingsElement("Cipher4AnalysisUseRotor9");
                hideSettingsElement("Cipher4AnalysisUseRotor0");
                hideSettingsElement("CipherRotor4From");
                hideSettingsElement("CipherRotor4To");
            }
            else
            {
                showSettingsElement("Cipher4AnalysisUseRotor1");
                showSettingsElement("Cipher4AnalysisUseRotor2");
                showSettingsElement("Cipher4AnalysisUseRotor3");
                showSettingsElement("Cipher4AnalysisUseRotor4");
                showSettingsElement("Cipher4AnalysisUseRotor5");
                showSettingsElement("Cipher4AnalysisUseRotor6");
                showSettingsElement("Cipher4AnalysisUseRotor7");
                showSettingsElement("Cipher4AnalysisUseRotor8");
                showSettingsElement("Cipher4AnalysisUseRotor9");
                showSettingsElement("Cipher4AnalysisUseRotor0");
                showSettingsElement("CipherRotor4From");
                showSettingsElement("CipherRotor4To");
            }

            if (!cipherRotor5Rotors)
            {
                hideSettingsElement("Cipher5AnalysisUseRotor1");
                hideSettingsElement("Cipher5AnalysisUseRotor2");
                hideSettingsElement("Cipher5AnalysisUseRotor3");
                hideSettingsElement("Cipher5AnalysisUseRotor4");
                hideSettingsElement("Cipher5AnalysisUseRotor5");
                hideSettingsElement("Cipher5AnalysisUseRotor6");
                hideSettingsElement("Cipher5AnalysisUseRotor7");
                hideSettingsElement("Cipher5AnalysisUseRotor8");
                hideSettingsElement("Cipher5AnalysisUseRotor9");
                hideSettingsElement("Cipher5AnalysisUseRotor0");
                hideSettingsElement("CipherRotor5From");
                hideSettingsElement("CipherRotor5To");
            }
            else
            {
                showSettingsElement("Cipher5AnalysisUseRotor1");
                showSettingsElement("Cipher5AnalysisUseRotor2");
                showSettingsElement("Cipher5AnalysisUseRotor3");
                showSettingsElement("Cipher5AnalysisUseRotor4");
                showSettingsElement("Cipher5AnalysisUseRotor5");
                showSettingsElement("Cipher5AnalysisUseRotor6");
                showSettingsElement("Cipher5AnalysisUseRotor7");
                showSettingsElement("Cipher5AnalysisUseRotor8");
                showSettingsElement("Cipher5AnalysisUseRotor9");
                showSettingsElement("Cipher5AnalysisUseRotor0");
                showSettingsElement("CipherRotor5From");
                showSettingsElement("CipherRotor5To");
            }
            #endregion 

            #region Control
            if (!controlRotor1Rotors)
            {
                hideSettingsElement("Control1AnalysisUseRotor1");
                hideSettingsElement("Control1AnalysisUseRotor2");
                hideSettingsElement("Control1AnalysisUseRotor3");
                hideSettingsElement("Control1AnalysisUseRotor4");
                hideSettingsElement("Control1AnalysisUseRotor5");
                hideSettingsElement("Control1AnalysisUseRotor6");
                hideSettingsElement("Control1AnalysisUseRotor7");
                hideSettingsElement("Control1AnalysisUseRotor8");
                hideSettingsElement("Control1AnalysisUseRotor9");
                hideSettingsElement("Control1AnalysisUseRotor0");
                hideSettingsElement("ControlRotor1From");
                hideSettingsElement("ControlRotor1To");
            }
            else
            {
                showSettingsElement("Control1AnalysisUseRotor1");
                showSettingsElement("Control1AnalysisUseRotor2");
                showSettingsElement("Control1AnalysisUseRotor3");
                showSettingsElement("Control1AnalysisUseRotor4");
                showSettingsElement("Control1AnalysisUseRotor5");
                showSettingsElement("Control1AnalysisUseRotor6");
                showSettingsElement("Control1AnalysisUseRotor7");
                showSettingsElement("Control1AnalysisUseRotor8");
                showSettingsElement("Control1AnalysisUseRotor9");
                showSettingsElement("Control1AnalysisUseRotor0");
                showSettingsElement("ControlRotor1From");
                showSettingsElement("ControlRotor1To");
            }

            if (!controlRotor2Rotors)
            {
                hideSettingsElement("Control2AnalysisUseRotor1");
                hideSettingsElement("Control2AnalysisUseRotor2");
                hideSettingsElement("Control2AnalysisUseRotor3");
                hideSettingsElement("Control2AnalysisUseRotor4");
                hideSettingsElement("Control2AnalysisUseRotor5");
                hideSettingsElement("Control2AnalysisUseRotor6");
                hideSettingsElement("Control2AnalysisUseRotor7");
                hideSettingsElement("Control2AnalysisUseRotor8");
                hideSettingsElement("Control2AnalysisUseRotor9");
                hideSettingsElement("Control2AnalysisUseRotor0");
                hideSettingsElement("ControlRotor2From");
                hideSettingsElement("ControlRotor2To");
            }
            else
            {
                showSettingsElement("Control2AnalysisUseRotor1");
                showSettingsElement("Control2AnalysisUseRotor2");
                showSettingsElement("Control2AnalysisUseRotor3");
                showSettingsElement("Control2AnalysisUseRotor4");
                showSettingsElement("Control2AnalysisUseRotor5");
                showSettingsElement("Control2AnalysisUseRotor6");
                showSettingsElement("Control2AnalysisUseRotor7");
                showSettingsElement("Control2AnalysisUseRotor8");
                showSettingsElement("Control2AnalysisUseRotor9");
                showSettingsElement("Control2AnalysisUseRotor0");
                showSettingsElement("ControlRotor2From");
                showSettingsElement("ControlRotor2To");
            }


            if (!controlRotor3Rotors)
            {
                hideSettingsElement("Control3AnalysisUseRotor1");
                hideSettingsElement("Control3AnalysisUseRotor2");
                hideSettingsElement("Control3AnalysisUseRotor3");
                hideSettingsElement("Control3AnalysisUseRotor4");
                hideSettingsElement("Control3AnalysisUseRotor5");
                hideSettingsElement("Control3AnalysisUseRotor6");
                hideSettingsElement("Control3AnalysisUseRotor7");
                hideSettingsElement("Control3AnalysisUseRotor8");
                hideSettingsElement("Control3AnalysisUseRotor9");
                hideSettingsElement("Control3AnalysisUseRotor0");
                hideSettingsElement("ControlRotor3From");
                hideSettingsElement("ControlRotor3To");
            }
            else
            {
                showSettingsElement("Control3AnalysisUseRotor1");
                showSettingsElement("Control3AnalysisUseRotor2");
                showSettingsElement("Control3AnalysisUseRotor3");
                showSettingsElement("Control3AnalysisUseRotor4");
                showSettingsElement("Control3AnalysisUseRotor5");
                showSettingsElement("Control3AnalysisUseRotor6");
                showSettingsElement("Control3AnalysisUseRotor7");
                showSettingsElement("Control3AnalysisUseRotor8");
                showSettingsElement("Control3AnalysisUseRotor9");
                showSettingsElement("Control3AnalysisUseRotor0");
                showSettingsElement("ControlRotor3From");
                showSettingsElement("ControlRotor3To");
            }

            if (!controlRotor4Rotors)
            {
                hideSettingsElement("Control4AnalysisUseRotor1");
                hideSettingsElement("Control4AnalysisUseRotor2");
                hideSettingsElement("Control4AnalysisUseRotor3");
                hideSettingsElement("Control4AnalysisUseRotor4");
                hideSettingsElement("Control4AnalysisUseRotor5");
                hideSettingsElement("Control4AnalysisUseRotor6");
                hideSettingsElement("Control4AnalysisUseRotor7");
                hideSettingsElement("Control4AnalysisUseRotor8");
                hideSettingsElement("Control4AnalysisUseRotor9");
                hideSettingsElement("Control4AnalysisUseRotor0");
                hideSettingsElement("ControlRotor4From");
                hideSettingsElement("ControlRotor4To");
            }
            else
            {
                showSettingsElement("Control4AnalysisUseRotor1");
                showSettingsElement("Control4AnalysisUseRotor2");
                showSettingsElement("Control4AnalysisUseRotor3");
                showSettingsElement("Control4AnalysisUseRotor4");
                showSettingsElement("Control4AnalysisUseRotor5");
                showSettingsElement("Control4AnalysisUseRotor6");
                showSettingsElement("Control4AnalysisUseRotor7");
                showSettingsElement("Control4AnalysisUseRotor8");
                showSettingsElement("Control4AnalysisUseRotor9");
                showSettingsElement("Control4AnalysisUseRotor0");
                showSettingsElement("ControlRotor4From");
                showSettingsElement("ControlRotor4To");
            }

            if (!controlRotor5Rotors)
            {
                hideSettingsElement("Control5AnalysisUseRotor1");
                hideSettingsElement("Control5AnalysisUseRotor2");
                hideSettingsElement("Control5AnalysisUseRotor3");
                hideSettingsElement("Control5AnalysisUseRotor4");
                hideSettingsElement("Control5AnalysisUseRotor5");
                hideSettingsElement("Control5AnalysisUseRotor6");
                hideSettingsElement("Control5AnalysisUseRotor7");
                hideSettingsElement("Control5AnalysisUseRotor8");
                hideSettingsElement("Control5AnalysisUseRotor9");
                hideSettingsElement("Control5AnalysisUseRotor0");
                hideSettingsElement("ControlRotor5From");
                hideSettingsElement("ControlRotor5To");
            }
            else
            {
                showSettingsElement("Control5AnalysisUseRotor1");
                showSettingsElement("Control5AnalysisUseRotor2");
                showSettingsElement("Control5AnalysisUseRotor3");
                showSettingsElement("Control5AnalysisUseRotor4");
                showSettingsElement("Control5AnalysisUseRotor5");
                showSettingsElement("Control5AnalysisUseRotor6");
                showSettingsElement("Control5AnalysisUseRotor7");
                showSettingsElement("Control5AnalysisUseRotor8");
                showSettingsElement("Control5AnalysisUseRotor9");
                showSettingsElement("Control5AnalysisUseRotor0");
                showSettingsElement("ControlRotor5From");
                showSettingsElement("ControlRotor5To");
            }
            #endregion 

            #region Index
            if (!indexRotor1Rotors)
            {
                hideSettingsElement("Index1AnalysisUseRotor1");
                hideSettingsElement("Index1AnalysisUseRotor2");
                hideSettingsElement("Index1AnalysisUseRotor3");
                hideSettingsElement("Index1AnalysisUseRotor4");
                hideSettingsElement("Index1AnalysisUseRotor5");
                hideSettingsElement("Index1AnalysisUseRotor6");
                hideSettingsElement("Index1AnalysisUseRotor7");
                hideSettingsElement("Index1AnalysisUseRotor8");
                hideSettingsElement("Index1AnalysisUseRotor9");
                hideSettingsElement("Index1AnalysisUseRotor0");
                hideSettingsElement("IndexRotor1From");
                hideSettingsElement("IndexRotor1To");
            }
            else
            {
                showSettingsElement("Index1AnalysisUseRotor1");
                showSettingsElement("Index1AnalysisUseRotor2");
                showSettingsElement("Index1AnalysisUseRotor3");
                showSettingsElement("Index1AnalysisUseRotor4");
                showSettingsElement("Index1AnalysisUseRotor5");
                showSettingsElement("Index1AnalysisUseRotor6");
                showSettingsElement("Index1AnalysisUseRotor7");
                showSettingsElement("Index1AnalysisUseRotor8");
                showSettingsElement("Index1AnalysisUseRotor9");
                showSettingsElement("Index1AnalysisUseRotor0");
                showSettingsElement("IndexRotor1From");
                showSettingsElement("IndexRotor1To");
            }

            if (!indexRotor2Rotors)
            {
                hideSettingsElement("Index2AnalysisUseRotor1");
                hideSettingsElement("Index2AnalysisUseRotor2");
                hideSettingsElement("Index2AnalysisUseRotor3");
                hideSettingsElement("Index2AnalysisUseRotor4");
                hideSettingsElement("Index2AnalysisUseRotor5");
                hideSettingsElement("Index2AnalysisUseRotor6");
                hideSettingsElement("Index2AnalysisUseRotor7");
                hideSettingsElement("Index2AnalysisUseRotor8");
                hideSettingsElement("Index2AnalysisUseRotor9");
                hideSettingsElement("Index2AnalysisUseRotor0");
                hideSettingsElement("IndexRotor2From");
                hideSettingsElement("IndexRotor2To");
            }
            else
            {
                showSettingsElement("Index2AnalysisUseRotor1");
                showSettingsElement("Index2AnalysisUseRotor2");
                showSettingsElement("Index2AnalysisUseRotor3");
                showSettingsElement("Index2AnalysisUseRotor4");
                showSettingsElement("Index2AnalysisUseRotor5");
                showSettingsElement("Index2AnalysisUseRotor6");
                showSettingsElement("Index2AnalysisUseRotor7");
                showSettingsElement("Index2AnalysisUseRotor8");
                showSettingsElement("Index2AnalysisUseRotor9");
                showSettingsElement("Index2AnalysisUseRotor0");
                showSettingsElement("IndexRotor2From");
                showSettingsElement("IndexRotor2To");
            }


            if (!indexRotor3Rotors)
            {
                hideSettingsElement("Index3AnalysisUseRotor1");
                hideSettingsElement("Index3AnalysisUseRotor2");
                hideSettingsElement("Index3AnalysisUseRotor3");
                hideSettingsElement("Index3AnalysisUseRotor4");
                hideSettingsElement("Index3AnalysisUseRotor5");
                hideSettingsElement("Index3AnalysisUseRotor6");
                hideSettingsElement("Index3AnalysisUseRotor7");
                hideSettingsElement("Index3AnalysisUseRotor8");
                hideSettingsElement("Index3AnalysisUseRotor9");
                hideSettingsElement("Index3AnalysisUseRotor0");
                hideSettingsElement("IndexRotor3From");
                hideSettingsElement("IndexRotor3To");
            }
            else
            {
                showSettingsElement("Index3AnalysisUseRotor1");
                showSettingsElement("Index3AnalysisUseRotor2");
                showSettingsElement("Index3AnalysisUseRotor3");
                showSettingsElement("Index3AnalysisUseRotor4");
                showSettingsElement("Index3AnalysisUseRotor5");
                showSettingsElement("Index3AnalysisUseRotor6");
                showSettingsElement("Index3AnalysisUseRotor7");
                showSettingsElement("Index3AnalysisUseRotor8");
                showSettingsElement("Index3AnalysisUseRotor9");
                showSettingsElement("Index3AnalysisUseRotor0");
                showSettingsElement("IndexRotor3From");
                showSettingsElement("IndexRotor3To");
            }

            if (!indexRotor4Rotors)
            {
                hideSettingsElement("Index4AnalysisUseRotor1");
                hideSettingsElement("Index4AnalysisUseRotor2");
                hideSettingsElement("Index4AnalysisUseRotor3");
                hideSettingsElement("Index4AnalysisUseRotor4");
                hideSettingsElement("Index4AnalysisUseRotor5");
                hideSettingsElement("Index4AnalysisUseRotor6");
                hideSettingsElement("Index4AnalysisUseRotor7");
                hideSettingsElement("Index4AnalysisUseRotor8");
                hideSettingsElement("Index4AnalysisUseRotor9");
                hideSettingsElement("Index4AnalysisUseRotor0");
                hideSettingsElement("IndexRotor4From");
                hideSettingsElement("IndexRotor4To");
            }
            else
            {
                showSettingsElement("Index4AnalysisUseRotor1");
                showSettingsElement("Index4AnalysisUseRotor2");
                showSettingsElement("Index4AnalysisUseRotor3");
                showSettingsElement("Index4AnalysisUseRotor4");
                showSettingsElement("Index4AnalysisUseRotor5");
                showSettingsElement("Index4AnalysisUseRotor6");
                showSettingsElement("Index4AnalysisUseRotor7");
                showSettingsElement("Index4AnalysisUseRotor8");
                showSettingsElement("Index4AnalysisUseRotor9");
                showSettingsElement("Index4AnalysisUseRotor0");
                showSettingsElement("IndexRotor4From");
                showSettingsElement("IndexRotor4To");
            }

            if (!indexRotor5Rotors)
            {
                hideSettingsElement("Index5AnalysisUseRotor1");
                hideSettingsElement("Index5AnalysisUseRotor2");
                hideSettingsElement("Index5AnalysisUseRotor3");
                hideSettingsElement("Index5AnalysisUseRotor4");
                hideSettingsElement("Index5AnalysisUseRotor5");
                hideSettingsElement("Index5AnalysisUseRotor6");
                hideSettingsElement("Index5AnalysisUseRotor7");
                hideSettingsElement("Index5AnalysisUseRotor8");
                hideSettingsElement("Index5AnalysisUseRotor9");
                hideSettingsElement("Index5AnalysisUseRotor0");
                hideSettingsElement("IndexRotor5From");
                hideSettingsElement("IndexRotor5To");
            }
            else
            {
                showSettingsElement("Index5AnalysisUseRotor1");
                showSettingsElement("Index5AnalysisUseRotor2");
                showSettingsElement("Index5AnalysisUseRotor3");
                showSettingsElement("Index5AnalysisUseRotor4");
                showSettingsElement("Index5AnalysisUseRotor5");
                showSettingsElement("Index5AnalysisUseRotor6");
                showSettingsElement("Index5AnalysisUseRotor7");
                showSettingsElement("Index5AnalysisUseRotor8");
                showSettingsElement("Index5AnalysisUseRotor9");
                showSettingsElement("Index5AnalysisUseRotor0");
                showSettingsElement("IndexRotor5From");
                showSettingsElement("IndexRotor5To");
            }
            #endregion 
        }

        private void hideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }

        private void showSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

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
