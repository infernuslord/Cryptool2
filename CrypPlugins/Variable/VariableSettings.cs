using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.Plugins.Variable
{
    class VariableSettings : ISettings
    {
        #region Variable Name
        private String variableName = "";
        [TaskPane("Variable Name", "The variable this component is linked with.", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String VariableName
        {
            get { return variableName; }
            set
            {
                variableName = (String)value;
                OnPropertyChanged("VariableName");
            }
        }
        #endregion
                        
        #region ISettings Members

        private bool hasChanges;
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

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(p));
        }

        #endregion
    }
}
