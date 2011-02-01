using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Xml;
using System.Windows.Media.Animation;
using System.Threading;
using System.Collections;
using Cryptool.PluginBase.Miscellaneous;

namespace Soap
{
    /// <summary>
    /// Interaktionslogik für SoapPresentation.xaml
    /// </summary>
    public partial class SoapPresentation : UserControl
    {
        public DataSet ds;
        private string lastURI;
        private Soap soap;
        public TreeViewItem origSoapItem,securedSoapItem;
        public TreeViewItem item1 = null;
        public Hashtable namespacesTable;
        private int status,generateHeaderSteps,referencesCounter,referencesSteps;
        private DoubleAnimation TextSizeAnimation, TextSizeAnimationReverse, opacityAnimation, TextSizeAnimation1, TextSizeAnimationReverse1, opacityAnimation1;
        private SolidColorBrush elemBrush;
        private SignatureAnimator sigAnimator;
        private EncryptionAnimator encAnimator;
        public  bool animationRunning;


        public SoapPresentation(Soap soap)
        {
            InitializeComponent();
            elemBrush = new SolidColorBrush(Colors.MediumVioletRed);
           
            status = 0;
            this.soap = soap;
            animationRunning = false;
            namespacesTable = new Hashtable();
        }


        public void startstopanimation()
        {
            if (animationRunning)
            {
                if (sigAnimator != null)
                {
                    sigAnimator.startstopAnimation();
                }
                if (encAnimator != null)
                {
                    encAnimator.playpause();

                }
            }
            else
            {
                soap.CreateInfoMessage("No animation running");
            }
        }


        private TreeViewItem findItem(TreeViewItem item, string bezeichner)
        {
            StackPanel tempHeader1 = (StackPanel)item.Header;
            string Bezeichner = getNameFromPanel(tempHeader1,false);
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
                findItem(childItem, bezeichner);
            }
            if (item1 != null)
            {
                return item1;
            }
            return null;
        }

