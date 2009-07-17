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

        private bool hasChanges;
        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        #endregion

        //private int scaling = 1;
        private int unknownSymbolHandling = 0;
        //private int trimInputString=0;
        private int caseSensitivity = 0;
        private int grammLength = 1;
        //private int boundaryFragments = 1;

        //[TaskPane("Scaling", "Scale the result? Default is procentual.", "", 0, false, DisplayLevel.Experienced, ControlType.ComboBox, new string[] { "No (absolute)", "Procentual", "log2" })]
        //public int Scaling
        //{
        //    get { return this.scaling; }
        //    set
        //    {
        //        if (value != scaling)
        //        {
        //            HasChanges = true;
        //            scaling = value;
        //        }

        //        OnPropertyChanged("Scaling");
        //    }
        //}
        
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
        [TaskPane("Enter the length of the gramms to be investigated.", "Groups of how many characters should be checked?", "", 1, false, DisplayLevel.Expert, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 100)]
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
        [ContextMenu("Handling of unknown characters", "What should be done with encountered characters at the input which are not in the alphabet?", 4, DisplayLevel.Expert, ContextMenuControlType.ComboBox, null, new string[] { "Don't count", "Count" })]
        [TaskPane("Handling of unknown characters", "What should be done with encountered characters at the input which are not in the alphabet?", null, 4, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] { "Don't count", "Count" })]
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

        /// <summary>
        /// This option is to choose whether additional n-grams shall be used at word boundary for n-grams with n>=2.
        /// Example trigrams for the word "cherry":
        /// che
        /// her
        /// err
        /// rry
        /// The following fragments at word boundary may be included optionally:
        /// __c
        /// _ch
        /// ry_
        /// y__
        /// The underline char represents a whitespace.
        /// </summary>
        //[TaskPane("Word boundary fragments", "Include additional fragments with whitespaces at word boundary? Only relevant for length >= 2.", "", 10, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] { "Include fragments", "No fragments at boundary" })]
        //public int BoundaryFragments
        //{
        //    get { return this.boundaryFragments; }
        //    set
        //    {
        //        if (value != boundaryFragments)
        //        {
        //            HasChanges = true;
        //            boundaryFragments = value;
        //        }

        //        OnPropertyChanged("BoundaryFragments");
        //    }
        //}
        
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
