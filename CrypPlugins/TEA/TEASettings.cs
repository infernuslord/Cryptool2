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

namespace Cryptool.TEA
{
    public class TEASettings : ISettings
    {
        #region ISettings Members

        private bool hasChanges = false;
        private int action = 0; //0=encrypt, 1=decrypt
        private int padding = 0; //0="Zeros"=default, 1="None", 2="PKCS7"
        private int version = 0; //0="TEA"=default, 1="XTEA"
        private int rounds = 64;

        [ContextMenu("Action","Do you want the input data to be encrypted or decrypted?",1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] { 1, 2}, "Encrypt","Decrypt")]
        [TaskPane("Action", "Do you want the input data to be encrypted or decrypted?", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public int Action
        {
            get { return this.action; }
            set { this.action = (int)value; }
        }

        [ContextMenu("Padding mode", "Select a mode to fill partial data blocks.", 3, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, "Zeros", "None", "PKCS7")]
        [TaskPane("Padding Mode", "Select a mode to fill partial data blocks.", "", 3, false, DisplayLevel.Experienced, ControlType.ComboBox, new String[] { "Zeros", "None", "PKCS7" })]
        public int Padding
        {
            get { return this.padding; }
            set
            {
                if (((int)value) != padding) hasChanges = true;
                this.padding = (int)value;
                //OnPropertyChanged("Padding");
            }
        }

        [ContextMenu("TEA version", "Select the version of TEA you want to use.", 4, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, "TEA", "XTEA")]
        [TaskPane("TEA version", "Select the version of TEA you want to use.", "", 4, false, DisplayLevel.Experienced, ControlType.ComboBox, new String[] { "TEA (1994)", "XTEA (1997)" })]
        public int Version
        {
            get { return this.version; }
            set
            {
                if (((int)value) != version) hasChanges = true;
                this.version = (int)value;
                //OnPropertyChanged("Padding");
            }
        }

        [TaskPane("Number of rounds", "This applies only to XTEA. Default are 64 rounds.", "Rounds", 5, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int Rounds
        {
            get { return this.rounds; }
            set
            {
                if (((int)value) != rounds) hasChanges = true;
                this.rounds = value;
                //OnPropertyChanged("Rounds");
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
