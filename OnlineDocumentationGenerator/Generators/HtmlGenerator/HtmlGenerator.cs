using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using System.Linq;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Editor;
using OnlineDocumentationGenerator.DocInformations;

namespace OnlineDocumentationGenerator.Generators.HtmlGenerator
{
    public class HtmlGenerator : Generator
    {
        private ObjectConverter _objectConverter;

        public override void Generate()
        {
            _objectConverter = new ObjectConverter(DocPages, OutputDir);
            GenerateDocPages();
            GenerateIndexPages();
            CopyAdditionalResources();
        }

        private void GenerateIndexPages()
        {
            foreach (var lang in AvailableLanguages)
            {
                var indexHtml = TagReplacer.ReplaceLanguageSwitchs(Properties.Resources.TemplateIndex, lang);
                var languageSelectionCode = GenerateIndexLanguageSelectionCode(AvailableLanguages, lang);
                indexHtml = TagReplacer.ReplaceLanguageSelectionTag(indexHtml, languageSelectionCode);
                var componentListCode = GenerateComponentListCode(DocPages.FindAll(x => x is ComponentDocumentationPage), lang);
                indexHtml = TagReplacer.ReplaceComponentList(indexHtml, componentListCode);
                var editorListCode = GenerateEditorListCode(DocPages.FindAll(x => x is EditorDocumentationPage), lang);
                indexHtml = TagReplacer.ReplaceEditorList(indexHtml, editorListCode);

                var filename = OnlineHelp.GetIndexFilename(lang);
                StoreIndexPage(indexHtml, filename);
            }
        }

        private static string GenerateComponentListCode(IEnumerable<EntityDocumentationPage> componentDocumentationPages, string lang)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<table border=\"0\" cellspacing=\"3\" cellpadding=\"3\">");

            var anchorBuilder = new StringBuilder();
            anchorBuilder.Append("<p>");

            var query = from pages in componentDocumentationPages
                        orderby pages.Localizations[pages.Localizations.ContainsKey(lang) ? lang : "en"].Name
                        select pages;
            
            char actualIndexCharacter = ' ';
            foreach (var page in query)
            {        
                
                var linkedLang = page.Localizations.ContainsKey(lang) ? lang : "en";
                var pp = page.Localizations[linkedLang];
                if (actualIndexCharacter != pp.Name[0])
                {
                    actualIndexCharacter = pp.Name.ToUpper()[0];
                    stringBuilder.AppendLine(string.Format("<tr><td><h2 id=\"{0}\">{0}</h2></td><td></td></tr>", actualIndexCharacter));
                    anchorBuilder.AppendLine(string.Format("<a href=\"#{0}\"><b>{0}</b><a>&nbsp;", actualIndexCharacter));
                }
                stringBuilder.AppendLine(string.Format("<tr><td><a href=\"{0}\">{1}</a></td><td>{2}</td></tr>",
                    OnlineHelp.GetDocFilename(pp.Type, linkedLang), pp.Name, pp.ToolTip));
            }
            stringBuilder.AppendLine("</table>");

