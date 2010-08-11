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
using System.IO;
using System.Security.Cryptography;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
using Cryptool.PluginBase.Control;
using System.Threading;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace Cryptool.Plugins.Cryptography.Encryption
{
    /// <summary>
    /// This plugin encrypts / decrypts texts with the simplified DES alogrithm (SDES)
    /// It can be used as plugin in a normal encryption/decryption chanin or be 
    /// used by the KeySearcher to do bruteforcing
    /// </summary>
    [Author("Nils Kopal", "nils.kopal@cryptool.de", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo(false, "SDES", "Simplified Data Encryption Standard", "SDES/DetailedDescription/Description.xaml", "SDES/icon.png", "SDES/Images/encrypt.png", "SDES/Images/decrypt.png")]
    [EncryptionType(EncryptionType.SymmetricBlock)]
    public class SDES : IEncryption
    {
        #region Private variables

        private SDESSettings settings;
        private CryptoolStream inputStream;
        private CryptoolStream outputStream;
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
        private byte[] inputKey;
        private byte[] inputIV;        
        private bool stop = false;
        private UserControl presentation = new SDESPresentation();
        private SDESControl controlSlave;

        #endregion

        #region events

        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion

        #region public

        /// <summary>
        /// Tells you wether input changed or not
        /// </summary>
        public bool InputChanged
        { get; set; }

        /// <summary>
        /// Constructs a new SDES
        /// </summary>
        public SDES()
        {
            InputChanged = false;
            this.settings = new SDESSettings();
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        /// <summary>        
        /// The status of the plugin changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if(OnPluginStatusChanged != null)OnPluginStatusChanged(this, args);
        }

        /// <summary>
        /// Sets/Gets the settings of this plugin
        /// </summary>
        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (SDESSettings)value; }
        }
    
        /// <summary>
        /// Is this Plugin in Status stop?
        /// </summary>
        /// <returns></returns>
        public bool getStop()
        {
            return stop;
        }

        /// <summary>
        /// Gets/Sets the input of the SDES plugin (the text which should be encrypted/decrypted)
        /// </summary>
        [PropertyInfo(Direction.InputData, "Input", "Data to be encrypted or decrypted", null, true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public CryptoolStream InputStream
        {
            get 
            {
                try
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
                catch (Exception ex)
                {
                    GuiLogMessage("getInputStream: " + ex.Message, NotificationLevel.Error);
                    return null;
                }

            }
            set 
            {
                this.inputStream = value;
                if (value != null)
                {
                    listCryptoolStreamsOut.Add(value);                      
                }
                OnPropertyChanged("InputStream");
            }
        }

        /// <summary>
        /// Gets/Sets the output of the SDES plugin (the text which is encrypted/decrypted)
        /// </summary>
        [PropertyInfo(Direction.OutputData, "Output stream", "Encrypted or decrypted output data", null, true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public CryptoolStream OutputStream
        {
            get
            {
                try
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
                catch (Exception ex)
                {
                    GuiLogMessage("getOutputStream: " + ex.Message, NotificationLevel.Error);
                    return null;
                }
            }
            set
            {

                this.outputStream = value;
                if (value != null)
                {
                    listCryptoolStreamsOut.Add(value);
                }
                OnPropertyChanged("OutputStream");
            }
        }

        /// <summary>
        /// Gets/Sets the key which should be used.Must be 10 bytes  (only 1 or 0 allowed).
        /// </summary>
        [PropertyInfo(Direction.InputData, "Key", "Must be 10 bytes (only 1 or 0 allowed).", null, true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public byte[] InputKey
        {
            get { return this.inputKey; }
            set
            {
                InputChanged = true;
                this.inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        /// <summary>
        /// Gets/Sets the Initialization Vector which should be used.Must be 8 bytes  (only 1 or 0 allowed).
        /// </summary>
        [PropertyInfo(Direction.InputData, "IV", "IV to be used in chaining modes, must be 8 bytes (only 1 or 0 allowed).", null, true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public byte[] InputIV
        {
            get { return this.inputIV; }
            set
            {
                this.inputIV = value;
                OnPropertyChanged("InputIV");
            }
        }

        /// <summary>
        /// Start encrypting
        /// </summary>
        public void Encrypt()
        {
            //Encrypt Stream
            process(0);
        }

        /// <summary>
        /// Start decrypting
        /// </summary>
        public void Decrypt()
        {
            //Decrypt Stream
            process(1);
        }

        /// <summary>
        /// Called by the environment to start this plugin
        /// </summary>
        public void Execute()
        {
            process(settings.Action);
        }


        /// <summary>
        /// Get the Presentation of this plugin
        /// </summary>
        public UserControl Presentation
        {
            get { return this.presentation; }
        }

        /// <summary>
        /// Get the QuickWatchPresentation of this plugin
        /// </summary>
        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        /// <summary>
        /// Called by the environment to do initialization
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called by the envorinment if this plugin is unloaded
        /// closes all streams
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.stop = false;
                inputKey = null;
                inputIV = null;

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
        }

        /// <summary>
        /// Called by the environment of this plugin to stop it
        /// </summary>
        public void Stop()
        {
            this.stop = true;
        }

        /// <summary>
        /// Called by the environment of this plugin after execution
        /// </summary>
        public void PostExecution()
        {
            Dispose();
        }

        /// <summary>
        /// Called by the environment of this plugin before execution
        /// </summary>
        public void PreExecution()
        {
            Dispose();
        }

        /// <summary>
        /// A property of this plugin changed
        /// </summary>
        /// <param name="name">propertyname</param>
        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// Logs a message into the messages of crypttool
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="logLevel">logLevel</param>
        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        /// <summary>
        /// Sets the current progess of this plugin
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="max">max</param>
        public void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
            }
        }

        /// <summary>
        /// Called by the environment of this plugin if it is set to pause
        /// </summary>
        public void Pause()
        {

        }

        /// <summary>
        /// Sets/Gets the ControlSlave of this plugin
        /// </summary>
        [PropertyInfo(Direction.ControlSlave, "SDES Slave", "Direct access to SDES.", "", DisplayLevel.Beginner)]
        public IControlEncryption ControlSlave
        {
            get
            {
                if (controlSlave == null)
                    controlSlave = new SDESControl(this);
                return controlSlave;
            }
        } 

        #endregion public

        #region private

        /// <summary>
        /// This method checks if the input stream is valid. If it is not valid it sets it to a dummy stream
        /// (this funcionality is stolen from DES plugin ;) )
        /// </summary>
        private void checkForInputStream()
        {
            if (settings.Action == 0 && (inputStream == null || (inputStream != null && inputStream.Length == 0)))
            {
                //create some input
                String dummystring = "Dummy string - no input provided - \"Hello SDES World\" - dummy string - no input provided!";
                this.inputStream = new CryptoolStream();
                this.inputStream.OpenRead(this.GetPluginInfoAttribute().Caption, Encoding.Default.GetBytes(dummystring.ToCharArray()));
                // write a warning to the ouside word
                GuiLogMessage("WARNING - No input provided. Using dummy data. (" + dummystring + ")", NotificationLevel.Warning);
            }
        }

        /// <summary>
        /// Checks if the Input key is not null and has length 10 and only contains 1s and 0s
        /// and if Input IV is not null and has length 8 and only contains 1s and 0s
        /// </summary>
        /// <returns>true if ok</returns>
        private bool areKeyAndIVValid()
        {

            if (this.inputKey == null || this.inputKey.Length != 10)
            {
                GuiLogMessage("The Key has to have the length of 10 bytes (containing only '1' and '0')", NotificationLevel.Error);
                return false;
            }
            if (this.inputIV == null || this.inputIV.Length != 8)
            {
                GuiLogMessage("The IV has to have the length of 8 bytes (containing only '1' and '0')", NotificationLevel.Error);
                return false;
            }

            foreach (char character in inputKey)
            {
                if (character != '0' && character != '1')
                {
                    GuiLogMessage("Invalid character in Key: '" + character + "' - may only contain '1' and '0'", NotificationLevel.Error);
                    return false;
                }
            }

            foreach (char character in inputIV)
            {
                if (character != '0' && character != '1')
                {
                    GuiLogMessage("Invalid character in IV: '" + character + "' - may only contain '1' and '0'", NotificationLevel.Error);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Starts the encryption/decryption process with SDES
        /// </summary>
        /// <param name="action">0 = encrypt, 1 = decrypt</param>
        private void process(int action)
        {
            if (controlSlave is object && InputStream is object && InputIV is object)
            {
                controlSlave.onStatusChanged();
            }

            try
            {
                checkForInputStream();
                if (!areKeyAndIVValid())
                {
                    return;
                }

                if (inputStream == null || (inputStream != null && inputStream.Length == 0))
                {
                    GuiLogMessage("No input given. Not using dummy data in decrypt mode. Aborting now.", NotificationLevel.Error);
                    return;
                }

                if (this.inputStream.CanSeek) this.inputStream.Position = 0;
                
                outputStream = new CryptoolStream();
                listCryptoolStreamsOut.Add(outputStream);
                outputStream.OpenWrite(this.GetPluginInfoAttribute().Caption);
                
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();               
                DateTime startTime = DateTime.Now;

                //Encrypt
                if(action == 0){
                    
                    if (this.settings.Mode == 0)
                    {
                        GuiLogMessage("Starting encryption with ecb", NotificationLevel.Info);
                        ElectronicCodeBook ecb = new ElectronicCodeBook(this);
                        ecb.encrypt(inputStream, outputStream, Tools.stringToBinaryByteArray(enc.GetString(this.inputKey)));
                    }
                    else if (this.settings.Mode == 1)
                    {
                        GuiLogMessage("Starting encryption with cbc", NotificationLevel.Info);
                        CipherBlockChaining cbc = new CipherBlockChaining(this);
                        cbc.encrypt(inputStream, outputStream, Tools.stringToBinaryByteArray(enc.GetString(this.inputKey)), Tools.stringToBinaryByteArray(enc.GetString(this.inputIV)));
                    }
                }
                //Decrypt
                else if (action == 1)
                {
                                       
                    if (this.settings.Mode == 0)
                    {
                        GuiLogMessage("Starting decryption with ecb", NotificationLevel.Info);
                        ElectronicCodeBook ecb = new ElectronicCodeBook(this);
                        ecb.decrypt(inputStream, outputStream, Tools.stringToBinaryByteArray(enc.GetString(this.inputKey)));
                    }
                    if (this.settings.Mode == 1)
                    {
                        GuiLogMessage("Starting decryption with cbc", NotificationLevel.Info);
                        CipherBlockChaining cbc = new CipherBlockChaining(this);
                        cbc.decrypt(inputStream, outputStream, Tools.stringToBinaryByteArray(enc.GetString(this.inputKey)), Tools.stringToBinaryByteArray(enc.GetString(this.inputIV)));
                    }
                }

                DateTime stopTime = DateTime.Now;
                TimeSpan duration = stopTime - startTime;
                if (!stop)
                {
                    GuiLogMessage("En-/Decryption complete! ", NotificationLevel.Info);
                    GuiLogMessage("Wrote data to file: " + outputStream.FileName, NotificationLevel.Info);
                    GuiLogMessage("Time used: " + duration.ToString(), NotificationLevel.Debug);
                    OnPropertyChanged("OutputStream");
               
                }else{                    
                    GuiLogMessage("Aborted!", NotificationLevel.Info);
                }

                //avoid unnecessary error messages because of wrong input/output streams:
                outputStream.Flush();
                outputStream.Close();
                inputStream.Close();                
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
                GuiLogMessage(exception.StackTrace, NotificationLevel.Error);
            }
            finally
            {              
                ProgressChanged(1, 1);
            }
        }

        #endregion

    }//end SDES

    /// <summary>
    /// This Class is for controlling the SDES with a "brute forcer" like KeySearcher
    /// </summary>
    public class SDESControl : IControlEncryption
    {
        #region private
        private SDES plugin;
        private byte[] input;
        ElectronicCodeBook ecb;
        CipherBlockChaining cbc;
        #endregion

        #region events
        public event KeyPatternChanged keyPatternChanged; //not used, because we only have one key length
        public event IControlStatusChangedEventHandler OnStatusChanged;
        #endregion

        #region public

        /// <summary>
        /// Constructs a new SDESControl
        /// </summary>
        /// <param name="Plugin"></param>
        public SDESControl(SDES Plugin)
        {
            this.plugin = Plugin;
            this.ecb = new ElectronicCodeBook(plugin);
            this.cbc = new CipherBlockChaining(plugin);
        }

        /// <summary>
        /// Called by SDES if its status changes
        /// </summary>
        public void onStatusChanged()
        {
            if(OnStatusChanged != null)
                OnStatusChanged(this, true);
        }
      
        /// <summary>
        /// Called by a Master to start encryption
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="bytesToUse">bytesToUse</param>
        /// <returns>encrypted text</returns>
        public byte[] Encrypt(byte[] key, int bytesToUse)
        {
            return execute(key, bytesToUse, 0);
        }

        /// <summary>
        /// Called by a Master to start decryption with ciphertext
        /// </summary>
        /// <param name="ciphertext">encrypted text</param>
        /// <param name="key">key</param>
        /// <param name="bytesToUse">bytesToUse</param>
        /// <returns>decrypted text</returns>
        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV, int bytesToUse)
        {
            return execute(ciphertext, key, bytesToUse, 1);
        }

        /// <summary>
        /// Called by a Master to start decryption with ciphertext
        /// </summary>
        /// <param name="ciphertext">encrypted text</param>
        /// <param name="key">key</param>
        /// <returns>decrypted text</returns>
        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV)
        {
            return execute(ciphertext, key, ciphertext.Length, 1);
        }

        /// <summary>
        /// Called by a Master to start decryption
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="bytesToUse">bytesToUse</param>
        /// <returns>decrypted text</returns>
        public byte[] Decrypt(byte[] key, byte[] IV, int bytesToUse)
        {
            return execute(key, bytesToUse, 1);
        }

        /// <summary>
        /// Get the key pattern of the SDES algorithm
        /// </summary>
        /// <returns>[01][01][01][01][01][01][01][01][01][01]</returns>
        public string getKeyPattern()
        {
            return "[01][01][01][01][01][01][01][01][01][01]";
        }

        /// <summary>
        /// Makes a byte Array out of a String
        /// example
        /// "10101" -> 1,0,1,0,1
        /// 
        /// A 0 is interpreted as 0
        /// any other character as 1
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte[] getKeyFromString(string key, ref int[] arrayPointers, ref int[] arraySuccessors, ref int[] arrayUppers)
        {
            byte[] bkey = new byte[10];
            int count = 0;
            foreach (char c in key)
                if (c == '*')
                    return null;    //blocks not supported yet
                else if (c == '0')
                    bkey[count++] = 0;
                else
                    bkey[count++] = 1;
            return bkey;
        }

        public IControlEncryption clone()
        {
            return new SDESControl(plugin);
        }

        public void Dispose()
        {
            //closeStreams();
        }

        #endregion

        #region private

        /// <summary>
        /// Called by itself to start encryption/decryption
        /// </summary>
        /// /// <param name="data">The data for encryption/decryption</param>
        /// <param name="key">key</param>
        /// <param name="bytesToUse">bytesToUse</param>
        /// <returns>encrypted/decrypted text</returns>
        private byte[] execute(byte[] data, byte[] key, int bytesToUse, int action)
        {
            byte[] output;
            if (bytesToUse > 0)
                output = new byte[bytesToUse];
            else
                output = new byte[data.Length];


            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            string IVString = "00000000";
            if (plugin.InputIV != null)
            {
                IVString = enc.GetString(plugin.InputIV);
            }
            
            if (((SDESSettings)plugin.Settings).Mode == 0 && action == 0)
            {
                output = ecb.encrypt(data, key, bytesToUse);
            }
            else if (((SDESSettings)plugin.Settings).Mode == 1 && action == 0)
            {
                output = cbc.encrypt(data, key, Tools.stringToBinaryByteArray(IVString), bytesToUse);
            }
            else if (((SDESSettings)plugin.Settings).Mode == 0 && action == 1)
            {
                output = ecb.decrypt(data, key, bytesToUse);
            }
            else if (((SDESSettings)plugin.Settings).Mode == 1 && action == 1)
            {
                output = cbc.decrypt(data, key, Tools.stringToBinaryByteArray(IVString), bytesToUse);
            }
            return output;

        }

        /// <summary>
        /// Called by itself to start encryption/decryption
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="bytesToUse">bytesToUse</param>
        /// <returns>encrypted/decrypted text</returns>
        private byte[] execute(byte[] key, int bytesToUse, int action)
        {
            
            if (input == null || plugin.InputChanged)
            {
                plugin.InputChanged = false;
                input = new byte[bytesToUse];

                byte[] buffer = new byte[1];
                
                int i = 0;
                CryptoolStream inputstream = plugin.InputStream;
                while ((inputstream.Read(buffer, 0, 1)) > 0 && i < bytesToUse)
                {
                    input[i] = buffer[0];
                    i++;
                }
            }

            return execute(input, key, bytesToUse, action);
        }

        #endregion

        #region IControlEncryption Member


        public void changeSettings(string setting, object value)
        {

        }
        #endregion
    }
       
    /// <summary>
    /// Encapsulates the SDES algorithm
    /// </summary>
    public class SDES_algorithm
    {
        private SDES mSdes;         //to call some methods on the plugin
        private int fkstep = 0;     //for presentation to check the number of fk we are in
        private int mode = 0;       //for presentation to check the mode we use (0 = en/1 = decrypt)

        public SDES_algorithm(SDES sdes)
        {
            this.mSdes = sdes;
        }

        /// <summary>
        /// Encrypt-function        
        /// Encrypts the input plaintext with the given key 
        /// </summary>
        /// <param name="plaintext">plaintext as byte array of size 8</param>
        /// <param name="key">key as byte array of size 10</param>
        /// <returns>ciphertext as byte array of size 8</returns>
        public byte[] encrypt(byte[] plaintext, byte[] key)
        {
            this.mode = 0; // to tell presentation what we are doing

            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt.Text =
                    Tools.byteArrayToStringWithSpaces(key);
                }
                , null);
            }
            //calculate sub key 1
            byte[] vp10 = p10(key);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_p10_input.Text =
                    Tools.byteArrayToStringWithSpaces(key);
                    ((SDESPresentation)mSdes.Presentation).key_txt_ls1_input_1.Text =
                    Tools.byteArrayToStringWithSpaces(vp10);
                }
                , null);
            }

            byte[] vls1 = ls_1(vp10);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_p8_1_input.Text =
                    Tools.byteArrayToStringWithSpaces(vls1);
                }
                , null);
            }

            byte[] key1 = p8(vls1);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_k1.Text =
                    Tools.byteArrayToStringWithSpaces(key1);
                }
                , null);
            }

            //calculate sub key 2
            vls1 = ls_1(vls1);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_p10_copy.Text =
                    Tools.byteArrayToStringWithSpaces(vp10);
                    ((SDESPresentation)mSdes.Presentation).key_txt_ls1_2.Text =
                    Tools.byteArrayToStringWithSpaces(vp10);
                    ((SDESPresentation)mSdes.Presentation).key_txt_ls1_3.Text =
                   Tools.byteArrayToStringWithSpaces(vls1);
                }
                , null);
            }

            vls1 = ls_1(vls1);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_p8_2_input.Text =
                    Tools.byteArrayToStringWithSpaces(vls1);
                }
               , null);
            }

            byte[] key2 = p8(vls1);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_k2.Text =
                    Tools.byteArrayToStringWithSpaces(key2);
                }
               , null);
            }

            // ip_inverse(fk_2(sw(fk_1(ip(plaintext))))) :

            byte[] ip = this.ip(plaintext);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).encrypt_txt_plaintext.Text =
                    Tools.byteArrayToStringWithSpaces(plaintext);
                    ((SDESPresentation)mSdes.Presentation).encrypt_txt_ip_input.Text =
                    Tools.byteArrayToStringWithSpaces(plaintext);
                    ((SDESPresentation)mSdes.Presentation).encrypt_txt_ip_output.Text =
                    Tools.byteArrayToStringWithSpaces(ip);
                }
               , null);
            }

            byte[] fk1 = fk(ip, key1);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).encrypt_txt_sw_input.Text =
                    Tools.byteArrayToStringWithSpaces(fk1);                    
                }
               , null);
            }

            byte[] swtch = sw(fk1);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).encrypt_txt_sw_output.Text =
                    Tools.byteArrayToStringWithSpaces(swtch);
                }
               , null);
            }

            byte[] fk2 = fk(swtch, key2);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).encrypt_txt_ip_invers_input.Text =
                    Tools.byteArrayToStringWithSpaces(fk2);
                }
               , null);
            }                   

            byte[] ciphertext = ip_inverse(fk2);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).encrypt_txt_ip_invers_output.Text =
                    Tools.byteArrayToStringWithSpaces(ciphertext);
                }
               , null);
            }    

            return ciphertext;

        }//end encrypt

        /// <summary>
        /// Decrypt-function
        /// Decrypts the input ciphertext with the given key
        /// </summary>
        /// <param name="ciphertext">ciphertext as byte array of size 8</param>
        /// <param name="key"> key as byte array of size 10</param>
        /// <returns>plaintext as byte array of size 8</returns>
        public byte[] decrypt(byte[] ciphertext, byte[] key)
        {
            this.mode = 1; // to tell presentation what we are doing

            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt.Text =
                    Tools.byteArrayToStringWithSpaces(key);
                }
                , null);
            }
            //calculate sub key 1
            byte[] vp10 = p10(key);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_p10_input.Text =
                    Tools.byteArrayToStringWithSpaces(key);
                    ((SDESPresentation)mSdes.Presentation).key_txt_ls1_input_1.Text =
                    Tools.byteArrayToStringWithSpaces(vp10);
                }
                , null);
            }

            byte[] vls1 = ls_1(vp10);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_p8_1_input.Text =
                    Tools.byteArrayToStringWithSpaces(vls1);
                }
                , null);
            }

            byte[] key1 = p8(vls1);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_k1.Text =
                    Tools.byteArrayToStringWithSpaces(key1);
                }
                , null);
            }

            //calculate sub key 2
            vls1 = ls_1(vls1);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_p10_copy.Text =
                    Tools.byteArrayToStringWithSpaces(vp10);
                    ((SDESPresentation)mSdes.Presentation).key_txt_ls1_2.Text =
                    Tools.byteArrayToStringWithSpaces(vp10);
                    ((SDESPresentation)mSdes.Presentation).key_txt_ls1_3.Text =
                   Tools.byteArrayToStringWithSpaces(vls1);
                }
                , null);
            }

            vls1 = ls_1(vls1);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_p8_2_input.Text =
                    Tools.byteArrayToStringWithSpaces(vls1);
                }
               , null);
            }

            byte[] key2 = p8(vls1);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).key_txt_k2.Text =
                    Tools.byteArrayToStringWithSpaces(key2);
                }
               , null);
            }

            // ip_inverse(fk_1(sw(fk_2(ip(ciphertext))))) :

            byte[] ip = this.ip(ciphertext);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).decrypt_txt_plaintext.Text =
                    Tools.byteArrayToStringWithSpaces(ciphertext);
                    ((SDESPresentation)mSdes.Presentation).decrypt_txt_ip_input.Text =
                    Tools.byteArrayToStringWithSpaces(ciphertext);
                    ((SDESPresentation)mSdes.Presentation).decrypt_txt_ip_output.Text =
                    Tools.byteArrayToStringWithSpaces(ip);
                }
               , null);
            }

            byte[] fk2 = fk(ip, key2);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).decrypt_txt_sw_input.Text =
                    Tools.byteArrayToStringWithSpaces(fk2);                  
                }
               , null);
            }

            byte[] swtch = sw(fk2); 
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).decrypt_txt_sw_output.Text =
                    Tools.byteArrayToStringWithSpaces(swtch);
                }
               , null);
            }

            byte[] fk1 = fk(swtch, key1);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).decrypt_txt_ip_invers_input.Text =
                    Tools.byteArrayToStringWithSpaces(fk1);
                }
               , null);
            }

            byte[] plaintext = ip_inverse(fk1);
            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((SDESPresentation)mSdes.Presentation).decrypt_txt_ip_invers_output.Text =
                    Tools.byteArrayToStringWithSpaces(plaintext);
                }
               , null);
            }            

            return plaintext;

        }//end decrypt

        ///<summary>
        ///p10-function
        ///Permutates the input bytes array of "10 bits" to another by 
        ///the following rule:
        ///
        ///src    dest
        ///1   -> 3
        ///2   -> 5
        ///3   -> 2
        ///4   -> 7
        ///5   -> 4
        ///6   -> 10
        ///7   -> 1
        ///8   -> 9
        ///9   -> 8
        ///10  -> 6
        ///</summary>
        ///<param name="bits">byte array of size 10</param>
        ///<returns>byte array of size 10</returns>
        ///
        private byte[] p10(byte[] bits)
        {

            byte[] p10 = new byte[10];

            p10[1 - 1] = bits[3 - 1];
            p10[2 - 1] = bits[5 - 1];
            p10[3 - 1] = bits[2 - 1];
            p10[4 - 1] = bits[7 - 1];
            p10[5 - 1] = bits[4 - 1];
            p10[6 - 1] = bits[10 - 1];
            p10[7 - 1] = bits[1 - 1];
            p10[8 - 1] = bits[9 - 1];
            p10[9 - 1] = bits[8 - 1];
            p10[10 - 1] = bits[6 - 1];

            //mSdes.GuiLogMessage("P10 with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(p10), NotificationLevel.Debug);
            return p10;

        }//end p10

        ///<summary>
        ///p8-function
        ///Permutates the input bytes array of "8 bits" to another by 
        ///the following rule:
        ///
        ///src    dest
        ///1   -> 6
        ///2   -> 3
        ///3   -> 7
        ///4   -> 4
        ///5   -> 8
        ///6   -> 5
        ///7   -> 10
        ///8   -> 9
        ///</summary>
        ///<param name="bits">byte array of size 10</param>
        ///<returns>byte array of size 8</returns>
        private byte[] p8(byte[] bits)
        {

            byte[] p8 = new byte[8];

            p8[1 - 1] = bits[6 - 1];
            p8[2 - 1] = bits[3 - 1];
            p8[3 - 1] = bits[7 - 1];
            p8[4 - 1] = bits[4 - 1];
            p8[5 - 1] = bits[8 - 1];
            p8[6 - 1] = bits[5 - 1];
            p8[7 - 1] = bits[10 - 1];
            p8[8 - 1] = bits[9 - 1];

            //mSdes.GuiLogMessage("P8 with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(p8), NotificationLevel.Debug);
            return p8;

        }//end p8

        ///<summary>
        ///ip-function (initial permutation)
        ///Permutates the input array of "8 bits" to another by 
        ///the following rule:
        ///
        ///src    dest
        ///1   -> 2
        ///2   -> 6
        ///3   -> 3
        ///4   -> 1
        ///5   -> 4
        ///6   -> 8
        ///7   -> 5
        ///8   -> 7
        ///</summary>
        ///<param name="bits">byte array of size 8</param>
        ///<returns>byte array of size 8</returns>
        private byte[] ip(byte[] bits)
        {

            byte[] ip = new byte[8];

            ip[1 - 1] = bits[2 - 1];
            ip[2 - 1] = bits[6 - 1];
            ip[3 - 1] = bits[3 - 1];
            ip[4 - 1] = bits[1 - 1];
            ip[5 - 1] = bits[4 - 1];
            ip[6 - 1] = bits[8 - 1];
            ip[7 - 1] = bits[5 - 1];
            ip[8 - 1] = bits[7 - 1];

            //mSdes.GuiLogMessage("ip with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ip), NotificationLevel.Debug);
            return ip;

        }//end ip

        ///<summary>
        ///ip^-1-function (initial permutation inverse)
        ///Permutates the input array of "8 bits" to another by 
        ///the following rule:
        ///
        ///src    dest
        ///1   -> 4
        ///2   -> 1
        ///3   -> 3
        ///4   -> 5
        ///5   -> 7
        ///6   -> 2
        ///7   -> 8
        ///8   -> 6
        ///</summary>
        ///<param name="bits">byte array of size 8</param>
        ///<returns>byte array of size 8</returns>
        private byte[] ip_inverse(byte[] bits)
        {

            byte[] ip_inverse = new byte[8];

            ip_inverse[1 - 1] = bits[4 - 1];
            ip_inverse[2 - 1] = bits[1 - 1];
            ip_inverse[3 - 1] = bits[3 - 1];
            ip_inverse[4 - 1] = bits[5 - 1];
            ip_inverse[5 - 1] = bits[7 - 1];
            ip_inverse[6 - 1] = bits[2 - 1];
            ip_inverse[7 - 1] = bits[8 - 1];
            ip_inverse[8 - 1] = bits[6 - 1];

            //mSdes.GuiLogMessage("ip_inverse with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ip_inverse), NotificationLevel.Debug);		
            return ip_inverse;

        }//end ip_inverse

        ///<summary>
        ///fk-function
        ///
        ///combines the following functions:
        ///
        ///right is the right part of the input array
        ///left is the left part of the input array
        ///
        ///(right | left) := (inputarray))
        ///ret := exclusive_or(left,F(right,key)) + right)
        ///</summary>
        ///<param name="bits">byte array of size 8</param>
        ///<param name="key">byte array of size 8</param>
        ///<returns>byte array of size 8</returns>
        private byte[] fk(byte[] bits, byte[] key)
        {
            byte[] left = { bits[1 - 1], bits[2 - 1], bits[3 - 1], bits[4 - 1] };
            byte[] right = { bits[5 - 1], bits[6 - 1], bits[7 - 1], bits[8 - 1] };

            byte[] exclusive_oder = Tools.exclusive_or(left, F(right, key));

            byte[] ret = {exclusive_oder[1-1],exclusive_oder[2-1],exclusive_oder[3-1],exclusive_oder[4-1],
				     right[1-1],right[2-1],right[3-1],right[4-1]};

            fkstep++;
            if (fkstep == 2)
                fkstep = 0;

            //mSdes.GuiLogMessage("fk with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ret), NotificationLevel.Debug);
            return ret;

        }//end fk

        ///<summary>
        ///ls-1 function
        ///</summary>
        ///<param name="bits">byte array of size 10</param>
        ///<returns>byte array of size 10</returns>
        private byte[] ls_1(byte[] bits)
        {

            byte[] ls_1 = new byte[10];

            ls_1[1 - 1] = bits[2 - 1];
            ls_1[2 - 1] = bits[3 - 1];
            ls_1[3 - 1] = bits[4 - 1];
            ls_1[4 - 1] = bits[5 - 1];
            ls_1[5 - 1] = bits[1 - 1];
            ls_1[6 - 1] = bits[7 - 1];
            ls_1[7 - 1] = bits[8 - 1];
            ls_1[8 - 1] = bits[9 - 1];
            ls_1[9 - 1] = bits[10 - 1];
            ls_1[10 - 1] = bits[6 - 1];

            //mSdes.GuiLogMessage("ls-1 with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ls_1), NotificationLevel.Debug);
            return ls_1;

        }//end ls_1

        ///<summary>
        ///switch-function
        ///
        ///switches the left side and the right side of the 8 bit array
        ///(left|right) -> (right|left)
        ///</summary>
        ///<param name="bits">byte array of size 8</param>
        ///<returns>byte array of size 8</returns>
        private byte[] sw(byte[] bits)
        {

            byte[] ret = {bits[5-1],bits[6-1],bits[7-1],bits[8-1],
				         bits[1-1],bits[2-1],bits[3-1],bits[4-1]};

            //mSdes.GuiLogMessage("sw with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ret), NotificationLevel.Debug);
            return ret;

        }//end sw

        ///<summary>
        ///F-function
        ///
        ///combines both s-boxes and permutates the return value with p4
        ///p4( s0(exclusive_or(ep(number),key) | s1(exclusive_or(ep(number),key) )
        ///</summary>
        ///<param name="bits">byte array of size 8</param>
        ///<param name="bits">key of size 8</param>
        ///<returns>byte array of size 8</returns>
        private byte[] F(byte[] bits, byte[] key)
        {

            byte[] ep = this.ep(bits);

            byte[] exclusive = Tools.exclusive_or(ep, key);

            byte[] s0_input = { exclusive[1 - 1], exclusive[2 - 1], exclusive[3 - 1], exclusive[4 - 1] };
            byte[] s0 = sbox_0(s0_input);

            byte[] s1_input = { exclusive[5 - 1], exclusive[6 - 1], exclusive[7 - 1], exclusive[8 - 1] };
            byte[] s1 = sbox_1(s1_input);

            byte[] s0_s1 = { s0[1 - 1], s0[2 - 1], s1[1 - 1], s1[2 - 1] };
            byte[] ret = p4(s0_s1);

            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (mode == 0 && fkstep == 0)
                    {
                        ((SDESPresentation)mSdes.Presentation).encrypt_txt_sbox1_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 0 && fkstep == 1)
                    {
                        ((SDESPresentation)mSdes.Presentation).encrypt_txt_sbox2_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 1 && fkstep == 0)
                    {
                        ((SDESPresentation)mSdes.Presentation).decrypt_txt_sbox1_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 1 && fkstep == 1)
                    {
                        ((SDESPresentation)mSdes.Presentation).decrypt_txt_sbox2_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                }
               , null);
            }

            //mSdes.GuiLogMessage("F with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(key) + " ist " + Tools.intArray2String(ret), NotificationLevel.Debug);
            return ret;

        }//end F

        ///<summary>
        ///p4-function
        ///Permutates the input array of "4 bits" to another by 
        ///the following rule:
        ///
        ///src    dest
        ///1   -> 2
        ///2   -> 4
        ///3   -> 3
        ///4   -> 1
        ///</summary>
        ///<param name="bits">byte array of size 4</param>
        ///<returns>byte array of size 4</returns>
        private byte[] p4(byte[] bits)
        {

            byte[] ret = new byte[4];
            ret[1 - 1] = bits[2 - 1];
            ret[2 - 1] = bits[4 - 1];
            ret[3 - 1] = bits[3 - 1];
            ret[4 - 1] = bits[1 - 1];

            return ret;

        }//end p4

        ///<summary>
        ///ep-function
        ///Permutates the input array of "4 bits" to another array of "8 bits" by 
        ///the following rule:
        ///
        ///src    dest
        ///1   -> 4
        ///2   -> 1
        ///3   -> 2
        ///4   -> 3
        ///5   -> 2
        ///6   -> 3
        ///7   -> 4
        ///8   -> 1
        ///</summary>
         ///<param name="bits">byte array of size 4</param>
        ///<returns>byte array of size 8</returns>
        private byte[] ep(byte[] bits)
        {

            byte[] ep = new byte[8];
            ep[1 - 1] = bits[4 - 1];
            ep[2 - 1] = bits[1 - 1];
            ep[3 - 1] = bits[2 - 1];
            ep[4 - 1] = bits[3 - 1];
            ep[5 - 1] = bits[2 - 1];
            ep[6 - 1] = bits[3 - 1];
            ep[7 - 1] = bits[4 - 1];
            ep[8 - 1] = bits[1 - 1];

            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (mode == 0 && fkstep == 0)
                    {
                        ((SDESPresentation)mSdes.Presentation).encrypt_txt_ep_output.Text =
                        Tools.byteArrayToStringWithSpaces(ep);
                    }
                    if (mode == 0 && fkstep == 1)
                    {
                        ((SDESPresentation)mSdes.Presentation).encrypt_txt_ep_output1.Text =
                        Tools.byteArrayToStringWithSpaces(ep);
                    }
                    if (mode == 1 && fkstep == 0)
                    {
                        ((SDESPresentation)mSdes.Presentation).decrypt_txt_ep_output.Text =
                        Tools.byteArrayToStringWithSpaces(ep);
                    }
                    if (mode == 1 && fkstep == 1)
                    {
                        ((SDESPresentation)mSdes.Presentation).decrypt_txt_ep_output1.Text =
                        Tools.byteArrayToStringWithSpaces(ep);
                    }
                }
               , null);
            }

            return ep;
        }

        ///<summary>
        ///SBox-0
        ///
        ///S0 =  1 0 3 2
        ///      3 2 1 0
        ///      0 2 1 3    
        ///      3 1 3 2           
        ///</summary>
        ///<param name="bits">byte array of size 4</param>
        ///<returns>byte array of size 2</returns>
        private byte[] sbox_0(byte[] bits)
        {

            int row = 2 * bits[1 - 1] + 1 * bits[4 - 1];
            int column = 2 * bits[2 - 1] + 1 * bits[3 - 1];

            byte[,][] sbox_0 = new byte[4, 4][]
                            {
                            {new byte[] {0,1}, new byte[] {0,0}, new byte[] {1,1}, new byte[] {1,0}},
	     				 	{new byte[] {1,1}, new byte[] {1,0}, new byte[] {0,1}, new byte[] {0,0}},
	     				 	{new byte[] {0,0}, new byte[] {1,0}, new byte[] {0,1}, new byte[] {1,1}},
	     				 	{new byte[] {1,1}, new byte[] {0,1}, new byte[] {1,1}, new byte[] {1,0}}
                            };

            byte[] ret = sbox_0[row, column];

            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (mode == 0 && fkstep == 0)
                    {
                        ((SDESPresentation)mSdes.Presentation).encrypt_txt_s0_1_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 0 && fkstep == 1)
                    {
                        ((SDESPresentation)mSdes.Presentation).encrypt_txt_s0_2_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 1 && fkstep == 0)
                    {
                        ((SDESPresentation)mSdes.Presentation).decrypt_txt_s0_1_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 1 && fkstep == 1)
                    {
                        ((SDESPresentation)mSdes.Presentation).decrypt_txt_s0_2_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                }
               , null);
            }

            //mSdes.GuiLogMessage("S0 with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ret), NotificationLevel.Debug);
            return ret;

        }//end sbox-0


        ///<summary>
        ///SBox-1
        ///
        ///S1 =  0 1 2 3
        ///      2 0 1 3
        ///      3 0 1 0
        ///      2 1 0 3
        ///</summary>
        ///<param name="bits">byte array of size 4</param>
        ///<returns>byte array of size 2</returns>
        private byte[] sbox_1(byte[] bits)
        {

            int row = 2 * bits[1 - 1] + 1 * bits[4 - 1];
            int column = 2 * bits[2 - 1] + 1 * bits[3 - 1];

            byte[,][] sbox_1 = new byte[4, 4][]
                            {
                            {new byte[] {0,0}, new byte[] {0,1}, new byte[] {1,0}, new byte[] {1,1}},
				 			{new byte[] {1,0}, new byte[] {0,0}, new byte[] {0,1}, new byte[] {1,1}},
				 			{new byte[] {1,1}, new byte[] {0,0}, new byte[] {0,1}, new byte[] {0,0}},
				 			{new byte[] {1,0}, new byte[] {0,1}, new byte[] {0,0}, new byte[] {1,1}}
                            };

            byte[] ret = sbox_1[row, column];

            if (this.mSdes.Presentation.IsVisible)
            {
                ((SDESPresentation)mSdes.Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (mode == 0 && fkstep == 0)
                    {
                        ((SDESPresentation)mSdes.Presentation).encrypt_txt_s1_1_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 0 && fkstep == 1)
                    {
                        ((SDESPresentation)mSdes.Presentation).encrypt_txt_s1_2_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 1 && fkstep == 0)
                    {
                        ((SDESPresentation)mSdes.Presentation).decrypt_txt_s1_1_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                    if (mode == 1 && fkstep == 1)
                    {
                        ((SDESPresentation)mSdes.Presentation).decrypt_txt_s1_2_output.Text =
                        Tools.byteArrayToStringWithSpaces(ret);
                    }
                }
               , null);
            }

            //mSdes.GuiLogMessage("S1 with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ret), NotificationLevel.Debug);		
            return ret;

        }//end sbox-1

    }

    ///<summary>
    ///Encapsulates some necessary functions
    ///</summary>
    public class Tools
    {

        /// <summary>
        /// transforms a byte array into a String with spaces after each byte
        /// example:
        ///     1,0 => "1 0"
        /// </summary>
        /// <param name="byt">byt</param>
        /// <returns>s</returns>
        public static String byteArrayToStringWithSpaces(byte[] byt)
        {
            String s = "";

            foreach (byte b in byt)
            {
                s = s + b + " ";
            }
            return s;
        }
        ///<summary>
        ///Converts an byte array to a String
        ///</summary>
        ///<param name="bits">byte array of size n</param>
        ///<returns>String</returns>
        public static String byteArray2String(byte[] bits)
        {

            String ret = "";
            for (int i = 0; i < bits.Length; i++)
            {
                ret += ("" + bits[i]);
            }
            return ret;

        }//end byteArray2String

        ///<summary>
        ///Converts the given byte array to a printable String
        ///
        ///example {72, 101, 108, 108, 111} -> "Hello"
        ///</summary>
        ///<param name="bits">byte array of size n</param>
        ///<returns>String</returns>
        public static String byteArray2PrintableString(byte[] bits)
        {

            String ret = "";
            for (int i = 0; i < bits.Length; i++)
            {
                ret += ("" + (char)bits[i]);
            }
            return ret;

        }// byteArray2PrintableString

        ///<summary>
        ///equals-function
        ///
        ///returns true if both integer arrays are equal
        ///</summary>
        ///<param name="a">byte array of size n</param>
        ///<param name="b">byte array of size n</param>
        ///<returns>bool</returns>
        public static bool byteArrays_Equals(byte[] a, byte[] b)
        {

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;

        }//end byteArrays_Equals	

        ///<summary>
        ///converts an Byte to an byte array of (0,1)
        ///
        ///100 -> {1,1,0,0,1,0,0}
        ///</summary>
        ///<param name="byt">byte array of size n</param>
        ///<returns>byte array</returns>
        public static byte[] byteToByteArray(byte byt)
        {

            byte[] bytearray = new byte[8];

            for (int i = 7; i >= 0; i--)
            {

                bytearray[i] = (byte)(byt % 2);
                byt = (byte)Math.Floor((double)(byt / 2));

            }

            return bytearray;

        }//end byteTointArray

        ///<summary>
        ///converts an byte array of (0,1) to an byte
        ///
        ///{1,1,0,0,1,0,0} -> 100 
        ///</summary>
        ///<param name="bytearray">byte array of size n</param>
        ///<returns>byte</returns>
        public static byte byteArrayToByte(byte[] bytearray)
        {

            int byt = 0;

            byt = (bytearray[0] * 128)
                        + (bytearray[1] * 64)
                        + (bytearray[2] * 32)
                        + (bytearray[3] * 16)
                        + (bytearray[4] * 8)
                        + (bytearray[5] * 4)
                        + (bytearray[6] * 2)
                        + (bytearray[7] * 1);

            return (byte)byt;

        }//end byteArrayToInteger

        ///<summary>
        ///Exclusiv-OR function
        ///
        ///Does a exlusiv-or on two byte arrays 
        ///
        ///example {1,0,1} XOR {1,0,0} -> {0,0,1}
        ///</summary>
        ///<param name="bitsA">byte array of size n</param>
        ///<param name="bitsB">byte array of size n</param>
        ///<returns>byte array of size n</returns>
        public static byte[] exclusive_or(byte[] bitsA, byte[] bitsB)
        {

            byte[] exclusive_or_AB = new byte[bitsA.Length];

            for (int i = 0; i < bitsA.Length; i++)
            {

                if ((bitsA[i] == 0 && bitsB[i] == 1) ||
                   (bitsA[i] == 1 && bitsB[i] == 0)
                )
                {
                    exclusive_or_AB[i] = 1;
                }
                else
                {
                    exclusive_or_AB[i] = 0;
                }//end if

            }//end for

            return exclusive_or_AB;

        }//end exclusive_or

        ///<summary>
        ///converts string to an byte array
        ///
        ///example "Hello" -> {72, 101, 108, 108, 111}
        ///</summary>
        ///<param name="s">String</param>
        ///<returns>byte array</returns>
        public static byte[] stringToByteArray(String s)
        {
            byte[] bytearray = new byte[s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                bytearray[i] = (byte)s[i];
            }

            return bytearray;

        }// end stringToByteArray

        ///<summary>
        ///converts a binary string to an byte array
        ///
        ///example "10010" -> {1, 0, 0, 1, 0}
        ///</summary>
        ///<param name="s">String</param>
        ///<returns>byte array</returns>
        public static byte[] stringToBinaryByteArray(String s)
        {
            byte[] bytearray = new byte[s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '1')
                {
                    bytearray[i] = 1;
                }
                else if (s[i] == '0')
                {
                    bytearray[i] = 0;
                }
                else
                {
                    throw new Exception("Invalid Character '" + s[i] + "' at position " + i + " in String which represents binary values: " + s);
                }
            }

            return bytearray;

        }// end stringToByteArray
    }

    ///<summary>
    ///Encapsulates the CipherBlockChaining algorithm
    ///</summary>
    public class CipherBlockChaining
    {

        private SDES mSdes;
        private SDES_algorithm mAlgorithm;

        /// <summary>
        /// Constructs a CipherBlockChaining for SDES
        /// </summary>
        /// <param name="sdes">plugin</param>
        public CipherBlockChaining(SDES sdes)
        {
            this.mSdes = sdes;
            this.mAlgorithm = new SDES_algorithm(sdes);
        }

        ///<summary>
        ///Encrypts the given plaintext with the given key
        ///using CipherBlockChaining 
        ///</summary>
        public void encrypt(CryptoolStream inputstream, CryptoolStream outputstream, byte[] key, byte[] vector)
        {

            byte[] buffer = new byte[1];
            int position = 0;

            while (!this.mSdes.getStop() && (inputstream.Read(buffer, 0, 1)) > 0)
            {
                //Step 1 get plaintext symbol
                byte symbol = buffer[0];
                //Step 2 exclusiv OR with vector
                vector = Tools.exclusive_or(vector, Tools.byteToByteArray(symbol));
                //Step 3 decrypt vector with key
                vector = mAlgorithm.encrypt(vector, key);
                //Step 4 store symbol in ciphertext
                outputstream.Write(Tools.byteArrayToByte(vector));

                if ((int)(inputstream.Position * 100 / inputstream.Length) > position)
                {
                    position = (int)(inputstream.Position * 100 / inputstream.Length);
                    mSdes.ProgressChanged(inputstream.Position, inputstream.Length);
                }
                outputstream.Flush();
            }

        }//end encrypt

        ///
        ///Encrypts the given plaintext with the given key
        ///using CipherBlockChaining 
        ///
        /// bytesToUse tells the algorithm how many bytes it has to encrypt
        /// bytesToUse = 0 => encrypt all
        public byte[] encrypt(byte[] input, byte[] key, byte[] vector, [Optional, DefaultParameterValue(0)] int bytesToUse)
        {
            int until = input.Length;            

            if (bytesToUse < until && bytesToUse > 0)
                until = bytesToUse;

            byte[] output = new byte[until];

            for (int i = 0; i < until; i++)
            {
                vector = Tools.exclusive_or(vector, Tools.byteToByteArray(input[i]));
                vector = mAlgorithm.encrypt(vector, key);
                output[i] = Tools.byteArrayToByte(vector);

            }//end while
            
            return output;

        }//end encrypt

        ///<summary>
        ///Decrypts the given plaintext with the given Key
        ///using CipherBlockChaining 
        ///</summary>
        public void decrypt(CryptoolStream inputstream, CryptoolStream outputstream, byte[] key, byte[] vector)
        {

            byte[] buffer = new byte[1];
            int position = 0;

            while (!this.mSdes.getStop() && (inputstream.Read(buffer, 0, 1)) > 0)
            {
                //Step 1 get Symbol of Ciphertext
                byte symbol = buffer[0];
                //Step 2 Decrypt symbol with key and exclusiv-or with vector
                outputstream.Write((Tools.byteArrayToByte(Tools.exclusive_or(mAlgorithm.decrypt(Tools.byteToByteArray(symbol), key), vector))));
                //Step 3 let vector be the decrypted Symbol
                vector = Tools.byteToByteArray(buffer[0]);

                if ((int)(inputstream.Position * 100 / inputstream.Length) > position)
                {
                    position = (int)(inputstream.Position * 100 / inputstream.Length);
                    mSdes.ProgressChanged(inputstream.Position, inputstream.Length);
                }
                outputstream.Flush();
            }

        }//end decrypt

        ///
        ///Decrypt the given plaintext with the given key
        ///using CipherBlockChaining 
        ///
        /// bytesToUse tells the algorithm how many bytes it has to encrypt
        /// bytesToUse = 0 => encrypt all
        public byte[] decrypt(byte[] input, byte[] key, byte[] vector, [Optional, DefaultParameterValue(0)] int bytesToUse)
        {

            int until = input.Length;
           
            if (bytesToUse < until && bytesToUse > 0)
                until = bytesToUse;

            byte[] output = new byte[until];

            for (int i = 0; i < until; i++)
            {
                output[i] = (Tools.byteArrayToByte(Tools.exclusive_or(mAlgorithm.decrypt(Tools.byteToByteArray(input[i]), key), vector)));
                vector = Tools.byteToByteArray(input[i]);
                           
            }//end while

            return output;

        }//end encrypt

    }//end class CipherBlockChaining

    ///<summary>
    ///Encapsulates the ElectronicCodeBook algorithm
    ///</summary>
    public class ElectronicCodeBook
    {

        private SDES mSdes;
        private SDES_algorithm mAlgorithm;

        /// <summary>
        /// Constructs a ElectronicCodeBook for SDES
        /// </summary>
        /// <param name="sdes">plugin</param>
        public ElectronicCodeBook(SDES sdes)
        {
            this.mSdes = sdes;
            this.mAlgorithm = new SDES_algorithm(sdes);
        }

        ///
        ///Encrypts the given plaintext with the given key
        ///using ElectronicCodeBookMode 
        ///
        public void encrypt(CryptoolStream inputstream, CryptoolStream outputstream, byte[] key)
        {

            byte[] buffer = new byte[1];
            int position = 0;

            while (!this.mSdes.getStop() && (inputstream.Read(buffer, 0, 1)) > 0)
            {
                //Step 1 get plaintext symbol
                byte symbol = buffer[0]; ;
                //Step 2 encrypt symbol
                outputstream.Write(Tools.byteArrayToByte(this.mAlgorithm.encrypt(Tools.byteToByteArray(symbol), key)));

                if ((int)(inputstream.Position * 100 / inputstream.Length) > position)
                {
                    position = (int)(inputstream.Position * 100 / inputstream.Length);
                    mSdes.ProgressChanged(inputstream.Position, inputstream.Length);
                }
                outputstream.Flush();

            }//end while

        }//end encrypt

        ///
        ///Encrypts the given plaintext with the given key
        ///using ElectronicCodeBookMode 
        ///
        /// bytesToUse tells the algorithm how many bytes it has to encrypt
        /// bytesToUse = 0 => encrypt all
        public byte[] encrypt(byte[] input, byte[] key, [Optional, DefaultParameterValue(0)] int bytesToUse)
        {

            int until = input.Length;

            if(bytesToUse < until && bytesToUse > 0)
                until = bytesToUse;

            byte[] output = new byte[until];

            for(int i=0;i<until;i++)
            {                
                //Step 2 encrypt symbol
                output[i] = Tools.byteArrayToByte(this.mAlgorithm.encrypt(Tools.byteToByteArray(input[i]), key));

            }//end while

            return output;

        }//end encrypt

        ///<summary>
        ///Decrypts the given plaintext with the given Key
        ///using ElectronicCodeBook mode 
        ///</summary>
        public void decrypt(CryptoolStream inputstream, CryptoolStream outputstream, byte[] key)
        {

            byte[] buffer = new byte[1];
            int position = 0;

            while (!this.mSdes.getStop() && (inputstream.Read(buffer, 0, 1)) > 0)
            {
                //Step 1 get plaintext symbol
                byte symbol = buffer[0];
                //Step 2 encrypt symbol
                outputstream.Write(Tools.byteArrayToByte(this.mAlgorithm.decrypt(Tools.byteToByteArray(symbol), key)));

                if ((int)(inputstream.Position * 100 / inputstream.Length) > position)
                {
                    position = (int)(inputstream.Position * 100 / inputstream.Length);
                    mSdes.ProgressChanged(inputstream.Position, inputstream.Length);
                }
                outputstream.Flush();

            }//end while

        }//end decrypt

        ///
        ///Decrypt the given plaintext with the given key
        ///using ElectronicCodeBookMode 
        ///
        /// bytesToUse tells the algorithm how many bytes it has to decrypt
        /// bytesToUse = 0 => encrypt all
        public byte[] decrypt(byte[] input, byte[] key, [Optional, DefaultParameterValue(0)] int bytesToUse)
        {
            int until = input.Length;

            if (bytesToUse < until && bytesToUse > 0)
                until = bytesToUse;
            
            byte[] output = new byte[until];

            for (int i = 0; i < until; i++)
            {
                //Step 2 encrypt symbol
                output[i] = Tools.byteArrayToByte(this.mAlgorithm.decrypt(Tools.byteToByteArray(input[i]), key));

            }//end while

            return output;

        }//end encrypt

    }//end class ElectronicCodeBook

}//end namespace Cryptool.Plugins.Cryptography.Encryption