/*                              
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
using System.Collections.Generic;
using System.Numerics;
using Cryptool.PluginBase.IO;
using DiscreteLogarithm;

namespace Cryptool.Plugins.DiscreteLogarithm
{
    /// <summary>
    /// This plugin calculates the discrete logarithm of the input.
    /// The input contains of a the BigInteger value and base and the modulo value to determine the residue class
    /// </summary>
    [Author("Sven Rech", null, "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("DiscreteLogarithm.Properties.Resources", "PluginCaption", "PluginTooltip", "DiscreteLogarithm/DetailedDescription/doc.xml", "DiscreteLogarithm/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    class DiscreteLogarithm : ICrypComponent
    {
        #region private members

        private DiscreteLogarithmSettings settings = new DiscreteLogarithmSettings();
        private BigInteger inputValue;
        private BigInteger inputBase;
        private BigInteger inputMod;
        private BigInteger outputLogarithm;
        private bool running;

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
        /// Called by the environment before execution
        /// </summary>
        public void PreExecution()
        {
            running = false;
        }

        /// <summary>
        /// Called by the environment to execute this plugin
        /// </summary>
        public void Execute()
        {
            running = true;

            if (inputMod<2)
            {
                GuiLogMessage("Input modulo not valid!", NotificationLevel.Error);
                return;
            }

            inputBase %= inputMod;
            if (inputBase<2)
            {
                GuiLogMessage("Input base not valid!", NotificationLevel.Error);
                return;
            }

            inputValue %= inputMod;
            if (inputValue<1)
            {
                GuiLogMessage("Input value not valid!", NotificationLevel.Error);
                return;
            }

            switch (settings.Algorithm)
            {
                case 0:
                    //Enumeration();
                    Shanks();
                    break;
                case 1:
                    IndexCalculus();
                    break;
            }
        }

        private void IndexCalculus()
        {
            //TODO: Make index calculus method stoppable ;)
            IndexCalculusMethod ic = new IndexCalculusMethod();
            try
            {
                BigInteger res = ic.DiscreteLog(inputValue, inputBase, inputMod);
                OutputLogarithm = res;
            }
            catch (Exception ex)
            {
                GuiLogMessage("Index-Calculus error: " + ex.Message, NotificationLevel.Error);
            }
        }

        private void Enumeration()
        {
            BigInteger t = inputBase;
            BigInteger counter = 1;
            while (t != 1 && t != inputValue && running)
            {
                t = (t * inputBase) % inputMod;
                counter++;
            }

            if (!running)
                return;

            if (t == inputValue)
                OutputLogarithm = counter;
            else
                GuiLogMessage("Input base is not a generator of the given residue class", NotificationLevel.Error);
        }
        /// <summary>
        /// Baby-step giant-step algorithm by Daniel Shanks
        /// </summary>
        private void Shanks()
        {
            BigInteger m = inputMod.Sqrt() + 1;
            Dictionary<BigInteger, BigInteger> hashtab = new Dictionary<BigInteger, BigInteger>();

            if (BigIntegerHelper.GCD(inputBase, inputMod) > 1)
            {
                GuiLogMessage("Input base is not a generator of the given residue class", NotificationLevel.Error);
                return;
            }

            BigInteger g_inv = BigIntegerHelper.ModInverse(inputBase, inputMod);
            BigInteger g_m = BigInteger.ModPow(inputBase, m, inputMod);

            GuiLogMessage("Generating baby steps table with " + m + " entries.", NotificationLevel.Debug);
            
            // baby-steps
            BigInteger v = inputValue;
            for (BigInteger j = 0; j < m; j++)
            {
                if (!running) return;
                if (v == 1)
                {
                    OutputLogarithm = j;
                    return;
                }
                if (hashtab.ContainsKey(v)) break;
                hashtab.Add(v, j);
                v = (v * g_inv) % inputMod;

                ProgressChanged((int)j, (int)(2 * m));
            }

            // giant-steps
            v = 1;
            for (BigInteger i = 0; i <= m; i++)
            {
                if (!running) return;
                if (hashtab.ContainsKey(v))
                {
                    OutputLogarithm = i * m + hashtab[v];
                    return;
                }
                v = (v * g_m) % inputMod;

                ProgressChanged((int)(m + i), (int)(2 * m));
            }

            GuiLogMessage("Input base is not a generator of the given residue class", NotificationLevel.Error);
        }

        /// <summary>
        /// Called by the environment after execution of this plugin
        /// </summary>
        public void PostExecution()
        {
            running = false;
        }

        /// <summary>
        /// Called by the environment to stop this plugin
        /// </summary>
        public void Stop()
        {
            running = false;
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
        [PropertyInfo(Direction.InputData, "InputValueCaption", "InputValueTooltip")]
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
        [PropertyInfo(Direction.InputData, "InputBaseCaption", "InputBaseTooltip")]
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
        [PropertyInfo(Direction.InputData, "InputModCaption", "InputModTooltip")]
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
        [PropertyInfo(Direction.OutputData, "OutputLogarithmCaption", "OutputLogarithmTooltip")]
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
