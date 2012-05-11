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

namespace Cryptool.Plugins.Yao2
{

    [Author("Ondřej Skowronek", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]

    [PluginInfo("Yao 2", "Plugin for Yao´s Millionaire Problem", "Yao2/userdoc.xml", new[] { "Yao2/icon.png" })]
 
    [ComponentCategory(ComponentCategory.Protocols)]
    public class Yao2 : ICrypComponent
    {
        #region Private Variables

      

        int z;

        #endregion

        #region Data Properties


        List<int> Ys = new List<int>();

        int count;

        [PropertyInfo(Direction.InputData, "Y", "Y")]
        public int Y
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Max money", "Maximal ammount of money")]
        public int maxMoney
        {
            get;
            set;
        }


        [PropertyInfo(Direction.InputData, "p", "p")]
        public int p
        {
            get;
            set;
        }
        [PropertyInfo(Direction.InputData, "I", "Alice´s ammount of money")]
        public int I
        {
            get;
            set;
        }


        [PropertyInfo(Direction.OutputData, "Z", "Z")]
        public List<int> Zs
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
            Ys = new List<int>();
            Zs = new List<int>();
            count = 0;
        }


        public void Execute()
        {

            ProgressChanged(0, 1);
            Ys.Add(Y);
            count++;
            
            if (count == maxMoney)
            {
                
                for (int i = 0; i < maxMoney; i++)
                {
                    z = Ys[i] % p;
                    if (i >= I)
                    {                        
                        Zs.Add(z + 1);
                    }
                    else
                    {
                        Zs.Add(z);
                    }
                    OnPropertyChanged("Zs");
                }

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
