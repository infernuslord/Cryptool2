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
using System.Numerics;
using System.Collections.Generic;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.ObliviousTransfer3
{
    [Author("Ondřej Skowronek", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]

    [PluginInfo("Oblivious Transfer 3", "Plugin for oblivious transfer protocol", "ObliviousTransfer3/userdoc.xml", new[] { "ObliviousTransfer3/icon.png" })]

    [ComponentCategory(ComponentCategory.Protocols)]
    public class ObliviousTransfer3 : ICrypComponent
    {
        #region Private Variables

   

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "ecryptedMessages", "encrypted messages from Alice")]
        public List<BigInteger> cryptedMessages
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "b", "index of wanted message")]
        public int b
        {
            get;
            set;
        }


        [PropertyInfo(Direction.InputData, "k", "randomly generated key")]
        public BigInteger k
        {
            get;
            set;
        }


        [PropertyInfo(Direction.OutputData, "message", "wanted message")]
        public BigInteger message
        {
            get;
            set;
        }


        #endregion

        #region IPlugin Members

       
        public ISettings Settings
        {
            get { return null; }
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
            if (b >= cryptedMessages.Count)
            {
                GuiLogMessage("b is too big for ammount of message", NotificationLevel.Error);
            }
            else
            {
                message = cryptedMessages[b] - k;
                OnPropertyChanged("message");
                ProgressChanged(1, 1);
            }
            
            
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
