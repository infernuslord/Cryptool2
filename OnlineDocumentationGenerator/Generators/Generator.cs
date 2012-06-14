using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OnlineDocumentationGenerator.DocInformations;

namespace OnlineDocumentationGenerator.Generators
{
    public abstract class Generator
    {
        protected List<PluginDocumentationPage> DocPages = new List<PluginDocumentationPage>();
        protected HashSet<string> AvailableLanguages = new HashSet<string>();

        public string OutputDir
        {
            get; set;
        }

        /// <summary>
        /// Adds a documentation page for the given entity to generate in all available localizations.
        /// </summary>
        /// <param name="pluginDocumentationPage">The class with all informations about the entity</param>
        public void AddDocumentationPage(PluginDocumentationPage pluginDocumentationPage)
        {
            DocPages.Add(pluginDocumentationPage);

            foreach (var lang in pluginDocumentationPage.AvailableLanguages)
            {
                AvailableLanguages.Add(lang);
            }
        }

        /// <summary>
        /// Generates all specified pages and an index page.
        /// </summary>
        public abstract void Generate();
    }
}
