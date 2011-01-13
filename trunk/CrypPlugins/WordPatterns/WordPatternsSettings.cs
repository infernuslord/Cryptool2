using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Windows.Data;

namespace WordPatterns
{
    enum Case
    {
        Sensitive,
        Insensitive
    }

    class WordPatternsSettings : ISettings
    {
        private bool hasChanges = false;

        private Case caseSelection = Case.Insensitive;

        [TaskPane("Case sensitivity", "Choose whether uppercase/lowercase should be treated as different (case sensitive) or equal (case insensitive)", "", 1, false, ControlType.ComboBox)]
        public Case CaseSelection
        {
            get { return caseSelection; }
            set
            {
                if (caseSelection != value)
                {
                    caseSelection = value;
                    hasChanges = true;
                    OnPropertyChanged("CaseSelection");
                }
            }
        }

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #region ISettings Members

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

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
