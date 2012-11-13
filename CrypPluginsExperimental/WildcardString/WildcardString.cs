/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.WildcardString{

   [Author("Christopher Konze", "Christopher.Konze@cryptool.org", "University of Kassel", "http://www.uni-kassel.de/eecs/")]
   [PluginInfo("WildcardString.Properties.Resources", "StringInsertCaption", "StringInsertTooltip", "StringInsert/userdoc.xml", new[] { "StringInsert/Images/default.png" })]
   [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class WildcardString : ICrypComponent
    {
        #region Private Variables

       private readonly WildcardStringSettings settings = new WildcardStringSettings();

        #endregion

        #region Data Properties

        /// <summary>
        /// </summary>
        [PropertyInfo(Direction.InputData, "InsertString", "InsertStringTooltip")]
        public string InsertString
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "String1", "StringTooltip", false)]
        public string String1
        {
            get;
            set;
        }
        [PropertyInfo(Direction.InputData, "String2", "StringTooltip", false)]
        public string String2
        {
            get;
            set;
        }
        [PropertyInfo(Direction.InputData, "String3", "StringTooltip", false)]
        public string String3
        {
            get;
            set;
        }
        [PropertyInfo(Direction.InputData, "String4", "StringTooltip", false)]
        public string String4
        {
            get;
            set;
        }
        [PropertyInfo(Direction.InputData, "String5", "StringTooltip", false)]
        public string String5
        {
            get;
            set;
        }
        [PropertyInfo(Direction.InputData, "String6", "StringTooltip", false)]
        public string String6
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputString", "OutputStringTooltip")]
        public string OutputString
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            InsertString = InsertString.Replace("%1%", String1);
            InsertString = InsertString.Replace("%2%", String2);
            InsertString = InsertString.Replace("%3%", String3);
            InsertString = InsertString.Replace("%4%", String4);
            InsertString = InsertString.Replace("%5%", String5);
            InsertString = InsertString.Replace("%6%", String6);
          
            OutputString = InsertString;
            OnPropertyChanged("OutputString");
          
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
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
