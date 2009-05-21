using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using System.Collections.Generic;
using Cryptool.PluginBase.Analysis;

namespace Cryptool.CaesarAnalysisHelper
{
    [Author("Fabian Enkler", "", "", "")]
    [PluginInfo(false, "CaesarAnalysisHelper", "This plugin is designed in order to make a cryptanalysis of a caesarcipher based on the frequency test.", "", "CaesarAnalysisHelper/icon.png")]
    public class CaesarAnalysisHelper : IAnalysisMisc
    {
        private readonly CaesarAnalysisHelperSettings settings;

        public event PropertyChangedEventHandler PropertyChanged;
        // disable the warning, if we don't intend to use these events..
#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
#pragma warning restore

        public CaesarAnalysisHelper()
        {
            this.settings = new CaesarAnalysisHelperSettings();
        }

        public ISettings Settings
        {
            get { return settings; }
        }

        private string encryptedText;
        [PropertyInfo(Direction.Input, "Encrypted text", "The caesar encrpyted text", null, false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string EncryptedText
        {
            get { return encryptedText; }
            set
            {
                encryptedText = value;
                OnPropertyChanged("EncryptedText");
            }
        }

        private string frequencyList = string.Empty;
        [PropertyInfo(Direction.Input, "Frequency List", "This is the analysis input from the frequency test.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text,
            null)]
        public string FrequencyList
        {
            get { return frequencyList; }
            set
            {
                frequencyList = value;
                OnPropertyChanged("FrequencyList");
            }
        }

        /*private string dictionary;
        [PropertyInfo(Direction.Input, "Dictionary", "Dictionary for Bruteforce attack", null, false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string Dictionary
        {
            get { return dictionary; }
            set
            {
                dictionary = value;
                OnPropertyChanged("Dictionary");
            }
        }

        private string bruteForceDecryptedText;
        [PropertyInfo(Direction.Input, "Bruteforce decrypted text", "This the text, which was decrypted by the caesar plugin.", null, false, false, DisplayLevel.Professional, QuickWatchFormat.Text, null)]
        public string BruteForceDecryptedText
        {
            get { return bruteForceDecryptedText; }
            set
            {
                bruteForceDecryptedText = value;
                OnPropertyChanged("BruteForceDecryptedText");
            }
        }*/

        private int key;
        [PropertyInfo(Direction.Output, "Key", "This is the estimated key.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public int Key
        {
            get
            {
                return key;
            }
        }

        /*private string bruteForceEncryptedText;
        [PropertyInfo(Direction.Output, "Bruteforce encrypted text", "This the text, which should be encrypted by the caesar plugin with bruteforce.", null, false, false, DisplayLevel.Professional, QuickWatchFormat.Text, null)]
        public string BruteForceEnCryptedText
        {
            get { return bruteForceEncryptedText; }
            set
            {
                bruteForceEncryptedText = value;
                OnPropertyChanged("BruteForceEncryptedText");
            }
        }

        private int bruteForceKeyOutput;
        [PropertyInfo(Direction.Output, "Bruteforce key", "This key should be connected to the caesar bruteforce plugin.", null, false, false, DisplayLevel.Professional, QuickWatchFormat.Text, null)]
        public int BruteForceKeyOutput
        {
            get { return bruteForceKeyOutput; }
            set
            {
                bruteForceKeyOutput = value;
                OnPropertyChanged("BruteForceKeyOutput");
            }
        }*/

        public void CryptoAnalysis()
        {
            var KeyList = new Dictionary<int, int>();
            int Counter = 0;
            foreach (var i in CountChars(frequencyList))
            {
                if (Counter < 5)
                {
                    if (!KeyList.ContainsKey(i))
                        KeyList.Add(i, 5 - Counter);
                    else
                    {
                        KeyList[i] += 5 - Counter;
                    }
                    Counter++;
                }
            }

            Counter = 0;
            foreach (var i in CountBigrams(encryptedText))
            {
                if (Counter < 5)
                {
                    if (!KeyList.ContainsKey(i))
                        KeyList.Add(i, 5 - Counter);
                    else
                    {
                        KeyList[i] += 5 - Counter;
                    }
                    Counter++;
                }
            }

            var items = (from k in KeyList.Keys
                         orderby KeyList[k] descending
                         select k);
            var ResultList = new List<int>();
            foreach (var i in items)
            {
                ResultList.Add(i);
            }
            if (ResultList.Count > 0)
            {
                key = ResultList[0];
                OnPropertyChanged("Key");
            }
        }

        private List<int> CountChars(string text)
        {
            var Dic = new Dictionary<char, int>();

            if (!string.IsNullOrEmpty(text))
            {
                foreach (var s in text.Split(new[] { "\r\n" }, StringSplitOptions.None))
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        string[] tmpArr = s.Split(new[] { ':' });
                        if (tmpArr.Length > 1)
                        {

                            char c = tmpArr[0][0];
                            int Count;
                            int.TryParse(tmpArr[1], out Count);
                            if (!Dic.ContainsKey(c))
                                Dic.Add(c, 0);
                            Dic[c] += Count;
                        }
                    }
                }

                var items = (from k in Dic.Keys
                             orderby Dic[k] descending
                             select k);

                var Result = new List<int>();
                foreach (var c in items)
                {
                    int tmp = c - settings.FrequentChar;
                    if (tmp > 0)
                        Result.Add(tmp);
                }
                return Result;
            }
            return new List<int>();
        }

        private static List<int> CountBigrams(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {

                var BigramDic = new Dictionary<string, int>();
                for (int i = 0; i < text.Length - 1; i++)
                {
                    string tmp = text.Substring(i, 2);
                    if (!tmp.Contains(" "))
                    {
                        if (!BigramDic.ContainsKey(tmp))
                            BigramDic.Add(tmp, 0);
                        BigramDic[tmp]++;
                    }
                }

                var items = (from k in BigramDic.Keys
                             orderby BigramDic[k] descending
                             select k);

                var Bigrams = new[] { "er", "en", "ch", "de" };
                var KeyList = new Dictionary<int, int>();
                foreach (var s in items)
                {
                    int Counter = 0;
                    string CurrentBigramm;
                    do
                    {
                        if (Counter < Bigrams.Length)
                            CurrentBigramm = Bigrams[Counter];
                        else
                        {
                            CurrentBigramm = string.Empty;
                            break;
                        }
                        Counter++;
                    } while (!(CurrentBigramm[1] - CurrentBigramm[0] == s[1] - s[0]));

                    if (!String.IsNullOrEmpty(CurrentBigramm))
                    {
                        int tmpkey = s[0] - CurrentBigramm[0];
                        if (!KeyList.ContainsKey(tmpkey))
                            KeyList.Add(tmpkey, 0);
                        KeyList[tmpkey]++;
                    }
                }
                var items2 = (from k in KeyList.Keys
                              orderby KeyList[k] descending
                              select k);
                var Result = new List<int>();
                foreach (var s in items2)
                {
                    Result.Add(s);
                }

                return Result;
            }
            return new List<int>();
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            encryptedText = string.Empty;
            key = 0;
            frequencyList = string.Empty;
        }

        public void Execute()
        {
            CryptoAnalysis();
        }

        public void PostExecution()
        {

        }

        public void Pause()
        {

        }

        public void Stop()
        {

        }

        public void Initialize()
        {

        }

        public void Dispose()
        {

        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

/*
        private void GuiNotification(string text)
        {
            GuiNotification(text, NotificationLevel.Debug);
        }
*/

/*
        private void GuiNotification(string text, NotificationLevel Level)
        {
            if (OnGuiLogNotificationOccured != null)
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(text, this, Level));
        }
*/
    }
}
