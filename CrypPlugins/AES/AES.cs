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
using System.Security.Cryptography;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.Remoting.Contexts;
using Cryptool.PluginBase.Control;
using System.Reflection;
using NativeCryptography;

namespace Cryptool.Plugins.Cryptography.Encryption
{
    [Author("Dr. Arno Wacker", "arno.wacker@cryptool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo(false, "AES", "Advanced Encryption Standard (Rijndael)", "AES/DetailedDescription/Description.xaml", "AES/Images/AES.png", "AES/Images/encrypt.png", "AES/Images/decrypt.png", "AES/Images/Rijndael.png")]
    [EncryptionType(EncryptionType.SymmetricBlock)]
    public class AES : ContextBoundObject, IEncryption
    {
        #region Private variables
        private AESSettings settings;
        private CryptoolStream inputStream;
        private CryptoolStream outputStream;
        private byte[] inputKey;
        private byte[] inputIV;
        private CryptoStream p_crypto_stream;
        private bool stop = false;
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
        #endregion

        public AES()
        {
            this.settings = new AESSettings();
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
          if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, args);
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (AESSettings)value; }
        }

        [PropertyInfo(Direction.InputData, "Input", "Data to be encrypted or decrypted", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
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

        [PropertyInfo(Direction.InputData, "Key", "The provided key should be 16, 24 or 32 bytes, dependig on the settings. Too short/long keys will be extended/truncated!", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public byte[] InputKey
        {
            get { return this.inputKey; }
            set 
            { 
              this.inputKey = value;
              OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.InputData, "IV", "The initialisation vector (IV) which is used in chaining modes. It always must be the same as the blocksize.", "", false, false, DisplayLevel.Professional, QuickWatchFormat.Hex, null)]
        public byte[] InputIV
        {
            get { return this.inputIV; }
            set 
            { 
              this.inputIV = value;
              OnPropertyChanged("InputIV");
            }
        }

        [PropertyInfo(Direction.OutputData, "Output stream", "Encrypted or decrypted output data", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public CryptoolStream OutputStream
        {
            get 
            {
              if (this.outputStream != null)
              {
                CryptoolStream cs = new CryptoolStream();
                listCryptoolStreamsOut.Add(cs);

                  if (outputStream.FileName == null)
                  {
                      return null;
                  }

                  try
                  {
                      cs.OpenRead(this.outputStream.FileName);
                  }
                  catch (FileNotFoundException)
                  {
                      GuiLogMessage("File not found: " + outputStream.FileName, NotificationLevel.Warning);
                      return null;
                  }

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
            // first set all paramter, then assign input values!
            switch (settings.Mode)
            { //0="ECB"=default, 1="CBC", 2="CFB", 3="OFB"
                case 1: alg.Mode = CipherMode.CBC; break;
                case 2: alg.Mode = CipherMode.CFB; break;
                // case 3: alg.Mode = CipherMode.OFB; break;
                default: alg.Mode = CipherMode.ECB; break;
            }

            switch (settings.Padding)
            { //0="Zeros"=default, 1="None", 2="PKCS7" , 3="ANSIX923", 4="ISO10126"
                case 1: alg.Padding = PaddingMode.None; break;
                case 2: alg.Padding = PaddingMode.PKCS7; break;
                case 3: alg.Padding = PaddingMode.ANSIX923; break;
                case 4: alg.Padding = PaddingMode.ISO10126; break;
                default: alg.Padding = PaddingMode.Zeros; break;
            }
            if (settings.CryptoAlgorithm == 1)
            {
                switch (settings.Blocksize)
                {
                    case 1: alg.BlockSize = 192; break;
                    case 2: alg.BlockSize = 256; break;
                    default:
                        alg.BlockSize = 128;
                        break;
                }
            }

            //check for a valid key
            if (this.inputKey == null)
            {                
                //create a trivial key 
                inputKey = new byte[16];
                // write a warning to the ouside word
                GuiLogMessage("ERROR: No key provided. Using 0x000..00!", NotificationLevel.Error);
            }

            int keySizeInBytes = (16 + settings.Keysize * 8);

            //prepare a long enough key
            byte[] key = new byte[keySizeInBytes];

            // copy the input key into the temporary key array
            Array.Copy(inputKey, key, Math.Min(inputKey.Length, key.Length));

            // Note: the SymmetricAlgorithm.Key setter clones the passed byte[] array and keeps his own copy
            alg.Key = key;

            if (inputKey.Length > keySizeInBytes)
            {
                GuiLogMessage("Overlength (" + inputKey.Length * 8 + " Bits) key provided. Removing trailing bytes to fit the desired key length of " + (keySizeInBytes * 8) + " Bits: " + bytesToHexString(key), NotificationLevel.Warning);
            }

            if (inputKey.Length < keySizeInBytes)
            {
                GuiLogMessage("Short (" + inputKey.Length * 8 + " Bits) key provided. Adding zero bytes to fill up to the desired key length of " + (keySizeInBytes * 8) + " Bits: " + bytesToHexString(key), NotificationLevel.Warning);
            }

            //check for a valid IV
            if (this.inputIV == null)
            {
                //create a trivial key 
                inputIV = new byte[alg.BlockSize / 8];
                GuiLogMessage("NOTE: No IV provided. Using 0x000..00!", NotificationLevel.Info);
            }
            alg.IV = this.inputIV;
        }

        private string bytesToHexString(byte[] array)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in array)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        private void checkForInputStream()
        {
            if (settings.Action == 0 && (inputStream == null || (inputStream != null && inputStream.Length == 0)))
            {
                //create some input
                String dummystring = "Dummy string - no input provided - \"Hello AES World\" - dummy string - no input provided!";
                this.inputStream = new CryptoolStream();
                this.inputStream.OpenRead(Encoding.Default.GetBytes(dummystring.ToCharArray()));
                // write a warning to the ouside word
                GuiLogMessage("WARNING: No input provided. Using dummy data. (" + dummystring + ")", NotificationLevel.Warning);
            }
        }

        public void Execute()
        {
            process(settings.Action);
        }

        public bool isStopped()
        {

            return this.stop;
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

            if (this.inputStream.CanSeek) this.inputStream.Position = 0;
            SymmetricAlgorithm p_alg = null;
            if (settings.CryptoAlgorithm == 1)
            { p_alg = new RijndaelManaged(); }
            else
            { p_alg = new AesCryptoServiceProvider(); }

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
            outputStream.OpenWrite();
            p_crypto_stream = new CryptoStream((Stream)inputStream, p_encryptor, CryptoStreamMode.Read);
            byte[] buffer = new byte[p_alg.BlockSize / 8];
            int bytesRead;
            int position = 0;
            string mode = action == 0 ? "encryption" : "decryption";
            GuiLogMessage("Starting " + mode + " [Keysize=" + p_alg.KeySize.ToString() + " Bits, Blocksize=" + p_alg.BlockSize.ToString() + " Bits]", NotificationLevel.Info);
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


            long outbytes = outputStream.Length;
            p_crypto_stream.Flush();            
            // p_crypto_stream.Close();
            DateTime stopTime = DateTime.Now;
            TimeSpan duration = stopTime - startTime;
            // (outputStream as CryptoolStream).FinishWrite();

            if (!stop)
            {
                mode = action == 0 ? "Encryption" : "Decryption";
                GuiLogMessage(mode + " complete! (in: " + inputStream.Length.ToString() + " bytes, out: " + outbytes.ToString() + " bytes)", NotificationLevel.Info);
                GuiLogMessage("Wrote data to file: " + outputStream.FileName, NotificationLevel.Info);
                GuiLogMessage("Time used: " + duration.ToString(), NotificationLevel.Debug);
                outputStream.Close();
                OnPropertyChanged("OutputStream");
            }
            CryptoolStream test = outputStream;
            if (stop)
            {
                outputStream.Close();
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
            outputStream = null;
            inputStream = null;

            //if (inputStream != null)
            //{
            //  inputStream.Flush();
            //  inputStream.Close();
            //  inputStream = null;
            //}
            //if (outputStream != null)
            //{
            //  outputStream.Flush();
            //  outputStream.Close();
            //  outputStream = null;
            //}
            foreach (CryptoolStream stream in listCryptoolStreamsOut)
            {
              stream.Close();
            }
            listCryptoolStreamsOut.Clear();

            if (p_crypto_stream != null)
            {
              p_crypto_stream.Flush();
              p_crypto_stream.Close();
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
          EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
          //if (PropertyChanged != null)
          //{
          //  PropertyChanged(this, new PropertyChangedEventArgs(name));
          //}
        }

        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;        

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
        
        public void Pause()
        {
          
        }

        #endregion

        private IControlEncryption controlSlave;
        [PropertyInfo(Direction.ControlSlave, "AES Slave", "AES Slave", "", DisplayLevel.Experienced)]
        public IControlEncryption ControlSlave
        {
          get 
          {
              if (controlSlave == null)
                  controlSlave = new AESControl(this);
              return controlSlave; 
          }
        }     
    }

    public class AESControl : IControlEncryption
    {
        public event KeyPatternChanged keyPatternChanged;
        public event IControlStatusChangedEventHandler OnStatusChanged;

        private AES plugin;
     
        public AESControl(AES Plugin)
        {
            this.plugin = Plugin;
        }

        #region IControlEncryption Members

        public byte[] Encrypt(byte[] key, int blocksize)
        {
            /// not implemented, currently not needed
            return null;
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV)
        {
            return Decrypt(ciphertext, key, IV, ciphertext.Length);
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV, int bytesToUse)
        {
            int size = bytesToUse > ciphertext.Length ? ciphertext.Length : bytesToUse;

            int bits = -1;
            switch (((AESSettings)plugin.Settings).Keysize)
            {
                case 0:
                    bits = 16*8;
                    break;
                case 1:
                    bits = 24*8;
                    break;
                case 2:
                    bits = 32*8;
                    break;
            }

            if (bits == -1)
                return null;

            return NativeCryptography.Crypto.decryptAES(ciphertext, key, IV, bits, size, ((AESSettings)plugin.Settings).Mode);
        }

        public string getKeyPattern()
        {
            int bytes = 0;
            switch (((AESSettings)plugin.Settings).Keysize)
            {
                case 0:
                    bytes = 16;
                    break;
                case 1:
                    bytes = 24;
                    break;
                case 2:
                    bytes = 32;
                    break;
            }
            string pattern = "";
            for (int i = 1; i < bytes; i++)
                pattern += "[0-9A-F][0-9A-F]-";
            pattern += "[0-9A-F][0-9A-F]";
            return pattern;
        }

        public IControlEncryption clone()
        {
            AESControl aes = new AESControl(plugin);
            return aes;
        }

        public void Dispose()
        {
        }

        public string GetOpenCLCode(int decryptionLength)
        {
            string opencl = Properties.Resources.AESOpenCL;

            int bits = -1;
            switch (((AESSettings)plugin.Settings).Keysize)
            {
                case 0:
                    bits = 16 * 8;
                    break;
                case 1:
                    bits = 24 * 8;
                    break;
                case 2:
                    bits = 32 * 8;
                    break;
            }
            if (bits == -1)
                return null;

            opencl = opencl.Replace("$$BITS$$", "" + bits);

            string decryptionCode = "";
            int blocks = decryptionLength / 16;
            if (blocks >= 1)
            {
                decryptionCode = AddOpenCLBlockDecryption(decryptionCode, 16);

                if (blocks > 1)
                {
                    decryptionCode += string.Format("for (int b = 1; b < {0}; b++) \n {{ \n ", blocks);
                    decryptionCode = AddOpenCLBlockDecryptionWithMode(decryptionCode, 16, "b");
                    decryptionCode += "}\n";
                }
            }

            if (decryptionLength % 16 != 0)
            {
                if (blocks == 0)
                    decryptionCode = AddOpenCLBlockDecryption(decryptionCode, decryptionLength % 16);
                else
                    decryptionCode = AddOpenCLBlockDecryptionWithMode(decryptionCode, decryptionLength % 16, ""+blocks);
            }

            opencl = opencl.Replace("$$AESDECRYPT$$", decryptionCode);

            return opencl;
        }

        private string AddOpenCLBlockDecryption(string decryptionCode, int size)
        {
            decryptionCode += "AES_decrypt(inn, block, &(key)); \n " + string.Format("for (int i = 0; i < {0}; i++) \n ", size)
                              + "{ \n unsigned char c = block[i]; \n "
                              + "$$COSTFUNCTIONCALCULATE$$ \n } \n";
            return decryptionCode;
        }

        private string AddOpenCLBlockDecryptionWithMode(string decryptionCode, int size, string block)
        {
            decryptionCode += string.Format("AES_decrypt((inn+{0}*16), block, &(key)); \n ", block)
                              + string.Format("for (int i = 0; i < {0}; i++) \n {{ \n ", size);
            switch (((AESSettings) plugin.Settings).Mode)
            {
                case 0: //ECB
                    decryptionCode += "unsigned char c = block[i]; \n";
                    break;
                case 1: //CBC
                    decryptionCode += string.Format("unsigned char c = block[i] ^ (inn+({0}-1)*16)[i]; \n", block);
                    break;
                case 2: //CFB
                    throw new NotImplementedException("CFB for OpenCL is not implemented!"); //not supported
            }
            decryptionCode += "$$COSTFUNCTIONCALCULATE$$ \n } \n";
            return decryptionCode;
        }

        public void changeSettings(string setting, object value)
        {
        }

        public IKeyTranslator getKeyTranslator()
        {
            return new KeySearcher.KeyTranslators.ByteArrayKeyTranslator();
        }

        #endregion
    }
}
