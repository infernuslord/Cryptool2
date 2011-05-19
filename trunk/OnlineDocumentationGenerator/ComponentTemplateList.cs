using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OnlineDocumentationGenerator
{
    public class ComponentTemplateList
    {
        public struct Template
        {
            public string Path { get; set; }
            public XElement Description { get; set; }
        }

        private readonly List<Template> _templates = new List<Template>();
        public List<Template> Templates
        {
            get { return _templates; }
        }

        public void Add(string path, XElement description)
        {
            Templates.Add(new Template() {Path = path, Description = description});
        }
    }
}
