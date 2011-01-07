using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LocalizationAttribute : Attribute
    {
        public string ResourceFile { get; private set; }

        public LocalizationAttribute(string resourceFile)
        {
            ResourceFile = resourceFile;
        }
    }
}
