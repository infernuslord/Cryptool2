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
    [PluginInfo(false, "Triple DES", "Triple Data Encryption Standard", null, 
      "TripleDES/Images/icon.png", "TripleDES/Images/encrypt.png", "TripleDES/Images/decrypt.png")]
    [EncryptionType(EncryptionType.SymmetricBlock)]
    public class TripleDES : IEncryption
    {
        #region Private variables
        private TripleDESSettings settings;
        private CryptoolStream inputStream;
        private CryptoolStream outputStream;
        private byte[] inputKey;
        private byte[] inputIV;
        private CryptoStream p_crypto_stream;
        private bool stop = false;
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
        #endregion

        public TripleDES()
        {
            this.settings = new TripleDESSettings();
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, args);
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (TripleDESSettings)value; }
        }

        [PropertyInfo(Direction.InputData, "Input", "Data to be encrypted or decrypted", null, true, false, DisplayLevel.Beginner,QuickWatchFormat.Hex, null)]
        public CryptoolStream InputStream
        {
            get
            {
                if (inputStream != null)
                {
                    CryptoolStream cs = new CryptoolStream();
                    cs.OpenRead(inputStream.FileName);
                    listCryptoolStreamsOut.Add(cs);
                    return cs;
                }
                else return null;
            }
            set
            {
                this.inputStream = value;
                if (value != null) listCryptoolStreamsOut.Add(value);
                OnPropertyChanged("InputStream");
            }
        }

        [PropertyInfo(Direction.InputData, "Key", "Must be 16 or 24 bytes.", null, true, false, DisplayLevel.Beginner,QuickWatchFormat.Hex, null)]
        public byte[] InputKey
        {
            get { return this.inputKey; }
            set
            {
                this.inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.InputData, "IV", "IV to be used in chaining modes, must be the same as the Blocksize in bytes (8 bytes).", null, true, false, DisplayLevel.Professional, QuickWatchFormat.Hex, null)]
        public byte[] InputIV
        {
            get { return this.inputIV; }
            set
            {
                this.inputIV = value;
                OnPropertyChanged("InputIV");
            }

        }

        [PropertyInfo(Direction.OutputData, "Output stream", "Encrypted or decrypted output data", null, true, false, DisplayLevel.Beginner,QuickWatchFormat.Hex, null)]
        public CryptoolStream OutputStream
        {
            get
            {
                if (this.outputStream != null)
                {
                    CryptoolStream cs = new CryptoolStream();
                    listCryptoolStreamsOut.Add(cs);
                    cs.OpenRead(this.outputStream.FileName);
                    return cs;
                }
                return null;
            }
            set
            {
                outputStream = value;
                if (value != null) listCryptoolStreamsOut.Add(value);
                OnPropertyChanged("OutputStream");
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
            { //0="Zeros"=default, 1="None", 2="PKCS7"
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
                String dummystring = "Dummy string - no input provided - \"Hello TripleDES World\" - dummy string - no input provided!";
                this.inputStream = new CryptoolStream();
                this.inputStream.OpenRead(this.GetPluginInfoAttribute().Caption, Encoding.Default.GetBytes(dummystring.ToCharArray()));
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
                if (inputStream == null || (inputStream != null && inputStream.Length == 0))
                {
                    GuiLogMessage("No input given. Not using dummy data in decrypt mode. Aborting now.", NotificationLevel.Error);
                    return;
                }

                if (this.inputStream.CanSeek) this.inputStream.Position = 0;
                SymmetricAlgorithm p_alg = new TripleDESCryptoServiceProvider();

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

                outputStream = new CryptoolStream();
                listCryptoolStreamsOut.Add(outputStream);
                outputStream.OpenWrite(this.GetPluginInfoAttribute().Caption);
                p_crypto_stream = new CryptoStream((Stream)inputStream, p_encryptor, CryptoStreamMode.Read);
                byte[] buffer = new byte[p_alg.BlockSize / 8];
                int bytesRead;
                int position = 0;
                GuiLogMessage("Starting encryption [Keysize=" + p_alg.KeySize.ToString() + " Bits, Blocksize=" + p_alg.BlockSize.ToString() + " Bits]", NotificationLevel.Info);
                DateTime startTime = DateTime.Now;
                while ((bytesRead = p_crypto_stream.Read(buffer, 0, buffer.Length)) > 0 && !stop)
                {
                    outputStream.Write(buffer, 0, bytesRead);

                    if ((int)(inputStream.Position * 100 / inputStream.Length) > position)
                    {
                        position = (int)(inputStream.Position * 100 / inputStream.Length);
                        ProgressChanged(inputStream.Position, inputStream.Length);
                    }
                }
                p_crypto_stream.Flush();
                // p_crypto_stream.Close();
                outputStream.Close();
                DateTime stopTime = DateTime.Now;
                TimeSpan duration = stopTime - startTime;
                // (outputStream as CryptoolStream).FinishWrite();
                if (!stop)
                {
                    GuiLogMessage("Encryption complete! (in: " + inputStream.Length.ToString() + " bytes, out: " + outputStream.Length.ToString() + " bytes)", NotificationLevel.Info);
                    GuiLogMessage("Wrote data to file: " + outputStream.FileName, NotificationLevel.Info);
                    GuiLogMessage("Time used: " + duration.ToString(), NotificationLevel.Debug);
                    OnPropertyChanged("OutputStream");
                }
                if (stop)
                {
                    GuiLogMessage("Aborted!", NotificationLevel.Info);
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
            //Encrypt Stream
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

                foreach (CryptoolStream stream in listCryptoolStreamsOut)
                {
                    stream.Close();
                }
                listCryptoolStreamsOut.Clear();

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