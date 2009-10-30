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
using Cryptool;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;


namespace Cryptool.Plugins.Substring
{
    [Author("Dennis Nolte", "nolte@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "Substring", "Generating Substring", "Substring/DetailedDescription/Description.xaml", "Substring/icon.png")]
    
    class Substring : IThroughput
    {
        #region IPlugin Members

        //Variables initialized
        private SubstringSettings settings = new SubstringSettings();
        private String inputString = "";
        private int inputPos = 0;
        private int inputLength = 0;
        private String outputString = "";

        public event Cryptool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;

        public event Cryptool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event Cryptool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;

        public Cryptool.PluginBase.ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (SubstringSettings)value; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            
        }

        public void Execute()
        {
            //checking if input is set
            if (inputString != null)
            {
                ProgressChanged(0.5, 1.0);
                //checking if the chosen parameters for startposition and length are still in the bound of the given string
                if ((settings.IntegerStartValue <= inputString.Length) & ((settings.IntegerStartValue + settings.IntegerLengthValue) <= inputString.Length))
                {
                    //checking if the user set the length value to 0...if the length = 0 the complete string starting from the startposition will be send
                    if (settings.IntegerLengthValue != 0)
                    {
                        OutputString = inputString.Substring(settings.IntegerStartValue, settings.IntegerLengthValue);
                    }
                    else
                    {
                        OutputString = inputString.Substring(settings.IntegerStartValue);
                    }
                    ProgressChanged(1.0, 1.0);
                    return;
                }
                else
                {
                    GuiLogMessage("Your Startposition and/or Length for Substring are invalid", NotificationLevel.Error);
                }
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

        #endregion

        #region SubstringInOut
        /// <summary>
        /// The inputs are defined
        /// </summary>
        [PropertyInfo(Direction.InputData, "String Input", "Input your String here", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public String InputString
        {
            get
            {
                return inputString;
            }
            set
            {
                this.inputString = value;
                OnPropertyChanged("InputString");
            }
        }

        [PropertyInfo(Direction.InputData, "Position Input", "Input your Position here", "", DisplayLevel.Beginner)]
        public int InputPos
        {
            get
            {
                return inputPos;
            }
            set
            {
                this.inputPos = value;
                settings.IntegerStartValue = value;
                OnPropertyChanged("InputPosition");
            }
        }

        [PropertyInfo(Direction.InputData, "Length Input", "Input your Length here", "", DisplayLevel.Beginner)]
        public int InputLength
        {
            get
            {
                return inputLength;
            }
            set
            {
                this.inputLength = value;
                settings.IntegerLengthValue = value;
                OnPropertyChanged("InputLength");
            }
        }

        /// <summary>
        /// The output is defined
        /// </summary>
        [PropertyInfo(Direction.OutputData, "String Output", "Your Substring will be send here", "", DisplayLevel.Beginner)]
        public String OutputString
        {
            get
            {
                return outputString;
            }
            set
            {
                this.outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members



        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event PluginProgressChangedEventHandler OnPluginProcessChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string p,NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion

        
    }
}
