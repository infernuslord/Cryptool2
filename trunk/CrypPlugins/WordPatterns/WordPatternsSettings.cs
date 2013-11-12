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
        private string separators = "";

        [TaskPane("CaseSelectionCaption", "CaseSelectionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "CaseSelectionList1", "CaseSelectionList2" })]
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
        
        /// <summary>
        /// Separator characters used to split the input
        /// </summary>
        [TaskPane("SeparatorsSettingCaption", "SeparatorsSettingTooltip", null, 4, false, ControlType.TextBox)]
        public string Separators
        {
            get
            {
                return this.separators;
            }
            set
            {
                if (this.separators != value)
                {
                    this.separators = value;
                    OnPropertyChanged("Separators");
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
        public void Initialize()
        {
            
        }

        #endregion
    }
}
