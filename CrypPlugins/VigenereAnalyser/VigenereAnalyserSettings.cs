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
        [ContextMenu("ELFCaption", "ELFTooltip", 2, ContextMenuControlType.ComboBox, null, new String[] { "ELFList1", "ELFList2", "ELFList3", "ELFList4", "ELFList5", "ELFList6" })]
        [TaskPane("ELFCaption", "ELFTooltip", null, 2, false, ControlType.ComboBox, new String[] { "ELFList1", "ELFList2", "ELFList3", "ELFList4", "ELFList5", "ELFList6" })]
        public int ELF // Expected Letter Frequencies
        {
            get { return this.elf; }
            set
            {
                if (value != elf)
                {
                    this.elf = value;
                    OnPropertyChanged("ELF");   
                }
            }
        }
        [ContextMenu("EICCaption", "EICTooltip", 2, ContextMenuControlType.ComboBox, null, new String[] { "ELFList1", "ELFList2", "ELFList3", "ELFList4", "ELFList5", "ELFList6" })]
        [TaskPane("EICCaption", "EICTooltip", null, 2, false, ControlType.ComboBox, new String[] { "ELFList1", "ELFList2", "ELFList3", "ELFList4", "ELFList5", "ELFList6" })]
        public int EIC // Expected Letter Frequencies
        {
            get { return this.eic; }
            set
            {
                if (value != eic)
                {
                    this.eic = value;
                    OnPropertyChanged("EIC");   
                }
            }
        }

        [TaskPane("Max_KeylengthCaption", "Max_KeylengthTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 100)]
        public int Max_Keylength
        {
            get { return this.max_keylength; }
            set
            {
                if (value != max_keylength)
                {
                    max_keylength = value;
                    OnPropertyChanged("Max_Keylength");
                }
            }
        }
        [ContextMenu("InternalKeyLengthAnalysisCaption", "InternalKeyLengthAnalysisTooltip", 2, ContextMenuControlType.ComboBox, null, new String[] { "Kasiski and Friedman tests (external)", "Sampled index of coincidence(internal)" })]
        [TaskPane("InternalKeyLengthAnalysisCaption", "InternalKeyLengthAnalysisTooltip", null, 2, false, ControlType.ComboBox, new String[] { "Kasiski and Friedman tests (external)", "Sampled index of coincidence (internal)" })]
        public int InternalKeyLengthAnalysis 
        {
            get { return this.internalKeyLengthAnalysis; }
            set
            {
                if (value != internalKeyLengthAnalysis)
                {
                    this.internalKeyLengthAnalysis = value;
                    OnPropertyChanged("InternalKeyLengthAnalysis");   
                }
            }
        }
        [ContextMenu( "ColumnAnalysisCaption", "ColumnAnalysisTooltip", 2, ContextMenuControlType.ComboBox, null, new String[] { "ColumnAnalysisList1", "ColumnAnalysisList2" })]
        [TaskPane( "ColumnAnalysisCaption", "ColumnAnalysisTooltip", null, 2, false, ControlType.ComboBox, new String[] { "ColumnAnalysisList1", "ColumnAnalysisList2" })]
        public int ColumnAnalysis
        {
            get { return this.columnAnalysis; }
            set
            {
                if (value != columnAnalysis)
                {
                    this.columnAnalysis = value;
                    OnPropertyChanged("ColumnAnalysis");
                }
            }
        }

        private string text;
        public string Text
        {
            get { return text; }
            set
            {
                if (value != text)
                {
                    text = value;
                    OnPropertyChanged("Text");
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
        }

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
