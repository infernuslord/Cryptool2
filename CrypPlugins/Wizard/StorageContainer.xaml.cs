using System;
using System.Collections;
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

namespace Wizard
{
    /// <summary>
    /// Interaction logic for StorageContainer.xaml
    /// </summary>
    public partial class StorageContainer : UserControl
    {
        private Action<string> _setValueDelegate;
        private Func<string> _getValueDelegate;
        private string _defaultKey;

        public StorageContainer()
        {
            InitializeComponent();
        }

        public void AddContent(Control content, string defaultKey, bool defaultKeyOnly)
        {
            StorageContainerContent.Content = content;
            _defaultKey = defaultKey;
            StorageButton.Visibility = defaultKeyOnly ? Visibility.Collapsed : Visibility.Visible;
            LoadButton.Visibility = defaultKeyOnly ? Visibility.Visible : Visibility.Collapsed;
            SaveButton.Visibility = LoadButton.Visibility;
        }

        public void SetValueMethod(Action<string> setValueDelegate)
        {
            _setValueDelegate = setValueDelegate;
        }

        public void GetValueMethod(Func<string> getValueDelegate)
        {
            _getValueDelegate = getValueDelegate;
        }

        private void StorageButtonClicked(object sender, RoutedEventArgs e)
        {
            var storageWindow = new StorageWindow(_getValueDelegate, _setValueDelegate, _defaultKey) { Owner = Application.Current.MainWindow };
            storageWindow.ShowDialog();
        }

        private void SaveButtonClicked(object sender, RoutedEventArgs e)
        {
            var key = _defaultKey;
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
                        Save(storage);
                    }
                    return;
                }
                c++;
            }

            storage.Insert(0, newEntry);
            Save(storage);
        }

        private static void Save(ArrayList storage)
        {
            Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage = storage;
            Cryptool.PluginBase.Properties.Settings.Default.Save();
        }

        private void LoadButtonClicked(object sender, RoutedEventArgs e)
        {
            var storage = Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage;
            if (storage != null)
            {
                foreach (var entry in storage.Cast<StorageEntry>())
                {
                    if (entry.Key == _defaultKey)
                    {
                        _setValueDelegate(entry.Value);
                        return;
                    }
                }
            }
            MessageBox.Show("No stored value available.", "No value", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
