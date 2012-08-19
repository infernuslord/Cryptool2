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
using Cryptool.PluginBase.Miscellaneous;
using Ionic.Zip;
using OnlineDocumentationGenerator.DocInformations;
using OnlineDocumentationGenerator.DocInformations.Localization;

namespace OnlineDocumentationGenerator.Generators.LaTeXGenerator
{
    public class LaTeXGenerator : Generator
    {
        public static readonly string HelpDirectory = "LaTeXDoc";

        private ObjectConverter _objectConverter;
        private TemplateDirectory _templatesDir;
        private string _lang;

        public LaTeXGenerator(string lang)
        {
            _lang = lang;
        }

        public override void Generate(TemplateDirectory templatesDir)
        {
            _templatesDir = templatesDir;
            _objectConverter = new ObjectConverter(DocPages, OutputDir);
            var code = GenerateTemplateCode(_lang);
            StoreLaTeX(code, "templates.tex");
            
            //CopyAdditionalResources();
        }

        private void WalkTemplateDirectory(TemplateDirectory templatesDir, StringBuilder stringBuilder, int depth, string lang)
        {
            var sectionBuilder = new StringBuilder("\\");
            for(int i=0;i<depth;i++)
            {
                sectionBuilder.Append("sub");
            }

            string templateSection;
            if (depth < 2)
            {
                var templateSectionBuilder = new StringBuilder(sectionBuilder.ToString());
                templateSectionBuilder.Append("subsection");
                templateSection = templateSectionBuilder.ToString();
            }
            else
            {
                templateSection = "\\paragraph";
            }
            sectionBuilder.Append("section");

            var section = sectionBuilder.ToString();
            stringBuilder.AppendLine(string.Format("{0}{1}", section, "{" + Helper.EscapeLaTeX(templatesDir.GetName(lang)) + "}"));

            foreach (var templateDocumentationPage in templatesDir.ContainingTemplateDocPages)
            {
                var locTemplate = templateDocumentationPage.CurrentLocalization;
                //var description = _objectConverter.Convert(locTemplate.SummaryOrDescription, templateDocumentationPage);
                var description = _objectConverter.Convert(locTemplate.Description, templateDocumentationPage);

                var templateName = Helper.EscapeLaTeX(templateDocumentationPage.CurrentLocalization.Name);
                stringBuilder.AppendLine(string.Format("{0}{1}", templateSection, "{" + templateName + "}"));
                stringBuilder.AppendLine(description);
            }
            foreach (var dir in templatesDir.SubDirectories)
            {
                WalkTemplateDirectory(dir, stringBuilder, depth + 1,lang);
            }
        }

        private string GenerateTemplateCode(string lang)
        {
            var cultureInfo = new CultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            
            var stringBuilder = new StringBuilder();
            foreach (var dir in _templatesDir.SubDirectories)
            {
                WalkTemplateDirectory(dir, stringBuilder, 0, lang);
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

                var streamWriter = new System.IO.StreamWriter(filePath, false, Encoding.UTF8);
                streamWriter.Write(content);
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error trying to write file {0}! Message: {1}", filePath, ex.Message));
            }
        }
        
        //private void CopyAdditionalResources()
        //{
        //    bool developer = AssemblyHelper.InstallationType == Ct2InstallationType.Developer;

        //    var additionalResources = XElement.Parse(Properties.Resources.AdditionalResources);
        //    foreach (var r in additionalResources.Elements("file"))
        //    {
        //        try
        //        {
        //            var path = r.Attribute("path").Value;
        //            int sIndex = path.IndexOf('/');
        //            var resUri = new Uri(string.Format("pack://application:,,,/{0};component/{1}",
        //                                               path.Substring(0, sIndex), path.Substring(sIndex + 1)));
        //            var fileName = Path.Combine(OutputDir, Path.Combine(OnlineHelp.HelpDirectory, Path.GetFileName(path)));
                    
        //            using (var resStream = Application.GetResourceStream(resUri).Stream)
        //            using (var streamWriter = new System.IO.StreamWriter(fileName, false))
        //            {
        //                resStream.CopyTo(streamWriter.BaseStream);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception(string.Format("Error trying to copy additional resource: {0}", ex.Message));
        //        }
        //    }

        //    foreach (var r in additionalResources.Elements("archive"))
        //    {
        //        var excl = r.Attribute("excludeDeveloper");
        //        if (!developer || excl == null || excl.Value.ToLower() == "false")
        //        {
        //            try
        //            {
        //                var path = r.Attribute("path").Value;
        //                int sIndex = path.IndexOf('/');
        //                var resUri = new Uri(string.Format("pack://application:,,,/{0};component/{1}",
        //                                                   path.Substring(0, sIndex), path.Substring(sIndex + 1)));

        //                //Extract archive:
        //                using (var resStream = Application.GetResourceStream(resUri).Stream)
        //                using (var zipPackage = ZipFile.Read(resStream))
        //                {
        //                    zipPackage.ExtractAll(OnlineHelp.HelpDirectory, ExtractExistingFileAction.OverwriteSilently);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                throw new Exception(string.Format("Error trying to copy additional resource archive: {0}", ex.Message));
        //            }
        //        }
        //    }
        //}
    }
}
