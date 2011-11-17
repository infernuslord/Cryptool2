/*
   Copyright 2011 Florian Marchal

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
using System.Security.Cryptography;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Resources;
using System.Reflection;

namespace Cryptool.Plugins.Cryptography.Encryption
{
    [Author("Florian Marchal", "florian@marchal.de", "", "")]
    [PluginInfo("Cryptool.RC4.Properties.Resources", "PluginCaption", "PluginTooltip", "RC4/DetailedDescription/doc.xml", "RC4/icon.png", "RC4/Images/encrypt.png", "RC4/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class RC4 : ICrypComponent
    {
        #region Private variables
        // RC4 settings
        private RC4Settings settings;
        // the input data provided by the user
        private ICryptoolStream inputData;
        // the input key provided by the user
        private ICryptoolStream inputKey;
        // the output stream
        private CStreamWriter outputStreamWriter;
        // indicates if we need to stop the algorithm
        private bool stop = false;
        #endregion

        public RC4()
        {
            this.settings = new RC4Settings();
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, args);
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (RC4Settings)value; }
        }

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", true)]
        public ICryptoolStream InputData
        {
            get
            {
                return inputData;
            }
            set
            {
                this.inputData = value;
            }
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public ICryptoolStream InputKey
        {
            get { return this.inputKey; }
            set
            {
                this.inputKey = value;
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICryptoolStream OutputStream
        {
            get
            {
                return outputStreamWriter;
            }
            set
            {

            }
        }

        public void Execute()
        {
            try
            {
                // this is for localization
                ResourceManager resourceManager = new ResourceManager("Cryptool.RC4.Properties.Resources", GetType().Assembly);
                
                // make sure we have a valid data input
                if (inputData == null)
                {
                    GuiLogMessage(resourceManager.GetString("ErrorInputDataNotProvided"), NotificationLevel.Error);
                    return;
                }

                // make sure we have a valid key input
                if (inputKey == null)
                {
                    GuiLogMessage(resourceManager.GetString("ErrorInputKeyNotProvided"), NotificationLevel.Error);
                    return;
                }
                
                // make sure the input key is within the desired range
                if ((inputKey.Length < 5 || inputKey.Length > 256)) 
                {
                    GuiLogMessage(resourceManager.GetString("ErrorInputKeyInvalidLength"), NotificationLevel.Error);
                   return;
                }

                // now execute the actual encryption
                using (CStreamReader reader = inputData.CreateReader())
                {
                    // create the output stream
                    outputStreamWriter = new CStreamWriter();

                    // some variables
                    int i = 0;
                    int j = 0;
                    // create the sbox
                    byte[] sbox = new byte[256];
                    // initialize the sbox sequentially
                    for (i = 0; i < 256; i++)
                    {
                        sbox[i] = (byte)(i);
                    }
                    // re-align the sbox (and incorporate the key)
                    j = 0;
                    string key = inputKey.ToString();
                    int keyLength = key.Length;
                    for (i = 0; i < 256; i++)
                    {
                        j = (j + sbox[i] + key[i % keyLength]) % 256;
                        byte sboxOld = sbox[i];
                        sbox[i] = sbox[j];
                        sbox[j] = sboxOld;
                    }

                    // process the input data using the modified sbox
                    int position = 0;
                    DateTime startTime = DateTime.Now;

                    // some inits
                    i = 0;
                    j = 0;

                    long bytesRead = 0;
                    const long blockSize = 256;
                    byte[] buffer = new byte[blockSize];

                    while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0 && !stop)
                    {
                        for (long n = 0; n < bytesRead; n++)
                        {
                            i = (i + 1) % 256;
                            j = (j + sbox[i]) % 256;
                            byte sboxOld = sbox[i];
                            sbox[i] = sbox[j];
                            sbox[j] = sboxOld;

                            byte sboxRandom = sbox[(sbox[i] + sbox[j]) % 256];
                            byte cipherByte = (byte)(sboxRandom ^ buffer[n]);
                            outputStreamWriter.WriteByte(cipherByte);
                        }

                        if ((int)(reader.Position * 100 / reader.Length) > position)
                        {
                            position = (int)(reader.Position * 100 / reader.Length);
                            ProgressChanged(reader.Position, reader.Length);
                        }
                    }
                    outputStreamWriter.Close();

                    // dump status information
                    DateTime stopTime = DateTime.Now;
                    TimeSpan duration = stopTime - startTime;
                    if (!stop)
                    {
                        GuiLogMessage("Encryption complete! (in: " + reader.Length.ToString() + " bytes, out: " + outputStreamWriter.Length.ToString() + " bytes)", NotificationLevel.Info);
                        GuiLogMessage("Time used: " + duration.ToString(), NotificationLevel.Debug);
                        OnPropertyChanged("OutputStream");
                    }
                    if (stop)
                    {
                        GuiLogMessage("Aborted!", NotificationLevel.Info);
                    }
                }
            }
            catch (CryptographicException cryptographicException)
            {
                GuiLogMessage(cryptographicException.Message, NotificationLevel.Error);
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
            finally
            {
                ProgressChanged(1, 1);
            }
        }

        #region IPlugin Member

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
            try
            {
                stop = false;
                inputKey = null;
                inputData = null;
                outputStreamWriter = null;
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
            this.stop = false;
        }

        public void Stop()
        {
            this.stop = true;
        }

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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
            }
        }

        #endregion
    }
}