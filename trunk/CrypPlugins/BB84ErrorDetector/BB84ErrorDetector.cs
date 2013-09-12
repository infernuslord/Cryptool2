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

namespace Cryptool.Plugins.BB84ErrorDetector
{
    [Author("Benedict Beuscher", "benedict. beuscher@stud.uni-due.de", "Uni Duisburg-Essen", "http://www.uni-due.de/")]

    [PluginInfo("Cryptool.Plugins.BB84ErrorDetector.Properties.Resources", "res_ErrorDetectorCaption", "res_ErrorDetectorTooltip", "BB84ErrorDetector/userdoc.xml", new[] { "CrypWin/images/default.png" })]
   
    [ComponentCategory(ComponentCategory.Protocols)]
    public class BB84ErrorDetector : ICrypComponent
    {
        #region Private Variables

        private string firstKey;
        private string secondKey;
        private string detectionMessage;


        private BB84ErrorDetectorSettings mySettings = new BB84ErrorDetectorSettings();
       

        #endregion


        #region Data Properties

        [PropertyInfo(Direction.InputData, "res_FirstKeyCaption", "res_FirstKeyTooltip")]
        public string FirstKey
        {
            get
            {
                return this.firstKey;
            }
            set
            {
                if (!value.Equals(firstKey))
                { this.firstKey = value; }
            }
        }


        [PropertyInfo(Direction.InputData, "res_SecondKeyCaption", "res_SecondKeyTooltip")]
        public string SecondKey
        {
            get
            {
                return this.secondKey;
            }
            set
            {
                if (!value.Equals(secondKey))
                { this.secondKey = value; }
            }
        }

        [PropertyInfo(Direction.OutputData, "res_MessageCaption", "res_MessageTooltip")]
        public string DetectionMessage
        {
            get
            {
                return this.detectionMessage;
            }
            set
            {
                if (!value.Equals(detectionMessage))
                {
                    this.detectionMessage = value;
                }
            }            
        }

        #endregion

        #region IPlugin Members

       
        public ISettings Settings
        {
            get { return mySettings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            ProgressChanged(0, 1);

            setBounds();

            detectionMessage = generateDetectionMessage();

            OnPropertyChanged("DetectionMessage");

            ProgressChanged(1, 1);
        }

        private void setBounds()
        {
            if (mySettings.EndIndex >= firstKey.Length)
            {
                mySettings.EndIndex = firstKey.Length - 1;
            }
        }

        private string generateDetectionMessage()
        {
            string message = "";
            double errorRatio = calculateErrorRate();
            if (errorRatio > ((double)mySettings.ThresholdValue)/100)
            {

                message = String.Format(Properties.Resources.res_KeyUnsecure, Math.Round(errorRatio, 3) * 100);
            }
            else
            {
                message = String.Format(Properties.Resources.res_KeySecure, Math.Round(errorRatio, 3) * 100);
            }

            return message;
        }

        private double calculateErrorRate()
        {
            double count = 0;
            double errors = 0;

            
            for (int i = mySettings.StartIndex; i <= mySettings.EndIndex; i++)
            {
                if (firstKey.Length > i && secondKey.Length > i)
                {
                    if (!firstKey[i].Equals(secondKey[i]))
                    {
                        errors++;
                    }
                }
                count++;
            }

            double errorRate = errors/count;
            return errorRate;
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
            mySettings.ThresholdValue = 11;
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
