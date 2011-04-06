using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Cryptool.PluginBase;
using OnlineDocumentationGenerator.Properties;

namespace OnlineDocumentationGenerator.Generators.HtmlGenerator
{
    public class HtmlGenerator : Generator
    {
        private readonly string _htmlTemplate = Properties.Resources.TemplatePluginDocumentationPage;
        private ObjectConverter _objectConverter = new ObjectConverter();

        public override void Generate()
        {
            GeneratePluginDocPages();
            GenerateIndexPages();
        }

        private void GenerateIndexPages()
        {
            foreach (var lang in availableLanguages)
            {
                var indexHtml = TagReplacer.ReplaceLanguageSwitchs(Properties.Resources.TemplateIndex, lang);
                var languageSelectionCode = GenerateIndexLanguageSelectionCode(availableLanguages, lang);
                indexHtml = TagReplacer.ReplaceLanguageSelectionTag(indexHtml, languageSelectionCode);
                var pluginListCode = GeneratePluginListCode(pluginPages, lang);
                indexHtml = TagReplacer.ReplacePluginList(indexHtml, pluginListCode);

                var filename = OnlineHelp.GetIndexFilename(lang);
                StoreIndexPage(indexHtml, filename);
            }
        }

        private static string GeneratePluginListCode(IEnumerable<PluginDocumentationPage> pluginDocumentationPages, string lang)
        {
            var pluginListCode = new StringBuilder();
            foreach (var pluginDocumentationPage in pluginDocumentationPages)
            {
                var linkedLang = pluginDocumentationPage.Localizations.ContainsKey(lang) ? lang : "en";
                var pp = pluginDocumentationPage.Localizations[linkedLang];
                pluginListCode.AppendLine(string.Format("<a href=\"{0}/{1}\">{2}</a> - {3}<br/>", OnlineHelp.RelativePluginDocDirectory, 
                    OnlineHelp.GetPluginDocFilename(pp.PluginType, linkedLang), pp.Name, pp.ToolTip));
            }

            return pluginListCode.ToString();
        }

        private void GeneratePluginDocPages()
        {
            foreach (var pluginDocumentationPage in pluginPages)
            {
                foreach (var lang in pluginDocumentationPage.AvailableLanguages)
                {
                    var localizedPluginDocumentationPage = pluginDocumentationPage.Localizations[lang];

                    var cultureInfo = new CultureInfo(lang);
                    Thread.CurrentThread.CurrentCulture = cultureInfo;
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;

                    var html = TagReplacer.ReplaceLanguageSwitchs(_htmlTemplate, lang);
                    html = TagReplacer.ReplacePluginDocTags(html, localizedPluginDocumentationPage, _objectConverter);
                    var languageSelectionCode = GeneratePluginLanguageSelectionCode(pluginDocumentationPage.PluginType, pluginDocumentationPage.AvailableLanguages, lang);
                    html = TagReplacer.ReplaceLanguageSelectionTag(html, languageSelectionCode);
                    var connectorListCode = GenerateConnectorListCode(localizedPluginDocumentationPage);
                    html = TagReplacer.ReplaceConnectorList(html, connectorListCode);
                    var settingsListCode = GenerateSettingsListCode(localizedPluginDocumentationPage);
                    html = TagReplacer.ReplaceSettingsList(html, settingsListCode);

                    var filename = OnlineHelp.GetPluginDocFilename(pluginDocumentationPage.PluginType, lang);
                    StorePluginDocPage(html, filename);
                }
            }
        }

        private static void StorePluginDocPage(string html, string filename)
        {
            var filePath = Path.Combine(OnlineHelp.PluginDocDirectory, filename);
            try
            {
                if (!Directory.Exists(OnlineHelp.PluginDocDirectory))
                {
                    Directory.CreateDirectory(OnlineHelp.PluginDocDirectory);
                }

                var streamWriter = new System.IO.StreamWriter(filePath, false, Encoding.UTF8);
                streamWriter.Write(html);
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error trying to write file {0}! Message: {1}", filePath, ex.Message);
            }
        }

