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

namespace Cryptool.BoolComparator
{
    [Author("Soeren Rinne", "soeren.rinne@cryptool.de", "Ruhr-Universitaet Bochum, Chair for System Security", "http://www.trust.rub.de/")]
    [PluginInfo("Cryptool.BoolComparator.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "BoolComparator/Images/icon.png", "BoolComparator/Images/icon.png", "BoolComparator/Images/icon.png")]
    public class BoolComparator : IThroughput
    {

        #region Private variables

        private BoolComparatorSettings settings;
        private bool inputOne;
        private bool inputTwo;
        private bool output;

        #endregion

        #region Public variables
        public int inputOneFlag;
        public int inputTwoFlag;
        #endregion

        #region Public interface

        /// <summary>
        /// Contructor
        /// </summary>
        public BoolComparator()
        {
            this.settings = new BoolComparatorSettings();
            ((BoolComparatorSettings)(this.settings)).LogMessage += Xor_LogMessage;
        }

        /// <summary>
        /// Get or set all settings for this algorithm
        /// </summary>
        public ISettings Settings
        {
            get { return (ISettings)this.settings;
            }
            set { this.settings = (BoolComparatorSettings)value;
            }
        }

        [PropertyInfo(Direction.InputData, "InputOneCaption", "InputOneTooltip", "", true, true, QuickWatchFormat.Text, null)]
        public bool InputOne
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return this.inputOne; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                this.inputOne = value;
                OnPropertyChanged("InputOne");
                // clean inputOne
                inputOneFlag = 1;
            }
        }

        [PropertyInfo(Direction.InputData, "InputTwoCaption", "InputTwoTooltip", "", false, true, QuickWatchFormat.Text, null)]
        public bool InputTwo
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return this.inputTwo; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                this.inputTwo = value;
                OnPropertyChanged("InputTwo");
                // clean inputTwo
                inputTwoFlag = 1;
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip", "", false, false, QuickWatchFormat.Text, null)]
        public bool Output
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
            // set input flags according to settings
            if (settings.FlagInputOne) inputOneFlag = 1;
            else inputOneFlag = -1;
            if (settings.FlagInputTwo) inputTwoFlag = 1;
            else inputTwoFlag = -1;
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
            if (inputOneFlag == 1 && inputTwoFlag == 1)
            {
                // flag all inputs as dirty
                inputOneFlag = -1;
                inputTwoFlag = -1;

                if (inputOne == inputTwo) output = true; else output = false;
                OnPropertyChanged("Output");
            }
        }

        public void Pause()
        {
        }

        #endregion
    }
}
