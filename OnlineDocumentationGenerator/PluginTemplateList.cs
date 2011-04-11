using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnlineDocumentationGenerator
{
    public class PluginTemplateList
    {
        public struct Template
        {
            public string Path { get; set; }
            public string Description { get; set; }
        }

        private readonly List<Template> _templates = new List<Template>();
        public List<Template> Templates
        {
            get { return _templates; }
        }

        public void Add(string path, string description)
        {
            Templates.Add(new Template() {Path = path, Description = description});
        }
    }
}
