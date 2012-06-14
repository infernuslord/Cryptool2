using System;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Cryptool.PluginBase;
using OnlineDocumentationGenerator.DocInformations.Localization;

namespace OnlineDocumentationGenerator.DocInformations
{
    public class ComponentDocumentationPage : PluginDocumentationPage
    {
        public PropertyInfoAttribute[] Connectors { get; private set; }

        public ComponentDocumentationPage(Type componentType) : base(componentType)
        {
            Connectors = PluginExtension.GetProperties(componentType);
        }

        protected override LocalizedPluginDocumentationPage CreateLocalizedEntityDocumentationPage(PluginDocumentationPage pluginDocumentationPage, Type componentType, XElement xml, string lang, BitmapFrame componentImage)
        {
            if (pluginDocumentationPage is ComponentDocumentationPage)
            {
                return new LocalizedComponentDocumentationPage((ComponentDocumentationPage) pluginDocumentationPage,
                                                               componentType, xml, lang, componentImage);
            }
            return null;
        }
    }
}
