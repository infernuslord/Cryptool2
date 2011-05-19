using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Cryptool.PluginBase;
using OnlineDocumentationGenerator.DocInformations;
using OnlineDocumentationGenerator.Properties;

namespace OnlineDocumentationGenerator.Generators.HtmlGenerator
{
    /// <summary>
    /// Class for converting an object to an html representation.
    /// Also takes care of referenced image resources while converting.
    /// </summary>
    class ObjectConverter
    {
        private readonly List<EntityDocumentationPage> _docPages;
        private readonly string _outputDir;
        private readonly HashSet<string> _createdImages = new HashSet<string>();

        public ObjectConverter(List<EntityDocumentationPage> docPages, string outputDir)
        {
            _docPages = docPages;
            _outputDir = outputDir;
        }

        public string Convert(object theObject, EntityDocumentationPage componentDocumentationPage)
        {
            if (theObject == null)
                return Resources.Not_available;

            if (theObject is XElement)
            {
                return ConvertXElement((XElement)theObject, componentDocumentationPage);
            }
            if (theObject is BitmapFrame)
            {
                return ConvertImageSource((BitmapFrame)theObject, componentDocumentationPage.EntityType.FullName, componentDocumentationPage.EntityType);
            }
            if (theObject is ComponentTemplateList)
            {
                return ConvertTemplateList((ComponentTemplateList)theObject, componentDocumentationPage);
            }
            if (theObject is Reference.ReferenceList)
            {
                return ((Reference.ReferenceList)theObject).ToHTML(Thread.CurrentThread.CurrentUICulture.Name);
            }
            if (theObject is PropertyInfoAttribute[])
            {
                return ConvertConnectorList((PropertyInfoAttribute[])theObject);
            }
            if (theObject is TaskPaneAttribute[])
            {
                return ConvertSettingsList((TaskPaneAttribute[]) theObject);
            }

            return theObject.ToString();
        }

        private string ConvertSettingsList(TaskPaneAttribute[] settings)
        {
            if (settings != null)
            {
                var codeBuilder = new StringBuilder();
                codeBuilder.AppendLine("<table border=\"1\">");
                codeBuilder.AppendLine(string.Format("<tr> <th>{0}</th> <th>{1}</th> <th>{2}</th> </tr>",
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Name,
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Description,
                                                     Resources.HtmlGenerator_GenerateSettingsListCode_Type));

                foreach (var setting in settings)
                {
                    codeBuilder.AppendLine(string.Format("<tr> <td>{0}</td> <td>{1}</td> <td>{2}</td> </tr>",
                                                         setting.Caption, setting.ToolTip,
                                                         setting.ControlType.ToString()));
                }

                codeBuilder.AppendLine("</table>");
                return codeBuilder.ToString();
            }
            return Resources.NoContent;
        }

        private string ConvertConnectorList(PropertyInfoAttribute[] connectors)
        {
            if (connectors != null)
            {
                var codeBuilder = new StringBuilder();
                codeBuilder.AppendLine("<table border=\"1\">");
                codeBuilder.AppendLine(string.Format("<tr> <th>{0}</th> <th>{1}</th> <th>{2}</th> <th>{3}</th> </tr>",
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Name,
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Description,
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Direction,
                                                     Resources.HtmlGenerator_GenerateConnectorListCode_Type));

                foreach (var connector in connectors)
                {
                    var type = connector.PropertyInfo.PropertyType.Name;
                    codeBuilder.AppendLine(
                        string.Format("<tr> <td>{0}</td> <td>{1}</td> <td>{2}</td> <td>{3}</td> </tr>",
                                      connector.Caption,
                                      connector.ToolTip,
                                      connector.Direction,
                                      type));
                }

                codeBuilder.AppendLine("</table>");
                return codeBuilder.ToString();
            }
            return Resources.NoContent;
        }

        private string ConvertTemplateList(ComponentTemplateList componentTemplateList, EntityDocumentationPage entityDocumentationPage)
        {
            if (componentTemplateList.Templates.Count == 0)
                return Resources.NoContent;

            var codeBuilder = new StringBuilder();
            codeBuilder.AppendLine("<table border=\"1\">");
            codeBuilder.AppendLine(string.Format("<tr> <th>{0}</th> <th>{1}</th> </tr>",
                Resources.File, Resources.Description));

            foreach (var template in componentTemplateList.Templates)
            {
                var link = Path.Combine(Path.Combine("..\\..", DocGenerator.TemplateDirectory), template.Path);
                var file = Path.GetFileName(template.Path);
                codeBuilder.AppendLine(string.Format("<tr> <td><a href=\"{0}\">{1}</a></td> <td>{2}</td> </tr>",
                    link, file, ConvertXElement(template.Description, entityDocumentationPage)));
            }

            codeBuilder.AppendLine("</table>");

            return codeBuilder.ToString();
        }

