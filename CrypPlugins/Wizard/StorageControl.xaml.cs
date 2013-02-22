using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for StorageControl.xaml
    /// </summary>
    public partial class StorageControl : UserControl
    {
        public delegate void CloseEventDelegate();

        public event CloseEventDelegate CloseEvent;

        private readonly Action<string> _setValueDelegate;
        private readonly ICollectionView _view;

        public StorageControl(string defaultValue, string defaultKey, Action<string> setValueDelegate)
        {
            InitializeComponent();

            _view = CollectionViewSource.GetDefaultView(Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage);
            _view.GroupDescriptions.Add(new PropertyGroupDescription("Key"));
            _view.SortDescriptions.Add(new SortDescription("Created", ListSortDirection.Ascending));
            KeyListBox.ItemsSource = _view;

            _setValueDelegate = setValueDelegate;
            StoreValue.Text = defaultValue;
            StoreKey.Text = defaultKey;
            RefreshSource();
            Cryptool.PluginBase.Properties.Settings.Default.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
                                                                                   {
                                                                                       if (args.PropertyName == "Wizard_Storage")
                                                                                       {
                                                                                           RefreshSource();
                                                                                       }
                                                                                   };
        }

        public StorageControl() : this(null, null, null)
        {
            LoadButton.Visibility = Visibility.Collapsed;
        }

        private void LoadButtonClicked(object sender, RoutedEventArgs e)
        {
            var selectedEntry = (StorageEntry) KeyListBox.SelectedValue;
            _setValueDelegate(selectedEntry.Value);
            OnCloseEvent();
        }

        private void AddButtonClicked(object sender, RoutedEventArgs e)
        {
            var newEntry = new StorageEntry(StoreKey.Text, StoreValue.Text, StoreDescription.Text);
            //StoreKey.Text = null;
            StoreValue.Text = null;
            StoreDescription.Text = null;

            var storage = Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage ?? new ArrayList();
            storage.Add(newEntry);
            SaveAndClose(storage);
        }

        private void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.Assert(KeyListBox.SelectedItem != null);
            var storage = Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage;
            Debug.Assert(storage != null);

            int c = 0;
            foreach (var entry in storage.Cast<StorageEntry>())
            {
                if (entry == KeyListBox.SelectedItem)
                {
                    var res = MessageBox.Show(Properties.Resources.RemoveEntryQuestion, Properties.Resources.RemoveEntry, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        storage.RemoveAt(c);
                        Save(storage);
                        RefreshSource();
                    }
                    return;
                }
                c++;
            }
        }

        private void RefreshSource()
        {
            var key = StoreKey.Text;
            _view.Refresh();

            if (KeyListBox.Items != null && !string.IsNullOrEmpty(key))
            {
                foreach (var item in KeyListBox.Items.Cast<StorageEntry>())
                {
                    if (item.Key == key)
                    {
                        KeyListBox.SelectedItem = item;
                        return;
                    }
                }
            }
        }

        private void SaveAndClose(ArrayList storage)
        {
            Save(storage);
            OnCloseEvent();
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

        private void OnCloseEvent()
        {
            if (CloseEvent != null)
            {
                CloseEvent();
            }
        }
    }
}