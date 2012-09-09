using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Cryptool.PluginBase;
using OnlineDocumentationGenerator.DocInformations.Utils;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;

namespace OnlineDocumentationGenerator.DocInformations.Localization
{
    public class LocalizedTemplateDocumentationPage : LocalizedEntityDocumentationPage
    {
        private XElement _xml;
        private string _filePath;

        public new TemplateDocumentationPage DocumentationPage { get { return base.DocumentationPage as TemplateDocumentationPage; } }

        public override string FilePath
        {
            get { return _filePath; }
        }

        public XElement Summary { get; private set; }
        public XElement Description { get; private set; }

        public string AuthorName
        {
            get { return DocumentationPage.AuthorName; }
        }

        public XElement SummaryOrDescription
        {
            get
            {
                if (Summary != null)
                    return Summary;
                if (Description != null)
                    return Description;
                return null;
            }
        }

        public LocalizedTemplateDocumentationPage(TemplateDocumentationPage templateDocumentationPage, string lang, BitmapFrame icon)
        {
            base.DocumentationPage = templateDocumentationPage;
            Lang = lang;
            Icon = icon;
            _xml = templateDocumentationPage.TemplateXML;
            _filePath = OnlineHelp.GetTemplateDocFilename(Path.Combine(templateDocumentationPage.RelativeTemplateDirectory, Path.GetFileName(templateDocumentationPage.TemplateFile)), lang);

            var cultureInfo = new CultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            var titleElement = XMLHelper.FindLocalizedChildElement(_xml, "title");
            if (titleElement != null)
            {
                Name = titleElement.Value;
            }
            
            Summary = XMLHelper.FindLocalizedChildElement(_xml, "summary");
            Description = XMLHelper.FindLocalizedChildElement(_xml, "description");
        }
    }
}