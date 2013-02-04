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

        public void AddContent(Control content, string defaultKey)
        {
            StorageContainerContent.Content = content;
            _defaultKey = defaultKey;
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
    }
}
