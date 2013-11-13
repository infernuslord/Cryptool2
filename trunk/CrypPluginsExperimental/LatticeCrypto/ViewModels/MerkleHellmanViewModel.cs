using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using LatticeCrypto.Models;
using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;

namespace LatticeCrypto.ViewModels
{
    public class MerkleHellmanViewModel: BaseViewModel
    {
        public MerkleHellmanModel MerkleHellman { get; set; }
        public RichTextBox History { get; set; }
        public string message;
        public VectorND cipher;

        public void GenerateNewMerkleHellman (int dim)
        {
            UiServices.SetBusyState();
            MerkleHellman = new MerkleHellmanModel(dim);

            //Für Masterarbeit
            //MerkleHellman = new MerkleHellmanModel(new BigInteger[] { 3, 11, 23, 46 }, 65, 98);

            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonGenerateNewCryptosystem + " **\r\n"))));
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPrivateKey)));
            paragraph.Inlines.Add(" " + PrivateKey + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPublicKey)));
            paragraph.Inlines.Add(" " + PublicKey + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelModulus)));
            paragraph.Inlines.Add(" " + Mod + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelMultiplier)));
            paragraph.Inlines.Add(" " + R + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelMultiplierInverse)));
            paragraph.Inlines.Add(" " + RI + "\r\n");
            History.Document.Blocks.Add(paragraph);

            NotifyPropertyChanged("MerkleHellman");
            NotifyPropertyChanged("PrivateKey");
            NotifyPropertyChanged("PublicKey");
            NotifyPropertyChanged("Mod");
            NotifyPropertyChanged("R");
            NotifyPropertyChanged("RI");
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
                            cipher = MerkleHellman.Encrypt(message);

                            Paragraph paragraph = new Paragraph();
                            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonEncrypt + " **\r\n"))));
                            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPlainText)));
                            paragraph.Inlines.Add(" " + Message + "\r\n");
                            paragraph.Inlines.Add(new Bold(new Run(Languages.labelCiphertext)));
                            paragraph.Inlines.Add(" " + Cipher + "\r\n");
                            History.Document.Blocks.Add(paragraph);

                            NotifyPropertyChanged("Cipher");
                            decryptCommand.RaiseCanExecuteChanged();
                            cryptanalysisCommand.RaiseCanExecuteChanged();
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
                            Message = MerkleHellman.Decrypt(cipher);

                            Paragraph paragraph = new Paragraph();
                            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonDecrypt + " **\r\n"))));
                            paragraph.Inlines.Add(new Bold(new Run(Languages.labelCiphertext)));
                            paragraph.Inlines.Add(" " + Cipher + "\r\n");
                            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPlainText)));
                            paragraph.Inlines.Add(" " + Message + "\r\n");
                            History.Document.Blocks.Add(paragraph);

                            NotifyPropertyChanged("Message");
                        }, parameter2 => cipher != null && !string.IsNullOrEmpty(cipher.ToString()));
                return decryptCommand;
            }
        }

        private RelayCommand cryptanalysisCommand;
        public RelayCommand CryptanalysisCommand
        {
            get
            {
                if (cryptanalysisCommand != null) return cryptanalysisCommand;
                cryptanalysisCommand = new RelayCommand(
                    parameter1 =>
                        {
                            try
                            {
                                UiServices.SetBusyState();
                                Paragraph paragraph = new Paragraph();
                                paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonCryptanalysis + " **\r\n"))));
                                paragraph.Inlines.Add(new Bold(new Run(Languages.labelCiphertext)));
                                paragraph.Inlines.Add(" " + Cipher + "\r\n");

                                Message = MerkleHellman.Cryptanalysis(cipher, paragraph);

                                History.Document.Blocks.Add(paragraph);

                                NotifyPropertyChanged("Message");
                            }
                            catch (Exception)
                            {
                                MessageBox.Show(Languages.errorNoSolutionFound, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            
                            NotifyPropertyChanged("Message");
                        }, parameter2 => cipher != null && !string.IsNullOrEmpty(cipher.ToString()));
                return cryptanalysisCommand;
            }
        }

        public string Message
        {
            get { return message; }
            set 
            {
                message = value;
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

        public string PrivateKey
        {
            get
            {
                return MerkleHellman == null ? "" : MerkleHellman.privateKey.ToString();
            }
        }

        public string PublicKey
        {
            get
            {
                return MerkleHellman == null ? "" : MerkleHellman.publicKey.ToString();
            }
        }

        public BigInteger Mod
        {
            get
            {
                return MerkleHellman == null ? 0 : MerkleHellman.mod;
            }
        }

        public BigInteger R
        {
            get
            {
                return MerkleHellman == null ? 0 : MerkleHellman.r;
            }
        }

        public BigInteger RI
        {
            get
            {
                return MerkleHellman == null ? 0 : MerkleHellman.rI;
            }
        }
    }
}
