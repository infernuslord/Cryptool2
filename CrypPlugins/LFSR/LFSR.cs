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
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
// for QuickwatchPresentation
using System.Windows.Threading;
using System.Threading;
using System.Windows.Automation.Peers;
// /for QuickwatchPresentation

namespace Cryptool.LFSR
{
    [Author("Soeren Rinne", "soeren.rinne@cryptool.org", "Ruhr-Universitaet Bochum, Chair for Embedded Security (EmSec)", "http://www.crypto.ruhr-uni-bochum.de/")]
    [PluginInfo(false, "LFSR", "Linear Feedback Shift Register (simple version)", "LFSR/DetailedDescription/Description.xaml", "LFSR/Images/LFSR.png", "LFSR/Images/encrypt.png", "LFSR/Images/decrypt.png")]
    [EncryptionType(EncryptionType.SymmetricBlock)]
    public class LFSR : IThroughput
    {
        #region IPlugin Members

        private LFSRSettings settings;
        private String inputTapSequence;
        private String inputSeed;
        private CryptoolStream inputClock;
        private CryptoolStream outputStream;
        private bool outputBool;
        private bool inputClockBool;
        private bool stop = false;
        private int outputInt;
        //private bool getNewSeed = true;
        private LFSRPresentation lFSRPresentation;
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();

        #endregion

