/*
   Copyright 2009 Matthäus Wander, Universität Duisburg-Essen

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
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;

namespace Cryptool.Plugin.ExamplePluginCT2
{
    [Author("Matthäus Wander", "wander@cryptool.org", "Universität Duisburg-Essen, Fachgebiet Verteilte Systeme", "http://www.vs.uni-due.de")]
    [PluginInfo(false, "Example Plugin", "Subtract one number from another", null, "CrypWin/images/default.png")]
    public class ExamplePluginCT2 : IIOMisc
    {
        #region Private Variables

        private ExamplePluginCT2Settings settings = new ExamplePluginCT2Settings();

        #endregion

        #region Data Properties

        /// <summary>
        /// Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "Minuend", "Input number", null, DisplayLevel.Beginner)]
        public int Minuend
        {
            get;
            set;
        }

        /// <summary>
        /// Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "Difference", "Result", null, DisplayLevel.Beginner)]
        public int Difference
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Optional: provide custom (tabbed) presentation to visualize your algorithm
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// Optional: provide custom (quickwatch) presentation to visualize your algorithm
        /// </summary>
        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        /// <summary>
        /// The algorithm you'd like to implement
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            Difference = Minuend - settings.Subtrahend;
            OnPropertyChanged("Difference");

            if (settings.Subtrahend < 0)
                GuiLogMessage("Subtrahend is negative", NotificationLevel.Debug);

            ProgressChanged(1, 1);
        }

        public void PostExecution()
        {
        }

        public void Pause()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
