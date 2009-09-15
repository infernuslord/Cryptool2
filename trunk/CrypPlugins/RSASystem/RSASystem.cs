/*                              
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

namespace Cryptool.Plugins.RSASystem
{
    [Author("Dennis Nolte", "nolte@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "RSASystem", "RSA En/Decryption", "", "RSASystem/icon.png")]

    [EncryptionType(EncryptionType.Asymmetric)]

    class RSASystem : IEncryption
    {
        #region IPlugin Members

        private RSASystemSettings settings = new RSASystemSettings();
        private BigInteger inputM = new BigInteger(1);
        private BigInteger inputP = new BigInteger(1);
        private BigInteger inputQ = new BigInteger(1);
        private BigInteger inputE = new BigInteger(1);
        private BigInteger outputC = new BigInteger(1);
        
        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (RSASystemSettings)value; }
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
           
        }

        public void Execute()
        {
            // encrypt
            if (this.settings.Action == 0)
            {
                BigInteger N = this.InputP * this.InputQ;                
                this.OutputC = this.InputM.modPow(this.InputE, N);
            }
            //decrypt
            else
            {
                BigInteger N = this.InputP * this.InputQ;
                BigInteger PhiN = (this.InputP - 1) * (this.InputQ - 1);
                BigInteger d = this.InputE.modInverse(PhiN);
                this.OutputC = this.InputM.modPow(d, N);
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
            
        }

        public void Initialize()
        {
            
        }

        public void Dispose()
        {
            
        }

        #endregion

        #region RSASystemInOut

        [PropertyInfo(Direction.InputData, "Message M Input", "Input your Message M here", "", DisplayLevel.Beginner)]
        public BigInteger InputM
        {
            get
            {
                return inputM;
            }
            set
            {
                this.inputM = value;
                OnPropertyChanged("InputM");
            }
        }

        [PropertyInfo(Direction.InputData, "Prime P Input", "Input your Prime P here", "", DisplayLevel.Beginner)]
        public BigInteger InputP
        {
            get
            {
                return inputP;
            }
            set
            {
                this.inputP = value;
                OnPropertyChanged("InputP");
            }
        }

        [PropertyInfo(Direction.InputData, "Prime Q Input", "Input your Prime Q here", "", DisplayLevel.Beginner)]
        public BigInteger InputQ
        {
            get
            {
                return inputQ;
            }
            set
            {
                this.inputQ = value;
                OnPropertyChanged("InputQ");
            }
        }

        [PropertyInfo(Direction.InputData, "Public Key E Input", "Input your public key E here", "", DisplayLevel.Beginner)]
        public BigInteger InputE
        {
            get
            {
                return inputE;
            }
            set
            {
                this.inputE = value;
                OnPropertyChanged("InputE");
            }
        }
        
        [PropertyInfo(Direction.OutputData, "Cipher C Output", "Your Cipher will be send here", "", DisplayLevel.Beginner)]
        public BigInteger OutputC
        {
            get
            {
                return outputC;
            }
            set
            {
                this.outputC = value;
                OnPropertyChanged("OutputC");
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
