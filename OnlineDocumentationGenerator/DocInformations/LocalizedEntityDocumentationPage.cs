using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Cryptool.PluginBase;

namespace OnlineDocumentationGenerator.DocInformations
{
    public abstract class LocalizedEntityDocumentationPage
    {
        protected XElement _xml;
        public EntityDocumentationPage DocumentationPage { get; private set; }

        public Reference.ReferenceList References
        {
            get { return DocumentationPage.References; }
        }

        public Type Type { get; private set; }
        public string ToolTip { get; private set; }
        public string Name { get; private set; }
        public string Lang { get; private set; }

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

        public BitmapFrame Icon
        {
            get;
            private set;
        }

        protected LocalizedEntityDocumentationPage(EntityDocumentationPage editorDocumentationPage, Type entityType, XElement xml, string lang, BitmapFrame icon)
        {
            DocumentationPage = editorDocumentationPage;
            Type = entityType;
            _xml = xml;
            Lang = lang;
            Icon = icon;

            var cultureInfo = new CultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            Name = entityType.GetPluginInfoAttribute().Caption;
            ToolTip = entityType.GetPluginInfoAttribute().ToolTip;
        }

        //finds elements according to the current language:
        protected static XElement FindLocalizedChildElement(XElement element, string xname)
        {
            const string defaultLang = "en";
            CultureInfo currentLang = Thread.CurrentThread.CurrentUICulture;

            IEnumerable<XElement> allElements = element.Elements(xname);
            IEnumerable<XElement> foundElements = null;

            if (allElements.Any())
            {
                foundElements = from descln in allElements 
                                where (new CultureInfo(descln.Attribute("lang").Value)).TwoLetterISOLanguageName == currentLang.TwoLetterISOLanguageName 
                                select descln;
                if (!foundElements.Any())
                {
                    foundElements = from descln in allElements 
                                    where (new CultureInfo(descln.Attribute("lang").Value)).TwoLetterISOLanguageName == defaultLang 
                                    select descln;
                }
            }

            if (foundElements == null || !foundElements.Any() || !allElements.Any())
            {
                if (!allElements.Any())
                {
                    return null;
                }
                else
                    return allElements.First();
            }

            return foundElements.First();
        }
    }
}