/* HOWTO: Change year, author name and organization.
   Copyright 2010 Your Name, University of Duckburg

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

namespace Cryptool.Plugins.SZ42
{
    public class LorenzSettings : ISettings
    {
        #region Private Variables

        private int limitation = 0;
        private string pattern = "";
        private string position = "";
        private int action = 0;
        private bool inputParsed = false;
        private bool outputParsed = false;
        private bool hasChanges = false;

        #endregion

        #region Public Interface

        /// <summary>
        /// We use this delegate to send log messages from the settings class to the Caesar plugin
        /// </summary>
        public delegate void LorenzLogMessage(string msg, NotificationLevel loglevel);
        public event LorenzLogMessage LogMessage;
        public delegate void LorenzReExecute();
        public event LorenzReExecute ReExecute;

        #endregion

        #region TaskPane Settings

        #region General Settings

        [TaskPane("Action", "Select the Algorithm action", "General Settings", 0, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public int Action
        {
            get
            {
                return this.action;
            }
            set
            {
                if (value != action) HasChanges = true;
                this.action = value;
                OnPropertyChanged("Action");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane("Limitation", "Select the limitation type", "General Settings", 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Chi2 One Back", "None" })]
        public int Limitation
        {
            get
            {
                return this.limitation;
            }
            set
            {
                if (value != limitation) HasChanges = true;
                this.limitation = value;
                OnPropertyChanged("Limitation");

                if (ReExecute != null) ReExecute();
            }
        }

        #endregion

        #region Wheels Configuration

        [TaskPane("Wheels Pattern", "Set the wheels pattern", "Wheels Configuration", 2, false, DisplayLevel.Beginner, ControlType.TextBox, "")]
        public string Pattern
        {
            get
            {
                return this.pattern;
            }
            set
            {
                if (value != pattern) HasChanges = true;
                this.pattern = value;
                OnPropertyChanged("Pattern");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane("Wheels Position", "Set the wheels position", "Wheels Configuration", 3, false, DisplayLevel.Beginner, ControlType.TextBox, "")]
        public string Position
        {
            get
            {
                return this.position;
            }
            set
            {
                if (value != position) HasChanges = true;
                this.position = value;
                OnPropertyChanged("Position");

                if (ReExecute != null) ReExecute();
            }
        }

        #endregion

        #region Format Settings

        [TaskPane("Input Parsed", "Set if the input is parsed or not", "Format Settings", 4, false, DisplayLevel.Beginner, ControlType.CheckBox, "")]
        public bool InputParsed
        {
            get
            {
                return this.inputParsed;
            }
            set
            {
                if (value != inputParsed) HasChanges = true;
                this.inputParsed = value;
                OnPropertyChanged("InputParsed");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane("Output Parsed", "Set if the output is parsed or not", "Format Settings", 5, false, DisplayLevel.Beginner, ControlType.CheckBox, "")]
        public bool OutputParsed
        {
            get
            {
                return this.outputParsed;
            }
            set
            {
                if (value != outputParsed) HasChanges = true;
                this.outputParsed = value;
                OnPropertyChanged("OutputParsed");

                if (ReExecute != null) ReExecute();
            }
        }

        #endregion


        #endregion

        #region ISettings Members

        /// <summary>
        /// HOWTO: This flags indicates whether some setting has been changed since the last save.
        /// If a property was changed, this becomes true, hence CrypTool will ask automatically if you want to save your changes.
        /// </summary>
        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                hasChanges = value;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
