using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using Cryptool.Core;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Editor;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using OnlineDocumentationGenerator.DocInformations;
using OnlineDocumentationGenerator.Generators;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;

namespace OnlineDocumentationGenerator
{
    public class DocGenerator
    {
        public static string TemplateDirectory = "Templates";
        public static Dictionary<string, List<TemplateDocumentationPage>> RelevantComponentToTemplatesMap = new Dictionary<string, List<TemplateDocumentationPage>>();
        
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public void Generate(string baseDirectory)
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            var currentUICulture = Thread.CurrentThread.CurrentUICulture;

            try
            {
                GenerateHTML(baseDirectory);
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

        private void ReadTemplates(string outputDir, string subdir, HtmlGenerator generator)
        {
            var directory = new DirectoryInfo(Path.Combine(outputDir, Path.Combine(TemplateDirectory, subdir)));
            foreach(var childdir in directory.GetDirectories())
            {
                ReadTemplates(outputDir, Path.Combine(subdir, childdir.Name), generator);
            }

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
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Error while trying to read templates for Online Help generation: {0}", ex.Message), NotificationLevel.Warning);
                }
            }
        }

        private void GenerateHTML(string outputDir)
        {
            var generator = new HtmlGenerator();
            generator.OutputDir = outputDir;

            ReadTemplates(outputDir, "", generator);
            ReadPlugins(generator);

            try
            {
                generator.Generate();
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Error trying to generate documentation: {0}", ex.Message), NotificationLevel.Error);
                MessageBox.Show("Error trying to open documentation! Please read the log for details.");
            }
        }

        private void ReadPlugins(HtmlGenerator generator)
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

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, message, logLevel);
        }
    }
}
