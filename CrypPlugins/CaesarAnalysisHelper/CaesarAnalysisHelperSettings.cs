using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.CaesarAnalysisHelper
{
    class CaesarAnalysisHelperSettings : ISettings
    {
        public bool HasChanges
        {
            get;
            set;
        }

        private char frequentChar = 'e';

        [TaskPane("Frequent Char", "The most frequent char in the text's language.", null, 4, false, DisplayLevel.Beginner, ControlType.TextBox, ValidationType.RegEx, "^([a-z]){1,1}$")]
        public char FrequentChar
        {
            get { return frequentChar; }
            set
            {
                frequentChar = value;
                PropertyChanged(this, new PropertyChangedEventArgs("FrequentChar"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}