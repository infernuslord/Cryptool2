using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations
{
    public class EditorDocumentationPage : EntityDocumentationPage
    {
        public EditorDocumentationPage(Type editorType) : base(editorType)
        {
        }

        protected override LocalizedEntityDocumentationPage CreateLocalizedEntityDocumentationPage(EntityDocumentationPage editorDocumentationPage, Type editorType, XElement xml, string lang, BitmapFrame editorImage)
        {
            if (editorDocumentationPage is EditorDocumentationPage)
            {
                return new LocalizedEditorDocumentationPage((EditorDocumentationPage)editorDocumentationPage,
                                                               editorType, xml, lang, editorImage);
            }
            return null;
        }
    }
}
