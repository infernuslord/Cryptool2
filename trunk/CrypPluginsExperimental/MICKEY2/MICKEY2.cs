/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, 
   software distributed under the License is distributed on an 
   "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
   either express or implied. See the License for the specific 
   language governing permissions and limitations under the License.
*/
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.MICKEY2
{
    [Author("Robin Nelle", "rnelle@mail.uni-mannheim.de",
        "Uni Mannheim - Lehrstuhl Prof. Dr. Armknecht",
        "http://ls.wim.uni-mannheim.de/")]
    [PluginInfo("MICKEY2.Resources", "PluginCaption", 
        "PluginTooltip", "MICKEY2/userdoc.xml", 
        new[] { "MICKEY2/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class MICKEY2 : ICrypComponent
    {
        #region Private Variables
        private string inputString = "";
        private string outputString;
        private string inputKey;
        private string inputIV;
        private uint[] inputIVArray;
        private uint[] inputKeyArray;
        private uint[] keystream;
        private bool stop = false;
        public MICKEY2Settings settings = new MICKEY2Settings();
        private uint[] registerR = new uint[100];

        //the non-linear register
        private uint[] registerS = new uint[100];

        //feedback tap positions for R
        private int[] RTAPS = {0, 1, 3, 4, 5, 6, 9,12,13,16,19,20,21,
                              22,25,28,37,38,41,42,45,46,50,52,54,56,
                              58,60,61,63,64,65,66,67,71,72,79,80,81,
                              82,87,88,89,90,91,92,94,95,96,97};

        //sequences for clocking the register S
        private uint[] COMP0 ={0,0,0,0,1,1,0,0,0,1,0,1,1,1,1,0,1,0,0,1,
                               0,1,0,1,0,1,0,1,0,1,1,0,1,0,0,1,0,0,0,0,
                               0,0,0,1,0,1,0,1,0,1,0,0,0,0,1,0,1,0,0,1,
                               1,1,1,0,0,1,0,1,0,1,1,1,1,1,1,1,1,1,0,1,
                               0,1,1,1,1,1,1,0,1,0,1,0,0,0,0,0,0,1,1};

        private uint[] COMP1 ={0,1,0,1,1,0,0,1,0,1,1,1,1,0,0,1,0,1,0,0,
                               0,1,1,0,1,0,1,1,1,0,1,1,1,1,0,0,0,1,1,0,
                               1,0,1,1,1,0,0,0,0,1,0,0,0,1,0,1,1,1,0,0,
                               0,1,1,1,1,1,1,0,1,0,1,1,1,0,1,1,1,1,0,0,
                               0,1,0,0,0,0,1,1,1,0,0,0,1,0,0,1,1,0,0};

        private uint[] FB0 = {1,1,1,1,0,1,0,1,1,1,1,1,1,1,1,0,0,1,0,
                              1,1,1,1,1,1,1,1,1,1,0,0,1,1,0,0,0,0,0,
                              0,1,1,1,0,0,1,0,0,1,0,1,0,1,0,0,1,0,1,
                              1,1,1,0,1,0,1,0,1,0,0,0,0,0,0,0,0,0,1,
                              1,0,1,0,0,0,1,1,0,1,1,1,0,0,1,1,1,0,0,
                              1,1,0,0,0};

        private uint[] FB1 = {1,1,1,0,1,1,1,0,0,0,0,1,1,1,0,1,0,0,1,
                              1,0,0,0,1,0,0,1,1,0,0,1,0,1,1,0,0,0,1,
                              1,0,0,0,0,0,1,1,0,1,1,0,0,0,1,0,0,0,1,
                              0,0,1,0,0,1,0,1,1,0,1,0,1,0,0,1,0,1,0,
                              0,0,1,1,1,1,0,1,1,1,1,1,0,0,0,0,0,0,1,
                              0,0,0,0,1};

        #endregion

        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (MICKEY2Settings)value; }
        }

        #region Data Properties
        /// <summary>
        /// Input interface to read the input data. 
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputStringCaption", 
            "InputStringTooltip", false)]
        public string InputString
        {
            get { return this.inputString; }
            set
            {
                this.inputString = value;
                OnPropertyChanged("InputString");
            }
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", 
            "InputKeyTooltip", true)]
        public string InputKey
        {
            get { return this.inputKey; }
            set
            {

                this.inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", 
            "InputIVTooltip", true)]
        public string InputIV
        {
            get { return this.inputIV; }
            set
            {
                this.inputIV = value;
                OnPropertyChanged("InputIV");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", 
            "OutputStringTooltip", true)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                this.outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        #endregion

        #region IPlugin Members



        public UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            ProgressChanged(0, 11);

            // initialise the registers R and S with all zeros
            initMickey();
            ProgressChanged(2, 11);

            // Load in IV
            ProgressChanged(3, 11);
            {
                this.inputIVArray = hextobin(InputIV);

                // Stop when wrong Input
                if (inputIVArray.Length > 80)
                {
                    GuiLogMessage("Wrong input IV Length " + 
                        inputIVArray.Length + 
                        ". IV Length must be <= 80 Bits", 
                        NotificationLevel.Error);
                    return;
                }

                ProgressChanged(4, 11);
                //Start only when right Input
                if (inputIVArray.Length <= 80)
                {
                    for (int i = 0; i <= inputIVArray.Length - 1; 
                        i++)
                    {
                        CLOCK_KG(true, inputIVArray[i]);
                    }
                }
            }


            // Load in Key
            ProgressChanged(5, 11);
            {
                this.inputKeyArray = hextobin(InputKey);

                // Stop when wrong Input
                if ((this.inputKeyArray.Length == 80) == false)
                {
                    GuiLogMessage("Wrong input Key Length " + 
                        inputKeyArray.Length + 
                        ". IV Length must be 80 Bits", 
                        NotificationLevel.Error);
                    return;
                }

                ProgressChanged(6, 11);
                for (int i = 0; i <= 79; i++)
                {
                    CLOCK_KG(true, this.inputKeyArray[i]);
                }
            }

            ProgressChanged(7, 11);
            //Preclock
            for (int i = 0; i <= 99; i++)
            {
                CLOCK_KG(true, 0);
            }


            //try to encode the plaintext
            ProgressChanged(8, 11);
            bool validInput = false;
            try
            {
                uint[] inputArray;
                inputArray = hextobin(InputString);

                if (inputArray.Length > 0)
                {
                    uint[] ciphertext = new uint[inputArray.Length];
                    validInput = true;

                    //generate keystream
                    generatingKeystream(ciphertext.Length);

                    //encode the plaintext
                    for (int i = 0; i < inputArray.Length; i++)
                    {
                        ciphertext[i] = ((inputArray[i] ^ 
                            this.keystream[i]));
                    }
                    this.keystream = ciphertext;
                }

            }
            catch (System.Exception exception)
            {
            }

            ProgressChanged(9, 11);
            if (validInput == false)
            {
                GuiLogMessage("No valid input of plaintext", 
                    NotificationLevel.Info);
                GuiLogMessage("Generating 512 byte (4096 bit) of"+
                    " keystream", NotificationLevel.Info);
                //generate keystream
                generatingKeystream(4096);
            }

            //generate output
            ProgressChanged(10, 11);
            outputString = "";
            for (int i = 0; i < this.keystream.Length; i++)
                this.outputString += this.keystream[i];
            if (this.settings.BinOutput == false)
            {
                this.outputString = bintohex(this.outputString);
            }
            this.OutputString = this.outputString;


            ProgressChanged(11, 11);
        }

        public void PostExecution()
        {
        }


        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        //Generating keystream
        public void generatingKeystream(int length)
        {
            keystream = new uint[length];
            for (int i = 0; i < length; i++)
            {
                this.keystream[i] = this.registerR[0] ^ 
                    this.registerS[0];
                CLOCK_KG(false, 0);
            }
        }

        # region clocking

        ///Clocking the register R
        public void CLOCK_R(uint INPUT_BIT_R, uint CONTROL_BIT_R)
        {
            uint[] registerRBefore = this.registerR;
            uint[] registerRAfter = new uint[100];// = this.registerR;

            uint FEEDBACK_BIT = registerRBefore[99] ^ INPUT_BIT_R;

            for (int i = 1; i <= 99; i++)
            {
                registerRAfter[i] = registerRBefore[i - 1];
            }
            registerRAfter[0] = 0;


            ///if i element of RTAPS 
            foreach (int i in RTAPS)
            {
                registerRAfter[i] = (registerRAfter[i] ^ FEEDBACK_BIT);

            }

            if (CONTROL_BIT_R == 1)
            {
                for (int i = 0; i <= 99; i++)
                {
                    registerRAfter[i] = (registerRAfter[i] ^ registerRBefore[i]);
                }
            }

            this.registerR = registerRAfter;

        }

        ///Clocking the register S
        public void CLOCK_S(uint INPUT_BIT_S, uint CONTROL_BIT_S)
        {
            uint[] registerSBefore = this.registerS;
            uint[] registerSAfter = new uint[100];
            uint[] registerSIntermediate = new uint[100];

            uint FEEDBACK_BIT = this.registerS[99] ^ INPUT_BIT_S;

            for (int i = 1; i <= 98; i++)
            {
                registerSIntermediate[i] = registerSBefore[i - 1] ^
                    ((registerSBefore[i] ^ this.COMP0[i]) &
                    (registerSBefore[i + 1] ^ this.COMP1[i]));
            }
            registerSIntermediate[0] = 0;
            registerSIntermediate[99] = registerSBefore[98];

            if (CONTROL_BIT_S == 0)
            {
                for (int i = 0; i <= 99; i++)
                {
                    registerSAfter[i] = registerSIntermediate[i] ^
                        (this.FB0[i] & FEEDBACK_BIT);
                }
            }

            else
            {
                for (int i = 0; i <= 99; i++)
                {
                    registerSAfter[i] = registerSIntermediate[i] ^
                        (this.FB1[i] & FEEDBACK_BIT);
                }
            }

            this.registerS = registerSAfter;

        }

        //Clocking the overall generator
        public void CLOCK_KG(bool MIXING, uint INPUT_BIT)
        {
            uint CONTROL_BIT_R = this.registerS[34] ^ this.registerR[67];
            uint CONTROL_BIT_S = this.registerS[67] ^ this.registerR[33];

            uint INPUT_BIT_R;
            if (MIXING == true)
            {
                INPUT_BIT_R = INPUT_BIT ^ this.registerS[50];
            }
            else
            {
                INPUT_BIT_R = INPUT_BIT;
            }

            uint INPUT_BIT_S = INPUT_BIT;

            CLOCK_R(INPUT_BIT_R, CONTROL_BIT_R);
            CLOCK_S(INPUT_BIT_S, CONTROL_BIT_S);
        }

        # endregion

        public void initMickey()
        {
            //Initialise the registerR with all zeros.
            for (int i = 0; i < this.registerR.Length; i++)
            {
                this.registerR[i] = 0;
            }

            //Initialise the registerS with all zeros.
            for (int i = 0; i < this.registerS.Length; i++)
            {
                this.registerS[i] = 0;
            }
        }

        #region transformations
        public uint[] hextobin(string hexString)
        {
            hexString = hexString.Replace(" ", "").ToLower();

            uint[] binArray = new uint[hexString.Length * 4];
            string binString = "";
            for (int i = 0; i < hexString.Length; i++)
            {
                if (hexString[i].Equals('0')) binString += "0000";
                else if (hexString[i].Equals('1')) 
                    binString += ("0001");
                else if (hexString[i].Equals('2')) 
                    binString += ("0010");
                else if (hexString[i].Equals('3')) 
                    binString += ("0011");
                else if (hexString[i].Equals('4')) 
                    binString += ("0100");
                else if (hexString[i].Equals('5')) 
                    binString += ("0101");
                else if (hexString[i].Equals('6')) 
                    binString += ("0110");
                else if (hexString[i].Equals('7')) 
                    binString += ("0111");
                else if (hexString[i].Equals('8')) 
                    binString += ("1000");
                else if (hexString[i].Equals('9')) 
                    binString += ("1001");
                else if (hexString[i].Equals('a')) 
                    binString += ("1010");
                else if (hexString[i].Equals('b')) 
                    binString += ("1011");
                else if (hexString[i].Equals('c')) 
                    binString += ("1100");
                else if (hexString[i].Equals('d')) 
                    binString += ("1101");
                else if (hexString[i].Equals('e')) 
                    binString += ("1110");
                else if (hexString[i].Equals('f')) 
                    binString += ("1111");
                else
                {
                    GuiLogMessage("No valid input character only 0-9"
                        +" and a-f", NotificationLevel.Error);
                    return null;
                }
            }

            for (int i = 0; i < binString.Length; i++)
            {
                binArray[i] = 
                    (uint)System.Convert.ToInt32(binString.
                    Substring(i, 1));
            }
            return binArray;
        }


        public string bintohex(string binString)
        {
            string[] binStringArray = 
                new string[binString.Length / 4];

            for (int i = 0; i < binStringArray.Length; i++)
            {
                binStringArray[i] = binString.Substring(i * 4, 4);
            }
            string hexString = "";

            for (int i = 0; i < binStringArray.Length; i++)
            {
                if (binStringArray[i].Equals("0000")) 
                    hexString += ('0');
                else if (binStringArray[i].Equals("0001")) 
                    hexString += ('1');
                else if (binStringArray[i].Equals("0010")) 
                    hexString += ('2');
                else if (binStringArray[i].Equals("0011")) 
                    hexString += ('3');
                else if (binStringArray[i].Equals("0100")) 
                    hexString += ('4');
                else if (binStringArray[i].Equals("0101")) 
                    hexString += ('5');
                else if (binStringArray[i].Equals("0110")) 
                    hexString += ('6');
                else if (binStringArray[i].Equals("0111")) 
                    hexString += ('7');
                else if (binStringArray[i].Equals("1000")) 
                    hexString += ('8');
                else if (binStringArray[i].Equals("1001")) 
                    hexString += ('9');
                else if (binStringArray[i].Equals("1010")) 
                    hexString += ('a');
                else if (binStringArray[i].Equals("1011")) 
                    hexString += ('b');
                else if (binStringArray[i].Equals("1100")) 
                    hexString += ('c');
                else if (binStringArray[i].Equals("1101")) 
                    hexString += ('d');
                else if (binStringArray[i].Equals("1110")) 
                    hexString += ('e');
                else if (binStringArray[i].Equals("1111")) 
                    hexString += ('f');
            }
            return hexString;
        }
        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler 
            OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler 
            OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel 
            logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, 
                this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, 
                new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, 
                this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
