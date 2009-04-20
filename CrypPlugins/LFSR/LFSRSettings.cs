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

namespace Cryptool.LFSR
{
    public class LFSRSettings : ISettings
    {
        #region ISettings Members

        private bool hasChanges = false;
        
        private int rounds = 4; //how many bits will be generated
        //[ContextMenu("Rounds", "How many bits shall be generated?", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] { 10, 50, 100 }, "10 bits", "50 bits", "100 bits")]
        //[TaskPane("Rounds", "How many bits shall be generated?", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        [TaskPane("Rounds", "How many bits shall be generated? Note: This only applies if no boolean clock is used.", null, 0, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int Rounds
        {
            get { return this.rounds; }
            set { 
                this.rounds = value;
                OnPropertyChanged("Rounds");
                HasChanges = true;
            }
        }

        private int clockingBit = 0;
        [TaskPane("Clocking Bit", "Which bit shall be generated as an additional output? For example as an clocking bit.", null, 0, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
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

        private bool useClockingBit = false;
        [ContextMenu("Use ClockingBit", "With this checkbox enabled, the clocking bit will be generated.", 0, DisplayLevel.Experienced, ContextMenuControlType.CheckBox, null, new string[] { "Use clocking bit?" })]
        [TaskPane("Use ClockingBit", "With this checkbox enabled, clocking bit will be generated.", null, 1, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool UseClockingBit
        {
            get { return this.useClockingBit; }
            set
            {
                this.useClockingBit = (bool)value;
                OnPropertyChanged("UseClockingBit");
                HasChanges = true;
            }
        }

        private bool useBoolClock = false;
        [ContextMenu("Use BoolClock", "With this checkbox enabled, BoolClock will be used.", 0, DisplayLevel.Experienced, ContextMenuControlType.CheckBox, null, new string[] { "Use Boolean clock?" })]
        [TaskPane("Use BoolClock", "With this checkbox enabled, BoolClock will be used.", null, 1, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool UseBoolClock
        {
            get { return this.useBoolClock; }
            set
            {
                this.useBoolClock = (bool)value;
                OnPropertyChanged("UseBoolClock");
                HasChanges = true;
            }
        }

        private bool alwaysCreateOutput = false;
        [ContextMenu("Always create output", "With this checkbox enabled, an output will be generated, even though the clock is set to false. The output bit will be the bit from the last clock cycle.", 0, DisplayLevel.Experienced, ContextMenuControlType.CheckBox, null, new string[] { "Always generate output?" })]
        [TaskPane("Always create output", "With this checkbox enabled, an output will be generated, even though the clock is set to false. The output bit will be the bit from the last clock cycle.", null, 1, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
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

        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
