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
using System.Xml.Linq;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using System.Threading;
using System.Collections;

namespace Wizard
{
    /// <summary>
    /// Interaction logic for WizardControl.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Wizard.Properties.Resources")]
    public partial class WizardControl : UserControl
    {

        private List<string> choicePath = new List<string>();
        private SolidColorBrush selectionBrush = new SolidColorBrush();
        private const string configXMLPath = "Wizard.Config.wizard.config.start.xml";
        private const string defaultLang = "en-US";
        private XElement wizardConfigXML;

        public WizardControl()
        {
            try
            {
                // DEBUG HELP string[] names = this.GetType().Assembly.GetManifestResourceNames();

                Stream fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(configXMLPath);
                XElement xml = XElement.Load(fileStream);
                GenerateXML(xml);
            }
            catch (Exception)
            {
                
            }

            InitializeComponent();

            selectionBrush.Color = Color.FromArgb(255, 200, 220, 245);
            descScroll.Background = selectionBrush;
            SetupPage(wizardConfigXML);
        }

        // generate the full XML tree for the wizard (recursive)
        private void GenerateXML(XElement xml)
        {
            try
            {
                //find all nested subcategories and add them to the tree
                IEnumerable<XElement> categories = xml.Elements("category");
                if (categories.Any())
                {
                    foreach (XElement cat in categories)
                    {
                        IEnumerable<XElement> files = cat.Elements("file");
                        if (files.Any())
                        {
                            foreach (XElement element in files)
                            {
                                XAttribute att = element.Attribute("resource");
                                if (att != null)
                                {
                                    string path = att.Value;
                                    Stream fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
                                    XElement sub = XElement.Load(fileStream);
                                    GenerateXML(sub);
                                    IEnumerable<XElement> elems = sub.Elements();
                                    if (elems.Any())
                                    {
                                        foreach (XElement ele in elems)
                                        {
                                            cat.Add(ele);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                wizardConfigXML = xml;

            }
            catch (Exception)
            {
                
            }
        }

        private void SetupPage(XElement element)
        {
            XElement parent = element.Parent;
            if (parent == null)
            {
                backButton.IsEnabled = false;
                abortButton.IsEnabled = false;
            }
            else
            {
                backButton.IsEnabled = true;
                abortButton.IsEnabled = true;
            }

            nextButton.IsEnabled = false;

            //set headline
            XElement headline = FindElementInElement(element, "headline");
            if (headline != null)
                taskHeader.Content = headline.Value;

            //set description label
            XElement desc = FindElementInElement(element, "desc");
            if (desc != null)
                descHeader.Content = desc.Value;

            //generate radio buttons
            IEnumerable<XElement> categories = element.Elements("category");
            IEnumerable<XElement> items = element.Elements("item");

            if (items.Any())
                categories = categories.Union(items);

            if (categories.Any())
            {
                foreach (XElement ele in categories)
                {
                    ContentControl c = new ContentControl();
                    Label l = new Label();
                    Image i = new Image();
                    StackPanel sp = new StackPanel();
                    c.Content = sp;
                    c.VerticalAlignment = VerticalAlignment.Stretch;

                    l.Height = 30;
                    l.HorizontalAlignment = HorizontalAlignment.Stretch;
                    XElement label = FindElementInElement(ele, "name");
                    if (label != null)
                        l.Content = label.Value;

                    i.Width = 26;
                    string image = ele.Attribute("image").Value;
                    if (image != null)
                    {
                        ImageSource ims = (ImageSource)TryFindResource(image);
                        if (ims != null)
                        {
                            i.Source = ims;
                            sp.Children.Add(i);
                        }
                    }

                    sp.VerticalAlignment = VerticalAlignment.Stretch;
                    sp.HorizontalAlignment = HorizontalAlignment.Stretch;
                    sp.Orientation = Orientation.Horizontal;
                    sp.Children.Add(l);

                    RadioButton rb = new RadioButton();
                    string id = ele.Attribute("id").Value;
                    if (id != null)
                        rb.Name = id;
                    rb.Checked += rb_Checked;
                    rb.HorizontalAlignment = HorizontalAlignment.Stretch;
                    rb.VerticalAlignment = VerticalAlignment.Stretch;
                    rb.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    rb.Content = c;
                    rb.Tag = ele;

                    radioButtonStackPanel.Children.Add(rb);
                    if (choicePath.Count > 0 && id == choicePath.Last())
                    {
                        choicePath.RemoveAt(choicePath.IndexOf(choicePath.Last()));
                        rb.IsChecked = true;
                        nextButton.IsEnabled = true;
                    }
                }
            }

        }

        void rb_Checked(object sender, RoutedEventArgs e)
        {
            ResetBackground();
            RadioButton b = (RadioButton)sender;
            ContentControl c = (ContentControl)b.Content;
            StackPanel sp = (StackPanel)c.Content;
            sp.Background = selectionBrush;
            XElement ele = (XElement)b.Tag;
            XElement desc = FindElementInElement(ele, "description");
            if (desc != null)
                description.Text = desc.Value;
            nextButton.IsEnabled = true;
        }

        private void ResetBackground()
        {
            UIElement[] uiea = new UIElement[radioButtonStackPanel.Children.Count];
            radioButtonStackPanel.Children.CopyTo(uiea, 0);
            foreach (UIElement uie in uiea)
            {
                RadioButton b = (RadioButton)uie;
                ContentControl c = (ContentControl)b.Content;
                StackPanel sp = (StackPanel)c.Content;
                sp.Background = Brushes.Transparent;
            }
        }

        //finds elements according to the current language
        private XElement FindElementInElement(XElement element, string xname)
        {
            string currentLang = System.Globalization.CultureInfo.CurrentCulture.Name;
            XElement foundElement = null;

            IEnumerable<XElement> descriptions = element.Elements(xname);
            if (descriptions.Any())
            {
                var description = from descln in descriptions where descln.Attribute("lang").Value == currentLang select descln;
                if (!description.Any())
                {
                    description = from descln in descriptions where descln.Attribute("lang").Value == defaultLang select descln;
                    if (description.Any())
                        foundElement = description.First();
                }
                else
                    foundElement = description.First();
            }

            return foundElement;
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            ResetBackground();
            UIElement[] uiea = new UIElement[radioButtonStackPanel.Children.Count];
            radioButtonStackPanel.Children.CopyTo(uiea, 0);
            foreach (UIElement uie in uiea)
            {
                RadioButton b = (RadioButton)uie;
                if (b.IsChecked != null && (bool)b.IsChecked)
                {
                    choicePath.Add(b.Name);
                    radioButtonStackPanel.Children.Clear();
                    description.Text = "";
                    SetupPage((XElement)b.Tag);
                    break;
                }
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            ResetBackground();
            UIElement[] uiea = new UIElement[radioButtonStackPanel.Children.Count];
            radioButtonStackPanel.Children.CopyTo(uiea, 0);
            if (uiea.Length > 0)
            {
                RadioButton b = (RadioButton)uiea[0];
                XElement ele = (XElement)b.Tag;
                radioButtonStackPanel.Children.Clear();
                description.Text = "";
                XElement grandParent = ele.Parent.Parent;
                if (grandParent != null)
                    SetupPage(grandParent);
                else
                    SetupPage(wizardConfigXML); 
            }
        }

        private void abortButton_Click(object sender, RoutedEventArgs e)
        {
            ResetBackground();
            radioButtonStackPanel.Children.Clear();
            choicePath.Clear();
            description.Text = "";
            SetupPage(wizardConfigXML);
        }

    }
}
