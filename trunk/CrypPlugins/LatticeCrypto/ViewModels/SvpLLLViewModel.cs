using System;
using System.IO;
using System.Windows;
using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using LatticeCrypto.Views;
using Microsoft.Win32;

namespace LatticeCrypto.ViewModels
{
    public class SvpLLLViewModel : SvpGaussViewModel
    {
        public SvpLLLViewModel()
        {
            ReductionMethod = ReductionMethods.reduceLLL;
        }
        
        private RelayCommand saveToClipboardCommand;
        public new RelayCommand SaveToClipboardCommand
        {
            get
            {
                if (saveToClipboardCommand != null) return saveToClipboardCommand;
                saveToClipboardCommand = new RelayCommand(
                    parameter1 =>
                    {
                        LatticeCopyOrSaveSelection selectionView = new LatticeCopyOrSaveSelection();
                        if (selectionView.ShowDialog() == false) return;


                        string latticeInfos;
                        switch (selectionView.selection)
                        {
                            default:
                                latticeInfos = Lattice.LatticeToString();
                                break;
                            case 1:
                                latticeInfos = Lattice.LatticeReducedToString();
                                break;
                            case 2:
                                latticeInfos =
                                    Languages.labelLatticeBasis + " " + Lattice.LatticeToString() + Environment.NewLine +
                                    Languages.labelLengthBasisVectors + " " + Lattice.VectorLengthToString() + Environment.NewLine +
                                    Languages.labelReducedLatticeBasis + " " + Lattice.LatticeReducedToString() + Environment.NewLine +
                                    Languages.labelSuccessiveMinima + " " + Lattice.VectorReducedLengthToString() + Environment.NewLine +
                                    Languages.labelDeterminant + " " + Lattice.Determinant + Environment.NewLine + 
                                    Languages.labelUnimodularTransformationMatrix + " " + Lattice.LatticeTransformationToString() ;
                                break;
                        }

                        Clipboard.SetText(latticeInfos);
                    });
                return saveToClipboardCommand;
            }
        }

        private RelayCommand saveToFileCommand;
        public new RelayCommand SaveToFileCommand
        {
            get
            {
                if (saveToFileCommand != null) return saveToFileCommand;
                saveToFileCommand = new RelayCommand(
                    parameter1 =>
                    {
                        LatticeCopyOrSaveSelection selectionView = new LatticeCopyOrSaveSelection();
                        if (selectionView.ShowDialog() == false) return;

                        SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*" };
                        if (saveFileDialog.ShowDialog() == false) return;
                        try
                        {
                            string[] latticeInfos;
                            switch (selectionView.selection)
                            {
                                default:
                                    latticeInfos = new[] { Lattice.LatticeToString() };
                                    break;
                                case 1:
                                    latticeInfos = new[] { Lattice.LatticeReducedToString() };
                                    break;
                                case 2:
                                    latticeInfos = new[]
                                    {
                                        Languages.labelLatticeBasis + " " + Lattice.LatticeToString(),
                                        Languages.labelLengthBasisVectors + " " + Lattice.VectorLengthToString(),
                                        Languages.labelReducedLatticeBasis + " " + Lattice.LatticeReducedToString(),
                                        Languages.labelSuccessiveMinima + " " + Lattice.VectorReducedLengthToString(),
                                        Languages.labelDeterminant + " " + Lattice.Determinant,
                                        Languages.labelUnimodularTransformationMatrix + " " + Lattice.LatticeTransformationToString()
                                    };
                                    break;
                            }

                            File.WriteAllLines(saveFileDialog.FileName, latticeInfos);
                        }
                        catch (IOException)
                        {
                            MessageBox.Show(Languages.errorSavingFile, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                return saveToFileCommand;
            }
        }
    }
}
