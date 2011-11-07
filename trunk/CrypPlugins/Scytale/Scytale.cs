/*                              
   Copyright 2009 Fabian Enkler, Arno Wacker (maintenance, updates), Uni Duisburg-Essen

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

using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Scytale
{
    [Author("Fabian Enkler, A. Wacker", "enkler@cryptool.org, wacker@cryptool.org", "Uni Duisburg-Essen", "http://www.vs.uni-duisburg-essen.de")]
    [PluginInfo("Cryptool.Scytale.Properties.Resources", false, "PluginCaption", "PluginTooltip", "Scytale/DetailedDescription/doc.xml", "Scytale/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Scytale : ICrypComponent
    {
        private readonly ScytaleSettings settings;
        private int CharsPerRow;

        public event PropertyChangedEventHandler PropertyChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured; 
#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public Scytale()
        {
            this.settings = new ScytaleSettings();
        }

        private string inputString = string.Empty;
        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", "", true, false, QuickWatchFormat.Text, null)]
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
        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", "", false, false, QuickWatchFormat.Text, null)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }



        [PropertyInfo(Direction.InputData, "StickSizeCaption", "StickSizeTooltip", "", false, false, QuickWatchFormat.Text, null)]
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
                Progress(i, inputString.Length - 2);
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
                Progress(i, totalChars - 1);
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

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
