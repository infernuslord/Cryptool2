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

namespace Cryptool.Trivium
{
    public class TriviumSettings : ISettings
    {
        #region ISettings Members

        private bool hasChanges = false;

        private int keystreamLength = 32;
        [TaskPane("Length of keystream", "How many bits of keystream in bits should be generated? Must be a multiple of 32.", null, 0, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int KeystreamLength
        {
            get { return this.keystreamLength; }
            set
            {
                this.keystreamLength = value;
                //OnPropertyChanged("KeystreamLength");
                HasChanges = true;
            }
        }

        private int initRounds = 1152;
        [TaskPane("Initialization rounds", "How many init rounds should be done? Default is 1152.", null, 1, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int InitRounds
        {
            get { return this.initRounds; }
            set
            {
                this.initRounds = value;
                //OnPropertyChanged("InitRounds");
                HasChanges = true;
            }
        }

        private bool useByteSwapping = false;
        [ContextMenu("Use byte swapping", "With this checkbox enabled, output bytes will be swapped.", 1, DisplayLevel.Experienced, ContextMenuControlType.CheckBox, null, new string[] { "Use byte swapping?" })]
        [TaskPane("Use byte swapping", "With this checkbox enabled, output bytes will be swapped.", null, 2, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool UseByteSwapping
        {
            get { return this.useByteSwapping; }
            set
            {
                this.useByteSwapping = (bool)value;
                //OnPropertyChanged("UseByteSwapping");
                HasChanges = true;
            }
        }

        private bool hexOutput = false;
        [ContextMenu("Generate Hex output", "With this checkbox enabled, output bytes will be displayed in hex.", 2, DisplayLevel.Experienced, ContextMenuControlType.CheckBox, null, new string[] { "Display as hex?" })]
        [TaskPane("Generate Hex output", "With this checkbox enabled, output bytes will be displayed in hex.", null, 3, false, DisplayLevel.Beginner, ControlType.CheckBox, "", null)]
        public bool HexOutput
        {
            get { return this.hexOutput; }
            set
            {
                this.hexOutput = (bool)value;
                //OnPropertyChanged("HexOutput");
                HasChanges = true;
            }
        }

        private string inputKey = string.Empty;
        [TaskPane("Key (Input for Cube Attack)", "Must be 10 bytes (80 bit) in Hex", null, 4, false, DisplayLevel.Beginner, ControlType.TextBox, null)]
        public string InputKey
        {
            get { return inputKey; }
            set
            {
                this.inputKey = value;
                //OnPropertyChanged("InputKey");
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

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
