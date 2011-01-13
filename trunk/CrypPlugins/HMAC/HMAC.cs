/*
   Copyright 2009 Holger Pretzsch, University of Duisburg-Essen

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
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.Remoting.Contexts;
using Cryptool.PluginBase;
using Cryptool;
using System.IO;
using System.Windows.Controls;
using System.ComponentModel;

namespace Cryptool.HMAC
{
    [Author("Holger Pretzsch", "mail@holger-pretzsch.de", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "HMAC", "HMAC algorithm", "HMAC/DetailedDescription/Description.xaml", "HMAC/HMAC.png")]
    [EncryptionType(EncryptionType.Classic)]
    public class HMAC : ICryptographicHash
    {
        #region Private variables

        private HMACSettings settings;
        private CryptoolStream inputData;
        private byte[] key;
        private byte[] outputData;
        private List<CryptoolStream> streamList = new List<CryptoolStream>();

        #endregion

        #region Public interface

        public HMAC()
        {
            this.settings = new HMACSettings();
        }

        public ISettings Settings
        {
            get { return this.settings; }

            set { this.settings = (HMACSettings)value; }
        }

        [PropertyInfo(Direction.InputData, "Input stream", "Input data to be processed", "", true, false, QuickWatchFormat.Hex, null)]
        public CryptoolStream InputData
        {

            get
            {
                if (inputData != null)
                {
                    CryptoolStream cs = new CryptoolStream();
                    cs.OpenRead(inputData.FileName);
                    streamList.Add(cs);
                    return cs;
                }
                return null;
            }

            set
            {
                this.inputData = value;
                OnPropertyChanged("InputData");
            }
        }

        [PropertyInfo(Direction.InputData, "Key", "Message digest key", "", true, false, QuickWatchFormat.Hex, null)]
        public byte[] Key
        {
            get
            {
                return key;
            }

            set
            {
                this.key = value;
                OnPropertyChanged("Key");
            }
        }

        [PropertyInfo(Direction.OutputData, "Digested value", "Digested value as stream", "", false, false, QuickWatchFormat.Hex, null)]
        public CryptoolStream OutputDataStream
        {
            get
            {
                if (outputData != null)
                {
                    CryptoolStream stream = new CryptoolStream();
                    streamList.Add(stream);
                    stream.OpenRead(outputData);
                    GuiLogMessage("Got request for hash (Stream)...", NotificationLevel.Debug);
                    return stream;
                }
                else
                {
                    return null;
                }
            }

            set { } // readonly
        }

        [PropertyInfo(Direction.OutputData, "Digested value", "Digested value as byte array", "", false, false, QuickWatchFormat.Hex, null)]
        public byte[] OutputData
        {
            get
            {
                return this.outputData;
            }

            set
            {
                outputData = value;
                OnPropertyChanged("OutputData");
                OnPropertyChanged("OutputDataStream");
            }
        }

        public void Execute()
        {
            ProgressChanged(0.5, 1.0);
            if (inputData != null && key != null)
            {
                System.Security.Cryptography.HMAC hmacAlgorithm;

                switch ((HMACSettings.HashFunction)settings.SelectedHashFunction)
                {
                    case HMACSettings.HashFunction.MD5:
                        hmacAlgorithm = new System.Security.Cryptography.HMACMD5();
                        break;
                    case HMACSettings.HashFunction.RIPEMD160:
                        hmacAlgorithm = new System.Security.Cryptography.HMACRIPEMD160();
                        break;
                    case HMACSettings.HashFunction.SHA1:
                        hmacAlgorithm = new System.Security.Cryptography.HMACSHA1();
                        break;
                    case HMACSettings.HashFunction.SHA256:
                        hmacAlgorithm = new System.Security.Cryptography.HMACSHA256();
                        break;
                    case HMACSettings.HashFunction.SHA384:
                        hmacAlgorithm = new System.Security.Cryptography.HMACSHA384();
                        break;
                    case HMACSettings.HashFunction.SHA512:
                        hmacAlgorithm = new System.Security.Cryptography.HMACSHA512();
                        break;
                    default:
                        GuiLogMessage("No hash algorithm for HMAC selected, using MD5.", NotificationLevel.Warning);
                        hmacAlgorithm = new System.Security.Cryptography.HMACMD5();
                        break;
                }

                hmacAlgorithm.Key = key;
                OutputData = hmacAlgorithm.ComputeHash(inputData);

                GuiLogMessage(String.Format("HMAC computed. (using hash algorithm {0}: {1})", settings.SelectedHashFunction, hmacAlgorithm.GetType().Name), NotificationLevel.Info);
            }
            else
            {
                if (inputData == null)
                    GuiLogMessage("No input data for HMAC algorithm.", NotificationLevel.Warning);
                if (key == null)
                    GuiLogMessage("No key for HMAC algorithm.", NotificationLevel.Warning);
            }

            ProgressChanged(1.0, 1.0);
        }


        public void Initialize() { }

        public void Dispose()
        {
            if (inputData != null)
            {
                inputData.Close();
                inputData = null;
            }
            foreach (CryptoolStream stream in streamList)
            {
                stream.Close();
            }
        }
        #endregion

        #region IPlugin Members

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void Stop() { }

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            Dispose();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region IPlugin Members

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void Pause() { }

        #endregion
    }
}
