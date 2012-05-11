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
using System.Collections.Generic;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.Yao3
{

    [Author("Ondřej Skowronek", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]

    [PluginInfo("Yao 3", "Plugin for Yao´s Millionaire Problem", "Yao3/userdoc.xml", new[] { "Yao3/icon.png" })]

    [ComponentCategory(ComponentCategory.Protocols)]
    public class Yao3 : ICrypComponent
    {
        #region Private Variables


        #endregion

        #region Data Properties

        

        int count;


        [PropertyInfo(Direction.InputData, "p", "N/2 bit random prime")]
        public int p
        {
            get;
            set;
        }


        [PropertyInfo(Direction.InputData, "Z", "Z")]
        public List<int> Zs
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "x", "x")]
        public int x
        {
            get;
            set;
        }


        [PropertyInfo(Direction.InputData, "J", "J")]
        public int J
        {
            get;
            set;
        }


        
        [PropertyInfo(Direction.OutputData, "BIsRicher?", "Is Bob richer than Alice?")]
        public bool BisRicher
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
            count = 0;
        }


        public void Execute()
        {
           
            ProgressChanged(0, 1);            
                        
            if (Zs[J - 1] != x % p)
            {
                BisRicher = true;
            }
            else
            {
                BisRicher = false;
            }
         

            OnPropertyChanged("BisRicher");
  
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
