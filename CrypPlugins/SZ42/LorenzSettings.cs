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
        private string[] patterns = new string[12];
        private string [] positions = new string[12];
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

        public string[] Patterns
        {
            get { return patterns; }
            //set { patterns = value; }
        }

        public string[] Positions
        {
            get { return positions; }
            //set { positions = value; }
        }

        #endregion

        #region TaskPane Settings

        #region General Settings

        [TaskPane( "ActionCaption", "ActionTooltip", "General Settings", 0, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
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

        [TaskPane( "LimitationCaption", "LimitationTooltip", "General Settings", 1, false, ControlType.ComboBox, new string[] { "Chi2 One Back", "None" })]
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

        #region Wheels Patterns

        [TaskPane( "Patternχ1Caption", "Patternχ1Tooltip", "Wheels Patterns", 2, false, ControlType.TextBox, "")]
        public string Patternχ1
        {
            get
            {
                return this.patterns[0];
            }
            set
            {
                if (value != patterns[0]) HasChanges = true;
                this.patterns[0] = value;
                OnPropertyChanged("Pattern");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "Patternχ2Caption", "Patternχ2Tooltip", "Wheels Patterns", 3, false, ControlType.TextBox, "")]
        public string Patternχ2
        {
            get
            {
                return this.patterns[1];
            }
            set
            {
                if (value != patterns[1]) HasChanges = true;
                this.patterns[1] = value;
                OnPropertyChanged("Patternχ2");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "Patternχ3Caption", "Patternχ3Tooltip", "Wheels Patterns", 4, false, ControlType.TextBox, "")]
        public string Patternχ3
        {
            get
            {
                return this.patterns[2];
            }
            set
            {
                if (value != patterns[2]) HasChanges = true;
                this.patterns[2] = value;
                OnPropertyChanged("Patternχ3");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "Patternχ4Caption", "Patternχ4Tooltip", "Wheels Patterns", 5, false, ControlType.TextBox, "")]
        public string Patternχ4
        {
            get
            {
                return this.patterns[3];
            }
            set
            {
                if (value != patterns[3]) HasChanges = true;
                this.patterns[3] = value;
                OnPropertyChanged("Patternχ4");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "Patternχ5Caption", "Patternχ5Tooltip", "Wheels Patterns", 6, false, ControlType.TextBox, "")]
        public string Patternχ5
        {
            get
            {
                return this.patterns[4];
            }
            set
            {
                if (value != patterns[4]) HasChanges = true;
                this.patterns[4] = value;
                OnPropertyChanged("Patternχ5");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "PatternΨ1 Caption", "PatternΨ1 Tooltip", "Wheels Patterns", 7, false, ControlType.TextBox, "")]
        public string PatternΨ1 
        {
            get
            {
                return this.patterns[5];
            }
            set
            {
                if (value != patterns[5]) HasChanges = true;
                this.patterns[5] = value;
                OnPropertyChanged("PatternΨ1");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "PatternΨ2Caption", "PatternΨ2Tooltip", "Wheels Patterns", 8, false, ControlType.TextBox, "")]
        public string PatternΨ2
        {
            get
            {
                return this.patterns[6];
            }
            set
            {
                if (value != patterns[6]) HasChanges = true;
                this.patterns[6] = value;
                OnPropertyChanged("PatternΨ2");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "PatternΨ3Caption", "PatternΨ3Tooltip", "Wheels Patterns", 9, false, ControlType.TextBox, "")]
        public string PatternΨ3
        {
            get
            {
                return this.patterns[7];
            }
            set
            {
                if (value != patterns[7]) HasChanges = true;
                this.patterns[7] = value;
                OnPropertyChanged("PatternΨ3");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "PatternΨ4Caption", "PatternΨ4Tooltip", "Wheels Patterns", 10, false, ControlType.TextBox, "")]
        public string PatternΨ4
        {
            get
            {
                return this.patterns[8];
            }
            set
            {
                if (value != patterns[8]) HasChanges = true;
                this.patterns[8] = value;
                OnPropertyChanged("PatternΨ4");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "PatternΨ5Caption", "PatternΨ5Tooltip", "Wheels Patterns", 11, false, ControlType.TextBox, "")]
        public string PatternΨ5
        {
            get
            {
                return this.patterns[9];
            }
            set
            {
                if (value != patterns[9]) HasChanges = true;
                this.patterns[9] = value;
                OnPropertyChanged("PatternΨ5");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "Patternμ61Caption", "Patternμ61Tooltip", "Wheels Patterns", 12, false, ControlType.TextBox, "")]
        public string Patternμ61
        {
            get
            {
                return this.patterns[10];
            }
            set
            {
                if (value != patterns[10]) HasChanges = true;
                this.patterns[10] = value;
                OnPropertyChanged("Patternμ61");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "Patternμ37Caption", "Patternμ37Tooltip", "Wheels Patterns", 13, false, ControlType.TextBox, "")]
        public string Patternμ37
        {
            get
            {
                return this.patterns[11];
            }
            set
            {
                if (value != patterns[11]) HasChanges = true;
                this.patterns[11] = value;
                OnPropertyChanged("Patternμ37");

                if (ReExecute != null) ReExecute();
            }
        }

        #endregion

        #region Wheels Positions

        [TaskPane( "Positionχ1Caption", "Positionχ1Tooltip", "Wheels Positions", 14, false, ControlType.TextBox, "")]
        public string Positionχ1
        {
            get
            {
                return this.positions[0];
            }
            set
            {
                if (value != positions[0]) HasChanges = true;
                this.positions[0] = value;
                OnPropertyChanged("Positionχ1");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "Positionχ2Caption", "Positionχ2Tooltip", "Wheels Positions", 15, false, ControlType.TextBox, "")]
        public string Positionχ2
        {
            get
            {
                return this.positions[1];
            }
            set
            {
                if (value != positions[1]) HasChanges = true;
                this.positions[1] = value;
                OnPropertyChanged("Positionχ2");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "Positionχ3Caption", "Positionχ3Tooltip", "Wheels Positions", 16, false, ControlType.TextBox, "")]
        public string Positionχ3
        {
            get
            {
                return this.positions[2];
            }
            set
            {
                if (value != positions[2]) HasChanges = true;
                this.positions[2] = value;
                OnPropertyChanged("Positionχ3");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "Positionχ4Caption", "Positionχ4Tooltip", "Wheels Positions", 17, false, ControlType.TextBox, "")]
        public string Positionχ4
        {
            get
            {
                return this.positions[3];
            }
            set
            {
                if (value != positions[3]) HasChanges = true;
                this.positions[3] = value;
                OnPropertyChanged("Positionχ4");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "Positionχ5Caption", "Positionχ5Tooltip", "Wheels Positions", 18, false, ControlType.TextBox, "")]
        public string Positionχ5
        {
            get
            {
                return this.positions[4];
            }
            set
            {
                if (value != positions[4]) HasChanges = true;
                this.positions[4] = value;
                OnPropertyChanged("Positionχ5");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "PositionΨ1Caption", "PositionΨ1Tooltip", "Wheels Positions", 19, false, ControlType.TextBox, "")]
        public string PositionΨ1
        {
            get
            {
                return this.positions[5];
            }
            set
            {
                if (value != positions[5]) HasChanges = true;
                this.positions[5] = value;
                OnPropertyChanged("PositionΨ1");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "PositionΨ2Caption", "PositionΨ2Tooltip", "Wheels Positions", 20, false, ControlType.TextBox, "")]
        public string PositionΨ2
        {
            get
            {
                return this.positions[6];
            }
            set
            {
                if (value != positions[6]) HasChanges = true;
                this.positions[6] = value;
                OnPropertyChanged("PositionΨ2");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "PositionΨ3Caption", "PositionΨ3Tooltip", "Wheels Positions", 21, false, ControlType.TextBox, "")]
        public string PositionΨ3
        {
            get
            {
                return this.positions[7];
            }
            set
            {
                if (value != positions[7]) HasChanges = true;
                this.positions[7] = value;
                OnPropertyChanged("PositionΨ3");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "PositionΨ4Caption", "PositionΨ4Tooltip", "Wheels Positions", 22, false, ControlType.TextBox, "")]
        public string PositionΨ4
        {
            get
            {
                return this.positions[8];
            }
            set
            {
                if (value != positions[8]) HasChanges = true;
                this.positions[8] = value;
                OnPropertyChanged("PositionΨ4");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "PositionΨ5Caption", "PositionΨ5Tooltip", "Wheels Positions", 23, false, ControlType.TextBox, "")]
        public string PositionΨ5
        {
            get
            {
                return this.positions[9];
            }
            set
            {
                if (value != positions[9]) HasChanges = true;
                this.positions[9] = value;
                OnPropertyChanged("PositionΨ5");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "Positionμ61Caption", "Positionμ61Tooltip", "Wheels Positions", 24, false, ControlType.TextBox, "")]
        public string Positionμ61
        {
            get
            {
                return this.positions[10];
            }
            set
            {
                if (value != positions[10]) HasChanges = true;
                this.positions[10] = value;
                OnPropertyChanged("Positionμ61");

                if (ReExecute != null) ReExecute();
            }
        }

        [TaskPane( "Positionμ37Caption", "Positionμ37Tooltip", "Wheels Positions", 25, false, ControlType.TextBox, "")]
        public string Positionμ37
        {
            get
            {
                return this.positions[10];
            }
            set
            {
                if (value != positions[10]) HasChanges = true;
                this.positions[10] = value;
                OnPropertyChanged("Positionμ37");

                if (ReExecute != null) ReExecute();
            }
        }

        #endregion

        #region Format Settings

        [TaskPane( "InputParsedCaption", "InputParsedTooltip", "Format Settings", 26, false, ControlType.CheckBox, "")]
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

        [TaskPane( "OutputParsedCaption", "OutputParsedTooltip", "Format Settings", 27, false, ControlType.CheckBox, "")]
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
