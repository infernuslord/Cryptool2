using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using System.Threading;
using System.Collections;
using System.Globalization;
using Cryptool.PluginBase;
using WorkspaceManager.Model;

namespace Wizard
{
    /// <summary>
    /// Interaction logic for WizardControl.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Wizard.Properties.Resources")]
    public partial class WizardControl : UserControl
    {
        private class PageInfo
        {
            public string name;
            public string description;
            public string image;
            public XElement tag;
        }

        List<PageInfo> currentHistory = new List<PageInfo>();
        private Dictionary<string, bool> selectedCategories = new Dictionary<string, bool>();
        private SolidColorBrush selectionBrush = new SolidColorBrush();
        private const string configXMLPath = "Wizard.Config.wizard.config.start.xml";
        private const string defaultLang = "en";
        private XElement wizardConfigXML;
        private Dictionary<string, PluginPropertyValue> propertyValueDict = new Dictionary<string, PluginPropertyValue>();
        private HashSet<TextBox> boxesWithWrongContent = new HashSet<TextBox>();

        internal event OpenTabHandler OnOpenTab;
        internal event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        internal string SamplesDir { set; private get; }

        public WizardControl()
        {
            try
            {
                // DEBUG HELP string[] names = this.GetType().Assembly.GetManifestResourceNames();

                Stream fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(configXMLPath);
                XElement xml = XElement.Load(fileStream);
                GenerateXML(xml);
            }
            catch (Exception)
            {
                
            }

            InitializeComponent();

            selectionBrush.Color = Color.FromArgb(255, 200, 220, 245);
            SetupPage(wizardConfigXML);
        }

