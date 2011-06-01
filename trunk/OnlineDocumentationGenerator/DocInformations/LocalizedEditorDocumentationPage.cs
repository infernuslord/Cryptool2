using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations
{
    public class LocalizedEditorDocumentationPage : LocalizedEntityDocumentationPage
    {
        public XElement Manual { get; private set; }

        public LocalizedEditorDocumentationPage(EditorDocumentationPage editorDocumentationPage, Type entityType, XElement xml, string lang, BitmapFrame icon) 
            : base(editorDocumentationPage, entityType, xml, lang, icon)
        {
            if (_xml != null)
                ReadInformationsFromXML();
        }

        private void ReadInformationsFromXML()
        {
            Manual = FindLocalizedChildElement(_xml, "usage");
        }
    }
}
