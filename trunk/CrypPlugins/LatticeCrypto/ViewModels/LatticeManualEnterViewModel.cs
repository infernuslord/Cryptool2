using System.Linq;
using System.Windows.Controls;
using LatticeCrypto.Models;
using System.Numerics;

namespace LatticeCrypto.ViewModels
{
    public class LatticeManualEnterViewModel : BaseViewModel
    {
        public LatticeND Lattice { get; set; }

        public LatticeManualEnterViewModel()
        {
            Lattice = new LatticeND(2, false);
            NotifyPropertyChanged("Lattice");
        }

        public void NewLattice(int dim)
        {
            Lattice = new LatticeND(dim, false);
            NotifyPropertyChanged("Lattice");
        }

        public LatticeND SetLattice(Grid grid, bool useRowVectors)
        {
            for (int i = 0; i < Lattice.Dim; i++)
                Lattice.Vectors[i] = new VectorND(Lattice.Dim);
            foreach (TextBox control in grid.Children.OfType<TextBox>())
                if (!useRowVectors)
                    Lattice.Vectors[Grid.GetColumn(control) / 2].values[Grid.GetRow(control)] = string.IsNullOrEmpty(control.Text) ? 0 : BigInteger.Parse(control.Text);
                else
                    Lattice.Vectors[Grid.GetRow(control) / 2].values[Grid.GetColumn(control)] = string.IsNullOrEmpty(control.Text) ? 0 : BigInteger.Parse(control.Text);
            Lattice.Determinant = Lattice.CalculateDeterminant(Lattice.Vectors);
            Lattice.AngleBasisVectors = Lattice.Vectors[0].AngleBetween(Lattice.Vectors[1]);
            Lattice.UseRowVectors = useRowVectors;
            return Lattice;
        }
    }
}
