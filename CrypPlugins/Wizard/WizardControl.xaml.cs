using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
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
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using System.Threading;
using System.Collections;
using System.Globalization;
using System.Xml.Schema;
using Cryptool.Core;
using Cryptool.PluginBase;
using WorkspaceManager.Model;
using Wizard.Properties;
using Path = System.IO.Path;
using ValidationType = System.Xml.ValidationType;

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

        ObservableCollection<PageInfo> currentHistory = new ObservableCollection<PageInfo>();
        private readonly RecentFileList _recentFileList = RecentFileList.GetSingleton();
        private Dictionary<string, bool> selectedCategories = new Dictionary<string, bool>();
        private SolidColorBrush selectionBrush = new SolidColorBrush();
        private const string configXMLPath = "Wizard.Config.wizard.config.start.xml";
        private const string defaultLang = "en";
        private XElement wizardConfigXML;
        private Dictionary<string, List<PluginPropertyValue>> propertyValueDict = new Dictionary<string, List<PluginPropertyValue>>();
        private HashSet<TextBox> boxesWithWrongContent = new HashSet<TextBox>();
        private HistoryTranslateTransformConverter historyTranslateTransformConverter = new HistoryTranslateTransformConverter();
        private List<TextBox> currentOutputBoxes = new List<TextBox>();
        private List<TextBox> currentInputBoxes = new List<TextBox>();
        private List<ContentControl> currentPresentations = new List<ContentControl>();
        private WorkspaceManager.WorkspaceManager currentManager = null;
        private bool canStopOrExecute = false;
        private string _title;

        internal event OpenEditorHandler OnOpenEditor;
        internal event OpenTabHandler OnOpenTab;
        internal event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        internal string SamplesDir { set; private get; }

        public WizardControl()
        {
            InitializeComponent();
            OuterScrollViewer.Focus();
            Loaded += delegate { Keyboard.Focus(this); };
        }

        public void Initialize()
        {
            try
            {
                // DEBUG HELP string[] names = this.GetType().Assembly.GetManifestResourceNames();

                XElement xml = GetXml(configXMLPath);
                GenerateXML(xml);
                
                currentHistory.CollectionChanged += delegate
                {
                    CreateHistory();
                };

                selectionBrush.Color = Color.FromArgb(255, 200, 220, 245);
                SetupPage(wizardConfigXML);
                AddToHistory(wizardConfigXML);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Couldn't create wizard: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        private XElement GetXml(string xmlPath)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.ValidationType = ValidationType.DTD;
            settings.ValidationEventHandler += delegate(object sender, ValidationEventArgs e)
                                                   {
                                                       GuiLogMessage(string.Format("Error validating wizard XML file {0}: {1}", xmlPath, e.Message), NotificationLevel.Error);
                                                   };
            settings.XmlResolver = new ResourceDTDResolver();

            XmlReader xmlReader = XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream(xmlPath), settings);

            //Stream fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(xmlPath);
            return XElement.Load(xmlReader);
        }

        private class ResourceDTDResolver : XmlResolver
        {
            public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                if (Path.GetFileName(absoluteUri.LocalPath) == "wizard.dtd")
                    return Assembly.GetExecutingAssembly().GetManifestResourceStream("Wizard.Config.wizard.dtd");
                return null;
            }

            public override ICredentials Credentials
            {
                set { }
            }
        }

        ~WizardControl()
        {
        }

        // generate the full XML tree for the wizard (recursive)
        private void GenerateXML(XElement xml)
        {
            try
            {
                IEnumerable<XElement> allFiles = xml.Elements("file");
                foreach(var ele in allFiles)
                {
                    XAttribute att = ele.Attribute("resource");
                    if (att != null)
                    {
                        string path = att.Value;
                        XElement sub = GetXml(path);
                        ele.AddAfterSelf(sub);
                    }
                }

                IEnumerable<XElement> allElements = xml.Elements();
                if (allElements.Any())
                {
                    foreach (XElement ele in allElements)
                    {
                        if (ele.Name != "file")
                        {
                            GenerateXML(ele);
                        }
                    }
                }

                wizardConfigXML = xml;

            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Could not GenerateXML: {0}", ex.Message), NotificationLevel.Error);
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

        #region HistoryTranslateTransformConverter

        [ValueConversion(typeof(double), typeof(double))]
        class HistoryTranslateTransformConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (double)value - 2;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        private void SetupPage(XElement element)
        {
            StopCurrentWorkspaceManager();
            nextButton.IsEnabled = true;
            CreateProjectButton.Visibility = Visibility.Hidden;

            currentOutputBoxes.Clear();
            currentInputBoxes.Clear();
            currentPresentations.Clear();
            SaveContent();
            boxesWithWrongContent.Clear();

            if ((element.Name == "loadSample") && (element.Attribute("file") != null) && (element.Attribute("title") != null))
            {
                LoadSample(element.Attribute("file").Value, element.Attribute("title").Value, true, element);
                abortButton_Click(null, null);
                return;
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
                taskHeader.Content = headline.Value.Trim();

            //set task description label
            XElement task = FindElementsInElement(element, "task").First();
            if (task != null)
                descHeader.Text = task.Value.Trim();


            if (element.Name == "input" || element.Name == "sampleViewer")
            {
                categoryGrid.Visibility = Visibility.Hidden;
                inputPanel.Visibility = Visibility.Visible;

                var inputs = from el in element.Elements()
                             where el.Name == "inputBox" || el.Name == "comboBox" || el.Name == "checkBox" || el.Name == "outputBox" || el.Name == "presentation"
                             select el;

                inputStack.Children.Clear();
                
                var allNexts = (from el in element.Elements()
                                 where el.Name == "input" || el.Name == "category" || el.Name == "loadSample" || el.Name == "sampleViewer"
                                 select el);
                if (allNexts.Count() > 0)
                {
                    inputPanel.Tag = allNexts.First();
                    if (allNexts.First().Name == "loadSample")
                        SwitchNextButtonContent();
                }
                else
                {
                    var dummy = new XElement("loadSample");
                    element.Add(dummy);
                    inputPanel.Tag = dummy;
                    SwitchNextButtonContent();
                }

                FillInputStack(inputs, element.Name.ToString(), (element.Name == "input"));

                if (element.Name == "sampleViewer" && (element.Attribute("file") != null))
                {
                    nextButton.IsEnabled = false;
                    if (element.Attribute("showCreateButton") == null || element.Attribute("showCreateButton").Value.ToLower() != "false")
                    {
                        CreateProjectButton.Visibility = Visibility.Visible;
                    }
                    
                    LoadSample(element.Attribute("file").Value, null, false, element);
                }

                string id = GetElementID((XElement)inputPanel.Tag);

            }
            else if (element.Name == "category")
            {
                categoryGrid.Visibility = Visibility.Visible;
                inputPanel.Visibility = Visibility.Hidden;
                
                radioButtonStackPanel.Children.Clear();

                //generate radio buttons
                var options = from el in element.Elements()
                              where el.Name == "category" || el.Name == "input" || el.Name == "loadSample" || el.Name == "sampleViewer"
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
                            l.Content = label.Value.Trim();

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
                        rb.Focusable = false;
                        string id = GetElementID(ele);
                        rb.Checked += rb_Checked;
                        rb.MouseDoubleClick += rb_MouseDoubleClick;
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

        void rb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            nextButton_Click(sender, e);
        }

        private void FillInputStack(IEnumerable<XElement> inputs, string type, bool isInput)
        {
            var inputFieldStyle = (Style)FindResource("InputFieldStyle");

            var groups = from input in inputs where input.Attribute("group") != null select input.Attribute("group").Value;
            groups = groups.Distinct();

            var inputGroups = new List<StackPanel>();
            var otherInputs = new List<StackPanel>();

            foreach (var group in groups)
            {
                var sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                sp.Tag = group;
                inputGroups.Add(sp);
            }

            foreach (var input in inputs)
            {
                try
                {
                    var stack = new StackPanel();

                    var description = new Label();
                    description.Content = FindElementsInElement(input, "description").First().Value.Trim();
                    description.HorizontalAlignment = HorizontalAlignment.Left;
                    description.FontWeight = FontWeights.Bold;
                    stack.Children.Add(description);

                    Control inputElement = CreateInputElement(input, inputFieldStyle, isInput);

                    //TODO add controls to same "level" if they have the same group

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

                    stack.Children.Add(inputElement);

                    if (input.Attribute("group") != null && inputGroups.Any())
                    {
                        var sp = from g in inputGroups where (string)g.Tag == input.Attribute("group").Value select g;
                        var group = sp.First();
                        group.Children.Add(stack);
                    }
                    else
                    {
                        stack.Tag = input;
                        otherInputs.Add(stack);
                    }

                }
                catch (Exception e)
                {
                    GuiLogMessage(string.Format("Error while creating wizard element {0}: {1}", input, e.Message), NotificationLevel.Error);
                }
            }

            foreach (var input in inputs)
            {
                if (input.Attribute("group") != null && inputGroups.Any())
                {
                    var sp = from g in inputGroups where (string)g.Tag == input.Attribute("group").Value select g;
                    var group = sp.First();
                    if (!inputStack.Children.Contains(group))
                        inputStack.Children.Add(group);
                }
                else
                {
                    var p = from g in otherInputs where (XElement)g.Tag == input select g;
                    var put = p.First();
                    inputStack.Children.Add(put);
                }
            }

        }

        private Control CreateInputElement(XElement input, Style inputFieldStyle, bool isInput)
        {
            Control element = null;

            string key = null;
            if (input.Name != "presentation")
                key = GetElementPluginPropertyKey(input);

            var pluginPropertyValue = GetPropertyValue(key, input.Parent);

            switch (input.Name.ToString())
            {
                case "inputBox":
                    var inputBox = new TextBox();
                    inputBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    inputBox.Tag = input;
                    inputBox.AcceptsReturn = true;
                    inputBox.TextWrapping = TextWrapping.Wrap;
                    if (input.Attribute("visibleLines") != null)
                    {
                        int visibleLines;
                        if (Int32.TryParse(input.Attribute("visibleLines").Value.Trim(), out visibleLines))
                        {
                            inputBox.MinLines = visibleLines;
                            inputBox.MaxLines = visibleLines;
                        }
                    }
                    inputBox.Style = inputFieldStyle;

                    if (input.Attribute("regex") != null)
                    {
                        var regex = new Regex(input.Attribute("regex").Value, RegexOptions.Compiled);

                        inputBox.TextChanged += delegate
                        {
                            CheckRegex(inputBox, regex);
                        };
                    }

                    if (key != null && pluginPropertyValue != null)
                        inputBox.Text = (string) pluginPropertyValue.Value;
                    else
                    {
                        var defaultvalues = FindElementsInElement(input, "defaultvalue");
                        var defaultvalue = defaultvalues.First();

                        if (!string.IsNullOrEmpty(defaultvalue.Value))
                            inputBox.Text = defaultvalue.Value;
                    }

                    if (!isInput)
                        currentInputBoxes.Add(inputBox);

                    element = inputBox;
                    break;

                case "comboBox":
                    ComboBox comboBox = new ComboBox();
                    comboBox.Style = inputFieldStyle;
                    comboBox.Tag = input;

                    var items = FindElementsInElement(input, "item");
                    foreach (var item in items)
                    {
                        ComboBoxItem cbi = new ComboBoxItem();
                        if (item.Attribute("content") != null)
                            cbi.Content = item.Attribute("content").Value;
                        comboBox.Items.Add(cbi);
                    }

                    if (key != null && pluginPropertyValue != null)
                    {
                        if (pluginPropertyValue.Value is int)
                        {
                            ComboBoxItem cbi =
                                (ComboBoxItem)comboBox.Items.GetItemAt((int)pluginPropertyValue.Value);
                            cbi.IsSelected = true;
                        }
                    }
                    else if (input.Attribute("defaultValue") != null)
                    {
                        int i = 0;
                        if (Int32.TryParse(input.Attribute("defaultValue").Value.Trim(), out i))
                        {
                            ComboBoxItem cbi = (ComboBoxItem)comboBox.Items.GetItemAt(i);
                            cbi.IsSelected = true;
                        }
                    }
                    else
                        ((ComboBoxItem) comboBox.Items.GetItemAt(0)).IsSelected = true;

                    element = comboBox;
                    break;

                case "checkBox":
                    CheckBox checkBox = new CheckBox();
                    checkBox.Tag = input;
                    checkBox.Style = inputFieldStyle;

                    var contents = FindElementsInElement(input, "content");
                    var content = contents.First();

                    if (!string.IsNullOrEmpty(content.Value))
                        checkBox.Content = content.Value.Trim();

                    if (key != null && pluginPropertyValue != null)
                    {
                        string value = (string)pluginPropertyValue.Value;
                        if (value.ToLower() == "true")
                            checkBox.IsChecked = true;
                        else
                            checkBox.IsChecked = false;
                    }
                    else if (input.Attribute("defaultValue") != null)
                    {
                        string value = input.Attribute("defaultValue").Value;
                        if (value.ToLower() == "true")
                            checkBox.IsChecked = true;
                        else
                            checkBox.IsChecked = false;
                    }

                    element = checkBox;
                    break;

                case "outputBox":
                    if (isInput)
                        break;

                    var outputBox = new TextBox();
                    outputBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    outputBox.Tag = input;
                    outputBox.AcceptsReturn = true;
                    outputBox.TextWrapping = TextWrapping.Wrap;
                    if (input.Attribute("visibleLines") != null)
                    {
                        int visibleLines;
                        if (Int32.TryParse(input.Attribute("visibleLines").Value.Trim(), out visibleLines))
                        {
                            outputBox.MinLines = visibleLines;
                            outputBox.MaxLines = visibleLines;
                        }
                    }
                    outputBox.Style = inputFieldStyle;

                    if (input.Attribute("regex") != null)
                    {
                        var regex = new Regex(input.Attribute("regex").Value, RegexOptions.Compiled);

                        outputBox.TextChanged += delegate
                        {
                            CheckRegex(outputBox, regex);
                        };
                    }
                    
                    outputBox.IsReadOnly = true;

                    currentOutputBoxes.Add(outputBox);
                    element = outputBox;
                    break;
                case "presentation":
                    if (isInput)
                        break;

                    var cc = new ContentControl();

                    double d;
                    if (input.Attribute("height") != null && Double.TryParse(input.Attribute("height").Value, out d))
                        cc.Height = d;

                    cc.Style = inputFieldStyle;

                    cc.Tag = input;

                    currentPresentations.Add(cc);
                    element = cc;
                    break;
            }

            return element;
        }

        private PluginPropertyValue GetPropertyValue(string key, XElement path)
        {
            if (key != null && propertyValueDict.ContainsKey(key))
            {
                foreach (var pv in propertyValueDict[key])
                {
                    if (IsSamePath(pv.Path, path))
                    {
                        return pv;
                    }
                }
            }
            return null;
        }

        private bool IsSamePath(XElement path, XElement path2)
        {
            if (path == path2 || path.Descendants().Contains(path2) || path2.Descendants().Contains(path))
            {
                return true;
            }
            return false;
        }

        private string GetElementPluginPropertyKey(XElement element)
        {
            var plugin = element.Attribute("plugin").Value;
            var property = element.Attribute("property").Value;
            string key = null;
            if (plugin != null && property != null)
                key = string.Format("{0}.{1}", plugin, property);
            return key;
        }

        private void CreateHistory()
        {
            StackPanel historyStack = new StackPanel();
            historyStack.Orientation = Orientation.Horizontal;

            foreach (var page in currentHistory)
            {
                var p = new ContentControl();
                p.Focusable = false;
                var bg = selectionBrush.Clone();
                bg.Opacity = 1 - (historyStack.Children.Count / (double)currentHistory.Count);
                var sp = new StackPanel { Orientation = Orientation.Horizontal, Background = bg };
                p.Content = sp;
                p.Tag = page.tag;
                p.MouseDoubleClick += new MouseButtonEventHandler(page_MouseDoubleClick);

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
                binding.Converter = historyTranslateTransformConverter;
                BindingOperations.SetBinding(translateTranform, TranslateTransform.XProperty, binding);

                historyStack.Children.Add(p);
            }

            history.Content = historyStack;
            history.ScrollToRightEnd();
        }

        void page_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SwitchButtonWhenNecessary();
            canStopOrExecute = false;

            var cc = (ContentControl)sender;
            var hs = (StackPanel)history.Content;
            int i = hs.Children.IndexOf(cc);
            history.Content = null;

            while (currentHistory.Count > i+1)
            {
                currentHistory.RemoveAt(currentHistory.Count - 1);
            }

            CreateHistory();

            XElement parent = (XElement)cc.Tag;

            if (parent == null)
                parent = wizardConfigXML;

            SetupPage(parent);
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

        private void LoadSample(string file, string title, bool openTab, XElement element)
        {
            try
            {
                _title = title;
                file = Path.Combine(SamplesDir, file);

                var model = ModelPersistance.loadModel(file);
                model.OnGuiLogNotificationOccured += delegate(IPlugin sender, GuiLogEventArgs args)
                                                         {
                                                             OnGuiLogNotificationOccured(sender, args);
                                                         };

                foreach (PluginModel pluginModel in model.GetAllPluginModels())
                {
                    pluginModel.Plugin.Initialize();
                }

                if (!openTab)
                {
                    CreateProjectButton.Tag = element;
                    RegisterEventsForLoadedSample(model);
                }

                FillDataToModel(model, element);

                //load sample:
                if (openTab)
                {
                    currentManager =
                        (WorkspaceManager.WorkspaceManager)
                        OnOpenEditor(typeof (WorkspaceManager.WorkspaceManager), null);
                    currentManager.Open(model);
                    if (Settings.Default.RunTemplate)
                    {
                        currentManager.SampleLoaded += NewEditorSampleLoaded;
                    }
                }
                else
                {
                    currentManager = new WorkspaceManager.WorkspaceManager();
                    currentManager.Open(model);
                    canStopOrExecute = true;
                    currentManager.Execute();
                }

                _recentFileList.AddRecentFile(file);
            }
            catch(Exception ex)
            {
                GuiLogMessage(string.Format("Error loading sample {0}: {1}", file,ex.Message),NotificationLevel.Error);
            }
        }

        private void NewEditorSampleLoaded(object sender, EventArgs e)
        {
            if (Settings.Default.RunTemplate && currentManager.CanExecute)
                currentManager.Execute();
            currentManager.SampleLoaded -= NewEditorSampleLoaded;
            OnOpenTab(currentManager, _title, null);
        }

        private void FillDataToModel(WorkspaceModel model, XElement element)
        {
            //Fill in all data from wizard to sample:
            foreach (var c in propertyValueDict)
            {
                foreach (var ppv in c.Value)
                {
                    if (IsSamePath(element, ppv.Path))
                    {
                        try
                        {
                            var plugins = ppv.PluginName.Split(';');
                            foreach (var plugin in model.GetAllPluginModels().Where(x => plugins.Contains(x.GetName())))
                            {
                                var settings = plugin.Plugin.Settings;

                                var property = plugin.Plugin.GetType().GetProperty(ppv.PropertyName) ??
                                               settings.GetType().GetProperty(ppv.PropertyName);

                                if (property != null)
                                {
                                    if (ppv.Value is string)
                                        property.SetValue(settings, (string)ppv.Value, null);
                                    else if (ppv.Value is int)
                                        property.SetValue(settings, (int)ppv.Value, null);
                                    else if (ppv.Value is bool)
                                        property.SetValue(settings, (bool)ppv.Value, null);
                                }
                                plugin.Plugin.Initialize();
                            }
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage(string.Format("Failed settings plugin property {0}.{1} to \"{2}\"!", ppv.PluginName, ppv.PropertyName, ppv.Value), NotificationLevel.Error);
                        }
                    }
                }
            }
        }

        private void RegisterEventsForLoadedSample(WorkspaceModel model)
        {
            //Register events for output boxes:
            foreach (var outputBox in currentOutputBoxes)
            {
                XElement ele = (XElement)outputBox.Tag;
                var pluginName = ele.Attribute("plugin").Value;
                var propertyName = ele.Attribute("property").Value;
                if (pluginName != null && propertyName != null)
                {
                    var plugin = model.GetAllPluginModels().Where(x => x.GetName() == pluginName).First().Plugin;
                    var settings = plugin.Settings;
                    object theObject = null;

                    var property = plugin.GetType().GetProperty(propertyName);
                    EventInfo propertyChangedEvent = null;
                    if (property != null)
                    {
                        propertyChangedEvent = plugin.GetType().GetEvent("PropertyChanged");
                        theObject = plugin;
                    }
                    else    //Use property from settings
                    {
                        property = settings.GetType().GetProperty(propertyName);
                        propertyChangedEvent = settings.GetType().GetEvent("PropertyChanged");
                        theObject = settings;
                    }

                    if (property != null && propertyChangedEvent != null)
                    {
                        TextBox box = outputBox;
                        propertyChangedEvent.AddEventHandler(theObject, (PropertyChangedEventHandler)delegate(Object sender, PropertyChangedEventArgs e)
                                                                                                         {
                                                                                                             if (e.PropertyName == propertyName)
                                                                                                             {
                                                                                                                 UpdateOutputBox(box, property, theObject);
                                                                                                             }
                                                                                                         });
                    }
                }
            }

            //fill presentations
            foreach (var presentation in currentPresentations)
            {
                var ele = (XElement)presentation.Tag;
                var pluginName = ele.Attribute("plugin").Value;
                if (!string.IsNullOrEmpty(pluginName))
                {
                    var plugin = model.GetAllPluginModels().Where(x => x.GetName() == pluginName).First().Plugin;
                    if (presentation.Content == null)
                    {
                        presentation.Content = plugin.Presentation;
                        if (presentation.Content.GetType().GetProperty("Text") != null)
                        {
                            var defaultvalues = FindElementsInElement(ele, "defaultvalue");
                            var defaultvalue = defaultvalues.First().Value;
                            if (!string.IsNullOrEmpty(defaultvalue))
                                presentation.Content.GetType().GetProperty("Text").SetValue(presentation.Content, defaultvalue, null);
                        }
                    }
                }
            }

            //Register events for input boxes:
            foreach (var inputBox in currentInputBoxes)
            {
                XElement ele = (XElement)inputBox.Tag;
                var pluginName = ele.Attribute("plugin").Value;
                var propertyName = ele.Attribute("property").Value;
                if (pluginName != null && propertyName != null)
                {
                    var pluginNames = pluginName.Split(';');
                    foreach (var plugin in model.GetAllPluginModels().Where(x => pluginNames.Contains(x.GetName())))
                    {
                        var settings = plugin.Plugin.Settings;
                        object theObject = null;

                        var property = plugin.Plugin.GetType().GetProperty(propertyName);
                        if (property != null)
                        {
                            theObject = plugin.Plugin;
                        }
                        else    //Use property from settings
                        {
                            property = settings.GetType().GetProperty(propertyName);
                            theObject = settings;
                        }

                        if (property != null)
                        {
                            TextBox box = inputBox;
                            PluginModel plugin1 = plugin;
                            inputBox.TextChanged += delegate
                                                        {
                                                            property.SetValue(settings, box.Text, null);
                                                            plugin1.Plugin.Initialize();
                                                        };
                        }
                    }
                }
            }
        }

        private void UpdateOutputBox(TextBox box, PropertyInfo property, object theObject)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                                             {
                                                                 box.Text = (string) property.GetValue(theObject, null);
                                                             }, null);
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
                description.Text = desc.Value.Trim();
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
                if (!allElements.Any())
                {
                    List<XElement> fe = new List<XElement>();
                    fe.Add(new XElement("dummy"));
                    return fe;
                }
                else
                    return allElements;
            }

            return foundElements;
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.ShowAnimations)
            {
                Storyboard mainGridStoryboardLeft = (Storyboard) FindResource("MainGridStoryboardNext1");
                mainGridStoryboardLeft.Begin();
            }
            else
            {
                SetNextContent(sender, e);
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            canStopOrExecute = false;
            if (Settings.Default.ShowAnimations)
            {
                Storyboard mainGridStoryboardLeft = (Storyboard) FindResource("MainGridStoryboardBack1");
                mainGridStoryboardLeft.Begin();
            }
            else
            {
                SetLastContent(sender, e);
            }
        }

        private void abortButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchButtonWhenNecessary();
            canStopOrExecute = false;

            foreach (RadioButton rb in radioButtonStackPanel.Children)
            {
                if (rb.IsChecked != null && (bool)rb.IsChecked)
                    rb.IsChecked = false;
            }

            history.Content = null;
            currentHistory.Clear();
            AddToHistory(wizardConfigXML);
            propertyValueDict.Clear();
            ResetSelectionDependencies();
            radioButtonStackPanel.Children.Clear();
            selectedCategories.Clear();
            description.Text = "";
            SetupPage(wizardConfigXML);
        }

        internal void StopCurrentWorkspaceManager()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
            {
                if (currentManager != null && currentManager.CanStop)
                    currentManager.Stop();
            }, null);
        }

        internal void ExecuteCurrentWorkspaceManager()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (currentManager != null && currentManager.CanExecute)
                    currentManager.Execute();
            }, null);
        }

        internal bool WizardCanStop()
        {
            if (!canStopOrExecute || currentManager == null)
                return false;
            else
                return currentManager.CanStop;
        }

        internal bool WizardCanExecute()
        {
            if (!canStopOrExecute || currentManager == null)
                return false;
            else
                return currentManager.CanExecute;
        }

        private void SwitchButtonWhenNecessary()
        {
            if (inputPanel.Visibility == Visibility.Visible)
            {
                if (inputPanel.Tag != null && ((XElement)inputPanel.Tag).Name == "loadSample")
                    SwitchNextButtonContent();
            }
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
                        AddToHistory(ele);
                        SetupPage(ele);
                        break;
                    }
                }
            }
            else if (inputPanel.Visibility == Visibility.Visible)
            {
                var nextElement = (XElement) inputPanel.Tag;
                AddToHistory(nextElement);
                SetupPage(nextElement);
            }

            if (Settings.Default.ShowAnimations)
            {
                Storyboard mainGridStoryboardLeft = (Storyboard) FindResource("MainGridStoryboardNext2");
                mainGridStoryboardLeft.Begin();
            }
        }

        private void SaveContent()
        {
            if (inputPanel.Visibility == Visibility.Visible)
            {
                foreach (var child in inputStack.Children)
                {
                    SaveControlContent(child);
                }
            }
        }

        private void AddToHistory(XElement ele)
        {
            try
            {
                var page = new PageInfo()
                                   {
                                       name = FindElementsInElement(ele, "name").First().Value.Trim(),
                                       description = FindElementsInElement(ele, "description").First().Value.Trim(),
                                       tag = ele
                                   };

                if (ele.Attribute("image") != null)
                {
                    page.image = ele.Attribute("image").Value;
                }

                currentHistory.Add(page);
            }
            catch (Exception)
            {
                GuiLogMessage("Error adding page to history", NotificationLevel.Error);
            }
        }

        private void SaveControlContent(object o)
        {
            var sp = (StackPanel)o;

            foreach (var input in sp.Children)
            {
                if (input is TextBox || input is ComboBox || input is CheckBox)
                {
                    Control c = (Control)input;
                    XElement ele = (XElement)c.Tag;
                    if (ele.Name == "outputBox")
                        continue;

                    PluginPropertyValue newEntry = null;
                    if (ele.Attribute("plugin") != null && ele.Attribute("property") != null)
                    {
                        if (input is TextBox)
                        {
                            TextBox textBox = (TextBox)input;
                            newEntry = new PluginPropertyValue()
                                       {
                                           PluginName = ele.Attribute("plugin").Value,
                                           PropertyName = ele.Attribute("property").Value,
                                           Value = textBox.Text,
                                           Path = ele.Parent
                                       };
                        }
                        else if (input is ComboBox)
                        {
                            ComboBox comboBox = (ComboBox)input;
                            newEntry = new PluginPropertyValue()
                            {
                                PluginName = ele.Attribute("plugin").Value,
                                PropertyName = ele.Attribute("property").Value,
                                Value = comboBox.SelectedIndex,
                                Path = ele.Parent
                            };
                        }
                        else if (input is CheckBox)
                        {
                            CheckBox checkBox = (CheckBox)input;
                            if (checkBox.IsChecked != null)
                            {
                                newEntry = new PluginPropertyValue()
                                {
                                    PluginName = ele.Attribute("plugin").Value,
                                    PropertyName = ele.Attribute("property").Value,
                                    Value = (bool)checkBox.IsChecked,
                                    Path = ele.Parent
                                };
                            }
                        }

                        var key = GetElementPluginPropertyKey(ele);
                        if (newEntry != null && key != null)
                        {
                            var pluginPropertyValue = GetPropertyValue(key, ele.Parent);
                            if (pluginPropertyValue != null)
                            {
                                pluginPropertyValue.Value = newEntry.Value;
                            }
                            else
                            {
                                if (propertyValueDict.ContainsKey(key))
                                {
                                    propertyValueDict[key].Add(newEntry);
                                }
                                else
                                {
                                    propertyValueDict.Add(key, new List<PluginPropertyValue>() {newEntry} );
                                }
                            }
                        }
                    }
                }
                else if (input is StackPanel)
                {
                    StackPanel stack = (StackPanel)input;
                    SaveControlContent(stack);
                }
            }
        }

        private void SetLastContent(object sender, EventArgs e)
        {
            canStopOrExecute = false;

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
                ele = (XElement) inputPanel.Tag;
                if (ele != null && ((XElement)inputPanel.Tag).Name == "loadSample")
                    SwitchNextButtonContent();
            }

            if (ele != null)
            {
                XElement grandParent = ele.Parent.Parent;
                if (grandParent == null)
                    grandParent = wizardConfigXML;

                if (currentHistory.Count > 0)
                    currentHistory.RemoveAt(currentHistory.Count - 1);

                SetupPage(grandParent);
            }

            if (Settings.Default.ShowAnimations)
            {
                Storyboard mainGridStoryboardLeft = (Storyboard) FindResource("MainGridStoryboardBack2");
                mainGridStoryboardLeft.Begin();
            }
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

        private void history_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point dir = e.GetPosition(history);
                if (dir.X < history.ActualWidth / 2)
                    history.LineRight();
                else if (dir.X > history.ActualWidth / 2)
                    history.LineLeft();
            }
        }

        private void KeyPressedDown(Key key)
        {
            switch (key)
            {
                case Key.Up:
                case Key.Down:
                    if (categoryGrid.Visibility == Visibility.Visible)
                    {
                        if (radioButtonStackPanel.Children.Count != 0)
                        {
                            int i = 0;
                            while (((RadioButton)radioButtonStackPanel.Children[i]).IsChecked == false)
                                i++;
                            ((RadioButton)radioButtonStackPanel.Children[i]).IsChecked = false;

                            if (key == Key.Down)
                            {
                                if (radioButtonStackPanel.Children.Count > i + 1)
                                    ((RadioButton)radioButtonStackPanel.Children[i + 1]).IsChecked = true;
                                else
                                    ((RadioButton)radioButtonStackPanel.Children[0]).IsChecked = true;
                            }
                            else   //Up
                            {
                                if (i - 1 >= 0)
                                    ((RadioButton)radioButtonStackPanel.Children[i - 1]).IsChecked = true;
                                else
                                    ((RadioButton)radioButtonStackPanel.Children[radioButtonStackPanel.Children.Count - 1]).IsChecked = true;
                            }
                        }
                    }
                    break;

                case Key.Left:
                    if (backButton.IsEnabled)
                        backButton_Click(null, null);
                    break;

                case Key.Right:
                    if (nextButton.IsEnabled)
                        nextButton_Click(null, null);
                    break;
            }
        }

        private void ScrollViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (OuterScrollViewer.IsKeyboardFocused || descScroll.IsKeyboardFocused || inputPanel.IsKeyboardFocused || history.IsKeyboardFocused)
            {
                KeyPressedDown(e.Key);
                e.Handled = true;
            }
        }

        private void CreateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            SaveControlContent(inputStack);
            var element = (XElement)CreateProjectButton.Tag;
            LoadSample(element.Attribute("file").Value, Properties.Resources.LoadedSampleTitle, true, element);
        }
    }

    internal class PluginPropertyValue
    {
        public string PluginName;
        public string PropertyName;
        public object Value;
        public XElement Path;
    }
}
