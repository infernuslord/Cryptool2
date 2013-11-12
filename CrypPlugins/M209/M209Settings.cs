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
using System.Collections.ObjectModel;

namespace Cryptool.Plugins.M209
{
    public class M209Settings : ISettings
    {
        #region Public M209 specific interface

        public delegate void M209ReExecute();
        public event M209ReExecute ReExecute;
        #endregion

        #region Private Variables
        private ObservableCollection<string> actionStrings = new ObservableCollection<string>();
        private ObservableCollection<string> rotorAStrings = new ObservableCollection<string>();
        private ObservableCollection<string> rotorBStrings = new ObservableCollection<string>();
        private ObservableCollection<string> reflectorStrings = new ObservableCollection<string>();
        private int model = 0;

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
        private int unknownSymbolHandling = 0; // 0=ignore, leave unmodified
        private int caseHandling = 0; // 0=preserve, 1, convert all to upper, 2= convert all to lower
        #endregion

        #region TaskPane Settings

        [ContextMenu("ModelCaption", "ModelTooltip", 0, ContextMenuControlType.ComboBox, null, new string[] { "ModelList1", "ModelList2"})]
        [TaskPane("ModelCaption", "ModelTooltip", null, 0, false, ControlType.ComboBox, new string[] { "ModelList1", "ModelList2"})]
        [PropertySaveOrder(1)]
        public int Model
        {
            get { return this.model; }
            set
            {
                if (value != model)
                {
                    this.model = value;
                    OnPropertyChanged("Model");
                }
            }
        }
        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        //[TaskPane("Startwert", "6 stelliger Startwert", null, 1, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "ActionList1", "ActionList2")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, true, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get
            {
                return this.selectedAction;
            }
            set
            {
                if (value != selectedAction)
                {
                    this.selectedAction = value;
                    OnPropertyChanged("Action");

                    //if (ReExecute != null) ReExecute();   
                }
            }
        }

        [TaskPaneAttribute("StartwertCaption", "StartwertTooltip", null, 3, true, ControlType.TextBox, ValidationType.RegEx, "^[A-Z]{6}$")]
        public string Startwert
        {
            get
            {
                return startwert;
            }
            set
            {
                if (startwert != value)
                {
                    startwert = value;
                    OnPropertyChanged("Startwert");
                }
            }
        }

        #region Wheel options
        [TaskPane("Rotor1Caption", "Rotor1Tooltip", "WheelGroup", 2, true, ControlType.TextBox, ValidationType.RegEx, "^[A-Z]{0,26}$")]
        public string Rotor1
        {
            get
            {
                return rotor1;
            }
            set
            {
                if (rotor1 != value)
                {
                    rotor1 = value;
                    OnPropertyChanged("Rotor1");
                }
            }
        }
        [TaskPane("Rotor2Caption", "Rotor2Tooltip", "WheelGroup", 2, true, ControlType.TextBox, ValidationType.RegEx, "^[A-VX-Z]{0,25}$")]
        public string Rotor2
        {
            get
            {
                return rotor2;
            }
            set
            {
                if (rotor2 != value)
                {
                    rotor2 = value;
                    OnPropertyChanged("Rotor2");
                }
            }
        }
        [TaskPane("Rotor3Caption", "Rotor3Tooltip", "WheelGroup", 2, true, ControlType.TextBox, ValidationType.RegEx, "^[A-VX]{0,23}$")]
        public string Rotor3
        {
            get
            {
                return rotor3;
            }
            set
            {
                if (rotor3 != value)
                {
                    rotor3 = value;
                    OnPropertyChanged("Rotor3");
                }
            }
        }
        [TaskPane("Rotor4Caption", "Rotor4Tooltip", "WheelGroup", 2, true, ControlType.TextBox, ValidationType.RegEx, "^[A-U]{0,21}$")]
        public string Rotor4
        {
            get
            {
                return rotor4;
            }
            set
            {
                if (rotor4 != value)
                {
                    rotor4 = value;
                    OnPropertyChanged("Rotor4");
                }
            }
        }
        [TaskPane("Rotor5Caption", "Rotor5Tooltip", "WheelGroup", 2, true, ControlType.TextBox, ValidationType.RegEx, "^[A-S]{0,19}$")]
        public string Rotor5
        {
            get
            {
                return rotor5;
            }
            set
            {
                if (rotor5 != value)
                {
                    rotor5 = value;
                    OnPropertyChanged("Rotor5");
                }
            }
        }
        [TaskPane("Rotor6Caption", "Rotor6Tooltip", "WheelGroup", 2, true, ControlType.TextBox, ValidationType.RegEx, "^[A-Q]{0,17}$")]
        public string Rotor6
        {
            get
            {
                return rotor6;
            }
            set
            {
                if (rotor6 != value)
                {
                    rotor6 = value;
                    OnPropertyChanged("Rotor6");
                }
            }
        }

