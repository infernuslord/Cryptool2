/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

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
using Cryptool.PluginBase.Cryptography;

using System.IO;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
// for [MethodImpl(MethodImplOptions.Synchronized)]
using System.Runtime.CompilerServices;

namespace Cryptool.BoolToInt
{
    [Author("Soeren Rinne", "soeren.rinne@cryptool.de", "Ruhr-Universitaet Bochum, Chair for System Security", "http://www.trust.rub.de/")]
    [PluginInfo(false, "BoolToInt", "Converts Boolean to Integer", "BoolToInt/DetailedDescription/Description.xaml", "BoolToInt/Images/icon.png", "BoolToInt/Images/icon.png", "BoolToInt/Images/icon.png")]
    public class BoolToInt : IThroughput
    {

        #region Private variables

        private BoolToIntSettings settings;
        private bool input;
        private int output;

        #endregion

        #region Public interface

        /// <summary>
        /// Contructor
        /// </summary>
        public BoolToInt()
        {
            this.settings = new BoolToIntSettings();
            ((BoolToIntSettings)(this.settings)).LogMessage += Xor_LogMessage;
        }

        /// <summary>
        /// Get or set all settings for this algorithm
        /// </summary>
        public ISettings Settings
        {
            get { return (ISettings)this.settings;
            }
            set { this.settings = (BoolToIntSettings)value;
            }
        }

        [PropertyInfo(Direction.InputData, "Input", "Input a boolean value to be processed", "", true, false, QuickWatchFormat.Text, null)]
        public bool Input
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return this.input; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                this.input = value;
                OnPropertyChanged("Input");
                // clean inputOne
            }
        }

        [PropertyInfo(Direction.OutputData, "Output", "Output after converting Boolean to Integer.", "", false, false, QuickWatchFormat.Text, null)]
        public int Output
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
        }

        public void PreExecution()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region Private methods

        private void Xor_LogMessage(string msg, NotificationLevel logLevel)
        {
            /*if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(msg, this, logLevel));
            }*/
        }

        #endregion

        #region IPlugin Members

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
#pragma warning restore

        public void Execute()
        {
            if (input) output = 1; else output = 0;
            OnPropertyChanged("Output");
        }

        public void Pause()
        {
        }

        #endregion
    }
}
