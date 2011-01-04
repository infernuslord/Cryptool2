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

namespace Wizard
{
    /// <summary>
    /// Interaction logic for WizardControl.xaml
    /// </summary>
    public partial class WizardControl : UserControl
    {

        private string culture = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private const string configXMLPath = "Wizard.Config.wizard.config.start.xml";
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
        }

        // Generate the full XML tree for the wizard 
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

                        GenerateXML(cat);

                    } 
                }

                wizardConfigXML = xml;

            }
            catch (Exception)
            {
                
            }
        }

        private void GetCategoryList()
        {

        }

    }
}
