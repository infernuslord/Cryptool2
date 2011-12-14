using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows.Forms;
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
        public string basepath;

        public Dictionary<string, TranslatedKey> TranslatedKey
        {
            get
            {
                return translatedKey;
            }
        }

        public void SaveAs(string lang, string modification)
        {
            Save(basepath + "." + modification + "." + AllResources.langext[lang] + ".resx", lang);
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

        public void Update(string lang)
        {
            List<ResXDataNode> nodelist = new List<ResXDataNode>();
            string filename = basepath + files[lang];

            // read nodes
            ResXResourceReader reader = new ResXResourceReader(filename);
            reader.BasePath = Path.GetDirectoryName(filename);
            reader.UseResXDataNodes = true; 
                
            foreach (DictionaryEntry entry in reader)
            {
                ResXDataNode dataNode = (ResXDataNode)entry.Value;
                if (dataNode.FileRef == null)
                {
                    string key = entry.Key.ToString();
                    string value = entry.Value.ToString();

                    if (translatedKey.ContainsKey(key))
                        if (translatedKey[key].Translations.ContainsKey(lang))
                            if (translatedKey[key].Translations[lang] != value)
                                dataNode = new ResXDataNode(key, translatedKey[key].Translations[lang]);
                }
                nodelist.Add(dataNode);
            }

            reader.Close();

            // write nodes
            ResXResourceWriter writer = new ResXResourceWriter(filename);
            foreach (ResXDataNode dataNode in nodelist)
            {
                writer.AddResource(dataNode);
            }
            writer.Generate();
            writer.Close();
        }

        public void Update()
        {
            foreach (string lang in files.Keys)
                Update(lang);
        }

        public void Load(string lang, string filename)
        {
            ResXResourceReader reader = new ResXResourceReader(filename);
            reader.BasePath = Path.GetDirectoryName(filename);
            reader.UseResXDataNodes = true;

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
            xml.Load(basepath + filename);
            
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
        public string basepath;

        public string getKey(string fname)
        {
            if (fname.IndexOf(basepath) == 0)
                fname = fname.Remove(0, basepath.Length);

            if (Path.GetExtension(fname) != ".resx") return null;
            string basename = Path.Combine(Path.GetDirectoryName(fname), Path.GetFileNameWithoutExtension(fname));

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

        public Dictionary<string, string> getExistingFileNames(string relpath)
        {
            Dictionary<string, string> result = new Dictionary<string, string> { };

            string f;
            foreach (string culture in cultures)
            {
                f = relpath + "." + langext[culture] + ".resx";
                if (File.Exists(basepath + f)) result[culture] = f;
            }

            f = relpath + ".resx";
            if (File.Exists(basepath + f)) result[defaultculture] = f;

            return result;
        }

        public TranslatedResource getResources(Dictionary<string, string> fnames)
        {
            TranslatedResource result = new TranslatedResource();
            result.basepath = basepath;

            foreach (string lang in fnames.Keys)
                result.LoadXML(lang, fnames[lang]);

            return result;
        }

        public TranslatedResource getResources(string relpath)
        {
            return getResources(getExistingFileNames(relpath));
        }

        public void Add(string fname)
        {
            string relpath = getKey(fname);
            if (relpath != null && !Resources.ContainsKey(relpath))
                Resources[relpath] = getResources(relpath);
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

        public void Update()
        {
            try
            {
                foreach (TranslatedResource tr in Resources.Values)
                    tr.Update();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while updating:\n"+e.Message, "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SaveText(string filename, string lang)
        {
            using (StreamWriter w = new StreamWriter(filename))
            {
                foreach (string res in Resources.Keys)
                    foreach (string key in Resources[res].TranslatedKey.Keys)
                        if (Resources[res].TranslatedKey[key].Translations.ContainsKey(lang))
                            w.WriteLine(Resources[res].TranslatedKey[key].Translations[lang]);
            }
        }

        public void SaveXML(string filename)
        {
            XmlTextWriter w = new XmlTextWriter(filename, null);
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
            this.basepath = n.Attributes["name"].Value;
            basepath = new Regex("[/\\\\]+$").Replace(basepath, "");

            XmlNodeList nl = xml.SelectNodes("/root/resource");
            foreach (XmlNode r in nl)
            {
                TranslatedResource tr = new TranslatedResource();
                tr.basepath = basepath;

                string b = r.Attributes["base"].Value;
                if (b.IndexOf(basepath) == 0) b = b.Remove(0, basepath.Length);

                XmlNode f = r.SelectSingleNode("files");
                foreach (XmlNode l in f.SelectNodes("file"))
                {
                    string lang = l.Attributes["lang"].Value;
                    tr.files[lang] = l.FirstChild.InnerText;
                    if (tr.files[lang].IndexOf(basepath) == 0) tr.files[lang] = tr.files[lang].Remove(0, basepath.Length);
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

        public string[] MatchFiles( string pathprefix )
        {
            List<string> result = new List<string>();

            foreach (string path in this.Resources.Keys)
            {
                if (path.IndexOf(pathprefix) == 0) result.Add(path);
            }

            return result.ToArray();
        }

        public TreeNode GetTree()
        {
            TreeNode root = new TreeNode(this.basepath);

            foreach (string key in this.Resources.Keys)
            {
                foreach (string file in this.Resources[key].files.Values)
                {
                    string[] dirs = file.Split(new char[] { '/', '\\' });
                    TreeNode t = root;
                    foreach (string dir in dirs)
                    {
                        if (dir.Length == 0) continue;
                        if (!t.Nodes.ContainsKey(dir)) { 
                            t.Nodes.Add(dir, dir);
                            t.Nodes[dir].Tag = t.Tag + "\\" + dir;
                        }
                        t = t.Nodes[dir];
                    }
                    t.ToolTipText = this.basepath + file;
                    t.Tag = key;
                }
            }

            root.Tag = "\\";    // must be here for tag creation to work properly
            return root;
        }

        public void ScanDir(string basepath)
        {
            this.basepath = basepath;
            Clear();
            DirSearch(basepath);
        }

        int DirSearch(string sDir)
        {
            int cnt = 0;

            try
            {
                foreach (string f in Directory.GetFiles(sDir, "*.resx"))
                {
                    Add(f);
                    cnt++;
                }

                foreach (string d in Directory.GetDirectories(sDir))
                {
                    int found = DirSearch(d);
                    if (found > 0) cnt += found;
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }

            return cnt;
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
