using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace TranspositionAnalyser
{
    /// <summary>
    /// Interaktionslogik für TranspositionAnalyserQuickWatchPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("TranspositionAnalyser.Properties.Resources")]
    public partial class TranspositionAnalyserQuickWatchPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();
        public event EventHandler doppelClick;

        // Alphabets used for converting the numeric key to a key word
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"; // for key sizes <= 52
        const string alphabet2 = "0123456789" + alphabet;                               // for key sizes <= 62 (use numbers only if letters do not suffice)

        public TranspositionAnalyserQuickWatchPresentation()
        {
            InitializeComponent();
            this.DataContext = entries;
        }

        // Convert the numeric key to a keyword based upon the alphabet string
        string getKeyword(byte[] key)
        {
            if (key.Length >= alphabet2.Length) return null;
            string abc = (key.Length <= alphabet.Length) ? alphabet : alphabet2;
            string keyword = "";
            foreach (var i in key) keyword += abc[i];
            return keyword;
        }
        
        string entryToText(ResultEntry entry)
        {
            string keyword = getKeyword(entry.KeyArray);

            string key = String.IsNullOrEmpty(keyword)
                ? "Key: " + String.Join(" ", entry.KeyArray)
                : "Key (numeric): " + String.Join(" ", entry.KeyArray) + "\n" + "Key (alphabetic): " + keyword;

            return "Rank: " + entry.Ranking + "\n" +
                   "Value: " + entry.Value + "\n" +
                   key + "\n" +
                   "Text: " + entry.Text;
        }

        public void ContextMenuHandler(Object sender, EventArgs eventArgs)
        {
            MenuItem menu = (MenuItem)((RoutedEventArgs)eventArgs).Source;
            ResultEntry entry = (ResultEntry)menu.CommandParameter;
            if (entry == null) return;

            if ((string)(menu.Tag) == "copy_text")
            {
                Clipboard.SetText(entry.Text);
            }
            else if ((string)(menu.Tag) == "copy_value")
            {
                Clipboard.SetText(entry.Value);
            }
            else if ((string)(menu.Tag) == "copy_key")
            {
                Clipboard.SetText(entry.Key);
            }
            else if ((string)(menu.Tag) == "copy_line")
            {
                Clipboard.SetText(entryToText(entry));
            }
            else if ((string)(menu.Tag) == "copy_all")
            {
                List<string> lines = new List<string>();
                foreach (var e in entries) lines.Add(entryToText(e));
                Clipboard.SetText(String.Join("\n\n",lines));
            }
        }

        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            doppelClick(sender, eventArgs);
        }
    }
}
