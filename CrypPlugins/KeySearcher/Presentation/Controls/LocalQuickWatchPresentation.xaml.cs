using System;
using System.Windows;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using KeySearcher;

namespace KeySearcherPresentation.Controls
{
    [Cryptool.PluginBase.Attributes.Localization("KeySearcher.Properties.Resources")]
    public partial class LocalQuickWatchPresentation
    {        
        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();

        public static readonly DependencyProperty IsOpenCLEnabledProperty =
            DependencyProperty.Register("IsOpenCLEnabled",
                typeof(Boolean),
                typeof(LocalQuickWatchPresentation), new PropertyMetadata(false));

        public Boolean IsOpenCLEnabled
        {
            get { return (Boolean)GetValue(IsOpenCLEnabledProperty); }
            set { SetValue(IsOpenCLEnabledProperty, value); }
        }

        public LocalQuickWatchPresentation()
        {
            InitializeComponent();
            this.DataContext = entries;
        }

        private void OpenCLPresentation_Loaded(object sender, RoutedEventArgs e)
        {
        }
        
        // Strings with nul characters are not displayed correctly in the clipboard
        string removeNuls(string s)
        {
            return s.Replace(Convert.ToChar(0x0).ToString(), "");
        }

        string entryToText(ResultEntry entry)
        {
            return "Rank: " + entry.Ranking + "\n" +
                   "Value: " + entry.Value + "\n" +
                   "Key: " + entry.Key + "\n" +
                   "Text: " + removeNuls(entry.Text);
        }

        public void ContextMenuHandler(Object sender, EventArgs eventArgs)
        {
            try
            {
                MenuItem menu = (MenuItem)((RoutedEventArgs)eventArgs).Source;
                ResultEntry entry = (ResultEntry)menu.CommandParameter;
                if (entry == null) return;
                string tag = (string)menu.Tag;

                if (tag == "copy_text")
                {
                    Clipboard.SetText(removeNuls(entry.Text));
                }
                else if (tag == "copy_value")
                {
                    Clipboard.SetText(entry.Value);
                }
                else if (tag == "copy_key")
                {
                    Clipboard.SetText(entry.Key);
                }
                else if (tag == "copy_line")
                {
                    Clipboard.SetText(entryToText(entry));
                }
                else if (tag == "copy_all")
                {
                    List<string> lines = new List<string>();
                    foreach (var e in entries) lines.Add(entryToText(e));
                    Clipboard.SetText(String.Join("\n\n", lines));
                }
            } 
            catch(Exception ex)
            {
                Clipboard.SetText("");
            }
        }
    }
}
