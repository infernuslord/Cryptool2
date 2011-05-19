using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Editor;

namespace Cryptool.PluginBase
{
    public class OnlineHelp
    {
        public static readonly string HelpDirectory = "OnlineDocumentation";
        public static readonly string RelativeComponentDocDirectory = "ComponentsDocs";
        public static readonly string ComponentDocDirectory = Path.Combine(HelpDirectory, RelativeComponentDocDirectory);

        public delegate void ShowDocPageHandler(Type entityType);
        public static event ShowDocPageHandler ShowDocPage;

        public static void InvokeShowPluginDocPage(Type plugin)
        {
            if (ShowDocPage != null) 
                ShowDocPage(plugin);
        }

        public static string GetDocFilename(Type plugin, string lang)
        {
            var filename = string.Format("{0}_{1}.html", plugin.FullName, lang);
            if (plugin.GetInterfaces().Contains(typeof(IEditor)))
            {
                return filename;
            }
            else
            {
                return Path.Combine(RelativeComponentDocDirectory, filename);
            }
        }

        public static string GetIndexFilename(string lang)
        {
            return string.Format("index_{0}.html", lang);
        }
    }
}
