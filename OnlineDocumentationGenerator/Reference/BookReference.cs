using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.Reference
{
    public class BookReference : Reference
    {
        public string Name
        {
            get
            {
                return GetLocalizedProperty("Name", Thread.CurrentThread.CurrentCulture.Name);
            }
        }

        public string Author
        {
            get
            {
                return GetLocalizedProperty("Author", Thread.CurrentThread.CurrentCulture.Name);
            }
        }

        public string Publisher
        {
            get
            {
                return GetLocalizedProperty("Publisher", Thread.CurrentThread.CurrentCulture.Name);
            }
        }

        public BookReference(XElement linkReferenceElement) : base(linkReferenceElement)
        {
            foreach (var e in linkReferenceElement.Elements())
            {
                var lang = "en";
                if (e.Attribute("lang") != null)
                {
                    lang = e.Attribute("lang").Value;
                }

                if (e.Name == "author")
                {
                    SetLocalizedProperty("Author", lang, e.Value);
                }
                else if (e.Name == "publisher")
                {
                    SetLocalizedProperty("Publisher", lang, e.Value);
                }
                else if (e.Name == "name")
                {
                    SetLocalizedProperty("Name", lang, e.Value);
                }
            }
        }

        public override string ToHTML(string lang)
        {
            string p = "";
            if (!string.IsNullOrEmpty(Publisher))
                string.Format("(<i>{0}</i>)", Publisher);
            return string.Format("<b>{0}</b> - {1} {2}", Name, Author, p);
        }
    }
}
