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
        
        private bool hasChanges = false;
        private int alphabet = 0;
        private int treatmentInvalidChars = 0;

        #endregion

        #region Initialization / Constructor

        public void Initialize()
        {
 
        }

        #endregion

        #region TaskPane Settings

        [TaskPane("ChooseAlphabetCaption", "ChooseAlphabetTooltip", "AlphabetGroup", 1, false, ControlType.ComboBox,
            new string[] { "ChooseAlphabetList1"})] //Add new value for another language, do it in the resource files to have multi-language support
        public int Alphabet
        {
            get { return alphabet; }
            set { alphabet = value; }
        }
 
        [TaskPane("TreatmentInvalidCharsCaption", "TreatmentInvalidCharsTooltip", "AdvancedSettingsGroup", 2, false, ControlType.ComboBox, new string[] { "ChooseInvalidCharsList1","ChooseInvalidCharsList2", "ChooseInvalidCharsList3"})]
        public int TreatmentInvalidChars
        {
            get { return treatmentInvalidChars; }
            set
            {
                treatmentInvalidChars = value;
                OnPropertyChanged("TreatmentInvalidChars");
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
