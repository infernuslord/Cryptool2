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

        public TranspositionAnalyserQuickWatchPresentation()
        {
            InitializeComponent();
            this.DataContext = entries;
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
            else if ((string)(menu.Tag) == "copy_all")
            {
                string abc = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                string key = "";
                if (entry.KeyArray.Length <= abc.Length)
                {
                    foreach(var i in entry.KeyArray)
                        key += abc[i];
                    key = "Key (numeric): " + String.Join(" ", entry.KeyArray) + "\n" + "Key (alphabetic): " + key;
                } else {
                   key = "Key: " + String.Join(" ", entry.KeyArray);
                }
                Clipboard.SetText(
                   "Rank: " + entry.Ranking + "\n" +
                   "Value: " + entry.Value + "\n" +
                   key + "\n" +
                   "Text: " + entry.Text);
            }
        }

        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            doppelClick(sender, eventArgs);
        }
    }
}
