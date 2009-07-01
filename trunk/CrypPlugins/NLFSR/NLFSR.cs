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
// for [MethodImpl(MethodImplOptions.Synchronized)]
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
// for QuickwatchPresentation
using System.Windows.Threading;
using System.Threading;
using System.Windows.Automation.Peers;
// for RegEx
using System.Text.RegularExpressions;

namespace Cryptool.NLFSR
{
    [Author("Soeren Rinne", "soeren.rinne@cryptool.de", "Ruhr-Universitaet Bochum, Chair for System Security", "http://www.trust.rub.de/")]
    [PluginInfo(false, "NLFSR", "Linear Feedback Shift Register", "NLFSR/DetailedDescription/Description.xaml", "NLFSR/Images/NLFSR.png", "NLFSR/Images/encrypt.png", "NLFSR/Images/decrypt.png")]
    [EncryptionType(EncryptionType.SymmetricBlock)]
    public class NLFSR : IThroughput
    {
        #region IPlugin Members

        private NLFSRSettings settings;
        private String inputTapSequence;
        private String inputSeed;
        private CryptoolStream inputClock;
        private CryptoolStream outputStream;
        private String outputString;
        private bool outputBool;
        private bool inputClockBool;
        private bool outputClockingBit;
        private string tapSequenceString = null;
        
        private NLFSRPresentation NLFSRPresentation;
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();

        #endregion

        #region public variables

        public bool stop = false;
        public bool newSeed = true;
        public String seedbuffer = "0";
        public String tapSequencebuffer = "1";
        public Char outputbuffer = '0';
        public bool lastInputPropertyWasBoolClock = false;

        // for process()
        public char[] tapSequenceCharArray = null;
        public int seedBits = 1; // dummy value for compiler
        public int actualRounds = 1; // dummy value for compiler
        public Boolean myClock = true;
        public char[] seedCharArray = null;
        public int clocking;
        public string outputStringBuffer = null;

        #endregion

        #region public interfaces

