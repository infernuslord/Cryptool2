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
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Editor;
using Cryptool.PluginBase.Miscellaneous;
using Ionic.Zip;
using OnlineDocumentationGenerator.DocInformations;
using OnlineDocumentationGenerator.DocInformations.Localization;

namespace OnlineDocumentationGenerator.Generators.HtmlGenerator
{
    public class HtmlGenerator : Generator
    {
        private ObjectConverter _objectConverter;
        private TemplateDirectory _templatesDir;

        public override void Generate(TemplateDirectory templatesDir)
        {
            _templatesDir = templatesDir;
            _objectConverter = new ObjectConverter(DocPages, OutputDir);
            GenerateDocPages();
            GenerateComponentIndexPages();
            GenerateTemplateIndexPages();
            GenerateEditorIndexPages();
            GenerateCommonIndexPages();
            CopyAdditionalResources();
        }
       
        private void GenerateComponentIndexPages()
        {
            foreach (var lang in AvailableLanguages)
            {
                var cultureInfo = new CultureInfo(lang);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                var indexHtml = TagReplacer.ReplaceLanguageSwitchs(Properties.Resources.TemplateComponentsIndex, lang);
                indexHtml = TagReplacer.ReplaceInstallVersionSwitchs(indexHtml, AssemblyHelper.InstallationType);
                var languageSelectionCode = GenerateIndexLanguageSelectionCode(AvailableLanguages, lang);
                indexHtml = TagReplacer.ReplaceLanguageSelectionTag(indexHtml, languageSelectionCode);
                var componentListCode = GenerateComponentListCode(DocPages.FindAll(x => x is ComponentDocumentationPage).Select(x => (ComponentDocumentationPage)x), lang);
                indexHtml = TagReplacer.ReplaceComponentList(indexHtml, componentListCode);
                var componentTreeCode = GenerateComponentTreeCode(DocPages.FindAll(x => x is ComponentDocumentationPage).Select(x => (ComponentDocumentationPage)x), lang);
                indexHtml = TagReplacer.ReplaceComponentTree(indexHtml, componentTreeCode);

                var filename = OnlineHelp.GetComponentIndexFilename(lang);
                StoreIndexPage(indexHtml, filename);
            }
        }

        private void GenerateTemplateIndexPages()
        {
            foreach (var lang in AvailableLanguages)
            {
                var cultureInfo = new CultureInfo(lang);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                var templatesHtml = TagReplacer.ReplaceLanguageSwitchs(Properties.Resources.TemplateTemplatesIndex, lang);
                var languageSelectionCode = GenerateTemplatesPageLanguageSelectionCode(AvailableLanguages, lang);
                templatesHtml = TagReplacer.ReplaceLanguageSelectionTag(templatesHtml, languageSelectionCode);
                var templatesListCode = GenerateTemplatesTree(lang);
                templatesHtml = TagReplacer.ReplaceTemplatesList(templatesHtml, templatesListCode);

                var filename = OnlineHelp.GetTemplatesIndexFilename(lang);
                StoreIndexPage(templatesHtml, filename);
            }
        }

        private void GenerateEditorIndexPages()
        {
            foreach (var lang in AvailableLanguages)
            {
                var cultureInfo = new CultureInfo(lang);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                var indexHtml = TagReplacer.ReplaceLanguageSwitchs(Properties.Resources.TemplateEditorIndex, lang);
                indexHtml = TagReplacer.ReplaceInstallVersionSwitchs(indexHtml, AssemblyHelper.InstallationType);
                var languageSelectionCode = GenerateIndexLanguageSelectionCode(AvailableLanguages, lang);
                indexHtml = TagReplacer.ReplaceLanguageSelectionTag(indexHtml, languageSelectionCode);
                var editorListCode = GenerateEditorListCode(DocPages.FindAll(x => x is EditorDocumentationPage).Select(x => (EditorDocumentationPage)x), lang);
                indexHtml = TagReplacer.ReplaceEditorList(indexHtml, editorListCode);

                var filename = OnlineHelp.GetEditorIndexFilename(lang);
                StoreIndexPage(indexHtml, filename);
            }
        }

