using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Analysis;
using System.ComponentModel;

namespace FriedmanTest
{
    class FriedmanTestSettings : ISettings
    {

        private bool hasChanges = false;
        private int kappa = 0; //0="English", 1="German", 2="French", 3="Spanish", 4="Italian",5="Portugeese"
        #region ISettings Members

        [ContextMenu("Kappa plain-text", "Select the language to be analysed", 2, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new String[] { "English", "German", "French", "Spanish", "Italian", "Portugeese" })]
        [TaskPane("Kappa plain-text", "Select the language to be analysed", null, 2, false, DisplayLevel.Experienced, ControlType.ComboBox, new String[] { "English", "German", "French", "Spanish", "Italian", "Portugeese" })]
        public int Kappa
        {
            get { return this.kappa; }
            set
            {
                if (((int)value) != kappa) hasChanges = true;
                this.kappa = (int)value;
                OnPropertyChanged("Kappa");
            }
        }

        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
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
