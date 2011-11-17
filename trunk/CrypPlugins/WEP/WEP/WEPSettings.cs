using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cryptool.PluginBase;
using System.Security.Cryptography;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.WEP
{
    /// <summary>
    /// Settings for the WEP plugins.
    /// You can choose between encryption and decryption, between saving to file and not,
    /// and you can set the number how many packets are going to be saved.
    /// </summary>
    public class WEPSettings : ISettings
    {
        #region Private variables

        private int action = 0;

        /// <summary>
        /// Encryption (=0) or decryption (=1)?
        /// </summary>
        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "ActionList1", "ActionList2")]
        [TaskPane("ActionCaption", "ActionTooltip", "", 1, false, ControlType.ComboBox, new String[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get { return this.action; }
            set
            {
                if ((int)value != action)
                {
                    this.action = (int)value;
                    OnPropertyChanged("Action");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIncon(int Icon)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, Icon));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
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
