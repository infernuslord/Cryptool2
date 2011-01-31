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


namespace WebService
{
    /// <summary>
    /// Interaktionslogik für WebServicePresentation.xaml
    /// </summary>
    public partial class WebServicePresentation : System.Windows.Controls.UserControl
    {
        public TreeViewItem item, inboundPolicy,soapInput, item1;
    
        private bool referenceValid = true;
        private ArrayList signatureCollection, tempReferenceCollection, tempTransformCollection;
        private string theWsdl;
         public WebService webService;
        private string lastURI;
        public Hashtable namespacesTable;
        private FlowDocument flowDocument;
        private Run methodVisibility, returnParam, methodName, openBrace, closeBrace, komma, openMethodBrace, closeMethodBrace;
        private Bold webMethod;
        private  FlowDocument doc;
        private Bold intparams, stringparams, doubleparams;
        private System.ComponentModel.SortDescription SD;
        private DispatcherTimer dispatcherTimer,decryptionTimer, referenceTimer, transformTimer;
        private DoubleAnimation TextSizeAnimation, TextSizeAnimationReverse,TextSizeAnimation1, TextSizeAnimationReverse1;
        private int status, referenceStatus,transformstatus, signatureNumber, actualSignatureNumber,signaturenumber, actualReferenceNumber,referenceNumber,transformNumber,transformCount;
        private AnimationController animationController;
        public bool allowExecute, isbusy;
        public DecryptionAnimation decAnimation;
        public WebServicePresentation(WebService webService)
        {
          
    
            InitializeComponent();
            actualSignatureNumber = 1;
            decAnimation = new DecryptionAnimation(this);
            slider1.Opacity = 0;
            allowExecute = false;
            isbusy = true;
       animationController = new AnimationController(this);
            this.signatureCollection = new ArrayList();
            this.tempTransformCollection = new ArrayList();
     

            StackPanel panel2= new StackPanel();
            panel2.Orientation = System.Windows.Controls.Orientation.Horizontal;
            panel2.Children.Add(new TextBlock(new Run("Tesfdsfsdft2")));
            panel2.Children.Add(new TextBlock(new Run("Test2")));
            panel2.Children.Add(new CheckBox());
        
            ComboBox encryptionCombo =new ComboBox();
            ComboBoxItem comboItem= new ComboBoxItem();
            comboItem.Content="Content";
            ComboBoxItem comboItem2 = new ComboBoxItem();
            comboItem2.Content ="Element";
           
            encryptionCombo.Items.Add(comboItem);
            encryptionCombo.Items.Add(comboItem2);
            encryptionCombo.SelectedItem = comboItem;
           

      
            Paragraph par = (Paragraph) this.richTextBox1.Document.Blocks.FirstBlock;
            par.LineHeight = 5;
            status = 1;
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            referenceTimer = new DispatcherTimer();
            referenceTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            decryptionTimer = new DispatcherTimer();
            decryptionTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            transformTimer = new DispatcherTimer();
            transformTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            TextSizeAnimation = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            TextSizeAnimationReverse = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            TextSizeAnimation1 = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            TextSizeAnimationReverse1 = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            decryptionTimer.Tick += new EventHandler(decryptionTimer_Tick);
            referenceTimer.Tick += new EventHandler(referenceTimer_Tick);
            transformTimer.Tick += new EventHandler(transformTimer_Tick);
            doc=new FlowDocument(); 
            webMethod =  new Bold(new Run("[WebMethod]"+"\n"));
            methodVisibility = new Run();
            returnParam = new Run();
            methodName = new Run();
            intparams = new Bold();
            stringparams = new Bold();
            doubleparams = new Bold();
            openBrace = new Run();
            closeBrace = new Run();
            komma = new Run(",");
            Paragraph para = new Paragraph(webMethod);
            openMethodBrace = new Run("\n{");
           
            textBlock2.Inlines.Add(new Run("}"));
            textBlock2.Visibility=Visibility.Visible;

            this.visualMethodName("methodName");

            this.richTextBox1.Document = doc;
            this.textBlock1.Inlines.Add(webMethod);
            this.textBlock1.Inlines.Add(methodVisibility);
            this.textBlock1.Inlines.Add(returnParam);
            this.textBlock1.Inlines.Add(methodName);
            this.textBlock1.Inlines.Add(openBrace);
           

            this.textBlock1.Inlines.Add(intparams);
            this.textBlock1.Inlines.Add(stringparams);
            this.textBlock1.Inlines.Add(doubleparams);
            this.textBlock1.Inlines.Add(closeBrace);
            textBlock1.Inlines.Add(openMethodBrace);
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
            TreeViewItem inboundPolicyRoot = new TreeViewItem();
            TextBlock block2 = new TextBlock();
            block2.Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
            StackPanel panel3 = new StackPanel();
            panel2.Children.Add(block2);
            inboundPolicyRoot.Header = panel3;
            inboundPolicyRoot.Header = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
        
            inboundPolicy = new TreeViewItem();
            inboundPolicy.Header = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
           
            namespacesTable = new Hashtable();

            SD = new System.ComponentModel.SortDescription("param", System.ComponentModel.ListSortDirection.Ascending);
            tempReferenceCollection = new ArrayList();
 
       
           
            webService.Settings.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Settings_PropertyChanged);
        }

      

       

