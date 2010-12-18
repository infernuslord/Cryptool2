using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;

namespace Cryptool.PluginBase.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingsTabAttribute : Attribute
    {
        public string ResourceFile { get; private set; }
        public string Caption { get; private set; }
        public string Address { get; private set; }

        public SettingsTabAttribute(string resourceFile, string caption, string address)
        {
            ResourceFile = resourceFile;
            Caption = caption;
            Address = address;
        }
    }
}
