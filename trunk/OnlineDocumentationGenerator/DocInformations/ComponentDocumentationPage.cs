using System;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Cryptool.PluginBase;

namespace OnlineDocumentationGenerator.DocInformations
{
    public class ComponentDocumentationPage : EntityDocumentationPage
    {
        public PropertyInfoAttribute[] Connectors { get; private set; }

        public ComponentDocumentationPage(Type componentType) : base(componentType)
        {
            Connectors = PluginExtension.GetProperties(componentType);
        }

        protected override LocalizedEntityDocumentationPage CreateLocalizedEntityDocumentationPage(EntityDocumentationPage componentDocumentationPage, Type componentType, XElement xml, string lang, BitmapFrame componentImage)
        {
            if (componentDocumentationPage is ComponentDocumentationPage)
            {
                return new LocalizedComponentDocumentationPage((ComponentDocumentationPage) componentDocumentationPage,
                                                               componentType, xml, lang, componentImage);
            }
            return null;
        }
    }
}
