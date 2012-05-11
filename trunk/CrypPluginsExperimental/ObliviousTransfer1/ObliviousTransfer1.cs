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

using System;
using System.Collections.Generic;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.ObliviousTransfer1
{
    [Author("Ondřej Skowronek", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]

    [PluginInfo("Oblivious Transfer 1", "Plugin for oblivious transfer protocol", "ObliviousTransfer1/userdoc.xml", new[] { "ObliviousTransfer1/icon.png" })]
    
    [ComponentCategory(ComponentCategory.Protocols)]
    public class ObliviousTransfer1 : ICrypComponent
    {
        #region Private Variables


        private readonly ObliviousTransfer1Settings settings = new ObliviousTransfer1Settings();

        #endregion

        #region Data Properties

        BigInteger k;


        [PropertyInfo(Direction.InputData, "x", "N bit generated random number")]
        public List<BigInteger> x
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
  
        [PropertyInfo(Direction.InputData, "e", "public RSA key")]
        public int e
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


        [PropertyInfo(Direction.OutputData, "v", "encrypted message")]
        public BigInteger v
        {
            get;
            set;
        }
        [PropertyInfo(Direction.OutputData, "k", "key for encryption")]
        public BigInteger K
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
        }


        public void Execute()
        {

            ProgressChanged(0, 1);

            if (b >= x.Count)
            {
                GuiLogMessage("Ammount of message parameter is lesser than 1", NotificationLevel.Error);
            }
            else
            {

                
                k = BigIntegerHelper.RandomIntLimit(settings.KLimit);
                if (k >= N)
                {
                    GuiLogMessage("Public key N is too small, RSA failed", NotificationLevel.Error);
                }
                else
                {
                    v = (x[b] + BigInteger.Pow(k, e)) % N;
                    OnPropertyChanged("v");
                    K = k;
                    OnPropertyChanged("K");
                    ProgressChanged(1, 1);
                }
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
