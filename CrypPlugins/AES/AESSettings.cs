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

        [ContextMenu("CryptoAlgorithmCaption", "CryptoAlgorithmTooltip", 1, ContextMenuControlType.ComboBox, null, "CryptoAlgorithmList1", "CryptoAlgorithmList2")]
        [TaskPane("CryptoAlgorithmCaption", "CryptoAlgorithmTooltip", "", 0, false, ControlType.ComboBox, new string[] { "CryptoAlgorithmList1", "CryptoAlgorithmList2" })]
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

        [ContextMenu("ActionCaption", "ActionTooltip", 2, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "ActionList1", "ActionList2")]
        [TaskPane("ActionCaption", "ActionTooltip", "", 2, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
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


        [ContextMenu("KeysizeCaption", "KeysizeTooltip", 3, ContextMenuControlType.ComboBox, null, "KeysizeList1", "KeysizeList2", "KeysizeList3")]
        [TaskPane("KeysizeCaption", "KeysizeTooltip", "", 3, false, ControlType.ComboBox, new String[] { "KeysizeList1", "KeysizeList2", "KeysizeList3" })]
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

        public int KeysizeAsBits
        {
            get
            {
                switch (this.keysize)
                {
                    case 0:
                        return 16 * 8;
                    case 1:
                        return 24 * 8;
                    case 2:
                        return 32 * 8;
                    default:
                        throw new InvalidOperationException("Selected keysize entry unknown: " + this.keysize);
                }   
            }
        }


        [ContextMenu("BlocksizeCaption", "BlocksizeTooltip", 4, ContextMenuControlType.ComboBox, null, "BlocksizeList1", "BlocksizeList2", "BlocksizeList3")]
        [TaskPane("BlocksizeCaption", "BlocksizeTooltip", "", 4, false, ControlType.ComboBox, new String[] { "BlocksizeList1", "BlocksizeList2", "BlocksizeList3" })]
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

        public int BlocksizeAsBytes
        {
            get
            {
                switch (this.cryptoAlgorithm)
                {
                    case 0:
                        return 16;
                    case 1:
                        switch (this.blocksize)
                        {
                            case 0:
                                return 16;
                            case 1:
                                return 24;
                            case 2:
                                return 32;
                            default:
                                throw new InvalidOperationException("Selected blocksize entry unknown: " + this.blocksize);
                        }
                    default:
                        throw new InvalidOperationException("Selected algorithm entry unknown: " + this.cryptoAlgorithm);
                }
            }
        }

        [ContextMenu("ModeCaption", "ModeTooltip", 5, ContextMenuControlType.ComboBox, null, new String[] { "ModeList1", "ModeList2", "ModeList3" })]
        [TaskPane("ModeCaption", "ModeTooltip", "", 5, false, ControlType.ComboBox, new String[] { "ModeList1", "ModeList2", "ModeList3" })]
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

        [ContextMenu("PaddingCaption", "PaddingTooltip", 6, ContextMenuControlType.ComboBox, null, "PaddingList1", "PaddingList2", "PaddingList3", "PaddingList4", "PaddingList5")]
        [TaskPane("PaddingCaption", "PaddingTooltip", "", 6, false, ControlType.ComboBox, new String[] { "PaddingList1", "PaddingList2", "PaddingList3", "PaddingList4", "PaddingList5" })]
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
