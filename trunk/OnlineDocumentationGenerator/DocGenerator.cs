using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Cryptool.Core;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Editor;
using Cryptool.PluginBase.Miscellaneous;
using OnlineDocumentationGenerator.DocInformations;
using OnlineDocumentationGenerator.Generators;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;

namespace OnlineDocumentationGenerator
{
    public class DocGenerator
    {
        public static string TemplateDirectory = "ProjectSamples";
        public static Dictionary<string, List<string>> RelevantComponentToTemplatesMap = new Dictionary<string, List<string>>();
        
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public void Generate(string outputDir = ".")
        {
            ReadTemplates(".");
            GenerateHTML(outputDir);
        }

        public static EntityDocumentationPage CreateDocumentationPage(Type type)
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

        private void ReadTemplates(string dir)
        {
            var directory = new DirectoryInfo(Path.Combine(TemplateDirectory, dir));
            foreach(var subdir in directory.GetDirectories())
            {
                ReadTemplates(Path.Combine(dir, subdir.Name));
            }

            foreach (var file in directory.GetFiles().Where(x => ((x.Extension.ToLower() == ".cte") || (x.Extension.ToLower() == ".cwm"))))
            {
                string xmlFile = Path.Combine(file.Directory.FullName, file.Name.Substring(0, file.Name.Length - 4) + ".xml");
                if (File.Exists(xmlFile))
                {
                    XElement xml = XElement.Load(xmlFile);
                    var relevantPlugins = xml.Element("relevantPlugins");
                    if (relevantPlugins != null)
                    {
                        foreach (var plugin in relevantPlugins.Elements("plugin"))
                        {
                            var name = plugin.Attribute("name");
                            if (name != null)
                            {
                                var template = Path.Combine(dir, file.Name);
                                if (RelevantComponentToTemplatesMap.ContainsKey(name.Value))
                                {
                                    RelevantComponentToTemplatesMap[name.Value].Add(template);
                                }
                                else
                                {
                                    RelevantComponentToTemplatesMap.Add(name.Value, new List<string>() {template});
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GenerateHTML(string outputDir)
        {
            var generator = new HtmlGenerator();
            generator.OutputDir = outputDir;

            var pluginManager = new PluginManager(null);
            foreach (Type type in pluginManager.LoadTypes(AssemblySigningRequirement.LoadAllAssemblies).Values)
            {
                if (type.GetPluginInfoAttribute() != null)
                {
                    try
                    {
                        var p = CreateDocumentationPage(type);
                        if (p != null)
                        {
                            generator.AddDocumentationPage(p);
                        }
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(string.Format("{0} error: {1}", type.GetPluginInfoAttribute().Caption, ex.Message), NotificationLevel.Error);
                    }
                }
            }

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

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, message, logLevel);
        }
    }
}
