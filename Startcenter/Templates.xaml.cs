using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Cryptool.Core;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Editor;
using Cryptool.PluginBase.Miscellaneous;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using Path = System.IO.Path;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for Templates.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class Templates : UserControl
    {
        private readonly RecentFileList _recentFileList = RecentFileList.GetSingleton();
        private string _templatesDir;

        public string TemplatesDir
        {
            set 
            {
                if (value != null)
                {
                    _templatesDir = value;
                    DirectoryInfo templateDir = new DirectoryInfo(value);
                    FillTemplatesNavigationPane(templateDir, TemplatesTreeView);
                }
            }
        }

        public event OpenEditorHandler OnOpenEditor;
        public event OpenTabHandler OnOpenTab;

        public Templates()
        {
            InitializeComponent();
        }

        private void FillTemplatesNavigationPane(DirectoryInfo templateDir, TreeView treeView)
        {
            var rootItem = new CTTreeViewItem(templateDir.Name);
            if (templateDir.Exists)
            {
                foreach (var subDirectory in templateDir.GetDirectories())
                {
                    HandleTemplateDirectories(subDirectory, rootItem);
                }

                MakeTemplateInformation(templateDir, rootItem);
            }
            
            //Add root directory entries to the treeview based on their order number:
            var counter = 0;
            var items = rootItem.Items.Cast<CTTreeViewItem>().ToList();
            rootItem.Items.Clear();
            while (items.Count > 0)
            {
                var item = items.FirstOrDefault(x => x.Order == counter);
                if (item != null)
                {
                    items.Remove(item);
                    treeView.Items.Add(item);
                }
                else
                {
                    treeView.Items.Add(new TreeViewItem() { Style = (Style) FindResource("SeparatorStyle") });

                    if (items.All(x => x.Order < 0))
                    {
                        foreach (var it in items)
                        {
                            treeView.Items.Add(it);
                        }
                        return;
                    }
                }
                counter++;
            }
        }

        private void HandleTemplateDirectories(DirectoryInfo directory, CTTreeViewItem parent)
        {
            if (directory == null)
                return;

            //Read directory metainfos:
            var dirName = directory.Name;
            Inline tooltip = null;
            ImageSource dirImage = null;
            var metainfo = directory.GetFiles("dir.xml");
            var order = -1;
            if (metainfo.Length > 0)
            {
                XElement metaXML = XElement.Load(metainfo[0].FullName);
                if (metaXML.Attribute("order") != null)
                {
                    order = int.Parse(metaXML.Attribute("order").Value);
                }

                var dirNameEl = XMLHelper.GetGlobalizedElementFromXML(metaXML, "name");
                if (dirNameEl.Value != null)
                {
                    dirName = dirNameEl.Value;
                }

                if (metaXML.Element("icon") != null && metaXML.Element("icon").Attribute("file") != null)
                {
                    var iconFile = Path.Combine(directory.FullName, metaXML.Element("icon").Attribute("file").Value);
                    if (File.Exists(iconFile))
                    {
                        dirImage = ImageLoader.LoadImage(new Uri(iconFile));
                    }
                }

                var summaryElement = XMLHelper.GetGlobalizedElementFromXML(metaXML, "summary");
                if (summaryElement != null)
                {
                    tooltip = XMLHelper.ConvertFormattedXElement(summaryElement);
                }
            }

            CTTreeViewItem item = new CTTreeViewItem(dirName, order, tooltip, dirImage);
            parent.Items.Add(item);

            foreach (var subDirectory in directory.GetDirectories())
                HandleTemplateDirectories(subDirectory, item);

            MakeTemplateInformation(directory, item);
        }

        private void MakeTemplateInformation(DirectoryInfo info, CTTreeViewItem parent)
        {
            SolidColorBrush bg = Brushes.Transparent;

            foreach (var file in info.GetFiles().Where(x => (x.Extension.ToLower() == ".cwm") || (x.Extension.ToLower() == ".component")))
            {
                if (file.Name.StartsWith("."))
                {
                    continue;
                }
                bool component = (file.Extension.ToLower() == ".component");
                string title = null;
                Span summary1 = new Span();
                Span summary2 = new Span();
                string xmlFile = Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + ".xml");
                string iconFile = null;
                Dictionary<string, List<string>> internationalizedKeywords = new Dictionary<string, List<string>>();
                if (File.Exists(xmlFile))
                {
                    try
                    {
                        XElement xml = XElement.Load(xmlFile);
                        var titleElement = XMLHelper.GetGlobalizedElementFromXML(xml, "title");
                        if (titleElement != null)
                            title = titleElement.Value;

                        var summaryElement = XMLHelper.GetGlobalizedElementFromXML(xml, "summary");
                        var descriptionElement = XMLHelper.GetGlobalizedElementFromXML(xml, "description");
                        if (summaryElement != null)
                        {
                            summary1.Inlines.Add(new Bold(XMLHelper.ConvertFormattedXElement(summaryElement)));
                            summary2.Inlines.Add(new Bold(XMLHelper.ConvertFormattedXElement(summaryElement)));  
                        }
                        if (descriptionElement != null && descriptionElement.Value.Length > 1) 
                        {
                            summary1.Inlines.Add(new LineBreak());
                            summary1.Inlines.Add(new LineBreak());
                            summary1.Inlines.Add(XMLHelper.ConvertFormattedXElement(descriptionElement));
                            summary2.Inlines.Add(new LineBreak());
                            summary2.Inlines.Add(new LineBreak());
                            summary2.Inlines.Add(XMLHelper.ConvertFormattedXElement(descriptionElement));  
                        }

                        if (xml.Element("icon") != null && xml.Element("icon").Attribute("file") != null)
                        {
                            iconFile = Path.Combine(file.Directory.FullName, xml.Element("icon").Attribute("file").Value);
                        }

                        foreach (var keywordTag in xml.Elements("keywords"))
                        {
                            var langAtt = keywordTag.Attribute("lang");
                            string lang = "en";
                            if (langAtt != null)
                            {
                                lang = langAtt.Value;
                            }
                            var keywords = keywordTag.Value;
                            if (keywords != null || keywords != "")
                            {
                                foreach (var keyword in keywords.Split(','))
                                {
                                    if (!internationalizedKeywords.ContainsKey(lang))
                                    {
                                        internationalizedKeywords.Add(lang, new List<string>());
                                    }
                                    internationalizedKeywords[lang].Add(keyword.Trim());
                                }
                            }
                        }
                    }
                    catch(Exception)
                    {
                        //we do nothing if the loading of an description xml fails => this is not a hard error
                    }
                }
                
                if ((title == null) || (title.Trim() == ""))
                {
                    title = component ? file.Name : Path.GetFileNameWithoutExtension(file.Name).Replace("-", " ").Replace("_", " ");
                }

                if (summary1.Inlines.Count == 0)
                {
                    string desc = component ? Properties.Resources.This_is_a_standalone_component_ : Properties.Resources.This_is_a_WorkspaceManager_file_;
                    summary1.Inlines.Add(new Run(desc));
                    summary2.Inlines.Add(new Run(desc));
                }

                if (iconFile == null || !File.Exists(iconFile))
                {
                    iconFile = Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + ".png");
                }
                ImageSource image = null;
                if (File.Exists(iconFile))
                {
                    try
                    {
                        image = ImageLoader.LoadImage(new Uri(iconFile));
                    }
                    catch (Exception)
                    {
                        image = null;
                    }
                }
                else
                {
                    var ext = file.Extension.Remove(0, 1);
                    if (!component && ComponentInformations.EditorExtension.ContainsKey(ext))
                    {
                        Type editorType = ComponentInformations.EditorExtension[ext];
                        image = editorType.GetImage(0).Source;
                    }
                }

                System.Collections.ArrayList list = new System.Collections.ArrayList();
                list.Add(new TabInfo()
                {
                    Filename = file,
                });

                ListBoxItem searchItem = CreateTemplateListBoxItem(file, title, summary1, image);

                if (internationalizedKeywords.Count > 0)
                {
                    list.Add(internationalizedKeywords);
                }

                ((StackPanel)searchItem.Content).Tag = list;
                TemplatesListBox.Items.Add(searchItem);

                CTTreeViewItem item = new CTTreeViewItem(file, title, summary2, image) { Background = bg };
                ToolTipService.SetShowDuration(item, Int32.MaxValue);
                item.MouseDoubleClick += TemplateItemDoubleClick;
                parent.Items.Add(item);
            }
        }

        private ListBoxItem CreateTemplateListBoxItem(FileInfo file, string title, Inline tooltip, ImageSource imageSource)
        {
            Image image = new Image();
            image.Source = imageSource;
            image.Margin = new Thickness(16, 0, 5, 0);
            image.Height = 25;
            image.Width = 25;
            TextBlock textBlock = new TextBlock();
            textBlock.FontWeight = FontWeights.DemiBold;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Text = title;
            textBlock.Tag = title;
            StackPanel stackPanel = new StackPanel();
            stackPanel.Margin = new Thickness(0, 2, 0, 2);
            stackPanel.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Children.Add(image);
            stackPanel.Children.Add(textBlock);

            ListBoxItem navItem = new ListBoxItem();
            navItem.Content = stackPanel;
            navItem.Tag = new KeyValuePair<string, string>(file.FullName, title);


            navItem.MouseDoubleClick += TemplateItemDoubleClick;
            if (tooltip != null)
            {
                var tooltipBlock = new TextBlock(tooltip) {TextWrapping = TextWrapping.Wrap, MaxWidth = 400};
                navItem.ToolTip = tooltipBlock;
            }

            ToolTipService.SetShowDuration(navItem, Int32.MaxValue);
            return navItem;
        }

        public event EventHandler<TemplateOpenEventArgs> TemplateLoaded;

        private void TemplateItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var infos = ((KeyValuePair<string, string>)((FrameworkElement)sender).Tag);
            if (infos.Key != null && Path.GetExtension(infos.Key) != null)
            {
                var fileExt = Path.GetExtension(infos.Key).ToLower().Substring(1);
                if (ComponentInformations.EditorExtension != null && ComponentInformations.EditorExtension.ContainsKey(fileExt))
                {
                    Type editorType = ComponentInformations.EditorExtension[fileExt];
                    TabInfo info = new TabInfo();
                    if (sender is CTTreeViewItem)
                    {
                        var templateItem = (CTTreeViewItem)sender;

                        info = new TabInfo()
                        {
                            Filename = templateItem.File,
                        };
                        //var tooltipInline = ((TextBlock) templateItem.ToolTip).Inlines.FirstOrDefault();
                        //if (tooltipInline != null)
                        //{
                        //    editor.Presentation.ToolTip = new TextBlock(tooltipInline) { TextWrapping = TextWrapping.Wrap, MaxWidth = 400 };
                        //}
                    }
                    else if (sender is ListBoxItem)
                    {
                        var searchItem = (ListBoxItem) sender;
                        info = (TabInfo)((System.Collections.ArrayList)((FrameworkElement)searchItem.Content).Tag)[0];

                        //var tooltipInline = ((TextBlock)searchItem.ToolTip).Inlines.FirstOrDefault();
                        //if (tooltipInline != null)
                        //{
                        //    editor.Presentation.ToolTip = new TextBlock(tooltipInline) { TextWrapping = TextWrapping.Wrap, MaxWidth = 400 };
                        //}
                        //editor.Presentation.Tag = ((Image)((StackPanel)searchItem.Content).Children[0]).Source;
                    }

                    if (TemplateLoaded != null)
                    {
                        TemplateLoaded.Invoke(this, new TemplateOpenEventArgs() { Info = info, Type = editorType });
                    }
                    //OnOpenTab(editor, info, null);     //rename tab header
                    _recentFileList.AddRecentFile(infos.Key);
                }
            }
        }

        private void TemplateSearchInputChanged(object sender, TextChangedEventArgs e)
        {
            List<string> searchWords = new List<string>();
            List<string> hitSearchWords = new List<string>();

            var searchWordsArray = SearchTextBox.Text.ToLower().Split(new char[] { ',', ' ' });
            foreach (var searchword in searchWordsArray)
            {
                var sw = searchword.Trim();
                if (sw != "" && !searchWords.Contains(sw)) searchWords.Add(sw);
            }

            if (searchWords.Count == 0)
            {
                TemplatesListBox.Visibility = Visibility.Collapsed;
                TemplatesTreeView.Visibility = Visibility.Visible;
                return;
            }

            TemplatesListBox.Visibility = Visibility.Visible;
            TemplatesTreeView.Visibility = Visibility.Collapsed;

            foreach (ListBoxItem item in TemplatesListBox.Items)
            {
                TextBlock textBlock = (TextBlock)((Panel)item.Content).Children[1];
                string title = (string)textBlock.Tag;

                // search template title for the search words
                hitSearchWords.Clear();
                SearchForHitWords(searchWords, hitSearchWords, new List<string>() { title });

                bool allSearchWordsFound = hitSearchWords.Count == searchWords.Count;

                // if the template title doesn't contain all search words, search also the keywords
                if (!allSearchWordsFound)
                {
                    List<string> keywords = SearchMatchingKeywords(item, searchWords, hitSearchWords);
                    allSearchWordsFound = hitSearchWords.Count == searchWords.Count;
                    if (allSearchWordsFound)
                        title += " (" + String.Join(", ", keywords) + ")";
                }

                item.Visibility = allSearchWordsFound ? Visibility.Visible : Visibility.Collapsed;

                // display matching text segments in bold font
                if (allSearchWordsFound)
                {
                    textBlock.Inlines.Clear();
                    int begin = 0;
                    int length;
                    int end = IndexOfFirstHit(title, searchWordsArray, begin, out length);
                    while (end != -1)
                    {
                        textBlock.Inlines.Add(title.Substring(begin, end - begin));
                        textBlock.Inlines.Add(new Bold(new Italic(new Run(title.Substring(end, length)))));
                        begin = end + length;
                        end = IndexOfFirstHit(title, searchWordsArray, begin, out length);
                    }
                    textBlock.Inlines.Add(title.Substring(begin, title.Length - begin));
                }
            }
        }

        private int IndexOfFirstHit(string text, string[] searchWords, int begin, out int length)
        {
            length = 0;
            int res = -1;
            foreach (var searchWord in searchWords)
            {
                if (searchWord != "")
                {
                    int e = text.IndexOf(searchWord, begin, StringComparison.OrdinalIgnoreCase);
                    if ((e > -1) && ((e < res) || (res < 0)))
                    {
                        res = e;
                        length = searchWord.Length;
                    }
                }
            }
            return res;
        }

        private List<string> SearchMatchingKeywords(ListBoxItem item, List<string> searchWords, List<string> hitSearchWords)
        {
            List<string> hitKeyWords = new List<string>();

            var tag = ((System.Collections.ArrayList)((FrameworkElement)item.Content).Tag);

            if (tag.Count == 2)
            {
                var internationalizedKeywords = (Dictionary<string, List<string>>)tag[1];
                List<string> langs = new List<string>() { CultureInfo.CurrentCulture.TextInfo.CultureName, CultureInfo.CurrentCulture.TwoLetterISOLanguageName };
                if (!langs.Contains("en")) langs.Add("en"); // check english keywords in any case
                foreach (var lang in langs)
                    if(lang!=null && internationalizedKeywords.ContainsKey(lang))
                        SearchForHitWords(searchWords, hitSearchWords, internationalizedKeywords[lang], hitKeyWords);
            }

            return hitKeyWords;
        }

        private void SearchForHitWords(List<string> searchWords, List<string> hitSearchWords, List<string> words, List<string> hitWords=null)
        {
            foreach (var searchWord in searchWords)
            {
                if (hitSearchWords.Contains(searchWord)) continue;
                foreach (var word in words)
                    if (word.ToLower().Contains(searchWord.ToLower()))
                    {
                        if (hitWords != null && !hitWords.Contains(word)) hitWords.Add(word);
                        if (!hitSearchWords.Contains(searchWord)) hitSearchWords.Add(searchWord);
                    }
            }
        }

        private void TemplateSearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SearchTextBox.Text = "";
            }
        }

        private string GetRelativePathBySubtracting(string path1, string path2)
        {
            var rel = path2.Substring(path1.Length);
            if (rel[0] == Path.DirectorySeparatorChar)
            {
                return rel.Substring(1);
            }
            return rel;
        }

        public void ShowHelp()
        {
            FrameworkElement item = null;
            if (TemplatesTreeView.Visibility == Visibility.Visible)
            {
                if (TemplatesTreeView.SelectedItem != null)
                {
                    item = (CTTreeViewItem) TemplatesTreeView.SelectedItem;
                }
            }
            else
            {
                if (TemplatesListBox.SelectedItem != null)
                {
                    item = (FrameworkElement) TemplatesListBox.SelectedItem;
                }
            }

            if (item == null)
            {
                return;
            }

            var infos = ((KeyValuePair<string, string>)item.Tag);
            var rel = GetRelativePathBySubtracting(_templatesDir, infos.Key);
            OnlineHelp.InvokeShowDocPage(new OnlineHelp.TemplateType(rel));
        }
    }

    public class TemplateOpenEventArgs : EventArgs
    {
        public TabInfo Info { get; set; }

        public Type Type { get; set; }
    }
}