        // generate the full XML tree for the wizard (recursive)
        private void GenerateXML(XElement xml)
        {
            try
            {
                //find all nested subcategories and add them to the tree
                IEnumerable<XElement> categories = xml.Elements("category");
                if (categories.Any())
                {
                    foreach (XElement cat in categories)
                    {
                        IEnumerable<XElement> files = cat.Elements("file");
                        if (files.Any())
                        {
                            foreach (XElement element in files)
                            {
                                XAttribute att = element.Attribute("resource");
                                if (att != null)
                                {
                                    string path = att.Value;
                                    Stream fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
                                    XElement sub = XElement.Load(fileStream);
                                    GenerateXML(sub);
                                    IEnumerable<XElement> elems = sub.Elements();
                                    if (elems.Any())
                                    {
                                        foreach (XElement ele in elems)
                                        {
                                            cat.Add(ele);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                wizardConfigXML = xml;

            }
            catch (Exception)
            {
                
            }
        }

        #region WidthConverter

        [ValueConversion(typeof(Double), typeof(Double))]
        private class WidthConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (double)value * (double)parameter;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private WidthConverter widthConverter = new WidthConverter();

        #endregion

        private void SetupPage(XElement element)
        {
            if ((element.Name == "loadSample") && (element.Attribute("file") != null) && (element.Attribute("title") != null))
            {
                LoadSample(element.Attribute("file").Value, element.Attribute("title").Value);
                abortButton_Click(null, null);
            }

            XElement parent = element.Parent;
            if (parent == null)
            {
                backButton.IsEnabled = false;
                abortButton.IsEnabled = false;
            }
            else
            {
                backButton.IsEnabled = true;
                abortButton.IsEnabled = true;
            }

            //nextButton.IsEnabled = false;

            //set headline
            XElement headline = FindElementsInElement(element, "headline").First();
            if (headline != null)
                taskHeader.Content = headline.Value;

            //set description label
            XElement desc = FindElementsInElement(element, "desc").First();
            if (desc != null)
                descHeader.Content = desc.Value;


            if (element.Name == "input" || element.Name == "output")
            {
                categoryGrid.Visibility = Visibility.Hidden;
                inputPanel.Visibility = Visibility.Visible;

                var inputs = from el in element.Elements()
                             where el.Name == "inputBox" || el.Name == "comboBox" || el.Name == "checkBox" 
                             select el;

                inputStack.Children.Clear();

                inputPanel.Tag = element.Element("input");
                if (inputPanel.Tag == null)
                    inputPanel.Tag = element.Element("category");
                if (inputPanel.Tag == null)
                {
                    inputPanel.Tag = element.Element("loadSample");
                    if (inputPanel.Tag != null)
                        SwitchNextButtonContent();
                }
                if (inputPanel.Tag == null)
                    inputPanel.Tag = element.Element("output");

                FillInputStack(inputs, element.Name.ToString());

                string id = GetElementID((XElement)inputPanel.Tag);

            }
            else if (element.Name == "category")
            {
                categoryGrid.Visibility = Visibility.Visible;
                inputPanel.Visibility = Visibility.Hidden;
                
                radioButtonStackPanel.Children.Clear();

                //generate radio buttons
                var options = from el in element.Elements()
                              where el.Name == "category" || el.Name == "input" || el.Name == "loadSample" || el.Name == "output"
                              select el;

                if (options.Any())
                {
                    bool isSelected = false;

                    foreach (XElement ele in options)
                    {
                        Border border = new Border();
                        Label l = new Label();
                        Image i = new Image();
                        StackPanel sp = new StackPanel();

                        border.Child = sp;
                        border.VerticalAlignment = VerticalAlignment.Stretch;
                        border.CornerRadius = new CornerRadius(5, 0, 0, 5);
                        border.BorderBrush = Brushes.LightSeaGreen;

                        l.Height = 30;
                        l.HorizontalAlignment = HorizontalAlignment.Stretch;
                        XElement label = FindElementsInElement(ele, "name").First();
                        if (label != null)
                            l.Content = label.Value;

                        i.Width = 26;
                        string image = ele.Attribute("image").Value;
                        if (image != null)
                        {
                            ImageSource ims = (ImageSource) TryFindResource(image);
                            if (ims != null)
                            {
                                i.Source = ims;
                                sp.Children.Add(i);
                            }
                        }

                        sp.VerticalAlignment = VerticalAlignment.Stretch;
                        sp.HorizontalAlignment = HorizontalAlignment.Stretch;
                        sp.Orientation = Orientation.Horizontal;
                        sp.Children.Add(l);

                        RadioButton rb = new RadioButton();
                        string id = GetElementID(ele);
                        rb.Checked += rb_Checked;
                        rb.HorizontalAlignment = HorizontalAlignment.Stretch;
                        rb.VerticalAlignment = VerticalAlignment.Stretch;
                        rb.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                        rb.Content = border;
                        rb.Tag = ele;
                        if (ele.Name == "loadSample")
                        {
                            RoutedEventHandler rbEvent = delegate
                                              {
                                                  SwitchNextButtonContent();
                                              };
                            rb.Checked += rbEvent;
                            rb.Unchecked += rbEvent;
                        }

                        radioButtonStackPanel.Children.Add(rb);
                        bool wasSelected = false;
                        selectedCategories.TryGetValue(GetElementID(ele), out wasSelected);
                        if (wasSelected)
                        {
                            rb.IsChecked = true;
                            isSelected = true;
                        }
                    }

                    if (!isSelected)
                    {
                        RadioButton b = (RadioButton)radioButtonStackPanel.Children[0];
                        b.IsChecked = true;
                        selectedCategories.Remove(GetElementID((XElement)b.Tag));
                        selectedCategories.Add(GetElementID((XElement)b.Tag), true);
                    }

                }
            }
        }

        private void FillInputStack(IEnumerable<XElement> inputs, string type)
        {
            var inputFieldStyle = (Style)FindResource("InputFieldStyle");

            foreach (var input in inputs)
            {
                try
                {
                    var description = new Label();
                    description.Content = FindElementsInElement(input, "description").First().Value.Trim();
                    description.HorizontalAlignment = HorizontalAlignment.Left;
                    description.FontWeight = FontWeights.Bold;
                    inputStack.Children.Add(description);

                    bool isInput = (type == "input");

                    Control inputElement = CreateInputElement(input, inputFieldStyle, isInput);

                    //Set width:
                    if (inputElement != null && input.Attribute("width") != null)
                    {
                        string width = input.Attribute("width").Value.Trim();
                        if (width.EndsWith("%"))
                        {
                            double percentage;
                            if (Double.TryParse(width.Substring(0, width.Length - 1), out percentage))
                            {
                                percentage /= 100;
                                Binding binding = new Binding("ActualWidth");
                                binding.Source = inputStack;
                                binding.Converter = widthConverter;
                                binding.ConverterParameter = percentage;
                                inputElement.SetBinding(FrameworkElement.WidthProperty, binding);
                            }
                        }
                        else
                        {
                            double widthValue;
                            if (Double.TryParse(width, out widthValue))
                            {
                                inputElement.Width = widthValue;
                            }
                        }
                    }

                    //Set alignment
                    if (inputElement != null && input.Attribute("alignment") != null)
                    {
                        switch (input.Attribute("alignment").Value.Trim().ToLower())
                        {
                            case "right":
                                inputElement.HorizontalAlignment = HorizontalAlignment.Right;
                                break;
                            case "left":
                                inputElement.HorizontalAlignment = HorizontalAlignment.Left;
                                break;
                            case "center":
                                inputElement.HorizontalAlignment = HorizontalAlignment.Center;
                                break;
                            case "stretch":
                                inputElement.HorizontalAlignment = HorizontalAlignment.Stretch;
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    GuiLogMessage(string.Format("Error while creating wizard element {0}: {1}", input, e.Message), NotificationLevel.Error);
                }

            }
        }

        private Control CreateInputElement(XElement input, Style inputFieldStyle, bool isInput)
        {
            Control element = null;

            if (input.Name == "inputBox")
            {
                var textBox = new TextBox();
                textBox.Tag = input;
                textBox.AcceptsReturn = true;
                if (input.Attribute("visibleLines") != null)
                {
                    int visibleLines;
                    if (Int32.TryParse(input.Attribute("visibleLines").Value.Trim(), out visibleLines))
                    {
                        textBox.MinLines = visibleLines;
                        textBox.MaxLines = visibleLines;
                    }
                }
                textBox.Style = inputFieldStyle;
                inputStack.Children.Add(textBox);

                if (isInput)
                {
                    if (input.Attribute("regex") != null)
                    {
                        var regex = new Regex(input.Attribute("regex").Value, RegexOptions.Compiled);

                        textBox.TextChanged += delegate
                        {
                            CheckRegex(textBox, regex);
                        };
                    }

                    if (propertyValueDict.ContainsKey(GetElementID(input)))
                        textBox.Text = (string) propertyValueDict[GetElementID(input)].Value;
                    else if (input.Attribute("defaultValue") != null)
                        textBox.Text = input.Attribute("defaultValue").Value.Trim();
                }
                else
                {
                    if (input.Attribute("plugin") != null && input.Attribute("property") != null)
                    {
                        var plugin = input.Attribute("plugin").Value;
                        var property = input.Attribute("property").Value;
                        foreach (var e in propertyValueDict.Values)
                        {
                            if (e.PluginName == plugin && e.PropertyName == property)
                                textBox.Text = (String)e.Value;
                        }
                    }
                }

                element = textBox;
            }
            else if (input.Name == "comboBox")
            {
                ComboBox cb = new ComboBox();
                cb.Style = inputFieldStyle;
                cb.Tag = input;

                var items = FindElementsInElement(input, "item");
                foreach (var item in items)
                {
                    ComboBoxItem cbi = new ComboBoxItem();
                    if (item.Attribute("content") != null)
                        cbi.Content = item.Attribute("content").Value;
                    cb.Items.Add(cbi);
                }

                if (isInput)
                {
                    if (propertyValueDict.ContainsKey(GetElementID(input)))
                    {
                        if (propertyValueDict[GetElementID(input)].Value is int)
                        {
                            ComboBoxItem cbi =
                                (ComboBoxItem) cb.Items.GetItemAt((int) propertyValueDict[GetElementID(input)].Value);
                            cbi.IsSelected = true;
                        }
                    }
                    else if (input.Attribute("defaultValue") != null)
                    {
                        int i = 0;
                        if (Int32.TryParse(input.Attribute("defaultValue").Value.Trim(), out i))
                        {
                            ComboBoxItem cbi = (ComboBoxItem) cb.Items.GetItemAt(i);
                            cbi.IsSelected = true;
                        }
                    }
                }
                else
                {
                    if (input.Attribute("plugin") != null && input.Attribute("property") != null)
                    {
                        var plugin = input.Attribute("plugin").Value;
                        var property = input.Attribute("property").Value;
                        foreach (var e in propertyValueDict.Values)
                        {
                            if (e.PluginName == plugin && e.PropertyName == property)
                                cb.SelectedIndex = (int) e.Value;
                        }
                    }
                }

                inputStack.Children.Add(cb);
                element = cb;
            }
            else if (input.Name == "checkBox")
            {
                CheckBox cb = new CheckBox();
                cb.Tag = input;
                cb.Style = inputFieldStyle;

                if (input.Attribute("content") != null)
                    cb.Content = input.Attribute("content").Value;

                if (isInput)
                {
                    if (propertyValueDict.ContainsKey(GetElementID(input)))
                    {
                        string value = (string) propertyValueDict[GetElementID(input)].Value;
                        if (value.ToLower() == "true")
                            cb.IsChecked = true;
                        else
                            cb.IsChecked = false;
                    }
                    else if (input.Attribute("defaultValue") != null)
                    {
                        string value = input.Attribute("defaultValue").Value;
                        if (value.ToLower() == "true")
                            cb.IsChecked = true;
                        else
                            cb.IsChecked = false;
                    }
                }
                else
                {
                    if (input.Attribute("plugin") != null && input.Attribute("property") != null)
                    {
                        var plugin = input.Attribute("plugin").Value;
                        var property = input.Attribute("property").Value;
                        foreach (var e in propertyValueDict.Values)
                        {
                            if (e.PluginName == plugin && e.PropertyName == property)
                                cb.IsChecked = (bool)e.Value;
                        }
                    }
                }

                inputStack.Children.Add(cb);
                element = cb;
            }

            if (!isInput && element != null)
                element.IsEnabled = false;

            return element;
        }

        private void CreateHistory()
        {
            StackPanel historyStack = new StackPanel();
            historyStack.Orientation = Orientation.Horizontal;

            foreach (var page in currentHistory)
            {
                var p = new ContentControl();
                var bg = selectionBrush.Clone();
                bg.Opacity = 1 - (historyStack.Children.Count / (double)currentHistory.Count);
                var sp = new StackPanel { Orientation = Orientation.Horizontal, Background = bg };
                p.Content = sp;
                p.Tag = page.tag;
                p.MouseDoubleClick += new MouseButtonEventHandler(p_MouseDoubleClick);

                Polygon triangle = new Polygon();
                triangle.Points = new PointCollection();
                triangle.Points.Add(new Point(0, 0));
                triangle.Points.Add(new Point(0, 10));
                triangle.Points.Add(new Point(10, 5));
                triangle.Fill = bg;
                triangle.Stretch = Stretch.Uniform;
                triangle.Width = 32;
                sp.Children.Add(triangle);

                if (page.image != null && FindResource(page.image) != null)
                {
                    var im = new Image { Source = (ImageSource)FindResource(page.image), Width = 32 };
                    sp.Children.Add(im);
                }
                var nameLabel = new Label { Content = page.name };
                sp.Children.Add(nameLabel);
                p.ToolTip = page.description;
                var translateTranform = new TranslateTransform();
                triangle.RenderTransform = translateTranform;
                Binding binding = new Binding("ActualWidth");
                binding.Source = p;
                BindingOperations.SetBinding(translateTranform, TranslateTransform.XProperty, binding);

                historyStack.Children.Add(p);
            }

            history.Content = historyStack;
            history.ScrollToRightEnd();
        }

        void p_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var cc = (ContentControl)sender;
            var hs = (StackPanel)history.Content;
            int i = hs.Children.IndexOf(cc);
            history.Content = null;

            while (currentHistory.Count > i)
            {
                currentHistory.RemoveAt(currentHistory.Count - 1);
            }

            XElement parent = ((XElement)cc.Tag).Parent;

            if (parent == null)
                parent = wizardConfigXML;

            SetupPage(parent);

            CreateHistory();
        }

        private void CheckRegex(TextBox textBox, Regex regex)
        {
            var match = regex.Match(textBox.Text);
            if (!match.Success || match.Index != 0 || match.Length != textBox.Text.Length)
            {
                textBox.Style = (Style) FindResource("TextInputInvalid");
                GuiLogMessage(string.Format("Content of textbox does not fit regular expression {0}.", regex.ToString()), NotificationLevel.Error);
                boxesWithWrongContent.Add(textBox);
                nextButton.IsEnabled = false;
            }
            else
            {
                textBox.Style = null;
                boxesWithWrongContent.Remove(textBox);
                if (boxesWithWrongContent.Count == 0)
                    nextButton.IsEnabled = true;
            }
        }

        private string GetElementID(XElement element)
        {
            if (element != null && element.Parent != null)
            {
                return GetElementID(element.Parent) + "[" + element.Parent.Nodes().ToList().IndexOf(element) + "]." + element.Name;
            }
            else
                return "";
        }

        private void LoadSample(string file, string title)
        {
            file = SamplesDir + "\\" + file;

            var newEditor = new WorkspaceManager.WorkspaceManager();
            var model = ModelPersistance.loadModel(file, newEditor);
            foreach (var c in propertyValueDict)
            {
                var ppv = c.Value;
                try
                {
                    var plugin = model.AllPluginModels.Where(x => x.Name == ppv.PluginName).First().Plugin;
                    var settings = plugin.Settings;
                    var property = settings.GetType().GetProperty(ppv.PropertyName);
                    if (ppv.Value is string)
                        property.SetValue(settings, (string)ppv.Value, null);
                    else if (ppv.Value is int)
                        property.SetValue(settings, (int)ppv.Value, null);
                    else if (ppv.Value is bool)
                        property.SetValue(settings, (bool)ppv.Value, null);
                }
                catch (Exception)
                {
                    GuiLogMessage(string.Format("Failed settings plugin property {0}.{1} to \"{2}\"!", ppv.PluginName, ppv.PropertyName, ppv.Value), NotificationLevel.Error);
                    return;
                }
            }

            OnOpenTab(newEditor, title, null);
            foreach (PluginModel pluginModel in model.AllPluginModels)
            {
                pluginModel.Plugin.Initialize();
            }
            newEditor.Open(model);                   
        }

        void rb_Checked(object sender, RoutedEventArgs e)
        {
            ResetSelectionDependencies();
            RadioButton b = (RadioButton)sender;
            b.Background = Brushes.LightSeaGreen;
            Border c = (Border)b.Content;
            c.BorderThickness = new Thickness(1, 1, 0, 1);
            c.Background = selectionBrush;
            XElement ele = (XElement)b.Tag;
            selectedCategories.Remove(GetElementID(ele));
            selectedCategories.Add(GetElementID(ele), true);
            XElement desc = FindElementsInElement(ele, "description").First();
            if (desc != null)
                description.Text = desc.Value;
            nextButton.IsEnabled = true;
        }

        private void ResetSelectionDependencies()
        {
            for (int i = 0; i < radioButtonStackPanel.Children.Count; i++)
            {
                RadioButton b = (RadioButton)radioButtonStackPanel.Children[i];
                XElement ele = (XElement)b.Tag;
                selectedCategories.Remove(GetElementID(ele));
                selectedCategories.Add(GetElementID(ele), false);
                b.Background = Brushes.Transparent;
                Border c = (Border)b.Content;
                c.BorderThickness = new Thickness(0);
                c.Background = Brushes.Transparent;
            }
        }

        //finds elements according to the current language
        private IEnumerable<XElement> FindElementsInElement(XElement element, string xname)
        {
            CultureInfo currentLang = System.Globalization.CultureInfo.CurrentCulture;

            IEnumerable<XElement> allElements = element.Elements(xname);
            IEnumerable<XElement> foundElements = null;

            if (allElements.Any())
            {
                foundElements = from descln in allElements where descln.Attribute("lang").Value == currentLang.TextInfo.CultureName select descln;
                if (!foundElements.Any())
                {
                    foundElements = from descln in allElements where descln.Attribute("lang").Value == currentLang.TwoLetterISOLanguageName select descln;
                    if (!foundElements.Any())
                        foundElements = from descln in allElements where descln.Attribute("lang").Value == defaultLang select descln;
                }
            }

            if (foundElements == null || !foundElements.Any() || !allElements.Any())
            {
                List<XElement> fe = new List<XElement>();
                fe.Add(new XElement("dummy"));
                return fe;
            }

            return foundElements;
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            Storyboard mainGridStoryboardLeft = (Storyboard)FindResource("MainGridStoryboardNext1");
            mainGridStoryboardLeft.Begin();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Storyboard mainGridStoryboardLeft = (Storyboard)FindResource("MainGridStoryboardBack1");
            mainGridStoryboardLeft.Begin();
        }

        private void abortButton_Click(object sender, RoutedEventArgs e)
        {
            if (inputPanel.Visibility == Visibility.Visible)
            {
                if (inputPanel.Tag != null && ((XElement)inputPanel.Tag).Name == "loadSample")
                    SwitchNextButtonContent();
            }

            foreach (RadioButton rb in radioButtonStackPanel.Children)
            {
                if (rb.IsChecked != null && (bool)rb.IsChecked)
                    rb.IsChecked = false;
            }

            history.Content = null;
            currentHistory.Clear();
            propertyValueDict.Clear();
            ResetSelectionDependencies();
            radioButtonStackPanel.Children.Clear();
            selectedCategories.Clear();
            description.Text = "";
            SetupPage(wizardConfigXML);
        }

        private void SetNextContent(object sender, EventArgs e)
        {
            if (categoryGrid.Visibility == Visibility.Visible)
            {
                for (int i = 0; i < radioButtonStackPanel.Children.Count; i++)
                {
                    RadioButton b = (RadioButton) radioButtonStackPanel.Children[i];
                    if (b.IsChecked != null && (bool) b.IsChecked)
                    {
                        var ele = (XElement) b.Tag;
                        SetupPage(ele);
                        var page = new PageInfo()
                                       {
                                           name = FindElementsInElement(ele, "name").First().Value,
                                           description = FindElementsInElement(ele, "description").First().Value,
                                           tag = ele
                                       };

                        if (ele.Attribute("image") != null)
                        {
                            page.image = ele.Attribute("image").Value;
                        }

                        currentHistory.Add(page);

                        break;
                    }
                }
            }
            else if (inputPanel.Visibility == Visibility.Visible)
            {
                foreach (var child in inputStack.Children)
                {
                    SaveControlContent(child);
                }
                var nextElement = (XElement) inputPanel.Tag;
                SetupPage(nextElement);
            }

            Storyboard mainGridStoryboardLeft = (Storyboard)FindResource("MainGridStoryboardNext2");
            mainGridStoryboardLeft.Begin();

            CreateHistory();
        }

        private void SaveControlContent(object o)
        {
            if (o is TextBox || o is ComboBox || o is CheckBox)
            {
                Control c = (Control)o;
                XElement ele = (XElement)c.Tag;
                var id = GetElementID(ele);
                PluginPropertyValue newEntry = new PluginPropertyValue();
                if (ele.Attribute("plugin") != null && ele.Attribute("property") != null)
                {
                    if (o is TextBox)
                    {
                        TextBox textBox = (TextBox)o;
                        newEntry = new PluginPropertyValue()
                                   {
                                       PluginName = ele.Attribute("plugin").Value,
                                       PropertyName = ele.Attribute("property").Value,
                                       Value = textBox.Text
                                   };
                    }
                    else if (o is ComboBox)
                    {
                        ComboBox comboBox = (ComboBox)o;
                        newEntry = new PluginPropertyValue()
                        {
                            PluginName = ele.Attribute("plugin").Value,
                            PropertyName = ele.Attribute("property").Value,
                            Value = comboBox.SelectedIndex
                        };
                    }
                    else if (o is CheckBox)
                    {
                        CheckBox checkBox = (CheckBox)o;
                        if (checkBox.IsChecked != null)
                        {
                            newEntry = new PluginPropertyValue()
                            {
                                PluginName = ele.Attribute("plugin").Value,
                                PropertyName = ele.Attribute("property").Value,
                                Value = (bool)checkBox.IsChecked
                            };
                        }
                    }

                    if (!propertyValueDict.ContainsKey(id))
                        propertyValueDict.Add(id, newEntry);
                    else
                        propertyValueDict[id] = newEntry;
                } 
            }
        }

        private void DeleteControlContent(object o)
        {
            if (o is TextBox || o is ComboBox || o is CheckBox)
            {
                Control control = (Control)o;
                XElement ele = (XElement)control.Tag;
                var id = GetElementID(ele);

                if (propertyValueDict.ContainsKey(id))
                    propertyValueDict.Remove(id);
            }
        }


        private void SetLastContent(object sender, EventArgs e)
        {
            XElement ele = null;
            if (categoryGrid.Visibility == Visibility.Visible && radioButtonStackPanel.Children.Count > 0)
            {
                RadioButton b = (RadioButton) radioButtonStackPanel.Children[0];
                ele = (XElement) b.Tag;

                foreach (RadioButton rb in radioButtonStackPanel.Children)
                {
                    if (rb.IsChecked != null && (bool)rb.IsChecked)
                        rb.IsChecked = false;
                }

            }
            else if (inputPanel.Visibility == Visibility.Visible)
            {
                //foreach (var child in inputStack.Children)
                //{
                //    DeleteControlContent(child);
                //}
                boxesWithWrongContent.Clear();

                ele = (XElement) inputPanel.Tag;
                if (ele != null && ((XElement)inputPanel.Tag).Name == "loadSample")
                    SwitchNextButtonContent();
            }

            if (ele != null)
            {
                XElement grandParent = ele.Parent.Parent;
                if (grandParent == null)
                    grandParent = wizardConfigXML;

                if (grandParent.Name == "category" && currentHistory.Count > 0)
                    currentHistory.RemoveAt(currentHistory.Count - 1);

                SetupPage(grandParent);
            }

            Storyboard mainGridStoryboardLeft = (Storyboard)FindResource("MainGridStoryboardBack2");
            mainGridStoryboardLeft.Begin();

            CreateHistory();
        }

        private void GuiLogMessage(string message, NotificationLevel loglevel)
        {
            if (OnGuiLogNotificationOccured != null)
                OnGuiLogNotificationOccured(null, new GuiLogEventArgs(message, null, loglevel));
        }

        private void SwitchNextButtonContent()
        {
            var tmp = nextButton.Content;
            nextButton.Content = nextButton.Tag;
            nextButton.Tag = tmp;
        }

    }

    internal struct PluginPropertyValue
    {
        public string PluginName;
        public string PropertyName;
        public object Value;
    }
}
