using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OnlineDocumentationGenerator.Properties;

namespace OnlineDocumentationGenerator.Reference
{
    public class ReferenceList
    {
        private List<Reference> _references = new List<Reference>();

        public void Add(Reference reference)
        {
            _references.Add(reference);
        }

        public string GetHTMLinkToRef(string refID)
        {
            int c = 1;
            foreach (var reference in _references)
            {
                if (reference.ID == refID)
                {
                    return string.Format("<a href=\"#{0}\">[{1}]</a>", refID, c);
                }
                c++;
            }
            return null;
        }

        public string ToHTML(string lang)
        {
            if (_references.Count == 0)
            {
                return Resources.NoContent;
            }

            var builder = new StringBuilder();
            builder.AppendLine(string.Format("<p>{0}</p>", Resources.References_description));
            builder.AppendLine("<p><ol>");

            foreach (var reference in _references)
            {
                if (reference.ID != null)
                {
                    builder.AppendLine(string.Format("<li id=\"{0}\">", reference.ID));
                }
                else
                {
                    builder.AppendLine("<li>");
                }
                builder.AppendLine(reference.ToHTML(lang));
                builder.AppendLine("</li>");
            }

            builder.AppendLine("</ol></p>");
            return builder.ToString();
        }
    }
}
