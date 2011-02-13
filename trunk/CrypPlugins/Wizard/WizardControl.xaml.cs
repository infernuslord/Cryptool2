using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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

        ObservableCollection<PageInfo> currentHistory = new ObservableCollection<PageInfo>();
        private Dictionary<string, bool> selectedCategories = new Dictionary<string, bool>();
        private SolidColorBrush selectionBrush = new SolidColorBrush();
        private const string configXMLPath = "Wizard.Config.wizard.config.start.xml";
        private const string defaultLang = "en";
        private XElement wizardConfigXML;
        private Dictionary<string, PluginPropertyValue> propertyValueDict = new Dictionary<string, PluginPropertyValue>();
        private HashSet<TextBox> boxesWithWrongContent = new HashSet<TextBox>();
        private HistoryTranslateTransformConverter historyTranslateTransformConverter = new HistoryTranslateTransformConverter();
        private List<TextBox> currentOutputBoxes = new List<TextBox>();
        private List<TextBox> currentInputBoxes = new List<TextBox>();
        private List<ContentControl> currentPresentations = new List<ContentControl>();

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
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            currentHistory.CollectionChanged += delegate
                                                    {
                                                        CreateHistory();
                                                    };

            selectionBrush.Color = Color.FromArgb(255, 200, 220, 245);
            SetupPage(wizardConfigXML);
            AddToHistory(wizardConfigXML);

            Loaded += delegate { Keyboard.Focus(this); };
        }

        ~WizardControl()
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
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
            nextButton.IsEnabled = true;

            currentOutputBoxes.Clear();
            currentInputBoxes.Clear();
            SaveContent();
            boxesWithWrongContent.Clear();

            if ((element.Name == "loadSample") && (element.Attribute("file") != null) && (element.Attribute("title") != null))
            {
                LoadSample(element.Attribute("file").Value, element.Attribute("title").Value, true);
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
                taskHeader.Content = headline.Value;

            //set description label
            XElement desc = FindElementsInElement(element, "desc").First();
            if (desc != null)
                descHeader.Content = desc.Value;


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

                if (element.Name == "sampleViewer" && (element.Attribute("file") != null) && (element.Attribute("title") != null))
                {
                    nextButton.IsEnabled = false;
                    LoadSample(element.Attribute("file").Value, element.Attribute("title").Value, false);
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
                              where el.Name == "category" || el.Name == "input" || el.Name == "loadSample"
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
                        rb.Focusable = false;
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

        private void FillInputStack(IEnumerable<XElement> inputs, string type, bool isInput)
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

            string key = null;
            if (input.Name != "presentation")
                key = GetElementPluginPropertyKey(input);

            switch (input.Name.ToString())
            {
                case "inputBox":
                    var inputBox = new TextBox();
                    inputBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    inputBox.Tag = input;
                    inputBox.AcceptsReturn = true;
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
                    inputStack.Children.Add(inputBox);

                    if (input.Attribute("regex") != null)
                    {
                        var regex = new Regex(input.Attribute("regex").Value, RegexOptions.Compiled);

                        inputBox.TextChanged += delegate
                        {
                            CheckRegex(inputBox, regex);
                        };
                    }

                    if (key != null && propertyValueDict.ContainsKey(key))
                        inputBox.Text = (string)propertyValueDict[key].Value;
                    else if (input.Attribute("defaultValue") != null)
                        inputBox.Text = input.Attribute("defaultValue").Value.Trim();

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


                    if (key != null && propertyValueDict.ContainsKey(key))
                    {
                        if (propertyValueDict[key].Value is int)
                        {
                            ComboBoxItem cbi =
                                (ComboBoxItem)comboBox.Items.GetItemAt((int)propertyValueDict[key].Value);
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


                    inputStack.Children.Add(comboBox);
                    element = comboBox;
                    break;

                case "checkBox":
                    CheckBox checkBox = new CheckBox();
                    checkBox.Tag = input;
                    checkBox.Style = inputFieldStyle;

                    if (input.Attribute("content") != null)
                        checkBox.Content = input.Attribute("content").Value;


                    if (key != null && propertyValueDict.ContainsKey(key))
                    {
                        string value = (string)propertyValueDict[key].Value;
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

                    inputStack.Children.Add(checkBox);
                    element = checkBox;
                    break;

                case "outputBox":
                    if (isInput)
                        break;

                    var outputBox = new TextBox();
                    outputBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    outputBox.Tag = input;
                    outputBox.AcceptsReturn = true;
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
                    inputStack.Children.Add(outputBox);

                    if (input.Attribute("regex") != null)
                    {
                        var regex = new Regex(input.Attribute("regex").Value, RegexOptions.Compiled);

                        outputBox.TextChanged += delegate
                        {
                            CheckRegex(outputBox, regex);
                        };
                    }
                    
                    outputBox.IsEnabled = false;

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

                    cc.Tag = input;
                    inputStack.Children.Add(cc);

                    currentPresentations.Add(cc);
                    element = cc;
                    break;
            }

            return element;
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

        private void LoadSample(string file, string title, bool openTab)
        {
            file = SamplesDir + "\\" + file;

            var newEditor = new WorkspaceManager.WorkspaceManager();
            var model = ModelPersistance.loadModel(file, newEditor);

            //Fill in all data from wizard to sample:
            foreach (var c in propertyValueDict)
            {
                var ppv = c.Value;
                try
                {
                    var plugin = model.AllPluginModels.Where(x => x.Name == ppv.PluginName).First().Plugin;
                    var settings = plugin.Settings;

                    var property = plugin.GetType().GetProperty(ppv.PropertyName) ?? settings.GetType().GetProperty(ppv.PropertyName);

                    if (property != null)
                    {
                        if (ppv.Value is string)
                            property.SetValue(settings, (string) ppv.Value, null);
                        else if (ppv.Value is int)
                            property.SetValue(settings, (int) ppv.Value, null);
                        else if (ppv.Value is bool)
                            property.SetValue(settings, (bool) ppv.Value, null);
                    }
                }
                catch (Exception)
                {
                    GuiLogMessage(string.Format("Failed settings plugin property {0}.{1} to \"{2}\"!", ppv.PluginName, ppv.PropertyName, ppv.Value), NotificationLevel.Error);
                }
            }

            //Register events for output boxes:
            foreach (var outputBox in currentOutputBoxes)
            {
                XElement ele = (XElement) outputBox.Tag;
                var pluginName = ele.Attribute("plugin").Value;
                var propertyName = ele.Attribute("property").Value;
                if (pluginName != null && propertyName != null)
                {
                    var plugin = model.AllPluginModels.Where(x => x.Name == pluginName).First().Plugin;
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
                    var plugin = model.AllPluginModels.Where(x => x.Name == pluginName).First().Plugin;
                    presentation.Content = plugin.Presentation;
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
                    var plugin = model.AllPluginModels.Where(x => x.Name == pluginName).First().Plugin;
                    var settings = plugin.Settings;
                    object theObject = null;

                    var property = plugin.GetType().GetProperty(propertyName);
                    if (property != null)
                    {
                        theObject = plugin;
                    }
                    else    //Use property from settings
                    {
                        property = settings.GetType().GetProperty(propertyName);
                        theObject = settings;
                    }

                    if (property != null)
                    {
                        inputBox.TextChanged += delegate
                                                    {
                                                        property.SetValue(settings, (string) inputBox.Text, null);
                                                        plugin.Initialize();
                                                    };
                    }
                }
            }

            //load sample:
            foreach (PluginModel pluginModel in model.AllPluginModels)
            {
                pluginModel.Plugin.Initialize();
            }
            newEditor.Open(model);

            if (openTab)
                OnOpenTab(newEditor, title, null);
            else
                newEditor.Execute();
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
            SwitchButtonWhenNecessary();

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

            Storyboard mainGridStoryboardLeft = (Storyboard)FindResource("MainGridStoryboardNext2");
            mainGridStoryboardLeft.Begin();
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
                                       name = FindElementsInElement(ele, "name").First().Value,
                                       description = FindElementsInElement(ele, "description").First().Value,
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
            if (o is TextBox || o is ComboBox || o is CheckBox)
            {
                Control c = (Control)o;
                XElement ele = (XElement)c.Tag;
                
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

                    var key = GetElementPluginPropertyKey(ele);
                    if (key != null)
                    {
                        if (!propertyValueDict.ContainsKey(key))
                            propertyValueDict.Add(key, newEntry);
                        else
                            propertyValueDict[key] = newEntry;
                    }
                } 
            }
        }

        private void DeleteControlContent(object o)
        {
            if (o is TextBox || o is ComboBox || o is CheckBox)
            {
                Control control = (Control)o;
                XElement ele = (XElement)control.Tag;

                var key = GetElementPluginPropertyKey(ele);
                if (key != null && propertyValueDict.ContainsKey(key))
                    propertyValueDict.Remove(key);
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

            Storyboard mainGridStoryboardLeft = (Storyboard)FindResource("MainGridStoryboardBack2");
            mainGridStoryboardLeft.Begin();
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

        private bool keyPressed = false;

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            var interestingKeys = new List<Key>() {Key.Up, Key.Down, Key.Left, Key.Right};
            if (this.IsMouseOver)
            {
                foreach (var k in interestingKeys)
                {
                    if ((Keyboard.GetKeyStates(k) & KeyStates.Down) > 0)
                    {
                        if (!keyPressed)
                        {
                            keyPressed = true;
                            KeyPressedDown(k);
                        }
                        return;
                    }
                }
            }
            
            keyPressed = false;
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
                        SetLastContent(null, null);
                    break;

                case Key.Right:
                    if (nextButton.IsEnabled)
                        SetNextContent(null, null);
                    break;
            }
        }

    }

    internal struct PluginPropertyValue
    {
        public string PluginName;
        public string PropertyName;
        public object Value;
    }
}
