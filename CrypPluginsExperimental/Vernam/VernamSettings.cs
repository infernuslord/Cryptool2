
using System;
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.Vernam
{
    
    public class VernamSettings : ISettings
    {
      

        public delegate void VernamLogMessage(string msg, NotificationLevel loglevel);
        public enum CipherMode { Encrypt = 0, Decrypt = 1 };
        public enum UnknownSymbolHandlingMode { Ignore = 0, Remove = 1, Replace = 2 };

        public CipherMode selectedCipherMode = CipherMode.Encrypt;
        public string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789";
        private UnknownSymbolHandlingMode unknownSymbolHandling = UnknownSymbolHandlingMode.Ignore;
        public event VernamLogMessage LogMessage;
        

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [PropertySaveOrder(4)]
        [TaskPane("CipherMode", "Change Mode to encryption or decryption", null, 1, true, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public CipherMode Action
        {
            get
            {
                return this.selectedCipherMode;
            }
            set
            {
                if (value != selectedCipherMode)
                {
                    this.selectedCipherMode = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [PropertySaveOrder(5)]
        [TaskPane("Unknown Symbol Handle", "Set what to do with unknown symbols", null, 4, true, ControlType.ComboBox, new string[] { "Ignore", "Remove", "Replace" })]
        public UnknownSymbolHandlingMode UnknownSymbolHandling
        {
            get { return this.unknownSymbolHandling; }
            set
            {
                if (value != unknownSymbolHandling)
                {
                    this.unknownSymbolHandling = value;
                    OnPropertyChanged("UnknownSymbolHandling");
                }
            }
        }

        [PropertySaveOrder(6)]
        [TaskPane("Input Alphabet", "fill out to specify alphabet", null, 6, true, ControlType.TextBox, "")]
        public string AlphabetSymbols
        {
            get { return this.alphabet; }
            set
            {
                string a = removeEqualChars(value);
                if (a.Length == 0) 
                {
                    OnLogMessage("no alphabet. using: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                }
                else if (!alphabet.Equals(a))
                {
                    this.alphabet = a;
                    OnLogMessage("new alphabet: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                    OnPropertyChanged("AlphabetSymbols");
                }
            }
        }

        private string removeEqualChars(string value)
        {
            int length = value.Length;

            for (int i = 0; i < length; i++)
            {
                for (int j = i + 1; j < length; j++)
                {
                    if (value[i] == value[j])
                    {
                        OnLogMessage("Removing duplicate letter: \'" + value[j] + "\' from alphabet!", NotificationLevel.Warning);
                        value = value.Remove(j, 1);
                        j--;
                        length--;
                    }
                }
            }

            return value;
        }

        private void OnLogMessage(string msg, NotificationLevel level)
        {
            if (LogMessage != null)
                LogMessage(msg, level);
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
