/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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

using System.IO;
using System.Security.Cryptography;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using System.Runtime.CompilerServices;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.Remoting.Contexts;
// for Visibility
using System.Windows;

namespace Cryptool.NLFSR
{
    public class NLFSRSettings : ISettings
    {
        #region ISettings Members

        private bool hasChanges = false;

        private string currentState;
        public string CurrentState
        {
            get { return currentState; }
            set
            {
                if (value != currentState)
                {
                    currentState = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPane("Draw NLFSR", "Initializes NLFSR and draws the presentation. This is used to view the NLFSR before pressing play.", null, 0, false, DisplayLevel.Beginner, ControlType.Button)]
        public void initNLFSR()
        {
            OnPropertyChanged("InitNLFSR");
        }
        
        private int rounds = 1; //how many bits will be generated
        //[ContextMenu("Rounds", "How many bits shall be generated?", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] { 10, 50, 100 }, "10 bits", "50 bits", "100 bits")]
        //[TaskPane("Rounds", "How many bits shall be generated?", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        [TaskPane("Number of rounds", "How many bits shall be generated? Note: This only applies if no boolean clock is used.", null, 2, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int Rounds
        {
            get { return this.rounds; }
            set { 
                this.rounds = value;
                OnPropertyChanged("Rounds");
                if (value != rounds) HasChanges = true;
            }
        }

        string polynomial;
        [TaskPane("Feedback function", "Define the feedback function. For example x5 * x2 + 1.", null, 0, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public string Polynomial
        {
            get { return this.polynomial; }
            set
            {
                this.polynomial = value;
                OnPropertyChanged("Polynomial");
                if (value != polynomial) HasChanges = true;
            }
        }

        string seed;
        [TaskPane("Seed", "Define the seed of the LFSR. For example 11100", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public string Seed
        {
            get { return this.seed; }
            set
            {
                this.seed = value;
                OnPropertyChanged("Seed");
                if (value != seed) HasChanges = true;
            }
        }

        private bool noQuickwatch = false;
        [ContextMenu("Do not display Quickwatch", "With this checkbox enabled, no quickwatch will be generated for better performance.", 0, DisplayLevel.Experienced, ContextMenuControlType.CheckBox, null, new string[] { "Display Quickwatch?" })]
        [TaskPane("Do not display Quickwatch", "With this checkbox enabled, no quickwatch will be generated for better performance.", null, 3, true, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool NoQuickwatch
        {
            get { return this.noQuickwatch; }
            set
            {
                this.noQuickwatch = (bool)value;
                OnPropertyChanged("NoQuickwatch");
                if ((bool)value != noQuickwatch) HasChanges = true;
            }
        }

        private bool saveCurrentState = false;
        [ContextMenu("Save the state of the NLFSR", "With this checkbox enabled, the current state will be restored after opening a .cte.", 0, DisplayLevel.Experienced, ContextMenuControlType.CheckBox, null, new string[] { "Save current state?" })]
        [TaskPane("Save the state of the NLFSR", "With this checkbox enabled, the current state will be restored after opening a .cte.", null, 3, true, DisplayLevel.Experienced, ControlType.CheckBox, "", null)]
        public bool SaveCurrentState
        {
            get { return this.saveCurrentState; }
            set
            {
                this.saveCurrentState = (bool)value;
                OnPropertyChanged("SaveCurrentState");
                if ((bool)value != saveCurrentState) HasChanges = true;
            }
        }

        private bool useClockingBit = false;
        [ContextMenu("Generate add. output bit", "With this checkbox enabled, the additional output bit will be generated.", 0, DisplayLevel.Experienced, ContextMenuControlType.CheckBox, null, new string[] { "Generate additional output bit?" })]
        [TaskPane("Generate add. output bit", "With this checkbox enabled, the additional output bit will be generated.", "Additional Output Bit", 0, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool UseClockingBit
        {
            get { return this.useClockingBit; }
            set
            {
                this.useClockingBit = (bool)value;
                OnPropertyChanged("UseClockingBit");
                if ((bool)value != useClockingBit) HasChanges = true;
                if (this.useClockingBit)
                    SettingChanged("ClockingBit", Visibility.Visible);
                else
                    SettingChanged("ClockingBit", Visibility.Collapsed);
            }
        }
        
        private int clockingBit = 0;
        [TaskPane("Additional output bit #", "Which bit shall be generated as an additional output? For example as an clocking bit.", "Additional Output Bit", 1, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int ClockingBit
        {
            get { return this.clockingBit; }
            set
            {
                this.clockingBit = value;
                OnPropertyChanged("ClockingBit");
                if (value != clockingBit) HasChanges = true;
            }
        }

        private bool useBoolClock = false;
        [ContextMenu("Use BoolClock", "With this checkbox enabled, BoolClock will be used.", 0, DisplayLevel.Experienced, ContextMenuControlType.CheckBox, null, new string[] { "Use Boolean clock?" })]
        [TaskPane("Use BoolClock", "With this checkbox enabled, BoolClock will be used.", "Clock Properties", 0, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool UseBoolClock
        {
            get { return this.useBoolClock; }
            set
            {
                this.useBoolClock = (bool)value;
                OnPropertyChanged("UseBoolClock");
                if ((bool)value != useBoolClock) HasChanges = true;
                if (this.useBoolClock)
                    SettingChanged("Rounds", Visibility.Collapsed);
                else
                    SettingChanged("Rounds", Visibility.Visible);
            }
        }

        private bool alwaysCreateOutput = false;
        [ContextMenu("Always create output", "With this checkbox enabled, an output will be generated, even though the clock is set to false. The output bit will be the bit from the last clock cycle.", 1, DisplayLevel.Experienced, ContextMenuControlType.CheckBox, null, new string[] { "Always generate output?" })]
        [TaskPane("Always create output", "With this checkbox enabled, an output will be generated, even though the clock is set to false. The output bit will be the bit from the last clock cycle.", "Clock Properties", 1, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool AlwaysCreateOutput
        {
            get { return this.alwaysCreateOutput; }
            set
            {
                this.alwaysCreateOutput = (bool)value;
                OnPropertyChanged("AlwaysCreateOutput");
                if ((bool)value != alwaysCreateOutput) HasChanges = true;
            }
        }

        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // this event is for disabling stuff in the settings pane
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            }
        }

        // these 2 functions are for disabling stuff in the settings pane
        private void SettingChanged(string setting, Visibility vis)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
            }
        }

        private void SettingChanged(string setting, Visibility vis, TaskPaneAttribute tpa)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis, tpa)));
            }
        }

        #endregion
    }
}
