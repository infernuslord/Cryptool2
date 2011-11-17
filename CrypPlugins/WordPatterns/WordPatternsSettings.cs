using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Windows.Data;

namespace WordPatterns
{
    public enum Case
    {
        Sensitive,
        Insensitive
    }

    public class WordPatternsSettings : ISettings
    {
        private Case caseSelection = Case.Insensitive;

        [TaskPane("CaseSelectionCaption", "CaseSelectionTooltip", "", 1, false, ControlType.ComboBox, new string[] { "CaseSelectionList1", "CaseSelectionList2" })]
        public Case CaseSelection
        {
            get { return caseSelection; }
            set
            {
                if (caseSelection != value)
                {
                    caseSelection = value;
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

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
