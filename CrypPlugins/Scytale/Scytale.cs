using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Scytale
{
    [Author("Fabian Enkler", "", "", "")]
    [PluginInfo(false, "Scytale", "Scytale -- classic transposition cipher", "Scytale/DetailedDescription/Description.xaml", "Scytale/icon.png")]
    [EncryptionType(EncryptionType.Classic)]
    public class Scytale : IEncryption
    {
        private readonly ScytaleSettings settings;
        private int CharsPerRow;

        public event PropertyChangedEventHandler PropertyChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured; 
#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

        public Scytale()
        {
            this.settings = new ScytaleSettings();
        }

        private string inputString = string.Empty;
        [PropertyInfo(Direction.InputData, "Text input", "Input a string to be processed by the Scytale cipher", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string InputString
        {
            get { return this.inputString; }
            set
            {
                if (value != inputString)
                {
                    this.inputString = value;
                    OnPropertyChanged("InputString");
                }
            }
        }

        private string outputString = string.Empty;
        [PropertyInfo(Direction.OutputData, "Text output", "The string after processing with the Scytale cipher", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }



        [PropertyInfo(Direction.InputData, "Stick size", "Same setting as Stick size value in the algorithms settings but as dynamic input.", "", false, false, DisplayLevel.Expert, QuickWatchFormat.Text, null)]
        public int StickSize
        {
            get { return settings.StickSize; }
            set
            {
                if (value != settings.StickSize)
                {
                    settings.StickSize = value;
                    OnPropertyChanged("StickSize");
                }
            }
        }



        public void PreExecution()
        {
        }

        public void Execute()
        {
            if (!string.IsNullOrEmpty(inputString))
            {
                if (settings.StickSize < 1)
                {
                    EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Got an invalid stick size of " + settings.StickSize + "! Reverting to 1.", this, NotificationLevel.Warning));
                    settings.StickSize = 1;
                }

                CharsPerRow = inputString.Length / settings.StickSize + 1;
                outputString = string.Empty;
                switch (settings.Action)
                {
                    case 0:
                        EncryptIt();
                        break;
                    case 1:
                        DecryptIt();
                        break;
                    default:
                        break;

                }
                OnPropertyChanged("OutputString");
            }
        }

        private void DecryptIt()
        {
            int Position = 0;
            for (int i = 0; i < inputString.Length - 1; i++)
            {
                outputString += inputString[Position];
                Position += settings.StickSize;
                if (Position >= inputString.Length)
                    Position -= inputString.Length - 1;

                //show the progress
                EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(i, inputString.Length - 2));
            }
            outputString = outputString.Replace('_', ' ').Trim();
        }

        private void EncryptIt()
        {
            inputString = inputString.Replace(' ', '_');
            int Position = 0;
            int totalChars = settings.StickSize * CharsPerRow;
            for (int i = 0; i < totalChars; i++)
            {
                if (Position > inputString.Length - 1)
                    outputString += "_";
                else
                {
                    outputString += inputString[Position];
                }
                Position += CharsPerRow;
                if (Position >= settings.StickSize * CharsPerRow)
                    Position -= settings.StickSize * CharsPerRow - 1;

                //show the progress
                EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(i, totalChars - 1));
            }
        }

        public void PostExecution()
        {
        }

        public void Pause()
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

        public ISettings Settings
        {
            get { return this.settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
