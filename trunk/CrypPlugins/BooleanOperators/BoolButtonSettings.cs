using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing; 

using Cryptool.PluginBase;
using System.IO;
using System.ComponentModel;
using Cryptool.PluginBase.Cryptography;




namespace Cryptool.Plugins.BoolButton
{
   public class BoolButtonSettings : ISettings
    {
       #region ISettings Members

        private bool hasChanges = false;
        private int bool_value = 0; //0 false; 1 true

        

        [ContextMenu("Value", "Set the boolean value", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] { 0, 1 }, "False", "True")]
        [TaskPane("Value", "Set the boolean value", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "False", "True" })]
        public int Value
        {
            get { return this.bool_value; }
            set
            {
                if ((value) != bool_value) hasChanges = true;
                this.bool_value = value;
                OnPropertyChanged("Value");

                // icon update is handled by BooleanInput
            }
        }

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

        #endregion

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
    }
}

