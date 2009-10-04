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

namespace ManInTheMiddle
{
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class MitmPresentation : UserControl
    {
        private TreeViewItem item1;
        public TreeViewItem soapItem;
        private string lastURI;
        private Hashtable namespacesTable;
        private ManInTHeMiddle mitm;

        public MitmPresentation(ManInTHeMiddle mitm)
        {
            InitializeComponent();
            item1 = new TreeViewItem();
            lastURI = "";
            namespacesTable = new Hashtable();
            this.mitm = mitm;
        }
        /// <summary>
        /// Returns the Name of the Element without the prefix
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
        private string getNameFromPanel(StackPanel panel, bool prefix)
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
        public void CopyXmlToTreeView(XmlNode xNode, ref TreeViewItem tviParent, XmlNode[] parameter)
        {
            namespacesTable.Clear();
           
            CopyXmlToTreeViewReal(xNode, ref tviParent,parameter);
        }

        public void CopyXmlToTreeViewReal(XmlNode xNode, ref TreeViewItem tviParent, XmlNode[] parameter)
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
                    item.Header = panel;
                    closeitem.Foreground = elemBrush;
                    tviParent.Items.Add(item);
                    foreach (XmlNode node in parameter)
                    {
                        if (node.Name.Equals(xNode.Name))
                        {
                            addEditImageToPanel(ref panel, xNode.Name);
                        }
                    }
                    if (xNode.HasChildNodes)
                    {
                        foreach (XmlNode child in xNode.ChildNodes)
                        {
                            lastURI = xNode.NamespaceURI; ;
                            CopyXmlToTreeViewReal(child, ref item, parameter);
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
                    elem1Name.Text = "/" + xNode.Name;
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
        void myImage2_MouseLeave(object sender, MouseEventArgs e)
        {
            Image img = (Image)sender;
            DoubleAnimation widhtAnimation = new DoubleAnimation(23, 18, TimeSpan.FromSeconds(0.2));
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



        private TreeViewItem findItem(TreeViewItem item, string bezeichner)
        {
            StackPanel tempHeader1 = (StackPanel)item.Header;
            string Bezeichner = getNameFromPanel(tempHeader1, false);
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void clearBoxes(TreeViewItem item)
        {


            if (item.HasItems)
            {
                bool childIsTextBox = false;
                string text = "";
                foreach (TreeViewItem childItem in item.Items)
                {
                    if (childItem.Header.GetType().ToString().Equals("System.Windows.Controls.StackPanel"))
                    {
                        StackPanel panel = (StackPanel)childItem.Header;

                        foreach (Object obj in panel.Children)
                        {
                            if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBox"))
                            {
                                TextBox box = (TextBox)obj;
                                //  soap.securedSOAP.GetElementsByTagName("")[0];

                                if (item.Header.GetType().ToString().Equals("System.Windows.Controls.StackPanel"))
                                {
                                    StackPanel parentPanel = (StackPanel)item.Header;
                                    if (parentPanel.Children.Count > 2)
                                    {
                                        TextBlock block = (TextBlock)parentPanel.Children[1];
                                        mitm.soap.GetElementsByTagName(block.Text)[0].InnerText = box.Text;
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
                    if (!text.Equals(""))
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
                clearBoxes(childItem);
            }

        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            clearBoxes(this.soapItem);
            TreeView tv = (TreeView)sender;
            if (tv.SelectedItem != null)
            {
                TreeViewItem item = (TreeViewItem)tv.SelectedItem;
                StackPanel tempPanel = (StackPanel)item.Header;
                Object temp = tempPanel.Children[0];
                string type = temp.GetType().ToString();

                if (type.Equals("System.Windows.Controls.TextBlock"))
                {
                    XmlNode[] parameter = mitm.getParameter();

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
                            box.Text = mitm.soap.GetElementsByTagName(name)[0].InnerXml.ToString(); ;
                            box.IsEnabled = true;

                            panel.Children.Add(box);
                            newItem.Header = panel;
                            
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
        void box_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                clearBoxes(soapItem);
            }
        }


    }
}
