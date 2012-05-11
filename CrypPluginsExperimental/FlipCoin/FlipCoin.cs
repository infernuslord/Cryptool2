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
using System;
using System.Numerics;
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.FlipCoin
{
   
    [Author("Ondřej Skowronek", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]
 
    [PluginInfo("Coin flip", "Plugin representing coin flipping", "FlipCoin/userdoc.xml", new[] { "FlipCoin/icon.png" })]

    [ComponentCategory(ComponentCategory.ToolsBoolean)]
    public class FlipCoin : ICrypComponent
    {
        #region Private Variables


        
        #endregion

        #region Data Properties



        [PropertyInfo(Direction.OutputData, "Coin flip bool", "Value of flipped coin")]
        public bool BoolValue
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


            BigInteger bigint = new BigInteger(2);
            bigint = BigIntegerHelper.RandomIntLimit(bigint);
            if (bigint == 0)
            {
                BoolValue = true;
            }
            else
            {
                BoolValue = false;
            }

        }

 
        public void Execute()
        {
            
            ProgressChanged(0, 1);            
            OnPropertyChanged("BoolValue");            
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
