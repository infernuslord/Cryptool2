using System;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.CaesarAnalysisHelper
{
    public enum Language
    {
        German,
        English,
        French,
        Spanish
    }

    class CaesarAnalysisHelperSettings : ISettings
    {
        internal char FrequentChar = 'e';

        private Language Lang = Language.German;

        [ContextMenu("Language", "The Language", 0, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new[] { "German", "English", "French", "Spanish" })]
        [TaskPane("Language", "The text's language.", null, 0, false, DisplayLevel.Beginner, ControlType.ComboBox, new[] { "German", "English", "French", "Spanish" })]
        public string TextLanguage
        {
            get
            {
                return Lang.ToString();
            }
            set
            {
                try
                {
                    Lang = (Language)Enum.Parse(typeof(Language), value);
                    switch (Lang)
                    {
                        case Language.German:
                        case Language.English:
                        case Language.French:
                        case Language.Spanish:
                            FrequentChar = 'e';
                            break;
                        default:
                            break;
                    }
                    OnPropertyChanged("TextLanguage");
                }
                catch (Exception)
                {
                }
            }
        }

        private bool hasChanges;
        public bool HasChanges
        {
            get { return hasChanges; }
            set
            {
                if (value != hasChanges)
                {
                    hasChanges = value;
                    OnPropertyChanged("HasChanges");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            if (name.ToLower() != "haschanges")
                HasChanges = true;
        }
    }
}