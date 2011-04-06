using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Cryptool.PluginBase;

namespace OnlineDocumentationGenerator.Generators.HtmlGenerator
{
    /// <summary>
    /// Class for converting an object to an html representation.
    /// Also takes care of referenced image resources while converting.
    /// </summary>
    class ObjectConverter
    {
        private readonly HashSet<string> _createdImages = new HashSet<string>();

        public string Convert(object theObject, PluginDocumentationPage pluginDocumentationPage)
        {
            if (theObject is XElement)
            {
                return ConvertXElement((XElement) theObject);
            }
            if (theObject is BitmapFrame)
            {
                return ConvertImageSource((BitmapFrame)theObject, pluginDocumentationPage.Localizations["en"].Name);
            }

            return theObject.ToString();
        }

        /// <summary>
        /// Converts the given imageSource parameter to a file in the doc directory and returns an html string referencing this.
        /// </summary>
        /// <param name="imageSource">The ImageSource containing the image to convert.</param>
        /// <param name="filename">The wished filename (withouth the extension)</param>
        /// <returns></returns>
        private string ConvertImageSource(BitmapFrame imageSource, string filename)
        {
            filename = filename + ".jpg";
            if (!_createdImages.Contains(filename))
            {
                //create image file:
                var file = Path.Combine(OnlineHelp.PluginDocDirectory, filename);
                using (var fileStream = new FileStream(file, FileMode.Create))
                {
                    var encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(imageSource);
                    encoder.QualityLevel = 100;
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
        /// <returns></returns>
        private string ConvertXElement(XElement xelement)
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
                    var nodeRep = ConvertXElement((XElement) node);
                    switch (nodeName)
                    {
                        case "b":
                        case "i":
                        case "u":
                            result.Append(string.Format("<{0}>{1}</{0}>", nodeName, nodeRep));
                            break;
                        case "img":
                            //TODO
                            break;
                        default:
                            continue;
                    }
                }
            }

            return result.ToString();
        }
    }
}
