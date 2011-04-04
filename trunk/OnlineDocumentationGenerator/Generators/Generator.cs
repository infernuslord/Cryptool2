using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnlineDocumentationGenerator.Generators
{
    public abstract class Generator
    {
        protected List<PluginDocumentationPage> pluginPages = new List<PluginDocumentationPage>();
        protected HashSet<string> availableLanguages = new HashSet<string>();

        /// <summary>
        /// Adds a documentation page for the given plugin to generate in all available localizations.
        /// </summary>
        /// <param name="pluginDocumentationPage">The class with all informations about the plugin</param>
        public void AddPluginDocumentationPage(PluginDocumentationPage pluginDocumentationPage)
        {
            pluginPages.Add(pluginDocumentationPage);

            foreach (var lang in pluginDocumentationPage.AvailableLanguages)
            {
                availableLanguages.Add(lang);
            }
        }

        /// <summary>
        /// Generates all specified pages and an index page.
        /// </summary>
        public abstract void Generate();
    }
}
