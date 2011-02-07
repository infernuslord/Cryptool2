using System;
using System.Collections.Generic;
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
using Microsoft.CSharp;
using System.CodeDom.Compiler;

using System.IO;
using System.Web.Services.Description;
using System.Xml;
using System.Collections;
using System.Windows.Media.Animation;
using System.Data;
using System.Security.Cryptography.Xml;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Threading;


namespace WebService
{
    /// <summary>
    /// Interaktionslogik für WebServicePresentation.xaml
    /// </summary>
    public partial class WebServicePresentation : System.Windows.Controls.UserControl
    {
        #region Fields
        private bool _referenceValid = true;
        private ArrayList _signatureCollection;
        private ArrayList _tempReferenceCollection;
        private ArrayList _tempTransformCollection;
        private string _webServiceDescription;
        private string _lastURI;
        private Run _methodVisibility;
        private Run _returnParam;
        private Run _methodName;
        private Run _openBrace;
        private Run _closeBrace;
        private Run _comma;
        private Run _openMethodBrace;
        private Run _closeMethodBrace;
        private Bold _webMethod;
        private FlowDocument doc;
        private Bold _intParams;
        private Bold _stringParams;
        private Bold _doubleParams;
        private System.ComponentModel.SortDescription SD;
        private DispatcherTimer _dispatcherTimer;
        private DispatcherTimer _decryptionTimer;
        private DispatcherTimer _referenceTimer;
        private DispatcherTimer _transformTimer;
        private DoubleAnimation _textSizeAnimation;
        private DoubleAnimation _textSizeAnimationReverse;
        private DoubleAnimation _textSizeAnimation1;
        private DoubleAnimation _textSizeAnimationReverse1;
        private int _status;
        private int _referenceStatus;
        private int _transformstatus;
        private int _signatureNumber;
        private int _actualSignatureNumber;
        private int _signaturenumber;
        private int _actualReferenceNumber;
        private int _referenceNumber;
        private int _transformNumber;
        private int _transformCount;
        private AnimationController _animationController;
        #endregion

        public bool isbusy;
        public Hashtable namespacesTable;
        public TreeViewItem item, soapInput, item1;
        public WebService webService;
        public DecryptionAnimation decryptionAnimation;

        #region Constructor
        public WebServicePresentation(WebService webService)
        {
            InitializeComponent();
            this._actualSignatureNumber = 1;
            decryptionAnimation = new DecryptionAnimation(this);
            slider1.Opacity = 0;
            isbusy = true;
            this._animationController = new AnimationController(this);
            this._signatureCollection = new ArrayList();
            this._tempTransformCollection = new ArrayList();
            Paragraph par = (Paragraph)this.richTextBox1.Document.Blocks.FirstBlock;
            par.LineHeight = 5;
            this._status = 1;
            this._dispatcherTimer = new DispatcherTimer();
            this._dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            this._referenceTimer = new DispatcherTimer();
            this._referenceTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            this._decryptionTimer = new DispatcherTimer();
            this._decryptionTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            this._transformTimer = new DispatcherTimer();
            this._transformTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            this._textSizeAnimation = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            this._textSizeAnimationReverse = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            this._textSizeAnimation1 = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            this._textSizeAnimationReverse1 = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            this._dispatcherTimer.Tick += new EventHandler(DispatcherTimerTickEventHandler);
            this._referenceTimer.Tick += new EventHandler(ReferenceTimerTickEventHandler);
            this._transformTimer.Tick += new EventHandler(TransformTimerTickEventHandler);
            doc = new FlowDocument();
            this._webMethod = new Bold(new Run("[WebMethod]" + "\n"));
            this._methodVisibility = new Run();
            this._returnParam = new Run();
            this._methodName = new Run();
            this._intParams = new Bold();
            this._stringParams = new Bold();
            this._doubleParams = new Bold();
            this._openBrace = new Run();
            this._closeBrace = new Run();
            this._comma = new Run(",");
            this._openMethodBrace = new Run("\n{");
            textBlock2.Inlines.Add(new Run("}"));
            textBlock2.Visibility = Visibility.Visible;
            this.VisualMethodName("methodName");
            this.richTextBox1.Document = doc;
            this.textBlock1.Inlines.Add(this._webMethod);
            this.textBlock1.Inlines.Add(this._methodVisibility);
            this.textBlock1.Inlines.Add(this._returnParam);
            this.textBlock1.Inlines.Add(this._methodName);
            this.textBlock1.Inlines.Add(this._openBrace);
            this.textBlock1.Inlines.Add(this._intParams);
            this.textBlock1.Inlines.Add(this._stringParams);
            this.textBlock1.Inlines.Add(this._doubleParams);
            this.textBlock1.Inlines.Add(this._closeBrace);
            textBlock1.Inlines.Add(_openMethodBrace);
            item = new TreeViewItem();
            item1 = new TreeViewItem();
            item.Header = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
            item.IsExpanded = true;
            soapInput = new TreeViewItem();
            TextBlock block = new TextBlock();
            block.Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
            StackPanel panel = new StackPanel();
            panel.Children.Add(block);
            soapInput.Header = panel;
            soapInput.IsExpanded = true;
            this.webService = webService;
            namespacesTable = new Hashtable();
            SD = new System.ComponentModel.SortDescription("param", System.ComponentModel.ListSortDirection.Ascending);
            this._tempReferenceCollection = new ArrayList();
            webService.Settings.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(SettingsPropertyChangedEventHandler);
        }
        #endregion

