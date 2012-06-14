using System;
using System.Collections.Generic;
using OnlineDocumentationGenerator.DocInformations.Localization;

namespace OnlineDocumentationGenerator.DocInformations
{
    public abstract class EntityDocumentationPage
    {
        public string AuthorName { get; protected set; }
        public Dictionary<string, LocalizedEntityDocumentationPage> Localizations { get; protected set; }

        public abstract string Name { get; }

        public abstract string DocPath { get; }

        protected EntityDocumentationPage()
        {
            Localizations = new Dictionary<string, LocalizedEntityDocumentationPage>();
        }
    }
}