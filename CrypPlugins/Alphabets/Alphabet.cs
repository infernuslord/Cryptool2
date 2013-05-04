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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Threading;
using System.Threading;

namespace Cryptool.Alphabets
{
    [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni-Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("Cryptool.Alphabets.Properties.Resources", "PluginCaption", "PluginTooltip", "Alphabets/DetailedDescription/doc.xml", "Alphabets/icon.gif")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class Alphabet : ICrypComponent
    {
        private AlphabetPresentation alphabetPresentation;

        private AlphabetSettings settings;
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (AlphabetSettings)value; }
        }

        DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };

        private int count = 0;
        public Alphabet()
        {
            settings = new AlphabetSettings();
            alphabetPresentation = new AlphabetPresentation(settings);
            Presentation = this.alphabetPresentation;
            alphabetPresentation.AlphabetChanged += new EventHandler(alphabetPresentation_AlphabetChanged);
            settings.PropertyChanged += settings_PropertyChanged;
            
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            List<AlphabetItemData> tmp = new List<AlphabetItemData>();
            tmp.Add(AlphabetItem.CyrillicAlphabet.Data);
            tmp.Add(AlphabetItem.GreekAlphabet.Data);
            tmp.Add(AlphabetItem.BasicLatinAlphabet.Data);
            settings.Data = AlphabetSettings.Serialize(tmp);
            alphabetPresentation.SetNewItems(AlphabetSettings.Deserialize(settings.Data));
        }

        void alphabetPresentation_AlphabetChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("AlphabetOutput");
        }

        void alphabetPresentation_OnGuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
        {
            GuiLogMessage(args.Message, args.NotificationLevel);
        }

        void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (count == 0)
            {
                alphabetPresentation.SetNewItems(AlphabetSettings.Deserialize(settings.Data));
                timer.Stop();
                count++;
            }
        }

        string alphabetString = string.Empty;

        [PropertyInfo(Direction.OutputData, "AlphabetOutputCaption", "AlphabetOutputTooltip", false)]
        public string AlphabetOutput
        {
            get
            {
                return alphabetString;
            }
            set { } //readonly
        }


        public UserControl Presentation { get; private set; }

        public void Initialize()
        {

        }

        public void Dispose()
        {
        }

        public void Stop()
        {
        }

        public void PreExecution()
        {
            this.alphabetPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this.alphabetString = alphabetPresentation.GetAlphabet();
            }, null);

        }

        public void PostExecution()
        {
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #region IPlugin Members

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        public void Execute()
        {
            OnPropertyChanged("AlphabetOutput");
            ShowProgress(100, 100);
        }

        #endregion

        #region Private
        private void ShowProgress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
        #endregion
    }
}
