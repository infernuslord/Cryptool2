﻿/*
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

namespace Cryptool.Plugins.DiningFinal
{
    [Author("Ondřej Skowronek", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]

    [PluginInfo("Dining Final", "Count the final result from dining cryptographers protocol", "DiningFinal/userdoc.xml", new[] { "DiningFinal/icon.png" })]
    
    [ComponentCategory(ComponentCategory.Protocols)]
    public class DiningFinal : ICrypComponent
    {
        #region Private Variables



        #endregion


        
        bool result;

        #region Data Properties


        [PropertyInfo(Direction.InputData, "Cryptographer", "Values from cryptographs")]
        public bool Input
        {
            get;
            set;
        }


        [PropertyInfo(Direction.OutputData, "Result", "Has some cryptograph paid?")]
        public bool Result
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
            result = false;
        }


        public void Execute()
        {
            
            ProgressChanged(0, 1);
            result = result ^ Input;
            Result = result;
            OnPropertyChanged("Result");
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