        private void GenerateCommonIndexPages()
        {
            foreach (var lang in AvailableLanguages)
            {
                var cultureInfo = new CultureInfo(lang);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                var commonHtml = TagReplacer.ReplaceLanguageSwitchs(Properties.Resources.TemplateCommonIndex, lang);
                var languageSelectionCode = GenerateTemplatesPageLanguageSelectionCode(AvailableLanguages, lang);
                commonHtml = TagReplacer.ReplaceLanguageSelectionTag(commonHtml, languageSelectionCode);
                var commonListCode = GenerateCommonListCode(DocPages.FindAll(x => x is CommonDocumentationPage).Select(x => (CommonDocumentationPage)x), lang);
                commonHtml = TagReplacer.ReplaceCommonList(commonHtml, commonListCode);
                //var templatesListCode = GenerateTemplatesTree(lang);
                //templatesHtml = TagReplacer.ReplaceTemplatesList(templatesHtml, templatesListCode);

                var filename = OnlineHelp.GetCommonIndexFilename(lang);
                StoreIndexPage(commonHtml, filename);
            }
        }

        private string GenerateTemplatesTree(string lang)
        {            
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(@"
                                <script type='text/javascript'>
			                    <!--  
                                function ShowHideDiv(divName) {
                                if (document.getElementById(divName)) {
                                    document.getElementById(divName).style.display = (document.getElementById(divName).style.display == 'none') ? 'inline' : 'none';
                                    }
                                }
                                </script>");            
            var anchorBuilder = new StringBuilder();
            anchorBuilder.Append("<p>");
            foreach (var dir in _templatesDir.SubDirectories)
            {
                WalkTemplateDirectory(dir, stringBuilder, 0, lang);
            }
            anchorBuilder.Append("</p>");
            anchorBuilder.Append(stringBuilder);
            return anchorBuilder.ToString();
        }

        /// <summary>
        /// Unique id for div of template tables
        /// </summary>
        private int _uid;

        private void WalkTemplateDirectory(TemplateDirectory templatesDir, StringBuilder stringBuilder, int depth, string lang)
        {
            var spacesStringBuilder = new StringBuilder();
            for(int i=0;i<depth*2;i++)
            {
                spacesStringBuilder.Append("&nbsp;");
            }
            _uid++;
            var id = "ID_" + _uid;
            var spaces = spacesStringBuilder.ToString();            
            stringBuilder.AppendLine("<table width=\"100%\" border=\"0\" cellspacing=\"3\" cellpadding=\"3\" >");
            stringBuilder.AppendLine(string.Format("<tr><td colspan=\"4\">{0}<a href=\"#\" onclick=\"ShowHideDiv('{1}'); return false;\">{2}</a></td></tr>", spaces, id, templatesDir.GetName()));
            stringBuilder.AppendLine("</table>");
            stringBuilder.AppendLine(string.Format("<div style=\"display:none;\" id=\"{0}\">", id));
            stringBuilder.AppendLine("<table width=\"100%\" border=\"0\" cellspacing=\"3\" cellpadding=\"3\">");         
            foreach (var templateDocumentationPage in templatesDir.ContainingTemplateDocPages)
            {
                var locTemplate = templateDocumentationPage.CurrentLocalization;
                var description = _objectConverter.Convert(locTemplate.SummaryOrDescription, templateDocumentationPage);
                description = description.Replace("../", ""); //correct relative paths in images                
                stringBuilder.AppendLine(string.Format("<tr><td>{0}&nbsp;</td><td><div class=\"boximage\"><img src=\"{1}\"></div></td><td><a href=\"{2}\">{3}</a></td><td>{4}</td></tr>", spaces, templateDocumentationPage.Icon, locTemplate.FilePath, locTemplate.Name, description));                
            }            
            foreach (var dir in templatesDir.SubDirectories)
            {
                stringBuilder.AppendLine("<tr><td colspan=\"4\">");
                WalkTemplateDirectory(dir, stringBuilder, depth + 1,lang);
                stringBuilder.AppendLine("</td></tr>");
            }
            stringBuilder.AppendLine("</table>");
            stringBuilder.AppendLine("</div>");
        }

        private static string GenerateComponentListCode(IEnumerable<ComponentDocumentationPage> componentDocumentationPages, string lang)
        {
            var stringBuilder = new StringBuilder();            
            stringBuilder.AppendLine("<table width=\"100%\" border=\"0\" cellspacing=\"3\" cellpadding=\"3\" class=\"filterable\">");

            var anchorBuilder = new StringBuilder();
            anchorBuilder.Append("<p>");

            var query = from pages in componentDocumentationPages
                        orderby pages.Localizations[pages.Localizations.ContainsKey(lang) ? lang : "en"].Name
                        select pages;
            
            char actualIndexCharacter = ' ';
            foreach (var page in query)
            {        
                
                var linkedLang = page.Localizations.ContainsKey(lang) ? lang : "en";
                var pp = (LocalizedComponentDocumentationPage)page.Localizations[linkedLang];
                if (actualIndexCharacter != pp.Name[0])
                {
                    actualIndexCharacter = pp.Name.ToUpper()[0];
                    stringBuilder.AppendLine(string.Format("<tr><td><h2 id=\"{0}\">{0}</h1></td><td></td></tr>", actualIndexCharacter));
                    anchorBuilder.AppendLine(string.Format("<a href=\"#{0}\"><b>{0}</b><a>&nbsp;", actualIndexCharacter));
                }
                stringBuilder.AppendLine(string.Format("<tr><td><a href=\"{0}\">{1}</a></td><td>{2}</td></tr>",
                    OnlineHelp.GetPluginDocFilename(pp.PluginType, linkedLang), pp.Name, pp.ToolTip));
            }
            stringBuilder.AppendLine("</table>");
            stringBuilder.AppendLine("<script type=\"text/javascript\" src=\"filterTable.js\"></script>");

            anchorBuilder.Append("</p>");
            anchorBuilder.Append(stringBuilder);
            return anchorBuilder.ToString();
        }

        private static string GenerateComponentTreeCode(IEnumerable<PluginDocumentationPage> componentDocumentationPages, string lang)        
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<table width=\"100%\" border=\"0\" cellspacing=\"3\" cellpadding=\"3\">");

