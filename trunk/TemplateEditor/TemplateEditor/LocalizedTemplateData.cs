using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemplateEditor
{
    public class LocalizedTemplateData
    {
        public string Lang { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> Keywords { get; set; }
    }
}
