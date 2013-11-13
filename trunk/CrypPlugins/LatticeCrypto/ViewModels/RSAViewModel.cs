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
    public class RSAViewModel: BaseViewModel
    {
        public RSAModel RSAModel { get; set; }
        public RichTextBox History { get; set; }
        private string knownMessage;
        private string unknownMessageResult;
        private int unknownStart;
        private int unknownLength;
        private BigInteger cipher;

        public void GenerateNewRSA (int bitSize)
        {
            UiServices.SetBusyState();
            RSAModel = new RSAModel(bitSize);

            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonGenerateNewCryptosystem + " **\r\n"))));
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPrimeP)));
            paragraph.Inlines.Add(" " + PrimP + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPrimeQ)));
            paragraph.Inlines.Add(" " + PrimQ + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelModulus)));
            paragraph.Inlines.Add(" " + ModulusN + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPrivateExponentD)));
            paragraph.Inlines.Add(" " + ExpD + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPublicExponentE)));
            paragraph.Inlines.Add(" " + ExpE + "\r\n");
            History.Document.Blocks.Add(paragraph);

            NotifyPropertyChanged("PrimP");
            NotifyPropertyChanged("PrimQ");
            NotifyPropertyChanged("ModulusN");
            NotifyPropertyChanged("ExpD");
            NotifyPropertyChanged("ExpE");
            NotifyPropertyChanged("ValidationInfo");
            encryptCommand.RaiseCanExecuteChanged();
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
                            Cipher = RSAModel.Encrypt(message).ToString();

                            Paragraph paragraph = new Paragraph();
                            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonEncrypt + " **\r\n"))));
                            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPlainText)));
                            paragraph.Inlines.Add(" " + Message + "\r\n");
                            paragraph.Inlines.Add(new Bold(new Run(Languages.labelCiphertext)));
                            paragraph.Inlines.Add(" " + Cipher + "\r\n");
                            History.Document.Blocks.Add(paragraph);
                            
                            decryptCommand.RaiseCanExecuteChanged();
                            cryptanalysisCommand.RaiseCanExecuteChanged();
                        }, parameter2 => !string.IsNullOrEmpty(Message) && !ValidationInfo.Equals(Languages.errorMessageTooLong));
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
                            Message = RSAModel.Decrypt(cipher);

                            Paragraph paragraph = new Paragraph();
                            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonDecrypt + " **\r\n"))));
                            paragraph.Inlines.Add(new Bold(new Run(Languages.labelCiphertext)));
                            paragraph.Inlines.Add(" " + Cipher + "\r\n");
                            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPlainText)));
                            paragraph.Inlines.Add(" " + Message + "\r\n");
                            History.Document.Blocks.Add(paragraph);

                            NotifyPropertyChanged("Message");
                        }, parameter2 => !string.IsNullOrEmpty(Cipher));
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
                            UiServices.SetBusyState();
                            Paragraph paragraph = new Paragraph();
                            
                            try
                            {
                                if (unknownStart == 0 && unknownLength == 0)
                                {
                                    MessageBox.Show(Languages.infoMarkKnownPlaintext, Languages.information, MessageBoxButton.OK, MessageBoxImage.Information);
                                    return;
                                }
                                
                                paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonCryptanalysis + " **\r\n"))));
                                paragraph.Inlines.Add(new Bold(new Run(Languages.labelCiphertext)));
                                paragraph.Inlines.Add(" " + Cipher + "\r\n");
                                paragraph.Inlines.Add(new Bold(new Run(Languages.labelKnownPlainText)));
                                paragraph.Inlines.Add(" " + KnownMessage + "\r\n");

                                string left = message.Substring(0, unknownStart);
                                string right = message.Substring(unknownStart + unknownLength);
                                UnknownMessageResult = RSAModel.StereotypedAttack(left, right, unknownLength, cipher, "4");

                                paragraph.Inlines.Add(new Bold(new Run(Languages.labelResultUnknownPlainText)));
                                paragraph.Inlines.Add(" " + UnknownMessageResult + "\r\n");
                            }
                            catch (Exception ex)
                            {
                                UnknownMessageResult = "";

                                paragraph.Inlines.Add(new Bold(new Run(Languages.labelAbort)));
                                paragraph.Inlines.Add(" " + ex.Message + "\r\n");

                                MessageBox.Show(ex.Message, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            finally
                            {
                                History.Document.Blocks.Add(paragraph);
                            }
                            
                        }, parameter2 => !string.IsNullOrEmpty(Cipher));
                return cryptanalysisCommand;
            }
        }

        //public void SetKnownMessageRange (int start, int length)
        //{
        //    String newKnownMessage = "";
        //    for (int i = 0; i < start; i++)
        //        newKnownMessage += "*";
        //    newKnownMessage += message.Substring(start, length);
        //    for (int i = 0; i < message.Length - start - length; i++)
        //        newKnownMessage += "*";

        //    KnownMessage = newKnownMessage;
        //}

        public void SetUnknownMessageRange(int start, int length)
        {
            unknownStart = start;
            unknownLength = length;
            string newKnownMessage = "";
            if (start > 0)
                newKnownMessage += message.Substring(0, start);
            for (int i = 0; i < length; i++)
                newKnownMessage += "*";
            if (start + length < message.Length)
                newKnownMessage += message.Substring(start + length);

            KnownMessage = newKnownMessage;
            UnknownMessageResult = "";
            NotifyPropertyChanged("ValidationInfo");
        }

        private string message;
        public string Message
        {
            get { return message; }
            set 
            {
                message = value;
                encryptCommand.RaiseCanExecuteChanged();
                KnownMessage = "";
                UnknownMessageResult = "";
                unknownStart = 0;
                unknownLength = 0;
                NotifyPropertyChanged("ValidationInfo");
            }
        }

        public string KnownMessage
        {
            get { return !string.IsNullOrEmpty(knownMessage) ? knownMessage : ""; }
            set 
            {
                if (knownMessage == value) return;
                knownMessage = value;
                NotifyPropertyChanged("KnownMessage");
            }
        }

        public string UnknownMessageResult
        {
            get { return unknownMessageResult; }
            set
            {
                if (unknownMessageResult == value) return;
                unknownMessageResult = value;
                NotifyPropertyChanged("UnknownMessageResult");
            }
        }

        public string Cipher
        {
            get
            {
                return cipher != 0 ? cipher.ToString() : "";
            }
            private set
            {
                BigInteger.TryParse(value, out cipher);
                NotifyPropertyChanged("Cipher");
            }
        }

        public string ValidationInfo
        {
            get 
            {
                if (!string.IsNullOrEmpty(message) && message.Length >= BlockSize)
                    return Languages.errorMessageTooLong;
                if (!string.IsNullOrEmpty(message) && string.IsNullOrEmpty(knownMessage))
                    return Languages.infoMarkKnownPlaintext;
                return "";
            }
        }

        public string PrimP
        {
            get
            {
                return RSAModel == null ? "" : RSAModel.GetPrimPToString();
            }
        }

        public string PrimQ
        {
            get
            {
                return RSAModel == null ? "" : RSAModel.GetPrimQToString();
            }
        }

        public string ModulusN
        {
            get
            {
                return RSAModel == null ? "" : RSAModel.GetModulusNToString();
            }
        }

        public string ExpD
        {
            get
            {
                return RSAModel == null ? "" : RSAModel.GetPrivateExponentToString();
            }
        }

        public string ExpE
        {
            get
            {
                return RSAModel == null ? "" : RSAModel.GetPublicExponentToString();
            }
        }

        public int BlockSize
        {
            get
            {
                return RSAModel == null ? 0 : RSAModel.GetBlockSize();
            }
        }
    }
}
