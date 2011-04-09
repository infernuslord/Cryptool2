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
                    FillTemplatesNavigationPane(templateDir, TempaltesListBox);
                }
            }
        }

        public event OpenEditorHandler OnOpenEditor;

        public Templates()
        {
            InitializeComponent();
        }

        private void FillTemplatesNavigationPane(DirectoryInfo templateDir, System.Windows.Controls.TreeView parent)
        {

            CTTreeViewItem item = new CTTreeViewItem(templateDir.Name, true);
            parent.Items.Add(item);
            foreach (var subDirectory in templateDir.GetDirectories())
                handleTemplateDirectories(subDirectory, item);

            MakeTemplateInformation(templateDir, item);
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
            return;
        }

        private void MakeTemplateInformation(DirectoryInfo info, CTTreeViewItem parent)
        {
            var styleInformationXML = Path.Combine(info.FullName, "StyleInformation.xml");
            SolidColorBrush bg = Brushes.Transparent;

            if (File.Exists(styleInformationXML))
            {
                XElement xml = XElement.Load(styleInformationXML);
                if (xml.Element("background") != null)
                {
                    Color color = (Color)ColorConverter.ConvertFromString(xml.Element("background").Value);
                    parent.Background = bg = new SolidColorBrush(Color.FromArgb(80, color.R, color.G, color.B));
                }
            }

            foreach (var file in info.GetFiles().Where(x => ((x.Extension.ToLower() == ".cte") || (x.Extension.ToLower() == ".cwm"))))
            {
                bool cte = (file.Extension.ToLower() == ".cte");
                string title = null;
                string description = null;
                string xmlFile = Path.Combine(file.Directory.FullName, file.Name.Substring(0, file.Name.Length - 4) + ".xml");
                string iconFile = null;
                if (File.Exists(xmlFile))
                {
                    XElement xml = XElement.Load(xmlFile);
                    title = GetGlobalizedElementFromXML(xml, "title");
                    description = GetGlobalizedElementFromXML(xml, "description");
                    if (xml.Element("icon") != null && xml.Element("icon").Attribute("file") != null)
                        iconFile = Path.Combine(file.Directory.FullName, xml.Element("icon").Attribute("file").Value);
                }
                if (title == null)
                {
                    title = file.Name.Remove(file.Name.Length - 4).Replace("-", " ").Replace("_", " ");
                    if (cte)
                        description = "This is an AnotherEditor template.";
                    else
                        description = "This is a WorkspaceManager template.";
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

                //ListBoxItem navItem = CreateSampleListBoxItem(file, title, description, image);
                //navListBoxSearch.Items.Add(navItem);

                CTTreeViewItem item = new CTTreeViewItem(file, title, description, image) { Background = bg };
                item.ToolTip = string.Format("Sample \"{0}\"", title);
                ToolTipService.SetShowDuration(item, Int32.MaxValue);
                //item.MouseDoubleClick += sampleNavItem_doubleClick;
                parent.Items.Add(item);
            }
        }

        private string GetGlobalizedElementFromXML(XElement xml, string element)
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
                    return xml.Element(element).Value;
                else
                    return null;
            }

            return foundElements.First().Value;
        }

        private void TemplateSearchInputChanged(object sender, TextChangedEventArgs e)
        {
            //if (PluginSearchTextBox.Text == "")
            //{
            //    if (navPaneItemSearch.IsSelected)
            //        navPaneItemClassic.IsSelected = true;
            //    navPaneItemSearch.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    navPaneItemSearch.Visibility = Visibility.Visible;
            //    navPaneItemSearch.IsSelected = true;

            //    foreach (ListBoxItem items in navListBoxSearch.Items)
            //    {
            //        var panel = (System.Windows.Controls.Panel)items.Content;
            //        TextBlock textBlock = (TextBlock)panel.Children[1];
            //        string text = textBlock.Text;

            //        bool hit = text.ToLower().Contains(PluginSearchTextBox.Text.ToLower());
            //        Visibility visibility = hit ? Visibility.Visible : Visibility.Collapsed;
            //        items.Visibility = visibility;

            //        if (hit)
            //        {
            //            textBlock.Inlines.Clear();
            //            int begin = 0;
            //            int end = text.IndexOf(PluginSearchTextBox.Text, begin, StringComparison.OrdinalIgnoreCase);
            //            while (end != -1)
            //            {
            //                textBlock.Inlines.Add(text.Substring(begin, end - begin));
            //                textBlock.Inlines.Add(new Bold(new Italic(new Run(text.Substring(end, PluginSearchTextBox.Text.Length)))));
            //                begin = end + PluginSearchTextBox.Text.Length;
            //                end = text.IndexOf(PluginSearchTextBox.Text, begin, StringComparison.OrdinalIgnoreCase);
            //            }
            //            textBlock.Inlines.Add(text.Substring(begin, text.Length - begin));
            //        }
            //    }
            //}
        }

        private void TemplateSearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                PluginSearchTextBox.Text = "";
            }
        }
    }
}
