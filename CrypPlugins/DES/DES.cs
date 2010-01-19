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
using Cryptool.PluginBase.Control;
using System.Reflection;
// Reference to the CubeAttackController interface (own dll)
using Cryptool.CubeAttackController;

namespace Cryptool.Plugins.Cryptography.Encryption
{
    [PluginInfo(false, "DES", "Data Encryption Standard", "DES/DetailedDescription/Description.xaml", "DES/icon.png", "DES/Images/encrypt.png", "DES/Images/decrypt.png")]
    [EncryptionType(EncryptionType.SymmetricBlock)]
    public class DES : IEncryption
    {
        #region Private variables
        private DESSettings settings;
        private CryptoolStream inputStream;
        private CryptoolStream outputStream;
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
        private byte[] inputKey;
        private byte[] inputIV;
        private CryptoStream p_crypto_stream;
        private bool stop = false;
        private IControlEncryption controlSlave;
        #endregion

        public DES()
        {
            this.settings = new DESSettings();
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if(OnPluginStatusChanged != null)OnPluginStatusChanged(this, args);
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (DESSettings)value; }
        }

        [PropertyInfo(Direction.InputData, "Input", "Data to be encrypted or decrypted", null, true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
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

        [PropertyInfo(Direction.InputData, "Key", "Must be 8 bytes.", null, true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public byte[] InputKey
        {
            get { return this.inputKey; }
            set
            {
                this.inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.InputData, "IV", "IV to be used in chaining modes, must be the same as the Blocksize in bytes (8 bytes).", null, true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public byte[] InputIV
        {
            get { return this.inputIV; }
            set
            {
                this.inputIV = value;
                OnPropertyChanged("InputIV");
            }
        }
        
        [PropertyInfo(Direction.OutputData, "Output stream", "Encrypted or decrypted output data", null, true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
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
                inputKey = new byte[8];
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
                String dummystring = "Dummy string - no input provided - \"Hello DES World\" - dummy string - no input provided!";
                this.inputStream = new CryptoolStream();
                this.inputStream.OpenRead(this.GetPluginInfoAttribute().Caption, Encoding.Default.GetBytes(dummystring.ToCharArray()));
                // write a warning to the ouside word
                GuiLogMessage("WARNING - No input provided. Using dummy data. (" + dummystring + ")", NotificationLevel.Warning);
            }
        }

        public void Execute()
        {
            try
            {
                //copy inputStream for Slave
                if (controlSlave is object && InputStream is object && InputIV is object)
                {
                    CryptoolStream cs = new CryptoolStream();
                    cs.OpenRead(inputStream.FileName); 
                    ((DESControl)controlSlave).InputStream = cs;
                }

                process(settings.Action);

                //Work with slave
                if (controlSlave is object && InputStream is object && InputIV is object)
                {
                    ((DESControl)controlSlave).reset();
                    ((DESControl)controlSlave).onStatusChanged();

                }
            }
            finally
            {
                if (controlSlave is object && ((DESControl)controlSlave).InputStream is object)
                    ((DESControl)controlSlave).closeStreams();
            }
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
                SymmetricAlgorithm p_alg = new DESCryptoServiceProvider();

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
                outputStream.Close();
                DateTime stopTime = DateTime.Now;
                TimeSpan duration = stopTime - startTime;
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

        public UserControl Presentation
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
                outputStream = null;

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

        public bool isStopped(){

            return this.stop;
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
        public void GuiLogMessage(string message, NotificationLevel logLevel)
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

        [PropertyInfo(Direction.ControlSlave, "DES Slave", "Direct access to DES.", "", DisplayLevel.Beginner)]
        public IControlEncryption ControlSlave
        {
          get
          {
              if (controlSlave == null)
                  controlSlave = new DESControl(this);
              return controlSlave;
          }
        }

        private IControlCubeAttack desSlave;
        [PropertyInfo(Direction.ControlSlave, "DES for cube attack", "Direct access to DES.", "", DisplayLevel.Beginner)]
        public IControlCubeAttack DESSlave
        {
            get
            {
                if (desSlave == null)
                    desSlave = new CubeAttackControl(this);
                return desSlave;
            }
        }
    }

    #region DESControl : IControlCubeAttack

    public class CubeAttackControl : IControlCubeAttack
    {
        public event IControlStatusChangedEventHandler OnStatusChanged;
        private DES plugin;

        public CubeAttackControl(DES Plugin)
        {
            this.plugin = Plugin;
        }

        #region IControlEncryption Members

        public int GenerateBlackboxOutputBit(object IV, object key, object length)
        {
            // public bits := plaintext
            // secret bits := key 
            SymmetricAlgorithm p_alg = new DESCryptoServiceProvider();
            string secretBits = string.Empty;
            string publicBits = string.Empty;

            // save public and secret bits as string
            int[] temp = key as int[];
            for (int i = 0; i < temp.Length; i++)
                secretBits += temp[i];
            temp = IV as int[];
            for (int i = 0; i < temp.Length; i++)
                publicBits += temp[i];

            // convert secret bits to byte array
            int[] arrInt = new int[8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (secretBits[(8 * i) + j] == '1')
                        arrInt[i] += (int)Math.Pow(2, 7 - j);
                }
            }
            byte[] keyByte = new byte[8];
            for (int i = 0; i < arrInt.Length; i++)
                keyByte[i] = (byte)arrInt[i];

            // convert public bits to byte array
            arrInt = new int[8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (publicBits[(8 * i) + j] == '1')
                        arrInt[i] += (int)Math.Pow(2, 7 - j);
                }
            }
            byte[] publicByte = new byte[8];
            for (int i = 0; i < arrInt.Length; i++)
                publicByte[i] = (byte)arrInt[i];

            ICryptoTransform p_encryptor;
            p_alg.IV = new byte[8];
            p_alg.Padding = PaddingMode.Zeros;
            try
            {
                p_alg.Key = keyByte;
            }
            catch
            {
                //dirty hack to allow weak keys:
                FieldInfo field = p_alg.GetType().GetField("KeyValue", BindingFlags.NonPublic | BindingFlags.Instance);
                field.SetValue(p_alg, keyByte);
            }
            try
            {
                p_encryptor = p_alg.CreateEncryptor();
            }
            catch
            {
                //dirty hack to allow weak keys:
                MethodInfo mi = p_alg.GetType().GetMethod("_NewEncryptor", BindingFlags.NonPublic | BindingFlags.Instance);
                object[] Par = { p_alg.Key, p_alg.Mode, p_alg.IV, p_alg.FeedbackSize, 0 };
                p_encryptor = mi.Invoke(p_alg, Par) as ICryptoTransform;
            }
            
            Stream inputPublic = new MemoryStream(publicByte);
            // starting encryption
            CryptoStream p_crypto_stream = new CryptoStream(inputPublic, p_encryptor, CryptoStreamMode.Read);
            byte[] buffer = new byte[p_alg.BlockSize / 8];
            p_crypto_stream.Read(buffer, 0, buffer.Length);
                
            // convert encrypted block to binary string
            string strBytes = string.Empty;
            for (int i = 0; i < buffer.Length; i++)
            {
                for (int j = 7; j >= 0; j--)
                    strBytes += (buffer[i] & 1 << j) > 0 ? 1 : 0;
            }
            p_crypto_stream.Flush();

            // return single output bit
            return Int32.Parse(strBytes.Substring((int)length-1, 1));
        }
        #endregion
    }

    #endregion

    public class DESControl : IControlEncryption
    {
        public event KeyPatternChanged keyPatternChanged;
        public event IControlStatusChangedEventHandler OnStatusChanged;
        private DES plugin;
        private SymmetricAlgorithm des_algorithm = null;
        private byte[] input;
        private Stream inputStream;
 
        public CryptoolStream InputStream{
            get;set;
        }

        public void reset()
        {
            input = null;
            des_algorithm = null;
        }

        public void closeStreams()
        {
            if (inputStream != null)
                inputStream.Close();
            if (InputStream != null)
                InputStream.Close();
        }

        public DESControl(DES Plugin)
        {
            this.plugin = Plugin;
        }

        #region IControlEncryption Members

        public byte[] Encrypt(byte[] key, int blocksize)
        {
            /// not implemented, currently not needed
            throw new NotImplementedException();
        }

        // TODO: add override with iv, mode, blocksize
        public byte[] Decrypt(byte[] ciphertext, byte[] key)
        {
            CryptoStream crypto_stream = null;
            byte[] output = new byte[8];
            
            // always recreating this instance is thread-safe, but may cost us some performance
            SymmetricAlgorithm des_algorithm  = new DESCryptoServiceProvider();

            try
            {
                des_algorithm.Key = key;
            }
            catch
            {
                //dirty hack to allow weak keys:
                FieldInfo field = des_algorithm.GetType().GetField("KeyValue", BindingFlags.NonPublic | BindingFlags.Instance);
                Console.WriteLine(des_algorithm.GetType());
                field.SetValue(des_algorithm, key);
            }

            ICryptoTransform p_decryptor;
            try
            {
                p_decryptor = des_algorithm.CreateDecryptor();
            }
            catch
            {
                //dirty hack to allow weak keys:
                MethodInfo mi = des_algorithm.GetType().GetMethod("_NewEncryptor", BindingFlags.NonPublic | BindingFlags.Instance);
                object[] Par = { des_algorithm.Key, des_algorithm.Mode, des_algorithm.IV, des_algorithm.FeedbackSize, 0 };
                p_decryptor = mi.Invoke(des_algorithm, Par) as ICryptoTransform;
            }

            crypto_stream = new CryptoStream(new MemoryStream(ciphertext, false), p_decryptor, CryptoStreamMode.Read);

            int read, readOverall = 0;
            do
            {
                read = crypto_stream.Read(output, readOverall, output.Length - readOverall);
                readOverall += read;
            } while (read > 0 && readOverall < output.Length);

            return output;

            //try
            //{
            //    des_algorithm.IV = this.plugin.InputIV;
            //}
            //catch
            //{
            //    //dirty hack to allow weak keys:
            //    FieldInfo field = des_algorithm.GetType().GetField("IVValue", BindingFlags.NonPublic | BindingFlags.Instance);
            //    field.SetValue(des_algorithm, this.plugin.InputIV);
            //}

            //switch (((DESSettings)plugin.Settings).Mode)
            //{ //0="ECB"=default, 1="CBC", 2="CFB", 3="OFB"
            //    case 1: alg.Mode = CipherMode.CBC; break;
            //    case 2: alg.Mode = CipherMode.CFB; break;
            //    case 3: alg.Mode = CipherMode.OFB; break;
            //    default: alg.Mode = CipherMode.ECB; break;
            //}
            //switch (((DESSettings)plugin.Settings).Padding)
            //{ //0="Zeros"=default, 1="None", 2="PKCS7"
            //    case 1: alg.Padding = PaddingMode.None; break;
            //    case 2: alg.Padding = PaddingMode.PKCS7; break;
            //    case 3: alg.Padding = PaddingMode.ANSIX923; break;
            //    case 4: alg.Padding = PaddingMode.ISO10126; break;
            //    default: alg.Padding = PaddingMode.Zeros; break;
            //}
        }

        [Obsolete("this signature doesn't pass the ciphertext, use Decrypt(byte[], byte[]) instead")]
        public byte[] Decrypt(byte[] key, int blocksize)
        {
            int size = (int)this.InputStream.Length;

            if (blocksize < size)
                size = blocksize;

            if (!(this.input is object))
            {
                input = new byte[size];
                this.InputStream.Seek(0, 0);
                for (int i = 0; i < size && i < this.InputStream.Length; i++)
                    input[i] = (byte)this.InputStream.ReadByte();
                inputStream = new MemoryStream(input);
            }
            inputStream.Seek(0, 0);

            CryptoStream crypto_stream = null;
            byte[] output = new byte[blocksize];

            //Decrypt Stream
            try
            {
                if (!(des_algorithm is object))
                {
                    des_algorithm = new DESCryptoServiceProvider();
                }

                this.ConfigureAlg(des_algorithm, key);

                ICryptoTransform p_decryptor;
                try
                {
                    p_decryptor = des_algorithm.CreateDecryptor();
                }
                catch
                {
                    //dirty hack to allow weak keys:
                    MethodInfo mi = des_algorithm.GetType().GetMethod("_NewEncryptor", BindingFlags.NonPublic | BindingFlags.Instance);
                    object[] Par = { des_algorithm.Key, des_algorithm.Mode, des_algorithm.IV, des_algorithm.FeedbackSize, 0 };
                    p_decryptor = mi.Invoke(des_algorithm, Par) as ICryptoTransform;
                }

                crypto_stream = new CryptoStream((Stream)inputStream, p_decryptor, CryptoStreamMode.Read);

                byte[] buffer = new byte[des_algorithm.BlockSize / 8];
                int bytesRead;
                int position = 0;

                while ((bytesRead = crypto_stream.Read(buffer, 0, buffer.Length)) > 0 && !plugin.isStopped())
                {
                    for (int i = 0; i < bytesRead; i++)
                    {
                        if (position + i < output.Length)
                        {
                            output[position + i] = buffer[i];
                        }
                        else
                        {
                            break;
                        }
                    }
                    position += bytesRead;
                }


            }
            catch (Exception exception)
            {
                des_algorithm = null;   // we got an exception so we do not use this object any more
                throw exception;
            }

            return output;
        }

        [Obsolete("being used by obsolete Decrypt(byte[], int)")]
        private void ConfigureAlg(SymmetricAlgorithm alg, byte[] key)
        {
            try
            {
                alg.Key = key;
            }
            catch
            {
                //dirty hack to allow weak keys:
                FieldInfo field = des_algorithm.GetType().GetField("KeyValue", BindingFlags.NonPublic | BindingFlags.Instance);
                field.SetValue(alg, key);
            }

            try
            {
                alg.IV = this.plugin.InputIV;
            }
            catch
            {
                //dirty hack to allow weak keys:
                FieldInfo field = des_algorithm.GetType().GetField("IVValue", BindingFlags.NonPublic | BindingFlags.Instance);
                field.SetValue(alg, this.plugin.InputIV);
            }

            switch (((DESSettings)plugin.Settings).Mode)
            { //0="ECB"=default, 1="CBC", 2="CFB", 3="OFB"
                case 1: alg.Mode = CipherMode.CBC; break;
                case 2: alg.Mode = CipherMode.CFB; break;
                case 3: alg.Mode = CipherMode.OFB; break;
                default: alg.Mode = CipherMode.ECB; break;
            }
            switch (((DESSettings)plugin.Settings).Padding)
            { //0="Zeros"=default, 1="None", 2="PKCS7"
                case 1: alg.Padding = PaddingMode.None; break;
                case 2: alg.Padding = PaddingMode.PKCS7; break;
                case 3: alg.Padding = PaddingMode.ANSIX923; break;
                case 4: alg.Padding = PaddingMode.ISO10126; break;
                default: alg.Padding = PaddingMode.Zeros; break;
            }

        }

        /// <summary>
        /// Called by DES if its status changes
        /// </summary>
        public void onStatusChanged()
        {
            if (OnStatusChanged != null)
                OnStatusChanged(this, true);
        }

        public string getKeyPattern()
        {
            return "[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-"
                +"[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]";
        }

        public byte[] getKeyFromString(string key, ref int[] arrayPointers, ref int[] arraySuccessors, ref int[] arrayUppers)
        {
            byte[] bkey = new byte[8];
            int counter = 0;
            bool allocated = false;

            for (int i = 0; i <= 7; i++)
            {
                string substr = key.Substring(i * 3, 2);

                if (!allocated && (substr[0] == '*' || substr[1] == '*'))
                {
                    arrayPointers = new int[8];
                    for (int j = 0; j < 8; j++)
                        arrayPointers[j] = -1;
                    arraySuccessors = new int[8];
                    arrayUppers = new int[8];
                    allocated = true;
                }

                if (substr[0] != '*' && substr[1] != '*')
                    bkey[i] = Convert.ToByte(substr, 16);
                else if (substr[0] == '*' && substr[1] == '*')
                {
                    bkey[i] = 0;
                    arrayPointers[counter] = i;
                    arraySuccessors[counter] = 1;
                    arrayUppers[counter] = 255;
                    counter++;
                }
                else if (substr[0] != '*' && substr[1] == '*')
                {
                    bkey[i] = Convert.ToByte(substr[0] + "0", 16);
                    arrayPointers[counter] = i;
                    arraySuccessors[counter] = 1;
                    arrayUppers[counter] = Convert.ToByte(substr[0] + "F", 16);
                    counter++;
                }
                else if (substr[0] == '*' && substr[1] != '*')
                {
                    bkey[i] = Convert.ToByte("0" + substr[1], 16);
                    arrayPointers[counter] = i;
                    arraySuccessors[counter] = 16;
                    arrayUppers[counter] = Convert.ToByte("F" + substr[1], 16);
                    counter++;
                }
            }
            return bkey;
        }

        public IControlEncryption clone()
        {
            DESControl des = new DESControl(plugin);
            CryptoolStream cs = new CryptoolStream();
            cs.OpenRead(InputStream.FileName);
            des.InputStream = cs;
            des.reset();
            return des;
        }

        public void Dispose()
        {
            closeStreams();
        }

        void IControlEncryption.changeSettings(string setting, object value)
        {
            throw new NotImplementedException();
        }

        public void setInput(object input)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}