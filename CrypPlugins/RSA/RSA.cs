﻿/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Cryptool.Plugins.RSA
{
    [Author("Dennis Nolte", "nolte@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "RSA", "RSA En/Decryption", "", "RSA/iconrsa.png", "RSA/iconrsa.png", "RSA/iconrsa.png")]

    [EncryptionType(EncryptionType.Asymmetric)]

    class RSA : IEncryption
    {
        #region IPlugin Members

        private RSASettings settings = new RSASettings();
        private BigInteger inputN = new BigInteger(1);
        private BigInteger inputmc = new BigInteger(1);
        private BigInteger inputed = new BigInteger(1);
        private BigInteger outputmc = new BigInteger(1);
        private byte[] inputText = null;
        private byte[] outputText = null;

        private bool stopped = false;

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (RSASettings)value; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            this.stopped = false;
        }

        public void Execute()
        {
            
            //calculate the BigIntegers
            try{
                
                this.OutputMC = InputMC.modPow(this.InputED, this.InputN);                
            }
            catch (Exception ex)
            {
                GuiLogMessage("RSA could not work because of: " + ex.Message, NotificationLevel.Error);                
            }

            //Calculate the OutputText
            if (this.InputText != null)
            {
                GuiLogMessage("starting RSA on texts", NotificationLevel.Info);

                //calculate block size from N                
                int blocksize = (int)Math.Ceiling(this.InputN.log(256));

                if (blocksize == 0)
                {
                    GuiLogMessage("Blocksize 0 - RSA can not work", NotificationLevel.Error);
                    return;
                }

                //calculate amount of blocks and the difference between the input text
                //and the blocked input text
                int blockcount = (int)Math.Ceiling((double)this.InputText.Length / blocksize);
                int difference = (blocksize - (this.InputText.Length % blocksize)) % blocksize;
               
                //Generate input and output array of correct block size
                byte[] output = new byte[blocksize * blockcount];           
                byte[] input = new byte[blocksize * blockcount];
                
                //Copy Input to input array and leave an amount of 'different' zeros at beginning of the array
                for (int i = 0; i < InputText.Length; i++)
                {
                    input[difference + i] = InputText[i];
                    if (stopped)
                        return;
                }
                
                //encrypt/decrypt each block
                for (int i = 0; i < blockcount; i++) //walk over the blocks
                {
                    //create a big integer from a block
                    byte[] help = new byte[blocksize];                    
                    for (int j = 0; j < blocksize; j++)
                    {
                        if(i * blocksize + j < input.Length)
                            help[j] = input[i * blocksize + j];
                        if (stopped)
                            return;

                    }
                    //Check if the text could be encrypted/decrypted
                    //this is only possible if the m < N
                    BigInteger bint = new BigInteger(help);
                    if (bint > this.InputN)
                    {
                        //Go out with an error because encryption/decryption is not possible
                        GuiLogMessage("The N is not suitable for encrypting this text: M = " + new BigInteger(help) + " > N = " + this.InputN + ". Choose another pair of primes!", NotificationLevel.Warning);
                    }
                    
                    //here we encrypt/decrypt with rsa algorithm
                    bint = bint.modPow(this.InputED, this.InputN);
                  
                    //create a block from the byte array of the BigInteger
                    byte[] bytes = bint.getBytes();
                    int diff = (blocksize - (bytes.Length % blocksize)) % blocksize;
                     
                    for (int j = 0; j < bytes.Length; j++)
                    {
                        output[i * blocksize + j + diff] = bytes[j];
                        if (stopped)
                            return;
                    }

                    if (stopped)
                        return;

                    ProgressChanged((double)i / blockcount, 1.0);
                
                }//end for i
                
                ProgressChanged(1.0, 1.0);
                this.OutputText = output;
            }
            
        }

        public void PostExecution()
        {
            
        }

        public void Pause()
        {
           
        }

        public void Stop()
        {
            this.stopped = true;
        }

        public void Initialize()
        {
            
        }

        public void Dispose()
        {
            
        }

        #endregion

        #region RSAInOut

        [PropertyInfo(Direction.InputData, "Public Key / Private Key N Input", "Input your Public Key / Private key N here", "", DisplayLevel.Beginner)]
        public BigInteger InputN
        {
            get
            {
                return inputN;
            }
            set
            {
                this.inputN = value;
                OnPropertyChanged("InputN");
            }
        }

        [PropertyInfo(Direction.InputData, "Message M / Ciphertext C Input", "Input your Message M / Ciphertext C here", "", DisplayLevel.Beginner)]
        public BigInteger InputMC
        {
            get
            {
                return inputmc;
            }
            set
            {
                this.inputmc = value;
                OnPropertyChanged("InputMC");
            }
        }

        [PropertyInfo(Direction.InputData, "Public Key E / Private Key D input", "Input your public Key E / Private Key D here", "", DisplayLevel.Beginner)]
        public BigInteger InputED
        {
            get
            {
                return inputed;
            }
            set
            {
                this.inputed = value;
                OnPropertyChanged("InputED");
            }
        }
        
        [PropertyInfo(Direction.OutputData, "Cipher C Output / Message M Output", "Your Cipher C / Message M will be send here", "", DisplayLevel.Beginner)]
        public BigInteger OutputMC
        {
            get
            {
                return outputmc;
            }
            set
            {
                this.outputmc = value;
                OnPropertyChanged("OutputMC");
            }
        }

        [PropertyInfo(Direction.InputData, "Text Input", "Input your Text here", "", DisplayLevel.Beginner)]
        public byte[] InputText
        {
            get
            {
                return inputText;
            }
            set
            {
                this.inputText = value;
                //GuiLogMessage("InputText: " + (int)inputText[0] + " " + (int)inputText[1] + " " + (int)inputText[2] + " " + (int)inputText[3] + " ", NotificationLevel.Info);
                OnPropertyChanged("InputText");
            }
        }

        [PropertyInfo(Direction.OutputData, "Text Output", "Your Text will be send here", "", DisplayLevel.Beginner)]
        public byte[] OutputText
        {
            get
            {
                return outputText;
            }
            set
            {
                this.outputText = value;
                //GuiLogMessage("OutputText: " + (int)outputText[0] + " " +(int)outputText[1] + " "+(int)outputText[2] + " "+(int)outputText[3] + " ", NotificationLevel.Info);
                OnPropertyChanged("OutputText");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members


        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event PluginProgressChangedEventHandler OnPluginProcessChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion
    }
}