        /// <summary>
        /// Converts the given imageSource parameter to a file in the doc directory and returns an html string referencing this.
        /// </summary>
        /// <param name="imageSource">The ImageSource containing the image to convert.</param>
        /// <param name="filename">The wished filename (withouth the extension)</param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private string ConvertImageSource(BitmapFrame imageSource, string filename, Type entityType)
        {
            filename = filename + ".png";
            if (!_createdImages.Contains(filename))
            {
                var dir = Path.Combine(Path.Combine(_outputDir, OnlineHelp.HelpDirectory), Path.GetDirectoryName(OnlineHelp.GetDocFilename(entityType, "en")));
                //create image file:
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var file = Path.Combine(dir, filename);
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
        /// Converts the given xelement, which is from the xml doc file, into an html formated representation.
        /// </summary>
        /// <param name="xelement"></param>
        /// <param name="entityDocumentationPage"></param>
        /// <returns></returns>
        private string ConvertXElement(XElement xelement, EntityDocumentationPage entityDocumentationPage)
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
                            var nodeRep = ConvertXElement((XElement)node, entityDocumentationPage);
                            result.Append(string.Format("<{0}>{1}</{0}>", nodeName, nodeRep));
                            break;
                        case "img":
                            var srcAtt = ((XElement) node).Attribute("src");
                            if (srcAtt != null)
                            {
                                int sIndex = srcAtt.Value.IndexOf('/');
                                var image = BitmapFrame.Create(new Uri(string.Format("pack://application:,,,/{0};component/{1}", 
                                    srcAtt.Value.Substring(0, sIndex), srcAtt.Value.Substring(sIndex + 1))));
                                var filename = string.Format("{0}_{1}", entityDocumentationPage.EntityType.FullName, Path.GetFileNameWithoutExtension(srcAtt.Value));
                                result.Append(ConvertImageSource(image, filename, entityDocumentationPage.EntityType));
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
                                result.AppendLine(ConvertXElement((XElement) node, entityDocumentationPage));
                            }
                            break;
                        case "enum":
                        case "list":
                            var t = (nodeName == "enum") ? "ol" : "ul";
                            result.AppendLine(string.Format("<{0}>", t));
                            foreach (var item in ((XElement)node).Elements("item"))
                            {
                                result.AppendLine(string.Format("<li>{0}</li>", ConvertXElement(item, entityDocumentationPage)));
                            }
                            result.AppendLine(string.Format("</{0}>", t));
                            break;
                        case "external":
                            var reference = ((XElement) node).Attribute("ref");
                            if (reference != null)
                            {
                                var linkText = ConvertXElement((XElement) node, entityDocumentationPage);
                                if (string.IsNullOrEmpty(linkText))
                                {
                                    linkText = reference.Value;
                                }
                                result.Append(string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", reference.Value, linkText));
                            }
                            break;
                        case "docRef":
                            var itemAttribute = ((XElement)node).Attribute("item");
                            if (itemAttribute != null)
                            {
                                var linkText = ConvertXElement((XElement)node, entityDocumentationPage);
                                if (string.IsNullOrEmpty(linkText))
                                {
                                    linkText = itemAttribute.Value;
                                }
                                
                                int dirLevel = OnlineHelp.GetDocFilename(entityDocumentationPage.EntityType, "en").Split(Path.PathSeparator).Length;
                                var d = "";
                                for (int i = 0; i < dirLevel; i++)
                                {
                                    d += Path.Combine(d, "..");
                                }
                                var entityLink = GetEntityLink(itemAttribute.Value);
                                if (entityLink != null)
                                {
                                    result.Append(string.Format("<a href=\"{0}\">{1}</a>", Path.Combine(d, entityLink), linkText));
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

        private string GetEntityLink(string entity)
        {
            foreach(var p in _docPages)
            {
                if (p.Localizations["en"].Name == entity)
                {
                    var lang = Thread.CurrentThread.CurrentUICulture.Name;
                    if (p.AvailableLanguages.Contains(lang))
                    {
                        return OnlineHelp.GetDocFilename(p.EntityType, lang);
                    }
                    else
                    {
                        return OnlineHelp.GetDocFilename(p.EntityType, "en");
                    }
                }
            }
            return null;
        }
    }
}
