using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using LatticeCrypto.Models;
using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;

namespace LatticeCrypto.ViewModels
{
    public class GGHViewModel : BaseViewModel
    {
        public LatticeND Lattice { get; set; }
        public RichTextBox History { get; set; }
        public ReductionMethods ReductionMethod { get; set; }
        public GGHModel GGH { get; set; }
        public Grid LeftGrid { get; set; }
        public Grid RightGrid { get; set; }
        private string message;
        private VectorND cipher;

        public GGHViewModel()
        {
            ReductionMethod = ReductionMethods.reduceLLL;
        }

        public void GenerateNewGGH(int dim, int l)
        {
            UiServices.SetBusyState();
            GGH = GGH != null && GGH.dim == dim ? new GGHModel(dim, l, GGH.errorVector) : new GGHModel(dim, l);
            //MatrixND privateKey = new MatrixND(3, 3);
            //privateKey[0, 0] = 7;
            //privateKey[0, 1] = 0;
            //privateKey[0, 2] = 0;
            //privateKey[1, 0] = 0;
            //privateKey[1, 1] = 5;
            //privateKey[1, 2] = 0;
            //privateKey[2, 0] = 0;
            //privateKey[2, 1] = 0;
            //privateKey[2, 2] = 3;
            //MatrixND publicKey = new MatrixND(3, 3);
            //publicKey[0, 0] = (14);
            //publicKey[0, 1] = (7);
            //publicKey[0, 2] = (14);
            //publicKey[1, 0] = (20);
            //publicKey[1, 1] = (20);
            //publicKey[1, 2] = (5);
            //publicKey[2, 0] = (9);
            //publicKey[2, 1] = (6);
            //publicKey[2, 2] = (6);
            //MatrixND error = new MatrixND(3, 1);
            //error[0, 0] = 1;
            //error[1, 0] = -1;
            //error[2, 0] = 1;
            //GGH = new GGHModel(3, privateKey, publicKey, error);
            
            Lattice = GGH.lattice;
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonGenerateNewCryptosystem + " **\r\n"))));
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPrivateKey)));
            paragraph.Inlines.Add(" " + Lattice.LatticeReducedToString() + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPublicKey)));
            paragraph.Inlines.Add(" " + Lattice.LatticeToString() + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelUnimodularTransformationMatrix)));
            paragraph.Inlines.Add(" " + Lattice.LatticeTransformationToString() + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelParameterL)));
            paragraph.Inlines.Add(" " + GGH.l + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelErrorVector)));
            paragraph.Inlines.Add(" " + GGH.errorVector + "\r\n");
            History.Document.Blocks.Add(paragraph);

            NotifyPropertyChanged("ErrorVector");
        }

        private RelayCommand generateErrorVectorCommand;
        public RelayCommand GenerateErrorVectorCommand
        {
            get
            {
                if (generateErrorVectorCommand != null) return generateErrorVectorCommand;
                generateErrorVectorCommand = new RelayCommand(
                    parameter1 =>
                        {
                            UiServices.SetBusyState();
                            GGH.GenerateErrorVector();

                            Paragraph paragraph = new Paragraph();
                            paragraph.Inlines.Add(
                                new Bold(
                                    new Underline(new Run("** " + Languages.buttonGenerateNewErrorVector + " **\r\n"))));
                            paragraph.Inlines.Add(new Bold(new Run(Languages.labelErrorVector)));
                            paragraph.Inlines.Add(" " + GGH.errorVector + "\r\n");
                            History.Document.Blocks.Add(paragraph);

                            NotifyPropertyChanged("ErrorVector");

                        });
                return generateErrorVectorCommand;
            }
        }

        private RelayCommand encryptCommand;
        public RelayCommand EncryptCommand
        {
            get
            {
                if (encryptCommand != null) return encryptCommand;
                encryptCommand = new RelayCommand(
                    parameter1 =>
                    {
                        UiServices.SetBusyState();
                        cipher = GGH.Encrypt(message);

                        Paragraph paragraph = new Paragraph();
                        paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonEncrypt + " **\r\n"))));
                        paragraph.Inlines.Add(new Bold(new Run(Languages.labelPlainText)));
                        paragraph.Inlines.Add(" " + Message + "\r\n");
                        paragraph.Inlines.Add(new Bold(new Run(Languages.labelCiphertext)));
                        paragraph.Inlines.Add(" " + Cipher + "\r\n");
                        History.Document.Blocks.Add(paragraph);

                        NotifyPropertyChanged("Cipher");
                        decryptCommand.RaiseCanExecuteChanged();
                    }, parameter2 => !string.IsNullOrEmpty(message));
                return encryptCommand;
            }
        }

        private RelayCommand decryptCommand;
        public RelayCommand DecryptCommand
        {
            get
            {
                if (decryptCommand != null) return decryptCommand;
                decryptCommand = new RelayCommand(
                    parameter1 =>
                    {
                        UiServices.SetBusyState();
                        Paragraph paragraph = new Paragraph();
                        try
                        {
                            Message = GGH.Decrypt(cipher);
                            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonDecrypt + " **\r\n"))));
                            paragraph.Inlines.Add(new Bold(new Run(Languages.labelCiphertext)));
                            paragraph.Inlines.Add(" " + Cipher + "\r\n");
                            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPlainText)));
                            paragraph.Inlines.Add(" " + Message + "\r\n");
                            NotifyPropertyChanged("Message");
                        }
                        catch (Exception ex)
                        {

                            paragraph.Inlines.Add(new Bold(new Run(Languages.labelAbort)));
                            paragraph.Inlines.Add(" " + ex.Message + "\r\n");

                            MessageBox.Show(string.Format(Languages.errorDecryptionError, ex.Message), Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        finally
                        {
                            History.Document.Blocks.Add(paragraph);
                        }
                        
                    }, parameter2 => cipher != null && !string.IsNullOrEmpty(cipher.ToString()));
                return decryptCommand;
            }
        }

        public void UpdateTextBoxes()
        {
            if (LeftGrid.RowDefinitions.Count != Lattice.Dim)
            {
                LeftGrid.RowDefinitions.Clear();
                LeftGrid.ColumnDefinitions.Clear();
                RightGrid.RowDefinitions.Clear();
                RightGrid.ColumnDefinitions.Clear();

                for (int i = 0; i < Lattice.Dim; i++)
                {
                    LeftGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
                    LeftGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    RightGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
                    RightGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }

                LeftGrid.Children.Clear();
                RightGrid.Children.Clear();

                for (int i = 0; i < Lattice.Dim; i++)
                {
                    for (int j = 0; j < Lattice.Dim; j++)
                    {
                        TextBlock leftTextBlock = new TextBlock
                        {
                            Text = Util.FormatBigInt(Lattice.Vectors[i].values[j]),
                            Margin = new Thickness(10, 0, 10, 0),
                            TextAlignment = TextAlignment.Right
                        };
                        Grid.SetColumn(leftTextBlock, i);
                        Grid.SetRow(leftTextBlock, j);
                        LeftGrid.Children.Add(leftTextBlock);

                        TextBlock rightTextBlock = new TextBlock
                        {
                            Text = Util.FormatBigInt(Lattice.ReducedVectors[i].values[j]),
                            Margin = new Thickness(10, 0, 10, 0),
                            TextAlignment = TextAlignment.Right
                        };
                        Grid.SetColumn(rightTextBlock, i);
                        Grid.SetRow(rightTextBlock, j);
                        RightGrid.Children.Add(rightTextBlock);
                    }
                }
            }
            else
            {
                foreach (TextBlock textBlock in LeftGrid.Children)
                    textBlock.Text = Util.FormatBigInt(Lattice.Vectors[Grid.GetColumn(textBlock)].values[Grid.GetRow(textBlock)]);
                foreach (TextBlock textBlock in RightGrid.Children)
                    textBlock.Text = Util.FormatBigInt(Lattice.ReducedVectors[Grid.GetColumn(textBlock)].values[Grid.GetRow(textBlock)]);
            }
        }

        public string Message
        {
            get { return message; }
            set
            {
                message = value.TrimEnd('\0');
                encryptCommand.RaiseCanExecuteChanged();
            }
        }

        public string Cipher
        {
            get
            {
                return cipher == null ? "" : cipher.ToString();
            }
        }

        public string ErrorVector
        {
            get
            {
                return GGH == null ? "" : GGH.errorVector.ToString();
            }
        }
    }
}