        void decryptionTimer_Tick(object sender, EventArgs e)
        {
            
        }
        public void resetSoapInputItem()
        {
            soapInput = new TreeViewItem();
            TextBlock block = new TextBlock();
            block.Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
            StackPanel panel = new StackPanel();
            panel.Children.Add(block);
            soapInput.Header = panel;
            soapInput.IsExpanded = true;
            this.treeView4.Items.Clear();
        }

        public void resetPolicyItem()
        {
           // this.treeView2.Items.Clear();
       inboundPolicy = new TreeViewItem();
            TextBlock block = new TextBlock();
            block.Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
            StackPanel panel = new StackPanel();
            panel.Children.Add(block);
            inboundPolicy.Header = panel;
            inboundPolicy.IsExpanded = true;
        }
        public void CopyXmlToTreeView(XmlNode xNode, ref TreeViewItem tviParent, bool withPics)
        {
            SolidColorBrush elemBrush = new SolidColorBrush(Colors.MediumVioletRed);
            if (xNode != null)
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
                tbName.Text = xNode.Name;
                tbTagOpen.Foreground = elemBrush;
                tbTagClose.Foreground = elemBrush;
                tbName.Foreground = elemBrush;
                if (!xNode.NodeType.ToString().Equals("Text"))
                {
                    item.Name = "OpenItemXmlNode";
                    panel.Name = "OpenPanelXMLNode";
                    TreeViewItem closeitem = new TreeViewItem();
                    panel.Children.Insert(0, tbTagOpen);
                    panel.Children.Add(tbName);
                    if (!xNode.NamespaceURI.Equals(""))
                    {
                       insertNamespace(ref panel, xNode.NamespaceURI, xNode.Prefix);
                    }
                    if (xNode.Attributes != null)
                    {
                        insertAttributes(ref panel, xNode.Attributes);
                    }
         
                    panel.Children.Add(tbTagClose);

                    if (withPics)

                    {
                        if (xNode.Name.Equals("s:Body"))
                        {
                            addOpenLockToPanel(ref panel, xNode.Name, false);
                        }
                        else
                        {
                            addOpenLockToPanel(ref panel, xNode.Name, true);
                        }
                    }  
                   
                
                    item.Header = panel;
                    closeitem.Foreground = elemBrush;
                    tviParent.Items.Add(item);
                    if (xNode.HasChildNodes)
                    {
                        foreach (XmlNode child in xNode.ChildNodes)
                        {
                            lastURI = xNode.NamespaceURI; ;
                            CopyXmlToTreeView(child, ref item,withPics);
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
                    elem1.Text = "/"+xNode.Name;
                    panel1.Children.Add(elem1);
                    TextBlock tbTagClose3 = new TextBlock();
                    tbTagClose3.Name = "tbTagClose";
                    tbTagClose3.Text = ">";
                    panel1.Children.Add(tbTagClose3);
                  
                    closeitem.Header = panel1;
                    
                    tviParent.Items.Add(closeitem);
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
                    tbText.Text = xNode.Value;
                    TextBlock emptyTextBlock = new TextBlock();
                    emptyTextBlock.Text = "";
                    panel.Children.Insert(0,emptyTextBlock);
                    panel.Children.Add(tbText);
                    item.Header = panel;
                    tviParent.Items.Add(item);
                }
            }
        }
         private void addOpenLockToPanel(ref StackPanel panel,string name, bool open)
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
                {string[] nameSplitter=splitter[1].ToString().Split(new Char[] {'+'});
                  
                    splitter[1] = nameSplitter[0];
                 nameSpace=   nameSplitter[1].ToString()+ ":" + splitter[n - 1].ToString();
                
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
                myImage2.MouseLeftButtonDown += new MouseButtonEventHandler(myImage2_MouseLeftButtonDown);
            }
            if (!open)
            {
                myImage2.MouseLeftButtonDown+=new MouseButtonEventHandler(myImage2_MouseLeftButtonDownClose);
            }
           
            
            myImage2.ToolTip = "Click this picture to encrypt the <" + name + "> element";
            //myImage2.MouseEnter += new MouseEventHandler(myImage2_MouseEnter);
            //myImage2.MouseLeave += new MouseEventHandler(myImage2_MouseLeave);
            //ContextMenu menu = new ContextMenu();
            //MenuItem item1 = new MenuItem();
            //item1.Header="Test";
            //item1.IsCheckable=true;
            //menu.Items.Add(item1);
            
            //myImage2.ContextMenu = menu;
            panel.Children.Add(myImage2);
        }
         void myImage2_MouseLeftButtonDownClose(object sender, MouseButtonEventArgs e)
         {
             Image img = (Image)sender;
             string name = img.Name;
       




         }
         void myImage2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
         {
             Image img = (Image)sender;
             string name = img.Name;
      
        
         
             
            
         }
        public StackPanel insertAttributes(ref StackPanel panel, XmlAttributeCollection attributes)
        {
            foreach (XmlAttribute tempAttribute in attributes)
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
        public StackPanel insertNamespace(ref StackPanel panel, string nspace, string Prefix)
        {
            if (!namespacesTable.ContainsValue(nspace))
            {
                namespacesTable.Add(nspace, nspace);
                TextBlock xmlns = new TextBlock();
                xmlns.Name = "xmlns";
                xmlns.Text = " xmlns";
                TextBlock prefix = new TextBlock();
                prefix.Name = "xmlnsPrefix";
                if (!Prefix.Equals(""))
                { prefix.Text = ":" + Prefix; }
                else { prefix.Text = ""; }
                SolidColorBrush valueBrush = new SolidColorBrush(Colors.Blue);
                TextBlock value = new TextBlock();
                value.Name = "xmlnsValue";
                value.Text = "=" + "\"" + nspace + "\"";
                value.Foreground = valueBrush;
                panel.Children.Add(xmlns);
                panel.Children.Add(prefix);
                panel.Children.Add(value);
            }
            return panel;
        }
        void dispatcherTimer_Tick(object sender, EventArgs e)
        {  
            switch(status)
            {
            case 1:
        
                signatureNumber = this.webService.GetSignatureNumber();
                this.textBox2.ScrollToLine(this.textBox2.LineCount - 1);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                this.textBox2.Text+="\n Check for Signature Element";
                this.textBox2.ScrollToLine(this.textBox2.LineCount - 1);
         
                this.treeView4.Items.Refresh();
    
               this.findItem((TreeViewItem)this.signatureCollection[this.signaturenumber],"Signature",1).BringIntoView();

               this.animateFoundElements((TreeViewItem)this.signatureCollection[this.signaturenumber], (TreeViewItem)signatureCollection[this.signaturenumber]); 
               status = 2;
                slider1.Value++;
                break;
            case 2:
                this.textBox2.Text+="\n Canonicalize SignedInfo";
                this.textBox2.ScrollToLine(this.textBox2.LineCount - 1);
                this.findItem((TreeViewItem)this.signatureCollection[this.signaturenumber], "SignedInfo",1).BringIntoView();
                this.animateFoundElements(this.findItem((TreeViewItem)this.signatureCollection[this.signaturenumber], "/SignedInfo", actualSignatureNumber), this.findItem((TreeViewItem)this.signatureCollection[this.signaturenumber], "/ds:SignedInfo>", actualSignatureNumber));
                   
               
            
                status=3;
                slider1.Value++;
                break;
            case 3:
                this.textBox2.Text+="\n -->Find Canonicalization Algorithm";
                this.textBox2.ScrollToLine(this.textBox2.LineCount - 1);
                this.findItem((TreeViewItem)this.signatureCollection[this.signaturenumber], "CanonicalizationMethod", actualSignatureNumber).BringIntoView();
                this.animateFoundElements(this.findItem((TreeViewItem)this.signatureCollection[this.signaturenumber], "/CanonicalizationMethod", actualSignatureNumber), this.findItem((TreeViewItem)this.signatureCollection[this.signaturenumber], "/ds:CanonicalizationMethod>", actualSignatureNumber));
               
                
                status = 4;
                slider1.Value++;
                break;
            case 4:
               // this.calculateBox.Text = this.webService.getValidator().canonicalizeSignedInfo(this.signaturenumber);

                this.textBox2.ScrollToLine(this.textBox2.LineCount - 1);
                this.findSignatureItems((TreeViewItem)signatureCollection[this.signaturenumber], "ds:Reference");
                this.initializeReferenceAnimation();
                
             
             
               status = 5;

               dispatcherTimer.Stop();
               slider1.Value++;
               break;
          
            case 5:
             

                   this.textBox2.Text += "\n Signature Validation";
                   this.textBox2.ScrollToLine(this.textBox2.LineCount - 1);
                   this.textBox2.Text += "\n -> Find Signature Method";
                   this.textBox2.ScrollToLine(this.textBox2.LineCount - 1);
                   this.findItem(this.soapInput, "SignatureMethod", actualSignatureNumber).BringIntoView();
                   this.animateFoundElements(this.findItem((TreeViewItem)this.signatureCollection[this.signaturenumber], "SignatureMethod", actualSignatureNumber), this.findItem((TreeViewItem)this.signatureCollection[this.signaturenumber], "/ds:SignatureMethod", actualSignatureNumber));
                   status = 6;
                   slider1.Value++;
               
             
               break;
                case 6:
               this.textBox2.Text += "\n Get public key for signature validation";
               this.textBox2.ScrollToLine(this.textBox2.LineCount - 1);
               this.findItem((TreeViewItem)signatureCollection[this.signaturenumber], "KeyInfo", 1).BringIntoView();
               this.animateFoundElements(this.findItem((TreeViewItem)signatureCollection[this.signaturenumber], "KeyInfo", 1), this.findItem((TreeViewItem)signatureCollection[this.signaturenumber], "/ds:KeyInfo", 1));
               status = 7;
               break;
            case 7:
               

                   this.textBox2.Text += "\n -> Validate SignatureValue over SignedInfo";
                   this.textBox2.ScrollToLine(this.textBox2.LineCount - 1);

                   this.findItem((TreeViewItem)this.signatureCollection[this.signaturenumber], "SignatureValue", actualSignatureNumber).BringIntoView();
                   this.animateFoundElements(this.findItem((TreeViewItem)this.signatureCollection[this.signaturenumber], "SignatureValue", actualSignatureNumber), this.findItem((TreeViewItem)this.signatureCollection[this.signaturenumber], "/ds:SignatureValue", actualSignatureNumber));
               
               dispatcherTimer.Stop();
               status = 8;
               slider1.Value++;
               this.animationController.getControllerTimer().Start();
               break;
                case 8:
               this.tempReferenceCollection.Clear();
               this.animationController.getControllerTimer().Start();
               signatureNumber = this.webService.GetSignatureNumber();
               isbusy = false;
               this.actualSignatureNumber++;
               if (this.signaturenumber + 1 < this.signatureNumber)
               {
                   isbusy = true;
                   this.signaturenumber++;
                   status = 1;
                   slider1.Value++;
               }

             
               break;
            }
          
           
        }
        void referenceTimer_Tick(object sender, EventArgs e)
        {
            int n = this.webService.Validator().GetReferenceNumber(signaturenumber);
            switch (referenceStatus)
            {
                 case 1:
                    referenceTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    this.textBox2.Text += "\n Reference Validation";
                    this.textBox2.ScrollToLine(this.textBox2.LineCount - 1);
                    referenceStatus++;
                 break;
                case 2:
                   this.textBox2.Text += "\n -> Find Reference Element";
                   this.textBox2.ScrollToLine(this.textBox2.LineCount - 1);
                   // this.findSignatureItems((TreeViewItem)signatureCollection[this.i], "ds:Reference").BringIntoView() ;
                   this.findItem((TreeViewItem)this.tempReferenceCollection[this.referenceNumber], "Reference", actualSignatureNumber).BringIntoView();
                   this.animateFoundElements(this.findItem((TreeViewItem)this.tempReferenceCollection[this.referenceNumber], "Reference", actualSignatureNumber), this.findItem((TreeViewItem)this.tempReferenceCollection[this.referenceNumber], "/ds:Reference", actualSignatureNumber));
                   referenceStatus++;
                   break;

                case 3:
                   this.textBox2.Text += "\n -> Get referenced Element";
                  this.findItem(this.soapInput, this.webService.Validator().GetSignatureReferenceName(signaturenumber),actualReferenceNumber).BringIntoView();
                  this.animateFoundElements(this.findItem(this.soapInput, this.webService.Validator().GetSignatureReferenceName(signaturenumber), actualReferenceNumber), this.findItem(this.soapInput, this.webService.Validator().GetSignatureReferenceName(signaturenumber), actualReferenceNumber));
                   referenceStatus++;
                   break;
                
                case 4:
                   this.textBox2.Text += "\n  -> Apply Transforms";
                   this.textBox2.ScrollToLine(this.textBox2.LineCount - 1);
                   this.findItem((TreeViewItem)this.tempReferenceCollection[this.referenceNumber], "Transforms", actualSignatureNumber).BringIntoView();
                   this.animateFoundElements(this.findItem((TreeViewItem)this.tempReferenceCollection[this.referenceNumber], "Transforms", actualSignatureNumber), this.findItem((TreeViewItem)this.tempReferenceCollection[this.referenceNumber], "/ds:Transforms", actualSignatureNumber));
                   this.transformCount = this.webService.Validator().GetTransformsCounter(signaturenumber, referenceNumber);
                   referenceTimer.Stop();
                   referenceStatus++;
                   this.findSignatureItems((TreeViewItem)this.tempReferenceCollection[this.referenceNumber], "ds:Transform");
                   initializeTransformAnimation();
                   
                   break;
                case 5:
                   this.textBox2.Text += "\n  -> Digest References";
                   this.textBox2.Text += "\n    -> Find DigestAlgorithm";
                   this.textBox2.ScrollToLine(this.textBox2.LineCount - 1);
                   this.findItem((TreeViewItem)this.tempReferenceCollection[this.referenceNumber], "DigestMethod", actualSignatureNumber).BringIntoView();
                   this.animateFoundElements(this.findItem((TreeViewItem)this.tempReferenceCollection[this.referenceNumber], "DigestMethod", actualSignatureNumber), this.findItem((TreeViewItem)this.tempReferenceCollection[this.referenceNumber], "/ds:DigestMethod", actualSignatureNumber));
                   referenceStatus++;
                   break;
                case 6:
                   this.textBox2.Text +="\n    -> Calculated DigestValue:" +"\n       "+this.webService.Validator().DigestElement(signaturenumber,referenceNumber);
                   this.textBox2.ScrollToLine(this.textBox2.LineCount - 1);
                   referenceStatus++;
                   break;
                case 7:
                    this.textBox2.Text+="\n    -> Compare the DigestValues:";
                    this.textBox2.ScrollToLine(this.textBox2.LineCount - 1);
                   this.findItem((TreeViewItem)this.tempReferenceCollection[this.referenceNumber], "DigestValue", actualSignatureNumber).BringIntoView();
                   this.animateFoundElements(this.findItem((TreeViewItem)this.tempReferenceCollection[this.referenceNumber], "DigestValue", actualSignatureNumber), this.findItem((TreeViewItem)this.tempReferenceCollection[this.referenceNumber], "/ds:DigestValue", actualSignatureNumber));
                   referenceStatus++;
                   break;
                case 8:
                   if (this.webService.Validator().CompareDigestValues(signaturenumber, referenceNumber, this.webService.Validator().DigestElement(signaturenumber, referenceNumber)))
                   {

                       this.textBox2.Text += "\n Reference Validation succesfull";
                       this.textBox2.ScrollToLine(this.textBox2.LineCount - 1);
                       this.referenceValid = true;
                       referenceStatus++;
                   }
                   else
                   {
                       this.textBox2.Text += "\n Reference Validation failed";
                       referenceStatus++;
                       this.referenceValid = false;
                   }
                    
                    break;
                case 9:
                    referenceTimer.Stop();
                    referenceNumber++;
                   // status = 7;
                    dispatcherTimer.Start();
                    break;
            }
            
        }
        void transformTimer_Tick(object sender, EventArgs e)
        {
            switch (transformstatus)
            {
                case 1:
                    transformTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);
                    this.textBox2.Text += "\n Make Transforms";
                    transformstatus++;

                    break;
                case 2:
                    this.textBox2.Text+="\n -> Find Transform";
                    TreeViewItem tempTransform = (TreeViewItem)this.tempTransformCollection[transformNumber];
                    tempTransform.BringIntoView();
                    this.animateFoundElements(tempTransform, tempTransform);
                    transformstatus++;
                    break;

                case 3:
                    this.textBox2.Text += "\n  ->execute Transform";
                   this.textBox2.Text+="\n"+ this.webService.Validator().MakeTransforms(signaturenumber, referenceNumber, transformNumber);
                   
                    
                    transformstatus++;
                    break;

                case 4:
                    if (this.transformNumber + 1 < this.transformCount)
                    {
                        this.transformNumber++;
                        transformstatus = 2;
                        slider1.Value++;
                    }
                    else
                    {
                        transformTimer.Stop();
                        referenceTimer.Start();
                        
                        referenceStatus = 5;
                    }
                    

                    break;




            }
        }
        public void initializeTransformAnimation()
        {
            transformstatus = 1;
            transformNumber = 0;
            transformTimer.Start();
         

        }

