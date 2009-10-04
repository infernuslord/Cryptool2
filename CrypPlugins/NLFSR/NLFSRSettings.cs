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
                HasChanges = true;
            }
        }

        string polynomial;
        [TaskPane("Polynomial", "Define the feedback polynomial. For example x^5 * x^2 + 1.", null, 0, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public string Polynomial
        {
            get { return this.polynomial; }
            set
            {
                this.polynomial = value;
                OnPropertyChanged("Polynomial");
                HasChanges = true;
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
                HasChanges = true;
            }
        }

        private bool noQuickwatch = false;
        [ContextMenu("Do not display Quickwatch", "With this checkbox enabled, no quickwatch will be generated for better performance.", 0, DisplayLevel.Experienced, ContextMenuControlType.CheckBox, null, new string[] { "Display Quickwatch?" })]
        [TaskPane("Do not display Quickwatch", "With this checkbox enabled, no quickwatch will be generated for better performance.", null, 1, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool NoQuickwatch
        {
            get { return this.noQuickwatch; }
            set
            {
                this.noQuickwatch = (bool)value;
                OnPropertyChanged("NoQuickwatch");
                HasChanges = true;
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
                HasChanges = true;
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
                HasChanges = true;
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
                HasChanges = true;
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
                HasChanges = true;
            }
        }

        private bool createDirtyOutputOnFalseClock = false;
        [ContextMenu("Create dirty output on false clock", "With this checkbox enabled, an the output is the dirty (-1) if the clock is set to false.", 2, DisplayLevel.Experienced, ContextMenuControlType.CheckBox, null, new string[] { "Create dirty output?" })]
        [TaskPane("Create dirty output on false clock", "With this checkbox enabled, an the output is the dirty (-1) if the clock is set to false.", "Clock Properties", 2, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool CreateDirtyOutputOnFalseClock
        {
            get { return this.createDirtyOutputOnFalseClock; }
            set
            {
                this.createDirtyOutputOnFalseClock = (bool)value;
                OnPropertyChanged("CreateDirtyOutputOnFalseClock");
                HasChanges = true;
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
