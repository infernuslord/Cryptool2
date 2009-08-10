using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using System.ComponentModel;


namespace Cryptool.VigenereAnalyser
{
    class VigenereAnalyserSettings : ISettings
    {
        #region ISettings Members
        private int elf = 0;
        public int internalKeyLengthAnalysis = 0;
        public int columnAnalysis = 0;
        private bool hasChanges = false;
        [ContextMenu("Expected Letter Frequency of a language", "Select the Null hypothesis for the Chi-square statistic", 2, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new String[] { "English", "German", "French", "Spanish", "Italian", "Portugeese" })]
        [TaskPane("Expected Letter Frequency of a language", "Select the Null hypothesis for the Chi-square statistic", null, 2, false, DisplayLevel.Experienced, ControlType.ComboBox, new String[] { "English", "German", "French", "Spanish", "Italian", "Portugeese" })]
        public int ELF // Expected Letter Frequencies
        {
            get { return this.elf; }
            set
            {
                if (((int)value) != elf) hasChanges = true;
                this.elf = (int)value;
                OnPropertyChanged("ELF");
            }
        }
        [ContextMenu("Method of keylength analysis", "Select the internal or external method for analysis of the keylength", 2, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new String[] { "Use Kasiski and Friedman tests (external)","Use Regression-Covariance test (internal)" })]
        [TaskPane("Method of keylength analysis", "Select the internal or external method for analysis of the keylength", null, 2, false, DisplayLevel.Experienced, ControlType.ComboBox, new String[] { "Use Kasiski and Friedman tests (external)", "Use Regression-Covariance test (internal)" })]
        public int InternalKeyLengthAnalysis 
        {
            get { return this.internalKeyLengthAnalysis; }
            set
            {
                if (((int)value) != internalKeyLengthAnalysis) hasChanges = true;
                this.internalKeyLengthAnalysis = (int)value;
                OnPropertyChanged("InternalKeyLengthAnalysis");
            }
        }
        [ContextMenu("Method of column analysis", "Select the method of analysis of the ciphertext columns", 2, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new String[] { "Use most frequent letter", "Use sum of squares" })]
        [TaskPane("Method of column analysis", "Select the method of analysis of the ciphertext columns", null, 2, false, DisplayLevel.Experienced, ControlType.ComboBox, new String[] { "Use most frequent letter", "Use sum of squares" })]
        public int ColumnAnalysis
        {
            get { return this.columnAnalysis; }
            set
            {
                if (((int)value) != columnAnalysis) hasChanges = true;
                this.columnAnalysis = (int)value;
                OnPropertyChanged("ColumnAnalysis");
            }
        }
        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; OnPropertyChanged("HasChanges"); }
        }
        private string text;
        public string Text
        {
            get { return text; }
            set
            {
                if (value != text) hasChanges = true;
                text = value;
            }
        }
        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
