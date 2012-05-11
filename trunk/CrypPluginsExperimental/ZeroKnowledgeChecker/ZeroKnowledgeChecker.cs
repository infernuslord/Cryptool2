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
using System;
using System.Numerics;
using Cryptool.PluginBase.Miscellaneous;


namespace Cryptool.Plugins.ZeroKnowledgeChecker
{
   
    [Author("Ondřej Skowronek", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]

    [PluginInfo("Zero Knowledge Checker", "Plugin for Zero Knowledge protocol", "ZeroKnowledgeChecker/userdoc.xml", new[] { "ZeroKnowledgeChecker/icon.png" })]
    
    [ComponentCategory(ComponentCategory.Protocols)]
    public class ZeroKnowledgeChecker : ICrypComponent
    {
        #region Private Variables


        private readonly ZeroKnowledgeCheckerSettings settings = new ZeroKnowledgeCheckerSettings();

        private int numberOfAttempt = 0;

        private BigInteger attempt;


      


        #endregion

        #region Data Properties

        
        [PropertyInfo(Direction.InputData, "Input", "Checked value")]
        public BigInteger Input
        {
            get;
            set;
        }


        [PropertyInfo(Direction.OutputData, "AmmountOfOptions", "Ammount of options")]
        public int AmmountOfOptions
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputRandom", "Output of random value")]
        public BigInteger OutputRandom
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "Success", "Does Alice know secret?")]
        public bool Success
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "RateOfSuccess", "RateOfSuccessTooltip")]
        public int RateOfSuccess
        {
            get;
            set;
        }


        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

       
        public UserControl Presentation
        {
            get { return null; }
        }

       
        public void PreExecution()
        {
            numberOfAttempt = 0;
            AmmountOfOptions = settings.AmmountOfOptions;            
            Success = true;
        }

        
        public void Execute()
        {
            ProgressChanged(0, 1);

            
            
            if (numberOfAttempt != 0)
            {
                if (attempt != Input)
                {
                    Success = false;
                }

            }
            

            BigInteger bigint = new BigInteger(settings.AmmountOfOptions);

            attempt = BigIntegerHelper.RandomIntLimit(bigint);
            OutputRandom = attempt;
            
            if (numberOfAttempt < settings.AmmountOfAttempts)
            {
                OnPropertyChanged("OutputRandom");
                OnPropertyChanged("AmmountOfOptions");
            }
            else
            {
                
                double rate = Math.Pow(1 / (double) settings.AmmountOfOptions, settings.AmmountOfAttempts);
                RateOfSuccess = (int) Math.Ceiling(rate * 100);
                OnPropertyChanged("Success");
                OnPropertyChanged("RateOfSuccess");
            }

            numberOfAttempt++;

            ProgressChanged(numberOfAttempt, 10);
        }
        
        public void PostExecution()
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
