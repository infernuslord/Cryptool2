using System;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LatticeCrypto.Models;
using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using LatticeCrypto.ViewModels;
using Microsoft.Win32;

namespace LatticeCrypto.Views
{
    /// <summary>
    /// Interaktionslogik für LatticeBasicsView.xaml
    /// </summary>
    public partial class CvpView : ILatticeCryptoUserControl
    {
        private Point point;
        private CvpViewModel viewModel;

        public CvpView()
        {
            Initialized += delegate
            {
                History.Document.Blocks.Clear();
                viewModel = (CvpViewModel) DataContext;
                viewModel.History = History;
                viewModel.canvas = canvas;
                viewModel.SetInitialNDLattice();
                viewModel.CalculatePixelsPerPoint();
                UpdateTextBoxes();
            };

            InitializeComponent();

            SizeChanged += delegate
            {
                if (rowLattice.Height == new GridLength(0)) return;
                viewModel.CalculatePixelsPerPoint();
                viewModel.GenerateLatticePoints(false, false);
                ValidateTargetVector(false);
                viewModel.FindClosestVector(false);
                viewModel.UpdateCanvas();
            };
            canvas.SizeChanged += delegate
            {
                viewModel.GenerateLatticePoints(false, false);
                ValidateTargetVector(false);
                viewModel.FindClosestVector(false);
                viewModel.UpdateCanvas();
            };
        }


        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Grid.IsMouseCaptured) return;

            viewModel.SetCanvasPosition(point, e.GetPosition(this));
            viewModel.GenerateLatticePoints(false, false);
            viewModel.FindClosestVector(false);
            viewModel.UpdateCanvas();
        }


        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            viewModel.SetCanvasTransform(canvas.RenderTransform);
            Grid.ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
        }

        private void ButtonCamera_Click(object sender, RoutedEventArgs e)
        {
            Util.CreateSaveBitmap(canvas);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                viewModel.ResetCanvasPosition();
                viewModel.GenerateNewLattice(2, BigInteger.Parse(textRangeStart.Text), BigInteger.Parse(textRangeEnd.Text));
                UpdateTextBoxes();
                viewModel.CalculatePixelsPerPoint();
                viewModel.GenerateLatticePoints(true, false);
                viewModel.FindClosestVector(false);
                viewModel.UpdateCanvas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0 && zoomIn.Command.CanExecute(null))
                zoomIn.Command.Execute(null);
            else if (e.Delta < 0 && zoomOut.Command.CanExecute(null))
                zoomOut.Command.Execute(null);
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            point = e.GetPosition(this);
            Grid.CaptureMouse();
            Cursor = Cursors.SizeAll;
        }

        private void Button_CVP_Click(object sender, RoutedEventArgs e)
        {
            viewModel.GenerateLatticePoints(false, false);
            ValidateTargetVector(true);
            viewModel.FindClosestVector(true);
            viewModel.UpdateCanvas();
        }

        private void ValidateTargetVector(bool showMessageBox)
        {
            if (showMessageBox && (string.IsNullOrEmpty(textTargetVectorX.Text) || string.IsNullOrEmpty(textTargetVectorY.Text)))
            {
                MessageBox.Show(Languages.errorNoTargetPoint, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            try
            {
                viewModel.TargetVectorX = int.Parse(textTargetVectorX.Text);
                viewModel.TargetVectorY = int.Parse(textTargetVectorY.Text);
            }
            catch (Exception)
            {
                viewModel.TargetVectorX = null;
                viewModel.TargetVectorY = null;
                if (showMessageBox)
                    MessageBox.Show(Languages.errorOnlyIntegersAllowed, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LatticeManualInputView inputView = new LatticeManualInputView(2, viewModel.Lattice, true);
            if (inputView.ShowDialog() != true) return;
            viewModel.ResetCanvasPosition();
            viewModel.SetLatticeManually(inputView.returnLattice);
            viewModel.CalculatePixelsPerPoint();
            viewModel.GenerateLatticePoints(true, false);
            viewModel.FindClosestVector(false);
            viewModel.UpdateCanvas();
            UpdateTextBoxes();
        }

        private void UpdateTextBoxes()
        {
            LatticeND lattice = viewModel.Lattice;

            textReduced00.Text = Util.FormatBigInt(lattice.ReducedVectors[0].values[0]);
            textReduced11.Text = Util.FormatBigInt(lattice.ReducedVectors[1].values[1]);

            if (!lattice.UseRowVectors)
            {
                textReduced01.Text = Util.FormatBigInt(lattice.ReducedVectors[0].values[1]);
                textReduced10.Text = Util.FormatBigInt(lattice.ReducedVectors[1].values[0]);
                SepColumnRed.Width = new GridLength(10);
                SepRowRed.Height = new GridLength(0);
            }
            else
            {
                textReduced01.Text = Util.FormatBigInt(lattice.ReducedVectors[1].values[0]);
                textReduced10.Text = Util.FormatBigInt(lattice.ReducedVectors[0].values[1]);
                SepColumnRed.Width = new GridLength(0);
                SepRowRed.Height = new GridLength(10);
            }

            textReducedLength0.Text = Util.FormatDoubleGUI(lattice.ReducedVectors[0].Length);
            textReducedLength1.Text = Util.FormatDoubleGUI(lattice.ReducedVectors[1].Length);
        }

        private void History_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (History.Text.EndsWith("\n\n"))
            //    History.ScrollToEnd();
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
            if (tryParseStart >= tryParseEnd)
            {
                errorText.Text = Languages.errorFromBiggerThanTo;
                buttonGenerate.IsEnabled = false;
                errorText.Visibility = Visibility.Visible;
                return;
            }
            BigInteger maxCodomain = BigInteger.Max(BigInteger.Abs(tryParseStart), BigInteger.Abs(tryParseEnd));
            BigInteger range = BigInteger.Abs(tryParseEnd - tryParseStart);
            if (range * 5 >= maxCodomain) return;
            errorText.Text = Languages.errorCodomainTooSmall;
            errorText.Visibility = Visibility.Visible;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
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
                viewModel.ResetCanvasPosition();
                viewModel.CalculatePixelsPerPoint();
                viewModel.GenerateLatticePoints(true, true);
                UpdateTextBoxes();
            }
            catch (Exception)
            {
                MessageBox.Show(Languages.errorParsingLattice, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                String str = Clipboard.GetText();
                viewModel.SetLatticeManually(Util.ConvertStringToLatticeND(str));
                viewModel.ResetCanvasPosition();
                viewModel.CalculatePixelsPerPoint();
                viewModel.GenerateLatticePoints(true, true);
                UpdateTextBoxes();
            }
            catch (Exception)
            {
                MessageBox.Show(Languages.errorParsingLattice, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Help_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Equals(sender, btnHelpCodomain))
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(OnlineHelp.OnlineHelpActions.Codomain);
            else if (Equals(sender, btnHelpSuccessiveMinima))
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(OnlineHelp.OnlineHelpActions.SuccessiveMinima);
            else if (Equals(sender, btnTargetPoint))
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(OnlineHelp.OnlineHelpActions.TargetPoint);
            e.Handled = true;
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
