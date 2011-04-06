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

        [TaskPane( "initNLFSRCaption", "initNLFSRTooltip", null, 0, false, ControlType.Button)]
        public void initNLFSR()
        {
            OnPropertyChanged("InitNLFSR");
        }
        
        private int rounds = 1; //how many bits will be generated
        //[ContextMenu("Rounds", "How many bits shall be generated?", 1, ContextMenuControlType.ComboBox, new int[] { 10, 50, 100 }, "10 bits", "50 bits", "100 bits")]
        //[TaskPane("Rounds", "How many bits shall be generated?", null, 1, false, ControlType.TextBox)]
        [TaskPane( "RoundsCaption", "RoundsTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
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
        [TaskPane( "PolynomialCaption", "PolynomialTooltip", null, 0, false, ControlType.TextBox)]
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
        [TaskPane( "SeedCaption", "SeedTooltip", null, 1, false, ControlType.TextBox)]
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
        [ContextMenu( "NoQuickwatchCaption", "NoQuickwatchTooltip", 0, ContextMenuControlType.CheckBox, null, new string[] { "Display Quickwatch?" })]
        [TaskPane( "NoQuickwatchCaption", "NoQuickwatchTooltip", null, 3, true, ControlType.CheckBox, "", null)]
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
        [ContextMenu( "SaveCurrentStateCaption", "SaveCurrentStateTooltip", 0, ContextMenuControlType.CheckBox, null, new string[] { "Save current state?" })]
        [TaskPane( "SaveCurrentStateCaption", "SaveCurrentStateTooltip", null, 3, true, ControlType.CheckBox, "", null)]
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
        [ContextMenu( "UseClockingBitCaption", "UseClockingBitTooltip", 0, ContextMenuControlType.CheckBox, null, new string[] { "Generate additional output bit?" })]
        [TaskPane( "UseClockingBitCaption", "UseClockingBitTooltip", "Additional Output Bit", 0, false, ControlType.CheckBox, "", null)]
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
        [TaskPane( "ClockingBitCaption", "ClockingBitTooltip", "Additional Output Bit", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
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
        [ContextMenu( "UseBoolClockCaption", "UseBoolClockTooltip", 0, ContextMenuControlType.CheckBox, null, new string[] { "Use Boolean clock?" })]
        [TaskPane( "UseBoolClockCaption", "UseBoolClockTooltip", "Clock Properties", 0, false, ControlType.CheckBox, "", null)]
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
        [ContextMenu( "AlwaysCreateOutputCaption", "AlwaysCreateOutputTooltip", 1, ContextMenuControlType.CheckBox, null, new string[] { "Always generate output?" })]
        [TaskPane( "AlwaysCreateOutputCaption", "AlwaysCreateOutputTooltip", "Clock Properties", 1, false, ControlType.CheckBox, "", null)]
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
