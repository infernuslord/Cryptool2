﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LatticeCrypto.Utilities;
using LatticeCrypto.ViewModels;

namespace LatticeCrypto.Views
{
    /// <summary>
    /// Interaktionslogik für GGHView.xaml
    /// </summary>
    public partial class GGHView : ILatticeCryptoUserControl
    {
        private GGHViewModel viewModel;

        public GGHView()
        {
            Initialized += delegate
            {
                History.Document.Blocks.Clear();
                viewModel = (GGHViewModel)DataContext;
                viewModel.History = History;
                viewModel.LeftGrid = leftGrid;
                viewModel.RightGrid = rightGrid;
                viewModel.GenerateNewGGH((int)scrollBar.Value, (int)scrollBar2.Value);
                viewModel.UpdateTextBoxes();
            };

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            viewModel.GenerateNewGGH((int)scrollBar.Value, (int)scrollBar2.Value);
            viewModel.UpdateTextBoxes();
        }

        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (rowGGH.Height == new GridLength(0))
            {
                rowGGH.Height = new GridLength(1, GridUnitType.Star);
                rowLog.Height = new GridLength(55);
            }
            else
            {
                rowGGH.Height = new GridLength(0);
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
