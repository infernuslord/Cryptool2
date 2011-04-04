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
                    generator.AddPluginDocumentationPage(new PluginDocumentationPage(pluginType));
                }
            }

            generator.Generate();
        }

        static void Main(string[] args)
        {
            var gen = new DocGenerator();
            gen.Generate();
        }
    }
}
