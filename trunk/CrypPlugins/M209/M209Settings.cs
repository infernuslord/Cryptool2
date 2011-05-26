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

namespace Cryptool.Plugins.M209
{
    public class M209Settings : ISettings
    {
        #region Public M209 specific interface

        public delegate void M209ReExecute();
        public event M209ReExecute ReExecute;
        #endregion

        #region Private Variables

        private bool hasChanges = false;

        private int selectedAction = 0;

        private string startwert = "AAAAAA";

        // aktive Pins an den Rotoren
        private string rotor1 = "ABDHIKMNSTVW";
        private string rotor2 = "ADEGJKLORSUX"; 
        private string rotor3 = "ABGHJLMNRSTUX";
        private string rotor4 = "CEFHIMNPSTU";
        private string rotor5 = "BDEFHIMNPS";
        private string rotor6 = "ABDHKNOQ";

        public string[] bar = new string[27] {
            "36","06","16","15","45","04","04","04","04",
            "20","20","20","20","20","20","20","20","20",
            "20","25","25","05","05","05","05","05","05"
        };

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        //[TaskPane("Startwert", "6 stelliger Startwert", null, 1, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "Encrypt", "Decrypt")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, true, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public int Action
        {
            get
            {
                return this.selectedAction;
            }
            set
            {
                if (value != selectedAction) HasChanges = true;
                this.selectedAction = value;
                OnPropertyChanged("Action");

                //if (ReExecute != null) ReExecute();
            }
        }



        [TaskPaneAttribute("StartwertCaption", "StartwertTooltip", null, 3, true, ControlType.TextBox, ValidationType.RegEx, "^([A-Z}]){6,6}$")]
        public string Startwert
        {
            get
            {
                return startwert;
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (startwert != value)
                {
                    startwert = value;
                    hasChanges = true;
                }
            }
        }

        #region Wheel options
        [TaskPane("Rotor1Caption", "Rotor1Tooltip", "Wheel options", 2, true, ControlType.TextBox, ValidationType.RegEx, "^([A-Z}]){0,26}$")]
        public string Rotor1
        {
            get
            {
                return rotor1;
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (rotor1 != value)
                {
                    rotor1 = value;
                    hasChanges = true;
                }
            }
        }
        [TaskPane("Rotor2Caption", "Rotor2Tooltip", "Wheel options", 2, true, ControlType.TextBox, ValidationType.RegEx, "^([A-V}X,Y,Z]){0,25}$")]
        public string Rotor2
        {
            get
            {
                return rotor2;
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (rotor2 != value)
                {
                    rotor2 = value;
                    hasChanges = true;
                }
            }
        }
        [TaskPane("Rotor3Caption", "Rotor3Tooltip", "Wheel options", 2, true, ControlType.TextBox, ValidationType.RegEx, "^([A-V}X]){0,23}$")]
        public string Rotor3
        {
            get
            {
                return rotor3;
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (rotor3 != value)
                {
                    rotor3 = value;
                    hasChanges = true;
                }
            }
        }
        [TaskPane("Rotor4Caption", "Rotor4Tooltip", "Wheel options", 2, true, ControlType.TextBox, ValidationType.RegEx, "^([A-U}]){0,21}$")]
        public string Rotor4
        {
            get
            {
                return rotor4;
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (rotor4 != value)
                {
                    rotor4 = value;
                    hasChanges = true;
                }
            }
        }
        [TaskPane("Rotor5Caption", "Rotor5Tooltip", "Wheel options", 2, true, ControlType.TextBox, ValidationType.RegEx, "^([A-S}]){0,19}$")]
        public string Rotor5
        {
            get
            {
                return rotor5;
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (rotor5 != value)
                {
                    rotor5 = value;
                    hasChanges = true;
                }
            }
        }
        [TaskPane("Rotor6Caption", "Rotor6Tooltip", "Wheel options", 2, true, ControlType.TextBox, ValidationType.RegEx, "^([A-Q}]){0,17}$")]
        public string Rotor6
        {
            get
            {
                return rotor6;
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (rotor6 != value)
                {
                    rotor6 = value;
                    hasChanges = true;
                }
            }
        }

        //WheelOptions
        #endregion

