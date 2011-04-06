using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.IO;
using System.ComponentModel;
using Cryptool.PluginBase.Cryptography;

namespace Cryptool.Plugins.Cryptography.Encryption
{
    public class DESSettings : ISettings
    {
        private bool hasChanges = false;
        private int action = 0; //0=encrypt, 1=decrypt
        private int mode = 0; //0="ECB", 1="CBC", 2="CFB", 3="OFB"
        private int padding = 0; //0="Zeros"=default, 1="None", 2="PKCS7", 3="ANSIX923", 4="ISO10126"
        
        [ContextMenu( "ActionCaption", "ActionTooltip",1, ContextMenuControlType.ComboBox, new int[] { 1, 2}, "Encrypt","Decrypt")]
        [TaskPane( "ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
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

        [ContextMenu( "ModeCaption", "ModeTooltip",2,ContextMenuControlType.ComboBox,null, new String[] {"Electronic Code Book (ECB)","Cipher Block Chaining (CBC)","Cipher Feedback (CFB)"})]
        [TaskPane( "ModeTPCaption", "ModeTPTooltip", null, 2, false, ControlType.ComboBox, new String[] { "Electronic Bode Book (ECB)","Cipher Block Chaining (CBC)","Cipher Feedback (CFB)" })]
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

        [ContextMenu( "PaddingCaption", "PaddingTooltip",3, ContextMenuControlType.ComboBox,null,"Zeros","None","PKCS7","ANSIX923","ISO10126")]
        [TaskPane( "PaddingTPCaption", "PaddingTPTooltip", "", 3, false, ControlType.ComboBox, new String[] { "Zeros", "None", "PKCS7", "ANSIX923","ISO10126" })]
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

    }
}
