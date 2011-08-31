/*                              
   Copyright 2011 Nils Kopal, Uni Duisburg-Essen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Documents;
using System.Windows;

namespace CrypDocumentationEditor
{
    public class Documentation
    {
        private XmlDocument xml = null;
        public const string DEFAULT_LANGUAGE = "en";

        public Documentation()
        {
            try
            {
                Language = DEFAULT_LANGUAGE;
                xml = new XmlDocument();

                XmlDeclaration declaration = (XmlDeclaration)xml.CreateNode(XmlNodeType.XmlDeclaration, "xml", null);
                declaration.Encoding = "utf-8";
                XmlNode documentationNode = xml.CreateNode(XmlNodeType.Element, "documentation", null);
                XmlNode introductionNode = xml.CreateNode(XmlNodeType.Element, "introduction", null);
                XmlNode usageNode = xml.CreateNode(XmlNodeType.Element, "usage", null);
                XmlNode presentationNode = xml.CreateNode(XmlNodeType.Element, "presentation", null);
                XmlNode languageNode = xml.CreateNode(XmlNodeType.Element, "language", null);

                XmlAttribute languageNodeAttribute = xml.CreateAttribute("culture");
                languageNodeAttribute.Value = Language;                

                XmlAttribute introductionlangAttribute = xml.CreateAttribute("lang");
                introductionlangAttribute.Value = Language;
                XmlAttribute usagelangAttribute = xml.CreateAttribute("lang");
                usagelangAttribute.Value = Language;
                XmlAttribute presentationlangAttribute = xml.CreateAttribute("lang");
                presentationlangAttribute.Value = Language;

                xml.AppendChild(declaration);
                xml.AppendChild(documentationNode);
                documentationNode.AppendChild(languageNode);
                languageNode.Attributes.Append(languageNodeAttribute);
                documentationNode.AppendChild(introductionNode);
                introductionNode.Attributes.Append(introductionlangAttribute);
                documentationNode.AppendChild(usageNode);
                usageNode.Attributes.Append(usagelangAttribute);
                documentationNode.AppendChild(presentationNode);
                presentationNode.Attributes.Append(presentationlangAttribute);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public string Language
        {
            get;
            set;
        }

        public void Load(string filename)
        {            
            xml.Load(filename);                    
        }

        public void Save(string filename)
        {
            xml.Save(filename);
        }

        public FlowDocument Introduction
        {
            get
            {
                XmlNode introduction = xml.SelectSingleNode("/documentation/introduction[@lang='"+ Language +"']");
                string text = introduction.InnerXml.Trim();
                FlowDocument document = new FlowDocument();
                Paragraph para = new Paragraph();
                para.Inlines.Add(new Run(text));
                document.Blocks.Add(para);
                return document;
            }
            set
            {
                XmlNode introduction = xml.SelectSingleNode("/documentation/introduction[@lang='" + Language + "']");
                StringBuilder builder = new StringBuilder();
                foreach (Paragraph p in value.Blocks)
                {
                    foreach (Run r in p.Inlines)
                    {
                        builder.Append(r.Text + Environment.NewLine);
                    }
                }
                introduction.InnerXml = builder.ToString();
            }
        }

        public FlowDocument Usage
        {
            get
            {
                XmlNode usage = xml.SelectSingleNode("/documentation/usage[@lang='" + Language + "']");
                string text = usage.InnerXml.Trim();
                FlowDocument document = new FlowDocument();
                Paragraph para = new Paragraph();
                para.Inlines.Add(new Run(text));
                document.Blocks.Add(para);
                return document;
            }
            set
            {
                XmlNode usage = xml.SelectSingleNode("/documentation/usage[@lang='" + Language + "']");
                StringBuilder builder = new StringBuilder();
                foreach (Paragraph p in value.Blocks)
                {
                    foreach (Run r in p.Inlines)
                    {
                        builder.Append(r.Text + Environment.NewLine);
                    }
                }
                usage.InnerXml = builder.ToString();
            }
        }

        public FlowDocument Presentation
        {
            get
            {
                XmlNode presentation = xml.SelectSingleNode("/documentation/presentation[@lang='" + Language + "']");
                string text = presentation.InnerXml.Trim();
                FlowDocument document = new FlowDocument();
                Paragraph para = new Paragraph();
                para.Inlines.Add(new Run(text));
                document.Blocks.Add(para);
                return document;
            }
            set
            {
                XmlNode presentation = xml.SelectSingleNode("/documentation/presentation[@lang='" + Language + "']");
                StringBuilder builder = new StringBuilder();
                foreach (Paragraph p in value.Blocks)
                {
                    foreach (Run r in p.Inlines)
                    {
                        builder.Append(r.Text + Environment.NewLine);
                    }
                }
                presentation.InnerXml = builder.ToString();
            }
        }

        public string[] GetLanguages()
        {
            XmlNodeList nodes = xml.SelectNodes("/documentation/language");
            if (nodes.Count == 0)
            {
                return null;
            }
            string[] languages = new string[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                languages[i] = nodes[i].Attributes["culture"].Value;
            }
            return languages;
        }

        public void AddLanguage(string language)
        {
            XmlNode languageNode = xml.CreateNode(XmlNodeType.Element, "language", null);
            XmlAttribute languageNodeAttribute = xml.CreateAttribute("culture");
            languageNodeAttribute.Value = language;
            languageNode.Attributes.Append(languageNodeAttribute);

            XmlNode documentationNode = xml.SelectSingleNode("/documentation");

            XmlNode introductionNode = xml.CreateNode(XmlNodeType.Element, "introduction", null);
            XmlNode usageNode = xml.CreateNode(XmlNodeType.Element, "usage", null);
            XmlNode presentationNode = xml.CreateNode(XmlNodeType.Element, "presentation", null);                        

            XmlAttribute introductionlangAttribute = xml.CreateAttribute("lang");
            introductionlangAttribute.Value = language;
            XmlAttribute usagelangAttribute = xml.CreateAttribute("lang");
            usagelangAttribute.Value = language;
            XmlAttribute presentationlangAttribute = xml.CreateAttribute("lang");
            presentationlangAttribute.Value = language;

            documentationNode.AppendChild(languageNode);            
            languageNode.Attributes.Append(languageNodeAttribute);
            documentationNode.AppendChild(introductionNode);
            introductionNode.Attributes.Append(introductionlangAttribute);
            documentationNode.AppendChild(usageNode);
            usageNode.Attributes.Append(usagelangAttribute);
            documentationNode.AppendChild(presentationNode);
            presentationNode.Attributes.Append(presentationlangAttribute);            
        }
    }



}
