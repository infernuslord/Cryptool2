/*
   Copyright 2014 Nils Kopal, Applied Information Security, Uni Kassel
   http://www.uni-kassel.de/eecs/fachgebiete/ais/mitarbeiter/nils-kopal-m-sc.html

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
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Threading;

namespace Cryptool.VigenereAnalyzer
{
    /// <summary>
    /// Interaktionslogik für AssignmentPresentation.xaml
    /// </summary>
    [PluginBase.Attributes.Localization("Cryptool.VigenereAnalyzer.Properties.Resources")]
    public partial class AssignmentPresentation : UserControl
    {

        public ObservableCollection<ResultEntry> BestList = new ObservableCollection<ResultEntry>();
        //public event EventHandler doppelClick;

        #region Variables

        private UpdateOutput _updateOutputFromUserChoice;

        #endregion

        #region Properties

        public UpdateOutput UpdateOutputFromUserChoice
        {
            get { return _updateOutputFromUserChoice; }
            set { _updateOutputFromUserChoice = value; }
        }

        #endregion

        #region constructor

        public AssignmentPresentation()
        {
            InitializeComponent();
            DataContext = BestList;
        }

        #endregion

        #region Main Methods

        public void DisableGUI()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ListView.IsEnabled = false;
            }, null);
        }

        public void EnableGUI()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ListView.IsEnabled = true;
            }, null);
        }

        #endregion

        #region Helper Methods

        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            var lvi = sender as ListViewItem;
            var r = lvi.Content as ResultEntry;

            if (r != null)
            {
                _updateOutputFromUserChoice(r.Key, r.Text);
            }
        }

        public void HandleSingleClick(Object sender, EventArgs eventArgs)
        {
            
        }

        // Strings with nul characters are not displayed correctly in the clipboard
        string removeNuls(string s)
        {
            return s.Replace(Convert.ToChar(0x0).ToString(), "");
        }

        string entryToText(ResultEntry entry)
        {
            return "Rank: " + entry.Ranking + Environment.NewLine +
                   "Value: " + entry.ExactValue + Environment.NewLine +
                   "Key: " + entry.Key + Environment.NewLine +
                   "KeyLength: " + entry.KeyLength + Environment.NewLine +
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
                    Clipboard.SetText("" + entry.Value);
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
                    foreach (var e in BestList) lines.Add(entryToText(e));
                    Clipboard.SetText(String.Join(Environment.NewLine, lines));
                }
            }
            catch (Exception ex)
            {
                Clipboard.SetText("");
            }
        }

        #endregion
    }
}
