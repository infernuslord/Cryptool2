using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cryptool.PluginBase;

namespace OnlineDocumentationGenerator.Generators.HtmlGenerator
{
    public class HtmlGenerator : Generator
    {
        private readonly string _htmlTemplate = Properties.Resources.TemplatePluginDocumentationPage;

        public override void Generate()
        {
            foreach (var pluginDocumentationPage in pluginPages)
            {
                foreach (var lang in pluginDocumentationPage.AvailableLanguages)
                {
                    var localizedPluginDocumentationPage = pluginDocumentationPage.Localizations[lang];

                    var html = TagReplacer.ReplaceLanguageSwitchs(_htmlTemplate, lang);
                    html = TagReplacer.ReplacePluginDocTags(html, localizedPluginDocumentationPage);
                    var languageSelectionCode = GenerateLanguageSelectionCode(pluginDocumentationPage.PluginType, pluginDocumentationPage.AvailableLanguages, lang);
                    html = TagReplacer.ReplaceLanguageSelectionTag(html, languageSelectionCode);

                    var filename = OnlineHelp.GetPluginDocFilename(pluginDocumentationPage.PluginType, lang);
                    StorePluginDocPage(html, filename);
                }
            }

            //TODO: Generate index page!
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

        private static Dictionary<string, string> _languagePresentationString = new Dictionary<string, string>() {{"en", "English"}, {"de-DE", "Deutsch"}};
        private static string GenerateLanguageSelectionCode(Type pluginType, List<string> availableLanguages, string lang)
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
    }
}
