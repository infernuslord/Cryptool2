using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Miscellaneous;

namespace OnlineDocumentationGenerator.Generators.LaTeXGenerator
{
    public class LaTeXGenerator : Generator
    {
        public static readonly string HelpDirectory = "LaTeXDoc";

        private ObjectConverter _objectConverter;
        private TemplateDirectory _templatesDir;
        private readonly string _lang;
        private readonly bool _noIcons;
        private readonly bool _showAuthors;

        public LaTeXGenerator(string lang, bool noIcons, bool showAuthors)
        {
            _lang = lang;
            _noIcons = noIcons;
            _showAuthors = showAuthors;
        }

        public override void Generate(TemplateDirectory templatesDir)
        {
            var cultureInfo = new CultureInfo(_lang);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo; 

            _templatesDir = templatesDir;
            _objectConverter = new ObjectConverter(DocPages, OutputDir);
            var tableCode = GenerateTemplateOverviewTableCode(_lang);
            var descriptionCode = GenerateTemplateDescriptionCode(_lang);

            var latexCode = Properties.Resources.LaTeXTemplate.Replace("$CONTENT$", string.Format("{0}\n{1}", tableCode, descriptionCode));
            var versionString = GetVersion();
            latexCode = latexCode.Replace("$VERSION$", versionString);
            StoreLaTeX(latexCode, "templates-"+_lang+".tex");
        }

        private string GetVersion()
        {
            switch (AssemblyHelper.BuildType)
            {
                case Ct2BuildType.Developer:
                    return "Developer " + AssemblyHelper.Version;
                case Ct2BuildType.Nightly:
                    return "Nightly Build " + AssemblyHelper.Version;
                case Ct2BuildType.Beta:
                    return "Beta " + AssemblyHelper.Version;
                case Ct2BuildType.Stable:
                    return "Stable " + AssemblyHelper.Version;
            }
            return AssemblyHelper.Version.ToString();
        }

        private string GenerateTemplateOverviewTableCode(string lang)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("\\chapter*{" + OnlineDocumentationGenerator.Properties.Resources.LatexGenerator_ChapterTitle + "}");
            stringBuilder.AppendLine("\\addcontentsline{toc}{chapter}{" + OnlineDocumentationGenerator.Properties.Resources.LatexGenerator_ChapterTitle + "}");
            stringBuilder.AppendLine("\\begin{longtable}{lp{0.6\\textwidth}}");
            
            foreach (var dir in _templatesDir.SubDirectories)
            {
                GenerateTemplateOverviewTableSection(dir, stringBuilder, 0, lang);
            }

            stringBuilder.AppendLine("\\end{longtable}");
            return stringBuilder.ToString();
        }

        private void GenerateTemplateOverviewTableSection(TemplateDirectory templatesDir, StringBuilder stringBuilder, int depth, string lang)
        {
            const string hspace = "\\hspace{3mm} ";
            var spaces = (depth>0) ? String.Format("\\hspace{{{0}mm}} ", depth*6) : "";

            stringBuilder.AppendLine("\\multicolumn{2}{l}{" + spaces + " \\textbf{" + Helper.EscapeLaTeX(templatesDir.GetName(lang)) + "}} \\\\");

            Boolean itemadded = false;

            foreach (var templateDocumentationPage in templatesDir.ContainingTemplateDocPages)
            {
                var locTemplate = templateDocumentationPage.CurrentLocalization;
                var summary = _objectConverter.Convert(locTemplate.Summary, templateDocumentationPage);
                string includeIcon = "";
                if (locTemplate.Icon != null)
                {
                    var icon = _objectConverter.GetImagePath(locTemplate.Icon, templateDocumentationPage.Name);
                    includeIcon = "\\includegraphics[width=16pt, height=16pt]{" + icon + "}";
                }

                var templateName = Helper.EscapeLaTeX(templateDocumentationPage.CurrentLocalization.Name);
                var templateNameMiniPage = "\\begin{minipage}[c]{0.3\\textwidth}" + templateName + "\\end{minipage}";
                stringBuilder.AppendLine(string.Format("{0} {1} {2} & {3} \\\\", spaces + hspace, includeIcon, templateNameMiniPage, summary));

                itemadded = true;
            }

            if( itemadded ) stringBuilder.AppendLine("\\\\");

            foreach (var dir in templatesDir.SubDirectories)
            {
                GenerateTemplateOverviewTableSection(dir, stringBuilder, depth + 1, lang);
            }
        }

        private string SectionFromDepth(int depth)
        {
            if (depth == 0) 
                return "\\chapter";

            if (depth < 4)
                return "\\" + string.Join("", Enumerable.Repeat("sub", depth-1)) + "section";

            return "\\paragraph";
        }

        private void GenerateTemplateDescriptionSection(TemplateDirectory templatesDir, StringBuilder stringBuilder, int depth, string lang)
        {
            stringBuilder.AppendLine(string.Format("{0}{{{1}}}", SectionFromDepth(depth), Helper.EscapeLaTeX(templatesDir.GetName(lang))));

            foreach (var templateDocumentationPage in templatesDir.ContainingTemplateDocPages)
            {
                var locTemplate = templateDocumentationPage.CurrentLocalization;

                var description = _objectConverter.Convert(locTemplate.Description, templateDocumentationPage);
                description = Regex.Replace(description, "[\r\n]+", "\n");

                var templateName = Helper.EscapeLaTeX(templateDocumentationPage.CurrentLocalization.Name);
                stringBuilder.AppendLine(string.Format("{0}{{{1}}}", SectionFromDepth(depth+1), templateName));
                stringBuilder.AppendLine(description);
                if (_showAuthors)
                {
                    var author = _objectConverter.Convert(locTemplate.AuthorName, templateDocumentationPage);
                    stringBuilder.AppendLine("");
                    stringBuilder.AppendLine("Author: " + author);
                    stringBuilder.AppendLine("");
                }
                if (!_noIcons)
                {
                    var icon = _objectConverter.Convert(locTemplate.Icon, templateDocumentationPage);
                    stringBuilder.AppendLine(icon);
                }
            }
            foreach (var dir in templatesDir.SubDirectories)
            {
                GenerateTemplateDescriptionSection(dir, stringBuilder, depth + 1,lang);
            }
        }

        private string GenerateTemplateDescriptionCode(string lang)
        {
            var stringBuilder = new StringBuilder();
            //stringBuilder.AppendLine("\\chapter{" + OnlineDocumentationGenerator.Properties.Resources.LatexGenerator_ChapterSubTitle + "}");
            bool first = true;
            foreach (var dir in _templatesDir.SubDirectories)
            {
                if (!first) stringBuilder.AppendLine("\\newpage");
                first = false;
                GenerateTemplateDescriptionSection(dir, stringBuilder, 0, lang);
            }
            return stringBuilder.ToString();
        }
        
        private void StoreLaTeX(string content, string filename)
        {
            var outDir = Path.Combine(OutputDir, HelpDirectory);
            var filePath = Path.Combine(outDir, filename);
            try
            {
                if (!Directory.Exists(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }

                var streamWriter = new StreamWriter(filePath, false, Encoding.UTF8);
                streamWriter.Write(content);
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error trying to write file {0}! Message: {1}", filePath, ex.Message));
            }
        }
    }
}
