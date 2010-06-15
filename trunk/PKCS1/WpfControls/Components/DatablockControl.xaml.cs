using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using PKCS1;
using PKCS1.Library;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1;


namespace PKCS1.WpfControls.Components
{
    /// <summary>
    /// Interaktionslogik für HwControl.xaml
    /// </summary>
    public partial class DatablockControl : UserControl
    {     
        public DatablockControl()
        {
            InitializeComponent();
            Initialize();
        }

        public void Initialize()
        {
            // ComboBox befüllen
            this.cbHashFunc.Items.Add(HashFunctionHandler.SHA1);
            this.cbHashFunc.Items.Add(HashFunctionHandler.SHA256);
            this.cbHashFunc.Items.Add(HashFunctionHandler.SHA384);
            this.cbHashFunc.Items.Add(HashFunctionHandler.SHA512);
            this.cbHashFunc.Items.Add(HashFunctionHandler.MD2);
            this.cbHashFunc.Items.Add(HashFunctionHandler.MD5);
            this.cbHashFunc.SelectedIndex = 0;

            this.rbTextFromBox.IsChecked = true;

            Datablock.getInstance().RaiseParamChangedEvent += handleParamChanged;
        }

        #region Eventhandlng

        private void handleParamChanged(ParameterChangeType type)
        {
            if (ParameterChangeType.Message == type)
            {
                this.tbInputText.Text = Datablock.getInstance().Message;
                this.tbHashDigest.Text = Datablock.getInstance().GetHashDigestToHexString();
            }
        }

        public event ParamChanged RaiseDataBlockGenerated;

        private void OnRaiseDataBlockGenerated(ParameterChangeType type)
        {
            if (null != RaiseDataBlockGenerated)
            {
                RaiseDataBlockGenerated(type);
            }
        }

        #endregion

        // fired when Checkbox Selection changed 
        // Hashfunction IdentificationTextbox filled & selected Hashfunction is set
        private void cbHashFunc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Datablock.getInstance().HashFunctionIdent = (HashFunctionIdent)this.cbHashFunc.SelectedValue;
            this.tbHashIdent.Text = Datablock.getInstance().HashFunctionIdent.DERIdent;

            // HashDigest Textboxen leeren; werden bei Execute befüllt
            this.tbHashDigest.Text = String.Empty;
            this.lblHashDigestLength.Content = String.Empty;
        }

        private void bGenerate_Click(object sender, RoutedEventArgs e)
        {
            //TODO komplett implementieren. Abfrage ob Textinput null ist
            string textToHash = "";

            if (this.rbTextFromBox.IsChecked == true)
            {
                textToHash = this.tbInputText.Text;
            }
            else if (this.rbTextfromFile.IsChecked == true)
            {
                // TODO hier den Text aus dem File lesen
                textToHash = "muss noch implementiert werden!";
            }
            else
            {
                //TODO else abändern? weglassen?
                textToHash = "konnte Text nicht lesen";
            }

            // Text setzen, Hash wird automatisch generiert, da in Datablock das Event getriggert wird und hier im Handling Hashgenerierung auslöst
            Datablock.getInstance().Message = textToHash;
            // Hash generieren und abfragen
            //this.tbHashDigest.Text = Datablock.getInstance().GetHashDigestToHexString();
            OnRaiseDataBlockGenerated(ParameterChangeType.DataBlock);
        }

        private void tbInputText_KeyUp(object sender, KeyEventArgs e)
        {
            //TODO: Hier Inhalt der Textbox auf null prüfen und ggf Ausführen Button sperren
            //siehe InputSingleControl.xaml.cs in Primes
        }

        private void tbHashIdent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(this.tbHashIdent.Text != String.Empty )
            {
                this.lblHashIdentLength.Content = "(Länge: "+ this.tbHashIdent.Text.Length*4 +" bit)";
            }
        }

        private void tbHashDigest_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.tbHashDigest.Text != String.Empty)
            {
                this.lblHashDigestLength.Content = "(Länge: " + this.tbHashDigest.Text.Length*4 + " bit)";
            }
        }

        private void rbTextfromFile_Checked(object sender, RoutedEventArgs e)
        {
            this.tbInputFile.IsEnabled = true;
            this.tbInputText.IsEnabled = false;
        }

        private void rbTextFromBox_Checked(object sender, RoutedEventArgs e)
        {
            this.tbInputFile.IsEnabled = false;
            this.tbInputText.IsEnabled = true;
        }
    }
}
