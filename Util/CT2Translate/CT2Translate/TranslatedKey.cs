using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;
using System.Resources;
using System.Xml;

namespace WindowsFormsApplication1
{
    class TranslatedKey
    {
        Dictionary<string, string> translations = new Dictionary<string, string>();

        public Dictionary<string, string> Translations
        {
            get
            {
                return translations;
            }
        }

        public void Add(string lang, string text)
        {
            translations[lang] = text;
        }
    }

    class TranslatedResource
    {
        Dictionary<string,TranslatedKey> translatedKey = new Dictionary<string,TranslatedKey>();
        public Dictionary<string, string> files = new Dictionary<string, string>();
        public Dictionary<string, bool> modified = new Dictionary<string, bool>();
        public string basename;

        public Dictionary<string, TranslatedKey> TranslatedKey
        {
            get
            {
                return translatedKey;
            }
        }

        public void SaveAs(string lang, string modification)
        {
            Save(basename + "." + modification + "." + AllResources.langext[lang] + ".resx", lang);
        }

        public void Save()
        {
            foreach (string lang in files.Keys) 
                Save(files[lang],lang);
        }

        public void Save(string lang)
        {
            if (files.ContainsKey(lang))
                Save(files[lang], lang);
        }

        public void Save(string filename, string lang)
        {
            IResourceWriter writer = new ResXResourceWriter(filename);

            foreach (string key in translatedKey.Keys)
            {
                if (translatedKey[key].Translations.ContainsKey(lang))
                {
                    string value = translatedKey[key].Translations[lang];
                    writer.AddResource(key, value+"Hello, world!");
                }
            }

            writer.Generate();
            writer.Close();
        }

