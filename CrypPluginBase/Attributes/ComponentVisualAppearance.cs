using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentVisualAppearance : Attribute
    {
        public enum VisualAppearanceEnum { Closed, Opened };

        public VisualAppearanceEnum DefaultVisualAppearance;

        public ComponentVisualAppearance(VisualAppearanceEnum defaultVisualAppearance)
        {
            DefaultVisualAppearance = defaultVisualAppearance;
        }
    }
}