        public void initializeReferenceAnimation()
        {

            referenceStatus = 1;
            referenceNumber = 0;
            referenceTimer.Start();
           

        }
        public void initializeAnimation()
        {
            status = 1;
            signaturenumber=0;
          
        }
        public void animateFoundElements(TreeViewItem item, TreeViewItem item2,int i)
        {
            Storyboard storyBoard = new Storyboard();
            TextSizeAnimation = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            TextSizeAnimationReverse = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            TextSizeAnimation1 = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            TextSizeAnimationReverse1 = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            storyBoard.Children.Add(TextSizeAnimation);
            storyBoard.Children.Add(TextSizeAnimationReverse);
            storyBoard.Children[0].BeginTime = new TimeSpan(0, 0, i);
            storyBoard.Children[1].BeginTime = new TimeSpan(0, 0, i+2);
            storyBoard.Children.Add(TextSizeAnimation1);
            storyBoard.Children.Add(TextSizeAnimationReverse1);
            storyBoard.Children[2].BeginTime = new TimeSpan(0, 0, i);
            storyBoard.Children[3].BeginTime = new TimeSpan(0, 0, i+2);
            Storyboard.SetTarget(TextSizeAnimation, item);
            Storyboard.SetTarget(TextSizeAnimationReverse, item);
            Storyboard.SetTarget(TextSizeAnimation1, item2);
            Storyboard.SetTarget(TextSizeAnimationReverse1, item2);
            Storyboard.SetTargetProperty(TextSizeAnimation, new PropertyPath(TextBlock.FontSizeProperty));
            Storyboard.SetTargetProperty(TextSizeAnimationReverse, new PropertyPath(TextBlock.FontSizeProperty));
            Storyboard.SetTargetProperty(TextSizeAnimation1, new PropertyPath(TextBlock.FontSizeProperty));
            Storyboard.SetTargetProperty(TextSizeAnimationReverse1, new PropertyPath(TextBlock.FontSizeProperty));
            storyBoard.Begin();
            StackPanel panel = (StackPanel)item.Header;
            TextBlock block = (TextBlock)panel.Children[0];
//block.BeginAnimation(TextBlock.FontSizeProperty,TextSizeAnimation);
            //block.BeginAnimation(TextBlock.FontSizeProperty, TextSizeAnimationReverse);

        }
        public void animateFoundElements(TreeViewItem item, TreeViewItem item2)
        {
            Storyboard storyBoard = new Storyboard();
            TextSizeAnimation = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            TextSizeAnimationReverse = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            TextSizeAnimation1 = new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            TextSizeAnimationReverse1 = new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            storyBoard.Children.Add(TextSizeAnimation);
            storyBoard.Children.Add(TextSizeAnimationReverse);
            storyBoard.Children[0].BeginTime = new TimeSpan(0, 0, 2);
            storyBoard.Children[1].BeginTime = new TimeSpan(0, 0, 4);
            storyBoard.Children.Add(TextSizeAnimation1);
            storyBoard.Children.Add(TextSizeAnimationReverse1);
            storyBoard.Children[2].BeginTime = new TimeSpan(0, 0, 2);
            storyBoard.Children[3].BeginTime = new TimeSpan(0, 0, 4);
            Storyboard.SetTarget(TextSizeAnimation, item);
            Storyboard.SetTarget(TextSizeAnimationReverse, item);
            Storyboard.SetTarget(TextSizeAnimation1, item2);
            Storyboard.SetTarget(TextSizeAnimationReverse1, item2);
            Storyboard.SetTargetProperty(TextSizeAnimation, new PropertyPath(TextBlock.FontSizeProperty));
            Storyboard.SetTargetProperty(TextSizeAnimationReverse, new PropertyPath(TextBlock.FontSizeProperty));
            Storyboard.SetTargetProperty(TextSizeAnimation1, new PropertyPath(TextBlock.FontSizeProperty));
            Storyboard.SetTargetProperty(TextSizeAnimationReverse1, new PropertyPath(TextBlock.FontSizeProperty));
            storyBoard.Begin();
            StackPanel panel = (StackPanel)item.Header;
            TextBlock block = (TextBlock)panel.Children[0];
   
            storyBoard.Children.Clear();//block.BeginAnimation(TextBlock.FontSizeProperty,TextSizeAnimation);
            //block.BeginAnimation(TextBlock.FontSizeProperty, TextSizeAnimationReverse);

        }


