using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator.DocInformations.Localization
{
    public abstract class LocalizedEntityDocumentationPage
    {
        public EntityDocumentationPage DocumentationPage { get; protected set; }
        public string Name { get; protected set; }
        public string Lang { get; protected set; }
        public abstract string FilePath { get; }

        public BitmapFrame Icon
        {
            get; protected set;
        }
    }
}