using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using System.ComponentModel;


namespace Cryptool.VigenereAnalyser
{
    public enum Mode
    {
        Vigenere = 0,
        Autokey = 1
    };

    public enum Language
    {
        Englisch = 0,
        German = 1
    };

    public enum UnknownSymbolHandlingMode
    {
        Ignore = 0,
        Remove = 1,
        Replace = 2
    };

    public enum KeyStyle
    {
        Random = 0,
        NaturalLanguage=1        
    }

    class VigenereAnalyserSettings : ISettings
    {      
        private Mode _mode = Mode.Vigenere;
        private int _fromKeylength;
        private int _toKeyLength = 20;
        private Language _language = Language.Englisch;
        private bool _greedy;
        private int _restarts = 50;
        private KeyStyle _keyStyle;

        public void Initialize()
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [TaskPane("ModeCaption", "ModeTooltip", null, 1, false, ControlType.ComboBox, new []{"Vigenere", "VigenereAutokey"})]
        public Mode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                if (value != _mode)
                {
                    _mode = value;
                    OnPropertyChanged("Mode");
                }
            }
        }

        [TaskPane("FromKeylengthCaption", "FromKeylengthTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int FromKeylength
        {
            get
            {
                return _fromKeylength;
            }
            set
            {
                if (value != _fromKeylength)
                {
                    _fromKeylength = value;
                    OnPropertyChanged("FromKeyLength");
                }
            }
        }

        [TaskPane("ToKeyLengthCaption", "ToKeyLengthTooltip", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int ToKeyLength
        {
            get
            {
                return _toKeyLength;
            }
            set
            {
                if (value != _toKeyLength)
                {
                    _toKeyLength = value;
                    OnPropertyChanged("ToKeyLength");
                }
            }
        }

        [TaskPane("RestartsCaption", "RestartsTooltip", null, 4, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 10000)]
        public int Restarts
        {
            get
            {
                return _restarts;
            }
            set
            {
                if (value != _restarts)
                {
                    _restarts = value;
                    OnPropertyChanged("Restarts");
                }
            }
        }

        [TaskPane("LanguageCaption", "LanguageTooltip", null, 5, false, ControlType.ComboBox, new string[]{"Englisch","German"})]
        public Language Language
        {
            get
            {
                return _language;
            }
            set
            {
                if (value != _language)
                {
                    _language = value;
                    OnPropertyChanged("Language");
                }
            }
        }

        [TaskPane("GreedyCaption", "GreedyTooltip", null, 6, false, ControlType.CheckBox)]
        public bool Greedy
        {
            get
            {
                return _greedy;
            }
            set
            {
                if (value != _greedy)
                {
                    _greedy = value;
                    OnPropertyChanged("Greedy");
                }
            }
        }

        [TaskPane("KeyStyleCaption", "KeyStyleTooltip", null, 7, false, ControlType.ComboBox, new string[] { "Random", "NaturalLanguage" })]
        public KeyStyle KeyStyle
        {
            get
            {
                return _keyStyle;
            }
            set
            {
                if (value != _keyStyle)
                {
                    _keyStyle = value;
                    OnPropertyChanged("KeyStyle");
                }
            }
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
