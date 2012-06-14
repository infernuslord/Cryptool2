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
        public static readonly string RelativeTemplateDocDirectory = "Templates";

        public struct TemplateType
        {
            public string RelativeTemplateFilePath;
            public TemplateType(string relativeTemplateFilePath)
            {
                RelativeTemplateFilePath = relativeTemplateFilePath;
            }
        }

        public delegate void ShowDocPageHandler(object docEntity);
        public static event ShowDocPageHandler ShowDocPage;
        
        public static void InvokeShowDocPage(object docEntity)
        {
            if (ShowDocPage != null)
                ShowDocPage(docEntity);
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

        public static string GetTemplateDocFilename(string relativTemplateFilePath, string lang)
        {
            var flattenedPath = Path.GetDirectoryName(relativTemplateFilePath).Replace(Path.DirectorySeparatorChar, '.');
            var filename = string.Format("{0}.{1}_{2}.html", flattenedPath, Path.GetFileNameWithoutExtension(relativTemplateFilePath), lang);
            return Path.Combine(RelativeTemplateDocDirectory, filename);
        }

        public static string GetIndexFilename(string lang)
        {
            return string.Format("index_{0}.html", lang);
        }

        public static string GetTemplatesIndexFilename(string lang)
        {
            return string.Format("templates_{0}.html", lang);
        }
        
    }
}