        public void Load(string lang, string filename)
        {
            ResXResourceReader reader = new ResXResourceReader(filename);
            reader.BasePath = Path.GetDirectoryName(filename);

            try
            {
                foreach (DictionaryEntry entry in reader)
                {
                    string key = entry.Key.ToString();
                    string value = entry.Value.ToString();
                    if (!translatedKey.ContainsKey(key)) 
                        translatedKey.Add(key, new TranslatedKey());
                    translatedKey[key].Add(lang, value);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

            reader.Close();

            files[lang] = filename;
        }

        /* lädt die resx-Dateien ohne die referenzierten externen Keys */
        public void LoadXML(string lang, string filename)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(filename);
            
            try
            {
                XmlNodeList nodelist = xml.DocumentElement.SelectNodes("data[not(@type or @mimetype)]");
                foreach (XmlNode n in nodelist)
                {
                    //string key = n.SelectSingleNode("@name").Value;
                    //string value = n.SelectSingleNode(".//value").InnerText;
                    string key = n.Attributes["name"].Value;
                    string value = n.ChildNodes[1].InnerText;
                    
                    if (!translatedKey.ContainsKey(key))
                        translatedKey.Add(key, new TranslatedKey());
                    translatedKey[key].Add(lang, value);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            
            //xml.Save("C:\\Users\\u1\\Desktop\\test2.xml");
            files[lang] = filename;
        }

    }

    class AllResources
    {
        public static string[] cultures = new string[] { "en", "de" };
        public static string defaultculture = "en";
        public static Dictionary<string, string> langext = new Dictionary<string, string> { { "en", "en-EN" }, { "de", "de-DE" } };

        public Dictionary<string, TranslatedResource> Resources = new Dictionary<string, TranslatedResource>();

        public static string getBasename(string fname)
        {
            //Match match = Regex.Match(fname, "^(.*).resx$");
            //if (!match.Success) return null;
            if (Path.GetExtension(fname) != ".resx") return null;
            //string basename = match.Groups[1].Value;
            string basename = Path.Combine(Path.GetDirectoryName(fname),Path.GetFileNameWithoutExtension(fname));

            foreach (string culture in cultures)
            {
                Match match = Regex.Match(basename, "^(.*)." + langext[culture] + "$");
                if (match.Success) return match.Groups[1].Value;
            }

            return basename;
        }

        public static string getCulture(string fname)
        {
            if (Path.GetExtension(fname) != ".resx") return null;
            string basename = Path.GetFileNameWithoutExtension(fname);
            string lang = Path.GetExtension(basename);

            foreach (string culture in cultures)
            {
                Match match = Regex.Match(basename, "^(.*)." + langext[culture] + "$");
                if (match.Success) return culture;
            }

            return AllResources.defaultculture;
        }

        public static Dictionary<string, string> getExistingFileNames(string basename)
        {
            Dictionary<string, string> result = new Dictionary<string, string> {};

            string f;
            foreach (string culture in cultures)
            {
                f = basename + "." + langext[culture] + ".resx";
                if (File.Exists(f)) result[culture] = f;
            }

            f = basename + ".resx";
            if (File.Exists(f)) result[defaultculture] = f;

            return result;
        }

        public static TranslatedResource getResources(Dictionary<string, string> fnames)
        {
            TranslatedResource result = new TranslatedResource();

            foreach (string lang in fnames.Keys)
                result.LoadXML(lang, fnames[lang]);

            return result;
        }

        public static TranslatedResource getResources(string basename)
        {
            TranslatedResource result = getResources(getExistingFileNames(basename));
            result.basename = basename;
            return result;
        }

        public void Add(string fname)
        {
            string basename = getBasename(fname);
            if (basename != null && !Resources.ContainsKey(basename))
                Resources[basename] = getResources(basename);
        }

        public void Add(string[] fname)
        {
            foreach (string f in fname)
                Add(f);
        }

        public void Clear()
        {
            Resources.Clear();
        }

        public void SaveText(string filename, string lang)
        {
            using (StreamWriter w = new StreamWriter(filename))
            {
                foreach (string res in Resources.Keys)
                    foreach (string key in Resources[res].TranslatedKey.Keys)
                        if( Resources[res].TranslatedKey[key].Translations.ContainsKey(lang) )
                            w.WriteLine(Resources[res].TranslatedKey[key].Translations[lang]);
            }
        }

        public void SaveXML(string filename, string basepath)
        {
            XmlTextWriter w = new XmlTextWriter(filename,null);
            w.Formatting = Formatting.Indented;

            w.WriteStartElement("root");

            w.WriteStartElement("basepath");
            w.WriteAttributeString("name", basepath);
            w.WriteEndElement();

            foreach (string res in Resources.Keys)
            {
                w.WriteStartElement("resource");
                w.WriteAttributeString("base", res);

                w.WriteStartElement("files");
                foreach (string lang in Resources[res].files.Keys)
                {
                    w.WriteStartElement("file");
                    w.WriteAttributeString("lang", lang);
                    w.WriteString(Resources[res].files[lang]);
                    w.WriteEndElement();
                }
                w.WriteEndElement();

                foreach (string key in Resources[res].TranslatedKey.Keys)
                {
                    w.WriteStartElement("key");
                    w.WriteAttributeString("value", key);
                    TranslatedKey t = Resources[res].TranslatedKey[key];
                    foreach (string lang in t.Translations.Keys)
                    {
                        w.WriteStartElement("value");
                        w.WriteAttributeString("lang", lang);
                        w.WriteString(t.Translations[lang]);
                        w.WriteEndElement();
                    }
                    w.WriteEndElement();
                }
                w.WriteEndElement();
            }

            w.Close();
        }

        public void LoadXML(string filename)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(filename);

            XmlNode n = xml.SelectSingleNode("/root/basepath");
            string basepath = n.Attributes["name"].Value;

            XmlNodeList nl = xml.SelectNodes("/root/resource");
            foreach( XmlNode r in nl ) {
                TranslatedResource tr = new TranslatedResource();
                tr.basename = basepath;
                
                string b = r.Attributes["base"].Value;

                XmlNode f = r.SelectSingleNode("files");
                foreach (XmlNode l in f.SelectNodes("file"))
                {
                    string lang = l.Attributes["lang"].Value;
                    tr.files[lang] = l.FirstChild.InnerText;
                }

                foreach (XmlNode k in r.SelectNodes("key"))
                {
                    string key = k.Attributes["value"].Value;
                    TranslatedKey tk = new TranslatedKey();
                    foreach (XmlNode t in k.ChildNodes)
                    {
                        string lang = t.Attributes["lang"].Value;
                        tk.Translations[lang] = t.InnerText;
                    }
                    tr.TranslatedKey[key] = tk;
                }

                Resources[b] = tr;
            }
        }
    }
    //<key value="ActionCaption">
    //  <translation lang="de">
    //    <value>Aktion</value>
    //  </translation>
    //  <translation lang="en">
    //    <value>Action</value>
    //  </translation>
    //</key>

}
