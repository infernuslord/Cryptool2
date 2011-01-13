using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cryptool.PluginBase;
using System.Security.Cryptography;
using System.ComponentModel;

namespace Cryptool.Plugins.Cryptography.Encryption
{
    public class TripleDESSettings : ISettings
    {
        private bool hasChanges = false;
        private int action = 0; //0=encrypt, 1=decrypt
        private int mode = 0; //0="ECB", 1="CBC", 2="CFB", 3="OFB"
        private int padding = 0; //0="Zeros"=default, 1="None", 2="PKCS7", 3="ANSIX923", 4="ISO10126

        [ContextMenu("Action","Do you want the input data to be encrypted or decrypted?",1,ContextMenuControlType.ComboBox,new int[] {1,2},"Encrypt","Decrypt")]
        [TaskPane("Action", "Do you want the input data to be encrypted or decrypted?", null, 1, false,ControlType.ComboBox, new string[] {"Encrypt","Decrypt"})]
        public int Action
        {
            get { return this.action; }
            set
            {
                if (((int)value) != action) hasChanges = true;
                this.action = (int)value;
                OnPropertyChanged("Action");
            }
        }

        [ContextMenu("Chaining mode","Select the block cipher mode of operation.",2,ContextMenuControlType.ComboBox, null, new string[] {"Electronic Code Book (ECB)","Cipher Block Chaining (CBC)","Cipher Feedback (CFB)"})]
        [TaskPane("Chaining mode", "Select the block cipher mode of operation.","", 2, false, ControlType.ComboBox, new string[] {"Electronic Code Book (ECB)","Cipher Block Chaining (CBC)","Cipher Feedback (CFB)"})]
        public int Mode
        {
            get { return this.mode; }
            set
            {
                if (((int)value) != mode) hasChanges = true;
                this.mode = (int)value;
                OnPropertyChanged("Mode");
            }
        }

        [ContextMenu("Padding mode","Select a mode to fill partial data blocks.",3, ContextMenuControlType.ComboBox, null,"Zeros","None","PKCS7","ANSIX923","ISO10126")]
        [TaskPane("Padding mode", "Select a mode to fill partial data blocks.", "", 3, false,ControlType.ComboBox, new String[]{"Zeros","None","PKCS7","ANSIX923","ISO10126"})]
        public int Padding
        {
            get { return this.padding; }
            set
            {
                if (((int)value) != padding) hasChanges = true;
                this.padding = (int)value;
                OnPropertyChanged("Padding");
            }
        }

        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

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

        public event StatusChangedEventHandler OnPluginStatusChanged;

        private void ChangePluginIcon(int Icon)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, Icon));
        }
    }
}
