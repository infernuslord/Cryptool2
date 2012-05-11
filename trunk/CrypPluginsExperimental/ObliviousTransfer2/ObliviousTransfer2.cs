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

namespace Cryptool.Plugins.ObliviousTransfer2
{
    [Author("Ondřej Skowronek", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]

    [PluginInfo("Oblivious Transfer 2", "Plugin for oblivious transfer protocol", "ObliviousTransfer2/userdoc.xml", new[] { "ObliviousTransfer2/icon.png" })]
   
    [ComponentCategory(ComponentCategory.Protocols)]
    public class ObliviousTransfer2 : ICrypComponent
    {
        #region Private Variables


 

        #endregion


        List<BigInteger> messages;
        BigInteger k;
        int count;

        #region Data Properties



        [PropertyInfo(Direction.InputData, "message", "message that Alice want to send")]
        public BigInteger message
        {
            get;
            set;
        }
        [PropertyInfo(Direction.InputData, "Count", "ammount of messages")]
        public int Count
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "x", "random messagess")]
        public List<BigInteger> x
        {
            get;
            set;
        }
        [PropertyInfo(Direction.InputData, "v", "encrypted k")]
        public int v
        {
            get;
            set;
        }
        [PropertyInfo(Direction.InputData, "d", "private RSA key")]
        public int d
        {
            get;
            set;
        }
        [PropertyInfo(Direction.InputData, "N", "public RSA key")]
        public int N
        {
            get;
            set;
        }


        [PropertyInfo(Direction.OutputData, "ecryptedMessages", "encrypted messages, encryptable with right k")]
        public  List<BigInteger> cryptedMessages
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
            messages = new List<BigInteger>();
            count = 0;
            k = new BigInteger();
            cryptedMessages = new List<BigInteger>();
        }


        public void Execute()
        {
            
            ProgressChanged(0, 1);


            
            messages.Add(message);
            count++;

            if (count == Count)
            {
                for (int i = 0; i < messages.Count; i++)
                {
                    if ((v - x[i]) >= N)
                    {
                        GuiLogMessage("Public key N is too small, RSA failed", NotificationLevel.Error);
                    }
                    k = BigInteger.Pow(v - x[i], d) % N;
                    cryptedMessages.Add(messages[i] + k);                    
                }
                OnPropertyChanged("cryptedMessages");


                
            }

            
            
            ProgressChanged(1, 1);
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
