using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using Cryptool.PluginBase;

namespace Cryptool.Plugins.Comparators
{
    class ComparatorsSettings : ISettings
    {
        #region private variables
        private int comparator = 0; // 0 =, 1 !=, 2 <, 3 >, 4 <=, 5 >=
        private bool hasChanges;
        #endregion

        #region taskpane
        [TaskPane("Comparator", "Choose the Comperator", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "=", "!=", "<", ">", "<=", ">=" })]
        public int Comparator
        {
            get { return this.comparator; }
            set
            {
                if ((int)value != this.comparator)
                {
                    this.comparator = (int)value;
                    OnPropertyChanged("Comparator");
                    HasChanges = true;

                    switch (comparator)
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
                        case 4:
                            ChangePluginIcon(4);
                            break;
                        case 5:
                            ChangePluginIcon(5);
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
