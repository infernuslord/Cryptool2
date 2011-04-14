/* HOWTO: Change year, author name and organization.
   Copyright 2010 Your Name, University of Duckburg

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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.Miscellaneous;

namespace Solitaire
{
    [Author("Coen Ramaekers", "c.f.w.ramaekers@student.tue.nl", "Technische Universiteit Eindhoven", "http://www.win.tue.nl")]
    [PluginInfo("Solitaire.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "Solitaire/sol.jpg")]
    [EncryptionType(EncryptionType.Classic)]
    public class Solitaire : IEncryption
    {
        #region Private Variables

        private SolitaireSettings settings;

        private SolitaireQuickWatchPresentation myPresentation;

        private string inputString, outputString, outputStream, password, deckstate, initialDeck, finalDeck;

        private StringBuilder output, stream, sb;

        private bool isPlayMode = false;

        private int numberOfCards;
        
        private int[] deck, newDeck;

        private enum CipherMode { encrypt, decrypt };

        #endregion

        #region Data Properties

        public Solitaire()
        {
            output = new StringBuilder("");
            stream = new StringBuilder("");
            sb = new StringBuilder(152);
            settings = new SolitaireSettings();
            myPresentation = new SolitaireQuickWatchPresentation(this);
            QuickWatchPresentation = myPresentation;
        }

        /// <summary>
        /// Read the text which is to be encrypted or decrypted.
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", null, false, false, QuickWatchFormat.None ,null)]
        public string InputString
        {
            get { return this.inputString; }
            set
            {
                if (value != InputString)
                {
                    this.inputString = value;
                    OnPropertyChanged("InputString");
                }
            }
        }

        /// <summary>
        /// Read the password with which the deckstate is generated.
        /// </summary>
        [PropertyInfo(Direction.InputData, "PasswordCaption", "PasswordTooltip", null, false, false, QuickWatchFormat.None, null)]
        public string Password
        {
            get { return this.password; }
            set
            {
                if (value != Password)
                {
                    this.password = value;
                    OnPropertyChanged("Password");
                }
            }
        }

        /// <summary>
        /// Read a given deckstate.
        /// </summary>
        [PropertyInfo(Direction.InputData, "DeckstateCaption", "DeckstateTooltip", null, false, false, QuickWatchFormat.None, null)]
        public string Deckstate
        {
            get { return this.deckstate; }
            set
            {
                if (value != Deckstate)
                {
                    this.deckstate = value;
                    OnPropertyChanged("Deckstate");
                }
            }
        }


        /// <summary>
        /// Outputs the encrypted or decrypted text.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", null, false, false, QuickWatchFormat.Text, null)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        /// <summary>
        /// Displays the initial deck.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "InitialDeckCaption", "InitialDeckTooltip", null, false, false, QuickWatchFormat.Text, null)]
        public string InitialDeck
        {
            get { return this.initialDeck; }
            set
            {
                initialDeck = value;
                OnPropertyChanged("InitialDeck");
            }
        }


        /// <summary>
        /// Displays the final deck.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "FinalDeckCaption", "FinalDeckTooltip", null, false, false, QuickWatchFormat.Text, null)]
        public string FinalDeck
        {
            get { return this.finalDeck; }
            set
            {
                finalDeck = value;
                OnPropertyChanged("FinalDeck");
            }
        }

        /// <summary>
        /// Outputs the stream used to encrypt or decrypt.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", null, false, false, QuickWatchFormat.Text, null)]
        public string OutputStream
        {
            get { return this.outputStream; }
            set
            {
                outputStream = value;
                OnPropertyChanged("OutputStream");
            }
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
            set { this.settings = (SolitaireSettings)value; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get;
            private set;
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            isPlayMode = true;
            if (settings.ActionType == 0)
            {
                GuiLogMessage("Encrypting", NotificationLevel.Debug);
                if (settings.StreamType == 0) SolitaireCipher(CipherMode.encrypt, true);
                if (settings.StreamType == 1) SolitaireManual(0);
            }
            else
            {
                if (settings.StreamType == 0) SolitaireCipher(CipherMode.decrypt, true);
                if (settings.StreamType == 1) SolitaireManual(1);
            }
        }

        public void PostExecution()
        {
            isPlayMode = false;
        }

        public void Pause()
        {
        }

        public void Stop()
        {
            myPresentation.button1.IsEnabled = true;
            myPresentation.button2.IsEnabled = false;
            myPresentation.button3.IsEnabled = false;
            myPresentation.button4.IsEnabled = false;
            myPresentation.button5.IsEnabled = false;
            myPresentation.button6.IsEnabled = false;
            myPresentation.button7.IsEnabled = false;
            myPresentation.stop();
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        #region Private Methods

        private void SolitaireManual(int mode)
        {
            numberOfCards = settings.NumberOfCards;
            if (inputString != null)
            {
                GuiLogMessage("Input: " + inputString, NotificationLevel.Debug);
                switch (settings.GenerationType)
                {
                    case 0: //Ascending
                        KeyTheDeckAsc(numberOfCards);
                        break;

                    case 1: //Descending
                        KeyTheDeckDesc(numberOfCards);
                        break;

                    case 2: //Given state
                        if (deckstate != null) KeyTheDeckSequence(deckstate);
                        else GuiLogMessage("Given deckstate missing!", NotificationLevel.Error);
                        break;

                    case 3: //Password
                        if (password != null) KeyTheDeckPassword(password, numberOfCards);
                        else GuiLogMessage("Password missing!", NotificationLevel.Error);
                        break;

                    case 4: //Random
                        KeyTheDeckRandom(numberOfCards);
                        break;
                }
            }
            if (deck != null)
            {
                myPresentation.enable(numberOfCards, mode);
            }
        }

        public void SolitaireCipher(int mode, bool seperator)
        {
            if (mode == 0) SolitaireCipher(CipherMode.encrypt, seperator);
            else SolitaireCipher(CipherMode.decrypt, seperator);
        }

        private void SolitaireCipher(CipherMode mode, bool seperator)
        {
            output.Clear();
            stream.Clear();
            numberOfCards = settings.NumberOfCards;


            if (inputString != null)
            {
                FormatText(ref inputString);
                switch (settings.GenerationType)
                {
                    case 0: //Ascending
                        KeyTheDeckAsc(numberOfCards);
                        break;

                    case 1: //Descending
                        KeyTheDeckDesc(numberOfCards);
                        break;

                    case 2: //Given state
                        if (deckstate != null) KeyTheDeckSequence(deckstate);
                        else GuiLogMessage("Given deckstate missing!", NotificationLevel.Error);
                        break;

                    case 3: //Password
                        if (password != null) KeyTheDeckPassword(password, numberOfCards);
                        else GuiLogMessage("Password missing!", NotificationLevel.Error);
                        break;

                    case 4: //Random
                        KeyTheDeckRandom(numberOfCards);
                        break;
                }

                if (deck != null)
                {
                    numberOfCards = deck.Length;
                    int curKey, curChar, j = 1;
                    for (int i = 0; i < inputString.Length; i++)
                    {
                        PushAndCut(numberOfCards);
                        curKey = deck[0];
                        curChar = ((int)inputString[i] - 64);
                        while (curChar == -32 & i < inputString.Length)
                        {
                            i++;
                            curChar = ((int)inputString[i] - 64);
                        }
                        

                        if (curKey == numberOfCards)
                            curKey = deck[numberOfCards-1];
                        else
                            curKey = deck[curKey];

                        if (mode == CipherMode.encrypt)
                            curChar = (curChar + curKey);
                        else
                        {
                            if (curChar < curKey) curChar += 26;
                            curChar = (curChar - curKey);
                        }
                        if (curKey < numberOfCards - 1)
                        {
                            if (curChar > 26) curChar %= 26;
                            if (curChar < 1) curChar += 26;
                            output.Append((char)(curChar + 64));
                            stream.Append(Convert.ToString(curKey));
                            if (i != inputString.Length - 1) stream.Append(",");
                            if (j % 5 == 0 & seperator) output.Append(" ");
                            j++;
                        }
                        else i--;

                        if (seperator) ProgressChanged(i, inputString.Length - 1);
                    }
                    outputString = output.ToString();
                    outputStream = stream.ToString();
                    finalDeck = GetDeck(numberOfCards);
                    OnPropertyChanged("FinalDeck");
                    OnPropertyChanged("OutputString");
                    OnPropertyChanged("OutputStream");
                    OnPropertyChanged("OutputData");
                }
            }
        }

        public String GetDeck(int numberOfCards)
        {
            sb.Clear();
            for (int i = 0; i < numberOfCards; i++)
            {
                sb.Append((deck[i] == numberOfCards - 1) ? "A" : ((deck[i] == numberOfCards) ? "B" : deck[i].ToString()));
                if (i != numberOfCards-1) sb.Append(",");
            }
            return sb.ToString();
        }

        internal int[] GetDeck()
        {
            return deck;
        }

        public void FormatText(ref String msg)
        {
            msg = msg.ToUpper();
            Regex regex = new Regex("[^A-Z]", RegexOptions.None);
            if (regex.IsMatch(msg)) msg = regex.Replace(msg, "");
            while (msg.Length % 5 != 0) msg = msg + "X";
        }

        private void FormatPass(ref String msg)
        {
            msg = msg.ToUpper();
            Regex regex = new Regex("[^A-Z0-9]", RegexOptions.None);
            if (regex.IsMatch(msg)) msg = regex.Replace(msg, "");
        }

        private void KeyTheDeckPassword(string pass, int numberOfCards)
        {
            deck = new int[numberOfCards];
            for (int i = 0; i < numberOfCards; i++) deck[i] = i + 1;
            FormatPass(ref pass);
            int curChar;
            for (int i = 0; i < pass.Length; i++)
            {
                PushAndCut(numberOfCards);
                if (Regex.IsMatch(pass.Substring(i, 1), "[A-Z]{1}")) curChar = (int)pass[i] - 65;
                else curChar = Convert.ToInt16(pass.Substring(i, 1));
                CountCut(curChar + 1, numberOfCards);
            }
            initialDeck = GetDeck(numberOfCards);
            OnPropertyChanged("InitialDeck");
        }

        private void KeyTheDeckSequence(string seq)
        {
            seq = seq.ToUpper();
            if (Regex.IsMatch(seq, "^([1-5]?[0-9]{1})|[AB]{1}(,([1-5]?[0-9]{1})|[AB]{1})*$") & !Regex.IsMatch(seq, "[5]{1}[5-9]{1}"))
            {
                bool test = true;
                if (seq.Contains("A")) if (!seq.Contains("B")) test = false;
                if (seq.Contains("B")) if (!seq.Contains("A")) test = false;
                if (test)
                {
                    string[] sequence = seq.Split(new char[] { Convert.ToChar(",") });
                    HashSet<string> set = new HashSet<string>(sequence);
                    if (set.Count < sequence.Length)
                    {
                        GuiLogMessage("Sequence contained duplicates! These have been removed.", NotificationLevel.Warning);
                        sequence = new string[set.Count];
                        set.CopyTo(sequence);
                    }
                    int numberOfCards = sequence.Length;
                    if (numberOfCards <= 54)
                    {
                        deck = new int[numberOfCards];
                        for (int i = 0; i < numberOfCards; i++)
                        {
                            if (sequence[i].Equals("A")) deck[i] = numberOfCards - 1;
                            else if (sequence[i].Equals("B")) deck[i] = numberOfCards;
                            else deck[i] = int.Parse(sequence[i]);
                        }
                    }
                    else GuiLogMessage("Too many cards (>54)", NotificationLevel.Error);
                }
                else GuiLogMessage("Sequence contains only one of A and B", NotificationLevel.Error);
            }
            else
            {
                string reason = "";
                if (Regex.IsMatch(seq, ",{1},{1}")) reason = " Consecutive commas";
                if (Regex.IsMatch(seq, "[5]{1}[5-9]{1}")) reason = reason + (reason.Length > 0 ? ", t" : " T") + "oo large value";
                if (!Regex.IsMatch(seq, "^[,0-9AB]*$")) reason = reason + (reason.Length > 0 ? ", s" : " S") + "trange characters (other than numbers, commas, A, B)"; 
                GuiLogMessage("Error in inserted sequence!" + reason + (reason.Length > 0 ? "." : ""), NotificationLevel.Error);
            }
            initialDeck = GetDeck(deck.Length);
            OnPropertyChanged("InitialDeck");
        }

        private void KeyTheDeckAsc(int numberOfCards)
        {
            deck = new int[numberOfCards];
            for (int i = 0; i < numberOfCards; i++) deck[i] = i + 1;
            initialDeck = GetDeck(numberOfCards);
            OnPropertyChanged("InitialDeck");
        }

        private void KeyTheDeckDesc(int numberOfCards)
        {
            deck = new int[numberOfCards];
            for (int i = 0; i < numberOfCards; i++) deck[i] = numberOfCards - i;
            initialDeck = GetDeck(numberOfCards);
            OnPropertyChanged("InitialDeck");
        }

        private void KeyTheDeckRandom(int numberOfCards)
        {
            deck = new int[numberOfCards];
            ArrayList choices = new ArrayList();
            for (int i = 0; i < numberOfCards; i++) choices.Add(i+1);
            Random r = new Random();
            int randomIndex = 0;
            while (choices.Count > 0)
            {
                randomIndex = r.Next(0, choices.Count);
                deck[choices.Count - 1] = (int)choices[randomIndex];
                choices.RemoveAt(randomIndex);
            }
            initialDeck = GetDeck(numberOfCards);
            OnPropertyChanged("InitialDeck");
        }

        internal void PushAndCut(int numberOfCards)
        {
            MoveCardDown(numberOfCards - 1, numberOfCards);
            MoveCardDown(numberOfCards, numberOfCards);
            MoveCardDown(numberOfCards, numberOfCards);
            TripleCut(numberOfCards);
            CountCut(numberOfCards);
        }

        internal void InversePushAndCut(int numberOfCards)
        {
            InverseCountCut(numberOfCards);
            InverseTripleCut(numberOfCards);
            MoveCardUp(numberOfCards, numberOfCards);
            MoveCardUp(numberOfCards, numberOfCards);
            MoveCardUp(numberOfCards - 1, numberOfCards);
        }

        internal void MoveCardDown(int card, int numberOfCards)
        {
            if (deck != null)
            {
                int pos = Array.IndexOf(deck, card);
                if (pos == numberOfCards-1)
                {
                    BottomToTop(numberOfCards);
                    MoveCardDown(card, numberOfCards);
                }
                else
                {
                    deck[pos] = deck[pos + 1];
                    deck[pos + 1] = card;
                }
            }
        }

        internal void MoveCardUp(int card, int numberOfCards)
        {
            int pos = Array.IndexOf(deck, card);
            if (pos == 0)
            {
                TopToBottom(numberOfCards);
                MoveCardUp(card, numberOfCards);
            }
            else
            {
                deck[pos] = deck[pos - 1];
                deck[pos - 1] = card;
            }
        }

        internal void BottomToTop(int numberOfCards)
        {
            int card = deck[numberOfCards - 1];
            for (int i = numberOfCards-1; i > 0; i--)
                deck[i] = deck[i - 1];
            deck[0] = card;
        }

        internal void TopToBottom(int numberOfCards)
        {
            int card = deck[0];
            for (int i = 0; i < numberOfCards; i++)
                deck[i] = deck[i + 1];
            deck[numberOfCards] = card;
        }

        internal void TripleCut(int numberOfCards)
        {
            int jokerTop = Math.Min(Array.IndexOf(deck, numberOfCards - 1), Array.IndexOf(deck, numberOfCards));
            int jokerBottom = Math.Max(Array.IndexOf(deck, numberOfCards - 1), Array.IndexOf(deck, numberOfCards));

            newDeck = new int[numberOfCards];
            int lengthBottom = numberOfCards - 1 - jokerBottom;
            int lengthMiddle = jokerBottom - jokerTop - 1;

            Array.Copy(deck, jokerBottom + 1, newDeck, 0, lengthBottom);
            Array.Copy(deck, jokerTop, newDeck, lengthBottom, lengthMiddle + 2);
            Array.Copy(deck, 0, newDeck, lengthBottom + lengthMiddle + 2, jokerTop);

            newDeck.CopyTo(deck, 0);
        }

        internal void InverseTripleCut(int numberOfCards)
        {
            TripleCut(numberOfCards);
        }

        internal void CountCut(int cutPos, int numberOfCards)
        {
            newDeck = new int[numberOfCards];
            if (cutPos < numberOfCards-1)
            {
                Array.Copy(deck, cutPos, newDeck, 0, numberOfCards - 1 - (cutPos));
                Array.Copy(deck, 0, newDeck, numberOfCards - 1 - (cutPos), cutPos);
                newDeck[numberOfCards - 1] = deck[numberOfCards - 1];
                newDeck.CopyTo(deck, 0);
            }
        }

        internal void InverseCountCut(int cutPos, int numberOfCards)
        {
            newDeck = new int[numberOfCards];
            if (cutPos < numberOfCards -1)
            {
                Array.Copy(deck, 0, newDeck, cutPos, numberOfCards - 1 - cutPos);
                Array.Copy(deck, numberOfCards - 1 - cutPos, newDeck, 0, cutPos);
                newDeck[numberOfCards - 1] = deck[numberOfCards-1];
                newDeck.CopyTo(deck, 0);
            }
        }

        internal void CountCut(int numberOfCards)
        {
            CountCut(deck[numberOfCards - 1], numberOfCards);
        }

        internal void InverseCountCut(int numberOfCards)
        {
            InverseCountCut(deck[numberOfCards - 1], numberOfCards);
        }

        public void changeSettings(string setting, object value)
        {
            if (setting.Equals("Action Type")) settings.ActionType = (int)value;
            else if (setting.Equals("Cards")) settings.NumberOfCards = (int)value;
            else if (setting.Equals("Deck Generation")) settings.GenerationType = (int)value;
            else if (setting.Equals("Stream Generation")) settings.StreamType = (int)value;
        }

        #endregion
    }
 }

