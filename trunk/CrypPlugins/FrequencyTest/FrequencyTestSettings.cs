using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.FrequencyTest
{
    public class FrequencyTestSettings : ISettings
    {
        #region ISettings Members
        
        public bool HasChanges
        {
            get
            {
                return false;
                //throw new NotImplementedException();
            }
            set
            {
                //throw new NotImplementedException();
            }
        }

        #endregion
        public int unknownSymbolHandling = 0;
        //private int trimInputString=0;
        private int caseSensitivity = 0;
        private int grammLength = 1;
        
        /// <summary>
        /// Visible setting how to deal with alphabet case. 0 = case insentive, 1 = case sensitive
        /// </summary>
        [PropertySaveOrder(1)]
        [ContextMenu("Alphabet case sensitivity", "Should upper and lower case be treated differently? (Should a == A)", 7, DisplayLevel.Expert, ContextMenuControlType.ComboBox, null, new string[] { "Case insensitive", "Case sensitive" })]
        [TaskPane("Alphabet case sensitivity", "Should upper and lower case be treated differently? (Should a == A)", "", 7,false, DisplayLevel.Expert, ControlType.ComboBox, new string[] { "Case insensitive", "Case sensitive" })]
        public int CaseSensitivity
        {
            get { return this.caseSensitivity; }
            set
            {
                if (value != caseSensitivity)
                {
                    HasChanges = true;
                    caseSensitivity = value;
                }

                OnPropertyChanged("CaseSensitivity");
            }
        }
        [PropertySaveOrder(2)]
        [TaskPane("Gramm Length (integer)", "Enter Gramm Length.", "", 1,false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 100)]
        public int GrammLength
        {
            get { return this.grammLength; }
            set
            {
                if (value != grammLength)
                {
                    HasChanges = true;
                    grammLength = value;
                }
            }
        }
        [PropertySaveOrder(3)]
        [ContextMenu("Unknown symbol handling", "What should be done with encountered characters at the input which are not in the alphabet?", 4, DisplayLevel.Expert, ContextMenuControlType.ComboBox, null, new string[] { "Ignore (leave unmodified)", "Remove" })]
        [TaskPane("Unknown symbol handling", "What should be done with encountered characters at the input which are not in the alphabet?", null, 4, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] { "Ignore (leave unmodified)", "Remove"})]
        public int RemoveUnknownSymbols
        {
            get { return this.unknownSymbolHandling; }
            set
            {
                if (value != unknownSymbolHandling)
                {
                    HasChanges = true;
                    unknownSymbolHandling = value;
                }

                OnPropertyChanged("RemoveUnknownSymbols");
            }
        }
        
        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

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