        public void CopyXmlToTreeView(XmlNode xNode, ref TreeViewItem tviParent)
        {
            namespacesTable.Clear();
            if (securedSoapItem != null)
            {
                clearBoxes(securedSoapItem);
            }
            CopyXmlToTreeViewReal(xNode, ref tviParent);
        }

     
        public void CopyXmlToTreeViewReal(XmlNode xNode, ref TreeViewItem tviParent)
        {
            SolidColorBrush elemBrush = new SolidColorBrush(Colors.MediumVioletRed);
                if(xNode!=null)
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
                   tbName.Text =  xNode.Name;
                   tbTagOpen.Foreground = elemBrush;
                   tbTagClose.Foreground = elemBrush;
                   tbName.Foreground = elemBrush;
                if(!xNode.NodeType.ToString().Equals("Text")){
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
                    if (!animationRunning)
                    {
                        XmlNode[] ElementsToEnc = soap.GetElementsToEncrypt();
                       
                        foreach (XmlNode node in ElementsToEnc)
                        {
                            if (node.Name.Equals(xNode.Name))
                            {
                                addOpenLockToPanel(ref panel, xNode.Name);
                            }
                        }
                        XmlNode[] EncryptedElements = soap.GetEncryptedElements();
                        foreach (XmlNode node in EncryptedElements)
                        {
                            if (node.Name.Equals(xNode.Name))
                            {
                                addClosedLockToPanel(ref panel);
                            }
                        }
                        XmlNode[] signedElements = soap.GetSignedElements();
                        foreach (XmlNode node in signedElements)
                        {
                            if (node.Name.Equals(xNode.Name))
                            {
                                string id ="";
                                foreach(XmlAttribute att in node.Attributes)
                                {
                                    if (att.Name.Equals("Id"))
                                    {
                                        id = att.Value;
                                    }
                                }
                                addSignedIconToPanel(ref panel, xNode.Name, id);
                            }
                        }
                        XmlNode[] elementsToSign = soap.GetElementsToSign();
                        foreach (XmlNode node in elementsToSign)
                        {
                            if (node.Name.Equals(xNode.Name))
                            {
                                addToSignIconToPanel(ref panel, xNode.Name);
                            }
                        }
                        XmlNode[] parameters = soap.GetParameterToEdit();
                        foreach (XmlNode node in parameters)
                        {
                            if (node.Name.Equals(xNode.Name))
                            {
                                addEditImageToPanel(ref panel, xNode.Name);
                            }
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
                            CopyXmlToTreeViewReal(child, ref item);
                        }
                    }
                    StackPanel panel1 = new StackPanel();
                    panel1.Orientation = System.Windows.Controls.Orientation.Horizontal;
                    TextBlock elem1Open = new TextBlock();
                    elem1Open.Text = "<";
                    panel1.Children.Insert(0, elem1Open);
                    TextBlock elem1Close = new TextBlock();
                    elem1Close.Text = ">";
                    TextBlock elem1Name = new TextBlock();
                    elem1Name.Text = "/"+xNode.Name;
                    panel1.Children.Add(elem1Name);
                    panel1.Children.Add(elem1Close);

                    closeitem.Header = panel1;
                 
                    tviParent.Items.Add(closeitem);
                }
                else
                {
                    item.Name = "OpenItemTextNode";
                    panel.Name = "OpenPanelTextNode";
                    TextBlock tbText = new TextBlock();
                    tbText.Name = "TextNode";
                    tbText.Text = xNode.Value;
                    panel.Children.Add(tbText);
                    item.Header = panel;
                    tviParent.Items.Add(item);
                }
                } 
        }

        private void addOpenLockToPanel(ref StackPanel panel,string name)
        {
            string[] splitter = name.Split(new char[] { ':' });
            if (splitter.Length > 1)
            {
                name = splitter[0] + "_" + splitter[1];
            }
            else
            {
                name = splitter[0];
            }
            System.Drawing.Bitmap bitmap = Resource1.OpenLock;
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(ms.ToArray());
            bi.EndInit();
            Image myImage2 = new Image();
            myImage2.Source = bi;
            myImage2.Name = name;
            int i = panel.Children.Count;
            myImage2.MouseLeftButtonDown += new MouseButtonEventHandler(myImage2_MouseLeftButtonDown);
            myImage2.ToolTip = "Click this picture to encrypt the <" + name + "> element";
            myImage2.MouseEnter += new MouseEventHandler(myImage2_MouseEnter);
            myImage2.MouseLeave += new MouseEventHandler(myImage2_MouseLeave);
            panel.Children.Add(myImage2);
        }

        void myImage2_MouseLeave(object sender, MouseEventArgs e)
        {
            Image img = (Image)sender;
            DoubleAnimation widhtAnimation = new DoubleAnimation(23 , 18, TimeSpan.FromSeconds(0.2));
            widhtAnimation.AutoReverse = false;
            DoubleAnimation heightAnimation = new DoubleAnimation(23, 18, TimeSpan.FromSeconds(0.2));
            heightAnimation.AutoReverse = false;
            img.BeginAnimation(Image.WidthProperty, widhtAnimation);
            img.BeginAnimation(Image.HeightProperty, heightAnimation);
        }

        void myImage2_MouseEnter(object sender, MouseEventArgs e)
        {
            Image img = (Image)sender;
            DoubleAnimation widhtAnimation = new DoubleAnimation(18, 23, TimeSpan.FromSeconds(0.2));
            widhtAnimation.AutoReverse = false;
            DoubleAnimation heightAnimation = new DoubleAnimation(18, 23, TimeSpan.FromSeconds(0.2));
            heightAnimation.AutoReverse = false;
            img.BeginAnimation(Image.WidthProperty, widhtAnimation);
            img.BeginAnimation(Image.HeightProperty, heightAnimation);
        }

        private void addClosedLockToPanel(ref StackPanel panel)
        {

            System.Drawing.Bitmap bitmap = Resource1.ClosedLock;
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(ms.ToArray());
            bi.EndInit();
            Image myImage2 = new Image();
            myImage2.Source = bi;
            myImage2.Name = "EncryptedData";
            int i = panel.Children.Count;
            myImage2.ToolTip = "This Element is encrypted";
            panel.Children.Add(myImage2);
            myImage2.MouseEnter += new MouseEventHandler(myImage2_MouseEnter);
            myImage2.MouseLeave += new MouseEventHandler(myImage2_MouseLeave);
        }

        private void addSignedIconToPanel(ref StackPanel panel, string name, string id)
        {
            string[] splitter = name.Split(new char[] { ':' });
            if (splitter.Length > 1)
            {
                name = splitter[0] + "_" + splitter[1];
            }
            else
            {
                name = splitter[0];
            }
            System.Drawing.Bitmap bitmap = Resource1.ClosedCert;
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(ms.ToArray());
            bi.EndInit();
            Image signedImage = new Image();
            signedImage.Source = bi;
            signedImage.Name = name + "_" + id;
            int i = panel.Children.Count;
            signedImage.ToolTip = "The Element: " + name + " " + id + "is signed";
            panel.Children.Add(signedImage);
            signedImage.MouseDown += new MouseButtonEventHandler(signedImage_MouseDown);
            signedImage.MouseEnter += new MouseEventHandler(myImage2_MouseEnter);
            signedImage.MouseLeave += new MouseEventHandler(myImage2_MouseLeave);
        }

        void signedImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!animationRunning)
            {
                clearBoxes(securedSoapItem);
                namespacesTable.Clear();
                Image signIcon = (Image)sender;
                string[] name = signIcon.Name.Split(new char[] { '_' });
                string id = name[2];
                soap.RemoveSignature(id);
            }
        }

        private void addToSignIconToPanel(ref StackPanel panel, string name)
        {
            string[] splitter = name.Split(new char[] { ':' });
            if (splitter.Length > 1)
            {
                name = splitter[0] + "_" + splitter[1];
            }
            else
            {
                name = splitter[0];
            }
            System.Drawing.Bitmap bitmap = Resource1.OpenCert;
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(ms.ToArray());
            bi.EndInit();
            Image signImage = new Image();
            signImage.Source = bi;
            signImage.Name = name;
            int i = panel.Children.Count;
            signImage.ToolTip = "Click here to sign the: " + name + " Element";
            panel.Children.Add(signImage);
            signImage.MouseDown += new MouseButtonEventHandler(signImage_MouseDown);
            signImage.MouseEnter += new MouseEventHandler(myImage2_MouseEnter);
            signImage.MouseLeave += new MouseEventHandler(myImage2_MouseLeave);
        }

        private void addEditImageToPanel(ref StackPanel panel, string name)
        {
            string[] splitter = name.Split(new char[] { ':' });
            if (splitter.Length > 1)
            {
                name = splitter[0] + "_" + splitter[1];
            }
            else
            {
                name = splitter[0];
            }
            System.Drawing.Bitmap bitmap = Resource1.EditIcon;
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(ms.ToArray());
            bi.EndInit();
            Image editImage = new Image();
            editImage.Source = bi;
            editImage.Name = name;
            int i = panel.Children.Count;
            editImage.ToolTip = "Click here or on the element name to edit the: " + name + " Element";
            panel.Children.Add(editImage);
            editImage.MouseEnter += new MouseEventHandler(myImage2_MouseEnter);
            editImage.MouseLeave += new MouseEventHandler(myImage2_MouseLeave);
        }

        void signImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!animationRunning)
            {

                if (soap.GetSignatureAlgorithm() == null)
                {
                    soap.CreateErrorMessage("You have to select a signature algorithm before you can sign parts of the message");
                }
                else
                {
                    namespacesTable.Clear();
                    Image signIcon = (Image)sender;
                    string[] test = signIcon.Name.Split(new char[] { '_' });
                    string name = test[0] + ":" + test[1];
                    XmlElement[] array = new XmlElement[1];
                    array[0] = (XmlElement)soap.SecuredSoap.GetElementsByTagName(name)[0];
                    if (!soap.GetXPathTransForm())
                    {
                        soap.AddIdToElement(name);
                    }
                    if (!soap.GetShowSteps())
                    {
                        if (!soap.CheckSecurityHeader())
                        {
                            soap.CreateSecurityHeaderAndSoapHeader();
                        }
                        soap.SignElementsManual(array);
                        soap.ShowSecuredSoap();
                    }
                    else
                    {
                        animationRunning = true;

                        sigAnimator = new SignatureAnimator(ref this.treeView, ref this.soap);
                        sigAnimator.startAnimation(array);
                        soap.CreateInfoMessage("Signature animation started");
                    }
                }
            }
        }

        public void endAnimation()
        {
            if (animationRunning)
            {
                if (sigAnimator != null)
                {
                    sigAnimator.endAnimation();
                }
                if (encAnimator != null)
                {
                    encAnimator.endAnimation();
                }
            }
            else
            {
                soap.CreateInfoMessage("No animation running");
            }
        }

        public void setAnimationSpeed(int s)
        {
            if (sigAnimator != null)
            {
                sigAnimator.setAnimationSpeed(s);
            }
            if(encAnimator != null)
            {
                encAnimator.setAnimationSpeed(s);
            }
        }

        void myImage2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            clearBoxes(securedSoapItem);
            if (!animationRunning)
            {
                namespacesTable.Clear();
                Image img = (Image)sender;
                string name = img.Name;
                string[] splitter = name.Split(new char[] { '_' });
                if (splitter.Length > 1)
                {
                    name = splitter[0] + ":" + splitter[1];
                }
                else
                {
                    name = splitter[0];
                }
                XmlElement[] array = new XmlElement[1];
                array[0] = (XmlElement)soap.SecuredSoap.GetElementsByTagName(name)[0];
                soap.AddIdToElement(name);
                
                if (soap.GotKey)
                {
                    soap.EncryptElements(array);
                    if (!soap.GetIsShowEncryptionsSteps())
                    {
                        soap.ShowSecuredSoap();
                    }
                    else
                    {
                        encAnimator = new EncryptionAnimator(ref this.treeView, ref  soap);
                        encAnimator.startAnimation(array);
                        animationRunning = true;
                    }
                }
                else
                {
                    soap.CreateErrorMessage("No key for encryption available. Create one in a Web Service Plugin");
                }
            }
        }

        public StackPanel insertNamespace(ref StackPanel panel, string nspace,string Prefix)
        {
            if (!namespacesTable.ContainsValue(nspace))
            {
                namespacesTable.Add(nspace, nspace);
                TextBlock xmlns = new TextBlock();
                xmlns.Name = "xmlns";
                xmlns.Text= " xmlns";
                TextBlock prefix = new TextBlock();
                prefix.Name="xmlnsPrefix";
                if (!Prefix.Equals(""))
                {prefix.Text = ":" + Prefix;}
                else { prefix.Text = "";}
                SolidColorBrush valueBrush = new SolidColorBrush(Colors.Blue);
                TextBlock value = new TextBlock();
                value.Name = "xmlnsValue";
                value.Text ="="+"\""+nspace+"\"";
                value.Foreground = valueBrush;
                panel.Children.Add(xmlns);
                panel.Children.Add(prefix);
                panel.Children.Add(value);
            }
            return panel;
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



        private void deleteItem(string text, TreeViewItem delItem)
        {
            if (delItem.HasItems)
            {
                int count = 0;
                foreach (TreeViewItem child in delItem.Items)
                {
                    StackPanel tempHeader1 = (StackPanel)child.Header;
                    TextBlock text1 = (TextBlock)tempHeader1.Children[0];
                    if (text1.Text.Equals(text))
                    {
                        delItem.Items.RemoveAt(count);
                        break;
                        if (child.HasItems)
                        {
                            deleteItem(text, child);
                        }
                    }
                    else 
                    {
                        if(child.HasItems)
                        {
                            deleteItem(text, child);
                        }
                    }
                    count++;
                }
            }
        }





        private void formatOrigTV(TreeViewItem item1)
        {
            StackPanel tempHeader = (StackPanel)item1.Header;
            TextBlock elem = new TextBlock();
            DoubleAnimation widthAnimation =
            new DoubleAnimation(16, 11, TimeSpan.FromSeconds(1));
            widthAnimation.AutoReverse = false;
            elem = (TextBlock) tempHeader.Children[0];
            if (elem.FontSize == 16.0)
            {
                item1.BeginAnimation(TreeViewItem.FontSizeProperty, widthAnimation);
            }
            item1.FontStyle = FontStyles.Normal;
            string s = item1.Header.ToString();
            if (item1.HasItems)
            {
                foreach(TreeViewItem child in item1.Items)
                {
                    formatOrigTV( child);
                }
            }
        }
        private bool ret;
        private void checkForItem(TreeViewItem item, string search)
        {
            StackPanel tempHeader = (StackPanel)item.Header;
            TextBlock elem = new TextBlock();
            elem = (TextBlock) tempHeader.Children[0];
            if (elem.Text.Equals(search))
            {
                ret = true;
            }
            else
            {
                foreach(TreeViewItem child in item.Items )
                {
                checkForItem(child,search);
                }
            }
        }

        private void setLbEnd()
        {
            lbSignSteps.SelectedIndex = 0;
        }

        /// <summary>
        /// Returns the Name of the Element without the prefix
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
           private string getNameFromPanel(StackPanel panel,bool prefix)
           {
               foreach (object obj in panel.Children)
               {
                   if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                   {
                       TextBlock tb = (TextBlock)obj;
                       if (tb.Name.Equals("tbName"))
                       {
                           
                           string name = tb.Text;
                           if (!prefix)
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

           private void treeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
           {
             
               if (soap.WSDLLoaded)
               {
                   clearBoxes(securedSoapItem);
                   TreeView tv = (TreeView)sender;
                   if (tv.SelectedItem != null)
                   {
                       TreeViewItem item = (TreeViewItem)tv.SelectedItem;
                       StackPanel tempPanel = (StackPanel)item.Header;
                       Object temp = tempPanel.Children[0];
                       string type = temp.GetType().ToString();

                       if (type.Equals("System.Windows.Controls.TextBlock"))
                       {
                           XmlNode[] parameter = soap.GetParameterToEdit();

                           string name = getNameFromPanel(tempPanel, true);

                           foreach (XmlNode node in parameter)
                           {
                               if (node.Name.Equals(name))
                               {
                                   string text = "";
                                   if (item.HasItems)
                                   {
                                       TreeViewItem childItem = (TreeViewItem)item.Items[0];
                                       StackPanel childPanel = (StackPanel)childItem.Header;
                                       text = getNameFromPanel(childPanel, false);
                                       item.Items.RemoveAt(0);
                                   }
                                   item.IsExpanded = true;
                                   TreeViewItem newItem = new TreeViewItem();
                                   item.Items.Add(newItem);
                                   newItem.IsExpanded = true;
                                   StackPanel panel = new StackPanel();
                                   TextBox box = new TextBox();
                                   box.Height = 23;
                                   box.Width = 80;
                                   box.Text = soap.SecuredSoap.GetElementsByTagName(name)[0].InnerXml.ToString(); ;
                                   box.IsEnabled = true;

                                   panel.Children.Add(box);
                                   newItem.Header = panel;
                                   box.IsKeyboardFocusedChanged += new DependencyPropertyChangedEventHandler(box_IsKeyboardFocusedChanged);
                                   box.KeyDown += new KeyEventHandler(box_KeyDown);
                                   StackPanel parentPanel = (StackPanel)item.Header;
                                   TextBlock parentBlock = (TextBlock)parentPanel.Children[0];
                                   name = getNameFromPanel(tempPanel, false);
                                   box.Name = name;
                               }
                           }
                       }
                   }
               }
           }

           void box_KeyDown(object sender, KeyEventArgs e)
           {
               if (e.Key == Key.Return)
               {
                   inputEnd(sender, true);  
               }
           }

           void box_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
           {
          
           }

           private void inputEnd(object sender, bool enter)
           {
               clearBoxes(securedSoapItem);
               soap.ShowSecuredSoap();
           }


           private void clearBoxes(TreeViewItem item)
           {
               if (item.HasItems)
               {
                   bool childIsTextBox = false;
                   string text ="";
                   foreach(TreeViewItem childItem in item.Items)
                   {
                       if (childItem.Header.GetType().ToString().Equals("System.Windows.Controls.StackPanel"))
                       {
                           StackPanel panel = (StackPanel)childItem.Header;
                          
                               foreach(Object obj in panel.Children)
                               {
                                   if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBox"))
                                   {
                                       TextBox box = (TextBox)obj;
                                       if (item.Header.GetType().ToString().Equals("System.Windows.Controls.StackPanel"))
                                       {
                                           StackPanel parentPanel = (StackPanel)item.Header;
                                           if (parentPanel.Children.Count > 2)
                                           {
                                               TextBlock block = (TextBlock)parentPanel.Children[1];
                                               soap.SecuredSoap.GetElementsByTagName(block.Text)[0].InnerText = box.Text;
                                               soap.SaveSoap();
                                               text = box.Text;
                                               childIsTextBox = true;
                                           }
                                       }

                                   }
                               }
                       }
                   }
                   if (childIsTextBox)
                   {
                       item.Items.RemoveAt(0);
                       if(!text.Equals(""))
                       {
                       TreeViewItem newItem = new TreeViewItem();
                       StackPanel newPanel = new StackPanel();
                       TextBlock block = new TextBlock();
                       block.Text = text;
                       newPanel.Children.Add(block);
                       newItem.Header = newPanel;
                       item.Items.Add(newItem);
                       }
                   }
               }
             
                
               foreach (TreeViewItem childItem in item.Items)
               {
                   clearBoxes( childItem);
               }
              
           }

           public void addTextToInformationBox(string text)
           {
               lbSignSteps.Items.Add(text);
               lbSignSteps.ScrollIntoView(text);
           }

           private void expander1_Expanded(object sender, RoutedEventArgs e)
           {
           }

           private void image1_ImageFailed(object sender, ExceptionRoutedEventArgs e)
           {
           }
      
    }
}
