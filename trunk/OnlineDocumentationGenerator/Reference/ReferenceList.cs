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

        public string ToHTML(string lang)
        {
            if (_references.Count == 0)
            {
                return Resources.NoContent;
            }

            var builder = new StringBuilder();
            builder.AppendLine(string.Format("<p>{0}</p>", Resources.References_description));
            builder.AppendLine("<p><ul>");

            foreach (var reference in _references)
            {
                builder.AppendLine("<li>");
                builder.AppendLine(reference.ToHTML(lang));
                builder.AppendLine("</li>");
            }

            builder.AppendLine("</ul></p>");
            return builder.ToString();
        }
    }
}
