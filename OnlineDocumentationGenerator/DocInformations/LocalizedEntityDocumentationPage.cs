using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations
{
    public abstract class LocalizedEntityDocumentationPage
    {
        public EntityDocumentationPage DocumentationPage { get; protected set; }
        public string Name { get; protected set; }
        public string Lang { get; protected set; }

        public string AuthorName
        {
            get { return DocumentationPage.AuthorName; }
        }

        public BitmapFrame Icon
        {
            get; protected set;
        }

        protected static XElement FindLocalizedChildElement(XElement element, string xname)
        {
            const string defaultLang = "en";
            CultureInfo currentLang = Thread.CurrentThread.CurrentUICulture;

            IEnumerable<XElement> allElements = element.Elements(xname);
            IEnumerable<XElement> foundElements = null;

            if (allElements.Any())
            {
                foundElements = from descln in allElements 
                                where (new CultureInfo(descln.Attribute("lang").Value)).TwoLetterISOLanguageName == currentLang.TwoLetterISOLanguageName 
                                select descln;
                if (!foundElements.Any())
                {
                    foundElements = from descln in allElements 
                                    where (new CultureInfo(descln.Attribute("lang").Value)).TwoLetterISOLanguageName == defaultLang 
                                    select descln;
                }
            }

            if (foundElements == null || !foundElements.Any() || !allElements.Any())
            {
                if (!allElements.Any())
                {
                    return null;
                }
                else
                    return allElements.First();
            }

            return foundElements.First();
        }
    }
}