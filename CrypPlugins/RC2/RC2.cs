/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

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
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;

namespace Cryptool.Plugins.Cryptography.Encryption
{
    [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni-Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("Cryptool.RC2.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "RC2/icon.png", "RC2/Images/encrypt.png", "RC2/Images/decrypt.png")]
    [EncryptionType(EncryptionType.SymmetricBlock)]
    public class RC2 : IEncryption
    {
        #region Private variables
        private RC2Settings settings;
        private ICryptoolStream inputStream;
        private CStreamWriter outputStreamWriter;
        private byte[] inputKey;
        private byte[] inputIV;
        private CryptoStream p_crypto_stream;
        private bool stop = false;
        #endregion

        public RC2()
        {
            this.settings = new RC2Settings();
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, args);
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (RC2Settings)value; }
        }

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", "", true, false,QuickWatchFormat.Hex, null)]
        public ICryptoolStream InputStream
        {
            get
            {
                return inputStream;
                }
            set
            {
                this.inputStream = value;

                //wander 20100427: unnecessary, event should've been propagated by editor
                //OnPropertyChanged("InputStream");
            }
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", "", true, false,QuickWatchFormat.Hex, null)]
        public byte[] InputKey
        {
            get { return this.inputKey; }
            set
            {
                this.inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", "InputIVTooltip", null, false, false, QuickWatchFormat.Hex, null)]
        public byte[] InputIV
        {
            get { return this.inputIV; }
            set
            {
                this.inputIV = value;
                OnPropertyChanged("InputIV");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", null, true, false,QuickWatchFormat.Hex, null)]
        public ICryptoolStream OutputStream
        {
            get
            {
                return outputStreamWriter;
                }
            set
            {
                //wander 20100427: unnecessary, propagated by Execute()
                //OnPropertyChanged("OutputStream");
            }
        }

        private void ConfigureAlg(SymmetricAlgorithm alg)
        {
            //check for a valid key
            if (this.inputKey == null)
            {
                //create a trivial key 
                inputKey = new byte[16];
                // write a warning to the ouside word
                GuiLogMessage("WARNING - No key provided. Using 0x000..00!", NotificationLevel.Warning);
            }
            alg.Key = this.inputKey;

            //check for a valid IV
            if (this.inputIV == null)
            {
                //create a trivial key 
                inputIV = new byte[alg.BlockSize / 8];
                GuiLogMessage("WARNING - No IV provided. Using 0x000..00!", NotificationLevel.Warning);
            }
            alg.IV = this.inputIV;

            switch (settings.Mode)
            { //0="ECB"=default, 1="CBC", 2="CFB", 3="OFB"
                case 1: alg.Mode = CipherMode.CBC; break;
                case 2: alg.Mode = CipherMode.CFB; break;
                case 3: alg.Mode = CipherMode.OFB; break;
                default: alg.Mode = CipherMode.ECB; break;
            }
            switch (settings.Padding)
            { //0="Zeros"=default, 1="None", 2="PKCS7", 3="ANSIX923", 4="ISO10126"
                case 1: alg.Padding = PaddingMode.None; break;
                case 2: alg.Padding = PaddingMode.PKCS7; break;
                case 3: alg.Padding = PaddingMode.ANSIX923; break;
                case 4: alg.Padding = PaddingMode.ISO10126; break;
                default: alg.Padding = PaddingMode.Zeros; break;
            }
        }

        private void checkForInputStream()
        {
            if (settings.Action == 0 && (inputStream == null || (inputStream != null && inputStream.Length == 0)))
            {
                //create some input
                String dummystring = "Dummy string - no input provided - \"Hello RC2 World\" - dummy string - no input provided!";
                this.inputStream = new CStreamWriter(Encoding.Default.GetBytes(dummystring));
                // write a warning to the ouside word
                GuiLogMessage("WARNING - No input provided. Using dummy data. (" + dummystring + ")", NotificationLevel.Warning);
            }
        }

        public void Execute()
        {
            process(settings.Action);
        }

        private void process(int action)
        {
            //Encrypt/Decrypt Stream
            try
            {
                checkForInputStream();
                if (inputStream == null || inputStream.Length == 0)
                {
                    GuiLogMessage("No input given. Not using dummy data in decrypt mode. Aborting now.", NotificationLevel.Error);
                    return;
                }

                using (CStreamReader reader = inputStream.CreateReader())
                {
                SymmetricAlgorithm p_alg = new RC2CryptoServiceProvider();

                ConfigureAlg(p_alg);

                ICryptoTransform p_encryptor = null;
                switch (action)
                {
                    case 0:
                        p_encryptor = p_alg.CreateEncryptor();
                        break;
                    case 1:
                        p_encryptor = p_alg.CreateDecryptor();
                        break;
                }

                outputStreamWriter = new CStreamWriter();
                p_crypto_stream = new CryptoStream((Stream)reader, p_encryptor, CryptoStreamMode.Read);
                byte[] buffer = new byte[p_alg.BlockSize / 8];
                int bytesRead;
                int position = 0;
                GuiLogMessage("Starting encryption [Keysize=" + p_alg.KeySize.ToString() + " Bits, Blocksize=" + p_alg.BlockSize.ToString() + " Bits]", NotificationLevel.Info);
                DateTime startTime = DateTime.Now;
                while ((bytesRead = p_crypto_stream.Read(buffer, 0, buffer.Length)) > 0 && !stop)
                {
                        outputStreamWriter.Write(buffer, 0, bytesRead);

                        if ((int)(reader.Position * 100 / reader.Length) > position)
                    {
                            position = (int)(reader.Position * 100 / reader.Length);
                            ProgressChanged(reader.Position, reader.Length);
                    }
                }
                p_crypto_stream.Flush();
                // p_crypto_stream.Close();
                    outputStreamWriter.Close();
                DateTime stopTime = DateTime.Now;
                TimeSpan duration = stopTime - startTime;
                // (outputStream as CryptoolStream).FinishWrite();
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
                // TODO: For an unknown reason p_crypto_stream can not be closed after exception.
                // Trying so makes p_crypto_stream throw the same exception again. So in Dispose 
                // the error messages will be doubled. 
                // As a workaround we set p_crypto_stream to null here.
                p_crypto_stream = null;
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

        public void Encrypt()
        {
            //Encrypt stream
            process(0);
        }

        public void Decrypt()
        {
            //Decrypt Stream
            process(1);
        }

        #region IPlugin Member

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
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
                inputIV = null;
                inputStream = null;
                outputStreamWriter = null;

                if (p_crypto_stream != null)
                {
                    p_crypto_stream.Flush();
                    p_crypto_stream.Clear();
                    p_crypto_stream = null;
                }
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

        public void Pause()
        {
        }

        #endregion
    }
}
