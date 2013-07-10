/*
   Copyright 2013 Nils Kopal, Universität Kassel

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Substitution
{
    [Author("Nils Kopal", "Nils.Kopal@Uni-Kassel.de", "Universität Kassel", "http://www.uni-kassel.de")]
    [PluginInfo("Substitution.Properties.Resources", "PluginCaption", "PluginTooltip", "Substitution/DetailedDescription/doc.xml", 
      new[] { "Substitution/Images/icon.png", "Substitution/Images/encrypt.png", "Substitution/Images/decrypt.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Substitution : ICrypComponent
    {
        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
        public string InputString
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public string OutputString
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "SourceAlphabetCaption", "SourceAlphabetTooltip", false)]
        public string SourceAlphabet
        {
            get; 
            set;
        }

        [PropertyInfo(Direction.InputData, "DestinationAlphabetCaption", "DestinationAlphabetTooltip", false)]
        public string DestinationAlphabet
        {
            get;
            set;
        }


        private SubstitutionSettings _settings = new SubstitutionSettings();

        public void PreExecution()
        {
            
        }

        public void PostExecution()
        {
            
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public ISettings Settings
        {
            get { return _settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void Execute()
        {
            //Nullpointer Exception here (DestinationAlphabet / SourceAlphabet)
            ProgressChanged(0,1);
            var dict = GenerateSubstitutionDictionary(SourceAlphabet.Replace(Environment.NewLine, String.Empty), 
                                                      DestinationAlphabet.Replace(Environment.NewLine, String.Empty));

            if (((SubstitutionSettings) Settings).SymbolChoice == SymbolChoice.Random)
            {
                OutputString = Substitute(InputString, dict);
            }
            else
            {
                OutputString = Substitute(InputString, dict, false);
            }
            OnPropertyChanged("OutputString");
            ProgressChanged(1, 1);
        }

        public void Stop()
        {
            
        }

        public void Initialize()
        {
            ((SubstitutionSettings) Settings).UpdateTaskPaneVisibility();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public void Dispose()
        {
            
        }

        /// <summary>
        /// Generates a dictionary for substitution using a source alphabet and a destination alphabet
        /// </summary>
        /// <param name="sourceAlphabet"></param>
        /// <param name="destinationAlphabet"></param>
        /// <returns></returns>
        private Dictionary<string, string> GenerateSubstitutionDictionary(string sourceAlphabet, string destinationAlphabet)
        {
            var dictionary = new Dictionary<string, string>();
            
            for (int si = 0, di = 0; si < sourceAlphabet.Length && di < destinationAlphabet.Length; si++)
            {
                var sourceCharacter = "";
                var destinationCharacter = "";
                //1. Find next source character (a "character" may be one or more chars in the string)
                if (sourceAlphabet[si] == '[')
                {
                    for (si++; si < sourceAlphabet.Length; si++)
                    {
                        if (sourceAlphabet[si] != ']')
                        {
                            sourceCharacter += sourceAlphabet[si];
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    sourceCharacter = new string(sourceAlphabet[si], 1);
                }
                //2. Find next destination character (a "character" may be one or more chars in the string)
                if (destinationAlphabet[di] == '[')
                {
                    for (di++; di < destinationAlphabet.Length; di++)
                    {
                        if (destinationAlphabet[di] != ']')
                        {
                            destinationCharacter += destinationAlphabet[di];
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    destinationCharacter = new string(destinationAlphabet[di], 1);
                }
                di++;
                //3. Add Substitution rule to our dictionary
                if (!dictionary.ContainsKey(sourceCharacter))
                {
                    dictionary.Add(sourceCharacter, destinationCharacter);
                }
                else
                {
                    GuiLogMessage(string.Format("A substitution for '{0}' already exsists ('{0}'->'{2}'). Ignore the new one ('{0}'->'{1}').", sourceCharacter, destinationCharacter, dictionary[sourceCharacter]),NotificationLevel.Warning);
                }
            }
            return dictionary;
        }

        /// <summary>
        /// Substitute a given Text using a dictionary which contains the mapping
        /// </summary>
        /// <param name="text"></param>
        /// <param name="substitutionDictionary"></param>
        /// <param name="randomDistribution"></param>
        /// <returns></returns>
        private string Substitute(string text, Dictionary<string, string> substitutionDictionary, bool randomDistribution = true)
        {
            var substitution = new StringBuilder();
            var random = new Random();

            //we search for the "longest" source character
            var maxLength = substitutionDictionary.Keys.Select(key => key.Length).Concat(new[] { 0 }).Max();

            var actualCharacter = "";
            
            //this dictionary is used when we do not want a randomDistribution at poly alphabetic substitution (for example a->[1|2|3])
            //it stores the actual index in the [1|2|3] array
            var polyCounterDictionary = new Dictionary<string, int>();
            for (var position = 0; position < text.Length;)
            {
                for (var lengthActualCharacter = Math.Min(maxLength,(text.Length - position)); lengthActualCharacter >= 0; lengthActualCharacter--)
                {
                    actualCharacter = text.Substring(position, lengthActualCharacter);

                    if (lengthActualCharacter == 0)
                    {
                        actualCharacter = text.Substring(position, 1);
                        position++;
                        switch (((SubstitutionSettings) Settings).UnknownSymbolHandling)
                        {
                            case UnknownSymbolHandling.LeaveAsIs:
                                substitution.Append(actualCharacter);
                                break;
                            case UnknownSymbolHandling.Replace:
                                substitution.Append(((SubstitutionSettings) Settings).ReplacementSymbol);
                                break;
                            case UnknownSymbolHandling.Remove:
                                break;
                        }                        
                    }
                    else if (ExistsSubstitionMapping(substitutionDictionary, actualCharacter))
                    {
                        position += lengthActualCharacter;
                        var substitutionCharacter = GetSubstitutionValue(substitutionDictionary, actualCharacter);
                        if (substitutionCharacter.Contains("|"))
                        {
                            var substitutionCharacters = substitutionCharacter.Split(new[] {'|', '[', ']'});
                            //choose a random character from the substitution array
                            if (randomDistribution)
                            {
                                var randomCharacterNumber = random.Next(substitutionCharacters.Length);
                                substitutionCharacter = substitutionCharacters[randomCharacterNumber];
                            }
                            else
                            //choose the next character from the substitution array
                            {
                                if (polyCounterDictionary.ContainsKey(actualCharacter))
                                {
                                    polyCounterDictionary[actualCharacter] = (polyCounterDictionary[actualCharacter] + 1)%
                                                                             substitutionCharacters.Length;
                                }
                                else
                                {
                                    polyCounterDictionary.Add(actualCharacter, 0);
                                }
                                substitutionCharacter = substitutionCharacters[polyCounterDictionary[actualCharacter]];
                            }
                        }
                        substitution.Append(substitutionCharacter);
                        break;
                    }                    
                }
                ProgressChanged(position, text.Length);
            }           
            return substitution.ToString();
        }

        /// <summary>
        /// Get the substitution value from a dictionary for the given substitution key
        /// </summary>
        /// <param name="substitutionDictionary"></param>
        /// <param name="substitutionKey"></param>
        /// <returns></returns>
        private string GetSubstitutionValue(Dictionary<string, string> substitutionDictionary, string substitutionKey)
        {
            //It can be that the key stands without [], so we can find it with ContainstKey and return it
            if (substitutionDictionary.ContainsKey(substitutionKey))
            {
                return substitutionDictionary[substitutionKey];
            }
            //We did not find it, so we have to search all Keys
            foreach (var key in substitutionDictionary.Keys)
            {
                //we only have to check keys which are arrays
                if (key.Contains("|"))
                {
                    var keys = key.Split(new[] { '|', '[', ']' });
                    if (keys.Any(arraykey => arraykey.Equals(substitutionKey)))
                    {
                        return substitutionDictionary[key];
                    }
                }
            }
            return "?";
        }

        /// <summary>
        /// Makes a lookup in a substitution dictionary wether a possible substitution exists or not
        /// </summary>
        /// <param name="substitutionDictionary"></param>
        /// <param name="substitutionKey"></param>
        /// <returns></returns>
        private bool ExistsSubstitionMapping(Dictionary<string, string> substitutionDictionary, string substitutionKey)
        {
            //It can be that the key stands without [], so we can find it with ContainstKey and return true
            if (substitutionDictionary.ContainsKey(substitutionKey))
            {
                return true;
            }
            //We did not find it, so we have to search all Keys
            foreach (var key in substitutionDictionary.Keys)
            {
                //we only have to check keys which are arrays
                if (key.Contains("|"))
                {
                    var keys = key.Split(new[] { '|', '[', ']' });
                    if (keys.Any(arraykey => arraykey.Equals(substitutionKey)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
