using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.Miscellaneous
{
    public class ZoomChanged : EventArgs
    {
        public double Value { get; set; }
    }
}
