using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
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

        public SolitaireQuickWatchPresentation(Solitaire plugin)
        {
            this.plugin = plugin;
            InitializeComponent();
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

        private void showDeck(string deck)
        {
            int[] oldDeck = stringToDeck((new TextRange(textBox1.Document.ContentStart, textBox1.Document.ContentEnd)).Text);
            int[] newDeck = stringToDeck(deck);
            textBox1.Document.Blocks.Clear();
            for (int i = 0; i < numberOfCards; i++)
            {
                if (oldDeck[i] != newDeck[i])
                {
                    
                    textBox1.AppendText((newDeck[i] == numberOfCards - 1) ? "A" : ((newDeck[i] == numberOfCards) ? "B" : newDeck[i].ToString()));
                }
                else
                {
                    textBox1.AppendText((newDeck[i] == numberOfCards - 1) ? "A" : ((newDeck[i] == numberOfCards) ? "B" : newDeck[i].ToString()));
                }
                textBox1.Document.FontWeight = FontWeight.FromOpenTypeWeight(400);
                if (i != numberOfCards-1) textBox1.AppendText(",");
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
                textBox1.Document.Blocks.Clear();
                textBox1.AppendText(plugin.GetDeck(numberOfCards));
                textBox2.Text = plugin.InputString;
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
                    plugin.FinalDeck = textBox1.ToString();
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
                    showDeck(plugin.GetDeck(numberOfCards));

                }
                plugin.FinalDeck = textBox1.ToString(); 
                plugin.OutputStream = textBox3.Text;
                plugin.OutputString = textBox4.Text;
            }
        }
    }
}
