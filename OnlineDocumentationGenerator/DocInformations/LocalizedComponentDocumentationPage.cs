using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Cryptool.PluginBase;

namespace OnlineDocumentationGenerator.DocInformations
{
    public class LocalizedComponentDocumentationPage : LocalizedEntityDocumentationPage
    {
        private readonly ComponentTemplateList _templates = new ComponentTemplateList();

        public ComponentTemplateList Templates
        {
            get { return _templates; }
        }

        public PropertyInfoAttribute[] Connectors {
            get { return DocumentationPage.Connectors; }
        }

        public new ComponentDocumentationPage DocumentationPage
        {
            get
            {
                return (ComponentDocumentationPage)base.DocumentationPage;
            }
        }

        public XElement Introduction { get; private set; }
        public XElement Manual { get; private set; }
        public XElement Presentation { get; private set; }

        public LocalizedComponentDocumentationPage(ComponentDocumentationPage componentDocumentationPage, Type entityType, XElement xml, string lang, BitmapFrame icon)
            : base(componentDocumentationPage, entityType, xml, lang, icon)
        {
            var name = Type.Name;
            if (DocGenerator.RelevantComponentToTemplatesMap.ContainsKey(name))
            {
                var templates = DocGenerator.RelevantComponentToTemplatesMap[name];
                foreach (var template in templates)
                {
                    string templateXMLFile = Path.Combine(DocGenerator.TemplateDirectory, template.Substring(0, template.Length - 4) + ".xml");
                    if (File.Exists(templateXMLFile))
                    {
                        XElement templateXml = XElement.Load(templateXMLFile);
                        var description = FindLocalizedChildElement(templateXml, "description");
                        if (description != null)
                        {
                            Templates.Add(template, description);
                        }
                    }
                }
            }

            if (_xml != null)
                ReadInformationsFromXML();
        }

        private void ReadInformationsFromXML()
        {
            Introduction = FindLocalizedChildElement(_xml, "introduction");
            Manual = FindLocalizedChildElement(_xml, "manual");
            Presentation = FindLocalizedChildElement(_xml, "presentation");
        }
    }
}
