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
