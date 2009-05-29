using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Security.Cryptography;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using System.Runtime.CompilerServices;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.Remoting.Contexts;

namespace Cryptool.HIGHT
{
    public class HIGHTSettings : ISettings
    {
        #region ISettings Members

        private bool hasChanges = false;
        private int action = 0; //0=encrypt, 1=decrypt
        private int padding = 0; //0="Zeros"=default, 1="None", 2="PKCS7"

        [ContextMenu("Action","Do you want the input data to be encrypted or decrypted?", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] { 1, 2}, "Encrypt","Decrypt")]
        [TaskPane("Action", "Do you want the input data to be encrypted or decrypted?", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public int Action
        {
            get { return this.action; }
            set
            {
                if (((int)value) != padding) hasChanges = true;
                this.action = (int)value;
            }
        }

        [ContextMenu("Padding mode", "Select a mode to fill partial data blocks.", 2, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, "Zeros", "None", "PKCS7")]
        [TaskPane("Padding Mode", "Select a mode to fill partial data blocks.", null, 2, false, DisplayLevel.Experienced, ControlType.ComboBox, new String[] { "Zeros", "None", "PKCS7" })]
        public int Padding
        {
            get { return this.padding; }
            set
            {
                if (((int)value) != padding) hasChanges = true;
                this.padding = (int)value;
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

        #endregion
    }
}
