using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Cryptool.PluginBase;

namespace OnlineDocumentationGenerator
{
    public class PluginDocumentationPage
    {
        public Type PluginType { get; private set; }
        private readonly XElement _xml;

        public string AuthorURL { get; private set; }
        public string AuthorInstitute { get; private set; }
        public string AuthorEmail { get; private set; }
        public string AuthorName { get; private set; }

        public PropertyInfoAttribute[] PluginConnectors { get; private set; }
        public TaskPaneAttribute[] Settings { get; private set; }

        public Dictionary<string, LocalizedPluginDocumentationPage> Localizations { get; private set; }

        public List<string> AvailableLanguages
        {
            get { return Localizations.Keys.ToList(); }
        }

        public PluginDocumentationPage(Type pluginType)
        {
            PluginType = pluginType;
            var pluginImage = pluginType.GetImage(0).Source;
            Localizations = new Dictionary<string, LocalizedPluginDocumentationPage>();
            _xml = GetPluginXML(pluginType);

            AuthorName = pluginType.GetPluginAuthorAttribute().Author;
            AuthorEmail = pluginType.GetPluginAuthorAttribute().Email;
            AuthorInstitute = pluginType.GetPluginAuthorAttribute().Institute;
            AuthorURL = pluginType.GetPluginAuthorAttribute().URL;

            PluginConnectors = PluginExtension.GetProperties(pluginType);
            //Try to find out the settings class type of this plugin:
            var members = pluginType.GetMembers(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var memberInfo in members)
            {
                if (memberInfo.MemberType == MemberTypes.Field)
                {
                    var t = ((FieldInfo) memberInfo).FieldType;
                    if (t.GetInterfaces().Contains(typeof(ISettings)))
                    {
                        Settings = t.GetSettingsProperties(pluginType);
                        break;
                    }
                }
            }

            if (_xml == null || _xml.Name != "documentation")
            {
                //plugin doesn't have a proper _xml file
                _xml = null;
                Localizations.Add("en", new LocalizedPluginDocumentationPage(this, pluginType, null, "en", pluginImage as BitmapFrame));
            }
            else
            {
                foreach (var lang in GetAvailableLanguagesFromXML())
                {
                    Localizations.Add(lang, new LocalizedPluginDocumentationPage(this, pluginType, _xml, lang, pluginImage as BitmapFrame));
                }
                if (!Localizations.ContainsKey("en"))
                    throw new Exception("Plugin documentation should at least support english language!");
            }
        }

        private IEnumerable<string> GetAvailableLanguagesFromXML()
        {
            return _xml.Elements("language").Select(langElement => langElement.Attribute("culture").Value);
        }

        private static XElement GetPluginXML(Type pluginType)
        {
            try
            {
                var descriptionUrl = pluginType.GetPluginInfoAttribute().DescriptionUrl;
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
            catch (Exception ex)
            {
                //Console.Error.WriteLine(string.Format("Error loading XML file of plugin {0}: {1}", pluginType.GetPluginInfoAttribute().Caption, ex.Message));
                return null;
            }
        }
    }
}