        public LFSR()
        {
            this.settings = new LFSRSettings();
            //((LFSRSettings)(this.settings)).LogMessage += LFSR_LogMessage;

            lFSRPresentation = new LFSRPresentation();
            Presentation = lFSRPresentation;
            //lFSRPresentation.textBox0.TextChanged += textBox0_TextChanged;
        }

        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (LFSRSettings)value; }
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
            }
        }

        [PropertyInfo(Direction.Input, "Seed", "Seed of the LFSR in binary presentation.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public String InputSeed
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return inputSeed; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                inputSeed = value;
                OnPropertyChanged("InputSeed");
            }
        }

        [PropertyInfo(Direction.Input, "Clock", "Optional clock input. LFSR only advances if clock is 1.", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
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
        }

        [PropertyInfo(Direction.Input, "Clock", "Optional clock input. LFSR only advances if clock is true.", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public Boolean InputClockBool
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return inputClockBool; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                inputClockBool = value;
                OnPropertyChanged("InputClockBool");
            }
        }

        [PropertyInfo(Direction.Output, "Output stream", "LFSR Output Stream", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public CryptoolStream OutputStream
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
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
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                outputStream = value;
                if (value != null) listCryptoolStreamsOut.Add(value);
                OnPropertyChanged("OutputStream");
            }
        }

        [PropertyInfo(Direction.Output, "Boolean Output", "Boolean Output.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public bool OutputBool
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return outputBool; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                outputBool = (bool)value;
                OnPropertyChanged("OutputBool");
            }
        }

        [PropertyInfo(Direction.Output, "Int Output", "Int Output.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public int OutputInt
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return outputInt; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                outputInt = (int)value;
                OnPropertyChanged("OutputInt");
            }
        }

        public void Dispose()
        {
            try
            {
                stop = false;
                outputStream = null;
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

        private void checkForInputTapSequence()
        {
            if ((inputTapSequence == null || (inputTapSequence != null && inputTapSequence.Length == 0)))
            {
                //create some input
                String dummystring = "1011";
//                this.inputTapSequence = new String();
                inputTapSequence = dummystring;
                // write a warning to the outside world
                GuiLogMessage("WARNING - No TapSequence provided. Using dummy data. (" + dummystring + ")", NotificationLevel.Warning);
            }
        }

        private void checkForInputSeed()
        {
            if ((inputSeed == null || (inputSeed != null && inputSeed.Length == 0)))
            {
                //create some input
                String dummystring = "1010";
                //this.inputSeed = new CryptoolStream();
                inputSeed = dummystring;
                // write a warning to the outside world
                GuiLogMessage("WARNING - No Seed provided. Using dummy data. (" + dummystring + ")", NotificationLevel.Warning);
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
                GuiLogMessage("FYI - No clock provided. Asuming always 1.", NotificationLevel.Info);
            }
        }

        private bool checkForClockEvent()
        {
            // checks if current event is a clock event
            //PropertyChangingEventArgs.??
            return true;
        }

        public void Execute()
        {
            processLFSR();
        }

        private void processLFSR()
        {
            // process LFSR
            
            try
            {
                //if (!checkForClockEvent()) return;
                checkForInputTapSequence();
                checkForInputSeed();

                if (inputSeed == null || (inputSeed != null && inputSeed.Length == 0))
                {
                    GuiLogMessage("No Seed given. Aborting now.", NotificationLevel.Error);
                    return;
                }

                if (inputTapSequence == null || (inputTapSequence != null && inputTapSequence.Length == 0))
                {
                    GuiLogMessage("No TapSequence given. Aborting now.", NotificationLevel.Error);
                    return;
                }

                if (inputTapSequence.Length != inputSeed.Length)
                {
                    // stop, because seed and tapSequence must have same length
                    GuiLogMessage("ERROR - Seed and tapSequence must have same length. Aborting now.", NotificationLevel.Error);
                    return;
                }

                int tapSequenceBits = inputTapSequence.Length;
                int seedBits = inputSeed.Length;

                GuiLogMessage("inputTapSequence length [bits]: " + tapSequenceBits.ToString(), NotificationLevel.Debug);
                GuiLogMessage("inputSeed length [bits]: " + seedBits.ToString(), NotificationLevel.Debug);
                
                String seedbuffer = "0";
                String tapSequencebuffer;
                Char outputbuffer;

                //read seed one time until stop of chain
                /*if (getNewSeed)
                {
                    seedbuffer = inputSeed;
                    getNewSeed = false;
                }*/
                seedbuffer = inputSeed;
                
                // read tapSequence
                tapSequencebuffer = inputTapSequence;

                // convert tapSequence into char array
                char[] tapSequenceCharArray = tapSequencebuffer.ToCharArray();

                //check if last tap is 1, otherwise stop
                if (tapSequenceCharArray[tapSequenceCharArray.Length - 1] == '0')
                {
                    GuiLogMessage("ERROR - Last tap of tapSequence must 1. Aborting now.", NotificationLevel.Error);
                    return;
                }

                // check if tapSequence is binary
                foreach (char character in tapSequenceCharArray)
                {
                    if (character != '0' && character != '1')
                    {
                        GuiLogMessage("ERROR 0 - TapSequence has to be binary. Aborting now. Character is: " + character, NotificationLevel.Error);
                        return;
                    }
                }

                // convert seed into char array
                char[] seedCharArray = seedbuffer.ToCharArray();

                // check if seed is binary
                foreach (char character in seedCharArray)
                {
                    if (character != '0' && character != '1')
                    {
                        GuiLogMessage("ERROR 0 - Seed has to be binary. Aborting now. Character is: " + character, NotificationLevel.Error);
                        return;
                    }
                }

                lFSRPresentation.DrawLFSR(seedCharArray, tapSequenceCharArray);

                // open output stream
                outputStream = new CryptoolStream();
                listCryptoolStreamsOut.Add(outputStream);
                outputStream.OpenWrite(this.GetPluginInfoAttribute().Caption);

                

                // check which clock to use
                Boolean myClock = true;

                if (settings.UseBoolClock)
                {
                    myClock = inputClockBool;
                }
                else if (!settings.UseBoolClock)
                {
                    // read stream clocks
                    checkForInputClock();
                    String stringClock = inputClock.ReadByte().ToString();
                    inputClock.Position = 0;
                    if (String.Equals(stringClock, "49")) myClock = true; else myClock = false;
                }

                // check if Rounds are given
                int defaultRounds = 4;
                int actualRounds;

                // check if Rounds in settings are given
                if (settings.Rounds == 0) actualRounds = defaultRounds; else actualRounds = settings.Rounds;
                
                // Here we go!
                //GuiLogMessage("Action is: Now!", NotificationLevel.Debug);
                DateTime startTime = DateTime.Now;

                //////////////////////////////////////////////////////
                // compute LFSR //////////////////////////////////////
                //////////////////////////////////////////////////////
                GuiLogMessage("Starting computation", NotificationLevel.Info);
                
                int i = 0;
                
                for (i = 0; i < actualRounds; i++)
                {
                    // compute only if clock = 1 or true
                    if (myClock)
                    {
                        StatusChanged((int)LFSRImage.Encode);

                        // make int output
                        if (seedCharArray[seedBits - 1] == '0') outputInt = 0;
                        else outputInt = 1;

                        // make bool output
                        if (seedCharArray[seedBits - 1] == '0') outputBool = false;
                        else outputBool = true;
                        GuiLogMessage("OutputBool is: " + outputBool.ToString(), NotificationLevel.Info);

                        // write last bit to output buffer, stream and bool
                        outputbuffer = seedCharArray[seedBits - 1];
                        outputStream.Write((Byte)outputbuffer);

                        // update outputs
                        OnPropertyChanged("OutputBool");
                        OnPropertyChanged("OutputInt");
                        OnPropertyChanged("OutputStream");

                        // shift seed array
                        char newBit = '0';

                        // compute new bit
                        bool firstDone = false;
                        for (int j = 0; j < seedBits; j++)
                        {
                            // check if tapSequence is 1
                            if (tapSequenceCharArray[j] == '1')
                            {
                                // if it is the first one, just take it
                                if (!firstDone)
                                {
                                    newBit = seedCharArray[j];
                                    firstDone = true;
                                }
                                // do an XOR with the first one
                                else
                                {
                                    newBit = (newBit ^ seedCharArray[j]).ToString()[0];
                                }
                            }
                        }
                        // keep output bit for presentation
                        char outputBit = seedCharArray[seedBits - 1];

                        // shift seed array
                        for (int j = seedBits - 1; j > 0; j--)
                        {
                            seedCharArray[j] = seedCharArray[j - 1];
                        }
                        seedCharArray[0] = newBit;

                        //update quickwatch presentation
                        lFSRPresentation.FillBoxes(seedCharArray, tapSequenceCharArray, outputBit);

                        // write current "seed" back to seedbuffer
                        //seedbuffer = seedCharArray.ToString();

                        //GuiLogMessage("New Bit: " + newBit.ToString(), NotificationLevel.Info);
                    }
                    else
                    {
                        StatusChanged((int)LFSRImage.Decode);
                        //GuiLogMessage("LFSR Clock is 0, no computation.", NotificationLevel.Info);
                        //return;
                    }
                    
                }

                // stop counter
                DateTime stopTime = DateTime.Now;
                // compute overall time
                TimeSpan duration = stopTime - startTime;

                if (!stop)
                {
                    GuiLogMessage("Complete!", NotificationLevel.Info);

                    GuiLogMessage("Time used: " + duration, NotificationLevel.Info);
                    outputStream.Close();
                    if (!settings.UseBoolClock) inputClock.Close();
                    OnPropertyChanged("OutputStream");
                }

                if (stop)
                {
                    outputStream.Close();
                    if (!settings.UseBoolClock) inputClock.Close();
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
            StatusChanged((int)LFSRImage.Default);
            lFSRPresentation.DeleteAll(100);
            //getNewSeed = true;
            this.stop = true;
        }

        public UserControl Presentation { get; private set; }

        public UserControl QuickWatchPresentation
        {
            get { return Presentation; }
        }

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

    enum LFSRImage
    {
        Default,
        Encode,
        Decode
    }
}