        #region Methods

        public void ResetSoapInputItem()
        {
            this.webService.InputString = this.webService.InputString;
            //this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            //{


            //    this.namespacesTable.Clear();
            //    this.presentation.CopyXmlToTreeView(this._inputDocument.ChildNodes[1], ref this.presentation.soapInput, false);

            //    this._animationTreeView.Items.Clear();

            //    this._animationTreeView.Items.Clear();

            //    soapInput = null;
            //    soapInput = new TreeViewItem();
            //    TextBlock block = new TextBlock();
            //    block.Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
            //    StackPanel panel = new StackPanel();
            //    panel.Children.Add(block);
            //    soapInput.Header = panel;
            //    soapInput.IsExpanded = true;
            //    XmlNode rootElement = this.webService.InputString.SelectSingleNode("/*");
            //    CopyXmlToTreeView(rootElement, ref this.soapInput, false);

            //    this._animationTreeView.Items.Add(soapInput);
            //    this._animationTreeView.Items.Refresh();
            //    this.InitializeAnimation();
            //    this.presentation._animationTreeView.Items.Add(this.presentation.soapInput);
            //    this.FindTreeViewItem(this.soapInput, "Envelope", 1).IsExpanded = true;
            //    this.FindTreeViewItem(this.soapInput, "Header", 1).IsExpanded = true;
            //    this.FindTreeViewItem(this.soapInput, "Security", 1).IsExpanded = true;
            //    this.FindTreeViewItem(this.soapInput, "Signature", 1).IsExpanded = true;
            //    this.FindTreeViewItem(this.soapInput, "Body", 1).IsExpanded = true;
            //    this._animationTreeView.Items.Refresh();

            //}, null);
            

        }

