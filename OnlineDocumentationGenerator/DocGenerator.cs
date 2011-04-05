using System;
using System.Collections.Generic;
using Cryptool.Core;
using Cryptool.PluginBase;
using OnlineDocumentationGenerator.Generators;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;

namespace OnlineDocumentationGenerator
{
    class DocGenerator
    {
        public void Generate()
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
                        Console.Out.WriteLine(string.Format("Plugin {0} error: {1}", pluginType.GetPluginInfoAttribute().Caption, ex.Message));
                    }
                }
            }

            generator.Generate();
        }

        [STAThread]
        static void Main(string[] args)
        {
            var gen = new DocGenerator();
            gen.Generate();
        }
    }
}
