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
using System.IO;
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool;
using Cryptool.PluginBase.Miscellaneous;


namespace Cryptool.Plugins.BooleanOperators
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("BooleanOperators.Properties.Resources", false, "PluginIfElseCaption", "PluginIfElseTooltip", "PluginDescriptionURL", "BooleanOperators/icons/ifelse.png")]
    public class BooleanIfElse : IThroughput
    {

        private Boolean input = false;
        private Boolean output_true = false;
        private Boolean output_false = false;

        private BooleanIfElseSettings settings;

        public BooleanIfElse()
        {
            this.settings = new BooleanIfElseSettings();
        }

        [PropertyInfo(Direction.InputData, "BooleanIfElseInputCaption", "BooleanIfElseInputTooltip", "")]
        public Boolean Input
        {
            get
            {
                return this.input; 
            }
            set 
            {
                this.input = value;
                OnPropertyChange("Input");
            }            
        }

        [PropertyInfo(Direction.OutputData, "BooleanIfElseOutput_trueCaption", "BooleanIfElseOutput_trueTooltip", "")]
        public Boolean Output_true
        {
            get 
            {
                return this.output_true;   
            }
            set 
            {   
                this.output_true = value;
                OnPropertyChange("Output_true");
            }
        }

        [PropertyInfo(Direction.OutputData, "BooleanIfElseOutput_falseCaption", "BooleanIfElseOutput_falseTooltip", "")]
        public Boolean Output_false
        {
            get
            {
                return this.output_false;
            }
            set
            {
                this.output_false = value;
                OnPropertyChange("Output_false");
            }
        }
        
        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (BooleanIfElseSettings)value; }
        }


        #region IPlugin Members

        public void Dispose()
        {
        }

        public void Execute()
        {
            Output_true = input;
            Output_false = !input;
            ProgressChanged(1, 1);

        }

        public void Initialize()
        {
        }
      
        public void Pause()
        {
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { 
                return null; 
            }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { 
                return null; 
            }
        }

        public void Stop()
        {
        }

        #endregion

        #region event handling

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChange(String propertyname)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(propertyname));
        }

        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, args);
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion
    }
}
