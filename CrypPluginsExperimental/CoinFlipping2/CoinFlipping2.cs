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

namespace Cryptool.Plugins.CoinFlipping2
{
    [Author("Ondřej Skowronek", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]

    [PluginInfo("Coin Flipping 2", "Plugin for Coin Flipping protocol", "CoinFlipping2/userdoc.xml", new[] { "CoinFlipping2/icon.png" })]
   
    [ComponentCategory(ComponentCategory.Protocols)]
    public class CoinFlipping2 : ICrypComponent
    {
        #region Private Variables

       
        private readonly CoinFlipping2Settings settings = new CoinFlipping2Settings();

        #endregion

        #region Data Properties

        


        [PropertyInfo(Direction.InputData, "Flipped value", "Value of flipped coin", true)]
        public bool CoinFlipA
        {
            get;
            set;
        }


        [PropertyInfo(Direction.InputData, "Tip from B", "Value of tip from sender B", true)]
        public bool CoinFlipB
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Key", "Key for hashfunction")]
        public string InputKey
        {
            get;
            set;
        }


     
        [PropertyInfo(Direction.OutputData, "Success of tip", "Value of sucessfulness of tipping from B")]
        public bool Success
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "Flipped coin value", "Value of flipped coin, sended to B, it can be changed from real one by dishonest A")]
        public string CoinResult
        {
            get;
            set;
        }


        [PropertyInfo(Direction.OutputData, "Key", "Key of hash function sended to B")]
        public string OutputKey
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

            if (settings.SettingsHonest == 1)
            {
                if (CoinFlipA == CoinFlipB)
                {
                    Success = true;
                }
                else
                {
                    Success = false;
                }
                if (CoinFlipA)
                {
                    CoinResult = "1";
                }
                else
                {
                    CoinResult = "0";
                }
      

            }
            else
            {
                if (CoinFlipB)
                {
                    CoinResult = "0";
                }
                else
                {
                    CoinResult = "1";
                }                
                
                Success = false;
            }
            OutputKey = InputKey;

            OnPropertyChanged("OutputKey");
            OnPropertyChanged("CoinResult");
            OnPropertyChanged("Success");
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
