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

namespace Wizard
{
    /// <summary>
    /// Interaction logic for WizardControl.xaml
    /// </summary>
    public partial class WizardControl : UserControl
    {

        private const string configXMLPath = "Wizard.Config.wizard.config.start.xml";
        private const string defaultLang = "en";
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
                                    string path = "Wizard.Config." + att.Value;
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

            string currentLang = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

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
                categories.Union(items);

            if (categories.Any())
            {
                foreach (XElement ele in categories)
                {
                    Label l = new Label();
                    l.Height = 30;
                    XElement label = FindElementInElement(ele, "name");
                    if (label != null)
                        l.Content = label.Value;

                    //Image i = new Image(); TODO

                    StackPanel sp = new StackPanel();
                    sp.VerticalAlignment = VerticalAlignment.Stretch;
                    sp.HorizontalAlignment = HorizontalAlignment.Stretch;
                    sp.Orientation = Orientation.Horizontal;
                    //add image
                    sp.Children.Add(l);

                    RadioButton rb = new RadioButton();
                    rb.HorizontalAlignment = HorizontalAlignment.Stretch;
                    rb.HorizontalContentAlignment = HorizontalAlignment.Left;
                    rb.Checked += rb_Checked;
                    rb.Content = sp;
                    rb.Tag = ele;

                    radioButtonStackPanel.Children.Add(rb);
                }
            }

        }

        void rb_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton b = (RadioButton)sender;
            XElement ele = (XElement)b.Tag;
            XElement desc = FindElementInElement(ele, "description");
            if (desc != null)
                description.Text = desc.Value;
            nextButton.IsEnabled = true;
        }

        //finds elements according to the current language
        private XElement FindElementInElement(XElement element, string xname)
        {
            string currentLang = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
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
            UIElement[] uiea = new UIElement[radioButtonStackPanel.Children.Count];
            radioButtonStackPanel.Children.CopyTo(uiea, 0);
            foreach (UIElement uie in uiea)
            {
                RadioButton b = (RadioButton)uie;
                if (b.IsChecked != null && (bool)b.IsChecked)
                {
                    radioButtonStackPanel.Children.Clear();
                    description.Text = "";
                    SetupPage((XElement)b.Tag);
                    break;
                }
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
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
            radioButtonStackPanel.Children.Clear();
            description.Text = "";
            SetupPage(wizardConfigXML);
        }

    }
}
