using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.CaesarAnalysisHelper
{
    class CaesarAnalysisHelperSettings : ISettings
    {
        private char frequentChar = 'e';

        [PropertySaveOrder(0)]
        [TaskPane("Frequent Char", "The most frequent char in the text's language.", null, 4, false, DisplayLevel.Beginner, ControlType.TextBox, ValidationType.RegEx, "^([a-z]){1,1}$")]
        public char FrequentChar
        {
            get { return frequentChar; }
            set
            {
                frequentChar = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("FrequentChar"));
            }
        }

        [PropertySaveOrder(1)]
        public bool HasChanges
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}