using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
using Cryptool.PluginBase.Control;
using System.Reflection;
using NativeCryptography;

namespace Cryptool.Plugins.Cryptography.Encryption
{
    [Author("Dr. Arno Wacker", "arno.wacker@cryptool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("Cryptool.Plugins.Cryptography.Encryption.Properties.Resources", "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "DES/icon.png", "DES/Images/encrypt.png", "DES/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class DES : ICrypComponent
    {
        #region Private variables
        private DESSettings settings;
        private byte[] inputKey;
        private byte[] inputIV;
        private CStreamWriter writer;
        private CryptoStream p_crypto_stream;
        private bool stop = false;
        private IControlEncryption controlSlave;
        #endregion


        #region Initialisation
        
        public DES()
        {
            this.settings = new DESSettings();
        }


        

        #endregion

        #region External connection properties

        [PropertyInfo(Direction.ControlSlave, "ControlSlaveCaption", "ControlSlaveTooltip")]
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
        [PropertyInfo(Direction.ControlSlave, "DESSlaveCaption", "DESSlaveTooltip")]
        public IControlCubeAttack DESSlave
        {
            get
            {
                if (desSlave == null)
                    desSlave = new CubeAttackControl(this);
                return desSlave;
            }
        }


        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", true)]
        public ICryptoolStream InputStream
        {
            get;
            set;
                }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public byte[] InputKey
        {
            get { return this.inputKey; }
            set
            {
                this.inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", "InputIVTooltip", false)]
        public byte[] InputIV
        {
            get { return this.inputIV; }
            set
            {
                this.inputIV = value;
                OnPropertyChanged("InputIV");
            }
        }
        
        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICryptoolStream OutputStream
        {
            get
            {
                return writer;
                }
            }

        #endregion       

        #region Public IPlugin Member

        #region Events
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        #endregion

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (DESSettings)value; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void Initialize()
        {
        }

        public void Execute()
        {
            process(settings.Action);
        }


        public void Stop()
        {
            this.stop = true;
        }

        public void Dispose()
        {
            try
            {
                stop = false;
                inputKey = null;
                inputIV = null;

                if (writer != null)
                {
                    writer.Dispose();
                    writer = null;
                }

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

           

        #region Public DES specific members

        public bool isStopped()
        {

            return this.stop;
        }

        #endregion


        #region Private methods

        private void ConfigureAlg(SymmetricAlgorithm alg)
        {
            //check for a valid key
           if (this.inputKey == null)
            {
                //create a trivial key 
                inputKey = new byte[8];
                // write a warning to the ouside world
                GuiLogMessage("ERROR: No key provided. Using 0x000..00!", NotificationLevel.Error);
            }
            alg.Key = this.inputKey;

            //check for a valid IV
            if (this.inputIV == null)
            {
                //create a trivial key 
                inputIV = new byte[alg.BlockSize / 8];
                GuiLogMessage("NOTE: No IV provided. Using 0x000..00!", NotificationLevel.Info);
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

        private void process(int action)
        {
            //Encrypt/Decrypt Stream
            try
            {
                if (InputStream == null || InputStream.Length == 0)
                {
                    GuiLogMessage("No input data, aborting now", NotificationLevel.Error);
                    return;
                }
                
                SymmetricAlgorithm p_alg = null;
                if (((DESSettings)settings).TripleDES)
                {
                    p_alg = new TripleDESCryptoServiceProvider(); 
                }
                else
                {
                    p_alg = new DESCryptoServiceProvider();
                }
               

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

                writer = new CStreamWriter();
                using (CStreamReader reader = InputStream.CreateReader())
                {
                    p_crypto_stream = new CryptoStream((Stream)reader, p_encryptor, CryptoStreamMode.Read);
                    byte[] buffer = new byte[p_alg.BlockSize / 8];
                    int bytesRead;
                    int position = 0;
                    GuiLogMessage("Starting encryption [Keysize=" + p_alg.KeySize.ToString() + " Bits, Blocksize=" + p_alg.BlockSize.ToString() + " Bits]", NotificationLevel.Info);
                    DateTime startTime = DateTime.Now;
                    while ((bytesRead = p_crypto_stream.Read(buffer, 0, buffer.Length)) > 0 && !stop)
                    {
                            writer.Write(buffer, 0, bytesRead);

                            if ((int)(reader.Position * 100 / reader.Length) > position)
                        {
                                position = (int)(reader.Position * 100 / reader.Length);
                                ProgressChanged(reader.Position, reader.Length);
                        }
                    }

                    p_crypto_stream.Flush();
                    DateTime stopTime = DateTime.Now;
                    TimeSpan duration = stopTime - startTime;
                    if (!stop)
                    {
                            GuiLogMessage("Encryption complete! (in: " + reader.Length.ToString() + " bytes, out: " + writer.Length.ToString() + " bytes)", NotificationLevel.Info);
                        GuiLogMessage("Time used: " + duration.ToString(), NotificationLevel.Debug);
                            writer.Close();
                        OnPropertyChanged("OutputStream");
                    }
                    if (stop)
                    {
                            writer.Close();
                        GuiLogMessage("Aborted!", NotificationLevel.Info);
                    }
                }
                ProgressChanged(1, 1);

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

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }


        private void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
            }
        }

        #endregion

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

        public int GenerateBlackboxOutputBit(int[] IV, int[] key, int length)
        {
            // public bits := plaintext
            // secret bits := key 
            SymmetricAlgorithm p_alg = new DESCryptoServiceProvider();
            string secretBits = string.Empty;
            string publicBits = string.Empty;

            // save public and secret bits as string
            for (int i = 0; i < key.Length; i++)
                secretBits += key[i];
            for (int i = 0; i < IV.Length; i++)
                publicBits += IV[i];

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
            return Int32.Parse(strBytes.Substring(length-1, 1));
        }
        #endregion
    }

    #endregion


    #region DESControl : IControlEncryption
    public class DESControl : IControlEncryption
    {
        public event KeyPatternChanged keyPatternChanged;
        public event IControlStatusChangedEventHandler OnStatusChanged;

        
        private DES plugin;

        public DESControl(DES Plugin)
        {
            this.plugin = Plugin;

            // Change the padding mode to zeroes, since we want to do bruteforcing..
            ((DESSettings)plugin.Settings).Padding = 0;

            ((DESSettings)plugin.Settings).PropertyChanged += new PropertyChangedEventHandler(DESControl_PropertyChanged);
        }

        void DESControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("TripleDES"))
            {
                if (keyPatternChanged != null)
                {
                    keyPatternChanged();
                }
            }
        }       

        public byte[] Encrypt(byte[] key, int blocksize)
        {
            // not implemented, currently not needed
            throw new NotImplementedException();
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV)
        {
            return Decrypt(ciphertext, key, IV, ciphertext.Length);
        }

        // TODO: add override with iv, mode, blocksize
        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV, int bytesToUse)
        {
            int size = bytesToUse > ciphertext.Length ? ciphertext.Length : bytesToUse;
            if (((DESSettings)plugin.Settings).TripleDES)
            {
                return NativeCryptography.Crypto.decryptTripleDES(ciphertext, key, IV, size, ((DESSettings)plugin.Settings).Mode);
            }
            else
            {
                return NativeCryptography.Crypto.decryptDES(ciphertext, key, IV, size, ((DESSettings)plugin.Settings).Mode);
            }
        }

        public int GetBlockSize()
        {
            return 8;
        }

        public string GetKeyPattern()
        {
            if (((DESSettings)plugin.Settings).TripleDES)
            {
                return "[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-"
               + "[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-"
               + "[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-" 
               + "[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-" 
               + "[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-" 
               + "[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]";
            }
            else
            {
                return "[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-"
                + "[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]-[0-9A-F][0-9A-F]";
            }            
        }

        public IControlEncryption clone()
        {
            DESControl des = new DESControl(plugin);
            return des;
        }

        public void Dispose()
        {
        }

        private const int DesBlocksize = 8;

        public string GetOpenCLCode(int decryptionLength, byte[] iv)
        {

            if (((DESSettings)plugin.Settings).TripleDES)
            {
                throw new NotImplementedException("Triple DES is not implemented in OpenCL yet");
            }

            string opencl = Properties.Resources.DESOpenCL;

            //if there is a relevant IV:
            bool useIV = false;
            if (iv != null && iv.Length != 0 && iv.Count(x => x != 0) > 0)
            {
                useIV = true;
                string IV = "";
                foreach (byte i in iv)
                {
                    IV += string.Format("0x{0:X}, ", i);
                }
                IV = IV.Substring(0, IV.Length - 2);
                opencl = opencl.Replace("$$IVARRAY$$", string.Format("__constant u8 IV[] = {{ {0} }};", IV));
            }
            else
            {
                opencl = opencl.Replace("$$IVARRAY$$", "");
            }

            string decryptionCode = string.Format("int decryptionLength = {0}; \n", decryptionLength);
            int blocks = decryptionLength / DesBlocksize;
            if (blocks >= 1)
            {
                if (!useIV)
                    decryptionCode = AddOpenCLBlockDecryption(decryptionCode, DesBlocksize);
                else
                    decryptionCode = AddOpenCLBlockDecryptionWithIV(decryptionCode, DesBlocksize);

                if (blocks > 1)
                {
                    decryptionCode += string.Format("for (int b = 1; b < {0}; b++) \n {{ \n ", blocks);
                    decryptionCode = AddOpenCLBlockDecryptionWithMode(decryptionCode, DesBlocksize, "b");
                    decryptionCode += "}\n";
                }
            }

            if (decryptionLength % DesBlocksize != 0)
            {
                if (blocks == 0)
                {
                    if (!useIV)
                        decryptionCode = AddOpenCLBlockDecryption(decryptionCode, decryptionLength % DesBlocksize);
                    else
                        decryptionCode = AddOpenCLBlockDecryptionWithIV(decryptionCode, decryptionLength % DesBlocksize);
                }
                else
                    decryptionCode = AddOpenCLBlockDecryptionWithMode(decryptionCode, decryptionLength % DesBlocksize, "" + blocks);
            }

            opencl = opencl.Replace("$$DESDECRYPT$$", decryptionCode);

            return opencl;
        }

        private string AddOpenCLBlockDecryption(string decryptionCode, int size)
        {
            decryptionCode += "DES_ecb_encrypt(inn, block, &(key)); \n "
                              + string.Format("for (int i = 0; i < {0}; i++) \n ", size)
                              + "{ \n unsigned char c = block[i]; \n "
                              + "$$COSTFUNCTIONCALCULATE$$ \n } \n";
            return decryptionCode;
        }

        private string AddOpenCLBlockDecryptionWithIV(string decryptionCode, int size)
        {
            switch (((DESSettings)plugin.Settings).Mode)
            {
                case 0: //ECB
                    return AddOpenCLBlockDecryption(decryptionCode, size);
                case 1: //CBC
                    decryptionCode += "DES_ecb_encrypt(inn, block, &(key)); \n "
                              + string.Format("for (int i = 0; i < {0}; i++) \n ", size)
                              + "{ \n unsigned char c = block[i] ^ IV[i]; \n "
                              + "$$COSTFUNCTIONCALCULATE$$ \n } \n";
                    return decryptionCode;
                    break;
                case 2: //CFB
                    throw new NotImplementedException("CFB for OpenCL is not implemented!"); //not supported
                default:
                    throw new NotImplementedException("Mode not supported by OpenCL!");
            }
        }

        private string AddOpenCLBlockDecryptionWithMode(string decryptionCode, int size, string block)
        {
            decryptionCode += string.Format("DES_ecb_encrypt((inn+{0}*DES_BLOCKSIZE), block, &(key)); \n ", block)
                              + string.Format("for (int i = 0; i < {0}; i++) \n {{ \n ", size);
            switch (((DESSettings)plugin.Settings).Mode)
            {
                case 0: //ECB
                    decryptionCode += "unsigned char c = block[i]; \n";
                    break;
                case 1: //CBC
                    decryptionCode += string.Format("unsigned char c = block[i] ^ (inn+({0}-1)*DES_BLOCKSIZE)[i]; \n", block);
                    break;
                case 2: //CFB
                    throw new NotImplementedException("CFB for OpenCL is not implemented!"); //not supported
                default:
                    throw new NotImplementedException("Mode not supported by OpenCL!");
            }
            decryptionCode += "$$COSTFUNCTIONCALCULATE$$ \n } \n";
            return decryptionCode;
        }

        public void changeSettings(string setting, object value)
        {
            throw new NotImplementedException();
        }

        public IKeyTranslator GetKeyTranslator()
        {
            return new KeySearcher.KeyTranslators.ByteArrayKeyTranslator();
        }
    }
    #endregion
}
