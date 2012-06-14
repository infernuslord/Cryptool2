using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using OnlineDocumentationGenerator.DocInformations.Localization;

namespace OnlineDocumentationGenerator.DocInformations
{
    public class EditorDocumentationPage : PluginDocumentationPage
    {
        public EditorDocumentationPage(Type pluginType) : base(pluginType)
        {
        }

        protected override LocalizedPluginDocumentationPage CreateLocalizedEntityDocumentationPage(PluginDocumentationPage pluginDocumentationPage, Type editorType, XElement xml, string lang, BitmapFrame editorImage)
        {
            if (pluginDocumentationPage is EditorDocumentationPage)
            {
                return new LocalizedEditorDocumentationPage((EditorDocumentationPage)pluginDocumentationPage,
                                                               editorType, xml, lang, editorImage);
            }
            return null;
        }
    }
}
