using System;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using LatticeCrypto.ViewModels;
using Microsoft.Win32;

namespace LatticeCrypto.Views
{
    /// <summary>
    /// Interaktionslogik für SvpLLLView.xaml
    /// </summary>
    public partial class SvpLLLView : ILatticeCryptoUserControl
    {
        private SvpLLLViewModel viewModel;

        public SvpLLLView()
        {
            Initialized += delegate
                               {
                                   History.Document.Blocks.Clear();
                                   viewModel = (SvpLLLViewModel) DataContext;
                                   viewModel.History = History;
                                   viewModel.SetInitialNDLattice();
                                   UpdateTextBoxes();
                               };
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            viewModel.GenerateNewLattice((int)scrollBar.Value, BigInteger.Parse(textRangeStart.Text), BigInteger.Parse(textRangeEnd.Text));
            UpdateTextBoxes();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LatticeManualInputView inputView = new LatticeManualInputView((int)scrollBar.Value, viewModel.Lattice, false);
            if (inputView.ShowDialog() != true) return;
            Cursor = Cursors.Wait;
            viewModel.SetLatticeManually(inputView.returnLattice);
            UpdateTextBoxes();
            Cursor = Cursors.Arrow;
        }

        private void ButtonLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*" };
            if (openFileDialog.ShowDialog() == false) return;
            string firstLine;
            try
            {
                firstLine = File.ReadAllLines(openFileDialog.FileName)[0];
            }
            catch (IOException)
            {
                MessageBox.Show(Languages.errorLoadingFile, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                viewModel.SetLatticeManually(Util.ConvertStringToLatticeND(firstLine));
                scrollBar.Value = viewModel.Lattice.Dim;
                UpdateTextBoxes();
            }
            catch (Exception)
            {
                MessageBox.Show(Languages.errorParsingLattice, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ButtonLoadFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                String str = Clipboard.GetText();
                viewModel.SetLatticeManually(Util.ConvertStringToLatticeND(str));
                scrollBar.Value = viewModel.Lattice.Dim;
                UpdateTextBoxes();
            }
            catch (Exception)
            {
                MessageBox.Show(Languages.errorParsingLattice, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UpdateTextBoxes()
        {
            if (leftGrid.RowDefinitions.Count != viewModel.Lattice.Dim)
            {
                leftGrid.RowDefinitions.Clear();
                leftGrid.ColumnDefinitions.Clear();
                rightGrid.RowDefinitions.Clear();
                rightGrid.ColumnDefinitions.Clear();

                for (int i = 0; i < viewModel.Lattice.Dim; i++)
                {
                    leftGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
                    leftGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    rightGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
                    rightGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }

                leftGrid.Children.Clear();
                rightGrid.Children.Clear();

                for (int i = 0; i < viewModel.Lattice.Dim; i++)
                {
                    for (int j = 0; j < viewModel.Lattice.Dim; j++)
                    {
                        TextBlock leftTextBlock = new TextBlock
                        {
                            Text = Util.FormatBigInt(viewModel.Lattice.Vectors[i].values[j]),
                            Margin = new Thickness(10, 0, 10, 0),
                            TextAlignment = TextAlignment.Right
                        };
                        Grid.SetColumn(leftTextBlock, !viewModel.Lattice.UseRowVectors ? i : j);
                        Grid.SetRow(leftTextBlock, !viewModel.Lattice.UseRowVectors ? j : i);
                        leftGrid.Children.Add(leftTextBlock);

                        TextBlock rightTextBlock = new TextBlock
                        {
                            Text = Util.FormatBigInt(viewModel.Lattice.ReducedVectors[i].values[j]),
                            Margin = new Thickness(10, 0, 10, 0),
                            TextAlignment = TextAlignment.Right
                        };
                        Grid.SetColumn(rightTextBlock, !viewModel.Lattice.UseRowVectors ? i : j);
                        Grid.SetRow(rightTextBlock, !viewModel.Lattice.UseRowVectors ? j : i);
                        rightGrid.Children.Add(rightTextBlock);
                    }
                }
            }
            else
            {
                if (!viewModel.Lattice.UseRowVectors)
                {
                    foreach (TextBlock textBlock in leftGrid.Children)
                        textBlock.Text = Util.FormatBigInt(viewModel.Lattice.Vectors[Grid.GetColumn(textBlock)].values[Grid.GetRow(textBlock)]);
                    foreach (TextBlock textBlock in rightGrid.Children)
                        textBlock.Text = Util.FormatBigInt(viewModel.Lattice.ReducedVectors[Grid.GetColumn(textBlock)].values[Grid.GetRow(textBlock)]);
                }
                else
                {
                    foreach (TextBlock textBlock in leftGrid.Children)
                        textBlock.Text = Util.FormatBigInt(viewModel.Lattice.Vectors[Grid.GetRow(textBlock)].values[Grid.GetColumn(textBlock)]);
                    foreach (TextBlock textBlock in rightGrid.Children)
                        textBlock.Text = Util.FormatBigInt(viewModel.Lattice.ReducedVectors[Grid.GetRow(textBlock)].values[Grid.GetColumn(textBlock)]);
                }
            }
        }


        private void ButtonLog_Click(object sender, RoutedEventArgs e)
        {
            rowLog.Height = rowLog.Height == new GridLength(0) ? new GridLength(55) : new GridLength(0);
        }

        private void ValidateCodomain(object sender, TextChangedEventArgs e)
        {
            if (errorText == null || buttonGenerate == null) return;
            errorText.Text = "";
            buttonGenerate.IsEnabled = true;
            errorText.Visibility = Visibility.Collapsed;

            if (textRangeStart.Text.Equals("") || textRangeEnd.Text.Equals(""))
            {
                errorText.Text = Languages.errorNoCodomain;
                buttonGenerate.IsEnabled = false;
                errorText.Visibility = Visibility.Visible;
                return;
            }

            BigInteger tryParseStart, tryParseEnd;
            if (!BigInteger.TryParse(textRangeStart.Text, out tryParseStart) || !BigInteger.TryParse(textRangeEnd.Text, out tryParseEnd))
            {
                errorText.Text = Languages.errorOnlyIntegersAllowed;
                buttonGenerate.IsEnabled = false;
                errorText.Visibility = Visibility.Visible;
                return;
            }
            if (tryParseStart < tryParseEnd) return;
            errorText.Text = Languages.errorFromBiggerThanTo;
            buttonGenerate.IsEnabled = false;
            errorText.Visibility = Visibility.Visible;
        }

        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (rowLattice.Height == new GridLength(0))
            {
                rowLattice.Height = new GridLength(1, GridUnitType.Star);
                rowLog.Height = new GridLength(55);
            }
            else
            {
                rowLattice.Height = new GridLength(0);
                rowLog.Height = new GridLength(1, GridUnitType.Star);
            }
        }

        private void Button_Help_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Equals(sender, btnHelpCodomain))
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(OnlineHelp.OnlineHelpActions.CodomainLLL);
            }
            else if (Equals(sender, btnHelpDimension))
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(OnlineHelp.OnlineHelpActions.DimensionLLL);
            }
            e.Handled = true;
        }

        private void History_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (History.Text.EndsWith("\r\n"))
            //    History.ScrollToEnd();
        }

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
    }
}
