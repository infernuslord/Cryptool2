using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.MonoalphabeticAnalysis
{
    public class MonoalphabeticAnalysisSettings : ISettings
    {
        #region ISettings Members

        private bool hasChanges;
        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        #endregion
        
        
        private int fastAproach = 0;
        

        
        [PropertySaveOrder(1)]
        [ContextMenu("Generate digram matrix internally", "When the digram matrix is generated internally, the time for calculating the cost function is significantly reduced. ", 7, DisplayLevel.Expert, ContextMenuControlType.ComboBox, null, new string[] { "Don't generate internally", "Generate internally" })]
        [TaskPane("Digram matrix", "When the digram matrix is generated internally, the time for calculating the cost function is significantly reduced.", "", 7, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] { "Don't generate internally", "Generate internally" })]
        public int FastAproach
        {
            get { return this.fastAproach; }
            set
            {
                if (value != fastAproach)
                {
                    HasChanges = true;
                    fastAproach = value;
                }

                OnPropertyChanged("Fast Aproach");
            }
        }
        /*
        private int caseSensitivity = 0;

        [PropertySaveOrder(2)]
        [ContextMenu("Case Sensitivity", "If Frequency Test is feeding case sensitive analysis as input the setting should be set.", 7, DisplayLevel.Expert, ContextMenuControlType.ComboBox, null, new string[] { "Case Insensitive", "Case Sensitive" })]
        [TaskPane("Case Sensitivity", "If Frequency Test is feeding case sensitive analysis as input the setting should be set.", "", 7, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] { "Case Insensitive", "Case Sensitive" })]
        public int CaseSensitivity
        {
            get { return this.caseSensitivity; }
            set
            {
                if (value != caseSensitivity)
                {
                    HasChanges = true;
                    caseSensitivity = value;
                }

                OnPropertyChanged("Case Sensitivity");
            }
        }

        private int language = 0;

        [PropertySaveOrder(3)]
        [ContextMenu("Language", "Choose expected language of cipher text to use the correlated built-in statistic. The option is applicable only when the inputs for frequency statistic data are not used. ", 7, DisplayLevel.Expert, ContextMenuControlType.ComboBox, null, new string[] { "English", "German", "French", "Spanish" })]
        [TaskPane("Language", "Choose expected language of cipher text to use the correlated built-in statistic. The option is applicable only when the inputs for frequency statistic data are not used. ", "", 7, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] { "English", "German", "French", "Spanish" })]
        public int Language
        {
            get { return this.language; }
            set
            {
                if (value != language)
                {
                    HasChanges = true;
                    language = value;
                }

                OnPropertyChanged("Language");
            }
        }   */
       

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