        public TreeViewItem findSignatureItems(TreeViewItem item, string bezeichner)
        {
          
            StackPanel tempHeader1 = (StackPanel)item.Header;
           
           // string Bezeichner = getNameFromPanel(tempHeader1);
            TextBlock text1 = (TextBlock)tempHeader1.Children[1];
            if (text1.Text.Equals(bezeichner))
            {
               
                    item1 = item;
                if(bezeichner.Equals("ds:Reference"))
                {
                    this.tempReferenceCollection.Add(item);
                }
                if (bezeichner.Equals("ds:Transform"))
                {
                    this.tempTransformCollection.Add(item);

                }
                    this.signatureCollection.Add(item);
                    return item;
             
            }
            foreach (TreeViewItem childItem in item.Items)
            {
                findSignatureItems(childItem, bezeichner);

            }
            if (item1 != null)
            {
                return item1;
            }

            return null;
        }
  
        public TreeViewItem findItem(TreeViewItem item, string bezeichner, int n)
        {
          
            StackPanel tempHeader1 = (StackPanel)item.Header;
            string Bezeichner = getNameFromPanel(tempHeader1);
            if (Bezeichner != null)
            {
                if (Bezeichner.Equals(bezeichner))
                {
                    item1 = item;
                    
                    return item;
                }
            }
            foreach (TreeViewItem childItem in item.Items)
            {
                findItem(childItem, bezeichner,4);
            }
            if (item1 != null)
            {
                return item1;
            }
            return null;
        }
        private string getNameFromPanel(StackPanel panel)
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

