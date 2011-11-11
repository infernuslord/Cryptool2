/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using System.Runtime.Remoting.Contexts;

namespace Cryptool.Plugins.Collector
{
    [Author("Sven Rech", "sven.rech@cryptool.com", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Collector.Properties.Resources", "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "Collector/icon.png")]
    [Synchronization]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    class Collector : ICrypComponent
    {
        #region Private Variables
        private CollectorSettings settings = new CollectorSettings();
        private bool freshOutput = false;
        #endregion

        #region Properties

        private Object input1 = null;
        [PropertyInfo(Direction.InputData, "Input1Caption", "Input1Tooltip", false, QuickWatchFormat.Text, null)]
        public Object Input1
        {
            get
            {
                return input1;
            }
            set
            {
                input1 = value;
                freshOutput = true;
                OnPropertyChanged("Input1");
                Output = value;
            }
        }

        private Object input2 = null;
        [PropertyInfo(Direction.InputData, "Input2Caption", "Input2Tooltip", false, QuickWatchFormat.Text, null)]
        public Object Input2
        {
            get
            {
                return input2;
            }
            set
            {
                input2 = value;
                freshOutput = true;
                OnPropertyChanged("Input2");
                Output = value;
            }
        }

        private Object output = null;
        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip")]
        public Object Output
        {
            get
            {
                return output;
            }
            set
            {
                output = value;
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

        public void PreExecution()
        {
            Dispose();

            freshOutput = false;
            input1 = null;
            input2 = null;
            output = null;
        }

        public void Execute()
        {
            if (freshOutput)
            {
                ProgressChanged(1, 1);

                OnPropertyChanged("Output");
                freshOutput = false;
            }
        }

        public void PostExecution()
        {
            Dispose();
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

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        #endregion
    }
}
