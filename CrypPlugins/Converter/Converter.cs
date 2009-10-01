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
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.CompilerServices;

namespace Cryptool.Plugins.Comparators
{
    [Author("Raoul Falk, Dennis Nolte", "falk@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "Converter", "Converts input to another type", "", "Converter/icons/icon.png")]

    class Converter : IThroughput
    {
        #region private variables

        private ComparatorsSettings settings = new ConverterSettings();
        private object inputOne;
        private object output;

        #endregion

        #region public interfaces

        public Converter()
        {
            this.settings = new ConverterSettings();
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (ConverterSettings)value; }
        }

        private void Comparators_LogMessage(string msg, NotificationLevel loglevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(msg, this, loglevel));
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        [PropertyInfo(Direction.InputData, "Input one", "Input one.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public object InputOne
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return inputOne; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (value != inputOne)
                {
                    inputOne = value;
                    OnPropertyChanged("InputOne");
                }
            }
        }

     

        [PropertyInfo(Direction.OutputData, "Output", "Output.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public object Output
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return output;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                this.output = value;
                OnPropertyChanged("Output");
            }
        }

        #endregion

        #region IPlugin members

        public void Dispose()
        {

        }

        public void Execute()
        {
           
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

        public void Stop()
        {

        }

        #endregion

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region event handling

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, args);
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion
    }
}
