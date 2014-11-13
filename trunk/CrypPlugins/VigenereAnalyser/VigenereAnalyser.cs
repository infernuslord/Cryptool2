using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using System.ComponentModel;
using System.Windows.Documents;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;

namespace Cryptool.VigenereAnalyser
{
    public delegate void PluginProgress(double current, double maximum);
    public delegate void UpdateOutput(String keyString, String plaintextString);

    [Author("Nils Kopal", "Nils.Kopal@Uni-Kassel.de", "Uni Duisburg", "http://ais.uni-kassel.de")]
    [PluginInfo("Cryptool.VigenereAnalyser.Properties.Resources",
    "PluginCaption", "PluginTooltip", "", "VigenereAnalyser/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class VigenereAnalyser : ICrypComponent
    {
        private const int MaxBestListEntries = 20;
        private readonly AssignmentPresentation _presentation = new AssignmentPresentation();
        private string _plaintext;
        private string _key;
        private readonly VigenereAnalyserSettings _settings = new VigenereAnalyserSettings();
        private double[,,,] _quadgrams;
        private bool _stopped;
        private DateTime _startTime;
        private DateTime _endTime;
        private string _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public VigenereAnalyser()
        {
            _presentation.UpdateOutputFromUserChoice+=UpdateOutputFromUserChoice;
        }

        private void UpdateOutputFromUserChoice(string keyString, string plaintextString)
        {
            Plaintext = plaintextString;
            Key = keyString;
        }
        
        [PropertyInfo(Direction.InputData, "CiphertextCaption", "CiphertextTooltip", true)]
        public string Ciphertext { get; set; }

        [PropertyInfo(Direction.InputData, "VigenreAlphabetCaption", "VigenereAlphabetTooltip", false)]
        public string VigenereAlphabet { get; set; }

        [PropertyInfo(Direction.OutputData, "PlaintextCaption", "PlaintextTooltip", true)]
        public String Plaintext
        {
            get { return _plaintext; }
            set { _plaintext = value; OnPropertyChanged("Plaintext"); }
        }

        [PropertyInfo(Direction.OutputData, "KeyCaption", "KeytTooltip", true)]
        public String Key
        {
            get { return _key; }
            set { _key = value; OnPropertyChanged("Key"); }
        }
        
        public void PreExecution()
        {
            if (_settings.Language == Language.Englisch)
            {
                Load4Grams("en-4gram-nocs.bin", "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            }
            else if (_settings.Language == Language.German)
            {
                Load4Grams("de-4gram-nocs.bin", "ABCDEFGHIJKLMNOPQRSTUVWXYZÄÜÖß");
            }
            else
            {
                Load4Grams("en-4gram-nocs.bin", "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            }            
            VigenereAlphabet = _alphabet;
        }

        public void PostExecution()
        {
            Ciphertext = null;
            VigenereAlphabet = null;
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings
        {
            get { return _settings; }
        }

        public UserControl Presentation
        {
            get { return _presentation; }
        }

        public void Execute()
        {
            // Clear presentation
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((AssignmentPresentation)Presentation).BestList.Clear();
            }, null);

            _stopped = false;
            ProgressChanged(0, 1);

            if (string.IsNullOrEmpty(Ciphertext))
            {
                GuiLogMessage("No Ciphertext given for analysis!", NotificationLevel.Error);
                return;
            }
            if (string.IsNullOrEmpty(VigenereAlphabet))
            {
                GuiLogMessage("No Vigenere Alphabet given for analysis!", NotificationLevel.Error);
                return;
            }
            if (_settings.ToKeyLength > RemoveInvalidChars(Ciphertext,_alphabet).Length)
            {
                _settings.ToKeyLength = RemoveInvalidChars(Ciphertext, _alphabet).Length;
                GuiLogMessage("Max tested keylength can not be longer than the ciphertext. Set max tested keylength to ciphertext length.",NotificationLevel.Warning);
            }
            if (_settings.ToKeyLength < _settings.FromKeylength)
            {
                var temp = _settings.ToKeyLength;
                _settings.ToKeyLength = _settings.FromKeylength;
                _settings.FromKeylength = temp;
            }
            var ciphertext = MapTextIntoNumberSpace(RemoveInvalidChars(Ciphertext.ToUpper(), _alphabet), _alphabet);
            UpdateDisplayStart();
            for (var keylength = _settings.FromKeylength; keylength <= _settings.ToKeyLength; keylength++)
            {
                HillclimbVigenere(ciphertext, keylength, _quadgrams, _settings.Restarts, _settings.Greedy);
                if (_stopped)
                {
                    UpdateDisplayEnd();
                    return;
                }
            }            
            UpdateDisplayEnd();
            if (_presentation.BestList.Count > 0)
            {
                Plaintext = _presentation.BestList[0].Text;
                Key = _presentation.BestList[0].Key;
            }
            
            ProgressChanged(1, 1);            
        }

        private void UpdateDisplayStart()
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _startTime = DateTime.Now;
                ((AssignmentPresentation)Presentation).startTime.Content = "" + _startTime;
                ((AssignmentPresentation)Presentation).endTime.Content = "";
                ((AssignmentPresentation)Presentation).elapsedTime.Content = "";
            }, null);
        }

        private void UpdateDisplayEnd()
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _endTime = DateTime.Now;
                var elapsedtime = _endTime.Subtract(_startTime);
                var elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                ((AssignmentPresentation)Presentation).endTime.Content = "" + _endTime;
                ((AssignmentPresentation)Presentation).elapsedTime.Content = "" + elapsedspan;

            }, null);
        }

