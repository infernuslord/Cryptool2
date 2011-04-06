using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OnlineDocumentationGenerator.Generators.HtmlGenerator
{
    class TagReplacer
    {
        private static readonly Regex FindPluginDocTagRegex = new Regex("<pluginDoc.*?property=\"(.*?)\".*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindLanguageSelectionTagRegex = new Regex("<languageSelection.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindBeginningLanguageSwitchTagRegex = new Regex("<languageSwitch.*?lang=\"(.*?)\".*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindEndingLanguageSwitchTagRegex = new Regex("</.*?languageSwitch.*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindPluginListTagRegex = new Regex("<pluginList.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindConnectorListTagRegex = new Regex("<connectorList.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindSettingsListTagRegex = new Regex("<settingsList.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string ReplacePluginDocTags(string html, LocalizedPluginDocumentationPage localizedPluginDocumentationPage, ObjectConverter objectConverter)
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
                    var propStr = objectConverter == null ? (propVal == null ? "Null" : propVal.ToString()) : objectConverter.Convert(propVal, localizedPluginDocumentationPage.PluginDocumentationPage);

                    htmlBuilder.Remove(pos, len);
                    htmlBuilder.Insert(pos, propStr);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error trying to replace pluginDoc tag with property {0}! Message: {1}", property, ex.Message));
                }
            }

            return htmlBuilder.ToString();
        }

        internal static string FindPluginDocTag(string html, out int pos, out int len)
        {
            var match = FindPluginDocTagRegex.Match(html);
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

        internal static bool FindLanguageSelectionTag(string html, out int pos, out int len)
        {
            var match = FindLanguageSelectionTagRegex.Match(html);
            pos = match.Index;
            len = match.Length;
            return match.Success;
        }

        public static string ReplaceLanguageSwitchs(string html, string lang)
        {
            var htmlBuilder = new StringBuilder(html);
            Match match = FindBeginningLanguageSwitchTagRegex.Match(htmlBuilder.ToString());
            while (match.Success)
            {
                var pos = match.Index;
                var len = match.Length;

                var match2 = FindEndingLanguageSwitchTagRegex.Match(htmlBuilder.ToString(), pos+len);
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

                match = FindBeginningLanguageSwitchTagRegex.Match(htmlBuilder.ToString());
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

        internal static bool FindPluginListTag(string html, out int pos, out int len)
        {
            var match = FindPluginListTagRegex.Match(html);
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

        internal static bool FindConnectorListTag(string html, out int pos, out int len)
        {
            var match = FindConnectorListTagRegex.Match(html);
            pos = match.Index;
            len = match.Length;
            return match.Success;
        }

        public static string ReplaceSettingsList(string html, string settingsListCode)
        {
            int pos;
            int len;
            var htmlBuilder = new StringBuilder(html);

            while (FindSettingsListTag(htmlBuilder.ToString(), out pos, out len))
            {
                htmlBuilder.Remove(pos, len);
                htmlBuilder.Insert(pos, settingsListCode);
            }

            return htmlBuilder.ToString();
        }

        internal static bool FindSettingsListTag(string html, out int pos, out int len)
        {
            var match = FindSettingsListTagRegex.Match(html);
            pos = match.Index;
            len = match.Length;
            return match.Success;
        }
    }
}
