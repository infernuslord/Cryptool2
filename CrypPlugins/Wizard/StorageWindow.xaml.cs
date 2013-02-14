using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Wizard
{
    /// <summary>
    /// Interaction logic for StorageWindow.xaml
    /// </summary>
    public partial class StorageWindow : Window
    {
        private readonly Func<string> _getValueDelegate;
        private readonly Action<string> _setValueDelegate;

        public StorageWindow(Func<string> getValueDelegate, Action<string> setValueDelegate, string defaultKey)
        {
            _getValueDelegate = getValueDelegate;
            _setValueDelegate = setValueDelegate;

            InitializeComponent();
            StoreKey.Text = defaultKey;
            KeyListBox.ItemsSource = Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage;
            if (KeyListBox.Items != null)
            {
                foreach (var item in KeyListBox.Items.Cast<StorageEntry>())
                {
                    if (item.Key == defaultKey)
                    {
                        KeyListBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void LoadButtonClicked(object sender, RoutedEventArgs e)
        {
            var selectedEntry = (StorageEntry) KeyListBox.SelectedValue;
            _setValueDelegate(selectedEntry.Value);
            DialogResult = true;
            Close();
        }

        private void StoreButtonClicked(object sender, RoutedEventArgs e)
        {
            var key = StoreKey.Text;
            var newEntry = new StorageEntry(key, _getValueDelegate());

            var storage = Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage;
            if (storage == null)
            {
                storage = new ArrayList();
            }
            int c = 0;
            foreach (var entry in storage.Cast<StorageEntry>())
            {
                if (entry.Key == key)
                {
                    var res = MessageBox.Show("An entry with this key already exists. Overwrite?", "Key already exists", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        storage[c] = newEntry;
                        SaveAndClose(storage);
                    }
                    return;
                }
                c++;
            }

            storage.Insert(0, newEntry);
            SaveAndClose(storage);
        }

        private void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.Assert(KeyListBox.SelectedItem != null);
            var key = ((StorageEntry) KeyListBox.SelectedItem).Key;
            var storage = Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage;
            Debug.Assert(storage != null);

            int c = 0;
            foreach (var entry in storage.Cast<StorageEntry>())
            {
                if (entry.Key == key)
                {
                    var res = MessageBox.Show("Are you sure you want to remove the selected key?", "Remove key", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        storage.RemoveAt(c);
                        Save(storage);
                        KeyListBox.ItemsSource = null;
                        KeyListBox.ItemsSource = Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage;
                    }
                    return;
                }
                c++;
            }
        }

        private void SaveAndClose(ArrayList storage)
        {
            Save(storage);
            DialogResult = true;
            Close();
        }

        private static void Save(ArrayList storage)
        {
            Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage = storage;
            Cryptool.PluginBase.Properties.Settings.Default.Save();
        }

        private void KeyListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (KeyListBox.SelectedItem != null)
            {
                StoreKey.Text = ((StorageEntry) KeyListBox.SelectedItem).Key;
            }
        }
    }
}