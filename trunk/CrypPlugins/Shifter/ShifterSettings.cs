using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using Cryptool.PluginBase;

namespace Cryptool.Plugins.Shifter
{
    class ShifterSettings : ISettings
    {
        #region private variables
        private int operand = 0; // 0 = left, 1 = right
        private int amount = 0; 
        private bool hasChanges;
        #endregion

        #region taskpane
        [TaskPane("Operator", "Choose to shift left or right", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "left", "right"})]
        public int Operand
        {
            get { return this.operand; }
            set
            {
                if ((int)value != this.operand)
                {
                    this.operand = (int)value;
                    OnPropertyChanged("Operand");
                    HasChanges = true;

                    switch (operand)
                    {
                        case 0:
                            ChangePluginIcon(0);
                            break;
                        case 1:
                            ChangePluginIcon(1);
                            break;
                    }
                }
            }
        }

        [TaskPane("Amount", "Choose how often", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public int Amount
        {
            get { return this.amount; }
            set
            {
                if ((int)value != this.amount)
                {
                    this.amount = (int)value;
                    OnPropertyChanged("Amount");
                    HasChanges = true;

                  
                }
            }
        }

        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }

        }
        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIcon(int Icon)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, Icon));
        }
        protected void OnPropertyChanged(string name)
        {

        }
        #endregion
    }
}
