/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Data;
using System.ComponentModel;
using Cryptool.PluginBase;

namespace Cryptool.Alphabets
{
    /// <summary>
    /// Interaction logic for AlphabetPresentation.xaml
    /// </summary>
    public partial class AlphabetPresentation : UserControl
    {
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private Alphabet alphabet;
        private DataTable tbl;
        private ContextMenu contextMenu;

        public AlphabetPresentation(Alphabet Alphabet)
        {
            InitializeComponent();
            this.alphabet = Alphabet;
            Binding bindingDefaultAlphabet = new Binding();
            bindingDefaultAlphabet.Mode = BindingMode.OneWay;
            bindingDefaultAlphabet.Path = new PropertyPath("DefaultAlphabet");
            bindingDefaultAlphabet.Source = this.alphabet.Settings;
            textBoxCurrentAlphabet.SetBinding(TextBox.TextProperty, bindingDefaultAlphabet);

            Binding bindingAlphabet = new Binding();
            bindingAlphabet.Mode = BindingMode.TwoWay;
            bindingAlphabet.Path = new PropertyPath("Alphabet");
            bindingAlphabet.Source = this.alphabet.Settings;
            textBoxAlphabet.SetBinding(TextBox.TextProperty, bindingAlphabet);

            listViewAlphabets.DataContext = createAlphabetTable();

            contextMenu = new ContextMenu();
            MenuItem item = new MenuItem();
            item.Header = "Delete";
            item.Click += new RoutedEventHandler(item_Click);
            contextMenu.Items.Add(item);

            listViewAlphabets.ContextMenu = contextMenu;

            //add default alphabet
            this.tbl.Rows.Add("X", "Default", ((AlphabetSettings)this.alphabet.Settings).Alphabet);

            controlStatus();
            Alphabet.Settings.PropertyChanged += Settings_PropertyChanged;
        }

        void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
          if (e.PropertyName == "OpenFilename")
          {
            this.load(((AlphabetSettings)this.alphabet.Settings).OpenFilename);
          }
          if (e.PropertyName == "TargetFilename")
          {
            this.save(((AlphabetSettings)this.alphabet.Settings).TargetFilename);
          }
        }

        private void item_Click(object sender, RoutedEventArgs e)
        {
            this.tbl.Rows[listViewAlphabets.SelectedIndex].Delete();
        }

        private DataTable createAlphabetTable()
        {
            tbl = new DataTable();
            tbl.Columns.Add("Default", typeof(string));
            tbl.Columns.Add("Name", typeof(string));
            tbl.Columns.Add("Alphabet", typeof(string));

            return tbl;
        }

        private void controlStatus()
        {
            buttonAddAlphabet.IsEnabled = (textBoxAlphabet.Text.Length != 0 && textBoxAlphabetName.Text.Length != 0);
        }

        private void save(string fileName)
        {
          try
          {
            if (fileName != null && fileName != string.Empty)
            {
              StreamWriter writer = new StreamWriter(fileName);
              foreach (DataRow row in this.tbl.Rows)
              {
                writer.WriteLine(row["Name"] + ";" + row["Alphabet"]);
              }
              writer.Close();
              GuiLogMessage("Alphabets saved to file: " + fileName, NotificationLevel.Info);
            }            
          }
          catch (Exception exception)
          {
            GuiLogMessage(exception.Message, NotificationLevel.Error);
          }
        }

        private void buttonAddAlphabet_Click(object sender, RoutedEventArgs e)
        {
            string alphName = textBoxAlphabetName.Text;
            foreach (DataRow row in this.tbl.Rows)
            {
                if ((string)row["Name"] == alphName)
                {
                    alphName += "_1";
                    break;
                }
            }

            this.tbl.Rows.Add(string.Empty, alphName, textBoxAlphabet.Text);
            textBoxAlphabetName.Clear();
            controlStatus();
        }

        private void textBoxAlphabet_TextChanged(object sender, TextChangedEventArgs e)
        {
            controlStatus();
        }

        private void load(string fileName)
        {
          try
          {
            if (File.Exists(fileName))
            {
              this.tbl.Clear();
              StreamReader reader = new StreamReader(fileName);
              string sLine;
              while ((sLine = reader.ReadLine()) != null)
              {
                string[] items = sLine.Split(';');
                this.tbl.Rows.Add(null, items[0], items[1]);
              }
              reader.Close();
              controlStatus();
              if (tbl.Rows.Count > 0)
              {
                tbl.Rows[0]["Default"] = "X";
                ((AlphabetSettings)this.alphabet.Settings).DefaultAlphabet = (string)tbl.Rows[0]["Alphabet"];
              }

              GuiLogMessage("Alphabets loaded from file: " + fileName + ".", NotificationLevel.Info);
            }

          }
          catch (Exception exception)
          {
            GuiLogMessage(exception.Message, NotificationLevel.Error);
          }
        }

        private void listViewAlphabets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.tbl.Rows.Count > 0 && listViewAlphabets.SelectedIndex != -1)
            {
                ((AlphabetSettings)this.alphabet.Settings).DefaultAlphabet = (string)tbl.Rows[listViewAlphabets.SelectedIndex]["Alphabet"];
                for (int i = 0; i < tbl.Rows.Count; i++)
                {
                    tbl.Rows[i]["Default"] = string.Empty;
                }
                tbl.Rows[listViewAlphabets.SelectedIndex]["Default"] = "X";
            }
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
          if (OnGuiLogNotificationOccured != null)
          {
            OnGuiLogNotificationOccured(null, new GuiLogEventArgs(message, null, logLevel));
          }
        }
    }
}
