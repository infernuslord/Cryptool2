using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Cryptool.PluginBase;
using OnlineDocumentationGenerator.DocInformations.Utils;

namespace OnlineDocumentationGenerator.DocInformations.Localization
{
    public class LocalizedTemplateDocumentationPage : LocalizedEntityDocumentationPage
    {
        private XElement _xml;
        private string _filePath;

        public override string FilePath
        {
            get { return _filePath; }
        }

        public string Author { get; private set; }
        public XElement Summary { get; private set; }
        public XElement Description { get; private set; }

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

        public LocalizedTemplateDocumentationPage(TemplateDocumentationPage templateDocumentationPage, string lang, BitmapFrame icon, string author)
        {
            Author = author;
            DocumentationPage = templateDocumentationPage;
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

            var authorElement = XMLHelper.FindLocalizedChildElement(_xml, "author");
            if (authorElement != null)
            {
                Author = authorElement.Value;
            }

            Summary = XMLHelper.FindLocalizedChildElement(_xml, "summary");
            Description = XMLHelper.FindLocalizedChildElement(_xml, "description");
        }
    }
}