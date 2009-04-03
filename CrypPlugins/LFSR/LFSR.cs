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

namespace Cryptool.LFSR
{
    [Author("Soeren Rinne", "soeren.rinne@cryptool.org", "Ruhr-Universitaet Bochum, Chair for Embedded Security (EmSec)", "http://www.crypto.ruhr-uni-bochum.de/")]
    [PluginInfo(false, "LFSR", "Linear Feedback Shift Register (simple version)", "LFSR/DetailedDescription/Description.xaml", "LFSR/Images/LFSR.png", "LFSR/Images/encrypt.png", "LFSR/Images/decrypt.png")]
    [EncryptionType(EncryptionType.SymmetricBlock)]
    public class LFSR : IEncryption
    {
        #region IPlugin Members

        private LFSRSettings settings;
        private String inputFeedback;
        private String inputSeed;
        private CryptoolStream inputClock;
        private CryptoolStream outputStream;
        private bool stop = false;
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();

        #endregion

        public LFSR()
        {
            this.settings = new LFSRSettings();
            //((LFSRSettings)(this.settings)).LogMessage += LFSR_LogMessage;
        }

        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (LFSRSettings)value; }
        }

        [PropertyInfo(Direction.Input, "Feedback", "Feedback function in binary presentation.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public String InputFeedback
        {
            get { return inputFeedback; }
            set
            {
                inputFeedback = value;
                OnPropertyChanged("InputFeedback");
            }
        }

        [PropertyInfo(Direction.Input, "Seed", "Seed of the LFSR in binary presentation.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public String InputSeed
        {
            get { return inputSeed; }
            set
            {
                inputSeed = value;
                OnPropertyChanged("InputSeed");
            }
        }

        [PropertyInfo(Direction.Input, "Clock", "Optional clock input. LFSR only advances if clock is true.", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public CryptoolStream InputClock
        {
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
            set
            {
                this.inputClock = value;
                if (value != null) listCryptoolStreamsOut.Add(value);
                OnPropertyChanged("InputClock");
            }
        }

        [PropertyInfo(Direction.Output, "Output stream", "LFSR Output Stream", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
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

        public void Dispose()
        {
            try
            {
                stop = false;
                outputStream = null;
                inputClock = null;

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

        private void checkForInputFeedback()
        {
            if ((inputFeedback == null || (inputFeedback != null && inputFeedback.Length == 0)))
            {
                //create some input
                String dummystring = "1";
//                this.inputFeedback = new String();
                inputFeedback = dummystring;
                // write a warning to the outside world
                GuiLogMessage("WARNING - No Feedback provided. Using dummy data. (" + dummystring + ")", NotificationLevel.Warning);
            }
        }

        private void checkForInputSeed()
        {
            if ((inputSeed == null || (inputSeed != null && inputSeed.Length == 0)))
            {
                //create some input
                String dummystring = "10100";
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
                GuiLogMessage("WARNING - No clock provided. Asuming always 1.", NotificationLevel.Warning);
            }
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
                checkForInputFeedback();
                checkForInputSeed();
                checkForInputClock();

                if (inputSeed == null || (inputSeed != null && inputSeed.Length == 0))
                {
                    GuiLogMessage("No Seed given. Aborting now.", NotificationLevel.Error);
                    return;
                }

                if (inputFeedback == null || (inputFeedback != null && inputFeedback.Length == 0))
                {
                    GuiLogMessage("No Feedback given. Aborting now.", NotificationLevel.Error);
                    return;
                }

                if (inputFeedback.Length != inputSeed.Length)
                {
                    // stop, because seed and feedback must have same length
                    GuiLogMessage("ERROR - Seed and feedback must have same length. Aborting now.", NotificationLevel.Error);
                    return;
                }

                int feedbackBits = inputFeedback.Length;
                int seedBits = inputSeed.Length;

                GuiLogMessage("inputFeedback length [bits]: " + feedbackBits.ToString(), NotificationLevel.Debug);
                GuiLogMessage("inputSeed length [bits]: " + seedBits.ToString(), NotificationLevel.Debug);
                
                String seedbuffer;
                String feedbackbuffer;
                Char outputbuffer;

                //read seed
                seedbuffer = inputSeed;
                
                // read feedback
                feedbackbuffer = inputFeedback;

                // convert feedback into char array
                char[] feedbackCharArray = feedbackbuffer.ToCharArray();

                // check if feedback is binary
                foreach (char character in feedbackCharArray)
                {
                    if (character != '0' && character != '1')
                    {
                        GuiLogMessage("ERROR 0 - Feedback has to be binary. Aborting now. Character is: " + character, NotificationLevel.Error);
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

                // open output stream
                outputStream = new CryptoolStream();
                listCryptoolStreamsOut.Add(outputStream);
                outputStream.OpenWrite(this.GetPluginInfoAttribute().Caption);

                // open clock

                
                // Here we go!
                GuiLogMessage("Action is: Now!", NotificationLevel.Debug);
                DateTime startTime = DateTime.Now;

                // compute LFSR
                if (true)
                {
                    GuiLogMessage("Starting computation", NotificationLevel.Info);
                    
                    int i = 0;
                    int defaultRounds = 4;
                    int actualRounds;

                    // check if Rounds are given
                    if (settings.Rounds == 0) actualRounds = defaultRounds; else actualRounds = settings.Rounds;

                    for (i = 0; i < actualRounds; i++)
                    {
                        //StatusChanged((int)LFSRImage.Encode);

                        // compute only if clock = 1
                        String clock = inputClock.ReadByte().ToString();
                        inputClock.Position = 0;
                        //GuiLogMessage("Clock is: " + clock, NotificationLevel.Info);

                        // check if clock is 1, which is 0x49 in ASCII
                        if (String.Equals(clock, "49"))
                        {
                            // write last bit to output buffer and stream
                            outputbuffer = seedCharArray[seedBits - 1];
                            outputStream.Write((Byte)outputbuffer);
                            // update output stream
                            OnPropertyChanged("OutputStream");

                            // shift seed array
                            char newBit = '0';

                            // compute new bit
                            bool firstDone = false;
                            for (int j = 0; j < seedBits; j++)
                            {
                                // check if feedback is 1
                                if (feedbackCharArray[j] == '1')
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

                            // shift seed array
                            for (int j = seedBits - 1; j > 0; j--)
                            {
                                seedCharArray[j] = seedCharArray[j - 1];
                            }
                            seedCharArray[0] = newBit;

                            //GuiLogMessage("New Bit: " + newBit.ToString(), NotificationLevel.Info);
                        }
                        else
                        {
                            StatusChanged((int)LFSRImage.Decode);
                            GuiLogMessage("LFSR Clock is 0", NotificationLevel.Info);
                            return;
                        }
                        
                    }
                }

                // stop counter
                DateTime stopTime = DateTime.Now;
                // compute overall time
                TimeSpan duration = stopTime - startTime;

                if (!stop)
                {
                    GuiLogMessage("Complete!", NotificationLevel.Debug);

                    GuiLogMessage("Time used: " + duration, NotificationLevel.Debug);
                    outputStream.Close();
                    OnPropertyChanged("OutputStream");
                }

                if (stop)
                {
                    outputStream.Close();
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
            StatusChanged((int)LFSRImage.Default);
            this.stop = true;
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
