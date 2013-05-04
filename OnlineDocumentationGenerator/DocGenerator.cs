using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Linq;
using Cryptool.Core;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Editor;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using OnlineDocumentationGenerator.DocInformations;
using OnlineDocumentationGenerator.DocInformations.Utils;
using OnlineDocumentationGenerator.Generators;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;

namespace OnlineDocumentationGenerator
{
    public class DocGenerator
    {
        public static string TemplateDirectory = "Templates";
        public static string CommonDirectory = "Common";
        public static Dictionary<string, List<TemplateDocumentationPage>> RelevantComponentToTemplatesMap = new Dictionary<string, List<TemplateDocumentationPage>>();

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public void Generate(string baseDir, Generator generator)
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            var currentUICulture = Thread.CurrentThread.CurrentUICulture;

            try
            {
                generator.OutputDir = baseDir;

                var templatesDir = ReadTemplates(baseDir, "", generator);
                ReadPlugins(generator);
                ReadCommonDocPages(generator);

                try
                {
                    generator.Generate(templatesDir);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Error trying to generate documentation: {0}", ex.Message), NotificationLevel.Error);
                    MessageBox.Show("Error trying to open documentation! Please read the log for details.");
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulture;
                Thread.CurrentThread.CurrentUICulture = currentUICulture;
            }
        }

        public static PluginDocumentationPage CreatePluginDocumentationPage(Type type)
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            var currentUICulture = Thread.CurrentThread.CurrentUICulture;

            try
            {

                if (type.GetInterfaces().Contains(typeof(IEditor)))
                {
                    return new EditorDocumentationPage(type);
                }
                else
                {
                    return new ComponentDocumentationPage(type);
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulture;
                Thread.CurrentThread.CurrentUICulture = currentUICulture;
            }
        }

        private static int CompareTemplateDirectories(TemplateDirectory x, TemplateDirectory y)
        {
            if (x.Order < y.Order) return -1;
            if (x.Order > y.Order) return 1;
            return String.Compare(x.GetName(), y.GetName());
        }

        private static int CompareTemplateDocPages(TemplateDocumentationPage x, TemplateDocumentationPage y)
        {
            return String.Compare(x.CurrentLocalization.Name, y.CurrentLocalization.Name);
        }

        private TemplateDirectory ReadTemplates(string baseDir, string subdir, Generator generator)
        {
            var directory = new DirectoryInfo(Path.Combine(baseDir, Path.Combine(TemplateDirectory, subdir)));
            var templateDir = new TemplateDirectory(directory);

            //recursively analyze subdirs:
            foreach(var childdir in directory.GetDirectories())
            {
                var subDir = ReadTemplates(baseDir, Path.Combine(subdir, childdir.Name), generator);
                templateDir.SubDirectories.Add(subDir);
            }
            templateDir.SubDirectories.Sort(CompareTemplateDirectories);

            foreach (var file in directory.GetFiles().Where(x => (x.Extension.ToLower() == ".cwm")))
            {
                try
                {
                    var templatePage = new TemplateDocumentationPage(file.FullName, subdir);
                    if (templatePage.RelevantPlugins != null)
                    {
                        foreach (var relevantPlugin in templatePage.RelevantPlugins)
                        {
                            if (RelevantComponentToTemplatesMap.ContainsKey(relevantPlugin))
                            {
                                RelevantComponentToTemplatesMap[relevantPlugin].Add(templatePage);
                            }
                            else
                            {
                                RelevantComponentToTemplatesMap.Add(relevantPlugin, new List<TemplateDocumentationPage>() { templatePage });
                            }
                        }
                    }
                    generator.AddDocumentationPage(templatePage);
                    templateDir.ContainingTemplateDocPages.Add(templatePage);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Error while trying to read templates for Online Help generation: {0} ({1})", ex.Message, file.FullName), NotificationLevel.Warning);
                }
                templateDir.ContainingTemplateDocPages.Sort(CompareTemplateDocPages);
            }

            return templateDir;
        }

        private void ReadPlugins(Generator generator)
        {
            var pluginManager = new PluginManager(null);
            foreach (Type type in pluginManager.LoadTypes(AssemblySigningRequirement.LoadAllAssemblies).Values)
            {
                if (type.GetPluginInfoAttribute() != null)
                {
                    try
                    {
                        var p = CreatePluginDocumentationPage(type);
                        if (p != null)
                        {
                            generator.AddDocumentationPage(p);
                        }
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(string.Format("{0} error: {1}", type.GetPluginInfoAttribute().Caption, ex.Message),
                                      NotificationLevel.Error);
                    }
                }
            }
        }

        private void ReadCommonDocPages(Generator generator)
        {
            try
            {
                generator.AddDocumentationPage(new CommonDocumentationPage(XElement.Parse(Properties.Resources.HomomorphicChiffres)));
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Error while trying to read common doc page: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, message, logLevel);
        }
    }
}
