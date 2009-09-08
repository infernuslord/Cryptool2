using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using Cryptool.PluginBase;

namespace Cryptool.Plugins.BinOperators
{
    class BinOperatorsSettings : ISettings
    {
        #region private variables
        private int operat = 0; // 0 =, 1 !=, 2 <, 3 >
        private bool hasChanges;
        #endregion

        #region taskpane
        [TaskPane("Operator", "Choose the operator", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "=", "!=", "<", ">" })]
        public int Operat
        {
            get { return this.operat; }
            set
            {
                if ((int)value != this.operat)
                {
                    this.operat = (int)value;
                    OnPropertyChanged("Operat");
                    HasChanges = true;

                    switch (operat)
                    {
                        case 0:
                            ChangePluginIcon(0);
                            break;
                        case 1:
                            ChangePluginIcon(1);
                            break;
                        case 2:
                            ChangePluginIcon(2);
                            break;
                        case 3:
                            ChangePluginIcon(3);
                            break;
                    }
                }
            }
        }
        #endregion

        #region ISettings Member

        public bool HasChanges
       {
            get { return hasChanges; }
            set { hasChanges = value; }
        
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            
        }
        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIcon(int Icon)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, Icon));
        }
        #endregion
    }
}
