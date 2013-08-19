/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System;
using System.Windows.Threading;
using System.Threading;

namespace Cryptool.Plugins.BB84PhotonEncoder
{
    
    [Author("Benedict Beuscher", "benedict. beuscher@stud.uni-due.de", "Uni Duisburg-Essen", "http://www.uni-due.de/")]

    [PluginInfo("Cryptool.Plugins.BB84PhotonEncoder.Properties.Resources", "res_photonEncodingCaption", "res_photonEncodingTooltip", "BB84PhotonEncoder/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    
    [ComponentCategory(ComponentCategory.Protocols)]
    public class BB84PhotonEncoder : ICrypComponent
    {
        #region Private Variables

        public bool synchron;

        private readonly BB84PhotonEncoderSettings settings = new BB84PhotonEncoderSettings();
        private string inputKey;
        private string inputBases;
        private string photonOutput;
        private BB84PhotonEncoderPresentation myPresentation;
        private int duration;


        #endregion

        public BB84PhotonEncoder()
        {
            synchron = true;
            myPresentation = new BB84PhotonEncoderPresentation();
            myPresentation.UpdateProgess += new EventHandler(update_progress);
            Presentation = myPresentation;
           
        }

        private void update_progress(object sender, EventArgs e)
        {
            ProgressChanged(myPresentation.Progress, 3000);
        }

        #region Data Properties

        [PropertyInfo(Direction.InputData, "res_keyInputCaption", "res_keyInputTooltip")]
        public string InputKey
        {
            get
            {
                return this.inputKey;
            }
            set
            {
                string newInput = filterValidInput(value);
                this.inputKey = newInput;
            }
        }

        private string filterValidInput(string value)
        {
            string outputString = "";
            for (int i = 0; i < value.Length; i++) 
            {
                if (value[i].Equals('0') || value[i].Equals('1'))
                {
                    outputString += value[i];
                }
            }

            return outputString;
        }

        [PropertyInfo(Direction.InputData, "res_basesInputCaption", "res_basesInputTooltip")]
        public string InputBases
        {
            get
            {
                return this.inputBases;
            }
            set
            {
                this.inputBases = value;
            }
        }


        [PropertyInfo(Direction.OutputData, "res_photonOutputCaption", "res_photonOutputTooltip")]
        public string PhotonOutput
        {
            get
            {
                return this.photonOutput;
            }
            set
            {} //readonly
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get;
            private set;
        }

        public void PreExecution()
        {
            myPresentation.SpeedFactor = settings.SpeedSetting;
        }

        public void Execute()
        {
            ProgressChanged(0, 1);
           
            string tempOutput = "";
            char[] tempBases = inputBases.ToCharArray();
            char[] tempKey = inputKey.ToCharArray();

            for (int i = 0; i < inputKey.Length; i++)
            {
               

                if (tempBases[i].Equals('+')){
                    if (tempKey[i].Equals('0'))
                    {    
                            tempOutput += getPlusBasePhoton(settings.PlusZeroEncoding);
                    }
                    else if (tempKey[i].Equals('1'))
                    {
                        tempOutput += getPlusBasePhoton(settings.PlusOneEncoding);
                    }
                }
                else if (tempBases[i].Equals('x')){
                   if (tempKey[i].Equals('0')){
                       tempOutput += getExBasePhoton(settings.XZeroEncoding);
                    }
                    else if (tempKey[i].Equals('1')){
                        tempOutput += getExBasePhoton(settings.XOneEncoding);
                    }
                }
            }
            this.photonOutput = tempOutput;



            if (Presentation.IsVisible)
            {
               
                Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    myPresentation.StartPresentation(inputKey, photonOutput, inputBases); }, null);

                if (!synchron)
                {
                    while (!myPresentation.hasFinished)
                    {
                        ProgressChanged(myPresentation.animationRepeats, inputBases.Length);
                    }
                }
                
                
            }
            OnPropertyChanged("PhotonOutput");
            ProgressChanged(1, 1);
        }

        

        private string getExBasePhoton(int p)
        {
            if (p == 0)
                return "\\";
            else
                return "/";
        }

        private string getPlusBasePhoton(int p)
        {
            if (p == 0)
                return "|";
            else
                return "-";
        }

        public void PostExecution()
        {
            Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            { myPresentation.StopPresentation(); }, null);
        }

        public void Stop()
        {
            Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            { myPresentation.StopPresentation(); }, null);
        }

        public void Initialize()
        {
            settings.XZeroEncoding = 1;
            settings.XOneEncoding = 0;
            settings.PlusZeroEncoding = 0;
            settings.PlusOneEncoding = 1;
            settings.SpeedSetting = 1;
            
        }

        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
