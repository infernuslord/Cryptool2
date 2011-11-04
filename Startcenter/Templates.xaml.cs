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

        public string TemplatesDir
        {
            set 
            {
                if (value != null)
                {
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
            CTTreeViewItem item = new CTTreeViewItem(templateDir.Name, true);
            treeView.Items.Add(item);
            if (templateDir.Exists)
            {
                foreach (var subDirectory in templateDir.GetDirectories())
                    handleTemplateDirectories(subDirectory, item);

                MakeTemplateInformation(templateDir, item);
            }

            item.IsExpanded = true;
        }

        private void handleTemplateDirectories(DirectoryInfo directory, CTTreeViewItem parent)
        {
            if (directory == null)
                return;

            CTTreeViewItem item = new CTTreeViewItem(directory.Name, true);
            parent.Items.Add(item);

            foreach (var subDirectory in directory.GetDirectories())
                handleTemplateDirectories(subDirectory, item);

            MakeTemplateInformation(directory, item);
        }

        private void MakeTemplateInformation(DirectoryInfo info, CTTreeViewItem parent)
        {
            SolidColorBrush bg = Brushes.Transparent;

            foreach (var file in info.GetFiles().Where(x => ((x.Extension.ToLower() == ".cte") 
                || (x.Extension.ToLower() == ".cwm") 
                || (x.Extension.ToLower() == ".component"))))
            {
                if (file.Name.StartsWith("."))
                {
                    continue;
                }
                bool component = (file.Extension.ToLower() == ".component");
                bool cte = (file.Extension.ToLower() == ".cte");
                string title = null;
                Inline description1 = null;
                Inline description2 = null;
                string xmlFile = Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + ".xml");
                string iconFile = null;
                Dictionary<string, List<string>> internationalizedKeywords = new Dictionary<string, List<string>>();
                if (File.Exists(xmlFile))
                {
                    try
                    {
                        XElement xml = XElement.Load(xmlFile);
                        var titleElement = Helper.GetGlobalizedElementFromXML(xml, "title");
                        if (titleElement != null)
                            title = titleElement.Value;

                        var descriptionElement = Helper.GetGlobalizedElementFromXML(xml, "description");
                        if (descriptionElement != null)
                        {
                            description1 = Helper.ConvertFormattedXElement(descriptionElement);
                            description2 = Helper.ConvertFormattedXElement(descriptionElement);
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
                    string desc = null;
                    if (component)
                    {
                        title = file.Name;
                        desc = Properties.Resources.This_is_a_standalone_component_;
                    }
                    else
                    {
                        title = Path.GetFileNameWithoutExtension(file.Name).Replace("-", " ").Replace("_", " ");
                        if (cte)
                            desc = Properties.Resources.This_is_an_AnotherEditor_file_;
                        else
                            desc = Properties.Resources.This_is_a_WorkspaceManager_file_;
                    }

                    description1 = new Run(desc);
                    description2 = new Run(desc);
                }

                if (iconFile == null || !File.Exists(iconFile))
                    iconFile = Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + ".png");
                ImageSource image = null;
                if (File.Exists(iconFile))
                {
                    try
                    {
                        image = new BitmapImage(new Uri(iconFile));
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

                ListBoxItem searchItem = CreateTemplateListBoxItem(file, title, description1, image);
                if (internationalizedKeywords.Count > 0)
                {
                    ((StackPanel)searchItem.Content).Tag = internationalizedKeywords;
                }
                TemplatesListBox.Items.Add(searchItem);

                CTTreeViewItem item = new CTTreeViewItem(file, title, description2, image) { Background = bg };
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
            var tooltipBlock = new TextBlock(tooltip) { TextWrapping = TextWrapping.Wrap, MaxWidth = 400 };
            navItem.ToolTip = tooltipBlock;

            ToolTipService.SetShowDuration(navItem, Int32.MaxValue);
            return navItem;
        }

        private void TemplateItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var infos = ((KeyValuePair<string, string>)((FrameworkElement)sender).Tag);
            if (infos.Key != null && Path.GetExtension(infos.Key) != null)
            {
                var fileExt = Path.GetExtension(infos.Key).ToLower().Substring(1);
                if (fileExt == "component")     //standalone component
                {
                    var pluginName = Path.GetFileNameWithoutExtension(infos.Key);
                    if (ComponentInformations.AllLoadedPlugins.ContainsKey(pluginName))
                    {
                        var type = ComponentInformations.AllLoadedPlugins[pluginName];
                        var content = type.CreateObject();
                        OnOpenTab(content, type.GetPluginInfoAttribute().Caption, null);
                        content.Presentation.ToolTip = type.GetPluginInfoAttribute().ToolTip;
                    }
                }
                else if (ComponentInformations.EditorExtension != null && ComponentInformations.EditorExtension.ContainsKey(fileExt))
                {
                    Type editorType = ComponentInformations.EditorExtension[fileExt];
                    string filename = infos.Key;
                    string title = infos.Value;
                    var editor = OnOpenEditor(editorType, title, filename);
                    editor.Presentation.ToolTip = Properties.Resources.This_is_a_template;
                    if (sender is CTTreeViewItem)
                    {
                        CTTreeViewItem templateItem = (CTTreeViewItem)sender;
                        editor.Presentation.Tag = templateItem.Icon;
                    }
                    else if (sender is ListBoxItem)
                    {
                        var searchItem = (ListBoxItem) sender;
                        editor.Presentation.Tag = ((Image)((StackPanel)searchItem.Content).Children[0]).Source;
                    }

                    editor.Open(infos.Key);
                    OnOpenTab(editor, title, null);     //rename tab header
                    _recentFileList.AddRecentFile(infos.Key);
                }
            }
        }

        private void TemplateSearchInputChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox.Text == "")
            {
                TemplatesListBox.Visibility = Visibility.Collapsed;
                TemplatesTreeView.Visibility = Visibility.Visible;
            }
            else
            {
                TemplatesListBox.Visibility = Visibility.Visible;
                TemplatesTreeView.Visibility = Visibility.Collapsed;

                foreach (ListBoxItem item in TemplatesListBox.Items)
                {
                    var panel = (Panel)item.Content;
                    TextBlock textBlock = (TextBlock)panel.Children[1];
                    string text = (string)textBlock.Tag;

                    var searchWordsArray = SearchTextBox.Text.ToLower().Split(new char[] {',', ' '});
                    var searchWords = new List<string>(searchWordsArray);
                    bool hit = true;
                    for (int i = searchWords.Count() - 1; i >= 0; i--)
                    {
                        var sw = searchWords[i].ToLower().Trim();
                        if ((sw != "") && (!text.ToLower().Contains(sw)))
                        {
                            hit = false;
                        }
                        else
                        {
                            searchWords.RemoveAt(i);
                        }
                    }
                    if (!hit)
                    {
                        text = AppendTextWithHitKeywords(item, searchWords, text, ref hit);
                    }

                    Visibility visibility = hit ? Visibility.Visible : Visibility.Collapsed;
                    item.Visibility = visibility;

                    if (hit)
                    {
                        textBlock.Inlines.Clear();
                        int begin = 0;
                        int length;
                        int end = IndexOfFirstHit(text, searchWordsArray, begin, out length);
                        while (end != -1)
                        {
                            textBlock.Inlines.Add(text.Substring(begin, end - begin));
                            textBlock.Inlines.Add(new Bold(new Italic(new Run(text.Substring(end, length)))));
                            begin = end + length;
                            end = IndexOfFirstHit(text, searchWordsArray, begin, out length);
                        }
                        textBlock.Inlines.Add(text.Substring(begin, text.Length - begin));
                    }
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

        private string AppendTextWithHitKeywords(ListBoxItem item, List<string> searchWords, string text, ref bool hit)
        {
            var tag = ((StackPanel) item.Content).Tag;
            if (tag != null)
            {
                List<string> hitKeywords = new List<string>();
                var internationalizedKeywords = (Dictionary<string, List<string>>) tag;
                string lang = null;
                if (internationalizedKeywords.ContainsKey(CultureInfo.CurrentCulture.TextInfo.CultureName))
                {
                    lang = CultureInfo.CurrentCulture.TextInfo.CultureName;
                }
                else if (internationalizedKeywords.ContainsKey(CultureInfo.CurrentCulture.TwoLetterISOLanguageName))
                {
                    lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                }

                SearchForHitKeywords(searchWords, internationalizedKeywords, lang, hitKeywords);
                if (lang != "en")
                {
                    SearchForHitKeywords(searchWords, internationalizedKeywords, "en", hitKeywords);
                }

                if (hitKeywords.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.Append(text);
                    sb.Append(" (");
                    foreach (var keyword in hitKeywords)
                    {
                        sb.Append(keyword);
                        sb.Append(", ");
                    }
                    sb.Remove(sb.Length - 2, 2);
                    sb.Append(")");
                    text = sb.ToString();
                    hit = true;
                }
                return text;
            }
            return text;
        }

        private void SearchForHitKeywords(List<string> searchWords, Dictionary<string, List<string>> internationalizedKeywords, string lang, List<string> hitKeywords)
        {
            List<string> searchWordsLeft = new List<string>(searchWords);
            if (lang != null)
            {
                foreach (var keyword in internationalizedKeywords[lang])
                {
                    for (int i = searchWordsLeft.Count - 1; i >= 0; i--)
                    {
                        var sw = searchWordsLeft[i].ToLower().Trim();
                        if ((sw != "") && (keyword.ToLower().Contains(sw)))
                        {
                            hitKeywords.Add(keyword);
                            searchWordsLeft.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            if (searchWordsLeft.Count > 0)
            {
                hitKeywords.Clear();
            }
        }

        private void TemplateSearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SearchTextBox.Text = "";
            }
        }
    }
}
