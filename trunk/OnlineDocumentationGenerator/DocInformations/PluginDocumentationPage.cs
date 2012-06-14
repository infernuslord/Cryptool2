using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Cryptool.PluginBase;
using OnlineDocumentationGenerator.DocInformations.Localization;

namespace OnlineDocumentationGenerator.DocInformations
{
    public abstract class PluginDocumentationPage : EntityDocumentationPage
    {
        protected XElement _xml;
        public Type PluginType { get; protected set; }
        public string AuthorURL { get; protected set; }
        public string AuthorInstitute { get; protected set; }
        public string AuthorEmail { get; protected set; }
        public TaskPaneAttribute[] Settings { get; protected set; }
        public ComponentCategory Category { get; protected set; }
        public Reference.ReferenceList References { get; protected set; }

        public override string Name
        {
            get { return PluginType.FullName; }
        }

        public override string DocPath
        {
            get { return Path.GetDirectoryName(OnlineHelp.GetDocFilename(PluginType, "en")); }
        }

        public PluginDocumentationPage(Type pluginType)
        {
            PluginType = pluginType;
            var image = pluginType.GetImage(0).Source;
            _xml = GetXML(pluginType);

            var authorAttribut = pluginType.GetPluginAuthorAttribute();
            if (authorAttribut != null)
            {
                AuthorName = authorAttribut.Author;
                AuthorEmail = authorAttribut.Email;
                AuthorInstitute = authorAttribut.Institute;
                AuthorURL = authorAttribut.URL;
            }

            if (pluginType.GetComponentCategoryAttributes().Count() > 0)
            {
                Category = pluginType.GetComponentCategoryAttributes().First().Category;
            }
            else
            {
                Category = ComponentCategory.Undefined;
            }
            Settings = GetSettings(pluginType);

            if (_xml == null || _xml.Name != "documentation")
            {
                //entity doesn't have a proper _xml file
                _xml = null;
                Localizations.Add("en", CreateLocalizedEntityDocumentationPage(this, pluginType, null, "en", image as BitmapFrame));
            }
            else
            {
                foreach (var lang in GetAvailableLanguagesFromXML())
                {
                    Localizations.Add(lang, CreateLocalizedEntityDocumentationPage(this, pluginType, _xml, lang, image as BitmapFrame));
                }
                if (!Localizations.ContainsKey("en"))
                    throw new Exception("Documentation should at least support english language!");

                ReadReferences();
            }
        }

        //Factory Method:
        protected abstract LocalizedPluginDocumentationPage CreateLocalizedEntityDocumentationPage(PluginDocumentationPage pluginDocumentationPage, Type entityType, XElement xml, string lang, BitmapFrame entityImage);

        private TaskPaneAttribute[] GetSettings(Type entityType)
        {
            //Try to find out the settings class type of this entity type:
            var members = entityType.GetMembers(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var memberInfo in members)
            {
                if (memberInfo.MemberType == MemberTypes.Field)
                {
                    var t = ((FieldInfo)memberInfo).FieldType;
                    if (t.GetInterfaces().Contains(typeof(ISettings)))
                    {
                        return t.GetSettingsProperties(entityType);
                    }
                }
            }
            return null;
        }

        public List<string> AvailableLanguages
        {
            get { return Localizations.Keys.ToList(); }
        }

        protected void ReadReferences()
        {
            if (_xml.Element("references") != null)
            {
                References = new Reference.ReferenceList();

                foreach (var refs in _xml.Element("references").Elements())
                {
                    switch (refs.Name.ToString())
                    {
                        case "linkReference":
                            References.Add(new Reference.LinkReference(refs));
                            break;
                        case "bookReference":
                            References.Add(new Reference.BookReference(refs));
                            break;
                    }
                }
            }
        }

        protected IEnumerable<string> GetAvailableLanguagesFromXML()
        {
            var langs = _xml.Elements("language").Select(langElement => langElement.Attribute("culture").Value);
            return langs.Select(lang => new CultureInfo(lang)).Select(cult => cult.TwoLetterISOLanguageName);
        }

        protected static XElement GetXML(Type type)
        {
            var descriptionUrl = type.GetPluginInfoAttribute().DescriptionUrl;
            if (descriptionUrl == null || Path.GetExtension(descriptionUrl).ToLower() != ".xml")
            {                    
                return null;
            }

            if (descriptionUrl != string.Empty)
            {
                int sIndex = descriptionUrl.IndexOf('/');
                var xmlUri = new Uri(string.Format("pack://application:,,,/{0};component/{1}",
                                                    descriptionUrl.Substring(0, sIndex), descriptionUrl.Substring(sIndex + 1)));
                var stream = Application.GetResourceStream(xmlUri).Stream;
                return XElement.Load(stream);
            }
            return null;
        }
    }
}