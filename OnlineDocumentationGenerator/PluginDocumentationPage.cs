using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Cryptool.PluginBase;

namespace OnlineDocumentationGenerator
{
    public class PluginDocumentationPage
    {
        public Type PluginType { get; private set; }
        private readonly XElement _xml;

        public Dictionary<string, LocalizedPluginDocumentationPage> Localizations { get; private set; }

        public List<string> AvailableLanguages
        {
            get { return Localizations.Keys.ToList(); }
        }

        public PluginDocumentationPage(Type pluginType)
        {
            PluginType = pluginType;
            Localizations = new Dictionary<string, LocalizedPluginDocumentationPage>();
            _xml = GetPluginXML(pluginType);

            if (_xml == null)
            {
                throw new Exception("Plugin does not contain a xml doc file!");
            }

            foreach (var lang in GetAvailableLanguagesFromXML())
            {
                Localizations.Add(lang, new LocalizedPluginDocumentationPage(pluginType, _xml, lang));
            }
            if (!Localizations.ContainsKey("en"))
                throw new Exception("Plugin documentation should at least support english language!");
        }

        private IEnumerable<string> GetAvailableLanguagesFromXML()
        {
            return _xml.Elements("language").Select(langElement => langElement.Attribute("culture").Value);
        }

        private XElement GetPluginXML(Type pluginType)
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
                    var stream = pluginType.Assembly.GetManifestResourceStream(descriptionUrl);
                    return XElement.Load(stream);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(string.Format("Error loading XML file of plugin {0}: {1}", pluginType.GetPluginInfoAttribute().Caption, ex.Message));
                return null;
            }
        }
    }
}
