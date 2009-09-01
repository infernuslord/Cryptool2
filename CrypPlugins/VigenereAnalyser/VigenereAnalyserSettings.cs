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
        private int max_keylength=15;
        private int elf = 0;
        private int eic = 0; 
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
        [ContextMenu("Expected Index of coincidence", "Adjust the expected index of coincidence", 2, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new String[] { "English", "German", "French", "Spanish", "Italian", "Portugeese" })]
        [TaskPane("Expected Index of coincidence", "Adjust the expected index of coincidence", null, 2, false, DisplayLevel.Experienced, ControlType.ComboBox, new String[] { "English", "German", "French", "Spanish", "Italian", "Portugeese" })]
        public int EIC // Expected Letter Frequencies
        {
            get { return this.eic; }
            set
            {
                if (((int)value) != eic) hasChanges = true;
                this.eic = (int)value;
                OnPropertyChanged("EIC");
            }
        }

        [TaskPane("Maximum Keylength", "Enter maximum keylength to be analysed by the 'Sampled Index of coincidence' method. default = 15", "", 1, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 100)]
        public int Max_Keylength
        {
            get { return this.max_keylength; }
            set
            {
                if (value != max_keylength)
                {
                    HasChanges = true;
                    max_keylength = value;
                }
            }
        }
        [ContextMenu("Method of keylength analysis", "Select the internal or external method for analysis of the keylength", 2, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new String[] { "Kasiski and Friedman tests (external)","Sampled index of coincidence(internal)" })]
        [TaskPane("Method of keylength analysis", "Select the internal or external method for analysis of the keylength", null, 2, false, DisplayLevel.Experienced, ControlType.ComboBox, new String[] { "Kasiski and Friedman tests (external)", "Sampled index of coincidence (internal)" })]
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
