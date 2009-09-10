/*                              
   Copyright 2009 Team Cryptool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace Cryptool.Plugins.Collector
{
    [Author("Sven Rech", "sven.rech@cryptool.com", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "Collector", "Collector", null, "Collector/icon.png")]
    class Collector : IThroughput
    {
        #region Private Variables
        private CollectorSettings settings = new CollectorSettings();
        private List<CryptoolStream> listCryptoolStreamOut = new List<CryptoolStream>();        
        #endregion

        #region Properties

        private Object input1 = null;
        [PropertyInfo(Direction.InputData, "First Input", "First input to be collected", "", DisplayLevel.Beginner)]
        public Object Input1
        {
            get
            {
                return input1;
            }
            set
            {
                input1 = value;                
                OnPropertyChanged("Input1");
                Output = value;
            }
        }

        private Object input2 = null;
        [PropertyInfo(Direction.InputData, "Second Input", "Second input to be collected", "", DisplayLevel.Beginner)]
        public Object Input2
        {
            get
            {
                return input2;
            }
            set
            {
                input2 = value;                
                OnPropertyChanged("Input2");
                Output = value;
            }
        }

        private Object output = null;
        [PropertyInfo(Direction.OutputData, "Output", "Output", "", DisplayLevel.Beginner)]
        public Object Output
        {
            get
            {
                return output;
            }
            set
            {
                if (value is CryptoolStream)
                {
                    CryptoolStream cs = new CryptoolStream();
                    cs.OpenRead((value as CryptoolStream).FileName);
                    listCryptoolStreamOut.Add(cs);
                    output = cs;
                }
                else
                    output = value;

                OnPropertyChanged("Output");
            }
        }

        #endregion

        #region IPlugin Members

        public event Cryptool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;

        public event Cryptool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public event Cryptool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public Cryptool.PluginBase.ISettings Settings
        {
            get { return settings; }
            set { settings = (CollectorSettings)value; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            Dispose();
        }

        public void Execute()
        {
            ProgressChanged(1, 1);
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void Pause()
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
            foreach (CryptoolStream cs in listCryptoolStreamOut)
                cs.Close();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        #endregion
    }
}
