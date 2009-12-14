using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Transposition
{
    class TranspositionSettings : ISettings
    {
        # region private variables

        private Boolean hasChanges = false;
        private int selectedAction = 0;

        private ReadInMode selectedReadIn = ReadInMode.byRow;
        private PermutationMode selectedPermutation = PermutationMode.byColumn;
        private ReadOutMode selectedReadOut = ReadOutMode.byColumn;
        private int Presentation_Speed = 1;
        //private int hexmode = 0;
        private NumberMode selectedNumberMode = NumberMode.asChar;
        
        # endregion

        #region public enums

        public enum ReadInMode { byRow = 0, byColumn = 1};
        public enum PermutationMode { byRow = 0, byColumn = 1 };
        public enum ReadOutMode { byRow = 0, byColumn = 1 };
        public enum NumberMode {asChar = 0, asHex = 1};

        # endregion

        #region ISettings Member

        [PropertySaveOrder(0)]
        public bool HasChanges
        {
            get { return hasChanges; }
            set { this.hasChanges = value; }
        }

        # endregion

        # region Settings

        [PropertySaveOrder(1)]
        [ContextMenu("Action", "Select the Algorithm action", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "Encrypt", "Decrypt")]
        [TaskPane("Action", "Select the Algorithm action", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public int Action
        {
            get { return this.selectedAction; }
            set
            {
                if (value != selectedAction) HasChanges = true;
                this.selectedAction = value;
                OnPropertyChanged("Action");
            }
        }

        [PropertySaveOrder(2)]
        [ContextMenu("Read in", "Select read in mode", 2, DisplayLevel.Expert, ContextMenuControlType.ComboBox, null, new string[] { "by row", "by column"})]
        [TaskPane("Read in", "Select read in mode", null, 2, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] { "by row", "by column" })]
        public int ReadIn
        {
            get { return (int) this.selectedReadIn; }
            set
            {
                if ((ReadInMode)value != selectedReadIn) HasChanges = true;
                this.selectedReadIn = (ReadInMode)value;
                OnPropertyChanged("ReadIn");
            }
        }

        [PropertySaveOrder(3)]
        [ContextMenu("Permutation", "Select permutation type", 3, DisplayLevel.Expert, ContextMenuControlType.ComboBox, null, new string[] { "by row", "by column" })]
        [TaskPane("Permutation", "Select permutation type", null, 3, false,DisplayLevel.Expert, ControlType.ComboBox, new string[] { "by row", "by column" })]
        public int Permutation
        {
            get { return (int)this.selectedPermutation; }
            set
            {
                if ((PermutationMode)value != selectedPermutation) HasChanges = true;
                this.selectedPermutation= (PermutationMode)value;
                OnPropertyChanged("Permutation");
            }
        }

        [PropertySaveOrder(4)]
        [ContextMenu("Read out", "Select read out type", 4, DisplayLevel.Expert, ContextMenuControlType.ComboBox, null, new string[] { "by row", "by column" })]
        [TaskPane("Read out", "Select read out type", null, 4, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] { "by row", "by column" })]
        public int ReadOut
        {
            get { return (int)this.selectedReadOut; }
            set
            {
                if ((ReadOutMode)value != selectedReadOut) HasChanges = true;
                this.selectedReadOut= (ReadOutMode)value;
                OnPropertyChanged("ReadOut");
            }
        }

        [PropertySaveOrder(5)]
        [TaskPane("Presentation Speed", "Change the pace of the Presentation", null, 5, true, DisplayLevel.Expert, ControlType.Slider, 1, 1000)]
        public int PresentationSpeed
        {
            get { return (int)Presentation_Speed; }
            set
            {
                if ((value) != Presentation_Speed) hasChanges = true;
                this.Presentation_Speed = value;
                OnPropertyChanged("Value");
            }
        }

        [PropertySaveOrder(6)]
        [ContextMenu("Number Representation Mode", "Select a mode of Representation", 6, DisplayLevel.Expert, ContextMenuControlType.ComboBox, null, new string[] { "char", "hex" })]
        [TaskPane("Number Representation Mode", "Select a mode of Representation", null, 6, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] { "char", "hex" })]
        public int Number
        {
            get { return (int)this.selectedNumberMode;}
            set
            {
                if ((NumberMode)value != selectedNumberMode) HasChanges = true;
                this.selectedNumberMode = (NumberMode)value;
                OnPropertyChanged("NumberMode");
            }
        }



        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