        public NLFSR()
        {
            this.settings = new NLFSRSettings();
            //((NLFSRSettings)(this.settings)).LogMessage += NLFSR_LogMessage;

            NLFSRPresentation = new NLFSRPresentation();
            Presentation = NLFSRPresentation;
            //NLFSRPresentation.textBox0.TextChanged += textBox0_TextChanged;
        }

        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (NLFSRSettings)value; }
        }

        [PropertyInfo(Direction.Input, "TapSequence", "TapSequence function in binary presentation.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public String InputTapSequence
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return inputTapSequence; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                inputTapSequence = value;
                OnPropertyChanged("InputTapSequence");
                lastInputPropertyWasBoolClock = false;
            }
        }

        [PropertyInfo(Direction.Input, "Seed", "Seed of the NLFSR in binary presentation.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public String InputSeed
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return inputSeed; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                inputSeed = value;
                OnPropertyChanged("InputSeed");
                lastInputPropertyWasBoolClock = false;
            }
        }
        /*
        [PropertyInfo(Direction.Input, "Clock", "Optional clock input. NLFSR only advances if clock is 1.", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public CryptoolStream InputClock
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                if (inputClock != null)
                {
                    CryptoolStream cs = new CryptoolStream();
                    cs.OpenRead(inputClock.FileName);
                    listCryptoolStreamsOut.Add(cs);
                    return cs;
                }
                else return null;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                this.inputClock = value;
                if (value != null) listCryptoolStreamsOut.Add(value);
                OnPropertyChanged("InputClock");
            }
        }*/

        [PropertyInfo(Direction.Input, "Clock", "Optional clock input. NLFSR only advances if clock is true.", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public Boolean InputClockBool
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return inputClockBool; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                inputClockBool = value;
                OnPropertyChanged("InputClockBool");
                lastInputPropertyWasBoolClock = true;
            }
        }

        [PropertyInfo(Direction.Output, "Output stream", "NLFSR Stream Output. Use this for bulk output.", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public CryptoolStream OutputStream
        {
            //[MethodImpl(MethodImplOptions.Synchronized)]
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
            //[MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                outputStream = value;
                if (value != null) listCryptoolStreamsOut.Add(value);
                OnPropertyChanged("OutputStream");
            }
        }

        [PropertyInfo(Direction.Output, "String Output", "Produces the output bits as a string with length==rounds. Use this output without a clock input.", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public String OutputString
        {
            get { return outputString; }
            set
            {
                outputString = value.ToString();
                OnPropertyChanged("OutputString");
            }
        }

        [PropertyInfo(Direction.Output, "Boolean Output", "NLFSR Boolean Output. Use this output together with a clock input.", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public bool OutputBool
        {
            get { return outputBool; }
            set
            {
                outputBool = (bool)value;
                //OnPropertyChanged("OutputBool");
            }
        }

        [PropertyInfo(Direction.Output, "Clocking Bit Output", "Clocking Bit Output.", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public bool OutputClockingBit
        {
            get { return outputClockingBit; }
            set
            {
                outputClockingBit = (bool)value;
                OnPropertyChanged("OutputClockingBit");
            }
        }
        
        private bool controllerOutput;
        [ControllerProperty(Direction.Output, "Controller Output", "", DisplayLevel.Beginner)]
        public object ControllerOutput
        {
            get { return controllerOutput; }
            set { controllerOutput = (bool)value; }
        }

        public void Dispose()
        {
            try
            {
                stop = false;
                outputStream = null;
                outputString = null;
                outputStringBuffer = null;
                inputClock = null;
                inputTapSequence = null;
                inputSeed = null;

                if (inputClock != null)
                {
                    inputClock.Flush();
                    inputClock.Close();
                    inputClock = null;
                }

                if (outputStream != null)
                {
                    outputStream.Flush();
                    outputStream.Close();
                    outputStream = null;
                }
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

        #endregion

        #region private functions

        private void checkForInputTapSequence()
        {
            if ((inputTapSequence == null || (inputTapSequence != null && inputTapSequence.Length == 0)))
            {
                // create some input
                String dummystring = "1011";
                // this.inputTapSequence = new String();
                inputTapSequence = dummystring;
                // write a warning to the outside world
                GuiLogMessage("WARNING - No TapSequence provided. Using dummy data (" + dummystring + ").", NotificationLevel.Warning);
            }
        }

        private void checkForInputSeed()
        {
            if ((inputSeed == null || (inputSeed != null && inputSeed.Length == 0)))
            {
                // create some input
                String dummystring = "1010";
                // this.inputSeed = new CryptoolStream();
                inputSeed = dummystring;
                // write a warning to the outside world
                GuiLogMessage("WARNING - No Seed provided. Using dummy data (" + dummystring + ").", NotificationLevel.Warning);
            }
        }

        private void checkForInputClock()
        {
            if ((inputClock == null || (inputClock != null && inputClock.Length == 0)))
            {
                //create some input
                String dummystring = "1";
                this.inputClock = new CryptoolStream();
                this.inputClock.OpenRead(this.GetPluginInfoAttribute().Caption, Encoding.Default.GetBytes(dummystring.ToCharArray()));
                // write a warning to the outside world
                GuiLogMessage("FYI - No clock input provided. Assuming number of rounds specified in NLFSR settings.", NotificationLevel.Info);
            }
        }

        // Function to make binary representation of polynomial
        private String MakeBinary(String strPoly)
        {
            bool gotX = false;
            // remove spaces
            strPoly = strPoly.Replace(" ", "");

            Regex gotXRegEx = new Regex("(\\+x\\+1)+$");
            if (gotXRegEx.IsMatch(strPoly)) gotX = true;
            // remove last '1'
            strPoly = strPoly.Remove(strPoly.Length - 1, 1);
            // remove all x
            strPoly = strPoly.Replace("x", "");
            // remove all ^
            strPoly = strPoly.Replace("^", "");

            // split in string array
            string[] strPolySplit = strPoly.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
            // convert to integer
            int[] intPolyInteger = new int[strPolySplit.Length];
            for (int i = strPolySplit.Length - 1; i >= 0; i--)
            {
                intPolyInteger[i] = Convert.ToInt32(strPolySplit[i]);
            }

            string strPolyBinary = null;
            if (strPoly.Length != 0)
            {
                Array.Sort(intPolyInteger);
                int highestOne = intPolyInteger[intPolyInteger.Length - 1];

                int j = intPolyInteger.Length - 1;
                for (int i = highestOne; i > 1; i--)
                {
                    if (j < 0 && (i != 1 || i != 0)) strPolyBinary += 0;
                    else if (intPolyInteger[j] == i)
                    {
                        strPolyBinary += 1;
                        j--;
                    }
                    else strPolyBinary += 0;
                }
            }
            if (gotX) strPolyBinary += 1;
            else strPolyBinary += 0;

            strPolyBinary = new String(ReverseOrder(strPolyBinary.ToCharArray()));
            //GuiLogMessage("strPolyBinary is: " + strPolyBinary, NotificationLevel.Info);

            return strPolyBinary;
        }

        // Function to test for NLFSR Polnyomial
        private bool IsPolynomial(String strPoly)
        {
            // delete spaces
            strPoly = strPoly.Replace(" ", "");
            //(x\^([2-9]|[0-9][0-9])\+)*[x]?([\+]?1){1}
            // TODO
            //Regex objPolynomial = new Regex("(x\\^([2-9]|[0-9][0-9])([\\*]|[\\+]|[\\|]|[\\-]|[_]|[°]|[v]|[\\^]))+[x]?");
            Regex objBoolExpression = new Regex("([0-1]([\\*]|[\\+]|[\\|]|[\\-]|[_]|[°]|[v]|[\\^])+[0-1]{1})");
            //Regex keinHoch0 = new Regex("(x\\^[0]\\+)+");
            //return !keinHoch0.IsMatch(strPoly) && objBoolExpression.IsMatch(strPoly);
            return objBoolExpression.IsMatch(strPoly);
        }

        // Function to turn around tapSequence (01101 -> 10110)
        private char[] ReverseOrder(char[] tapSequence)
        {
            //String tempString = new String(tapSequence);
            //GuiLogMessage("tapSequence before = " + tempString, NotificationLevel.Info);
            char[] tempCharArray = new char[tapSequence.Length];

            int temp;
            for (int j = tapSequence.Length - 1; j >= 0; j--)
            {
                temp = (j - tapSequence.Length + 1) % (tapSequence.Length);
                if (temp < 0) temp *= -1;
                //GuiLogMessage("temp = " + temp, NotificationLevel.Info);
                tempCharArray[j] = tapSequence[temp];
            }
            //tempString = new String(tempCharArray);
            //GuiLogMessage("tapSequence after = " + tempString, NotificationLevel.Info);
            return tempCharArray;
        }

        private string BuildPolynomialFromBinary(char [] tapSequence)
        {
            string polynomial = "Feedback polynomial: \n";
            char[] tempTapSequence = ReverseOrder(tapSequence);
            int power;

            //build polynomial
            for (int i = 0; i < tapSequence.Length; i++)
            {
                power = (i - tapSequence.Length + 1) * -1 % tapSequence.Length + 1;
                if (tempTapSequence[i] == '1')
                {
                    if (power == 1) polynomial += "x + ";
                    else if (power != 0) polynomial += "x^" + power + " + ";
                    //else polynomial += "1";
                }
            }
            // add last "+1"
            polynomial += "1";

            return  polynomial;
        }

        private string ReplaceVariables(string strExpressionWithVariables, char[] currentState)
        {
            string strExpression = strExpressionWithVariables;
            //GuiLogMessage("strExpression /w vars: " + strExpression, NotificationLevel.Info);
            char[] strFSRValues = new char[currentState.Length]; // fix the length

            // TODO: [i-1] ist falschrum -> gelöst durch ReverseOrder?
            char[] temp = new char[currentState.Length];
            temp = ReverseOrder(currentState);
            string replacement = null;

            for (int i = 1; i <= strFSRValues.Length; i++)
            {
                replacement = "x^" + i;
                strExpression = strExpression.Replace(replacement, temp[i-1].ToString());
                //GuiLogMessage("temp[i-1]: " + temp[i - 1].ToString(), NotificationLevel.Info);
            }
            //strExpression = strExpression.Replace("x", currentState[currentState.Length-1].ToString());

            // replace AND, NAND, OR, NOR, XOR, NXOR with symbols
            // NAND => -
            strExpression = strExpression.Replace("NAND", "-");
            // AND => +
            strExpression = strExpression.Replace("AND", "+");

            // NOR => _
            strExpression = strExpression.Replace("NOR", "_");

            // NXOR => °
            strExpression = strExpression.Replace("NXOR", "°");
            // XOR => *
            strExpression = strExpression.Replace("XOR", "*");

            // OR => |
            strExpression = strExpression.Replace("OR", "|");

            // replace ^ and v with symbols
            // ^ => AND => +
            strExpression = strExpression.Replace("^", "+");

            // v => OR => |
            strExpression = strExpression.Replace("v", "|");

            //GuiLogMessage("strExpression w/o vars: " + strExpression, NotificationLevel.Info);

            return strExpression;
        }

        private ATreeNode FillTree(string strExpression)
        {
            // fill tree
            //char[] charPolynomial = { '0', '*', '1', '+', '0', '*', '1', '+', '1' }; // sample for debug
            // replace spaces with nothing
            strExpression = strExpression.Replace(" ", "");
            char[] charPolynomial = strExpression.ToCharArray();
            int i;
            ATreeNode[] treeArray = new ATreeNode[charPolynomial.Length];

            for (i = 0; i < charPolynomial.Length; i++)
            {
                if (Char.IsDigit(charPolynomial[i]))
                // should be a digit
                {
                    if (i == 0)
                    {
                        // first digit
                        treeArray[i] = new ATreeNode("Constant");
                        if (charPolynomial[i] == '0') treeArray[i].ConstantData = false;
                        else treeArray[i].ConstantData = true;
                    }
                    else
                    {
                        // any other digit
                        treeArray[i] = new ATreeNode("Constant");
                        if (charPolynomial[i] == '0') treeArray[i].ConstantData = false;
                        else treeArray[i].ConstantData = true;
                        treeArray[i - 1].RightChild = treeArray[i];
                    }
                }
                else
                // should be an operand
                {

                    /*if (charPolynomial[i] == '!')
                    {
                        // ! operand
                        // TO DO
                    }
                    else*/
                    {
                        // * or + operands
                        if (i == 1)
                        {
                            treeArray[i] = new ATreeNode(charPolynomial[i].ToString());
                            treeArray[i].LeftChild = treeArray[i - 1];
                        }
                        else
                        {
                            treeArray[i] = new ATreeNode(charPolynomial[i].ToString());
                            treeArray[i].LeftChild = treeArray[i - 2];
                        }
                    }
                }
                // debug output
                //Console.Out.Write(treeArray[i].NodeType + " ");
            }

            return treeArray[treeArray.Length - 2];
        }

        private bool EvaluateTree(bool X, ATreeNode NodePointer)
        {
            //float RightTemp, LeftTemp, UnaryTemp;

            switch (NodePointer.NodeType)
            {
                case "+":
                    X = EvaluateTree(X, NodePointer.LeftChild) && EvaluateTree(X, NodePointer.RightChild);
                    break;

                case "-":
                    X = !(EvaluateTree(X, NodePointer.LeftChild) && EvaluateTree(X, NodePointer.RightChild));
                    break;

                case "*":
                    X = EvaluateTree(X, NodePointer.LeftChild) ^ EvaluateTree(X, NodePointer.RightChild);
                    break;

                case "°":
                    X = !(EvaluateTree(X, NodePointer.LeftChild) ^ EvaluateTree(X, NodePointer.RightChild));
                    break;

                case "|":
                    X = EvaluateTree(X, NodePointer.LeftChild) | EvaluateTree(X, NodePointer.RightChild);
                    break;

                case "_":
                    X = !(EvaluateTree(X, NodePointer.LeftChild) | EvaluateTree(X, NodePointer.RightChild));
                    break;

                case "XVariable":
                    // X = X;
                    break;

                case "Constant":
                    X = NodePointer.ConstantData;
                    break;
            }

            return X;
        }

        #endregion

        public void Execute()
        {
            NLFSRPresentation.DeleteAll(100);
            processNLFSR();
        }

        private void processNLFSR()
        {
            // check if event was from the boolean clock input
            // if so, check if boolean clock should be used
            // if not, do not process NLFSR
            if (lastInputPropertyWasBoolClock)
            {
                if (!settings.UseBoolClock) return;
                //GuiLogMessage("First if.", NotificationLevel.Info);
            }
                // if last event wasn't from the clock but clock shall be
                // the only event to start from, do not go on
            else
            {
                if (settings.UseBoolClock) return;
                //GuiLogMessage("Second if.", NotificationLevel.Info);
            }
            // process NLFSR
            
            try
            {
                /*char[] tapSequenceCharArray = null;
                int seedBits = 1; // dummy value for compiler
                int actualRounds = 1; // dummy value for compiler
                Boolean myClock = true;
                char[] seedCharArray = null;

                // open output stream
                outputStream = new CryptoolStream();
                listCryptoolStreamsOut.Add(outputStream);
                outputStream.OpenWrite(this.GetPluginInfoAttribute().Caption);
                */

                // make all this stuff only one time at the beginning of our chainrun
                if (newSeed)
                {
                    checkForInputTapSequence();
                    checkForInputSeed();

                    if (inputSeed == null || (inputSeed != null && inputSeed.Length == 0))
                    {
                        GuiLogMessage("No Seed given. Aborting now.", NotificationLevel.Error);
                        if (!settings.UseBoolClock) inputClock.Close();
                        return;
                    }

                    if (inputTapSequence == null || (inputTapSequence != null && inputTapSequence.Length == 0))
                    {
                        GuiLogMessage("No TapSequence given. Aborting now.", NotificationLevel.Error);
                        if (!settings.UseBoolClock) inputClock.Close();
                        return;
                    }

                    // read tapSequence
                    tapSequencebuffer = inputTapSequence;

                    // check if tapSequence is binary
                    /*bool tapSeqisBool = true;
                    foreach (char character in tapSequencebuffer)
                    {
                        if (character != '0' && character != '1')
                        {
                            tapSeqisBool = false;
                            //return;
                        }
                    }*/

                    // if tapSequence is not binary, await polynomial
                    /*if (!tapSeqisBool)
                    {
                        GuiLogMessage("TapSequence should not binary. Awaiting polynomial.", NotificationLevel.Info);
                        if (IsPolynomial(tapSequencebuffer))
                        {
                            GuiLogMessage(tapSequencebuffer + " is a valid polynomial.", NotificationLevel.Info);
                            tapSequenceString = tapSequencebuffer;
                            tapSequencebuffer = MakeBinary(tapSequencebuffer);
                            GuiLogMessage("Polynomial in binary form: " + tapSequencebuffer, NotificationLevel.Info);

                            // check if polynomial has false length
                            if (tapSequencebuffer.Length != inputSeed.Length)
                            {
                                GuiLogMessage("ERROR - Your polynomial " + tapSequencebuffer + " has to be the same length (" + tapSequencebuffer.Length + " Bits) as your seed (" + inputSeed.Length + " Bits). Aborting now.", NotificationLevel.Error);
                                if (!settings.UseBoolClock) inputClock.Close();
                                return;
                            }

                            //GuiLogMessage("Polynomial after length fitting: " + tapSequencebuffer, NotificationLevel.Info);
                        }
                        else
                        {
                            GuiLogMessage("ERROR - " + tapSequencebuffer + " is NOT a valid polynomial. Aborting now.", NotificationLevel.Error);
                            //Console.WriteLine("\n{0} is NOT a valid polynomial.", tapSequencebuffer);
                            if (!settings.UseBoolClock) inputClock.Close();
                            return;
                        }
                    }*/

                    // convert tapSequence into char array
                    //tapSequenceCharArray = ReverseOrder(tapSequencebuffer.ToCharArray());
                    tapSequenceCharArray = tapSequencebuffer.ToCharArray();

                    int tapSequenceBits = inputTapSequence.Length;
                    seedBits = inputSeed.Length;

                    GuiLogMessage("inputTapSequence length [bits]: " + tapSequenceBits.ToString(), NotificationLevel.Debug);
                    GuiLogMessage("inputSeed length [bits]: " + seedBits.ToString(), NotificationLevel.Debug);

                    //read seed only one time until stop of chain
                    seedbuffer = inputSeed;
                    newSeed = false;

                    //check if last tap is 1, otherwise stop
                    /*if (tapSequenceCharArray[tapSequenceCharArray.Length - 1] == '0')
                    {
                        GuiLogMessage("ERROR - Last tap of tapSequence must be 1. Aborting now.", NotificationLevel.Error);
                        return;
                    }*/

                    // convert seed into char array
                    seedCharArray = seedbuffer.ToCharArray();

                    // check if seed is binary
                    foreach (char character in seedCharArray)
                    {
                        if (character != '0' && character != '1')
                        {
                            GuiLogMessage("ERROR 0 - Seed has to be binary. Aborting now. Character is: " + character, NotificationLevel.Error);
                            return;
                        }
                    }
                    if (settings.UseClockingBit)
                    {
                        if (settings.ClockingBit < seedCharArray.Length) clocking = (seedCharArray.Length - settings.ClockingBit - 1);
                        else
                        {
                            clocking = -1;
                            GuiLogMessage("WARNING: Clocking Bit is too high. Ignored.", NotificationLevel.Warning);
                        }

                    }
                    else clocking = -1;

                    // check if Rounds are given
                    int defaultRounds = 1;

                    // check if Rounds in settings are given and use them only if no bool clock is selected
                    if (!settings.UseBoolClock)
                    {
                        if (settings.Rounds == 0) actualRounds = defaultRounds; else actualRounds = settings.Rounds;
                    }
                    else actualRounds = 1;
                }
                
                // Here we go!
                // check which clock to use
                if (settings.UseBoolClock)
                {
                    myClock = inputClockBool;
                }
                else if (!settings.UseBoolClock)
                {
                    // read stream clock
                    checkForInputClock();
                    inputClock.OpenWrite("NLFSR Restart");
                    String stringClock = inputClock.ReadByte().ToString();
                    inputClock.Position = 0;
                    if (String.Equals(stringClock, "49")) myClock = true; else myClock = false;
                    //inputClock.Close();
                }

                // draw NLFSR Quickwatch
                //TODO
                if (!settings.NoQuickwatch)
                {
                    NLFSRPresentation.DrawNLFSR(seedCharArray, seedCharArray, clocking);
                }

                // open output stream
                outputStream = new CryptoolStream();
                listCryptoolStreamsOut.Add(outputStream);
                outputStream.OpenWrite(this.GetPluginInfoAttribute().Caption);

                //GuiLogMessage("Action is: Now!", NotificationLevel.Debug);
                DateTime startTime = DateTime.Now;

                //////////////////////////////////////////////////////
                // compute NLFSR //////////////////////////////////////
                //////////////////////////////////////////////////////
                GuiLogMessage("Starting computation", NotificationLevel.Debug);
                
                int i = 0;
                
                for (i = 0; i < actualRounds; i++)
                {
                    // compute only if clock = 1 or true
                    if (myClock)
                    {
                        StatusChanged((int)NLFSRImage.Encode);

                        // make bool output
                        if (seedCharArray[seedBits - 1] == '0') outputBool = false;
                        else outputBool = true;
                        //GuiLogMessage("OutputBool is: " + outputBool.ToString(), NotificationLevel.Info);

                        // write last bit to output buffer, output stream buffer, stream and bool
                        outputbuffer = seedCharArray[seedBits - 1];
                        outputStream.Write((Byte)outputbuffer);
                        if (!settings.UseBoolClock) outputStringBuffer += seedCharArray[seedBits - 1];

                        // update outputs
                        OnPropertyChanged("OutputBool");
                        OnPropertyChanged("OutputStream");

                        // shift seed array
                        char newBit = '0';

                        ////////////////////
                        // compute new bit
                        // replace variables with bits from FSR
                        string tapPolynomial = null;
                        tapPolynomial = ReplaceVariables(tapSequencebuffer, seedCharArray);
                        if (!IsPolynomial(tapPolynomial))
                        {
                            GuiLogMessage("ERROR - " + tapSequencebuffer + " is NOT a valid polynomial. Aborting now.", NotificationLevel.Error);
                            if (!settings.UseBoolClock) inputClock.Close();
                            return;
                        }
                        GuiLogMessage("tapPolynomial is: " + tapPolynomial, NotificationLevel.Info);

                        bool resultBool = true;
                        resultBool = EvaluateTree(resultBool, FillTree(tapPolynomial));

                        GuiLogMessage("resultBool is: " + resultBool, NotificationLevel.Info);
                        if (resultBool) newBit = '1'; else newBit = '0';

                        // keep output bit for presentation
                        char outputBit = seedCharArray[seedBits - 1];

                        // shift seed array
                        for (int j = seedBits - 1; j > 0; j--)
                        {
                            seedCharArray[j] = seedCharArray[j - 1];
                            //GuiLogMessage("seedCharArray[" + j + "] is: " + seedCharArray[j], NotificationLevel.Info);
                        }
                        seedCharArray[0] = newBit;

                        //update quickwatch presentation
                        if (!settings.NoQuickwatch)
                        {
                            NLFSRPresentation.FillBoxes(seedCharArray, tapSequenceCharArray, outputBit, BuildPolynomialFromBinary(tapSequenceCharArray));
                        }

                        // write current "seed" back to seedbuffer
                        seedbuffer = null;
                        foreach (char c in seedCharArray) seedbuffer += c;

                        //GuiLogMessage("New Bit: " + newBit.ToString(), NotificationLevel.Info);
                    }
                    else
                    {
                        StatusChanged((int)NLFSRImage.Decode);

                        if (settings.AlwaysCreateOutput)
                        {
                            /////////
                            // but nevertheless fire an output event with dirty value / old value
                            /////////
                            if (settings.CreateDirtyOutputOnFalseClock)
                            {
                                outputBool = false;
                                outputbuffer = '2';
                                outputStream.Write((Byte)outputbuffer);

                                OnPropertyChanged("OutputBool");
                                OnPropertyChanged("OutputStream");

                                //update quickwatch presentation
                                if (!settings.NoQuickwatch)
                                {
                                    NLFSRPresentation.FillBoxes(seedCharArray, tapSequenceCharArray, '2', BuildPolynomialFromBinary(tapSequenceCharArray));
                                }
                            }
                            else
                            {
                                // make bool output
                                if (seedCharArray[seedBits - 1] == '0') outputBool = false;
                                else outputBool = true;
                                //GuiLogMessage("OutputBool is: " + outputBool.ToString(), NotificationLevel.Info);

                                // write last bit to output buffer, stream and bool
                                outputbuffer = seedCharArray[seedBits - 1];
                                outputStream.Write((Byte)outputbuffer);

                                OnPropertyChanged("OutputBool");
                                OnPropertyChanged("OutputStream");

                                //update quickwatch presentation
                                if (!settings.NoQuickwatch)
                                {
                                    NLFSRPresentation.FillBoxes(seedCharArray, tapSequenceCharArray, seedCharArray[seedBits - 1], BuildPolynomialFromBinary(tapSequenceCharArray));
                                }
                            }
                            /////////
                        }
                        else
                        {
                            // update quickwatch with current state but without any output bit
                            if (!settings.NoQuickwatch)
                            {
                                NLFSRPresentation.FillBoxes(seedCharArray, tapSequenceCharArray, ' ', BuildPolynomialFromBinary(tapSequenceCharArray));
                            }
                        }

                        //GuiLogMessage("NLFSR Clock is 0, no computation.", NotificationLevel.Info);
                        //return;
                    }
                    // in both cases update "clocking bit" if set in settings
                    if (settings.UseClockingBit)
                    {
                        // make clocking bit output only if its not out of bounds
                        if (clocking != -1)
                        {
                            if (seedCharArray[clocking] == '0') outputClockingBit = false;
                            else outputClockingBit = true;
                            OnPropertyChanged("OutputClockingBit");
                        }
                    }
                }

                controllerOutput = true;
                OnPropertyChanged("ControllerOutput");

                // stop counter
                DateTime stopTime = DateTime.Now;
                // compute overall time
                TimeSpan duration = stopTime - startTime;

                if (!stop)
                {
                    // finally write output string if no bool-clock was used
                    if (!settings.UseBoolClock)
                    {
                        outputString = outputStringBuffer;
                        OnPropertyChanged("OutputString");
                    }

                    GuiLogMessage("Complete!", NotificationLevel.Debug);

                    GuiLogMessage("Time used: " + duration, NotificationLevel.Debug);
                    outputStream.Close();
                    if (!settings.UseBoolClock) inputClock.Close();
                    OnPropertyChanged("OutputStream");
                }

                if (stop)
                {
                    outputStream.Close();
                    if (!settings.UseBoolClock) inputClock.Close();
                    GuiLogMessage("Aborted!", NotificationLevel.Debug);
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

        #region events and stuff

        public void Initialize()
        {
            //throw new NotImplementedException();
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
            //throw new NotImplementedException();
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            Dispose();
        }

        public void Stop()
        {
            StatusChanged((int)NLFSRImage.Default);
            newSeed = true;
            stop = true;
        }

        public UserControl Presentation { get; private set; }

        public UserControl QuickWatchPresentation
        {
            get { return Presentation; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            //EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            if (PropertyChanged != null)
            {
              PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private void StatusChanged(int imageIndex)
        {
            EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, imageIndex));
        }

        #endregion
    }

    #region Image

    enum NLFSRImage
    {
        Default,
        Encode,
        Decode
    }

    #endregion

    public class ATreeNode
    {
        #region private variables

        private ATreeNode _leftChild;
        private ATreeNode _rightChild;
        private ATreeNode _unaryChild;
        private string _nodeType;
        private bool _constantData;

        /*private enum _nodeType
        {
            Add,Subtract,Multiply,Divide,Power,AbsValue,Log,Negation,Sine,Square,SquareRoot,XVariable,Constant
        }*/

        #endregion

        #region public interfaces

        public ATreeNode LeftChild
        {
            get { return _leftChild; }
            set { _leftChild = value; }
        }

        public ATreeNode RightChild
        {
            get { return _rightChild; }
            set { _rightChild = value; }
        }

        public ATreeNode UnaryChild
        {
            get { return _unaryChild; }
            set { _unaryChild = value; }
        }

        public string NodeType
        {
            get { return _nodeType; }
        }

        public bool ConstantData
        {
            get { return _constantData; }
            set { _constantData = value; }
        }

        public bool HasChildren
        {
            get { return (_leftChild != null || _rightChild != null); }
        }

        #endregion

        // Constructor
        public ATreeNode(string nodeType)
        {
            _nodeType = nodeType;
            _leftChild = _rightChild = _unaryChild = null;
        }
    }
}
