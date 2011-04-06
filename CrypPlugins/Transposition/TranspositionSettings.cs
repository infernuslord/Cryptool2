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
        [ContextMenu( "ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "Encrypt", "Decrypt")]
        [TaskPane( "ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
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
        [ContextMenu( "ReadInCaption", "ReadInTooltip", 2, ContextMenuControlType.ComboBox, null, new string[] { "by row", "by column"})]
        [TaskPane( "ReadInCaption", "ReadInTooltip", null, 2, false, ControlType.ComboBox, new string[] { "by row", "by column" })]
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
        [ContextMenu( "PermutationCaption", "PermutationTooltip", 3, ContextMenuControlType.ComboBox, null, new string[] { "by row", "by column" })]
        [TaskPane( "PermutationCaption", "PermutationTooltip", null, 3, false, ControlType.ComboBox, new string[] { "by row", "by column" })]
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
        [ContextMenu( "ReadOutCaption", "ReadOutTooltip", 4, ContextMenuControlType.ComboBox, null, new string[] { "by row", "by column" })]
        [TaskPane( "ReadOutCaption", "ReadOutTooltip", null, 4, false, ControlType.ComboBox, new string[] { "by row", "by column" })]
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
        [TaskPane( "PresentationSpeedCaption", "PresentationSpeedTooltip", "Presentation", 6, true, ControlType.Slider, 1, 1000)]
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
        [ContextMenu( "NumberCaption", "NumberTooltip", 7, ContextMenuControlType.ComboBox, null, new string[] { "US-ASCII", "hex" })]
        [TaskPane( "NumberCaption", "NumberTooltip", "Presentation", 7, false, ControlType.ComboBox, new string[] { "US-ASCII", "hex" })]
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