        void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TargetFilename")
            {
                string filename = webService.WebServiceSettings.TargetFilename;
             
                {
                    this.save( filename);
                }
            }
        }
       
    
        public void refreshTextBlock(string animateBlock)
        {
            DoubleAnimation widthAnimation =new DoubleAnimation(11, 16, TimeSpan.FromSeconds(1));
            widthAnimation.AutoReverse = true;
            this.textBlock1.Inlines.Clear();
            this.textBlock1.Inlines.Add(webMethod);
        
            this.textBlock1.Inlines.Add(methodVisibility);
            this.textBlock1.Inlines.Add(returnParam);
            this.textBlock1.Inlines.Add(methodName);

            openBrace = new Run("(");
            this.textBlock1.Inlines.Add(openBrace);
            komma = new Run(",");

            this.textBlock1.Inlines.Add(intparams);
            TextRange intParamsText= new TextRange(intparams.ContentStart,intparams.ContentEnd);
            TextRange stringParamsText= new TextRange(stringparams.ContentStart,stringparams.ContentEnd);
            TextRange doubleParamsText = new TextRange(doubleparams.ContentStart, doubleparams.ContentEnd);
            if (!intParamsText.Text.Equals(""))
            {
                if (!(stringParamsText.Text.Equals("")))
                {
                    Run nochnRun = new Run(",");
                        this.textBlock1.Inlines.Add(nochnRun);
                         intParamsText = new TextRange(intparams.ContentStart, intparams.ContentEnd);
                         stringParamsText = new TextRange(stringparams.ContentStart, stringparams.ContentEnd);
                        doubleParamsText = new TextRange(doubleparams.ContentStart, doubleparams.ContentEnd);
                   
                }
                else{ 
                    if(!doubleParamsText.Text.Equals(""))
                    {
                        this.textBlock1.Inlines.Add(komma);
                }
                }
              }
            this.textBlock1.Inlines.Add(stringparams);
            if (!intParamsText.Text.Equals(""))
            {
                if (!(doubleParamsText.Text.Equals("")))
                {
                    this.textBlock1.Inlines.Add(komma);
                }
            }
            this.textBlock1.Inlines.Add(doubleparams);
            closeBrace = new Run(")");
            this.textBlock1.Inlines.Add(closeBrace);
            
            switch (animateBlock)
            {
                case "methodName":
                    methodName.BeginAnimation(Bold.FontSizeProperty, widthAnimation);
                    break;
                case"returnParam":
                    returnParam.BeginAnimation(Run.FontSizeProperty, widthAnimation);
                    break;
                case"int":
                    intparams.BeginAnimation(Bold.FontSizeProperty, widthAnimation);
                    break;
                case"string":
                    stringparams.BeginAnimation(Bold.FontSizeProperty, widthAnimation);
                    break;
                case"float":
                    doubleparams.BeginAnimation(Bold.FontSizeProperty, widthAnimation);
                    break;
            }
            textBlock1.Inlines.Add(openMethodBrace);
            textBlock2.Visibility = Visibility.Visible;

        }
        public void visualReturnParam(string returnType)
        {
            Run returnParam = new Run(" " + returnType);
            returnParam.Foreground = Brushes.Blue;
            this.returnParam = returnParam;
            this.refreshTextBlock("returnParam");


        }
        public void visualMethodName(string name)
        {
            Run visibility  = new Run("public");
            visibility.Foreground = Brushes.Blue;
            Run methodName = new Run(" "+name);
            this.methodVisibility = visibility;
            this.methodName = methodName;
            this.refreshTextBlock("methodName");
            
        }

        public void visualParam(string type, int anzahl)
        {
            Bold bold = new Bold();
            string paramName = "";
            for (int i = 1; i <= anzahl; i++)
            {

                Run typRun = new Run(type);
                typRun.Foreground = Brushes.Blue;
               
                if(type.Equals("int"))
                {
                    paramName = "intparam";
                }
                if (type.Equals("string"))
                {
                    paramName = "stringparam";
                }
                if(type.Equals("double"))
                {
                    paramName = "doubleparam";
                }
                Run nameRun;
                if (i < anzahl)
                {
                    nameRun = new Run(" " + paramName + "" + i + ",");
                }
                else { nameRun = new Run(" " + paramName + "" + i); }
                bold.Inlines.Add(typRun);
                bold.Inlines.Add(nameRun);
            }
            switch (type)
            {
                case "int": 
                    intparams = bold;
                    this.refreshTextBlock(type);
                    break;
                case "string":
                    stringparams = bold;
                    this.refreshTextBlock(type);
                    break;
                case "double":
                    doubleparams = bold;
                    this.refreshTextBlock(type);
                    break;




            }
                
            
            
        }
        public void save(string fileName)
        {
            if (this.webService.ServiceDescription != null)
            {
                ServiceDescription test = this.webService.ServiceDescription;
                StreamWriter stream = new StreamWriter(fileName);
                test.Write(stream);
                stream.Close();
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            

        }
        public void showWsdl(string wsdl)
        {
            this.theWsdl = wsdl;
        }
        public void compile()
        {
            TextRange richTextBoxText = new TextRange(this.richTextBox1.Document.ContentStart, this.richTextBox1.Document.ContentEnd);

            TextRange endBrace = new TextRange(this.textBlock2.Inlines.FirstInline.ContentStart, this.textBlock2.Inlines.FirstInline.ContentEnd);
            string header = copyContentToString(this.textBlock1);
          
            string codeToCompile = header + richTextBoxText.Text + endBrace.Text;
            webService.Compile(codeToCompile);
            this.webService.WebServiceSettings.Compiled = true;
        }
        public string copyContentToString(TextBlock block)
        {
            string blockString = "";
            foreach (Inline inline in textBlock1.Inlines)
            {
                TextRange inlineRange = new TextRange(inline.ContentStart, inline.ContentEnd);
               blockString += inlineRange.Text;
            }
            return blockString;
        }
        private void compileButton_Click(object sender, RoutedEventArgs e)
        {
            this.textBox3.Clear();
            this.compile();
        }


        public StackPanel insertNamespace(ref StackPanel panel, string nspace)
        {
            if (!namespacesTable.ContainsValue(nspace))
            {
                namespacesTable.Add(nspace, nspace);
                TextBlock xmlns = new TextBlock();
                xmlns.Text = " xmlns=";
                SolidColorBrush valueBrush = new SolidColorBrush(Colors.Blue);
                TextBlock value = new TextBlock();
                value.Text = "\"" + nspace + "\"";
                value.Foreground = valueBrush;
                panel.Children.Add(xmlns);
                panel.Children.Add(value);
            }
            return panel;
        }

        //public StackPanel insertAttributes(ref StackPanel panel, XmlAttributeCollection attributes)
        //{
        //    foreach (XmlAttribute tempAttribute in attributes)
        //    {
        //        if (!tempAttribute.Name.Contains("xmlns"))
        //        {
        //            TextBlock name = new TextBlock();
        //            name.Text = " " + tempAttribute.Name;


        //            TextBlock value = new TextBlock();
        //            value.Text = " =\"" + tempAttribute.Value + "\"";
        //            SolidColorBrush valueBrush = new SolidColorBrush(Colors.Blue);
        //            value.Foreground = valueBrush;

        //            panel.Children.Insert(1, name);
        //            panel.Children.Insert(2, value);

        //        }
        //        else
        //        {
        //            if (!namespacesTable.ContainsValue(tempAttribute.Value))
        //            {
        //                namespacesTable.Add(tempAttribute.Value, tempAttribute.Value);
        //                TextBlock name = new TextBlock();
        //                name.Text = " " + tempAttribute.Name;


        //                TextBlock value = new TextBlock();
        //                value.Text = " =\"" + tempAttribute.Value + "\"";
        //                SolidColorBrush valueBrush = new SolidColorBrush(Colors.Blue);
        //                value.Foreground = valueBrush;

        //                panel.Children.Insert(1, name);
        //                panel.Children.Insert(2, value);
        //            }
        //        }
        //    }
        //    return panel;



        //}
      

        private void richTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {TextRange code = new TextRange(this.richTextBox1.Document.ContentStart,this.richTextBox1.Document.ContentEnd);
        if (webService != null)
        {
            this.webService.WebServiceSettings.UserCode = code.Text;
        }


          
        }


        public AnimationController getAnimationController()
        {
            return this.animationController;
        }


        

  

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.webService.CheckSignature()==true)
            {
               
            }
            else
            {
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
        
            this.findSignatureItems((TreeViewItem)this.soapInput.Items[0], "ds:Signature");
            if (this.signatureCollection.Count > 0)
            {
                initializeAnimation();
                dispatcherTimer.Start();
            }
            else {// calculateBox.Text = "There is no signature in the message"; 
            }
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            namespacesTable.Clear();
            this.signatureCollection.Clear();
            this.tempReferenceCollection.Clear();
            this.textBox2.Clear();
      
            dispatcherTimer.Stop();
            animationController.getControllerTimer().Stop();
            decryptionTimer.Stop();
            this.textBox2.Clear();

            this.initializeAnimation();
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.actualSignatureNumber = 1;
          //  status = (int)this.slider1.Value+1;
        }

        private void button1_Click_2(object sender, RoutedEventArgs e)
        {
           
            this.animationController.getSecurityTotalNumber();
            
            decAnimation.initializeAnimation();
            decAnimation.fillEncryptedDataTreeviewElements();

        }
        public DispatcherTimer getSignatureTimer()
        {
            return this.dispatcherTimer;
        }

        private void button1_Click_3(object sender, RoutedEventArgs e)
        {
            this.animationController.initializeAnimation();
            this.button1.IsEnabled = false;
          
        }
       
    }
}
