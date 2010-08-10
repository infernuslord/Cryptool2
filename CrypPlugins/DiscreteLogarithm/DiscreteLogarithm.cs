﻿/*                              
   Copyright 2010 Team CrypTool (Sven Rech), Uni Duisburg-Essen

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
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Threading;
using System.Collections;
using System.Numerics;
using Cryptool.PluginBase.IO;

namespace Cryptool.Plugins.DiscreteLogarithm
{
    [Author("Sven Rech", null, "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "Discrete Logarithm", "Calculates the discrete logarithm", null, "DiscreteLogarithm/icon.png")]
    /// <summary>
    /// This plugin calculates the discrete logarithm of the input.
    /// The input contains of a the BigInteger value and base and the modulo value to determine the residue class
    /// </summary>
    class DiscreteLogarithm : IThroughput
    {
        #region private members

        private DiscreteLogarithmSettings settings = new DiscreteLogarithmSettings();
        private BigInteger inputValue;
        private BigInteger inputBase;
        private BigInteger inputMod;
        private BigInteger outputLogarithm;

        #endregion

        #region events

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;

        #endregion

        #region public
        
        /// <summary>
        /// Notify that a property changed
        /// </summary>
        /// <param name="name">property name</param>
        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Gets/Sets the Settings of this plugin
        /// </summary>
        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (DiscreteLogarithmSettings)value; }
        }

        /// <summary>
        /// Get the Presentation of this plugin
        /// </summary>
        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// Get the QuickWatchRepresentation of this plugin
        /// </summary>
        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        /// <summary>
        /// Called by the environment before execution
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called by the environment to execute this plugin
        /// </summary>
        public void Execute()
        {
            if (inputMod.IsZero || inputMod.IsOne)
            {
                GuiLogMessage("Input modulo not valid!", NotificationLevel.Error);
                return;
            }

            inputBase %= inputMod;
            if (inputBase.IsZero || inputBase.IsOne)
            {
                GuiLogMessage("Input base not valid!", NotificationLevel.Error);
                return;
            }

            inputValue %= inputMod;
            if (inputValue.IsZero)
            {
                GuiLogMessage("Input value not valid!", NotificationLevel.Error);
                return;
            }

            BigInteger t = inputBase;
            BigInteger counter = 1;
            while (t != 1 && t != inputValue)
            {
                t = (t * inputBase) % inputMod;
                counter++;
            }
            if (t == inputValue)
                OutputLogarithm = counter;
            else
                GuiLogMessage("Input base not a generator of given residue class", NotificationLevel.Error);
        }//end Execute

        /// <summary>
        /// Called by the environment after execution of this plugin
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Called by the environment to pause this plugin
        /// </summary>
        public void Pause()
        {
        }

        /// <summary>
        /// Called by the environment to stop this plugin
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called by the environment to initialize this plugin
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called by the environment to Dispose this plugin
        /// </summary>
        public void Dispose()
        {

        }

        /// <summary>
        /// Gets/Sets the value x in b^log_b(x) = x
        /// </summary>
        [PropertyInfo(Direction.InputData, "Input value", "Enter your input value here", "", DisplayLevel.Beginner)]
        public BigInteger InputValue
        {
            get
            {
                return inputValue;
            }
            set
            {
                inputValue = value;
                OnPropertyChanged("InputValue");
            }
        }

        /// <summary>
        /// Gets/Sets the base b in b^log_b(x) = x
        /// </summary>
        [PropertyInfo(Direction.InputData, "Input base/generator", "Enter your base/generator here", "", DisplayLevel.Beginner)]
        public BigInteger InputBase
        {
            get
            {
                return inputBase;
            }
            set
            {
                inputBase = value;
                OnPropertyChanged("InputBase");
            }
        }

        /// <summary>
        /// Gets/Sets the modulo value for the used residue class
        /// </summary>
        [PropertyInfo(Direction.InputData, "Input modulo", "Enter your modulo here", "", DisplayLevel.Beginner)]
        public BigInteger InputMod
        {
            get
            {
                return inputMod;
            }
            set
            {
                inputMod = value;
                OnPropertyChanged("InputMod");
            }
        }

        /// <summary>
        /// Gets/Sets the calculated discrete logarithm
        /// </summary>
        [PropertyInfo(Direction.OutputData, "discrete logarithm output", "Get the result here!", "", DisplayLevel.Beginner)]
        public BigInteger OutputLogarithm
        {
            get
            {
                return outputLogarithm;
            }
            set
            {
                outputLogarithm = value;
                OnPropertyChanged("OutputLogarithm");
            }
        }
        

        #endregion

        #region private

        /// <summary>
        /// Change the progress of this plugin
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="max">max</param>
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        /// <summary>
        /// Logg a message to cryptool
        /// </summary>
        /// <param name="p">p</param>
        /// <param name="notificationLevel">notificationLevel</param>
        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion

    }//end DiscreteLogarithm

}//end Cryptool.Plugins.DiscreteLogarithm