        public void Stop()
        {
            _stopped = true;
        }

        public void Initialize()
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            
        }

        /// <summary>
        /// Hillclimbs Vigenere or Vigenere Autokey
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="keylength"></param>
        /// <param name="ngrams4"></param>
        /// <param name="restarts"></param>
        /// <param name="greedy"></param>
        private void HillclimbVigenere(int[] ciphertext, int keylength, double[, , ,] ngrams4, int restarts = 10, bool greedy = false)
        {
            var globalbestkeycost = double.MinValue;
            var bestkey = new int[keylength];
            var alphabetlength = _alphabet.Length;
            var numalphabet = MapTextIntoNumberSpace(_alphabet, _alphabet);
            var numvigalphabet = MapTextIntoNumberSpace(VigenereAlphabet, _alphabet);
            var random = new Random(Guid.NewGuid().GetHashCode());
            var totalrestarts = restarts;

            while (restarts > 0)
            {
                //generate random key:
                var runkey = new int[keylength];
                for (var i = 0; i < runkey.Length; i++)
                {
                    runkey[i] = numalphabet[random.Next(0, alphabetlength)];
                }
                bool foundbetter;
                var bestkeycost = double.MinValue;
                do
                {
                    foundbetter = false;
                    //permutate key:                     
                    for (var i = 0; i < keylength; i++)
                    {
                        for (int j = 0; j < alphabetlength; j++)
                        {
                            //copy key
                            var copykey = new int[keylength];
                            for (int k = 0; k < keylength; k++)
                            {
                                copykey[k] = runkey[k];
                            }
                            copykey[i] = j;
                            var plaintext = _settings.Mode == Mode.Vigenere ? DecryptVigenereOwnAlphabet(ciphertext, copykey, numvigalphabet) :
                                DecryptAutokeyOwnAlphabet(ciphertext, copykey, numvigalphabet);
                            var costvalue = CalculateQuadgramCost(ngrams4, plaintext) + (_settings.KeyStyle == KeyStyle.NaturalLanguage ? CalculateQuadgramCost(ngrams4, copykey) : 0);
                            if (costvalue > bestkeycost)
                            {
                                bestkeycost = costvalue;
                                bestkey = copykey;
                                foundbetter = true;
                                if (greedy)
                                {
                                    runkey = bestkey;
                                }                          
                            }
                            if (_stopped)
                            {
                                return;
                            }
                        }
                    }
                    runkey = bestkey;
                } while (foundbetter);

                restarts--;
                if (bestkeycost > globalbestkeycost)
                {
                    globalbestkeycost = bestkeycost;
                    AddNewBestListEntry(bestkey, globalbestkeycost, ciphertext);
                }
                ProgressChanged((keylength - _settings.FromKeylength) * totalrestarts + totalrestarts - restarts, (_settings.ToKeyLength - _settings.FromKeylength + 1) * totalrestarts);
            }         
        }

        /// <summary>
        /// Add an entry to the BestList
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ciphertext"></param>
        private void AddNewBestListEntry(int[] key, double value,int[] ciphertext)
        {            
            var entry = new ResultEntry()
            {
                Key = MapNumbersIntoTextSpace(key, _alphabet),
                Text = MapNumbersIntoTextSpace(_settings.Mode == Mode.Vigenere ? DecryptVigenereOwnAlphabet(ciphertext, key,  MapTextIntoNumberSpace(VigenereAlphabet,_alphabet)) :
                    DecryptAutokeyOwnAlphabet(ciphertext, key, MapTextIntoNumberSpace(VigenereAlphabet, _alphabet)), _alphabet),
                Value = value
            };

            if (_presentation.BestList.Count == 0)
            {
                Plaintext = entry.Text;
                Key = entry.Key;
            }
            else if(entry.Value > _presentation.BestList.First().Value)
            {
                Plaintext = entry.Text;
                Key = entry.Key;
            }

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    if (_presentation.BestList.Count > 0 && entry.Value <= _presentation.BestList.Last().Value)
                    {
                        return;
                    }
                    _presentation.BestList.Add(entry);
                    _presentation.BestList = new ObservableCollection<ResultEntry>(_presentation.BestList.OrderByDescending(i => i.Value));
                    if (_presentation.BestList.Count > MaxBestListEntries)
                    {
                        _presentation.BestList.RemoveAt(MaxBestListEntries);
                    }
                    var ranking = 1;
                    foreach (var e in _presentation.BestList)
                    {
                        e.Ranking = ranking;
                        ranking++;
                    }
                    _presentation.ListView.DataContext = _presentation.BestList;
                }
                catch (Exception)
                {
                    //wtf?
                }
            }, null); 
        }

        /// <summary>
        /// Decrypts the given ciphertext using the given key and an own alphabet
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="key"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static int[] DecryptVigenereOwnAlphabet(int[] ciphertext, int[] key, int[] alphabet)
        {
            var plaintextlength = ciphertext.Length; // improves the speed because length has not to be calculated in the loop
            var plaintext = new int[plaintextlength];
            var keylength = key.Length; // improves the speed because length has not to be calculated in the loop
            var alphabetlength = alphabet.Length; // improves the speed because length has not to be calculated in the loop
            var lookup = new int[alphabetlength]; // improves the speed because length has not to be calculated in the loop
            for (var position = 0; position < alphabetlength; position++)
            {
                lookup[alphabet[position]] = position;
            }
            for (var position = 0; position < plaintextlength; position++)
            {
                plaintext[position] = alphabet[(lookup[ciphertext[position]] - lookup[key[position % keylength]] + alphabetlength) % alphabetlength];
            }
            return plaintext;
        }

        /// <summary>
        /// Decrypts the given ciphertext using the given key and an own alphabet
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="key"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static int[] DecryptAutokeyOwnAlphabet(int[] ciphertext, int[] key, int[] alphabet)
        {
            var plaintextlength = ciphertext.Length; // improves the speed because length has not to be calculated in the loop
            var plaintext = new int[plaintextlength];
            var keylength = key.Length; // improves the speed because length has not to be calculated in the loop
            var alphabetlength = alphabet.Length; // improves the speed because length has not to be calculated in the loop
            var lookup = new int[alphabetlength]; // improves the speed because length has not to be calculated in the loop
            for (var position = 0; position < alphabetlength; position++)
            {
                lookup[alphabet[position]] = position;
            }
            for (var position = 0; position < keylength; position++)
            {
                plaintext[position] = alphabet[(lookup[ciphertext[position]] - lookup[key[position % keylength]] + alphabetlength) % alphabetlength];
            }
            for (var position = keylength; position < plaintextlength; position++)
            {
                plaintext[position] = alphabet[(lookup[ciphertext[position]] - lookup[plaintext[position - keylength]] + alphabetlength) % alphabetlength];
            }
            return plaintext;
        }

        /// <summary>
        /// Calculate cost value based on 4-grams
        /// </summary>
        /// <param name="ngrams4"></param>
        /// <param name="plaintext"></param>
        /// <returns></returns>
        public static double CalculateQuadgramCost(double[, , ,] ngrams4, int[] plaintext)
        {
            double value = 0;
            var end = plaintext.Length - 3;

            for (var i = 0; i < end; i++)
            {
                value += ngrams4[plaintext[i], plaintext[i + 1], plaintext[i + 2], plaintext[i + 3]];
            }
            return value;
        }

        /// <summary>
        /// Maps a given array of numbers into the "textspace" defined by the alphabet
        /// </summary>
        /// <param name="numbers"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static string MapNumbersIntoTextSpace(int[] numbers, string alphabet)
        {
            var builder = new StringBuilder();
            foreach (var i in numbers)
            {
                builder.Append(alphabet[i]);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Maps a given string into the "numberspace" defined by the alphabet
        /// </summary>
        /// <param name="text"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static int[] MapTextIntoNumberSpace(string text, string alphabet)
        {
            var numbers = new int[text.Length];
            var position = 0;
            foreach (var c in text)
            {
                numbers[position] = alphabet.IndexOf(c);
                position++;
            }
            return numbers;
        }

        /// <summary>
        /// Calculates the mathemtical modulo operation: a mod n
        /// </summary>
        /// <param name="a"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private static int Mod(int a, int n)
        {
            var result = a % n;
            if (a < 0)
            {
                result += n;
            }
            return result;
        }

        /// <summary>
        /// Removes all chars from a given string which are not part of the alphabet
        /// </summary>
        /// <param name="text"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static string RemoveInvalidChars(string text, string alphabet)
        {
            var builder = new StringBuilder();
            foreach (var c in text)
            {
                if (alphabet.Contains(c))
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }


        /// <summary>
        /// Load 4Gram file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="alphabet"></param>
        private void Load4Grams(string filename, string alphabet)
        {
            _quadgrams = new double[alphabet.Length, alphabet.Length, alphabet.Length, alphabet.Length];
            using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < alphabet.Length; i++)
                    {
                        for (int j = 0; j < alphabet.Length; j++)
                        {
                            for (int k = 0; k < alphabet.Length; k++)
                            {
                                for (int l = 0; l < alphabet.Length; l++)
                                {
                                    var bytes = reader.ReadBytes(8);
                                    _quadgrams[i, j, k, l] = BitConverter.ToDouble(bytes, 0);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
    }

    public class ResultEntry
    {
        public int Ranking { get; set; }        
        public double Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }

        public double ExactValue
        {
            get { return Math.Abs(Value); }
        }
        
        public int KeyLength
        {
            get
            {
                return Key.Length;
            }
        }        
    }    

}
