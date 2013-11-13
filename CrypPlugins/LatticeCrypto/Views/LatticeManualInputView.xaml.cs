using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LatticeCrypto.Models;
using LatticeCrypto.Properties;
using LatticeCrypto.ViewModels;
using System.Numerics;

namespace LatticeCrypto.Views
{
    /// <summary>
    /// Interaktionslogik für LatticeManualInputView.xaml
    /// </summary>
    public partial class LatticeManualInputView
    {
        public int dimension;
        public LatticeND oldLattice;
        public LatticeND returnLattice;
        private LatticeManualEnterViewModel viewModel;
        private readonly bool checkLengths;
        private bool useRowVectors;

        public LatticeManualInputView(int newDimension, LatticeND currentLattice, bool checkLengths)
        {
            dimension = newDimension;
            this.checkLengths = checkLengths;

            Initialized += delegate
            {
                viewModel = (LatticeManualEnterViewModel) DataContext;

                if (newDimension == currentLattice.Dim)
                {
                    viewModel.NewLattice(dimension);
                    oldLattice = currentLattice;
                    CBRowVectors.IsChecked = oldLattice.UseRowVectors;
                    BuildLatticeGrid(currentLattice, false); 
                }
                else
                {
                    viewModel.NewLattice(dimension);
                    oldLattice = null;
                    BuildLatticeGrid(null, false);
                }
            };
            InitializeComponent();
        }

        private void BuildLatticeGrid(LatticeND lattice, bool justUpdate)
        {
            if (!justUpdate)
            {
                latticeGrid.RowDefinitions.Clear();
                latticeGrid.ColumnDefinitions.Clear();

                for (int i = 0; i < dimension; i++)
                {
                    latticeGrid.RowDefinitions.Add(new RowDefinition());
                    latticeGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    latticeGrid.Children.Clear();

                    //Zusatzspalten bzw. -zeilen für Zwischenräume
                    if (i < dimension - 1 && !useRowVectors)
                        latticeGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    else if (i < dimension - 1 && useRowVectors)
                        latticeGrid.RowDefinitions.Add(new RowDefinition());
                }

                //Leere Labels für den Zwischenraum
                for (int i = 1; i < dimension*2 - 1; i = i + 2)
                {
                    for (int j = 0; j < dimension; j++)
                    {
                        Label label = new Label();
                        if (!useRowVectors)
                            label.MinWidth = 10;
                        else
                            label.MinHeight = 10;
                        Grid.SetColumn(label, !useRowVectors ? i : j);
                        Grid.SetRow(label, !useRowVectors ? j : i);
                        latticeGrid.Children.Add(label);
                    }
                }

                for (int i = 0; i < dimension; i++)
                {
                    for (int j = 0; j < dimension; j++)
                    {
                        TextBox textBox = new TextBox {MinHeight = 25, MinWidth = 50};
                        textBox.GotKeyboardFocus += (sender, args) => ((TextBox) sender).SelectAll();
                        textBox.TabIndex = !useRowVectors ? dimension*i + j : dimension*j + i;

                        if (lattice != null)
                            textBox.Text = lattice.Vectors[!useRowVectors ? i : j].values[!useRowVectors ? j : i].ToString(CultureInfo.InvariantCulture);
                        textBox.TextChanged += ValidateLattice;
                        Grid.SetColumn(textBox, !useRowVectors ? 2*i : i);
                        Grid.SetRow(textBox, !useRowVectors ? j : 2*j);
                        latticeGrid.Children.Add(textBox);
                    }
                }
            }
            else
            {
                //Beim Transponieren muss nicht die komplette Grid neu gebaut werden, es reicht ein Update
                foreach (TextBox textBox in latticeGrid.Children.OfType<TextBox>())
                {
                    textBox.TextChanged -= ValidateLattice;
                    int col = Grid.GetColumn(textBox);
                    int row = Grid.GetRow(textBox);
                    textBox.Text = lattice.Vectors[!useRowVectors ? col/2 : row/2].values[!useRowVectors ? row : col].ToString(CultureInfo.InvariantCulture);
                    textBox.TextChanged += ValidateLattice;
                }
            }
        }

        private void ValidateLattice(object sender, TextChangedEventArgs e)
        {
            errorText.Text = "";
            buttonOK.IsEnabled = true;

            BigInteger tryParse;
            if (latticeGrid.Children.Cast<Control>().Any(control => control is TextBox && !((TextBox)control).Text.Equals("") && !BigInteger.TryParse(((TextBox)control).Text, out tryParse)))
            {
                errorText.Text = Languages.errorOnlyIntegersAllowed;
                buttonOK.IsEnabled = false;
                return;
            }
            if (latticeGrid.Children.Cast<Control>().Any(control => control is TextBox && ((TextBox)control).Text.Equals("")))
            {
                errorText.Text = Languages.errorNoLatticeEntered;
                buttonOK.IsEnabled = false;
                return;
            }
            LatticeND newLattice = viewModel.SetLattice(latticeGrid, CBRowVectors.IsChecked != null && (bool)CBRowVectors.IsChecked);
            if (oldLattice != null && newLattice.Equals(oldLattice))
            {
                errorText.Text = Languages.errorSameLattice;
                return;
            }
            if (newLattice.Determinant == 0)
            {
                errorText.Text = Languages.errorVectorsDependent;
                buttonOK.IsEnabled = false;
            }
            if (!checkLengths) return;
            if ((newLattice.Vectors[0].Length > newLattice.Vectors[1].Length && newLattice.Vectors[0].Length > 1000 * newLattice.Vectors[1].Length)
                || newLattice.Vectors[1].Length > newLattice.Vectors[0].Length && newLattice.Vectors[1].Length > 1000 * newLattice.Vectors[0].Length)
            {
                errorText.Text = Languages.errorBadLattice;
                //buttonOK.IsEnabled = false;
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            returnLattice = viewModel.SetLattice(latticeGrid, useRowVectors);
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonTranspose_Click(object sender, RoutedEventArgs e)
        {
            LatticeND tempLattice = viewModel.SetLattice(latticeGrid, useRowVectors);
            tempLattice.Transpose();
            BuildLatticeGrid(tempLattice, true);
        }

        private void CBRowVectors_Checked(object sender, RoutedEventArgs e)
        {
            useRowVectors = true;
            BuildLatticeGrid(viewModel.SetLattice(latticeGrid, false), false);
        }

        private void CBRowVectors_Unchecked(object sender, RoutedEventArgs e)
        {
            useRowVectors = false;
            BuildLatticeGrid(viewModel.SetLattice(latticeGrid, true), false);
        }
    }
}
