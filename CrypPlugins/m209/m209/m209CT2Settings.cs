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

namespace Cryptool.Plugins.m209
{
    public class ExamplePluginCT2Settings : ISettings
    {
        #region Public Caesar specific interface

        public delegate void m209ReExecute();
        public event m209ReExecute ReExecute;
        #endregion

        #region Private Variables

        private bool hasChanges = false;
        private string startwert = "AAAAAA";
        private bool a1,a2,a3,a4,a5,a6,
                     b1,b2,b3,b4,b5,b6,
                     c1,c2,c3,c4,c5,c6,
                     d1,d2,d3,d4,d5,d6,
                     e1,e2,e3,e4,e5,e6,
                     f1,f2,f3,f4,f5,f6,
                     g1,g2,g3,g4,g5,g6,
                     h1,h2,h3,h4,h5,h6,
                     i1,i2,i3,i4,i5,i6,
                     j1,j2,j3,j4,j5,j6,
                     k1,k2,k3,k4,k5,k6,
                     l1,l2,l3,l4,l5,l6,
                     m1,m2,m3,m4,m5,m6,
                     n1,n2,n3,n4,n5,n6,
                     o1,o2,o3,o4,o5,o6,
                     p1,p2,p3,p4,p5,p6,
                     q1,q2,q3,q4,q5,q6,
                     r1,r2,r3,r4,r5,r6,
                     s1,s2,s3,s4,s5,s7,
                     t1,t2,t3,t4,t5,t6,
                     u1,u2,u3,u4,u5,u6,
                     v1,v2,v3,v4,v5,v6,
                     w1,w2,w3,w4,w5,w6,
                     x1,x2,x3,x4,x5,x6,
                     y1,y2,y3,y4,y5,y6,
                     z1,z2,z3,z4,z5,z6 = false;
        private int selectedAction = 0;
        private bool analyzeKey = false;


        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        //[TaskPane("Startwert", "6 stelliger Startwert", null, 1, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        [ContextMenu("Action", "Select the Algorithm action", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "Encrypt", "Decrypt")]
        [TaskPane("Action", "setAlgorithmActionDescription", null, 1, true, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
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

                if (ReExecute != null) ReExecute();
            }
        }



        [TaskPaneAttribute("Key (Initial rotor setting)", "Please provide the initial rotor setting for each rotor, e.g. ABCD. ", null, 3, true, ControlType.TextBox, ValidationType.RegEx, "^([A-Z}]){6,6}$")]
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
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("A", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinA
        {
            get { return a1; }
            set
            {
                if (value != a1)
                {
                    a1 = value;
                    hasChanges = true;
                    OnPropertyChanged("A1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("B", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinB
        {
            get { return b1; }
            set
            {
                if (value != b1)
                {
                    b1 = value;
                    hasChanges = true;
                    OnPropertyChanged("B1");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("C", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinC
        {
            get { return c1; }
            set
            {
                if (value != c1)
                {
                    c1 = value;
                    hasChanges = true;
                    OnPropertyChanged("C1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("D", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinD
        {
            get { return d1; }
            set
            {
                if (value != d1)
                {
                    d1 = value;
                    hasChanges = true;
                    OnPropertyChanged("D1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("C", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinE
        {
            get { return e1; }
            set
            {
                if (value != e1)
                {
                    e1 = value;
                    hasChanges = true;
                    OnPropertyChanged("E1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("F", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinF
        {
            get { return f1; }
            set
            {
                if (value != f1)
                {
                    f1 = value;
                    hasChanges = true;
                    OnPropertyChanged("F1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("G", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinG
        {
            get { return g1; }
            set
            {
                if (value != g1)
                {
                    g1 = value;
                    hasChanges = true;
                    OnPropertyChanged("G1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("H", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinH
        {
            get { return h1; }
            set
            {
                if (value != h1)
                {
                    h1 = value;
                    hasChanges = true;
                    OnPropertyChanged("H1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("I", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinI
        {
            get { return i1; }
            set
            {
                if (value != i1)
                {
                    i1 = value;
                    hasChanges = true;
                    OnPropertyChanged("I1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("J", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinJ
        {
            get { return j1; }
            set
            {
                if (value != j1)
                {
                    j1 = value;
                    hasChanges = true;
                    OnPropertyChanged("J1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("K", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinK
        {
            get { return k1; }
            set
            {
                if (value != k1)
                {
                    k1 = value;
                    hasChanges = true;
                    OnPropertyChanged("K1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("L", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinL
        {
            get { return l1; }
            set
            {
                if (value != l1)
                {
                    l1 = value;
                    hasChanges = true;
                    OnPropertyChanged("L1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("M", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinM
        {
            get { return m1; }
            set
            {
                if (value != m1)
                {
                    m1 = value;
                    hasChanges = true;
                    OnPropertyChanged("M1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("N", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinN
        {
            get { return n1; }
            set
            {
                if (value != n1)
                {
                    n1 = value;
                    hasChanges = true;
                    OnPropertyChanged("N1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("O", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinO
        {
            get { return o1; }
            set
            {
                if (value != o1)
                {
                    o1 = value;
                    hasChanges = true;
                    OnPropertyChanged("O1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("P", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinP
        {
            get { return p1; }
            set
            {
                if (value != p1)
                {
                    p1 = value;
                    hasChanges = true;
                    OnPropertyChanged("P1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("Q", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinQ
        {
            get { return q1; }
            set
            {
                if (value != q1)
                {
                    q1 = value;
                    hasChanges = true;
                    OnPropertyChanged("Q1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("R", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinR
        {
            get { return r1; }
            set
            {
                if (value != r1)
                {
                    r1 = value;
                    hasChanges = true;
                    OnPropertyChanged("R1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("S", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinS
        {
            get { return s1; }
            set
            {
                if (value != s1)
                {
                    s1 = value;
                    hasChanges = true;
                    OnPropertyChanged("S1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("T", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinT
        {
            get { return t1; }
            set
            {
                if (value != t1)
                {
                    t1 = value;
                    hasChanges = true;
                    OnPropertyChanged("T1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("U", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinU
        {
            get { return u1; }
            set
            {
                if (value != u1)
                {
                    u1 = value;
                    hasChanges = true;
                    OnPropertyChanged("U1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("V", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinV
        {
            get { return v1; }
            set
            {
                if (value != v1)
                {
                    v1 = value;
                    hasChanges = true;
                    OnPropertyChanged("v1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("W", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinW
        {
            get { return w1; }
            set
            {
                if (value != w1)
                {
                    w1 = value;
                    hasChanges = true;
                    OnPropertyChanged("W1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("X", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinX
        {
            get { return x1; }
            set
            {
                if (value != x1)
                {
                    x1 = value;
                    hasChanges = true;
                    OnPropertyChanged("X1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("Y", "Activates the pin",
            "Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinY
        {
            get { return y1; }
            set
            {
                if (value != y1)
                {
                    y1 = value;
                    hasChanges = true;
                    OnPropertyChanged("Y1");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("Z", "Activates the pin","Wheel Options", 6, false, ControlType.CheckBox, "", null)]
        public bool PinZ
        {
            get { return z1; }
            set
            {
                if (value != z1)
                {
                    z1 = value;
                    hasChanges = true;
                    OnPropertyChanged("Z1");
                }
            }
        }

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
