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
        

        /// <summary>
        /// Visible setting how to deal with alphabet case. 0 = case insentive, 1 = case sensitive
        /// </summary>
        [PropertySaveOrder(1)]
        [ContextMenu("Fast Aproach", "Using Fast Aproach dramatically reduce the time needed as only two decryption attempts are made. ", 7, DisplayLevel.Expert, ContextMenuControlType.ComboBox, null, new string[] { "Don't use Fast Aproach", "Use Fast Aproach" })]
        [TaskPane("Fast Aproach", "Using Fast Aproach dramatically reduce the time needed as only two decryption attempts are made. ", "", 7, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] { "Don't Use Fast Aproach", "Use Fast Aproach" })]
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
