using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LatticeCrypto.Utilities;
using LatticeCrypto.ViewModels;

namespace LatticeCrypto.Views
{
    /// <summary>
    /// Interaktionslogik für MerkleHellman.xaml
    /// </summary>
    public partial class MerkleHellmanView : ILatticeCryptoUserControl
    {
        private MerkleHellmanViewModel viewModel;

        public MerkleHellmanView()
        {
            Initialized += delegate
            {
                History.Document.Blocks.Clear();
                viewModel = (MerkleHellmanViewModel) DataContext;
                viewModel.History = History;
                viewModel.GenerateNewMerkleHellman((int)scrollBar.Value);
            };

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            viewModel.GenerateNewMerkleHellman((int)scrollBar.Value);
        }

        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (rowMarkleHellman.Height == new GridLength(0))
            {
                rowMarkleHellman.Height = new GridLength(1, GridUnitType.Star);
                rowLog.Height = new GridLength(55);
            }
            else
            {
                rowMarkleHellman.Height = new GridLength(0);
                rowLog.Height = new GridLength(1, GridUnitType.Star);
            }
        }
        

        private void History_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (History.Text.EndsWith("\r\n"))
            //    History.ScrollToEnd();
        }

        #region Implementation of ILatticeCryptoUserControl

        public void Dispose()
        {
            //throw new System.NotImplementedException();
        }

        public void Init()
        {
            //throw new System.NotImplementedException();
        }

        public void SetTab(int i)
        {
            //throw new System.NotImplementedException();
        }

        #endregion
    }
}