            anchorBuilder.Append("</p>");
            anchorBuilder.Append(stringBuilder);
            return anchorBuilder.ToString();
        }

        private static string GenerateEditorListCode(IEnumerable<EntityDocumentationPage> editorDocumentationPages, string lang)
        {
            var stringBuilderListCode = new StringBuilder();
            stringBuilderListCode.AppendLine("<table border=\"0\" cellspacing=\"3\" cellpadding=\"3\">");

            var query = from pages in editorDocumentationPages
                        orderby pages.Localizations[pages.Localizations.ContainsKey(lang) ? lang : "en"].Name
                        select pages;
                        
            foreach (var page in query)
            {
                var linkedLang = page.Localizations.ContainsKey(lang) ? lang : "en";
                var pp = page.Localizations[linkedLang];
                stringBuilderListCode.AppendLine(string.Format("<tr><td><a href=\"{0}\">{1}</a></td><td>{2}</td></tr>",
                    OnlineHelp.GetDocFilename(pp.Type, linkedLang), pp.Name, pp.ToolTip));
            }
            stringBuilderListCode.AppendLine("</table>");

            return stringBuilderListCode.ToString();
        }

        private static string GetDocumentationTemplate(Type entityType)
        {
            if (entityType.GetInterfaces().Contains(typeof(IEditor)))
            {
                return Properties.Resources.TemplateEditorDocumentationPage;
            }
            else
            {
                return Properties.Resources.TemplateComponentDocumentationPage;
            }
        }

        private void GenerateDocPages()
        {
            foreach (var documentationPage in DocPages)
            {
                foreach (var lang in documentationPage.AvailableLanguages)
                {
                    var localizedEntityDocumentationPage = documentationPage.Localizations[lang];

                    var cultureInfo = new CultureInfo(lang);
                    Thread.CurrentThread.CurrentCulture = cultureInfo;
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;

                    var html = TagReplacer.ReplaceLanguageSwitchs(GetDocumentationTemplate(documentationPage.EntityType), lang);
                    html = TagReplacer.ReplaceDocItemTags(html, localizedEntityDocumentationPage, _objectConverter);
                    var languageSelectionCode = GenerateLanguageSelectionCode(documentationPage.EntityType, documentationPage.AvailableLanguages, lang);
                    html = TagReplacer.ReplaceLanguageSelectionTag(html, languageSelectionCode);

                    var filename = OnlineHelp.GetDocFilename(documentationPage.EntityType, lang);
                    StoreDocPage(html, filename);
                }
            }
        }

        private void StoreDocPage(string html, string filename)
        {
            var filePath = Path.Combine(OutputDir, Path.Combine(OnlineHelp.HelpDirectory, filename));
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                var streamWriter = new System.IO.StreamWriter(filePath, false, Encoding.UTF8);
                streamWriter.Write(html);
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error trying to write file {0}! Message: {1}", filePath, ex.Message));
            }
        }

        private void StoreIndexPage(string html, string filename)
        {
            var filePath = Path.Combine(OutputDir, Path.Combine(OnlineHelp.HelpDirectory, filename));
            try
            {
                if (!Directory.Exists(Path.Combine(OutputDir, OnlineHelp.HelpDirectory)))
                {
                    Directory.CreateDirectory(Path.Combine(OutputDir, OnlineHelp.HelpDirectory));
                }

                var streamWriter = new System.IO.StreamWriter(filePath, false, Encoding.UTF8);
                streamWriter.Write(html);
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error trying to write file {0}! Message: {1}", filePath, ex.Message));
            }
        }

        private static readonly Dictionary<string, string> _languagePresentationString = new Dictionary<string, string>() {{"en", "English"}, {"de-DE", "Deutsch"}};
        private static string GenerateLanguageSelectionCode(Type type, IEnumerable<string> availableLanguages, string lang)
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
                    var filename = Path.GetFileName(OnlineHelp.GetDocFilename(type, availableLanguage));
                    codeBuilder.AppendLine(string.Format("<a href=\"{0}\">{1}</a>", filename, _languagePresentationString[availableLanguage]));
                }
                codeBuilder.AppendLine("|");
            }

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

        private void CopyAdditionalResources()
        {
            var additionalResources = XElement.Parse(Properties.Resources.AdditionalResources);
            foreach (var r in additionalResources.Elements("file"))
            {
                try
                {
                    var path = r.Attribute("path").Value;
                    int sIndex = path.IndexOf('/');
                    var resUri = new Uri(string.Format("pack://application:,,,/{0};component/{1}",
                                                       path.Substring(0, sIndex), path.Substring(sIndex + 1)));
                    var fileName = Path.Combine(OutputDir, Path.Combine(OnlineHelp.HelpDirectory, Path.GetFileName(path)));
                    
                    using (var resStream = Application.GetResourceStream(resUri).Stream)
                    using (var streamWriter = new System.IO.StreamWriter(fileName, false))
                    {
                        resStream.CopyTo(streamWriter.BaseStream);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error trying to copy additional resource: {0}", ex.Message));
                }
            }
        }
    }
}
