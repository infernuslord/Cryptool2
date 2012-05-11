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
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.ZeroKnowledgeChecked
{
    [Author("Ondřej Skowronek", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]
    [PluginInfo("Zero Knowledge Checked", "Plugin for Zero Knowledge protocol", "ZeroKnowledgeChecked/userdoc.xml", new[] { "ZeroKnowledgeChecked/icon.png" })]    
    [ComponentCategory(ComponentCategory.Protocols)]
    public class ZeroKnowledgeChecked : ICrypComponent
    {
        #region Private Variables
        
        private readonly ZeroKnowledgeCheckedSettings settings = new ZeroKnowledgeCheckedSettings();

        #endregion

        #region Data Properties


        [PropertyInfo(Direction.InputData, "Input", "Input")]
        public BigInteger Input
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "AmmountOfOptions", "Ammmount of options")]
        public BigInteger AmmountOfOptions
        {
            get;
            set;
        }


        [PropertyInfo(Direction.OutputData, "Output", "Output")]
        public BigInteger Output
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

            if (settings.Secret)
            {
                Output = Input;
            }
            else
            {
                Output = BigIntegerHelper.RandomIntLimit(AmmountOfOptions);
            }

            OnPropertyChanged("Output");
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
