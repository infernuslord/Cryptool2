﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.Reference
{
    public class LinkReference : Reference
    {
        public string Link
        {
            get
            {
                return GetLocalizedProperty("Link", Thread.CurrentThread.CurrentCulture.Name);
            }
        }

        public string Caption
        {
            get
            {
                return GetLocalizedProperty("Caption", Thread.CurrentThread.CurrentCulture.Name);
            }
        }

        public LinkReference(XElement linkReferenceElement)
        {
            foreach (var e in linkReferenceElement.Elements())
            {
                var lang = "en";
                if (e.Attribute("lang") != null)
                {
                    lang = e.Attribute("lang").Value;
                }

                if (e.Name == "link")
                {
                    if (e.Attribute("url") != null)
                    {
                        SetLocalizedProperty("Link", lang, e.Attribute("url").Value);
                    }
                }
                else if (e.Name == "caption")
                {
                    SetLocalizedProperty("Caption", lang, e.Value);
                }
            }
        }

        public override string ToHTML(string lang)
        {
            return string.Format("{0} - <a href=\"{1}\" target=\"_blank\">{1}</a>", Caption, Link);
        }
    }
}
