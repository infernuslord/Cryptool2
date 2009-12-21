/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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
using System.IO;
using System.ComponentModel;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
using Cryptool.PluginBase.Miscellaneous;
using System.Security.Cryptography;
// for [MethodImpl(MethodImplOptions.Synchronized)]
using System.Runtime.CompilerServices;

namespace Cryptool.Appender
{
    [Author("Soeren Rinne", "soeren.rinne@cryptool.org", "Ruhr-Universitaet Bochum, Chair for System Security", "http://www.trust.rub.de/")]
    [PluginInfo(false, "Appender", "Appends values", "Appender/DetailedDescription/Description.xaml", "Appender/Images/icon.png")]
    public class Appender : IThroughput
    {

        #region Private variables

        private AppenderSettings settings;
        private object input = null;
        private String output;

        #endregion

        #region Public variables
        #endregion

        #region Public interface

        /// <summary>
        /// Contructor
        /// </summary>
        public Appender()
        {
            this.settings = new AppenderSettings();
        }

        /// <summary>
        /// Get or set all settings for this algorithm
        /// </summary>
        public ISettings Settings
        {
            get { return (ISettings)this.settings;
            }
            set { this.settings = (AppenderSettings)value;
            }
        }

        [PropertyInfo(Direction.InputData, "Input", "Input objects to be appended", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public object Input
        {
            get
            {
                return this.input;
            }

            set
            {
                this.input = value;
                OnPropertyChanged("Input");
            }
        }

        [PropertyInfo(Direction.OutputData, "Appended Output", "Output after appending the inputs", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public String Output
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return output;
            }
            set
            {   // is readonly
            }
        }

        #endregion

        #region IPlugin members

        public void Initialize()
        {            
        }

        public void Dispose()
        {
            output = "";
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void Stop()
        {
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            Dispose();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region Private methods

        #endregion

        #region IPlugin Members

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void Execute()
        {
            if (input is Boolean)
            {
                if ((bool)input)
                    output += "1";
                else
                    output += "0";

                OnPropertyChanged("Output");
                ProgressChanged(1, 1);
            }
            else if (input is String)
            {
                output += input;
                OnPropertyChanged("Output");
                ProgressChanged(1, 1);
            }
            else
            {
                output = "Type not supported";
                OnPropertyChanged("Output");
                GuiLogMessage("Input can not be appended: Input type not supported. Supported types are Boolean and String", NotificationLevel.Error);
            }
        }

        public void Pause()
        {
        }

        private void StatusChanged(int imageIndex)
        {
            EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, imageIndex));
        }

        #endregion
    }

    #region Image

    enum ANDImage
    {
        Default
    }

    #endregion
}
