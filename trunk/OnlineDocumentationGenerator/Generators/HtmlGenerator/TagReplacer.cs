using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OnlineDocumentationGenerator.Generators.HtmlGenerator
{
    public class TagReplacer
    {
        private static readonly Regex _findPluginDocTagRegex = new Regex("<pluginDoc.*?property=\"(.*?)\".*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _findLanguageSelectionTagRegex = new Regex("<languageSelection.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _findBeginningLanguageSwitchTagRegex = new Regex("<languageSwitch.*?lang=\"(.*?)\".*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _findEndingLanguageSwitchTagRegex = new Regex("</.*?languageSwitch.*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _findPluginListTagRegex = new Regex("<pluginList.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _findConnectorListTagRegex = new Regex("<connectorList.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string ReplacePluginDocTags(string html, LocalizedPluginDocumentationPage localizedPluginDocumentationPage)
        {
            int pos;
            int len;
            string property;
            var htmlBuilder = new StringBuilder(html);

            while ((property = FindPluginDocTag(htmlBuilder.ToString(), out pos, out len)) != null)
            {
                try
                {
                    var prop = typeof(LocalizedPluginDocumentationPage).GetProperty(property);
                    var propVal = prop.GetValue(localizedPluginDocumentationPage, null);
                    htmlBuilder.Remove(pos, len);
                    htmlBuilder.Insert(pos, propVal);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error trying to replace pluginDoc tag with property {0}! Message: {1}", property, ex.Message);
                }
            }

            return htmlBuilder.ToString();
        }

        public static string FindPluginDocTag(string html, out int pos, out int len)
        {
            var match = _findPluginDocTagRegex.Match(html);
            pos = match.Index;
            len = match.Length;
            if (!match.Success || match.Groups.Count < 2)
                return null;
            var property = match.Groups[1].Value;
            return property;
        }

        public static string ReplaceLanguageSelectionTag(string html, string languageSelectionCode)
        {
            int pos;
            int len;
            var htmlBuilder = new StringBuilder(html);

            while (FindLanguageSelectionTag(htmlBuilder.ToString(), out pos, out len))
            {
                htmlBuilder.Remove(pos, len);
                htmlBuilder.Insert(pos, languageSelectionCode);
            }

            return htmlBuilder.ToString();
        }

        public static bool FindLanguageSelectionTag(string html, out int pos, out int len)
        {
            var match = _findLanguageSelectionTagRegex.Match(html);
            pos = match.Index;
            len = match.Length;
            return match.Success;
        }

        public static string ReplaceLanguageSwitchs(string html, string lang)
        {
            var htmlBuilder = new StringBuilder(html);
            Match match = _findBeginningLanguageSwitchTagRegex.Match(htmlBuilder.ToString());
            while (match.Success)
            {
                var pos = match.Index;
                var len = match.Length;

                var match2 = _findEndingLanguageSwitchTagRegex.Match(htmlBuilder.ToString(), pos+len);
                if (!match2.Success)
                    throw new Exception("Error trying to replace language switch!");
                var pos2 = match2.Index;
                var len2 = match2.Length;
                
                if (match.Groups[1].Value == lang)
                {
                    htmlBuilder.Remove(pos2, len2);
                    htmlBuilder.Remove(pos, len);
                }
                else
                {
                    htmlBuilder.Remove(pos, (pos2 - pos) + len2);
                }

                match = _findBeginningLanguageSwitchTagRegex.Match(htmlBuilder.ToString());
            }

            return htmlBuilder.ToString();
        }

        public static string ReplacePluginList(string html, string pluginListCode)
        {
            int pos;
            int len;
            var htmlBuilder = new StringBuilder(html);

            while (FindPluginListTag(htmlBuilder.ToString(), out pos, out len))
            {
                htmlBuilder.Remove(pos, len);
                htmlBuilder.Insert(pos, pluginListCode);
            }

            return htmlBuilder.ToString();
        }

        public static bool FindPluginListTag(string html, out int pos, out int len)
        {
            var match = _findPluginListTagRegex.Match(html);
            pos = match.Index;
            len = match.Length;
            return match.Success;
        }

        public static string ReplaceConnectorList(string html, string connectorListCode)
        {
            int pos;
            int len;
            var htmlBuilder = new StringBuilder(html);

            while (FindConnectorListTag(htmlBuilder.ToString(), out pos, out len))
            {
                htmlBuilder.Remove(pos, len);
                htmlBuilder.Insert(pos, connectorListCode);
            }

            return htmlBuilder.ToString();
        }

        public static bool FindConnectorListTag(string html, out int pos, out int len)
        {
            var match = _findConnectorListTagRegex.Match(html);
            pos = match.Index;
            len = match.Length;
            return match.Success;
        }
    }
}
