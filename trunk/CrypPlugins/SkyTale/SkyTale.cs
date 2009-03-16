using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.SkyTale
{
    [Author("Fabian Enkler", "", "", "")]
    [PluginInfo(false, "SkyTale", "This the classical SkyTale cipher.", "", "SkyTale/icon.png")]
    [EncryptionType(EncryptionType.Classic)]
    class SkyTale : IEncryption
    {
        private readonly SkyTaleSettings settings;
        private int CharsPerRow;

        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public SkyTale()
        {
            this.settings = new SkyTaleSettings();
        }

        private string inputString = string.Empty;
        [PropertyInfo(Direction.Input, "Text input", "Input a string to be processed by the SkyTale cipher", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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
        [PropertyInfo(Direction.Output, "Text output", "The string after processing with the SkyTale cipher", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            if (!string.IsNullOrEmpty(inputString))
            {
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
            }
            outputString = outputString.Replace('_', ' ').Trim();
        }

        private void EncryptIt()
        {
            inputString = inputString.Replace(' ', '_');
            int Position = 0;
            for (int i = 0; i < settings.StickSize * CharsPerRow; i++)
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
