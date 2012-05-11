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
using System.Numerics;
using System.Collections.Generic;

using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;


namespace Cryptool.Plugins.RandomMessageGenerator
{

    [Author("Ondřej Skowronek", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]

    [PluginInfo("Random message generator", "Generate random messages", "RandomMessageGenerator/userdoc.xml", new[] { "RandomMessageGenerator/icon.png" })]
   
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class RandomMessageGenerator : ICrypComponent
    {
        #region Private Variables

        BigInteger maximum;

       
        private readonly RandomMessageGeneratorSettings settings = new RandomMessageGeneratorSettings();

        #endregion

        #region Data Properties

        
        [PropertyInfo(Direction.InputData, "Input name", "Input tooltip description")]
        public int AmmountOfMessage
        {
            get;
            set;
        }

       
        [PropertyInfo(Direction.OutputData, "Output name", "Output tooltip description")]
        public List<BigInteger> Messages
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
            Messages = new List<BigInteger>();
        }

        public void Execute()
        {            
            ProgressChanged(0, 1);

            Messages = new List<BigInteger>();
            maximum = new BigInteger(settings.MessageLimit);
            if (AmmountOfMessage < 1)
            {
                GuiLogMessage("Ammount of message parameter is lesser than 1", NotificationLevel.Error);
            }
            else
            {
                for (int i = 0; i < AmmountOfMessage; i++)
                {
                    Messages.Add(BigIntegerHelper.RandomIntLimit(maximum));
                }
                OnPropertyChanged("Messages");
                ProgressChanged(1, 1);
            }
            
            
        }

        public void PostExecution()
        {
            Dispose();
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
