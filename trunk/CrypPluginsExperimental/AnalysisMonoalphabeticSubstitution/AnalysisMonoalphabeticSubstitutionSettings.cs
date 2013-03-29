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
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Windows;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    public class AnalysisMonoalphabeticSubstitutionSettings : ISettings
    {
        #region Private Variables
        
        // General variables
        private bool hasChanges = false;
        // Language variables
        private const String smallEng = "abcdefghijklmnopqrstuvwxyz";
        private const String capEng = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const String smallGer = "abcdefghijklmnopqrstuvwxyzöäüß";
        private const String capGer = "ABCDEFGHIJKLMNOPQRSTUVWXYZÖÄÜ";
        private const String specChars = ",;.:-_<>#+*!$%/{([)]}=?";
        // "Alphabet" variables
        private int bo_alphabet = 0;
        private Boolean bo_smallLetters = true;
        private Boolean bo_capitalLetters = false;
        private Boolean bo_specialChars = false;
        private Boolean bo_space = false;
        private String bo_textAlphabet = smallEng;
        public Boolean bo_alphabet_has_changed = true;
        // "PTAlphabet" variables
        private int pt_alphabet = 0;
        private Boolean pt_smallLetters = true;
        private Boolean pt_capitalLetters = false;
        private Boolean pt_specialChars = false;
        private Boolean pt_space = false;
        private String pt_textAlphabet = smallEng;
        public Boolean pt_alphabet_has_changed = true;
        // "CTAlphabet" variables
        private int ct_alphabet = 0;
        private Boolean ct_smallLetters = true;
        private Boolean ct_capitalLetters = false;
        private Boolean ct_specialChars = false;
        private Boolean ct_space = false;
        private String ct_textAlphabet = smallEng;
        public Boolean ct_alphabet_has_changed = true;
        // Advanced settings variables
        private Boolean separateAlphabets = false;

        #endregion

        #region Initialization / Constructor

        public void Initialize(){
            if (separateAlphabets==true){
                hideSettingsElement("boAlphabet");
                hideSettingsElement("boSmallLetters");
                hideSettingsElement("boCapitalLetters");
                hideSettingsElement("boSpecialChars");
                hideSettingsElement("boSpace");
                hideSettingsElement("boTextAlphabet");
                showSettingsElement("ptAlphabet");
                showSettingsElement("ptSmallLetters");
                showSettingsElement("ptCapitalLetters");
                showSettingsElement("ptSpecialChars");
                showSettingsElement("ptSpace");
                showSettingsElement("ptTextAlphabet");
                showSettingsElement("ctAlphabet");
                showSettingsElement("ctSmallLetters");
                showSettingsElement("ctCapitalLetters");
                showSettingsElement("ctSpecialChars");
                showSettingsElement("ctSpace");
                showSettingsElement("ctTextAlphabet");   
            }else{
                showSettingsElement("boAlphabet");
                showSettingsElement("boSmallLetters");
                showSettingsElement("boCapitalLetters");
                showSettingsElement("boSpecialChars");
                showSettingsElement("boSpace");
                showSettingsElement("boTextAlphabet");
                hideSettingsElement("ptAlphabet");
                hideSettingsElement("ptSmallLetters");
                hideSettingsElement("ptCapitalLetters");
                hideSettingsElement("ptSpecialChars");
                hideSettingsElement("ptSpace");
                hideSettingsElement("ptTextAlphabet");
                hideSettingsElement("ctAlphabet");
                hideSettingsElement("ctSmallLetters");
                hideSettingsElement("ctCapitalLetters");
                hideSettingsElement("ctSpecialChars");
                hideSettingsElement("ctSpace");
                hideSettingsElement("ctTextAlphabet");
            }
        }

        #endregion

        #region TaskPane Settings

        // Settings if plaintext alphabet equals ciphertext alphabet
        [TaskPane("ChooseAlphabetCaption", "ChooseAlphabetTooltip", "AlphabetGroup", 1, false, ControlType.ComboBox,
            new string[] { "ChooseAlphabetList1", "ChooseAlphabetList2", "ChooseAlphabetList3" })]
        public int boAlphabet
        {
            get { return bo_alphabet; }
            set {
                bo_alphabet = value;
                switch (bo_alphabet)
                {
                    case 0:
                    case 1:
                        showSettingsElement("boSmallLetters");
                        showSettingsElement("boCapitalLetters");
                        showSettingsElement("boSpecialChars");
                        showSettingsElement("boSpace");
                        showSettingsElement("boTextAlphabet");
                        break;
                    case 2:
                        hideSettingsElement("boSmallLetters");
                        hideSettingsElement("boCapitalLetters");
                        hideSettingsElement("boSpecialChars");
                        hideSettingsElement("boSpace");
                        hideSettingsElement("boTextAlphabet");
                        break;
                }
                bo_textAlphabet = createAlphabetString(bo_alphabet, bo_smallLetters, bo_capitalLetters, bo_specialChars, bo_space);
                OnPropertyChanged("boTextAlphabet");
                OnPropertyChanged("boAlphabet");}
        }
        [TaskPane("SmallLettersCaption", "SmallLettersTooltip", "AlphabetGroup", 2, false, ControlType.CheckBox, null)]
        public Boolean boSmallLetters
        {
            get { return bo_smallLetters; }
            set { 
                bo_smallLetters = value;
                bo_textAlphabet = createAlphabetString(bo_alphabet,bo_smallLetters,bo_capitalLetters,bo_specialChars,bo_space);
                OnPropertyChanged("boSmallLetters");
                OnPropertyChanged("boTextAlphabet");}
        }
        [TaskPane("CapitalLettersCaption", "CapitalLettersTooltip", "AlphabetGroup", 3, false, ControlType.CheckBox, null)]
        public Boolean boCapitalLetters
        {
            get { return bo_capitalLetters; }
            set
            {
                bo_capitalLetters = value;
                bo_textAlphabet = createAlphabetString(bo_alphabet, bo_smallLetters, bo_capitalLetters, bo_specialChars, bo_space);
                OnPropertyChanged("boTextAlphabet");
                OnPropertyChanged("boCapitalLetters");
            }
        }
        [TaskPane("SpecialCharsCaption", "SpecialCharsTooltip", "AlphabetGroup", 4, false, ControlType.CheckBox, null)]
        public Boolean boSpecialChars
        {
            get { return bo_specialChars; }
            set { 
                bo_specialChars = value;
                bo_textAlphabet = createAlphabetString(bo_alphabet, bo_smallLetters, bo_capitalLetters, bo_specialChars, bo_space);
                OnPropertyChanged("boTextAlphabet");
                OnPropertyChanged("boSpecialChars");}
        }
        [TaskPane("SpaceCaption", "SpaceTooltip", "AlphabetGroup", 5, false, ControlType.CheckBox, null)]
        public Boolean boSpace
        {
            get { return bo_space; }
            set {
                bo_space = value;
                bo_textAlphabet = createAlphabetString(bo_alphabet, bo_smallLetters, bo_capitalLetters, bo_specialChars, bo_space);
                OnPropertyChanged("boTextAlphabet");
                OnPropertyChanged("boSpace");}
        }
        [TaskPane("AlphabetCaption", "AlphabetTooltip", "AlphabetGroup", 6, false, ControlType.TextBoxReadOnly, null)]
        public String boTextAlphabet
        {
            get { return bo_textAlphabet; }
            set {
                bo_textAlphabet = value;
                OnPropertyChanged("boTextAlphabet");
                bo_alphabet_has_changed = true;
            }
        }

        // Settings for plaintext alphabet
        [TaskPane("PTChooseAlphabetCaption", "PTChooseAlphabetTooltip","PTAlphabetGroup", 1, false, ControlType.ComboBox,
            new string[] { "ChooseAlphabetList1", "ChooseAlphabetList2", "ChooseAlphabetList3" })]
        public int ptAlphabet
        {
            get { return pt_alphabet; }
            set {
                pt_alphabet = value;
                switch (pt_alphabet)
                {
                    case 0:
                    case 1:
                        showSettingsElement("ptSmallLetters");
                        showSettingsElement("ptCapitalLetters");
                        showSettingsElement("ptSpecialChars");
                        showSettingsElement("ptSpace");
                        showSettingsElement("ptTextAlphabet");
                        break;
                    case 2:
                        hideSettingsElement("ptSmallLetters");
                        hideSettingsElement("ptCapitalLetters");
                        hideSettingsElement("ptSpecialChars");
                        hideSettingsElement("ptSpace");
                        hideSettingsElement("ptTextAlphabet");
                        break;
                }
                pt_textAlphabet = createAlphabetString(pt_alphabet, pt_smallLetters, pt_capitalLetters, pt_specialChars, pt_space);
                OnPropertyChanged("ptTextAlphabet");
                OnPropertyChanged("ptAlphabet");}
        }
        [TaskPane("PTSmallLettersCaption", "PTSmallLettersTooltip", "PTAlphabetGroup", 2, false, ControlType.CheckBox, null)]
        public Boolean ptSmallLetters
        {
            get { return pt_smallLetters; }
            set {
                pt_smallLetters = value;
                pt_textAlphabet = createAlphabetString(pt_alphabet, pt_smallLetters, pt_capitalLetters, pt_specialChars, pt_space);
                OnPropertyChanged("ptSmallLetters");
                OnPropertyChanged("ptTextAlphabet");}
        }
        [TaskPane("PTCapitalLettersCaption", "PTCapitalLettersTooltip", "PTAlphabetGroup", 3, false, ControlType.CheckBox, null)]
        public Boolean ptCapitalLetters
        {
            get { return pt_capitalLetters; }
            set
            {
                pt_capitalLetters = value;
                pt_textAlphabet = createAlphabetString(pt_alphabet, pt_smallLetters, pt_capitalLetters, pt_specialChars, pt_space);
                OnPropertyChanged("ptCapitalLetters");
                OnPropertyChanged("ptTextAlphabet");
            }
        }
        [TaskPane("PTSpecialCharsCaption", "PTSpecialCharsTooltip", "PTAlphabetGroup", 4, false, ControlType.CheckBox, null)]
        public Boolean ptSpecialChars
        {
            get { return pt_specialChars; }
            set {
                pt_specialChars = value;
                pt_textAlphabet = createAlphabetString(pt_alphabet, pt_smallLetters, pt_capitalLetters, pt_specialChars, pt_space);
                OnPropertyChanged("ptTextAlphabet");
                OnPropertyChanged("ptSpecialChars");}
        }
        [TaskPane("PTSpaceCaption", "PTSpaceTooltip", "PTAlphabetGroup", 5, false, ControlType.CheckBox, null)]
        public Boolean ptSpace
        {
            get { return pt_space; }
            set {
                pt_space = value;
                pt_textAlphabet = createAlphabetString(pt_alphabet, pt_smallLetters, pt_capitalLetters, pt_specialChars, pt_space);
                OnPropertyChanged("ptTextAlphabet");
                OnPropertyChanged("ptSpace");}
        }
        [TaskPane("PTAlphabetCaption", "PTAlphabetTooltip", "PTAlphabetGroup", 6, false, ControlType.TextBoxReadOnly, null)]
        public String ptTextAlphabet
        {
            get { return pt_textAlphabet; }
            set {
                pt_textAlphabet = value;
                OnPropertyChanged("ptTextAlphabet");
                pt_alphabet_has_changed = true;
            }
        }

        // Settings for ciphertext alphabet
        [TaskPane("CTChooseAlphabetCaption", "CTChooseAlphabetTooltip", "CTAlphabetGroup", 1, false, ControlType.ComboBox,
            new string[] { "ChooseAlphabetList1", "ChooseAlphabetList2", "ChooseAlphabetList3"})]
        public int ctAlphabet
        {
            get { return ct_alphabet; }
            set {
                ct_alphabet = value;
                switch (ct_alphabet)
                {
                    case 0:
                    case 1:
                        showSettingsElement("ctSmallLetters");
                        showSettingsElement("ctCapitalLetters");
                        showSettingsElement("ctSpecialChars");
                        showSettingsElement("ctSpace");
                        showSettingsElement("ctTextAlphabet");
                        break;
                    case 2:
                        hideSettingsElement("ctSmallLetters");
                        hideSettingsElement("ctCapitalLetters");
                        hideSettingsElement("ctSpecialChars");
                        hideSettingsElement("ctSpace");
                        hideSettingsElement("ctTextAlphabet");
                        break;
                }
                ct_textAlphabet = createAlphabetString(ct_alphabet, ct_smallLetters, ct_capitalLetters, ct_specialChars, ct_space);
                OnPropertyChanged("ctTextAlphabet");
                OnPropertyChanged("ctAlphabet");
            }
        }
        [TaskPane("CTSmallLettersCaption", "CTSmallLettersTooltip","CTAlphabetGroup", 2, false, ControlType.CheckBox,null)]
        public Boolean ctSmallLetters
        {
            get { return ct_smallLetters; }
            set {
                ct_smallLetters = value;
                ct_textAlphabet = createAlphabetString(ct_alphabet, ct_smallLetters, ct_capitalLetters, ct_specialChars, ct_space);
                OnPropertyChanged("ctTextAlphabet");
                OnPropertyChanged("ctSmallLetters");
            }
        }
        [TaskPane("CTCapitalLettersCaption", "CTCapitalLettersTooltip", "CTAlphabetGroup",3, false, ControlType.CheckBox, null)]
        public Boolean ctCapitalLetters
        {
            get { return ct_capitalLetters; }
            set
            {
                ct_capitalLetters = value;
                ct_textAlphabet = createAlphabetString(ct_alphabet, ct_smallLetters, ct_capitalLetters, ct_specialChars, ct_space);
                OnPropertyChanged("ctTextAlphabet");
                OnPropertyChanged("ctCapitalLetters");
            }
        }
        [TaskPane("CTSpecialCharsCaption", "CTSpecialCharsTooltip", "CTAlphabetGroup", 4, false, ControlType.CheckBox, null)]
        public Boolean ctSpecialChars
        {
            get { return ct_specialChars; }
            set
            {
                ct_specialChars = value;
                ct_textAlphabet = createAlphabetString(ct_alphabet, ct_smallLetters, ct_capitalLetters, ct_specialChars, ct_space);
                OnPropertyChanged("ctTextAlphabet");
                OnPropertyChanged("ctSpecialChars");
            }
        }
        [TaskPane("CTSpaceCaption", "CTSpaceTooltip", "CTAlphabetGroup", 5, false, ControlType.CheckBox, null)]
        public Boolean ctSpace
        {
            get { return ct_space; }
            set {
                ct_space = value;
                ct_textAlphabet = createAlphabetString(ct_alphabet, ct_smallLetters, ct_capitalLetters, ct_specialChars, ct_space);
                OnPropertyChanged("ctTextAlphabet");
                OnPropertyChanged("ctSpace");
            }
        }
        [TaskPane("CTAlphabetCaption", "CTAlphabetTooltip", "CTAlphabetGroup", 6, false, ControlType.TextBoxReadOnly, null)]
        public String ctTextAlphabet
        {
            get { return ct_textAlphabet; }
            set {
                ct_textAlphabet = value;
                OnPropertyChanged("ctTextAlphabet");
                ct_alphabet_has_changed = true;
            }
        }

        // Advanced settings
        [TaskPane("SeparateAlphabetsCaption", "SeparateAlphabetsTooltip", "AdvancedSettingsGroup", 1, false, ControlType.CheckBox, null)]
        public Boolean SeparateAlphabets
        {
            get { return separateAlphabets; }
            set { 
                separateAlphabets = value;
                OnPropertyChanged("SeparateAlphabets");
                if (separateAlphabets == true)
                {
                    hideSettingsElement("boAlphabet");
                    hideSettingsElement("boSmallLetters");
                    hideSettingsElement("boCapitalLetters");
                    hideSettingsElement("boSpecialChars");
                    hideSettingsElement("boSpace");
                    hideSettingsElement("boTextAlphabet");
                    showSettingsElement("ptAlphabet");
                    showSettingsElement("ptSmallLetters");
                    showSettingsElement("ptCapitalLetters");
                    showSettingsElement("ptSpecialChars");
                    showSettingsElement("ptSpace");
                    showSettingsElement("ptTextAlphabet");
                    showSettingsElement("ctAlphabet");
                    showSettingsElement("ctSmallLetters");
                    showSettingsElement("ctCapitalLetters");
                    showSettingsElement("ctSpecialChars");
                    showSettingsElement("ctSpace");
                    showSettingsElement("ctTextAlphabet");
                }
                else
                {
                    showSettingsElement("boAlphabet");
                    showSettingsElement("boSmallLetters");
                    showSettingsElement("boCapitalLetters");
                    showSettingsElement("boSpecialChars");
                    showSettingsElement("boSpace");
                    showSettingsElement("boTextAlphabet");
                    hideSettingsElement("ptAlphabet");
                    hideSettingsElement("ptSmallLetters");
                    hideSettingsElement("ptCapitalLetters");
                    hideSettingsElement("ptSpecialChars");
                    hideSettingsElement("ptSpace");
                    hideSettingsElement("ptTextAlphabet");
                    hideSettingsElement("ctAlphabet");
                    hideSettingsElement("ctSmallLetters");
                    hideSettingsElement("ctCapitalLetters");
                    hideSettingsElement("ctSpecialChars");
                    hideSettingsElement("ctSpace");
                    hideSettingsElement("ctTextAlphabet");
                }
                bo_alphabet_has_changed = true;
                pt_alphabet_has_changed = true;
                ct_alphabet_has_changed = true;
            }
        }

        #endregion

        #region ISettings Members

        /// <summary>
        /// HOWTO: This flags indicates whether some setting has been changed since the last save.
        /// If a property was changed, this becomes true, hence CrypTool will ask automatically if you want to save your changes.
        /// </summary>
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

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        #endregion

        #region Helper Functions

        private String createAlphabetString(int alph, Boolean small, Boolean cap, Boolean spec, Boolean space)
        {
            String res = "";
            if (alph == 0)
            {
                if (small)
                {
                    res += smallEng;
                }
                if (cap)
                {
                    res += capEng;
                }
            }
            else if (alph == 1)
            {
                if (small)
                {
                    res += smallGer;
                }
                if (cap)
                {
                    res += capGer;
                }
            }
            if (spec)
            {
                res += specChars;
            }
            if (space)
            {
                res += " ";
            }
            return res;
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
    }
}