            var anchorBuilder = new StringBuilder();
            anchorBuilder.Append("<p>");

            var query = from pages in componentDocumentationPages
                        orderby pages.Category
                        select pages;

            ComponentCategory actualCategory = ComponentCategory.Undefined;
            foreach (var page in query)
            {

                var linkedLang = page.Localizations.ContainsKey(lang) ? lang : "en";
                var pp = (LocalizedComponentDocumentationPage)page.Localizations[linkedLang];

                
                if (actualCategory != page.Category)
                {                    
                    actualCategory = page.Category;
                    string categoryName = null;
                    switch (page.Category)
                    {
                        case ComponentCategory.CiphersClassic:
                            categoryName = Properties.Resources.Classic_Ciphers;
                            break;
                        case ComponentCategory.CiphersModernSymmetric:
                            categoryName = Properties.Resources.CiphersModernSymmetric;
                            break;
                        case ComponentCategory.CiphersModernAsymmetric:
                            categoryName = Properties.Resources.CiphersModernAsymmetric;
                            break;
                        case ComponentCategory.Steganography:
                            categoryName = Properties.Resources.Steganography;
                            break;
                        case ComponentCategory.HashFunctions:
                            categoryName = Properties.Resources.HashFunctions;
                            break;
                        case ComponentCategory.CryptanalysisSpecific:
                            categoryName = Properties.Resources.CryptanalysisSpecific;
                            break;
                        case ComponentCategory.CryptanalysisGeneric:
                            categoryName = Properties.Resources.CryptanalysisGeneric;
                            break;
                        case ComponentCategory.Protocols:
                            categoryName = Properties.Resources.Protocols;
                            break;
                        case ComponentCategory.ToolsBoolean:
                            categoryName = Properties.Resources.ToolsBoolean;
                            break;
                        case ComponentCategory.ToolsDataflow:
                            categoryName = Properties.Resources.ToolsDataflow;
                            break;
                        case ComponentCategory.ToolsDataInputOutput:
                            categoryName = Properties.Resources.ToolsDataInputOutput;
                            break;
                        case ComponentCategory.ToolsMisc:
                            categoryName = Properties.Resources.ToolsMisc;
                            break;
                        case ComponentCategory.ToolsP2P:
                            categoryName = Properties.Resources.ToolsP2P;
                            break;
                        default:
                            categoryName = Properties.Resources.Unknown_Category;
                            break;
                    }
                    stringBuilder.AppendLine(string.Format("<tr><td><h2 id=\"{0}\">{0}</h1></td><td></td></tr>", categoryName));
                    anchorBuilder.AppendLine(string.Format("<a href=\"#{0}\"><b>{0}</b><a>&nbsp;", categoryName));
                }
                stringBuilder.AppendLine(string.Format("<tr><td><a href=\"{0}\">{1}</a></td><td>{2}</td></tr>",
                    OnlineHelp.GetPluginDocFilename(pp.PluginType, linkedLang), pp.Name, pp.ToolTip));
            }
            stringBuilder.AppendLine("</table>");
            anchorBuilder.Append("</p>");
            anchorBuilder.Append(stringBuilder);
            return anchorBuilder.ToString();
        }

