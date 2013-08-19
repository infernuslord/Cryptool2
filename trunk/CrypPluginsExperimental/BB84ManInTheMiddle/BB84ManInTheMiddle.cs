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
using System.Threading;
using BB84ManInTheMiddle;
using System.Windows.Threading;
using System.Text;
using System.Security.Cryptography;

namespace Cryptool.Plugins.BB84ManInTheMiddle
{
    [Author("Benedict Beuscher", "benedict.beuscher@stud.uni-due.de", "Uni Duisburg-Essen", "http://www.uni-due.de/")]


    [PluginInfo("Cryptool.Plugins.BB84ManInTheMiddle.Properties.Resources","res_MITMCaption", "res_MITMTooltip", "BB84ManInTheMiddle/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    
    [ComponentCategory(ComponentCategory.Protocols)]
    public class BB84ManInTheMiddle : ICrypComponent
    {
        #region Private Variables

        public bool synchron;

        private string inputPhotons;
        private string inputBases;
        private string outputPhotons;
        private string outputKey;
        private BB84ManInTheMiddlePresentation myPresentation;
        private RNGCryptoServiceProvider sRandom;
        

        private readonly BB84ManInTheMiddleSettings settings = new BB84ManInTheMiddleSettings();

        #endregion

        public BB84ManInTheMiddle()
        {
            synchron = true;

            myPresentation = new BB84ManInTheMiddlePresentation();
            myPresentation.UpdateProgress += new EventHandler(update_progress);
            Presentation = myPresentation;
        }

        private void update_progress(object sender, EventArgs e)
        {
            ProgressChanged(myPresentation.Progress, 3000);
        }


        #region Data Properties

        [PropertyInfo(Direction.InputData, "res_PhotonInputCaption", "res_PhotonInputTooltip",true)]
        public string InputPhotons
        {
            get
            {
                return this.inputPhotons;
            }
            set
            {
                if (!value.Equals(inputPhotons)){
                    inputPhotons = value;
                }
            }
        }

        [PropertyInfo(Direction.InputData, "res_BasesInputCaption", "res_BasesInputTooltip",true)]
        public string InputBases
        {
            get
            {
                return this.inputBases;
            }

            set
            {
                if (!value.Equals(inputBases))
                {
                    inputBases = value;
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "res_KeyOutputCaption", "res_KeyOutputTooltip")]
        public string OutputKey
        {
            get
            {
                return this.outputKey;
            }
            set
            {
                if (!value.Equals(outputKey))
                {
                    outputKey = value;
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "res_PhotonOutputCaption", "res_PhotonOutputTooltip")]
        public string OutputPhotons
        {
            get
            {
                return this.outputPhotons;
            }
            set
            {
                if (!value.Equals(outputPhotons))
                {
                    outputPhotons = value;
                }
            }
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

            if (settings.IsListening == 0)
            {
                decodeIncomingPhotons();
                forwardListenedPhotons();
                outputPhotons += "L";
            }
            else
            {
                forwardReceivedPhotons();
                displaySleepMessage();
                outputPhotons += "S";
            }

            

            OnPropertyChanged("OutputPhotons");
            OnPropertyChanged("OutputKey");


            showPresentationIfVisible();

            ProgressChanged(1, 1);
        }

        private void showPresentationIfVisible()
        {
           
            if (Presentation.IsVisible)
            {

                if (synchron)
                {
                    inputPhotons = "W" + inputPhotons;
                    inputBases = "W" + inputBases;
                    outputPhotons = "W" + outputPhotons;
                }

                if (settings.IsListening == 0)
                {
                        Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate{
                            myPresentation.StartPresentation(inputPhotons, inputBases, outputPhotons, true);}, null);

                        if (!synchron)
                        {
                            while (!myPresentation.hasFinished)
                            {
                                ProgressChanged(myPresentation.animationRepeats, inputBases.Length);
                            }
                        }
            }
                else
                {
                    Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        myPresentation.StartPresentation(inputPhotons, inputBases, outputPhotons, false);
                    }, null);

                    if (!synchron)
                    {
                        while (!myPresentation.hasFinished)
                        {
                            ProgressChanged(myPresentation.animationRepeats, inputBases.Length);
                        }
                    }
                }

                
               }
            
        }

        

        

        private void decodeIncomingPhotons()
        {
            StringBuilder listenedKey = new StringBuilder();
            

            for (int i = 0; i < inputPhotons.Length; i++)
            {
               listenedKey.Append(decodePhoton(inputPhotons[i], inputBases[i]));
            }

            outputKey = listenedKey.ToString();
            
        }

        private string decodePhoton(char photon, char pbase)
        {
            string returnBit = "";

            if (pbase.Equals('+'))
            { 
                if (photon.Equals('|'))
                {
                    returnBit = settings.PlusVerticallyDecoding;
                }
                else if (photon.Equals('-'))
                {
                    returnBit = settings.PlusHorizontallyDecoding;
                }
                else 
                {
                    returnBit = getRandomBit();
                }
            }
            else if (pbase.Equals('x'))
            {
                if (photon.Equals('/'))
                {
                    returnBit = settings.XTopRightDiagonallyDecoding;
                }
                else if (photon.Equals('\\'))
                {
                    returnBit = settings.XTopLeftDiagonallyDecoding;
                }
                else 
                {
                    returnBit = getRandomBit();
                }
            }
            return returnBit;
        }

        private string getRandomBit()
        {
            sRandom = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[4];
            sRandom.GetBytes(buffer);
            int result = BitConverter.ToInt32(buffer, 0);
            string returnString = ""+new Random(result).Next(2);
            return returnString;
        }

        private void forwardListenedPhotons()
        {
            String photonsToSend = "";
            for (int i = 0; i < outputKey.Length; i++)
            {
                photonsToSend += getPhotonFromBit(outputKey[i], inputBases[i]);
            }

            outputPhotons = photonsToSend;
            
        }

        private string getPhotonFromBit(char bit, char pbase)
        {
            String returnPhoton = "";

            if (pbase.Equals('+'))
            {
                
                if (settings.PlusHorizontallyDecoding[0].Equals(bit))
                {
                   
                    returnPhoton = "-";
                }
                else if (settings.PlusVerticallyDecoding[0].Equals(bit))
                {
                   
                    returnPhoton = "|";
                }
            }
            else if (pbase.Equals('x'))
            {

                if (settings.XTopLeftDiagonallyDecoding[0].Equals(bit))
                {
                    
                    returnPhoton = "\\";
                }
                else if (settings.XTopRightDiagonallyDecoding[0].Equals(bit))
                {
                   
                    returnPhoton = "/";
                }
            }
            return returnPhoton;
            
        }

        private void forwardReceivedPhotons()
        {
            this.OutputPhotons = this.inputPhotons;
            
        }

        private void displaySleepMessage()
        {
            outputKey = "Man in the middle is sleeping!";
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
            settings.PlusVerticallyDecoding = "0";
            settings.XTopRightDiagonallyDecoding = "0";
            settings.XTopLeftDiagonallyDecoding = "1";
            settings.PlusHorizontallyDecoding = "1";
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
