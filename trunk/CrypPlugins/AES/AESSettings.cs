/*
   Copyright 2008 Dr. Arno Wacker, University of Duisburg-Essen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cryptool.PluginBase;
using System.Security.Cryptography;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.Cryptography.Encryption
{
    public class AESSettings : ISettings
    {
        private bool hasChanges = false;
        private int action = 0; //0=encrypt, 1=decrypt
        private int cryptoAlgorithm = 0; // 0=AES, 1=Rijndael
        private int blocksize = 0; // 0=128, 1=192, 2=256
        private int keysize = 0; // 0=128, 1=192, 2=256
        private int mode = 0; //0="ECB", 1="CBC", 2="CFB", 3="OFB"
        private int padding = 0; ////0="Zeros"=default, 1="None", 2="PKCS7" , 3="ANSIX923", 4="ISO10126"

        [ContextMenu("Cryptographic algorithm", "Select which symmetric cipher you want to use", 1, ContextMenuControlType.ComboBox, null, "Advanced Encryption Standard (AES)", "Rijndael")]
        [TaskPane("Cryptographic algorithm", "Select which symmetric cipher you want to use", "", 0, false, ControlType.ComboBox, new string[] { "Advanced Encryption Standard (AES)", "Rijndael" })]
        public int CryptoAlgorithm
        {
            get { return this.cryptoAlgorithm; }
            set
            {
                if (((int)value) != cryptoAlgorithm) hasChanges = true;
                this.cryptoAlgorithm = (int)value;
                if (cryptoAlgorithm == 0)
                {
                    blocksize = 0;
                    OnPropertyChanged("Blocksize");
                }
                OnPropertyChanged("CryptoAlgorithm");

                switch (cryptoAlgorithm)
                {
                  case 0:
                    ChangePluginIcon(0);
                    break;
                  case 1:
                    ChangePluginIcon(3);
                    break;
                  default:
                    break;
                }
            }
        }

        [ContextMenu("Action", "Do you want the input data to be encrypted or decrypted?", 2, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "Encrypt", "Decrypt")]
        [TaskPane("Action", "Do you want the input data to be encrypted or decrypted?", "", 2, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
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


        [ContextMenu("Keysize", "Select the key size. Note that providing a longer key will result in cutting the overlapping bytes, providing a shorter key will result in filling up with zeroes.", 3, ContextMenuControlType.ComboBox, null, "128 Bits", "192 Bits", "256 Bits")]
        [TaskPane("Keysize", "Select the key size. Note that providing a longer key will result in cutting the overlapping bytes, providing a shorter key will result in filling up with zeroes.", "", 3, false, ControlType.ComboBox, new String[] { "128 Bits", "192 Bits", "256 Bits" })]
        public int Keysize
        {
            get { return this.keysize; }
            set
            {
                if (((int)value) != keysize) hasChanges = true;
                this.keysize = (int)value;
                OnPropertyChanged("Keysize");
            }
        }


        [ContextMenu("Blocksize", "Select the block size (applies only to Rijndael)", 4, ContextMenuControlType.ComboBox, null, "128 Bits", "192 Bits", "256 Bits")]
        [TaskPane("Blocksize", "Select the block size (applies only to Rijndael)", "", 4, false, ControlType.ComboBox, new String[] { "128 Bits", "192 Bits", "256 Bits" })]
        public int Blocksize
        {
            get { return this.blocksize; }
            set
            {
                if (((int)value) != blocksize) hasChanges = true;
                this.blocksize = (int)value;
                if (blocksize > 0)
                {
                    cryptoAlgorithm = 1;
                    OnPropertyChanged("CryptoAlgorithm");
                }
                OnPropertyChanged("Blocksize");
            }
        }

        [ContextMenu("Chaining mode", "Select the block cipher mode of operation.", 5, ContextMenuControlType.ComboBox, null, new String[] { "Electronic Code Book (ECB)", "Cipher Block Chaining (CBC)", "Cipher Feedback (CFB)" })]
        [TaskPane("Chaining mode", "Select the block cipher mode of operation.", "", 5, false, ControlType.ComboBox, new String[] { "Electronic Code Book (ECB)", "Cipher Block Chaining (CBC)", "Cipher Feedback (CFB)" })]
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

        [ContextMenu("Padding mode", "Select a mode to fill partial data blocks.", 6, ContextMenuControlType.ComboBox, null, "Zeros", "None", "PKCS7", "ANSIX923", "ISO10126")]
        [TaskPane("Padding mode", "Select a mode to fill partial data blocks.", "", 6, false, ControlType.ComboBox, new String[] { "Zeros", "None", "PKCS7", "ANSIX923", "ISO10126" })]
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
          EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        public event StatusChangedEventHandler OnPluginStatusChanged;

        private void ChangePluginIcon(int Icon)
        {
          if (OnPluginStatusChanged != null) OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, Icon));
        }
    }
}
