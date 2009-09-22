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
using System.Windows;

namespace Cryptool.Plugins.BooleanOperators
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-duisburg-essen.de")]
    [PluginInfo(true, "Boolean Input", "Boolean Input", "BooleanOperators/DetailedDescription/Description.xaml", "BooleanOperators/icons/false.png", "BooleanOperators/icons/true.png")]
    public class BooleanInput : IInput
    {
        private BooleanInputSettings settings;
        private Boolean output = false;

        public BooleanInput()
        {
            this.settings = new BooleanInputSettings();            
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
            this.settings.PropertyChanged += settings_PropertyChanged;
        }

        void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                // as the setting is not changeable in play mode, there is no need to update Output property
                //Output = (settings.Value == 1);

                settings_OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, settings.Value));
            }
        }

        [PropertyInfo(Direction.OutputData, "Output", "Output", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public Boolean Output
        {
            get
            {
                return this.output;
            }
            set
            {
                this.output = value;
                OnPropertyChange("Output");
            }
        }

        #region IPlugin Member

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Execute()
        {
            Output = (settings.Value == 1);
            settings_OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, settings.Value));

            ProgressChanged(1, 1);
        }

        public void Initialize()
        {
            // not working, see ticket #80
            settings_OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, settings.Value));
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
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (BooleanInputSettings)value; }
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
