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
using Cryptool.PluginBase.Cryptography;

using System.IO;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
// for [MethodImpl(MethodImplOptions.Synchronized)]
using System.Runtime.CompilerServices;

namespace Cryptool.Majority
{
    [Author("Soeren Rinne", "soeren.rinne@cryptool.de", "Ruhr-Universitaet Bochum, Chair for System Security", "http://www.trust.rub.de/")]
    [PluginInfo(true, "Majority", "Computes the majority of three boolean inputs\n1, if sum(1)>=sum(0); 0 else", "Majority/DetailedDescription/Description.xaml", "Majority/Images/icon.png", "Majority/Images/icon.png", "Majority/Images/icon.png")]
    public class Majority : IThroughput
    {

        #region Private variables

        private MajoritySettings settings;
        private bool inputOne;
        private bool inputTwo;
        private bool inputThree;
        private bool output;

        #endregion

        #region Public variables
        public int inputOneFlag = -1;
        public int inputTwoFlag = -1;
        public int inputThreeFlag = -1;
        #endregion

        #region Public interface

        /// <summary>
        /// Contructor
        /// </summary>
        public Majority()
        {
            this.settings = new MajoritySettings();
            ((MajoritySettings)(this.settings)).LogMessage += Xor_LogMessage;
        }

        /// <summary>
        /// Get or set all settings for this algorithm
        /// </summary>
        public ISettings Settings
        {
            get { return (ISettings)this.settings;
            }
            set { this.settings = (MajoritySettings)value;
            }
        }

        [PropertyInfo(Direction.InputData, "Majority Input One", " ", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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

        [PropertyInfo(Direction.InputData, "Majority Input Two", " ", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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

        [PropertyInfo(Direction.InputData, "Majority Input Three", " ", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public bool InputThree
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return this.inputThree; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                this.inputThree = value;
                OnPropertyChanged("InputThree");
                // clean inputTwo
                inputThreeFlag = 1;
            }
        }

        [PropertyInfo(Direction.OutputData, "Majority Output", " ", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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
            inputOneFlag = -1;
            inputTwoFlag = -1;
            inputThreeFlag = -1;
        }

        public void Dispose()
        {
            inputOneFlag = -1;
            inputTwoFlag = -1;
            inputThreeFlag = -1;
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
            if (inputOneFlag == 1 && inputTwoFlag == 1 && inputThreeFlag == 1)
            {
                // flag all inputs as dirty
                inputOneFlag = -1;
                inputTwoFlag = -1;
                inputThreeFlag = -1;

                // generate output
                int cntZeros = 0;
                int cntOnes = 0;
                if (inputOne) cntOnes++; else cntZeros++;
                if (inputTwo) cntOnes++; else cntZeros++;
                if (inputThree) cntOnes++; else cntZeros++;
                if (cntOnes >= cntZeros) output = true; else output = false;
                OnPropertyChanged("Output");
            }
        }

        public void Pause()
        {
        }

        #endregion
    }
}
