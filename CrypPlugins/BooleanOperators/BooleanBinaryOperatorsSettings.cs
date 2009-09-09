using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Cryptool;
using Cryptool.PluginBase;


namespace Cryptool.Plugins.BooleanOperators
{
    class BooleanBinaryOperatorsSettings : ISettings
    {
        private int operatorType = 0;
        /* 0 = AND
         * 1 = OR
         * 2 = NAND
         * 3 = NOR
         * 4 = XOR
         */

        #region ISettings Members        
        public bool HasChanges
        {
            get
            {
                return false;
                //throw new NotImplementedException();
            }
            set
            {
                //throw new NotImplementedException();
            }
        }

        [ContextMenu("Operator Type", "Operator Type", 0, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new string[] { "AND", "OR", "NAND", "NOR", "XOR" })]
        [TaskPane("Operator Type", "Operator Type", null, 2, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "AND", "OR", "NAND", "NOR", "XOR" })]
        public int OperatorType
        {
            get { return this.operatorType; }
            set
            {
                this.operatorType = value;
                OnPropertyChanged("OperatorType");
                ChangePluginIcon(value);                
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIcon(int Icon)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, Icon));
        }
    }
}
