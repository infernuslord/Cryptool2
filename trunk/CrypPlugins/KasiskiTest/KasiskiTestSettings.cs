using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.KasiskiTest
{
   public  class KasiskiTestSettings : ISettings
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
        
       
        [PropertySaveOrder(1)]
        [TaskPane( "GrammLengthCaption", "GrammLengthTooltip", "", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 100)]
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
        [ContextMenu("RemoveUnknownSymbolsCaption", "RemoveUnknownSymbolsTooltip", 4, ContextMenuControlType.ComboBox, null, new string[] { "RemoveUnknownSymbolsList1", "RemoveUnknownSymbolsList2" })]
        [TaskPane("RemoveUnknownSymbolsCaption", "RemoveUnknownSymbolsTooltip", null, 4, false, ControlType.ComboBox, new string[] { "RemoveUnknownSymbolsList1", "RemoveUnknownSymbolsList2" })]
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
        [TaskPane( "FactorSizeCaption", "FactorSizeTooltip", "", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 100)]
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
        [ContextMenu("CaseSensitivityCaption", "CaseSensitivityTooltip", 4, ContextMenuControlType.ComboBox, null, new string[] { "CaseSensitivityList1", "CaseSensitivityList2" })]
        [TaskPane("CaseSensitivityCaption", "CaseSensitivityTooltip", null, 4, false, ControlType.ComboBox, new string[] { "CaseSensitivityList1", "CaseSensitivityList2" })]
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