        #region Bar options
        [TaskPane("Bar1Caption", "Bar1Tooltip", "Bar options", 3, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar1
        {
            get
            {
                return bar[0];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[0] != value)
                {
                    bar[0] = value;
                    hasChanges = true;
                }
            }
        }
        [TaskPane("Bar2Caption", "Bar2Tooltip", "Bar options", 4, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar2
        {
            get
            {
                return bar[1];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[1] != value)
                {
                    bar[1] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar3Caption", "Bar3Tooltip", "Bar options", 5, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar3
        {
            get
            {
                return bar[2];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[2] != value)
                {
                    bar[2] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar4Caption", "Bar4Tooltip", "Bar options", 6, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar4
        {
            get
            {
                return bar[3];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[3] != value)
                {
                    bar[3] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar5Caption", "Bar5Tooltip", "Bar options", 7, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar5
        {
            get
            {
                return bar[4];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[4] != value)
                {
                    bar[4] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar6Caption", "Bar6Tooltip", "Bar options", 8, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar6
        {
            get
            {
                return bar[5];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[5] != value)
                {
                    bar[5] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar7Caption", "Bar7Tooltip", "Bar options", 9, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar7
        {
            get
            {
                return bar[6];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[6] != value)
                {
                    bar[6] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar8Caption", "Bar8Tooltip", "Bar options", 10, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar8
        {
            get
            {
                return bar[7];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[7] != value)
                {
                    bar[7] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar9Caption", "Bar9Tooltip", "Bar options", 11, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar9
        {
            get
            {
                return bar[8];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[8] != value)
                {
                    bar[8] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar10Caption", "Bar10Tooltip", "Bar options", 12, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar10
        {
            get
            {
                return bar[9];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[9] != value)
                {
                    bar[9] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar11Caption", "Bar11Tooltip", "Bar options", 13, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar11
        {
            get
            {
                return bar[10];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[10] != value)
                {
                    bar[10] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar12Caption", "Bar12Tooltip", "Bar options", 14, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar12
        {
            get
            {
                return bar[11];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[11] != value)
                {
                    bar[11] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar13Caption", "Bar13Tooltip", "Bar options", 15, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar13
        {
            get
            {
                return bar[12];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[12] != value)
                {
                    bar[12] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar14Caption", "Bar14Tooltip", "Bar options", 16, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar14
        {
            get
            {
                return bar[13];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[13] != value)
                {
                    bar[13] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar15Caption", "Bar15Tooltip", "Bar options", 17, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar15
        {
            get
            {
                return bar[14];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[14] != value)
                {
                    bar[14] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar16Caption", "Bar16Tooltip", "Bar options", 18, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar16
        {
            get
            {
                return bar[15];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[15] != value)
                {
                    bar[15] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar17Caption", "Bar17Tooltip", "Bar options", 19, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar17
        {
            get
            {
                return bar[16];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[16] != value)
                {
                    bar[16] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar18Caption", "Bar18Tooltip", "Bar options", 20, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar18
        {
            get
            {
                return bar[17];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[17] != value)
                {
                    bar[17] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar19Caption", "Bar19Tooltip", "Bar options", 21, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar19
        {
            get
            {
                return bar[18];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[18] != value)
                {
                    bar[18] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar20Caption", "Bar20Tooltip", "Bar options", 22, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar20
        {
            get
            {
                return bar[19];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[19] != value)
                {
                    bar[19] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar21Caption", "Bar21Tooltip", "Bar options", 23, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar21
        {
            get
            {
                return bar[20];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[20] != value)
                {
                    bar[20] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar22Caption", "Bar22Tooltip", "Bar options", 24, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar22
        {
            get
            {
                return bar[21];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[21] != value)
                {
                    bar[21] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar23Caption", "Bar23Tooltip", "Bar options", 25, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar23
        {
            get
            {
                return bar[22];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[22] != value)
                {
                    bar[22] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar24Caption", "Bar24Tooltip", "Bar options", 26, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar24
        {
            get
            {
                return bar[23];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[23] != value)
                {
                    bar[23] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar25Caption", "Bar25Tooltip", "Bar options", 27, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar25
        {
            get
            {
                return bar[24];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[24] != value)
                {
                    bar[24] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar26Caption", "Bar26Tooltip", "Bar options", 28, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar26
        {
            get
            {
                return bar[25];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[25] != value)
                {
                    bar[25] = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Bar27Caption", "Bar27Tooltip", "Bar options", 29, true, ControlType.TextBox, ValidationType.RegEx, "^([0-6}]){0,2}$")]
        public string Bar27
        {
            get
            {
                return bar[26];
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (bar[26] != value)
                {
                    bar[26] = value;
                    hasChanges = true;
                }
            }
        }

        // Bar Setting
        #endregion

        //Taskpane ende
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
