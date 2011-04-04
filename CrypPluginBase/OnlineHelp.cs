using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase
{
    public class OnlineHelp
    {
        public static readonly string HelpDirectory = "OnlineDocumentation";
        public static readonly string PluginDocDirectory = HelpDirectory+"/PluginDocs";

        public static string GetPluginDocFilename(Type plugin, string lang)
        {
            return string.Format("{0}_{1}.html", plugin.GetPluginInfoAttribute().Caption, lang);
        }
    }
}
