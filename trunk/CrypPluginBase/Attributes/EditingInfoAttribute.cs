using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EditingInfoAttribute : Attribute
    {
        public bool CanEdit { get; private set; }

        public EditingInfoAttribute(bool canEdit)
        {
            CanEdit = canEdit;
        }
    }
}
