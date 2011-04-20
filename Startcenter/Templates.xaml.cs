using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Editor;
using Path = System.IO.Path;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for Templates.xaml
    /// </summary>
    public partial class Templates : UserControl
    {
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
            foreach (var subDirectory in templateDir.GetDirectories())
                handleTemplateDirectories(subDirectory, item);

            MakeTemplateInformation(templateDir, item);
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

            //var styleInformationXML = Path.Combine(info.FullName, "StyleInformation.xml");
            //if (File.Exists(styleInformationXML))
            //{
            //    XElement xml = XElement.Load(styleInformationXML);
            //    if (xml.Element("background") != null)
            //    {
            //        Color color = (Color)ColorConverter.ConvertFromString(xml.Element("background").Value);
            //        parent.Background = bg = new SolidColorBrush(Color.FromArgb(80, color.R, color.G, color.B));
            //    }
            //}

            foreach (var file in info.GetFiles().Where(x => ((x.Extension.ToLower() == ".cte") || (x.Extension.ToLower() == ".cwm"))))
            {
                bool cte = (file.Extension.ToLower() == ".cte");
                string title = null;
                Inline description1 = null;
                Inline description2 = null;
                string xmlFile = Path.Combine(file.Directory.FullName, file.Name.Substring(0, file.Name.Length - 4) + ".xml");
                string iconFile = null;
                if (File.Exists(xmlFile))
                {
                    XElement xml = XElement.Load(xmlFile);
                    var titleElement = GetGlobalizedElementFromXML(xml, "title");
                    if (titleElement != null)
                        title = titleElement.Value;

                    var descriptionElement = GetGlobalizedElementFromXML(xml, "description");
                    if (descriptionElement != null)
                    {
                        description1 = ConvertFormattedXElement(descriptionElement);
                        description2 = ConvertFormattedXElement(descriptionElement);
                    }

                    if (xml.Element("icon") != null && xml.Element("icon").Attribute("file") != null)
                        iconFile = Path.Combine(file.Directory.FullName, xml.Element("icon").Attribute("file").Value);
                }
                if (title == null)
                {
                    title = file.Name.Remove(file.Name.Length - 4).Replace("-", " ").Replace("_", " ");
                    string desc = null;
                    if (cte)
                        desc = "This is an AnotherEditor template.";
                    else
                        desc = "This is a WorkspaceManager template.";
                    
                    description1 = new Run(desc);
                    description2 = new Run(desc);
                }

                if (iconFile == null || !File.Exists(iconFile))
                    iconFile = Path.Combine(file.Directory.FullName, file.Name.Substring(0, file.Name.Length - 4) + ".png");
                ImageSource image;
                if (File.Exists(iconFile))
                {
                    image = new BitmapImage(new Uri(iconFile));
                }
                else
                {
                    Type editorType = ComponentInformations.EditorExtension[file.Extension.Remove(0, 1)];
                    image = editorType.GetImage(0).Source;
                }

                ListBoxItem searchItem = CreateTemplateListBoxItem(file, title, description1, image);
                TemplatesListBox.Items.Add(searchItem);

                CTTreeViewItem item = new CTTreeViewItem(file, title, description2, image) { Background = bg };
                ToolTipService.SetShowDuration(item, Int32.MaxValue);
                item.MouseDoubleClick += TemplateItemDoubleClick;
                parent.Items.Add(item);
            }
        }

        private Inline ConvertFormattedXElement(XElement xelement)
        {
            var span = new Span();

            foreach (var node in xelement.Nodes())
            {
                if (node is XText)
                {
                    var lines = ((XText) node).Value.Split('\n');
                    var line = lines.Aggregate((a, b) => a + b.Trim());
                    span.Inlines.Add(new Run(line));
                }
                else if (node is XElement)
                {
                    var nodeName = ((XElement)node).Name.ToString();
                    switch (nodeName)
                    {
                        case "b":
                            var nodeRep = ConvertFormattedXElement((XElement)node);
                            span.Inlines.Add(new Bold(nodeRep));
                            break;
                        case "i":
                            nodeRep = ConvertFormattedXElement((XElement)node);
                            span.Inlines.Add(new Italic(nodeRep));
                            break;
                        case "u":
                            nodeRep = ConvertFormattedXElement((XElement)node);
                            span.Inlines.Add(new Underline(nodeRep));
                            break;
                        case "newline":
                            span.Inlines.Add(new LineBreak());
                            break;
                        case "external":
                            var reference = ((XElement)node).Attribute("ref");
                            if (reference != null)
                            {
                                var linkText = ConvertFormattedXElement((XElement)node);
                                if (linkText == null)
                                {
                                    linkText = new Run(reference.Value);
                                }
                                var link = new Hyperlink(linkText);
                                link.NavigateUri = new Uri(reference.Value);
                                span.Inlines.Add(link);
                            }
                            break;
                        case "pluginRef":
                            var plugin = ((XElement)node).Attribute("plugin");
                            if (plugin != null)
                            {
                                var linkText = ConvertFormattedXElement((XElement)node);
                                if (linkText == null)
                                {
                                    linkText = new Run(plugin.Value);
                                }
                                span.Inlines.Add(linkText);
                            }
                            break;
                        default:
                            continue;
                    }
                }
            }

            if (span.Inlines.Count == 0)
                return null;
            if (span.Inlines.Count == 1)
                return span.Inlines.First();

            return span;
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
                if (ComponentInformations.EditorExtension != null && ComponentInformations.EditorExtension.ContainsKey(fileExt))
                {
                    Type editorType = ComponentInformations.EditorExtension[fileExt];
                    string title = infos.Value;
                    var editor = OnOpenEditor(editorType, title);
                    editor.Presentation.ToolTip = "This is a template";
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
                }
            }
        }

        private XElement GetGlobalizedElementFromXML(XElement xml, string element)
        {
            CultureInfo currentLang = System.Globalization.CultureInfo.CurrentCulture;

            var allElements = xml.Elements(element);
            IEnumerable<XElement> foundElements = null;

            if (allElements.Any())
            {
                foundElements = from descln in allElements where descln.Attribute("lang").Value == currentLang.TextInfo.CultureName select descln;
                if (!foundElements.Any())
                {
                    foundElements = from descln in allElements where descln.Attribute("lang").Value == currentLang.TwoLetterISOLanguageName select descln;
                    if (!foundElements.Any())
                        foundElements = from descln in allElements where descln.Attribute("lang").Value == "en" select descln;
                }
            }

            if (foundElements == null || !foundElements.Any())
            {
                if (xml.Element(element) != null)
                    return xml.Element(element);
                else
                    return null;
            }

            return foundElements.First();
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

                foreach (ListBoxItem items in TemplatesListBox.Items)
                {
                    var panel = (Panel)items.Content;
                    TextBlock textBlock = (TextBlock)panel.Children[1];
                    string text = textBlock.Text;

                    bool hit = text.ToLower().Contains(SearchTextBox.Text.ToLower());
                    Visibility visibility = hit ? Visibility.Visible : Visibility.Collapsed;
                    items.Visibility = visibility;

                    if (hit)
                    {
                        textBlock.Inlines.Clear();
                        int begin = 0;
                        int end = text.IndexOf(SearchTextBox.Text, begin, StringComparison.OrdinalIgnoreCase);
                        while (end != -1)
                        {
                            textBlock.Inlines.Add(text.Substring(begin, end - begin));
                            textBlock.Inlines.Add(new Bold(new Italic(new Run(text.Substring(end, SearchTextBox.Text.Length)))));
                            begin = end + SearchTextBox.Text.Length;
                            end = text.IndexOf(SearchTextBox.Text, begin, StringComparison.OrdinalIgnoreCase);
                        }
                        textBlock.Inlines.Add(text.Substring(begin, text.Length - begin));
                    }
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
    }
}
