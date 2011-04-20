using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Cryptool.PluginBase;
using OnlineDocumentationGenerator.Properties;

namespace OnlineDocumentationGenerator.Generators.HtmlGenerator
{
    /// <summary>
    /// Class for converting an object to an html representation.
    /// Also takes care of referenced image resources while converting.
    /// </summary>
    class ObjectConverter
    {
        private readonly List<PluginDocumentationPage> _pluginPages;
        private readonly HashSet<string> _createdImages = new HashSet<string>();

        public ObjectConverter(List<PluginDocumentationPage> pluginPages)
        {
            _pluginPages = pluginPages;
        }

        public string Convert(object theObject, PluginDocumentationPage pluginDocumentationPage)
        {
            if (theObject == null)
                return Resources.Not_available;

            if (theObject is XElement)
            {
                return ConvertXElement((XElement)theObject, pluginDocumentationPage);
            }
            if (theObject is BitmapFrame)
            {
                return ConvertImageSource((BitmapFrame)theObject, pluginDocumentationPage.Localizations["en"].Name);
            }
            if (theObject is PluginTemplateList)
            {
                return ConvertPluginTemplateList((PluginTemplateList)theObject, pluginDocumentationPage);
            }

            return theObject.ToString();
        }

        private string ConvertPluginTemplateList(PluginTemplateList pluginTemplateList, PluginDocumentationPage pluginDocumentationPage)
        {
            if (pluginTemplateList.Templates.Count == 0)
                return "None";

            var codeBuilder = new StringBuilder();
            codeBuilder.AppendLine("<table border=\"1\">");
            codeBuilder.AppendLine(string.Format("<tr> <th>{0}</th> <th>{1}</th> </tr>",
                Resources.File, Resources.Description));

            foreach (var template in pluginTemplateList.Templates)
            {
                var link = Path.Combine(Path.Combine("..\\..", DocGenerator.TemplateDirectory), template.Path);
                var file = Path.GetFileName(template.Path);
                codeBuilder.AppendLine(string.Format("<tr> <td><a href=\"{0}\">{1}</a></td> <td>{2}</td> </tr>",
                    link, file, ConvertXElement(template.Description, pluginDocumentationPage)));
            }

            codeBuilder.AppendLine("</table>");

            return codeBuilder.ToString();
        }

        /// <summary>
        /// Converts the given imageSource parameter to a file in the doc directory and returns an html string referencing this.
        /// </summary>
        /// <param name="imageSource">The ImageSource containing the image to convert.</param>
        /// <param name="filename">The wished filename (withouth the extension)</param>
        /// <returns></returns>
        private string ConvertImageSource(BitmapFrame imageSource, string filename)
        {
            filename = filename + ".png";
            if (!_createdImages.Contains(filename))
            {
                //create image file:
                if (!Directory.Exists(OnlineHelp.PluginDocDirectory))
                {
                    Directory.CreateDirectory(OnlineHelp.PluginDocDirectory);
                }
                var file = Path.Combine(OnlineHelp.PluginDocDirectory, filename);
                using (var fileStream = new FileStream(file, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(imageSource);
                    encoder.Save(fileStream);
                }
            }
            _createdImages.Add(filename);
            return string.Format("<img src=\"{0}\" />", filename);
        }

        /// <summary>
        /// Converts the given xelement, which is from the plugin xml doc file, into an html formated representation.
        /// </summary>
        /// <param name="xelement"></param>
        /// <param name="pluginDocumentationPage"></param>
        /// <returns></returns>
        private string ConvertXElement(XElement xelement, PluginDocumentationPage pluginDocumentationPage)
        {
            var result = new StringBuilder();
            foreach (var node in xelement.Nodes())
            {
                if (node is XText)
                {
                    result.Append(((XText) node).Value);
                }
                else if (node is XElement)
                {
                    var nodeName = ((XElement) node).Name.ToString();
                    switch (nodeName)
                    {
                        case "b":
                        case "i":
                        case "u":
                            var nodeRep = ConvertXElement((XElement)node, pluginDocumentationPage);
                            result.Append(string.Format("<{0}>{1}</{0}>", nodeName, nodeRep));
                            break;
                        case "img":
                            var srcAtt = ((XElement) node).Attribute("src");
                            if (srcAtt != null)
                            {
                                int sIndex = srcAtt.Value.IndexOf('/');
                                var image = BitmapFrame.Create(new Uri(string.Format("pack://application:,,,/{0};component/{1}", 
                                    srcAtt.Value.Substring(0, sIndex), srcAtt.Value.Substring(sIndex + 1))));
                                var filename = string.Format("{0}_{1}", pluginDocumentationPage.Localizations["en"].Name, Path.GetFileNameWithoutExtension(srcAtt.Value));
                                result.Append(ConvertImageSource(image, filename));
                            }
                            break;
                        case "newline":
                            result.Append("<br/>");
                            break;
                        case "section":
                            var headline = ((XElement) node).Attribute("headline");
                            if (headline != null)
                            {
                                result.AppendLine(string.Format("<h3>{0}</h3>", headline.Value));
                                result.AppendLine(ConvertXElement((XElement) node, pluginDocumentationPage));
                            }
                            break;
                        case "enum":
                        case "list":
                            var t = (nodeName == "enum") ? "ol" : "ul";
                            result.AppendLine(string.Format("<{0}>", t));
                            foreach (var item in ((XElement)node).Elements("item"))
                            {
                                result.AppendLine(string.Format("<li>{0}</li>", ConvertXElement(item, pluginDocumentationPage)));
                            }
                            result.AppendLine(string.Format("</{0}>", t));
                            break;
                        case "external":
                            var reference = ((XElement) node).Attribute("ref");
                            if (reference != null)
                            {
                                var linkText = ConvertXElement((XElement) node, pluginDocumentationPage);
                                if (string.IsNullOrEmpty(linkText))
                                {
                                    linkText = reference.Value;
                                }
                                result.Append(string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", reference.Value, linkText));
                            }
                            break;
                        case "pluginRef":
                            var plugin = ((XElement)node).Attribute("plugin");
                            if (plugin != null)
                            {
                                var linkText = ConvertXElement((XElement)node, pluginDocumentationPage);
                                if (string.IsNullOrEmpty(linkText))
                                {
                                    linkText = plugin.Value;
                                }

                                var pluginLink = GetPluginLink(plugin.Value);
                                if (pluginLink != null)
                                {
                                    result.Append(string.Format("<a href=\"{0}\">{1}</a>", pluginLink, linkText));
                                }
                                else
                                {
                                    result.Append(string.Format("<i>{0}</i>", linkText));
                                }
                            }
                            break;
                        default:
                            continue;
                    }
                }
            }

            return result.ToString();
        }

        private string GetPluginLink(string plugin)
        {
            foreach(var p in _pluginPages)
            {
                if (p.Localizations["en"].Name == plugin)
                {
                    var lang = Thread.CurrentThread.CurrentUICulture.Name;
                    if (p.AvailableLanguages.Contains(lang))
                    {
                        return OnlineHelp.GetPluginDocFilename(p.PluginType, lang);
                    }
                    else
                    {
                        return OnlineHelp.GetPluginDocFilename(p.PluginType, "en");
                    }
                }
            }
            return null;
        }
    }
}
