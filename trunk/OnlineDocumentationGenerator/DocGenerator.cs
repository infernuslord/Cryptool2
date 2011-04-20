using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cryptool.Core;
using Cryptool.PluginBase;
using OnlineDocumentationGenerator.Generators;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;

namespace OnlineDocumentationGenerator
{
    class DocGenerator
    {
        public static string TemplateDirectory = "ProjectSamples";
        public static Dictionary<string, List<string>> RelevantPluginToTemplatesMap = new Dictionary<string, List<string>>();

        public void Generate()
        {
            ReadTemplates(".");
            GenerateHTML();
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
                                if (RelevantPluginToTemplatesMap.ContainsKey(name.Value))
                                {
                                    RelevantPluginToTemplatesMap[name.Value].Add(template);
                                }
                                else
                                {
                                    RelevantPluginToTemplatesMap.Add(name.Value, new List<string>() {template});
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GenerateHTML()
        {
            var generator = new HtmlGenerator();

            var pluginManager = new PluginManager(null);
            foreach (Type pluginType in pluginManager.LoadTypes(AssemblySigningRequirement.LoadAllAssemblies).Values)
            {
                if (pluginType.GetPluginInfoAttribute() != null)
                {
                    try
                    {
                        generator.AddPluginDocumentationPage(new PluginDocumentationPage(pluginType));
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(string.Format("Plugin {0} error: {1}", pluginType.GetPluginInfoAttribute().Caption, ex.Message));
                    }
                }
            }

            try
            {
                generator.Generate();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(string.Format("Error trying to generate documentation: {0}", ex.Message));
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            var gen = new DocGenerator();
            gen.Generate();
        }
    }
}