        //WheelOptions
        #endregion

        #region Bar options
        [TaskPane("Bar1Caption", "Bar1Tooltip", "BarGroup", 3, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar1
        {
            get
            {
                return bar[0];
            }
            set
            {
                if (bar[0] != value)
                {
                    bar[0] = value;
                    OnPropertyChanged("Bar1");
                }
            }
        }
        [TaskPane("Bar2Caption", "Bar2Tooltip", "BarGroup", 4, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar2
        {
            get
            {
                return bar[1];
            }
            set
            {
                if (bar[1] != value)
                {
                    bar[1] = value;
                    OnPropertyChanged("Bar2");
                }
            }
        }

        [TaskPane("Bar3Caption", "Bar3Tooltip", "BarGroup", 5, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar3
        {
            get
            {
                return bar[2];
            }
            set
            {
                if (bar[2] != value)
                {
                    bar[2] = value;
                    OnPropertyChanged("Bar3");
                }
            }
        }

        [TaskPane("Bar4Caption", "Bar4Tooltip", "BarGroup", 6, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar4
        {
            get
            {
                return bar[3];
            }
            set
            {
                if (bar[3] != value)
                {
                    bar[3] = value;
                    OnPropertyChanged("Bar4");
                }
            }
        }

        [TaskPane("Bar5Caption", "Bar5Tooltip", "BarGroup", 7, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar5
        {
            get
            {
                return bar[4];
            }
            set
            {
                if (bar[4] != value)
                {
                    bar[4] = value;
                    OnPropertyChanged("Bar5");
                }
            }
        }

        [TaskPane("Bar6Caption", "Bar6Tooltip", "BarGroup", 8, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar6
        {
            get
            {
                return bar[5];
            }
            set
            {
                if (bar[5] != value)
                {
                    bar[5] = value;
                    OnPropertyChanged("Bar6");
                }
            }
        }

        [TaskPane("Bar7Caption", "Bar7Tooltip", "BarGroup", 9, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar7
        {
            get
            {
                return bar[6];
            }
            set
            {
                if (bar[6] != value)
                {
                    bar[6] = value;
                    OnPropertyChanged("Bar7");
                }
            }
        }

        [TaskPane("Bar8Caption", "Bar8Tooltip", "BarGroup", 10, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar8
        {
            get
            {
                return bar[7];
            }
            set
            {
                if (bar[7] != value)
                {
                    bar[7] = value;
                    OnPropertyChanged("Bar8");
                }
            }
        }

        [TaskPane("Bar9Caption", "Bar9Tooltip", "BarGroup", 11, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar9
        {
            get
            {
                return bar[8];
            }
            set
            {
                if (bar[8] != value)
                {
                    bar[8] = value;
                    OnPropertyChanged("Bar9");
                }
            }
        }

        [TaskPane("Bar10Caption", "Bar10Tooltip", "BarGroup", 12, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar10
        {
            get
            {
                return bar[9];
            }
            set
            {
                if (bar[9] != value)
                {
                    bar[9] = value;
                    OnPropertyChanged("Bar10");
                }
            }
        }

        [TaskPane("Bar11Caption", "Bar11Tooltip", "BarGroup", 13, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar11
        {
            get
            {
                return bar[10];
            }
            set
            {
                if (bar[10] != value)
                {
                    bar[10] = value;
                    OnPropertyChanged("Bar11");
                }
            }
        }

        [TaskPane("Bar12Caption", "Bar12Tooltip", "BarGroup", 14, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar12
        {
            get
            {
                return bar[11];
            }
            set
            {
                if (bar[11] != value)
                {
                    bar[11] = value;
                    OnPropertyChanged("Bar12");
                }
            }
        }

        [TaskPane("Bar13Caption", "Bar13Tooltip", "BarGroup", 15, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar13
        {
            get
            {
                return bar[12];
            }
            set
            {
                if (bar[12] != value)
                {
                    bar[12] = value;
                    OnPropertyChanged("Bar13");
                }
            }
        }

        [TaskPane("Bar14Caption", "Bar14Tooltip", "BarGroup", 16, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar14
        {
            get
            {
                return bar[13];
            }
            set
            {
                if (bar[13] != value)
                {
                    bar[13] = value;
                    OnPropertyChanged("Bar14");
                }
            }
        }

        [TaskPane("Bar15Caption", "Bar15Tooltip", "BarGroup", 17, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar15
        {
            get
            {
                return bar[14];
            }
            set
            {
                if (bar[14] != value)
                {
                    bar[14] = value;
                    OnPropertyChanged("Bar15");
                }
            }
        }

        [TaskPane("Bar16Caption", "Bar16Tooltip", "BarGroup", 18, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar16
        {
            get
            {
                return bar[15];
            }
            set
            {
                if (bar[15] != value)
                {
                    bar[15] = value;
                    OnPropertyChanged("Bar16");
                }
            }
        }

        [TaskPane("Bar17Caption", "Bar17Tooltip", "BarGroup", 19, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar17
        {
            get
            {
                return bar[16];
            }
            set
            {
                if (bar[16] != value)
                {
                    bar[16] = value;
                    OnPropertyChanged("Bar17");
                }
            }
        }

        [TaskPane("Bar18Caption", "Bar18Tooltip", "BarGroup", 20, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar18
        {
            get
            {
                return bar[17];
            }
            set
            {
                if (bar[17] != value)
                {
                    bar[17] = value;
                    OnPropertyChanged("Bar18");
                }
            }
        }

        [TaskPane("Bar19Caption", "Bar19Tooltip", "BarGroup", 21, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar19
        {
            get
            {
                return bar[18];
            }
            set
            {
                if (bar[18] != value)
                {
                    bar[18] = value;
                    OnPropertyChanged("Bar19");
                }
            }
        }

        [TaskPane("Bar20Caption", "Bar20Tooltip", "BarGroup", 22, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar20
        {
            get
            {
                return bar[19];
            }
            set
            {
                if (bar[19] != value)
                {
                    bar[19] = value;
                    OnPropertyChanged("Bar20");
                }
            }
        }

        [TaskPane("Bar21Caption", "Bar21Tooltip", "BarGroup", 23, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar21
        {
            get
            {
                return bar[20];
            }
            set
            {
                if (bar[20] != value)
                {
                    bar[20] = value;
                    OnPropertyChanged("Bar21");
                }
            }
        }

        [TaskPane("Bar22Caption", "Bar22Tooltip", "BarGroup", 24, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar22
        {
            get
            {
                return bar[21];
            }
            set
            {
                if (bar[21] != value)
                {
                    bar[21] = value;
                    OnPropertyChanged("Bar22");
                }
            }
        }

        [TaskPane("Bar23Caption", "Bar23Tooltip", "BarGroup", 25, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar23
        {
            get
            {
                return bar[22];
            }
            set
            {
                if (bar[22] != value)
                {
                    bar[22] = value;
                    OnPropertyChanged("Bar23");
                }
            }
        }

        [TaskPane("Bar24Caption", "Bar24Tooltip", "BarGroup", 26, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar24
        {
            get
            {
                return bar[23];
            }
            set
            {
                if (bar[23] != value)
                {
                    bar[23] = value;
                    OnPropertyChanged("Bar24");
                }
            }
        }

        [TaskPane("Bar25Caption", "Bar25Tooltip", "BarGroup", 27, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar25
        {
            get
            {
                return bar[24];
            }
            set
            {
                if (bar[24] != value)
                {
                    bar[24] = value;
                    OnPropertyChanged("Bar25");
                }
            }
        }

        [TaskPane("Bar26Caption", "Bar26Tooltip", "BarGroup", 28, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar26
        {
            get
            {
                return bar[25];
            }
            set
            {
                if (bar[25] != value)
                {
                    bar[25] = value;
                    OnPropertyChanged("Bar26");
                }
            }
        }

        [TaskPane("Bar27Caption", "Bar27Tooltip", "BarGroup", 29, true, ControlType.TextBox, ValidationType.RegEx, "^[0-6]{0,2}$")]
        public string Bar27
        {
            get
            {
                return bar[26];
            }
            set
            {
                if (bar[26] != value)
                {
                    bar[26] = value;
                    OnPropertyChanged("Bar27");
                }
            }
        }

        // Bar Setting
        #endregion

        #region Text options

        [ContextMenu("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", 3, ContextMenuControlType.ComboBox, null, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", "TextOptionsGroup", 3, false, ControlType.ComboBox, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public int UnknownSymbolHandling
        {
            get { return this.unknownSymbolHandling; }
            set
            {
                if ((int)value != unknownSymbolHandling)
                {
                    this.unknownSymbolHandling = (int)value;
                    OnPropertyChanged("UnknownSymbolHandling");   
                }
            }
        }

        [ContextMenu("CaseHandlingCaption", "CaseHandlingTooltip", 4, ContextMenuControlType.ComboBox, null, new string[] { "CaseHandlingList1", "CaseHandlingList2", "CaseHandlingList3" })]
        [TaskPane("CaseHandlingCaption", "CaseHandlingTooltip", "TextOptionsGroup", 4, false, ControlType.ComboBox, new string[] { "CaseHandlingList1", "CaseHandlingList2", "CaseHandlingList3" })]
        public int CaseHandling
        {
            get { return this.caseHandling; }
            set
            {
                if ((int)value != caseHandling)
                {
                    this.caseHandling = (int)value;
                    OnPropertyChanged("CaseHandling");                    
                }
            }
        }

        #endregion

        //Taskpane ende
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
        }

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