        private static string GenerateEditorListCode(IEnumerable<PluginDocumentationPage> editorDocumentationPages, string lang)
        {
            var stringBuilderListCode = new StringBuilder();
            stringBuilderListCode.AppendLine("<table width=\"100%\"  border=\"0\" cellspacing=\"3\" cellpadding=\"3\">");

            var query = from pages in editorDocumentationPages
                        orderby pages.Localizations[pages.Localizations.ContainsKey(lang) ? lang : "en"].Name
                        select pages;
                        
            foreach (var page in query)
            {
                var linkedLang = page.Localizations.ContainsKey(lang) ? lang : "en";
                var pp = (LocalizedEditorDocumentationPage)page.Localizations[linkedLang];
                stringBuilderListCode.AppendLine(string.Format("<tr><td><a href=\"{0}\">{1}</a></td><td>{2}</td></tr>",
                    OnlineHelp.GetPluginDocFilename(pp.PluginType, linkedLang), pp.Name, pp.ToolTip));
            }
            stringBuilderListCode.AppendLine("</table>");

            return stringBuilderListCode.ToString();
        }

        private static string GenerateCommonListCode(IEnumerable<CommonDocumentationPage> commonDocumentationPages, string lang)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<table width=\"100%\" border=\"0\" cellspacing=\"3\" cellpadding=\"3\">");

            var anchorBuilder = new StringBuilder();
            anchorBuilder.Append("<p>");

            var query = from pages in commonDocumentationPages
                        orderby pages.Localizations[pages.Localizations.ContainsKey(lang) ? lang : "en"].Name
                        select pages;

            char actualIndexCharacter = ' ';
            foreach (var page in query)
            {

                var linkedLang = page.Localizations.ContainsKey(lang) ? lang : "en";
                var pp = (LocalizedCommonDocumentationPage)page.Localizations[linkedLang];
                //if (actualIndexCharacter != pp.Name[0])
                //{
                //    actualIndexCharacter = pp.Name.ToUpper()[0];
                //    stringBuilder.AppendLine(string.Format("<tr><td><h2 id=\"{0}\">{0}</h1></td><td></td></tr>", actualIndexCharacter));
                //    anchorBuilder.AppendLine(string.Format("<a href=\"#{0}\"><b>{0}</b><a>&nbsp;", actualIndexCharacter));
                //}
                stringBuilder.AppendLine(string.Format("<tr><td><a href=\"{0}\">{1}</a></td></tr>",
                    OnlineHelp.GetCommonDocFilename(page.Name, linkedLang), pp.Name));
            }
            stringBuilder.AppendLine("</table>");
            //stringBuilder.AppendLine("<script type=\"text/javascript\" src=\"filterTable.js\"></script>");

            anchorBuilder.Append("</p>");
            anchorBuilder.Append(stringBuilder);
            return anchorBuilder.ToString();
        }

        private static string GetDocumentationTemplate(EntityDocumentationPage entityDocPage)
        {
            if (entityDocPage is EditorDocumentationPage)
            {
                return Properties.Resources.TemplateEditorDocumentationPage;
            }
            if (entityDocPage is ComponentDocumentationPage)
            {
                return Properties.Resources.TemplateComponentDocumentationPage;
            }
            if (entityDocPage is TemplateDocumentationPage)
            {
                return Properties.Resources.TemplateTemplateDocumentationPage;
            }
            if (entityDocPage is CommonDocumentationPage)
            {
                return Properties.Resources.TemplateCommonDocumentationPage;
            }
            throw new Exception(string.Format("Unknown documentation page type {0}!", entityDocPage.GetType()));
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

                    var html = TagReplacer.ReplaceLanguageSwitchs(GetDocumentationTemplate(documentationPage), lang);
                    html = TagReplacer.ReplaceDocItemTags(html, localizedEntityDocumentationPage, _objectConverter);
                    var languageSelectionCode = GenerateLanguageSelectionCode(documentationPage, documentationPage.AvailableLanguages, lang);
                    html = TagReplacer.ReplaceLanguageSelectionTag(html, languageSelectionCode);
                    var localizedComponentDocumentationPage = localizedEntityDocumentationPage as LocalizedComponentDocumentationPage;
                    if(localizedComponentDocumentationPage != null)
                    {
                        html = TagReplacer.ReplaceSectionSwitchs(html, localizedComponentDocumentationPage);
                    }
                    var filename = documentationPage.Localizations[lang].FilePath;
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

        private static readonly Dictionary<string, string> _languagePresentationString = new Dictionary<string, string>() {{"en", "English"}, {"de", "Deutsch"}};
        private static readonly Dictionary<string, string> _languagePresentationIcon = new Dictionary<string, string>() { { "en", "en.png" }, { "de", "de.png" } };

        private static string GenerateLanguageSelectionCode(EntityDocumentationPage entityDocumentationPage, IEnumerable<string> availableLanguages, string lang)
        {
            var codeBuilder = new StringBuilder();

            foreach (var availableLanguage in availableLanguages)
            {
                var iconPath = "";
                for (int i = 0; i < entityDocumentationPage.DocDirPath.Split(Path.PathSeparator).Count(); i++)
                {
                    iconPath = Path.Combine("..", iconPath);
                }
                iconPath = Path.Combine(iconPath, _languagePresentationIcon[availableLanguage]);
                
                if (availableLanguage == lang)
                {
                    codeBuilder.AppendLine(string.Format("<img src=\"{1}\" border=\"0\"/>&nbsp;{0}", _languagePresentationString[lang], iconPath));
                }
                else
                {
                    var filename = Path.GetFileName(entityDocumentationPage.Localizations[availableLanguage].FilePath);
                    codeBuilder.AppendLine(string.Format("<a href=\"{0}\"><img src=\"{2}\" border=\"0\"/>&nbsp;{1}</a>", filename, _languagePresentationString[availableLanguage], iconPath));
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
                    codeBuilder.AppendLine(string.Format("<img src=\"{1}\" border=\"0\"/>&nbsp;{0}", _languagePresentationString[lang], _languagePresentationIcon[availableLanguage]));
                }
                else
                {
                    codeBuilder.AppendLine(string.Format("<a href=\"{0}\"><img src=\"{2}\" border=\"0\"/>&nbsp;{1}</a>", OnlineHelp.GetComponentIndexFilename(availableLanguage), _languagePresentationString[availableLanguage], _languagePresentationIcon[availableLanguage]));
                }
                codeBuilder.AppendLine("|");
            }

            return codeBuilder.ToString();
        }

        private static string GenerateTemplatesPageLanguageSelectionCode(IEnumerable<string> availableLanguages, string lang)
        {
            var codeBuilder = new StringBuilder();

            foreach (var availableLanguage in availableLanguages)
            {
                if (availableLanguage == lang)
                {
                    codeBuilder.AppendLine(string.Format("<img src=\"{1}\" border=\"0\"/>&nbsp;{0}", _languagePresentationString[lang], _languagePresentationIcon[availableLanguage]));
                }
                else
                {
                    codeBuilder.AppendLine(string.Format("<a href=\"{0}\"><img src=\"{2}\" border=\"0\"/>&nbsp;{1}</a>", OnlineHelp.GetTemplatesIndexFilename(availableLanguage), _languagePresentationString[availableLanguage], _languagePresentationIcon[availableLanguage]));
                }
                codeBuilder.AppendLine("|");
            }

            return codeBuilder.ToString();
        }

        private void CopyAdditionalResources()
        {
            bool developer = AssemblyHelper.InstallationType == Ct2InstallationType.Developer;

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

            foreach (var r in additionalResources.Elements("archive"))
            {
                var excl = r.Attribute("excludeDeveloper");
                if (!developer || excl == null || excl.Value.ToLower() == "false")
                {
                    try
                    {
                        var path = r.Attribute("path").Value;
                        int sIndex = path.IndexOf('/');
                        var resUri = new Uri(string.Format("pack://application:,,,/{0};component/{1}",
                                                           path.Substring(0, sIndex), path.Substring(sIndex + 1)));

                        //Extract archive:
                        using (var resStream = Application.GetResourceStream(resUri).Stream)
                        using (var zipPackage = ZipFile.Read(resStream))
                        {
                            zipPackage.ExtractAll(OnlineHelp.HelpDirectory, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Error trying to copy additional resource archive: {0}", ex.Message));
                    }
                }
            }
        }
    }
}
