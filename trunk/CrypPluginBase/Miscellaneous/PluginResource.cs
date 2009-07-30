using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cryptool.PluginBase.Miscellaneous
{
    public class PluginResource
    {
        private const string directory = "CrypPlugins";

        public static readonly string directoryPath;

        static PluginResource()
        {
            directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directory);
        }
    }
}
