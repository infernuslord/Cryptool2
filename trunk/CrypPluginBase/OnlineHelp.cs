using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase
{
    public class OnlineHelp
    {
        public static readonly string HelpDirectory = "OnlineDocumentation";
        public static readonly string RelativePluginDocDirectory = "PluginDocs";
        public static readonly string PluginDocDirectory = Path.Combine(HelpDirectory, RelativePluginDocDirectory);

        public delegate void ShowPluginDocPageHandler(Type plugin);
        public static event ShowPluginDocPageHandler ShowPluginDocPage;

        public static void InvokeShowPluginDocPage(Type plugin)
        {
            if (ShowPluginDocPage != null) 
                ShowPluginDocPage(plugin);
        }

        public static string GetPluginDocFilename(Type plugin, string lang)
        {
            return string.Format("{0}_{1}.html", plugin.GetPluginInfoAttribute().Caption, lang);
        }

        public static string GetIndexFilename(string lang)
        {
            return string.Format("index_{0}.html", lang);
        }
    }
}
