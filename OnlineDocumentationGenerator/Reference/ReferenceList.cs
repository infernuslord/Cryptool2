using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            var builder = new StringBuilder();
            builder.AppendLine("<ul>");

            foreach (var reference in _references)
            {
                builder.AppendLine("<li>");
                builder.AppendLine(reference.ToHTML(lang));
                builder.AppendLine("</li>");
            }

            builder.AppendLine("</ul>");
            return builder.ToString();
        }
    }
}
