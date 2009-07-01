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

namespace Cryptool.Trivium
{
    [Author("Soeren Rinne & Daehyun Strobel", "soeren.rinne@cryptool.de", "Ruhr-Universitaet Bochum, Chair for Embedded Security (EmSec)", "http://www.crypto.ruhr-uni-bochum.de/")]
    [PluginInfo(false, "Trivium", "Trivium", "Trivium/DetailedDescription/Description.xaml", "Trivium/icon.png", "Trivium/Images/encrypt.png", "Trivium/Images/decrypt.png")]
    [EncryptionType(EncryptionType.SymmetricBlock)]
    public class Trivium : IEncryption
    {
        #region IPlugin Members

        private TriviumSettings settings;
        private string inputString;
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

        [PropertyInfo(Direction.Input, "Input", "Data to be encrypted or decrypted.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public string InputString
        {
            get { return this.inputString; }
            set
            {
                this.inputString = value;
                OnPropertyChanged("InputString");
            }
        }

        [PropertyInfo(Direction.Input, "Key", "Must be 10 bytes (80 bit) in Hex.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public string InputKey
        {
            get { return this.inputKey; }
            set
            {
                this.inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }



        [PropertyInfo(Direction.Input, "IV", "Must be 10 bytes (80 bit) in Hex.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public string InputIV
        {
            get { return this.inputIV; }
            set
            {
                this.inputIV = value;
                OnPropertyChanged("InputIV");
            }
        }

        [PropertyInfo(Direction.Output, "Output stream", "Encrypted or decrypted output data", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
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

        private int[] hextobin(char[] hex)
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
                }
            }

            return bin;
        }

        private char[] bintohex(int[] bin)
        {
            int i;
            char[] hex = new char[bin.Length / 16];
            GuiLogMessage("output length: " + hex.Length, NotificationLevel.Info);
            string temp;

            for (i = 0; i < hex.Length; i++)
            {
                temp = null;
                temp += bin[i * 4];
                temp += bin[i * 4 + 1];
                temp += bin[i * 4 + 2];
                temp += bin[i * 4 + 3];

                switch (temp)
                {
                    case "0000":
                        hex[i] = '0';
                        break;
                    case "0001":
                        hex[i] = '1';
                        break;
                    case "0010":
                        hex[i] = '2';
                        break;
                    case "0011":
                        hex[i] = '3';
                        break;
                    case "0100":
                        hex[i] = '4';
                        break;
                    case "0101":
                        hex[i] = '5';
                        break;
                    case "0110":
                        hex[i] = '6';
                        break;
                    case "0111":
                        hex[i] = '7';
                        break;
                    case "1000":
                        hex[i] = '8';
                        break;
                    case "1001":
                        hex[i] = '9';
                        break;
                    case "1010":
                        hex[i] = 'a';
                        break;
                    case "1011":
                        hex[i] = 'b';
                        break;
                    case "1100":
                        hex[i] = 'c';
                        break;
                    case "1101":
                        hex[i] = 'd';
                        break;
                    case "1110":
                        hex[i] = 'e';
                        break;
                    case "1111":
                        hex[i] = 'f';
                        break;
                }
            }

            return hex;
        }


        public void Execute()
        {
            process(settings.Action);
        }

        private void process(int action)
        {
            //Encrypt/Decrypt String
            try
            {
                //read input
                int[] inputIntArray = new int[inputString.Length * 4];
                inputIntArray = hextobin(inputString.ToCharArray());
                long inputbytes = inputIntArray.Length;

                GuiLogMessage("input length [byte]: " + inputbytes, NotificationLevel.Info);

                long keybytes = inputKey.Length;
                int[] key = new int[keybytes];

                GuiLogMessage("inputKey length [byte]: " + keybytes.ToString(), NotificationLevel.Debug);

                if (keybytes != 20)
                {
                    GuiLogMessage("Given key has false length. Please provide a key with 20 Bytes length in Hex notation. Aborting now.", NotificationLevel.Error);
                    return;
                }

                long IVbytes = inputIV.Length;
                int[] IV = new int[IVbytes];
                GuiLogMessage("inputIV length [byte]: " + IVbytes.ToString(), NotificationLevel.Debug);

                if (IVbytes != 20)
                {
                    GuiLogMessage("Given IV has false length. Please provide an IV with 20 Bytes length in Hex notation. Aborting now.", NotificationLevel.Error);
                    return;
                }

                // convert from hex to binary
                IV = hextobin(inputIV.ToCharArray());
                key = hextobin(inputKey.ToCharArray());

                GuiLogMessage("length of IV: " + IV.Length, NotificationLevel.Info);
                GuiLogMessage("length of key: " + key.Length, NotificationLevel.Info);

                string keystream;

                // encryption/decryption
                DateTime startTime = DateTime.Now;

                if (action == 0)
                {
                    GuiLogMessage("Starting encryption [Keysize=80 Bits]", NotificationLevel.Info);
                    // init Trivium
                    initTrivium(IV, key);

                    // generate keystream with length of inputbytes
                    keystream = keystreamTrivium((int)inputbytes);
                    char[] charKeystream = keystream.ToCharArray();
                    int[] keystreamIntArray = hextobin(charKeystream);
                    int[] outputIntArray = new int[keystreamIntArray.Length];

                    char[] keyTemp = bintohex(keystreamIntArray);
                    string keyTempstr = null;

                    for (int i = 0; i < keyTemp.Length; i++)
                    {
                        keyTempstr += keyTemp[i];
                    }

                    GuiLogMessage("keystream: " + keyTempstr, NotificationLevel.Info);

                    // compute XOR with input and keystream
                    for (int i = 0; i < inputbytes; i++)
                    {
                        //generate XOR inputs
                        int one = inputIntArray[i];
                        int two = keystreamIntArray[i];
                        //GuiLogMessage("one: " + one + ", two: " + two, NotificationLevel.Info);
                        outputIntArray[i] = one ^ two;
                    }
                    char[] outputCharArray = bintohex(outputIntArray);
                    for (int i = 0; i < outputCharArray.Length; i++)
                    {
                        outputString += outputCharArray[i];
                    }
                    OnPropertyChanged("OutputString");
                } else if (action == 1) {
                    
                }

                DateTime stopTime = DateTime.Now;
                TimeSpan duration = stopTime - startTime;

                if (!stop)
                {
                    if (action == 0)
                    {
                        //GuiLogMessage("Encryption complete! (in: " + inputStream.Length.ToString() + " bytes, out: " + outbytes.ToString() + " bytes)", NotificationLevel.Info);
                    }
                    else
                    {
                        //GuiLogMessage("Decryption complete! (in: " + inputStream.Length.ToString() + " bytes, out: " + outbytes.ToString() + " bytes)", NotificationLevel.Info);
                    }
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

        private void initTrivium(int[] IV, int[] key)
        {
	        int i,j;
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

            for (i = 0; i < 1152; i++) // 1152 = 4 * 288
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

        private string keystreamTrivium(int nBits)
        {
            int i, j;
            uint z;

            string keystreamZ = null;

            for (i = 0; i < nBits; i++)
            {
                t1 = a[65] ^ a[92];
                t2 = b[68] ^ b[83];
                t3 = c[65] ^ c[110];
                z = t1 ^ t2 ^ t3;

                keystreamZ += z;

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
    }

    enum TriviumImage
    {
        Default,
        Encode,
        Decode
    }
}
