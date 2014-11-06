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
using System.Windows;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;
using Cryptool.PluginBase.Attributes;

namespace Cryptool.Plugins.Numbers
{
    [Author("Sven Rech, Nils Kopal", "sven.rech@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Cryptool.Plugins.Numbers.Properties.Resources", "PluginInputCaption", "PluginInputTooltip", "Numbers/DetailedDescription/doc.xml", "Numbers/icons/inputIcon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    [ComponentVisualAppearance(ComponentVisualAppearance.VisualAppearanceEnum.Opened)]
    class NumberInput : ICrypComponent
    {
        private NumberInputPresentation _presentation = new NumberInputPresentation();
        private Boolean _running = false;

        public NumberInput()
        {
            settings = new NumberInputSettings();
            _presentation.TextBox.TextChanged +=new TextChangedEventHandler(TextBox_TextChanged);
            DataObject.AddPastingHandler(_presentation.TextBox, OnCancelCommand);
            settings.PropertyChanged += settings_OnPropertyChanged;
        }

        private void settings_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowDigits")
            {
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    SetStatusBar();
                }, null);

            }
        }

        private void OnCancelCommand(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetData(DataFormats.Text) is string)
            {
                var s = (string)e.DataObject.GetData(DataFormats.Text);
                if (s.Any(c => !"01234567890+-*/^ ()AaBbCcDdEeFf#HhXx".Contains(c)))
                {
                    e.CancelCommand();
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs args)
        {
            settings.Number = _presentation.TextBox.Text;
            SetStatusBar();
            if (_running)
            {
                Execute();
            }
        }

        private void SetStatusBar()
        {
            if (settings.ShowDigits)
            {
                _presentation.StatusBar.Visibility = Visibility.Visible;
                int digits, bits;
                try
                {
                    GetDigitsAndBitsWithTimeOut(out digits, out bits);
                    //var number = GetNumberWithTimeOut(out digits, out bits);
                    //bits = number.BitCount(); 
                    //digits = BigInteger.Abs(number).ToString().Length;
                }
                catch (Exception ex)
                {
                    _presentation.StatusBar.Content = (ex is OutOfMemoryException || ex is OverflowException) ? "Overflow" : "Not a number";
                    return;
                }

                string digitText = (digits == 1) ? Properties.Resources.Digit : Properties.Resources.Digits;
                string bitText = (bits == 1) ? Properties.Resources.Bit : Properties.Resources.Bits;
                _presentation.StatusBar.Content = string.Format(" {0:#,0} {1}, {2:#,0} {3}", digits, digitText, bits, bitText);
            }
            else
            {
                _presentation.StatusBar.Visibility = Visibility.Collapsed;
            }
        }

        // status=0: no problems occured
        // status=1: overflow
        // status=2: not a number
        private int[] GetDigitsAndBits(out int status)
        {
            try
            {
                var number = GetNumber();
                int bits = number.BitCount();
                int digits = BigInteger.Abs(number).ToString().Length;
                status = 0;
                return new int[] { digits, bits };
            }
            catch (Exception ex)
            {
                status = (ex is OutOfMemoryException || ex is OverflowException) ? 1 : 2;
                return null;
            }
        }

        Thread workerThread;
        private void GetDigitsAndBitsWithTimeOut(out int digits, out int bits)
        {
            int[] result = null;
            int status = 0;

            if (workerThread != null && workerThread.IsAlive)
                workerThread.Abort();

            workerThread = new Thread(() => result = GetDigitsAndBits(out status));

            workerThread.Start();

            bool finished = workerThread.Join(TimeSpan.FromMilliseconds(500));
            if (!finished)
            {
                workerThread.Abort();
                throw new OverflowException();
            }

            if (status == 1)
                throw new OverflowException();

            if (status == 2)
                throw new Exception();
            
            digits = result[0];
            bits = result[1];
        }

        //private void GetDigitsAndBitsWithTimeOut(out int digits, out int bits)
        //{
        //    int[] result = null;

        //    var tokenSource = new CancellationTokenSource();
        //    CancellationToken token = tokenSource.Token;
        //    int timeOut = 500; // milliseconds

        //    bool success;
        //    var task = Task.Factory.StartNew(() => result = GetDigitsAndBits(), token);
        //    success = task.Wait(timeOut, token);

        //    if (!success)
        //        throw new OverflowException();

        //    digits = result[0];
        //    bits = result[1];
        //}

        private BigInteger GetNumber()
        {
            //The input from the taskpane is converted to a BigNumber and is sent to the output.
            if (settings.Number == null || settings.Number.Equals(""))
            {
                return BigInteger.Zero;
            }
            return BigIntegerHelper.ParseExpression(settings.Number);
        }

        #region Properties

        private BigInteger numberOutput = 0;
        /// <summary>
        /// The output is defined
        /// </summary>
        [PropertyInfo(Direction.OutputData, "NumberOutputCaption", "NumberOutputTooltip")]
        public BigInteger NumberOutput
        {
            get
            {
                return numberOutput;
            }
            set
            {
                numberOutput = value;
                OnPropertyChanged("NumberOutput");
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

        private NumberInputSettings settings;
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (NumberInputSettings)value; }
        }

        public UserControl Presentation
        {
            get { return _presentation; }
        }

        public void PreExecution()
        {
            _running = true;
        }

        public void Execute()
        {
            try
            {
                NumberOutput = GetNumber();
            }
            catch (Exception ex)
            {
                GuiLogMessage("Invalid Big Number input: " + ex.Message, NotificationLevel.Error);
                return;
            }
            
            ProgressChanged(1.0, 1.0);
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void Stop()
        {
            _running = false;
        }

        public void Initialize()
        {
            _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this._presentation.TextBox.Text = settings.Number;
            }
            , null);            
        }

        public void Dispose()
        {
            _running = false;
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
