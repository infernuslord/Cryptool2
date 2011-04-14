using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Solitaire
{
    /// <summary>
    /// Interaktionslogik für SolitaireQuickWatchPresentation.xaml
    /// </summary>
    public partial class SolitaireQuickWatchPresentation : System.Windows.Controls.UserControl
    {
        private Boolean enabled = false;
        private Solitaire plugin;
        private int numberOfCards, i, j;
        private enum CipherMode { encrypt, decrypt };
        private CipherMode mode;
        private System.Windows.Forms.RichTextBox rtb;
        private int[] oldDeck, newDeck;
        private System.Drawing.Font textFont;
        private System.Drawing.Font symbolFont;

        public SolitaireQuickWatchPresentation(Solitaire plugin)
        {
            this.plugin = plugin;

            InitializeComponent();
            
            //this.SizeChanged += new EventHandler(this.resetFontSize);
            this.rtb = windowsFormsHost1.FindName("richBox") as System.Windows.Forms.RichTextBox;
            textFont = new System.Drawing.Font(
                "Arial",
                9F               
            );
            rtb.Font = textFont;
            symbolFont = new System.Drawing.Font(
                "Arial",
                11F
            );
        }

        public void enable(int numberOfCards, int mode)
        {
            if (mode == 0) this.mode = CipherMode.encrypt;
            else this.mode = CipherMode.decrypt;
            this.numberOfCards = numberOfCards;
            enabled = true;
        }

        public void stop()
        {
            enabled = false;
        }

        private string convertDeckToSymbolDeck(string deck)
        {
            int[] tempDeck = stringToDeck(deck);
            string symbolDeck = "";
            for (int i = 0; i < tempDeck.Length; i++)
            {
                if (tempDeck[i] <= numberOfCards - 2)
                {
                    if (tempDeck[i] <= 9) symbolDeck = symbolDeck + "\u2660" + (tempDeck[i] + 1);
                    else if (tempDeck[i] == 10) symbolDeck = symbolDeck + "\u2660" + "J";
                    else if (tempDeck[i] == 11) symbolDeck = symbolDeck + "\u2660" + "Q";
                    else if (tempDeck[i] == 12) symbolDeck = symbolDeck + "\u2660" + "K";
                    else if (tempDeck[i] == 13) symbolDeck = symbolDeck + "\u2660" + "A";

                    else if (tempDeck[i] > 13 && tempDeck[i] <= 22) symbolDeck = symbolDeck + "\u2665" + (tempDeck[i] - 12);
                    else if (tempDeck[i] == 23) symbolDeck = symbolDeck + "\u2665" + "J";
                    else if (tempDeck[i] == 24) symbolDeck = symbolDeck + "\u2665" + "Q";
                    else if (tempDeck[i] == 25) symbolDeck = symbolDeck + "\u2665" + "K";
                    else if (tempDeck[i] == 26) symbolDeck = symbolDeck + "\u2665" + "A";

                    else if (tempDeck[i] > 26 && tempDeck[i] <= 35) symbolDeck = symbolDeck + "\u2666" + (tempDeck[i] - 25);
                    else if (tempDeck[i] == 36) symbolDeck = symbolDeck + "\u2666" + "J";
                    else if (tempDeck[i] == 37) symbolDeck = symbolDeck + "\u2666" + "Q";
                    else if (tempDeck[i] == 38) symbolDeck = symbolDeck + "\u2666" + "K";
                    else if (tempDeck[i] == 39) symbolDeck = symbolDeck + "\u2666" + "A";

                    else if (tempDeck[i] > 39 && tempDeck[i] <= 48) symbolDeck = symbolDeck + "\u2663" + (tempDeck[i] - 38);
                    else if (tempDeck[i] == 49) symbolDeck = symbolDeck + "\u2663" + "J";
                    else if (tempDeck[i] == 50) symbolDeck = symbolDeck + "\u2663" + "Q";
                    else if (tempDeck[i] == 51) symbolDeck = symbolDeck + "\u2663" + "K";
                    else if (tempDeck[i] == 52) symbolDeck = symbolDeck + "\u2663" + "A";
                }
                else
                {
                    if (tempDeck[i] == numberOfCards - 1) symbolDeck = symbolDeck + "A";
                    else if (tempDeck[i] == numberOfCards) symbolDeck = symbolDeck + "B";
                }

                if (i != tempDeck.Length - 1) symbolDeck = symbolDeck + ",";
            }
            return symbolDeck;
        }

        private string convertCardNumberToSymbol(string card)
        {
            string symbol = card;
            if (!(symbol.Equals(numberOfCards.ToString()) || symbol.Equals((numberOfCards - 1).ToString())))
            {
                switch (card)
                {
                    case "52": symbol = "\u2663" + "A"; break;
                    case "51": symbol = "\u2663" + "K"; break;
                    case "50": symbol = "\u2663" + "Q"; break;
                    case "49": symbol = "\u2663" + "J"; break;
                    case "48": symbol = "\u2663" + "10"; break;
                    case "47": symbol = "\u2663" + "9"; break;
                    case "46": symbol = "\u2663" + "8"; break;
                    case "45": symbol = "\u2663" + "7"; break;
                    case "44": symbol = "\u2663" + "6"; break;
                    case "43": symbol = "\u2663" + "5"; break;
                    case "42": symbol = "\u2663" + "4"; break;
                    case "41": symbol = "\u2663" + "3"; break;
                    case "40": symbol = "\u2663" + "2"; break;
                    case "39": symbol = "\u2666" + "A"; break;
                    case "38": symbol = "\u2666" + "K"; break;
                    case "37": symbol = "\u2666" + "Q"; break;
                    case "36": symbol = "\u2666" + "J"; break;
                    case "35": symbol = "\u2666" + "10"; break;
                    case "34": symbol = "\u2666" + "9"; break;
                    case "33": symbol = "\u2666" + "8"; break;
                    case "32": symbol = "\u2666" + "7"; break;
                    case "31": symbol = "\u2666" + "6"; break;
                    case "30": symbol = "\u2666" + "5"; break;
                    case "29": symbol = "\u2666" + "4"; break;
                    case "28": symbol = "\u2666" + "3"; break;
                    case "27": symbol = "\u2666" + "2"; break;
                    case "26": symbol = "\u2665" + "A"; break;
                    case "25": symbol = "\u2665" + "K"; break;
                    case "24": symbol = "\u2665" + "Q"; break;
                    case "23": symbol = "\u2665" + "J"; break;
                    case "22": symbol = "\u2665" + "10"; break;
                    case "21": symbol = "\u2665" + "9"; break;
                    case "20": symbol = "\u2665" + "8"; break;
                    case "19": symbol = "\u2665" + "7"; break;
                    case "18": symbol = "\u2665" + "6"; break;
                    case "17": symbol = "\u2665" + "5"; break;
                    case "16": symbol = "\u2665" + "4"; break;
                    case "15": symbol = "\u2665" + "3"; break;
                    case "14": symbol = "\u2665" + "2"; break;
                    case "13": symbol = "\u2660" + "A"; break;
                    case "12": symbol = "\u2660" + "K"; break;
                    case "11": symbol = "\u2660" + "Q"; break;
                    case "10": symbol = "\u2660" + "J"; break;
                    case "9": symbol = "\u2660" + "10"; break;
                    case "8": symbol = "\u2660" + "9"; break;
                    case "7": symbol = "\u2660" + "8"; break;
                    case "6": symbol = "\u2660" + "7"; break;
                    case "5": symbol = "\u2660" + "6"; break;
                    case "4": symbol = "\u2660" + "5"; break;
                    case "3": symbol = "\u2660" + "4"; break;
                    case "2": symbol = "\u2660" + "3"; break;
                    case "1": symbol = "\u2660" + "2"; break;
                }
            }
            return symbol;
        }

        private void showDeck(string deck)
        {
            newDeck = stringToDeck(deck);
            rtb.Text = convertDeckToSymbolDeck(deck);
            rtb.SelectAll();
            rtb.SelectionFont = textFont;
            rtb.SelectionColor = System.Drawing.Color.Black;
            string text;
            for (int i = 0; i < numberOfCards; i++)
            {
                if (oldDeck[i] != newDeck[i])
                {
                    text = convertCardNumberToSymbol((newDeck[i] == numberOfCards - 1) ? "A" : ((newDeck[i] == numberOfCards) ? "B" : newDeck[i].ToString()));
                    rtb.Select(rtb.Text.LastIndexOf(text), text.Length);
                    rtb.SelectionFont = new System.Drawing.Font(
                        textFont.FontFamily,
                        textFont.Size,
                        System.Drawing.FontStyle.Bold
                    );
                }
            }
            for (int i = 0; i < rtb.Text.Length; i++)
            {
                if (i == 0)
                {
                    if (rtb.Text.Substring(i, 1).Equals("\u2666") || rtb.Text.Substring(i, 1).Equals("\u2665"))
                    {
                        rtb.Select(i, 1);
                        rtb.SelectionColor = System.Drawing.Color.Red;
                        rtb.SelectionFont = symbolFont;
                    }
                    if (rtb.Text.Substring(i, 1).Equals("\u2660") || rtb.Text.Substring(i, 1).Equals("\u2663"))
                    {
                        rtb.Select(i, 1);
                        rtb.SelectionFont = symbolFont;
                    }
                }
                else if (rtb.Text.Substring(i,1).Equals(","))
                {
                    if (rtb.Text.Substring(i+1, 1).Equals("\u2666") || rtb.Text.Substring(i+1, 1).Equals("\u2665"))
                    {
                        rtb.Select(i+1, 1);
                        rtb.SelectionColor = System.Drawing.Color.Red;
                        rtb.SelectionFont = symbolFont;
                    }
                    if (rtb.Text.Substring(i+1, 1).Equals("\u2660") || rtb.Text.Substring(i+1, 1).Equals("\u2663"))
                    {
                        rtb.Select(i+1, 1);
                        rtb.SelectionFont = symbolFont;
                    }
                }

            }
        }

        private int[] stringToDeck(string seq)
        {
            string[] sequence = seq.Split(new char[] { Convert.ToChar(",") });
            HashSet<string> set = new HashSet<string>(sequence);
            if (set.Count < sequence.Length)
            {
                sequence = new string[set.Count];
                set.CopyTo(sequence);
            }
            int[] deck = new int[numberOfCards];
            for (int i = 0; i < numberOfCards; i++)
            {
                if (sequence[i].Equals("A")) deck[i] = numberOfCards - 1;
                else if (sequence[i].Equals("B")) deck[i] = numberOfCards;
                else deck[i] = int.Parse(sequence[i]);
            }
            return deck;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (enabled)
            {
                //rtb.Rtf = @"{\rtf1\ansi " + plugin.GetDeck(numberOfCards) + "}";
                oldDeck = stringToDeck(plugin.GetDeck(numberOfCards));
                showDeck(plugin.GetDeck(numberOfCards));
                string tmp = plugin.InputString;
                plugin.FormatText(ref tmp);
                textBox2.Text = tmp;
                textBox3.Text = "";
                textBox4.Text = "";
                i = 0;
                j = 0;
                button1.IsEnabled = false;
                button2.IsEnabled = true;
            }
        }
        
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (enabled)
            {
                oldDeck = stringToDeck(plugin.GetDeck(numberOfCards));
                plugin.MoveCardDown(numberOfCards - 1, numberOfCards);
                showDeck(plugin.GetDeck(numberOfCards));
                button2.IsEnabled = false;
                button3.IsEnabled = true;
                button7.IsEnabled = false;
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (enabled)
            {
                oldDeck = stringToDeck(plugin.GetDeck(numberOfCards));
                plugin.MoveCardDown(numberOfCards, numberOfCards);
                plugin.MoveCardDown(numberOfCards, numberOfCards);
                showDeck(plugin.GetDeck(numberOfCards));
                button3.IsEnabled = false;
                button4.IsEnabled = true;
            }
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (enabled)
            {
                oldDeck = stringToDeck(plugin.GetDeck(numberOfCards));
                plugin.TripleCut(numberOfCards);
                showDeck(plugin.GetDeck(numberOfCards));
                button4.IsEnabled = false;
                button5.IsEnabled = true;
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            if (enabled)
            {
                oldDeck = stringToDeck(plugin.GetDeck(numberOfCards));
                plugin.CountCut(numberOfCards);
                showDeck(plugin.GetDeck(numberOfCards));
                button5.IsEnabled = false;
                button6.IsEnabled = true;
            }
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            if (enabled)
            {
                int curKey, curChar;
                curKey = plugin.GetDeck()[0];
                if (i < textBox2.Text.Length)
                {
                    curChar = ((int)textBox2.Text[i] - 64);
                    while (i < textBox2.Text.Length & curChar == -32)
                    {
                        i++;
                        curChar = ((int)textBox2.Text[i] - 64);
                    }

                    if (curKey == numberOfCards)
                        curKey = plugin.GetDeck()[numberOfCards - 1];
                    else
                        curKey = plugin.GetDeck()[curKey];

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
                        j++;
                        textBox4.Text = textBox4.Text + (char)(curChar + 64);
                        textBox3.Text = textBox3.Text + Convert.ToString(curKey);
                        if (i != textBox2.Text.Length - 1) textBox3.Text = textBox3.Text + ",";
                        if (j % 5 == 0) textBox4.Text = textBox4.Text + " ";
                        i++;
                        plugin.OutputString = textBox4.Text;
                        plugin.OutputStream = textBox3.Text;
                    }

                    button2.IsEnabled = true;
                    button6.IsEnabled = false;
                    button7.IsEnabled = true;
                }
                else
                {
                    button6.IsEnabled = false;
                    plugin.FinalDeck = plugin.GetDeck(numberOfCards);
                }
            }
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            int curKey, curChar;
            if (enabled)
            {
                button2.IsEnabled = false;
                button3.IsEnabled = false;
                button4.IsEnabled = false;
                button5.IsEnabled = false;
                button6.IsEnabled = false;
                button7.IsEnabled = false;
                for (; i < textBox2.Text.Length; i++)
                {
                    plugin.MoveCardDown(numberOfCards - 1, numberOfCards);
                    plugin.MoveCardDown(numberOfCards, numberOfCards);
                    plugin.MoveCardDown(numberOfCards, numberOfCards);
                    plugin.TripleCut(numberOfCards);
                    plugin.CountCut(numberOfCards);
                    curKey = plugin.GetDeck()[0];
                    curChar = ((int)textBox2.Text[i] - 64);
                    while (i < textBox2.Text.Length & curChar == -32)
                    {
                        i++;
                        curChar = ((int)textBox2.Text[i] - 64);
                    }

                    

                    if (curKey == numberOfCards)
                        curKey = plugin.GetDeck()[numberOfCards - 1];
                    else
                        curKey = plugin.GetDeck()[curKey];

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
                        j++;
                        textBox4.Text = textBox4.Text + (char)(curChar + 64);
                        textBox3.Text = textBox3.Text + Convert.ToString(curKey);
                        if (i != textBox2.Text.Length - 1) textBox3.Text = textBox3.Text + ",";
                        if (j != 0 & j % 5 == 0) textBox4.Text = textBox4.Text + " ";
                    }
                    else i--;
                }
                showDeck(plugin.GetDeck(numberOfCards));
                plugin.FinalDeck = plugin.GetDeck(numberOfCards);
                plugin.OutputStream = textBox3.Text;
                plugin.OutputString = textBox4.Text;
            }
        }
    }
}
