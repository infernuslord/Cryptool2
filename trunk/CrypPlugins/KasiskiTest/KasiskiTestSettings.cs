using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Analysis;
using System.ComponentModel;

namespace KasiskiTest
{
    class KasiskiTestSettings : ISettings
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

        public int caseSensitivity = 0;
        public int unknownSymbolHandling = 1;
        public int grammLength = 3;
        public int factorSize = 20;
        /// <summary>
        /// Visible setting how to deal with alphabet case. 0 = case insentive, 1 = case sensitive
        /// </summary>
       
        [PropertySaveOrder(1)]
        [TaskPane("Gramm Length (integer)", "Enter maximum gramm length to be examined. Minimum/default = 3", "", 1, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 100)]
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
        [PropertySaveOrder(2)]
        [ContextMenu("Unknown symbol handling", "What should be done with encountered characters at the input which are not in the alphabet?", 4, DisplayLevel.Expert, ContextMenuControlType.ComboBox, null, new string[] { "Ignore (leave unmodified)", "Remove" })]
        [TaskPane("Unknown symbol handling", "What should be done with encountered characters at the input which are not in the alphabet?", null, 4, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] { "Ignore (leave unmodified)", "Remove" })]
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
        [PropertySaveOrder(3)]
        [TaskPane("Maximum Factor Size (integer)", "Enter maximum factor/key size to be examined. default = 20", "", 1, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 100)]
        public int FactorSize
        {
            get { return this.factorSize; }
            set
            {
                if (value != factorSize)
                {
                    HasChanges = true;
                    factorSize = value;
                }
            }
        }


        [PropertySaveOrder(4)]
        [ContextMenu("Case Sensitivity ", "is a==A ? \n Pay attention to the Alphabet used in the Cyphertext as well as the 'Case Sensitivity' of Vigenere cypher if the case ", 4, DisplayLevel.Expert, ContextMenuControlType.ComboBox, null, new string[] { "Yes 'a' should equal 'A'", "No 'a' should not equal 'A'" })]
        [TaskPane("Case Sensitivity ", "is a==A ? \n Pay attention to the Alphabet used in the Cyphertext as well as the 'Case Sensitivity' of Vigenere cypher if the case ", null, 4, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] { "Yes 'a' should equal 'A'", "No 'a' should not equal 'A'" })]
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

                OnPropertyChanged("Case Sensitive");
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
