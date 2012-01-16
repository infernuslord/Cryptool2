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
            public XElement Title { get; set; }           

            public override bool Equals(object obj)
            {
                if (Title.Value != null && obj != null && obj is Template && ((Template)obj).Title != null)
                {
                    return ((Template)obj).Title.Value.Equals(Title.Value);
                }
                else
                {
                    return false;
                }
            }
        }

        private readonly List<Template> _templates = new List<Template>();
        public List<Template> Templates
        {
            get { return _templates; }
        }

        public void Add(XElement title, string path, XElement description)
        {
            Templates.Add(new Template() { Title = title, Path = path, Description = description });
        }
    }
}
