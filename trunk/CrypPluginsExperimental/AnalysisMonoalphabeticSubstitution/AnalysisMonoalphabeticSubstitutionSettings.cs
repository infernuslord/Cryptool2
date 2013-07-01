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
 
        private const String specChars = ",;.:-_<>#+*!$%/{([)]}=?";
        // "Alphabet" variables
        private int bo_alphabet = 0;
        private Boolean bo_caseSensitive = false;
        // "PTAlphabet" variables
        private int pt_alphabet = 0;
        private Boolean pt_caseSensitive = false;
        // "CTAlphabet" variables
        private int ct_alphabet = 0;
        private Boolean ct_caseSensitive = false;
        // Advanced settings variables
        private Boolean separateAlphabets = false;

        #endregion

        #region Initialization / Constructor

        public void Initialize(){
            if (separateAlphabets==true){
                hideSettingsElement("boAlphabet");
                hideSettingsElement("boCaseSensitive");
                showSettingsElement("ptAlphabet");
                showSettingsElement("ptCaseSensitive");
                showSettingsElement("ctAlphabet");
                showSettingsElement("ctCaseSensitive");   
            }else{
                showSettingsElement("boAlphabet");
                showSettingsElement("boCaseSensitive");
                hideSettingsElement("ptAlphabet");
                hideSettingsElement("ptCaseSensitive");
                hideSettingsElement("ctAlphabet");
                hideSettingsElement("ctCaseSensitive");
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
            set
            {
                bo_alphabet = value;
                switch (bo_alphabet)
                {
                    case 0:
                    case 1:
                        showSettingsElement("boCaseSensitive");
                        break;
                    case 2:
                        hideSettingsElement("boCaseSensitive");
                        break;
                }
            }
        }
        [TaskPane("CaseSensitiveCaption", "CaseSensitiveTooltip", "AlphabetGroup", 2, false, ControlType.CheckBox, null)]
        public Boolean boCaseSensitive
        {
            get { return bo_caseSensitive; }
            set { bo_caseSensitive = value; }
        }

        // Settings for plaintext alphabet
        [TaskPane("PTChooseAlphabetCaption", "PTChooseAlphabetTooltip","PTAlphabetGroup", 1, false, ControlType.ComboBox,
            new string[] { "ChooseAlphabetList1", "ChooseAlphabetList2", "ChooseAlphabetList3" })]
        public int ptAlphabet
        {
            get { return pt_alphabet; }
            set
            {
                pt_alphabet = value;
                switch (pt_alphabet)
                {
                    case 0:
                    case 1:
                        showSettingsElement("ptCaseSensitive");
                        break;
                    case 2:
                        hideSettingsElement("ptCaseSensitive");
                        break;
                }
            }
        }
        [TaskPane("PTCaseSensitiveCaption", "PTCaseSensitiveTooltip", "PTAlphabetGroup", 2, false, ControlType.CheckBox, null)]
        public Boolean ptCaseSensitive
        {
            get { return pt_caseSensitive; }
            set { pt_caseSensitive = value; }
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
                        showSettingsElement("ctCaseSensitive");
                        break;
                    case 2:
                        hideSettingsElement("ctCaseSensitive");
                        break;
                }
            }
        }
        [TaskPane("CTCaseSensitiveCaption", "CTCaseSensitiveTooltip","CTAlphabetGroup", 2, false, ControlType.CheckBox,null)]
        public Boolean ctCaseSensitive
        {
            get { return ct_caseSensitive; }
            set { ct_caseSensitive = value; }
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
                    hideSettingsElement("boCaseSensitive");
                    showSettingsElement("ptAlphabet");
                    showSettingsElement("ptCaseSensitive");
                    showSettingsElement("ctAlphabet");
                    showSettingsElement("ctCaseSensitive");
                }
                else
                {
                    showSettingsElement("boAlphabet");
                    showSettingsElement("boCaseSensitive");
                    hideSettingsElement("ptAlphabet");
                    hideSettingsElement("ptCaseSensitive");
                    hideSettingsElement("ctAlphabet");
                    hideSettingsElement("ctCaseSensitive");
                }
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
