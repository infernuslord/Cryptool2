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

namespace Cryptool.Plugins.Cryptography.Encryption
{
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
        #endregion

        public SDES()
        {
            this.settings = new SDESSettings();
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if(OnPluginStatusChanged != null)OnPluginStatusChanged(this, args);
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (SDESSettings)value; }
        }
    
        public bool getStop()
        {
            return false;
        }

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

        [PropertyInfo(Direction.InputData, "Key", "Must be 10 bytes (only 1 or 0 allowed).", null, true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public byte[] InputKey
        {
            get { return this.inputKey; }
            set
            {
                this.inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.InputData, "IV", "IV to be used in chaining modes, must be 10 bytes (only 1 or 0 allowed).", null, true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public byte[] InputIV
        {
            get { return this.inputIV; }
            set
            {
                this.inputIV = value;
                OnPropertyChanged("InputIV");
            }
        }
        
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

        /**
         * Checks if the Input key is not null and has length 10 and only contains 1s and 0s
         * and if Input IV is not null and has length 8 and only contains 1s and 0s
         * 
         * returns true if ok
         * 
         **/
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

        public void Execute()
        {
            process(settings.Action);
        }

        //Encrypt/Decrypt Stream
        private void process(int action)
        {
            
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
                outputStream.Close();
                outputStream = null;
                inputStream.Close();
                inputStream = null;
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

        private IControlEncryption controlSlave;
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
        public void ProgressChanged(double value, double max)
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

    public class SDESControl : IControlEncryption
    {
        private SDES plugin;

        public SDESControl(SDES Plugin)
        {
            this.plugin = Plugin;
        }

        #region IControlEncryption Members

        public byte[] Encrypt(byte[] key)
        {
            ((SDESSettings)plugin.Settings).Action = 0;
            return execute(key);
        }

        public byte[] Decrypt(byte[] key)
        {
            ((SDESSettings)plugin.Settings).Action = 1;
            return execute(key);
        }

        public string getKeyPattern()
        {
            return "[01][01][01][01][01][01][01][01][01][01]";
        }

        public byte[] getKeyFromString(string key)
        {
            byte[] bkey = new byte[10];
            int count = 0;
            foreach (char c in key)
                if (c == '0')
                    bkey[count++] = (byte)'0';
                else
                    bkey[count++] = (byte)'1';
            return bkey;
        }

        private byte[] execute(byte[] key)
        {
            plugin.InputKey = key;
            CryptoolStream output = new CryptoolStream();
            output.OpenWrite();

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            if (((SDESSettings)plugin.Settings).Mode == 0)
            {                
                ElectronicCodeBook ecb = new ElectronicCodeBook(plugin);
                ecb.decrypt(plugin.InputStream, output, key);
            }
            else
            {
                CipherBlockChaining cbc = new CipherBlockChaining(plugin);
                cbc.decrypt(plugin.InputStream, output, key, Tools.stringToBinaryByteArray(enc.GetString(plugin.InputIV)));
            }            

            byte[] byteValues = new byte[output.Length];
            int bytesRead;
            output.Seek(0, SeekOrigin.Begin);
            bytesRead = output.Read(byteValues, 0, byteValues.Length);
            plugin.Dispose();
            output.Close();
            return byteValues;
        }

        #endregion
    }

    /**
     * 
     * Encapsulates the SDES algorithm
     * 
     **/
    public class SDES_algorithm
    {
        private SDES mSdes;

        public SDES_algorithm(SDES sdes)
        {
            this.mSdes = sdes;
        }

        /**
         * Encrypt-function
         * 
         * Encrypts the input plaintext with the given key 
         * 
         * @param plaintext as byte array of size 8
         * @param key as byte array of size 10
         * @return ciphertext as byte array of size 8
         */
        public byte[] encrypt(byte[] plaintext, byte[] key)
        {

            //calculate sub key 1
            byte[] key1 = p8(ls_1(p10(key)));
            //calculate sub key 2
            byte[] key2 = p8(ls_1(ls_1(ls_1(p10(key)))));

            // ip_inverse(fk_2(sw(fk_1(ip(plaintext))))) :
            byte[] ip = this.ip(plaintext);
            byte[] fk1 = fk(ip, key1);
            byte[] fk2 = fk(sw(fk1), key2);
            byte[] ciphertext = ip_inverse(fk2);

            return ciphertext;

        }//end encrypt

        /**
         * Decrypt-function
         * 
         * Decrypts the input ciphertext with the given key
         * 
         * @param plaintext as byte array of size 8
         * @param key as byte array of size 10
         * @return plaintext as byte array of size 8
         */
        public byte[] decrypt(byte[] ciphertext, byte[] key)
        {

            //calculate sub key 1
            byte[] key1 = p8(ls_1(p10(key)));
            //calculate sub key 2
            byte[] key2 = p8(ls_1(ls_1(ls_1(p10(key)))));

            // ip_inverse(fk_1(sw(fk_2(ip(ciphertext))))) :
            byte[] ip = this.ip(ciphertext);
            byte[] fk2 = fk(ip, key2);
            byte[] fk1 = fk(sw(fk2), key1);
            byte[] plaintext = ip_inverse(fk1);

            return plaintext;

        }//end decrypt

        /**
         * p10-function
         * Permutates the input bytes array of "10 bits" to another by 
         * the following rule:
         * 
         * src    dest
         * 1   -> 3
         * 2   -> 5
         * 3   -> 2
         * 4   -> 7
         * 5   -> 4
         * 6   -> 10
         * 7   -> 1
         * 8   -> 9
         * 9   -> 8
         * 10  -> 6
         * 
         * @param byte array of size 10
         * @return byte array of size 10
         */
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

        /**
         * p8-function
         * Permutates the input bytes array of "8 bits" to another by 
         * the following rule:
         * 
         * src    dest
         * 1   -> 6
         * 2   -> 3
         * 3   -> 7
         * 4   -> 4
         * 5   -> 8
         * 6   -> 5
         * 7   -> 10
         * 8   -> 9
         * 
         * @param byte array of size 10
         * @return byte array of size 8
         */
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

        /**
         * ip-function (initial permutation)
         * Permutates the input array of "8 bits" to another by 
         * the following rule:
         * 
         * src    dest
         * 1   -> 2
         * 2   -> 6
         * 3   -> 3
         * 4   -> 1
         * 5   -> 4
         * 6   -> 8
         * 7   -> 5
         * 8   -> 7
         * 
         * @param byte array of size 8
         * @return byte array of size 8
         */
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

        /**
         * ip^-1-function (initial permutation inverse)
         * Permutates the input array of "8 bits" to another by 
         * the following rule:
         * 
         * src    dest
         * 1   -> 4
         * 2   -> 1
         * 3   -> 3
         * 4   -> 5
         * 5   -> 7
         * 6   -> 2
         * 7   -> 8
         * 8   -> 6
         * 
         * @param byte array of size 8
         * @return byte array of size 8
         */
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

        /**
         * fk-function
         * 
         * combines the following functions:
         * 
         * right is the right part of the input array
         * left is the left part of the input array
         * 
         * (right | left) := (inputarray))
         * ret := exclusive_or(left,F(right,key)) + right)
         * 
         * @param byte array of size 8
         * @return byte array of size 8
         */
        private byte[] fk(byte[] bits, byte[] key)
        {

            byte[] left = { bits[1 - 1], bits[2 - 1], bits[3 - 1], bits[4 - 1] };
            byte[] right = { bits[5 - 1], bits[6 - 1], bits[7 - 1], bits[8 - 1] };

            byte[] exclusive_oder = Tools.exclusive_or(left, F(right, key));

            byte[] ret = {exclusive_oder[1-1],exclusive_oder[2-1],exclusive_oder[3-1],exclusive_oder[4-1],
				     right[1-1],right[2-1],right[3-1],right[4-1]};

            //mSdes.GuiLogMessage("fk with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ret), NotificationLevel.Debug);
            return ret;

        }//end fk

        /**
         * ls-1 function
         * 
         * @param byte array of size 10
         * @return byte array of size 10
         */
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

        /**
         * switch-function
         * 
         * switches the left side and the right side of the 8 bit array
         * (left|right) -> (right|left)
         * 
         * @param byte array of size 8
         * @return byte array of size 8
         */
        private byte[] sw(byte[] bits)
        {

            byte[] ret = {bits[5-1],bits[6-1],bits[7-1],bits[8-1],
				         bits[1-1],bits[2-1],bits[3-1],bits[4-1]};

            //mSdes.GuiLogMessage("sw with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ret), NotificationLevel.Debug);
            return ret;

        }//end sw

        /**
         * F-function
         * 
         * combines both s-boxes and permutates the return value with p4
         * p4( s0(exclusive_or(ep(number),key) | s1(exclusive_or(ep(number),key) )
         * 
         * @param byte array of size 8
         * @param key of size 8
         * @return byte array of size 8
         */
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

            //mSdes.GuiLogMessage("F with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(key) + " ist " + Tools.intArray2String(ret), NotificationLevel.Debug);
            return ret;

        }//end F

        /**
         * p4-function
         * Permutates the input array of "4 bits" to another by 
         * the following rule:
         * 
         * src    dest
         * 1   -> 2
         * 2   -> 4
         * 3   -> 3
         * 4   -> 1
         *  
         * @param byte array of size 4
         * @return byte array of size 4
         */
        private byte[] p4(byte[] bits)
        {

            byte[] ret = new byte[4];
            ret[1 - 1] = bits[2 - 1];
            ret[2 - 1] = bits[4 - 1];
            ret[3 - 1] = bits[3 - 1];
            ret[4 - 1] = bits[1 - 1];

            return ret;

        }//end p4

        /**
         * ep-function
         * Permutates the input array of "4 bits" to another array of "8 bits" by 
         * the following rule:
         * 
         * src    dest
         * 1   -> 4
         * 2   -> 1
         * 3   -> 2
         * 4   -> 3
         * 5   -> 2
         * 6   -> 3
         * 7   -> 4
         * 8   -> 1
         * 
         * @param byte array of size 4
         * @return byte array of size 8
         */
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

            return ep;
        }

        /**
         * SBox-0
         * 
         * S0 =  1 0 3 2
         *       3 2 1 0
         *       0 2 1 3    
         *       3 1 3 2           
         *
         * @param byte array of size 4
         * @return byte array of size 2
         */
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

            //mSdes.GuiLogMessage("S0 with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ret), NotificationLevel.Debug);
            return ret;

        }//end sbox-0


        /**
         * SBox-1
         * 
         * S1 =  0 1 2 3
         *       2 0 1 3
         *       3 0 1 0
         *       2 1 0 3
         * 
         * @param byte array of size 4
         * @return byte array of size 8
         */
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

            //mSdes.GuiLogMessage("S1 with " + Tools.intArray2String(bits) + " is " + Tools.intArray2String(ret), NotificationLevel.Debug);		
            return ret;

        }//end sbox-1

    }

    /**
    * 
    * Encapsulates some necessary functions
    * 
    **/
    public class Tools
    {

        /**
         * Converts an byte array to a String
         * 
         * @param byte array of size n
         * @return String
         */
        public static String byteArray2String(byte[] bits)
        {

            String ret = "";
            for (int i = 0; i < bits.Length; i++)
            {
                ret += ("" + bits[i]);
            }
            return ret;

        }//end byteArray2String

        /**
         * Converts the given byte array to a printable String
         * 
         * example {72, 101, 108, 108, 111} -> "Hello"
         * 
         * @param bits
         * @return String
         */
        public static String byteArray2PrintableString(byte[] bits)
        {

            String ret = "";
            for (int i = 0; i < bits.Length; i++)
            {
                ret += ("" + (char)bits[i]);
            }
            return ret;

        }// byteArray2PrintableString

        /**
         * equals-function
         * 
         * returns true if both integer arrays are equal
         * 
         * @param byte array of size n
         * @param byte array of size n
         * @return boolean
         */
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

        /**
         * converts an Byte to an byte array of (0,1)
         * 
         * 100 -> {1,1,0,0,1,0,0}
         * 
         * @param b
         * @return
         */
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

        /**
         * converts an byte array of (0,1) to an byte
         * 
         * {1,1,0,0,1,0,0} -> 100 
         * @param intarray
         * @return
         */
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

        /**
         * Exclusiv-OR function
         * 
         * Does a exlusiv-or on two byte arrays 
         * 
         * example {1,0,1} XOR {1,0,0} -> {0,0,1}
         * 
         * @param byte array of size n
         * @param byte array of size n
         * @return byte array of size n
         */
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

        /**
         * converts string to an byte array
         * 
         * example "Hello" -> {72, 101, 108, 108, 111}
         * 
         * @param s
         * @return
         */
        public static byte[] stringToByteArray(String s)
        {
            byte[] bytearray = new byte[s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                bytearray[i] = (byte)s[i];
            }

            return bytearray;

        }// end stringToByteArray

        /**
         * converts a binary string to an byte array
         * 
         * example "10010" -> {1, 0, 0, 1, 0}
         * 
         * @param s
         * @return
         */
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

    /**
    * 
    * Encapsulates the CipherBlockChaining algorithm
    * 
    **/
    public class CipherBlockChaining
    {

        private SDES mSdes;
        private SDES_algorithm mAlgorithm;

        public CipherBlockChaining(SDES sdes)
        {
            this.mSdes = sdes;
            this.mAlgorithm = new SDES_algorithm(sdes);
        }

        /**
         * Encrypts the given plaintext with the given key
         * using CipherBlockChaining 
         */
        public void encrypt(CryptoolStream inputstream, CryptoolStream outputstream, byte[] key, byte[] vector)
        {

            byte[] buffer = new byte[1];
            int position = 0;

            while ((inputstream.Read(buffer, 0, 1)) > 0 && !this.mSdes.getStop())
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

        /**
         * Decrypts the given plaintext with the given Key
         * using CipherBlockChaining 
         */
        public void decrypt(CryptoolStream inputstream, CryptoolStream outputstream, byte[] key, byte[] vector)
        {

            byte[] buffer = new byte[1];
            int position = 0;

            while ((inputstream.Read(buffer, 0, 1)) > 0 && !this.mSdes.getStop())
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

    }

    /**
    * 
    * Encapsulates the ElectronicCodeBook algorithm
    * 
    **/
    public class ElectronicCodeBook
    {

        private SDES mSdes;
        private SDES_algorithm mAlgorithm;

        public ElectronicCodeBook(SDES sdes)
        {
            this.mSdes = sdes;
            this.mAlgorithm = new SDES_algorithm(sdes);
        }


        /**
         * Encrypts the given plaintext with the given key
         * using ElectronicCodeBookMode 
         */
        public void encrypt(CryptoolStream inputstream, CryptoolStream outputstream, byte[] key)
        {

            byte[] buffer = new byte[1];
            int position = 0;

            while ((inputstream.Read(buffer, 0, 1)) > 0 && !this.mSdes.getStop())
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

        /**
         * Decrypts the given plaintext with the given Key
         * using ElectronicCodeBook mode 
         */
        public void decrypt(CryptoolStream inputstream, CryptoolStream outputstream, byte[] key)
        {

            byte[] buffer = new byte[1];
            int position = 0;

            while ((inputstream.Read(buffer, 0, 1)) > 0 && !this.mSdes.getStop())
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

    }

}