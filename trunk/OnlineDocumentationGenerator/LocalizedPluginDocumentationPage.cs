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
        private readonly Type _pluginType;
        private readonly XElement _xml;
        private readonly string _lang;
        private bool _startable;
        private readonly string _name;
        private readonly string _toolTip;

        public Type PluginType
        {
            get { return _pluginType; }
        }

        public string ToolTip
        {
            get { return _toolTip; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Lang
        {
            get { return _lang; }
        }

        public string Description
        {
            get; private set;
        }
        
        public LocalizedPluginDocumentationPage(Type pluginType, XElement xml, string lang)
        {
            _pluginType = pluginType;
            _xml = xml;
            _lang = lang;

            CultureInfo cultureInfo = new CultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            _startable = pluginType.GetPluginInfoAttribute().Startable;
            _name = pluginType.GetPluginInfoAttribute().Caption;
            _toolTip = pluginType.GetPluginInfoAttribute().ToolTip;

            ReadInformationsFromXML();
        }

        private void ReadInformationsFromXML()
        {
            var descriptionElement = FindLocalizedChildElement(_xml, "description");
            Description = descriptionElement.Value;
        }

        //finds elements according to the current language
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
