using System;
using System.Globalization;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Cryptool.PluginBase;

namespace OnlineDocumentationGenerator.DocInformations.Localization
{
    public abstract class LocalizedPluginDocumentationPage : LocalizedEntityDocumentationPage
    {
        protected XElement _xml;

        public new PluginDocumentationPage DocumentationPage { get { return base.DocumentationPage as PluginDocumentationPage; }}

        public override string FilePath
        {
            get { return OnlineHelp.GetDocFilename(PluginType, Lang); }
        }

        public Type PluginType { get; private set; }
        public string ToolTip { get; private set; }

        public TaskPaneAttribute[] Settings
        {
            get { return DocumentationPage.Settings; }
        }

        public string AuthorURL
        { 
            get { return DocumentationPage.AuthorURL; }
        }

        public string AuthorInstitute
        {
            get { return DocumentationPage.AuthorInstitute; }
        }

        public string AuthorEmail
        {
            get { return DocumentationPage.AuthorEmail; }
        }

        public string AuthorName
        {
            get { return DocumentationPage.AuthorName; }
        }

        protected LocalizedPluginDocumentationPage(PluginDocumentationPage editorDocumentationPage, Type pluginType, XElement xml, string lang, BitmapFrame icon)
        {
            base.DocumentationPage = editorDocumentationPage;
            PluginType = pluginType;
            _xml = xml;
            Lang = lang;
            Icon = icon;

            var cultureInfo = new CultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            Name = pluginType.GetPluginInfoAttribute().Caption;
            ToolTip = pluginType.GetPluginInfoAttribute().ToolTip;
        }
    }
}