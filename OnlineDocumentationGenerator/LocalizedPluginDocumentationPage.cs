using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Cryptool.PluginBase;

namespace OnlineDocumentationGenerator
{
    public class LocalizedPluginDocumentationPage
    {
        private readonly XElement _xml;

        public Type PluginType { get; private set; }

        public bool Startable { get; private set; }
        public string ToolTip { get; private set; }
        public string Name { get; private set; }
        public string Lang { get; private set; }

        public string Description { get; private set; }

        public string AuthorURL { get; private set; }
        public string AuthorInstitute { get; private set; }
        public string AuthorEmail { get; private set; }
        public string AuthorName { get; private set; }

        public PropertyInfoAttribute[] PluginConnectors { get; private set; }
        public TaskPaneAttribute[] Settings { get; private set; }
        
        public LocalizedPluginDocumentationPage(Type pluginType, XElement xml, string lang)
        {
            PluginType = pluginType;
            _xml = xml;
            Lang = lang;

            var cultureInfo = new CultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            Startable = pluginType.GetPluginInfoAttribute().Startable;
            Name = pluginType.GetPluginInfoAttribute().Caption;
            ToolTip = pluginType.GetPluginInfoAttribute().ToolTip;

            var plugin = pluginType.CreateObject();
            PluginConnectors = plugin.GetProperties();
            Settings = plugin.Settings.GetSettingsProperties(plugin);

            AuthorName = pluginType.GetPluginAuthorAttribute().Author;
            AuthorEmail = pluginType.GetPluginAuthorAttribute().Email;
            AuthorInstitute = pluginType.GetPluginAuthorAttribute().Institute;
            AuthorURL = pluginType.GetPluginAuthorAttribute().URL;

            ReadInformationsFromXML();
        }

        private void ReadInformationsFromXML()
        {
            var descriptionElement = FindLocalizedChildElement(_xml, "description");
            Description = descriptionElement.Value;
        }

        //finds elements according to the current language:
        private static XElement FindLocalizedChildElement(XElement element, string xname)
        {
            const string defaultLang = "en";
            CultureInfo currentLang = System.Globalization.CultureInfo.CurrentCulture;

            IEnumerable<XElement> allElements = element.Elements(xname);
            IEnumerable<XElement> foundElements = null;

            if (allElements.Any())
            {
                foundElements = from descln in allElements where descln.Attribute("lang").Value == currentLang.TextInfo.CultureName select descln;
                if (!foundElements.Any())
                {
                    foundElements = from descln in allElements where descln.Attribute("lang").Value == currentLang.TwoLetterISOLanguageName select descln;
                    if (!foundElements.Any())
                        foundElements = from descln in allElements where descln.Attribute("lang").Value == defaultLang select descln;
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
