using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cryptool.PluginBase;
using System.IO;
using System.ComponentModel;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
using Cryptool.PluginBase.Miscellaneous;
using System.Security.Cryptography;
// for IControl
using Cryptool.PluginBase.Control;
// reference to the TriviumController interface (own dll)
using Cryptool.TriviumController;

namespace Cryptool.Trivium
{
    [Author("Soeren Rinne, David Oruba & Daehyun Strobel", "soeren.rinne@cryptool.de", "Ruhr-Universitaet Bochum, Chair for Embedded Security (EmSec)", "http://www.trust.ruhr-uni-bochum.de/")]
    [PluginInfo(false, "Trivium", "Trivium", "Trivium/DetailedDescription/Description.xaml", "Trivium/icon.png", "Trivium/Images/encrypt.png", "Trivium/Images/decrypt.png")]
    [EncryptionType(EncryptionType.SymmetricBlock)]
    public class Trivium : IEncryption
    {
        #region IPlugin Members

        private TriviumSettings settings;
        private string inputString = null;
        private string outputString;
        private string inputKey;
        private string inputIV;
        private bool stop = false;
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();

        #endregion

        #region Public Variables
        public uint[] a = new uint[93];
        public uint[] b = new uint[84];
        public uint[] c = new uint[111];
        public uint t1, t2, t3;
        public int masterSlaveRounds = 0;
        #endregion