        private static void StoreIndexPage(string html, string filename)
        {
            var filePath = Path.Combine(OnlineHelp.HelpDirectory, filename);
            try
            {
                if (!Directory.Exists(OnlineHelp.HelpDirectory))
                {
                    Directory.CreateDirectory(OnlineHelp.HelpDirectory);
                }

                var streamWriter = new System.IO.StreamWriter(filePath, false, Encoding.UTF8);
                streamWriter.Write(html);
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error trying to write file {0}! Message: {1}", filePath, ex.Message);
            }
        }

        private static readonly Dictionary<string, string> _languagePresentationString = new Dictionary<string, string>() {{"en", "English"}, {"de-DE", "Deutsch"}};
        private static string GeneratePluginLanguageSelectionCode(Type pluginType, IEnumerable<string> availableLanguages, string lang)
        {
            var codeBuilder = new StringBuilder();

            foreach (var availableLanguage in availableLanguages)
            {
                if (availableLanguage == lang)
                {
                    codeBuilder.AppendLine(_languagePresentationString[lang]);
                }
                else
                {
                    codeBuilder.AppendLine(string.Format("<a href=\"{0}\">{1}</a>", OnlineHelp.GetPluginDocFilename(pluginType, availableLanguage), _languagePresentationString[availableLanguage]));
                }
                codeBuilder.AppendLine("|");
            }

            return codeBuilder.ToString();
        }

        private static string GenerateConnectorListCode(LocalizedPluginDocumentationPage localizedPluginDocumentationPage)
        {
            var codeBuilder = new StringBuilder();
            codeBuilder.AppendLine("<table border=\"1\">");
            codeBuilder.AppendLine(string.Format("<tr> <th>{0}</th> <th>{1}</th> </tr>", 
                Resources.HtmlGenerator_GenerateConnectorListCode_Name, 
                Resources.HtmlGenerator_GenerateConnectorListCode_Description));

            foreach (var pluginConnector in localizedPluginDocumentationPage.PluginConnectors)
            {
                codeBuilder.AppendLine(string.Format("<tr> <th>{0}</th> <th>{1}</th> </tr>", pluginConnector.Caption, pluginConnector.ToolTip));
            }

            codeBuilder.AppendLine("</table>");
            return codeBuilder.ToString();
        }

        private static string GenerateSettingsListCode(LocalizedPluginDocumentationPage localizedPluginDocumentationPage)
        {
            var codeBuilder = new StringBuilder();
            codeBuilder.AppendLine("<table border=\"1\">");
            codeBuilder.AppendLine(string.Format("<tr> <th>{0}</th> <th>{1}</th> <th>{2}</th> </tr>", 
                Resources.HtmlGenerator_GenerateConnectorListCode_Name, 
                Resources.HtmlGenerator_GenerateConnectorListCode_Description, 
                Resources.HtmlGenerator_GenerateSettingsListCode_Type));

            foreach (var setting in localizedPluginDocumentationPage.Settings)
            {
                codeBuilder.AppendLine(string.Format("<tr> <th>{0}</th> <th>{1}</th> <th>{2}</th> </tr>", setting.Caption, setting.ToolTip, setting.ControlType.ToString()));
            }

            codeBuilder.AppendLine("</table>");
            return codeBuilder.ToString();
        }

        private static string GenerateIndexLanguageSelectionCode(IEnumerable<string> availableLanguages, string lang)
        {
            var codeBuilder = new StringBuilder();

            foreach (var availableLanguage in availableLanguages)
            {
                if (availableLanguage == lang)
                {
                    codeBuilder.AppendLine(_languagePresentationString[lang]);
                }
                else
                {
                    codeBuilder.AppendLine(string.Format("<a href=\"{0}\">{1}</a>", OnlineHelp.GetIndexFilename(availableLanguage), _languagePresentationString[availableLanguage]));
                }
                codeBuilder.AppendLine("|");
            }

            return codeBuilder.ToString();
        }
    }
}