        public void CopyXmlToTreeView(XmlNode xmlNode, ref TreeViewItem treeViewItemParent, bool withPics)
        {
            SolidColorBrush elemBrush = new SolidColorBrush(Colors.MediumVioletRed);
            if (xmlNode != null)
            {
                TreeViewItem item = new TreeViewItem();
                item.IsExpanded = true;
                StackPanel panel = new StackPanel();
                panel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                TextBlock tbTagOpen = new TextBlock();
                TextBlock tbTagClose = new TextBlock();
                TextBlock tbName = new TextBlock();
                tbTagOpen.Name = "tbTagOpen";
                tbTagClose.Name = "tbTagClose";
                tbName.Name = "tbName";
                tbTagOpen.Text = "<";
                tbTagClose.Text = ">";
                tbName.Text = xmlNode.Name;
                tbTagOpen.Foreground = elemBrush;
                tbTagClose.Foreground = elemBrush;
                tbName.Foreground = elemBrush;
                if (!xmlNode.NodeType.ToString().Equals("Text"))
                {
                    item.Name = "OpenItemXmlNode";
                    panel.Name = "OpenPanelXMLNode";
                    TreeViewItem closeitem = new TreeViewItem();
                    panel.Children.Insert(0, tbTagOpen);
                    panel.Children.Add(tbName);
                    if (!xmlNode.NamespaceURI.Equals(""))
                    {
                        InsertNamespace(ref panel, xmlNode.NamespaceURI, xmlNode.Prefix);
                    }
                    if (xmlNode.Attributes != null)
                    {
                        InsertAttributes(ref panel, xmlNode.Attributes);
                    }

                    panel.Children.Add(tbTagClose);

                    if (withPics)
                    {
                        if (xmlNode.Name.Equals("s:Body"))
                        {
                            AddOpenLockToPanel(ref panel, xmlNode.Name, false);
                        }
                        else
                        {
                            AddOpenLockToPanel(ref panel, xmlNode.Name, true);
                        }
                    }


                    item.Header = panel;
                    closeitem.Foreground = elemBrush;
                    treeViewItemParent.Items.Add(item);
                    if (xmlNode.HasChildNodes)
                    {
                        foreach (XmlNode child in xmlNode.ChildNodes)
                        {
                            _lastURI = xmlNode.NamespaceURI; ;
                            CopyXmlToTreeView(child, ref item, withPics);
                        }
                    }
                    StackPanel panel1 = new StackPanel();
                    panel1.Orientation = System.Windows.Controls.Orientation.Horizontal;
                    TextBlock elem1 = new TextBlock();
                    TextBlock tbTagOpen3 = new TextBlock();
                    tbTagOpen3.Name = "tbTagOpen";
                    tbTagOpen3.Text = "<";
                    panel1.Children.Insert(0, tbTagOpen3);
                    elem1.Name = "tbName";
                    elem1.Text = "/" + xmlNode.Name;
                    panel1.Children.Add(elem1);
                    TextBlock tbTagClose3 = new TextBlock();
                    tbTagClose3.Name = "tbTagClose";
                    tbTagClose3.Text = ">";
                    panel1.Children.Add(tbTagClose3);

                    closeitem.Header = panel1;

                    treeViewItemParent.Items.Add(closeitem);
                }
                else
                {
                    TextBlock tbTagOpen2 = new TextBlock();
                    TextBlock tbTagClose2 = new TextBlock();

                    tbTagOpen2.Name = "tbTagOpen";
                    tbTagOpen2.Text = "<";
                    tbTagClose2.Name = "tbTagClose";
                    tbTagClose2.Text = ">";
                    item.Name = "OpenItemTextNode";
                    panel.Name = "OpenPanelTextNode";
                    TextBlock tbText = new TextBlock();
                    tbText.Name = "TextNode";
                    tbText.Text = xmlNode.Value;
                    TextBlock emptyTextBlock = new TextBlock();
                    emptyTextBlock.Text = "";
                    panel.Children.Insert(0, emptyTextBlock);
                    panel.Children.Add(tbText);
                    item.Header = panel;
                    treeViewItemParent.Items.Add(item);
                }
            }

        }
        private void AddOpenLockToPanel(ref StackPanel panel, string name, bool open)
        {
            System.Drawing.Bitmap bitmap;
            if (open)
            {
                bitmap = Resource1.OpenLock;
            }
            else
            {
                bitmap = Resource1.ClosedLock;
            }
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(ms.ToArray());
            bi.EndInit();
            Image myImage2 = new Image();
            myImage2.Source = bi;
            string nameSpace;
            if (name.Contains(":"))
            {
                string[] splitter = name.Split(new Char[] { ':' });
                int n = splitter.Length;
                if (splitter[1].ToString().Contains("+"))
                {
                    string[] nameSplitter = splitter[1].ToString().Split(new Char[] { '+' });

                    splitter[1] = nameSplitter[0];
                    nameSpace = nameSplitter[1].ToString() + ":" + splitter[n - 1].ToString();

                }
                myImage2.Name = splitter[1].ToString();
            }
            else
            {
                myImage2.Name = name;
            }
            int i = panel.Children.Count;
            if (open)
            {
                myImage2.MouseLeftButtonDown += new MouseButtonEventHandler(MyImage2MouseLeftButtonDownEventHandler);
            }
            if (!open)
            {
                myImage2.MouseLeftButtonDown += new MouseButtonEventHandler(MyImage2MouseLeftButtonDownCloseEventHandler);
            }


            myImage2.ToolTip = "Click this picture to encrypt the <" + name + "> element";
            panel.Children.Add(myImage2);
        }
       private void MyImage2MouseLeftButtonDownCloseEventHandler(object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;
            string name = img.Name;
        }
       private  void MyImage2MouseLeftButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;
            string name = img.Name;
        }
        public StackPanel InsertAttributes(ref StackPanel panel, XmlAttributeCollection xmlAttributes)
        {
            foreach (XmlAttribute tempAttribute in xmlAttributes)
            {
                if (!tempAttribute.Name.Contains("xmlns"))
                {
                    TextBlock name = new TextBlock();
                    name.Text = " " + tempAttribute.Name;
                    name.Name = "attributeName";
                    TextBlock value = new TextBlock();
                    value.Name = "attributeValue";
                    value.Text = " =\"" + tempAttribute.Value + "\"";
                    SolidColorBrush valueBrush = new SolidColorBrush(Colors.Blue);
                    value.Foreground = valueBrush;
                    panel.Children.Add(name);
                    panel.Children.Add(value);

                }
                else
                {
                    if (!namespacesTable.ContainsValue(tempAttribute.Value))
                    {
                        namespacesTable.Add(tempAttribute.Value, tempAttribute.Value);
                        TextBlock name = new TextBlock();
                        name.Text = " " + tempAttribute.Name;
                        TextBlock value = new TextBlock();
                        value.Text = " =\"" + tempAttribute.Value + "\"";
                        SolidColorBrush valueBrush = new SolidColorBrush(Colors.Blue);
                        value.Foreground = valueBrush;
                        panel.Children.Add(name);
                        panel.Children.Add(value);
                    }
                }
            }
            return panel;
        }
        public StackPanel InsertNamespace(ref StackPanel panel, string nameSpace, string nameSpacePrefix)
        {
            if (!namespacesTable.ContainsValue(nameSpace))
            {
                namespacesTable.Add(nameSpace, nameSpace);
                TextBlock xmlns = new TextBlock();
                xmlns.Name = "xmlns";
                xmlns.Text = " xmlns";
                TextBlock prefix = new TextBlock();
                prefix.Name = "xmlnsPrefix";
                if (!nameSpacePrefix.Equals(""))
                { prefix.Text = ":" + nameSpacePrefix; }
                else { prefix.Text = ""; }
                SolidColorBrush valueBrush = new SolidColorBrush(Colors.Blue);
                TextBlock value = new TextBlock();
                value.Name = "xmlnsValue";
                value.Text = "=" + "\"" + nameSpace + "\"";
                value.Foreground = valueBrush;
                panel.Children.Add(xmlns);
                panel.Children.Add(prefix);
                panel.Children.Add(value);
            }
            return panel;
        }
       private void DispatcherTimerTickEventHandler(object sender, EventArgs e)
        {
            switch (this._status)
            {
                case 1:

                    _signatureNumber = this.webService.GetSignatureNumber();
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    this._animationStepsTextBox.Text += "\n Check for Signature Element";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this._animationTreeView.Items.Refresh();
                    this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "Signature", 1).BringIntoView();
                    this.AnimateFoundElements((TreeViewItem)this._signatureCollection[this._signaturenumber], (TreeViewItem)_signatureCollection[this._signaturenumber]);
                    _status = 2;
                    slider1.Value++;
                    break;
                case 2:
                    this._animationStepsTextBox.Text += "\n Canonicalize SignedInfo";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "SignedInfo", 1).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "/SignedInfo", _actualSignatureNumber), this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "/ds:SignedInfo>", _actualSignatureNumber));
                    _status = 3;
                    slider1.Value++;
                    break;
                case 3:
                    this._animationStepsTextBox.Text += "\n -->Find Canonicalization Algorithm";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "CanonicalizationMethod", _actualSignatureNumber).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "/CanonicalizationMethod", _actualSignatureNumber), this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "/ds:CanonicalizationMethod>", _actualSignatureNumber));
                    _status = 4;
                    slider1.Value++;
                    break;
                case 4:
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindSignatureItems((TreeViewItem)_signatureCollection[this._signaturenumber], "ds:Reference");
                    this.InitializeReferenceAnimation();
                    _status = 5;
                    _dispatcherTimer.Stop();
                    slider1.Value++;
                    break;

                case 5:
                    this._animationStepsTextBox.Text += "\n Signature Validation";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this._animationStepsTextBox.Text += "\n -> Find Signature Method";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindTreeViewItem(this.soapInput, "SignatureMethod", _actualSignatureNumber).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "SignatureMethod", _actualSignatureNumber), this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "/ds:SignatureMethod", _actualSignatureNumber));
                    _status = 6;
                    slider1.Value++;


                    break;
                case 6:
                    this._animationStepsTextBox.Text += "\n Get public key for signature validation";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindTreeViewItem((TreeViewItem)_signatureCollection[this._signaturenumber], "KeyInfo", 1).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)_signatureCollection[this._signaturenumber], "KeyInfo", 1), this.FindTreeViewItem((TreeViewItem)_signatureCollection[this._signaturenumber], "/ds:KeyInfo", 1));
                    _status = 7;
                    break;
                case 7:
                    this._animationStepsTextBox.Text += "\n -> Validate SignatureValue over SignedInfo";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "SignatureValue", _actualSignatureNumber).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "SignatureValue", _actualSignatureNumber), this.FindTreeViewItem((TreeViewItem)this._signatureCollection[this._signaturenumber], "/ds:SignatureValue", _actualSignatureNumber));
                    _dispatcherTimer.Stop();
                    _status = 8;
                    slider1.Value++;
                    this._animationController.ControllerTimer.Start();
                    break;
                case 8:
                    this._tempReferenceCollection.Clear();
                    this._animationController.ControllerTimer.Start();
                    _signatureNumber = this.webService.GetSignatureNumber();
                    isbusy = false;
                    this._actualSignatureNumber++;
                    if (this._signaturenumber + 1 < this._signatureNumber)
                    {
                        isbusy = true;
                        this._signaturenumber++;
                        _status = 1;
                        slider1.Value++;
                    }

                    break;
            }


        }
      private void ReferenceTimerTickEventHandler(object sender, EventArgs e)
        {
            int n = this.webService.Validator.GetReferenceNumber(_signaturenumber);
            switch (_referenceStatus)
            {
                case 1:
                    _referenceTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    this._animationStepsTextBox.Text += "\n Reference Validation";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    _referenceStatus++;
                    break;
                case 2:
                    this._animationStepsTextBox.Text += "\n -> Find Reference Element";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    // this.findSignatureItems((TreeViewItem)signatureCollection[this.i], "ds:Reference").BringIntoView() ;
                    this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "Reference", _actualSignatureNumber).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "Reference", _actualSignatureNumber), this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "/ds:Reference", _actualSignatureNumber));
                    _referenceStatus++;
                    break;

                case 3:
                    this._animationStepsTextBox.Text += "\n -> Get referenced Element";
                    this.FindTreeViewItem(this.soapInput, this.webService.Validator.GetSignatureReferenceName(_signaturenumber), _actualReferenceNumber).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem(this.soapInput, this.webService.Validator.GetSignatureReferenceName(_signaturenumber), _actualReferenceNumber), this.FindTreeViewItem(this.soapInput, this.webService.Validator.GetSignatureReferenceName(_signaturenumber), _actualReferenceNumber));
                    _referenceStatus++;
                    break;

                case 4:
                    this._animationStepsTextBox.Text += "\n  -> Apply Transforms";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "Transforms", _actualSignatureNumber).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "Transforms", _actualSignatureNumber), this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "/ds:Transforms", _actualSignatureNumber));
                    this._transformCount = this.webService.Validator.GetTransformsCounter(_signaturenumber, _referenceNumber);
                    _referenceTimer.Stop();
                    _referenceStatus++;
                    this.FindSignatureItems((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "ds:Transform");
                    InitializeTransformAnimation();

                    break;
                case 5:
                    this._animationStepsTextBox.Text += "\n  -> Digest References";
                    this._animationStepsTextBox.Text += "\n    -> Find DigestAlgorithm";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "DigestMethod", _actualSignatureNumber).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "DigestMethod", _actualSignatureNumber), this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "/ds:DigestMethod", _actualSignatureNumber));
                    _referenceStatus++;
                    break;
                case 6:
                    this._animationStepsTextBox.Text += "\n    -> Calculated DigestValue:" + "\n       " + this.webService.Validator.DigestElement(_signaturenumber, _referenceNumber);
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    _referenceStatus++;
                    break;
                case 7:
                    this._animationStepsTextBox.Text += "\n    -> Compare the DigestValues:";
                    this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                    this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "DigestValue", _actualSignatureNumber).BringIntoView();
                    this.AnimateFoundElements(this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "DigestValue", _actualSignatureNumber), this.FindTreeViewItem((TreeViewItem)this._tempReferenceCollection[this._referenceNumber], "/ds:DigestValue", _actualSignatureNumber));
                    _referenceStatus++;
                    break;
                case 8:
                    if (this.webService.Validator.CompareDigestValues(_signaturenumber, _referenceNumber, this.webService.Validator.DigestElement(_signaturenumber, _referenceNumber)))
                    {

                        this._animationStepsTextBox.Text += "\n Reference Validation succesfull";
                        this._animationStepsTextBox.ScrollToLine(this._animationStepsTextBox.LineCount - 1);
                        this._referenceValid = true;
                        _referenceStatus++;
                    }
                    else
                    {
                        this._animationStepsTextBox.Text += "\n Reference Validation failed";
                        _referenceStatus++;
                        this._referenceValid = false;
                    }

                    break;
                case 9:
                    _referenceTimer.Stop();
                    _referenceNumber++;
                    // status = 7;
                    _dispatcherTimer.Start();
                    break;
            }

        }
       private void TransformTimerTickEventHandler(object sender, EventArgs e)
        {
            switch (_transformstatus)
            {
                case 1:
                    _transformTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    this._animationStepsTextBox.Text += "\n Make Transforms";
                    _transformstatus++;

                    break;
                case 2:
                    this._animationStepsTextBox.Text += "\n -> Find Transform";
                    TreeViewItem tempTransform = (TreeViewItem)this._tempTransformCollection[_transformNumber];
                    tempTransform.BringIntoView();
                    this.AnimateFoundElements(tempTransform, tempTransform);
                    _transformstatus++;
                    break;

                case 3:
                    this._animationStepsTextBox.Text += "\n  ->execute Transform";
                    this._animationStepsTextBox.Text += "\n" + this.webService.Validator.MakeTransforms(_signaturenumber, _referenceNumber, _transformNumber);
                    _transformstatus++;
                    break;

                case 4:
                    if (this._transformNumber + 1 < this._transformCount)
                    {
                        this._transformNumber++;
                        _transformstatus = 2;
                        slider1.Value++;
                    }
                    else
                    {
                        _transformTimer.Stop();
                        _referenceTimer.Start();

                        _referenceStatus = 5;
                    }


                    break;




            }
        }
        private void InitializeTransformAnimation()
        {
            _transformstatus = 1;
            _transformNumber = 0;
            _transformTimer.Start();


        }

        private void InitializeReferenceAnimation()
        {

            _referenceStatus = 1;
            _referenceNumber = 0;
            _referenceTimer.Start();


        }
        public void InitializeAnimation()
        {
            _status = 1;
            _signaturenumber = 0;

        }

        private void AnimateFoundElements(TreeViewItem item, TreeViewItem item2)
        {
            Storyboard storyBoard = new Storyboard();
            _textSizeAnimation = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            _textSizeAnimationReverse = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            _textSizeAnimation1 = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            _textSizeAnimationReverse1 = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            storyBoard.Children.Add(_textSizeAnimation);
            storyBoard.Children.Add(_textSizeAnimationReverse);
            storyBoard.Children[0].BeginTime = new TimeSpan(0, 0, 2);
            storyBoard.Children[1].BeginTime = new TimeSpan(0, 0, 4);
            storyBoard.Children.Add(_textSizeAnimation1);
            storyBoard.Children.Add(_textSizeAnimationReverse1);
            storyBoard.Children[2].BeginTime = new TimeSpan(0, 0, 2);
            storyBoard.Children[3].BeginTime = new TimeSpan(0, 0, 4);
            Storyboard.SetTarget(_textSizeAnimation, item);
            Storyboard.SetTarget(_textSizeAnimationReverse, item);
            Storyboard.SetTarget(_textSizeAnimation1, item2);
            Storyboard.SetTarget(_textSizeAnimationReverse1, item2);
            Storyboard.SetTargetProperty(_textSizeAnimation, new PropertyPath(TextBlock.FontSizeProperty));
            Storyboard.SetTargetProperty(_textSizeAnimationReverse, new PropertyPath(TextBlock.FontSizeProperty));
            Storyboard.SetTargetProperty(_textSizeAnimation1, new PropertyPath(TextBlock.FontSizeProperty));
            Storyboard.SetTargetProperty(_textSizeAnimationReverse1, new PropertyPath(TextBlock.FontSizeProperty));
            storyBoard.Begin();
            StackPanel panel = (StackPanel)item.Header;
            TextBlock block = (TextBlock)panel.Children[0];
            storyBoard.Children.Clear();

        }


        public TreeViewItem FindSignatureItems(TreeViewItem treeViewItem, string name)
        {

            StackPanel tempHeader1 = (StackPanel)treeViewItem.Header;

            // string Bezeichner = getNameFromPanel(tempHeader1);
            TextBlock text1 = (TextBlock)tempHeader1.Children[1];
            if (text1.Text.Equals(name))
            {

                item1 = treeViewItem;
                if (name.Equals("ds:Reference"))
                {
                    this._tempReferenceCollection.Add(treeViewItem);
                }
                if (name.Equals("ds:Transform"))
                {
                    this._tempTransformCollection.Add(treeViewItem);

                }
                this._signatureCollection.Add(treeViewItem);
                return treeViewItem;

            }
            foreach (TreeViewItem childItem in treeViewItem.Items)
            {
                FindSignatureItems(childItem, name);

            }
            if (item1 != null)
            {
                return item1;
            }

            return null;
        }

        public TreeViewItem FindTreeViewItem(TreeViewItem treeViewItem, string name, int n)
        {

            StackPanel tempHeader1 = (StackPanel)treeViewItem.Header;
            string panelName = GetNameFromPanel(tempHeader1);
            if (panelName != null)
            {
                if (panelName.Equals(name))
                {
                    item1 = treeViewItem;

                    return treeViewItem;
                }
            }
            foreach (TreeViewItem childItem in treeViewItem.Items)
            {
                FindTreeViewItem(childItem, name, 4);
            }
            if (item1 != null)
            {
                return item1;
            }
            return null;
        }
        private string GetNameFromPanel(StackPanel panel)
        {
            foreach (object obj in panel.Children)
            {
                if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                {
                    TextBlock tb = (TextBlock)obj;
                    if (tb.Name.Equals("tbName"))
                    {
                        string name = tb.Text;
                        if (!name.StartsWith("/"))
                        {

                            string[] splitter = name.Split(new Char[] { ':' });
                            name = splitter[splitter.Length - 1];
                            return name;
                        }
                        else
                        {
                            return name;
                        }
                    }
                }
            }
            return null;
        }

       private void SettingsPropertyChangedEventHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TargetFilename")
            {
                string filename = webService.WebServiceSettings.TargetFilename;

                {
                    this.Save(filename);
                }
            }
        }


        private void AnimateTextBlock(string animatedBlockName)
        {
            DoubleAnimation widthAnimation = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            widthAnimation.AutoReverse = true;
            this.textBlock1.Inlines.Clear();
            this.textBlock1.Inlines.Add(_webMethod);
            this.textBlock1.Inlines.Add(_methodVisibility);
            this.textBlock1.Inlines.Add(_returnParam);
            this.textBlock1.Inlines.Add(_methodName);
            _openBrace = new Run("(");
            this.textBlock1.Inlines.Add(_openBrace);
            _comma = new Run(",");
            this.textBlock1.Inlines.Add(_intParams);
            TextRange intParamsText = new TextRange(_intParams.ContentStart, _intParams.ContentEnd);
            TextRange stringParamsText = new TextRange(_stringParams.ContentStart, _stringParams.ContentEnd);
            TextRange doubleParamsText = new TextRange(_doubleParams.ContentStart, _doubleParams.ContentEnd);
            if (!intParamsText.Text.Equals(""))
            {
                if (!(stringParamsText.Text.Equals("")))
                {
                    Run nochnRun = new Run(",");
                    this.textBlock1.Inlines.Add(nochnRun);
                    intParamsText = new TextRange(_intParams.ContentStart, _intParams.ContentEnd);
                    stringParamsText = new TextRange(_stringParams.ContentStart, _stringParams.ContentEnd);
                    doubleParamsText = new TextRange(_doubleParams.ContentStart, _doubleParams.ContentEnd);

                }
                else
                {
                    if (!doubleParamsText.Text.Equals(""))
                    {
                        this.textBlock1.Inlines.Add(_comma);
                    }
                }
            }
            this.textBlock1.Inlines.Add(_stringParams);
            if (!intParamsText.Text.Equals(""))
            {
                if (!(doubleParamsText.Text.Equals("")))
                {
                    this.textBlock1.Inlines.Add(_comma);
                }
            }
            this.textBlock1.Inlines.Add(_doubleParams);
            _closeBrace = new Run(")");
            this.textBlock1.Inlines.Add(_closeBrace);

            switch (animatedBlockName)
            {
                case "methodName":
                    _methodName.BeginAnimation(Bold.FontSizeProperty, widthAnimation);
                    break;
                case "returnParam":
                    _returnParam.BeginAnimation(Run.FontSizeProperty, widthAnimation);
                    break;
                case "int":
                    _intParams.BeginAnimation(Bold.FontSizeProperty, widthAnimation);
                    break;
                case "string":
                    _stringParams.BeginAnimation(Bold.FontSizeProperty, widthAnimation);
                    break;
                case "float":
                    _doubleParams.BeginAnimation(Bold.FontSizeProperty, widthAnimation);
                    break;
            }
            textBlock1.Inlines.Add(_openMethodBrace);
            textBlock2.Visibility = Visibility.Visible;

        }
        public void VisualReturnParam(string returnType)
        {
            Run returnParam = new Run(" " + returnType);
            returnParam.Foreground = Brushes.Blue;
            this._returnParam = returnParam;
            this.AnimateTextBlock("returnParam");


        }
        public void VisualMethodName(string name)
        {
            Run visibility = new Run("public");
            visibility.Foreground = Brushes.Blue;
            Run methodName = new Run(" " + name);
            this._methodVisibility = visibility;
            this._methodName = methodName;
            this.AnimateTextBlock("methodName");

        }

        public void VisualParam(string parameterType, int parameterCount)
        {
            Bold bold = new Bold();
            string paramName = "";
            for (int i = 1; i <= parameterCount; i++)
            {

                Run typRun = new Run(parameterType);
                typRun.Foreground = Brushes.Blue;

                if (parameterType.Equals("int"))
                {
                    paramName = "intparam";
                }
                if (parameterType.Equals("string"))
                {
                    paramName = "stringparam";
                }
                if (parameterType.Equals("double"))
                {
                    paramName = "doubleparam";
                }
                Run nameRun;
                if (i < parameterCount)
                {
                    nameRun = new Run(" " + paramName + "" + i + ",");
                }
                else { nameRun = new Run(" " + paramName + "" + i); }
                bold.Inlines.Add(typRun);
                bold.Inlines.Add(nameRun);
            }
            switch (parameterType)
            {
                case "int":
                    _intParams = bold;
                    this.AnimateTextBlock(parameterType);
                    break;
                case "string":
                    _stringParams = bold;
                    this.AnimateTextBlock(parameterType);
                    break;
                case "double":
                    _doubleParams = bold;
                    this.AnimateTextBlock(parameterType);
                    break;

            }



        }
        public void Save(string fileName)
        {
            if (this.webService.ServiceDescription != null)
            {
                ServiceDescription test = this.webService.ServiceDescription;
                StreamWriter stream = new StreamWriter(fileName);
                test.Write(stream);
                stream.Close();
            }
        }

        public void ShowWsdl(string wsdl)
        {
            this._webServiceDescription = wsdl;
        }
        public string GetStringToCompile()
        {
            TextRange methodCode = new TextRange(this.richTextBox1.Document.ContentStart, this.richTextBox1.Document.ContentEnd);
            TextRange endBrace = new TextRange(this.textBlock2.Inlines.FirstInline.ContentStart, this.textBlock2.Inlines.FirstInline.ContentEnd);
            string header = CopyTextBlockContentToString(this.textBlock1);

            string codeToCompile = header + methodCode.Text + endBrace.Text;
            return codeToCompile;
        }
        public string CopyTextBlockContentToString(TextBlock block)
        {
            string blockString = "";
            foreach (Inline inline in textBlock1.Inlines)
            {
                TextRange inlineRange = new TextRange(inline.ContentStart, inline.ContentEnd);
                blockString += inlineRange.Text;
            }
            return blockString;
        }

        private void richTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange code = new TextRange(this.richTextBox1.Document.ContentStart, this.richTextBox1.Document.ContentEnd);
            if (webService != null)
            {
                this.webService.WebServiceSettings.UserCode = code.Text;
            }



        }


        public AnimationController getAnimationController()
        {
            return this._animationController;
        }






        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.webService.CheckSignature() == true)
            {

            }
            else
            {
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {

            this.FindSignatureItems((TreeViewItem)this.soapInput.Items[0], "ds:Signature");
            if (this._signatureCollection.Count > 0)
            {
                InitializeAnimation();
                _dispatcherTimer.Start();
            }
            else
            {// calculateBox.Text = "There is no signature in the message"; 
            }
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            decryptionAnimation = new DecryptionAnimation(this);
            this.button1.IsEnabled = true;
            namespacesTable.Clear();
            this._signatureCollection.Clear();
            this._tempReferenceCollection.Clear();
            this._animationStepsTextBox.Clear();

            _dispatcherTimer.Stop();
            _animationController.ControllerTimer.Stop();
            _decryptionTimer.Stop();
       

            this.ResetSoapInputItem();

            this._animationStepsTextBox.Clear();
         
          //  this.soapInput = new TreeViewItem();
        
           
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this._actualSignatureNumber = 1;
            //  status = (int)this.slider1.Value+1;
        }

        private void button1_Click_2(object sender, RoutedEventArgs e)
        {

            this._animationController.GetTotalSecurityElementsNumber();

            decryptionAnimation.initializeAnimation();
            decryptionAnimation.fillEncryptedDataTreeviewElements();

        }
        public DispatcherTimer getSignatureTimer()
        {
            return this._dispatcherTimer;
        }

        private void button1_Click_3(object sender, RoutedEventArgs e)
        {
            this._animationController.InitializeAnimation();
            this.button1.IsEnabled = false;

        }
        #endregion

        private void CompileButtonClickEventHandler(object sender, RoutedEventArgs e)
        {
             
            this.textBox3.Clear();
            this.webService.Compile(this.GetStringToCompile());
        }

        }

    
}