        public Trivium()
        {
            this.settings = new TriviumSettings();
            //((TriviumSettings)(this.settings)).LogMessage += Trivium_LogMessage;
        }

        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (TriviumSettings)value; }
        }

        [PropertyInfo(Direction.InputData, "Input", "Data to be encrypted or decrypted.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public string InputString
        {
            get { return this.inputString; }
            set
            {
                this.inputString = value;
                OnPropertyChanged("InputString");
            }
        }

        [PropertyInfo(Direction.InputData, "Key", "Must be 10 bytes (80 bit) in Hex.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public string InputKey
        {
            get { return this.inputKey; }
            set
            {
                this.inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.InputData, "IV", "Must be 10 bytes (80 bit) in Hex.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public string InputIV
        {
            get { return this.inputIV; }
            set
            {
                this.inputIV = value;
                OnPropertyChanged("InputIV");
            }
        }

        [PropertyInfo(Direction.OutputData, "Output stream", "Encrypted or decrypted output data", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                this.outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        public void Dispose()
        {
            try
            {
                stop = false;
                inputKey = null;
                outputString = null;
                inputString = null;

                foreach (CryptoolStream stream in listCryptoolStreamsOut)
                {
                    stream.Close();
                }
                listCryptoolStreamsOut.Clear();
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
            this.stop = false;
        }

        public int[] hextobin(char[] hex)
        {
            int i;
            int[] bin = new int[hex.Length * 4];

            for (i = 0; i < hex.Length; i++)
            {
                switch (hex[i])
                {
                    case '0':
                        bin[i * 4] = 0;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 0;
                        break;
                    case '1':
                        bin[i * 4] = 0;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 1;
                        break;
                    case '2':
                        bin[i * 4] = 0;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 0;
                        break;
                    case '3':
                        bin[i * 4] = 0;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 1;
                        break;
                    case '4':
                        bin[i * 4] = 0;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 0;
                        break;
                    case '5':
                        bin[i * 4] = 0;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 1;
                        break;
                    case '6':
                        bin[i * 4] = 0;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 0;
                        break;
                    case '7':
                        bin[i * 4] = 0;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 1;
                        break;
                    case '8':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 0;
                        break;
                    case '9':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 1;
                        break;
                    case 'a':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 0;
                        break;
                    case 'b':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 1;
                        break;
                    case 'c':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 0;
                        break;
                    case 'd':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 1;
                        break;
                    case 'e':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 0;
                        break;
                    case 'f':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 1;
                        break;
                    case 'A':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 0;
                        break;
                    case 'B':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 1;
                        break;
                    case 'C':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 0;
                        break;
                    case 'D':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 1;
                        break;
                    case 'E':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 0;
                        break;
                    case 'F':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 1;
                        break;
                }
            }

            return bin;
        }

        public string bintohex(string bin)
        {
            int i;
            string hex = null;
            string temp;

            for (i = 0; i < bin.Length / 4; i++)
            {
                temp = null;
                temp += bin[i * 4];
                temp += bin[i * 4 + 1];
                temp += bin[i * 4 + 2];
                temp += bin[i * 4 + 3];

                switch (temp)
                {
                    case "0000":
                        hex += '0';
                        break;
                    case "0001":
                        hex += '1';
                        break;
                    case "0010":
                        hex += '2';
                        break;
                    case "0011":
                        hex += '3';
                        break;
                    case "0100":
                        hex += '4';
                        break;
                    case "0101":
                        hex += '5';
                        break;
                    case "0110":
                        hex += '6';
                        break;
                    case "0111":
                        hex += '7';
                        break;
                    case "1000":
                        hex += '8';
                        break;
                    case "1001":
                        hex += '9';
                        break;
                    case "1010":
                        hex += 'A';
                        break;
                    case "1011":
                        hex += 'B';
                        break;
                    case "1100":
                        hex += 'C';
                        break;
                    case "1101":
                        hex += 'D';
                        break;
                    case "1110":
                        hex += 'E';
                        break;
                    case "1111":
                        hex += 'F';
                        break;
                }
            }

            return hex;
        }

        // TODO: check if input is boolean or hex
        // returns -1 if no hex
        // returns 1 is hex
        private int checkInput(string input)
        {
            int returnValue = 1;
            return returnValue;
        }


        public void Execute()
        {
            process(1);
        }

        private void process(int padding)
        {
            //Encrypt/Decrypt String
            try
            {
                // a test vector; should have the following output:
                // FC9659CB953A37F...
                //string IV_string = "288ff65dc42b92f960c7";
                //string key_string = "0f62b5085bae0154a7fa";
                string input_string = inputString;

                // check input string
                if (checkInput(input_string) == -1)
                    return;

                string IV_string = inputIV;
                string key_string = inputKey;

                int[] IV = new int[IV_string.Length * 4];
                int[] key = new int[key_string.Length * 4];
                
                IV = hextobin(IV_string.ToCharArray());
                key = hextobin(key_string.ToCharArray());

                GuiLogMessage("length of IV: " + IV.Length, NotificationLevel.Info);
                GuiLogMessage("length of key: " + key.Length, NotificationLevel.Info);

                // test if padding to do
                int bitsToPad = (32 - input_string.Length % 32) % 32;
                GuiLogMessage("Bits to pad: " + bitsToPad, NotificationLevel.Info);
                // pad partial block with zeros
                if (bitsToPad != 0)
                {
                    for (int i = 0; i < bitsToPad; i++)
                    {
                        input_string += "0";
                    }
                }

                string keystream;

                int keyStreamLength = settings.KeystreamLength;
                if ((keyStreamLength % 32) != 0)
                {
                    GuiLogMessage("Keystream length must be a multiple of 32. " + keyStreamLength + " != 32", NotificationLevel.Error);
                    return;
                }

                // encryption/decryption
                DateTime startTime = DateTime.Now;

                GuiLogMessage("Starting encryption [Keysize=80 Bits]", NotificationLevel.Info);

                // init Trivium
                initTrivium(IV, key);

                // generate keystream with given length (TODO: padding if inputstring % 32 != 0)
                // ACHTUNG, mag keine grossen zahlen als inputs
                // EDIT 30.07.09: Jetzt mag er große Zahlen ;)
                if (settings.KeystreamLength <= 0)
                {
                    keystream = keystreamTrivium(input_string.Length);
                    //GuiLogMessage("DEBUG: inputString.Length + bitsToPad: " + (inputString.Length + bitsToPad), NotificationLevel.Info);
                }
                else
                {
                    keystream = keystreamTrivium(settings.KeystreamLength);
                }
                
                DateTime stopTime = DateTime.Now;
                TimeSpan duration = stopTime - startTime;

                if (!settings.HexOutput)
                {
                    outputString = keystream;
                    GuiLogMessage("Keystream hex: " + bintohex(keystream), NotificationLevel.Info);
                }
                else
                {
                    outputString = bintohex(keystream);
                    GuiLogMessage("Keystream binary: " + keystream, NotificationLevel.Info);
                }
                OnPropertyChanged("OutputString");

                if (!stop)
                {
                    GuiLogMessage("Encryption complete in " + duration + "! (input length : " + input_string.Length + ", keystream/output length: " + keystream.Length + " bit)", NotificationLevel.Info);
                }

                if (stop)
                {
                    GuiLogMessage("Aborted!", NotificationLevel.Info);
                }
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

        public void initTrivium(int[] IV, int[] key)
        {
	        int i,j;

            int[] buffer = new int[8];

            if (settings.UseByteSwapping)
            {
                // Byte-Swapping Key
                for (int l = 0; l < 10; l++)
                {
                    for (int k = 0; k < 8; k++)
                        buffer[k] = key[((l * 8) + 7) - k];
                    for (int k = 0; k < 8; k++)
                        key[(l * 8) + k] = buffer[k];
                }

                // Byte-Swapping IV
                for (int l = 0; l < 10; l++)
                {
                    for (int k = 0; k < 8; k++)
                        buffer[k] = IV[((l * 8) + 7) - k];
                    for (int k = 0; k < 8; k++)
                        IV[(l * 8) + k] = buffer[k];
                }
            }

	        for (i = 0; i < 80; i++){
		        a[i] = (uint)key[i]; // hier key rein als binär
		        b[i] = (uint)IV[i]; // hier IV rein als binär
		        c[i] = 0;
	        }
	        while (i < 84){
		        a[i] = 0;
		        b[i] = 0;
		        c[i] = 0;
		        i++;
	        }
	        while (i < 93){
		        a[i] = 0;
		        c[i] = 0;
		        i++;
	        }
	        while (i < 108){
		        c[i] = 0;
		        i++;
	        }
	        while (i < 111){
		        c[i] = 1;
		        i++;
	        }

            // belegung fertig, jetzt takten ohne output
            // anzahl der takte laut settings oder lut master/slave
            int myRounds;

            if (masterSlaveRounds != 0)
                myRounds = masterSlaveRounds;
            else
                myRounds = settings.InitRounds;

            for (i = 0; i < myRounds; i++) // default 1152 = 4 * 288
            {
                t1 = a[65] ^ (a[90] & a[91]) ^ a[92] ^ b[77];
                t2 = b[68] ^ (b[81] & b[82]) ^ b[83] ^ c[86];
                t3 = c[65] ^ (c[108] & c[109]) ^ c[110] ^ a[68];
                for (j = 92; j > 0; j--)
                    a[j] = a[j - 1];
                for (j = 83; j > 0; j--)
                    b[j] = b[j - 1];
                for (j = 110; j > 0; j--)
                    c[j] = c[j - 1];
                a[0] = t3;
                b[0] = t1;
                c[0] = t2;
            }
        }

        public string keystreamTrivium(int nBits)
        {
            int i, j;
            uint z;

            string keystreamZ = null;
            List<int> keyOutput = new List<int>();

            for (i = 0; i < nBits; i++)
            {
                t1 = a[65] ^ a[92];
                t2 = b[68] ^ b[83];
                t3 = c[65] ^ c[110];
                z = t1 ^ t2 ^ t3;

                if (!settings.UseByteSwapping)
                    keystreamZ += z;
                else
                    keyOutput.Add((int)z);

                t1 = t1 ^ (a[90] & a[91]) ^ b[77];
                t2 = t2 ^ (b[81] & b[82]) ^ c[86];
                t3 = t3 ^ (c[108] & c[109]) ^ a[68];
                for (j = 92; j > 0; j--)
                    a[j] = a[j - 1];
                for (j = 83; j > 0; j--)
                    b[j] = b[j - 1];
                for (j = 110; j > 0; j--)
                    c[j] = c[j - 1];
                a[0] = t3;
                b[0] = t1;
                c[0] = t2;
            }

            if (settings.UseByteSwapping)
            {
                int[] temp = new int[nBits];

                // Little-Endian für den Keystream
                for (int k = 0; k < nBits; k++)
                    temp[k] = keyOutput[k];
                for (int l = 0; l < nBits / 32; l++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        keyOutput[(l * 32) + k] = (char)temp[(l * 32) + 24 + k];
                        keyOutput[(l * 32) + 8 + k] = (char)temp[(l * 32) + 16 + k];
                        keyOutput[(l * 32) + 16 + k] = (char)temp[(l * 32) + 8 + k];
                        keyOutput[(l * 32) + 24 + k] = (char)temp[(l * 32) + k];
                    }
                }
                GuiLogMessage(keyOutput.Count.ToString(), NotificationLevel.Info);
                foreach (int k in keyOutput)
                {
                    keystreamZ += k.ToString();
                }
            }

            return keystreamZ;
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

        public void Initialize()
        {
        }

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

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            Dispose();
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void Stop()
        {
            this.stop = true;
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            /*if (PropertyChanged != null)
            {
              PropertyChanged(this, new PropertyChangedEventArgs(name));
            }*/
        }

        private void StatusChanged(int imageIndex)
        {
            EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, imageIndex));
        }

        #endregion

        #region IControl

        private IControlTrivium triviumSlave;
        [PropertyInfo(Direction.ControlSlave, "Trivium Slave", "Direct access to Trivium.", "", DisplayLevel.Beginner)]
        public IControlTrivium TriviumSlave
        {
            get
            {
                if (triviumSlave == null)
                    triviumSlave = new TriviumControl(this);
                return triviumSlave;
            }
        }

        #endregion
    }

    #region TriviumControl : IControlTrivium

    public class TriviumControl : IControlTrivium
    {
        public event IControlStatusChangedEventHandler OnStatusChanged;
        private Trivium plugin;
        private TriviumSettings pluginSettings;

        public TriviumControl(Trivium Plugin)
        {
            this.plugin = Plugin;
        }

        public TriviumControl(TriviumSettings PluginSettings)
        {
            this.pluginSettings = PluginSettings;
        }

        #region IControlEncryption Members

        // here comes the slave side implementation
        public int GenerateTriviumKeystream(int[] IV, int[] key, int length, int rounds, bool byteSwapping)
        {
            string resultString;
            int resultInt;
            //pluginSettings.InitRounds = rounds;

            if (key == null)
            {
                key = new int[((TriviumSettings)plugin.Settings).InputKey.Length * 4];
                key = plugin.hextobin(((TriviumSettings)plugin.Settings).InputKey.ToCharArray());
                // key = new int[] { 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 0, 1, 0, 1 };
            }
            
            plugin.masterSlaveRounds = rounds;
            plugin.initTrivium(IV, key);

            resultString = plugin.keystreamTrivium(length);

            return resultInt = Int32.Parse(resultString.Substring(resultString.Length - 1, 1));
        }

        #endregion
    }

    #endregion

    enum TriviumImage
    {
        Default,
        Encode,
        Decode
    }
}
