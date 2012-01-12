using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OnlineDocumentationGenerator.DocInformations;
using OnlineDocumentationGenerator.Properties;

namespace OnlineDocumentationGenerator.Generators.HtmlGenerator
{
    class TagReplacer
    {
        private static readonly Regex FindDocItemTagRegex = new Regex("<docItem.*?property=\"(.*?)\".*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindLanguageSelectionTagRegex = new Regex("<languageSelection.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindBeginningLanguageSwitchTagRegex = new Regex("<languageSwitch.*?lang=\"(.*?)\".*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindEndingLanguageSwitchTagRegex = new Regex("</.*?languageSwitch.*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindComponentListTagRegex = new Regex("<componentList.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindComponentTreeTagRegex = new Regex("<componentTree.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FindEditorListTagRegex = new Regex("<editorList.*?/>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string ReplaceDocItemTags(string html, LocalizedEntityDocumentationPage localizedDocumentationPage, ObjectConverter objectConverter)
        {
            int pos;
            int len;
            string property;
            var htmlBuilder = new StringBuilder(html);

            while ((property = FindDocItemTag(htmlBuilder.ToString(), out pos, out len)) != null)
            {
                try
                {
                    var prop = localizedDocumentationPage.GetType().GetProperty(property);
                    var propVal = prop.GetValue(localizedDocumentationPage, null);
                    var propStr = objectConverter == null ? (propVal == null ? Resources.Null : propVal.ToString()) : objectConverter.Convert(propVal, localizedDocumentationPage.DocumentationPage);

                    htmlBuilder.Remove(pos, len);
                    htmlBuilder.Insert(pos, propStr);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error trying to replace DocItem tag with property {0}! Message: {1}", property, ex.Message));
                }
            }

            return htmlBuilder.ToString();
        }

        internal static string FindDocItemTag(string html, out int pos, out int len)
        {
            var match = FindDocItemTagRegex.Match(html);
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

        public static string ReplaceComponentList(string html, string componentListCode)
        {
            int pos;
            int len;
            var htmlBuilder = new StringBuilder(html);

            while (FindComponentListTag(htmlBuilder.ToString(), out pos, out len))
            {
                htmlBuilder.Remove(pos, len);
                htmlBuilder.Insert(pos, componentListCode);
            }

            return htmlBuilder.ToString();
        }

        public static string ReplaceComponentTree(string html, string componentTreeCode)
        {
            int pos;
            int len;
            var htmlBuilder = new StringBuilder(html);

            while (FindComponentTreeTag(htmlBuilder.ToString(), out pos, out len))
            {
                htmlBuilder.Remove(pos, len);
                htmlBuilder.Insert(pos, componentTreeCode);
            }

            return htmlBuilder.ToString();
        }

        internal static bool FindComponentListTag(string html, out int pos, out int len)
        {
            var match = FindComponentListTagRegex.Match(html);
            pos = match.Index;
            len = match.Length;
            return match.Success;
        }

        internal static bool FindComponentTreeTag(string html, out int pos, out int len)
        {
            var match = FindComponentTreeTagRegex.Match(html);
            pos = match.Index;
            len = match.Length;
            return match.Success;
        }

        public static string ReplaceEditorList(string html, string editorListCode)
        {
            int pos;
            int len;
            var htmlBuilder = new StringBuilder(html);

            while (FindEditorListTag(htmlBuilder.ToString(), out pos, out len))
            {
                htmlBuilder.Remove(pos, len);
                htmlBuilder.Insert(pos, editorListCode);
            }

            return htmlBuilder.ToString();
        }

        internal static bool FindEditorListTag(string html, out int pos, out int len)
        {
            var match = FindEditorListTagRegex.Match(html);
            pos = match.Index;
            len = match.Length;
            return match.Success;
        }
    }
}
