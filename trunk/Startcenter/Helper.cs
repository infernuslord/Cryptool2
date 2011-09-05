using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Startcenter
{
    public static class Helper
    {
        public static XElement GetGlobalizedElementFromXML(XElement xml, string element)
        {
            CultureInfo currentLang = System.Globalization.CultureInfo.CurrentCulture;

            var allElements = xml.Elements(element);
            IEnumerable<XElement> foundElements = null;

            if (allElements.Any())
            {
                foundElements = from descln in allElements where descln.Attribute("lang").Value == currentLang.TextInfo.CultureName select descln;
                if (!foundElements.Any())
                {
                    foundElements = from descln in allElements where descln.Attribute("lang").Value == currentLang.TwoLetterISOLanguageName select descln;
                    if (!foundElements.Any())
                        foundElements = from descln in allElements where descln.Attribute("lang").Value == "en" select descln;
                }
            }

            if (foundElements == null || !foundElements.Any())
            {
                if (xml.Element(element) != null)
                    return xml.Element(element);
                else
                    return null;
            }

            return foundElements.First();
        }

        public static Inline ConvertFormattedXElement(XElement xelement)
        {
            var span = new Span();

            foreach (var node in xelement.Nodes())
            {
                if (node is XText)
                {
                    var line = new Regex("\\s*\\n").Replace(((XText) node).Value, " ");
                    span.Inlines.Add(new Run(line));
                }
                else if (node is XElement)
                {
                    var nodeName = ((XElement)node).Name.ToString();
                    switch (nodeName)
                    {
                        case "b":
                            var nodeRep = ConvertFormattedXElement((XElement)node);
                            span.Inlines.Add(new Bold(nodeRep));
                            break;
                        case "i":
                            nodeRep = ConvertFormattedXElement((XElement)node);
                            span.Inlines.Add(new Italic(nodeRep));
                            break;
                        case "u":
                            nodeRep = ConvertFormattedXElement((XElement)node);
                            span.Inlines.Add(new Underline(nodeRep));
                            break;
                        case "newline":
                            span.Inlines.Add(new LineBreak());
                            break;
                        case "external":
                            var reference = ((XElement)node).Attribute("ref");
                            if (reference != null)
                            {
                                var linkText = ConvertFormattedXElement((XElement)node);
                                if (linkText == null)
                                {
                                    linkText = new Run(reference.Value);
                                }
                                var link = new Hyperlink(linkText);
                                link.NavigateUri = new Uri(reference.Value);
                                span.Inlines.Add(link);
                            }
                            break;
                        case "pluginRef":
                            var plugin = ((XElement)node).Attribute("plugin");
                            if (plugin != null)
                            {
                                var linkText = ConvertFormattedXElement((XElement)node);
                                if (linkText == null)
                                {
                                    linkText = new Run(plugin.Value);
                                }
                                span.Inlines.Add(linkText);
                            }
                            break;
                        default:
                            continue;
                    }
                }
            }

            if (span.Inlines.Count == 0)
                return null;
            if (span.Inlines.Count == 1)
                return span.Inlines.First();

            return span;
        }
    }
}
