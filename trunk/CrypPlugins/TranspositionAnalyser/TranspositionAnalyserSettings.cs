using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace TranspositionAnalyser
{
    class TranspositionAnalyserSettings : ISettings
    {
        private bool has_Changes = false;

        #region ISettings Member

        private int selectedAction = 0;
        public enum ActionMode { costfunction = 0, crib = 1 };
        [PropertySaveOrder(1)]
        [ContextMenu("Action", "Select the analyse algorithm", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new string[] { "Bruteforce with Cost function", "Bruteforce for crib"})]
        [TaskPane("Action", "Select the analyse algorithm", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Bruteforce with Cost function", "Bruteforce for crib" })]
        public int Action
        {
            get { return this.selectedAction; }
            set
            {
                if (value != selectedAction) HasChanges = true;
                this.selectedAction = value;
                OnPropertyChanged("Action");
            }
        }

        // FIX: REGEX 
        private int bruteforce_length = 8;
        [PropertySaveOrder(2)]
        [TaskPaneAttribute("Transposition Bruteforce length", "Enter the max length to be bruteforced", null, 2, true, DisplayLevel.Beginner, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int MaxLength
        {
            get { return bruteforce_length; }
            set
            {
                bruteforce_length = value;
            }
        }


        private String crib = "";
        [PropertySaveOrder(3)]
        [TaskPaneAttribute("Crib:", "Enter the crib to be searched for", null, 3, true, DisplayLevel.Beginner, ControlType.TextBox)]
        public String Crib
        {
            get { return crib; }
            set
            {
                crib = value;
            }
        }

        public bool HasChanges
        {
            get
            {
                return has_Changes;
            }
            set
            {
                has_Changes = true;
            }
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
