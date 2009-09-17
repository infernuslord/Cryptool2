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
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Cryptool.Plugins.RSA
{
    [Author("Sven Rech", "rech@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(true, "RSAKeyGenerator", "RSA Key Generator", "", "RSA/iconkey.png", "RSA/iconkey.png", "RSA/iconkey.png")]

    [EncryptionType(EncryptionType.Asymmetric)]
    class RSAKeyGenerator : IEncryption
    {

        #region Properties

        private BigInteger n;
        [PropertyInfo(Direction.OutputData, "N", "N", "", DisplayLevel.Beginner)]
        public BigInteger N
        {
            get
            {
                return n;
            }
            set
            {
                this.n = value;
                OnPropertyChanged("N");
            }
        }

        private BigInteger e;
        [PropertyInfo(Direction.OutputData, "E", "public exponent", "", DisplayLevel.Beginner)]
        public BigInteger E
        {
            get
            {
                return e;
            }
            set
            {
                this.e = value;
                OnPropertyChanged("E");
            }
        }

        private BigInteger d;
        [PropertyInfo(Direction.OutputData, "D", "private exponent", "", DisplayLevel.Beginner)]
        public BigInteger D
        {
            get
            {
                return d;
            }
            set
            {
                this.d = value;
                OnPropertyChanged("D");
            }
        }

        #endregion


        #region IPlugin Members

        public event Cryptool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;

        public event Cryptool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event Cryptool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;


        private RSAKeyGeneratorSettings settings = new RSAKeyGeneratorSettings();
        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (RSAKeyGeneratorSettings)value; }
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
            BigInteger p;
            BigInteger q;
            BigInteger n;
            BigInteger e;
            BigInteger d;

            ProgressChanged(0.5, 1.0);
            switch (settings.Source)
            {
                case 0:
                    try
                    {
                        p = new BigInteger(settings.P, 10);
                        q = new BigInteger(settings.Q, 10);
                        e = new BigInteger(settings.E, 10);
                    }
                    catch (Exception)
                    {
                        GuiLogMessage("Invalid input", NotificationLevel.Error);
                        return;
                    }

                    try
                    {
                        N = p * q;
                        E = e;
                        D = e.modInverse((p - 1) * (q - 1));
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage("Big Number fail: " + ex.Message, NotificationLevel.Error);
                        return;
                    }
                    break;

                case 1:
                    try
                    {
                        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                        RSAParameters rsaParameters = rsa.ExportParameters(true);
                        p = new BigInteger(rsaParameters.P);
                        q = new BigInteger(rsaParameters.Q);
                        n = new BigInteger(rsaParameters.Modulus);
                        e = new BigInteger(rsaParameters.Exponent);
                        d = new BigInteger(rsaParameters.D);
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(ex.Message, NotificationLevel.Error);
                        return;
                    }

                    N = n;
                    E = e;
                    D = d;
                    break;

                case 2:
                    break;
            }
            ProgressChanged(1.0, 1.0);
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

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
