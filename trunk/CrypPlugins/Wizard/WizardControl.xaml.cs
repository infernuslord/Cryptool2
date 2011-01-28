using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private List<string> choicePath = new List<string>();
        private SolidColorBrush selectionBrush = new SolidColorBrush();
        private const string configXMLPath = "Wizard.Config.wizard.config.start.xml";
        private const string defaultLang = "en";
        private XElement wizardConfigXML;
        private Dictionary<string, PluginPropertyValue> propertyValueDict = new Dictionary<string, PluginPropertyValue>();

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

        private void SetupPage(XElement element)
        {
            if ((element.Name == "loadSample") && (element.Attribute("file") != null) && (element.Attribute("title") != null))
            {
                LoadSample(element.Attribute("file").Value, element.Attribute("title").Value);
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

            nextButton.IsEnabled = false;

            //set headline
            XElement headline = FindElementsInElement(element, "headline").First();
            if (headline != null)
                taskHeader.Content = headline.Value;

            //set description label
            XElement desc = FindElementsInElement(element, "desc").First();
            if (desc != null)
                descHeader.Content = desc.Value;


            if (element.Name == "input")
            {
                categoryGrid.Visibility = Visibility.Hidden;
                inputPanel.Visibility = Visibility.Visible;

                var inputs = element.Elements("inputBox");
                inputs = inputs.Union(element.Elements("comboBox"));
                inputs = inputs.Union(element.Elements("checkBox"));
                inputStack.Children.Clear();
                
                foreach (var input in inputs)
                {
                    var description = new Label();
                    description.Content = FindElementsInElement(input, "description").First().Value;
                    inputStack.Children.Add(description);

                    if (input.Name == "inputBox")
                    {
                        var textBox = new TextBox();
                        textBox.Tag = input;
                        textBox.MinLines = 4;
                        inputStack.Children.Add(textBox);
                        if (propertyValueDict.ContainsKey(GetElementID(input)))
                            textBox.Text = (string)propertyValueDict[GetElementID(input)].Value;
                        else if (input.Attribute("defaultValue") != null)
                            textBox.Text = input.Attribute("defaultValue").Value; 
                    }
                    else if (input.Name == "comboBox")
                    {
                        ComboBox cb = new ComboBox();
                        cb.Tag = input;

                        var items = FindElementsInElement(input, "item");
                        foreach (var item in items)
                        {
                            ComboBoxItem cbi = new ComboBoxItem();
                            if (item.Attribute("content") != null)
                                cbi.Content = item.Attribute("content").Value;
                            cb.Items.Add(cbi);
                        }

                        
                        if (propertyValueDict.ContainsKey(GetElementID(input)))
                        {
                            if (propertyValueDict[GetElementID(input)].Value is int)
                            {
                                ComboBoxItem cbi = (ComboBoxItem)cb.Items.GetItemAt((int)propertyValueDict[GetElementID(input)].Value);
                                cbi.IsSelected = true;
                            }
                        }
                        else if (input.Attribute("defaultValue") != null)
                        {
                            int i = 0;
                            if (Int32.TryParse(input.Attribute("defaultValue").Value, out i))
                            {
                                ComboBoxItem cbi = (ComboBoxItem)cb.Items.GetItemAt(i);
                                cbi.IsSelected = true;
                            }
                        }

                        inputStack.Children.Add(cb);
                    }
                    else if (input.Name == "checkBox")
                    {
                        CheckBox cb = new CheckBox();
                        cb.Tag = input;

                        if (input.Attribute("content") != null)
                            cb.Content = input.Attribute("content").Value;
                        if (propertyValueDict.ContainsKey(GetElementID(input)))
                        {
                            string value = (string)propertyValueDict[GetElementID(input)].Value;
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

                        inputStack.Children.Add(cb);
                    }
                }

                var next = element.Element("input");
                if (next != null)
                    inputPanel.Tag = next;
                else
                    inputPanel.Tag = element.Element("category");
                if (inputPanel.Tag == null)
                    inputPanel.Tag = element.Element("loadSample");

                if (inputPanel.Tag != null)
                    nextButton.IsEnabled = true;

                string id = GetElementID((XElement)inputPanel.Tag);

                if (choicePath.Count > 0 && id == choicePath.Last())
                    choicePath.RemoveAt(choicePath.IndexOf(choicePath.Last()));
            }

            else if (element.Name == "category")
            {
                categoryGrid.Visibility = Visibility.Visible;
                inputPanel.Visibility = Visibility.Hidden;
                
                radioButtonStackPanel.Children.Clear();
                //ResetBackground();

                //generate radio buttons
                IEnumerable<XElement> categories = element.Elements("category");
                IEnumerable<XElement> inputs = element.Elements("input");
                IEnumerable<XElement> loadSamples = element.Elements("loadSamples");

                if (inputs.Any())
                    categories = categories.Union(inputs);
                if (loadSamples.Any())
                    categories = categories.Union(loadSamples);

                if (categories.Any())
                {
                    bool isSelected = false;

                    foreach (XElement ele in categories)
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

                        radioButtonStackPanel.Children.Add(rb);
                        if (choicePath.Count > 0 && id == choicePath.Last())
                        {
                            choicePath.RemoveAt(choicePath.IndexOf(choicePath.Last()));
                            rb.IsChecked = true;
                            isSelected = true;
                            nextButton.IsEnabled = true;
                        }
                    }

                    if (!isSelected)
                    {
                        RadioButton b = (RadioButton)radioButtonStackPanel.Children[0];
                        b.IsChecked = true;
                    }

                }
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
            newEditor.Open(model);                   
        }

        void rb_Checked(object sender, RoutedEventArgs e)
        {
            ResetBackground();
            RadioButton b = (RadioButton)sender;
            b.Background = Brushes.LightSeaGreen;
            Border c = (Border)b.Content;
            c.BorderThickness = new Thickness(1, 1, 0, 1);
            c.Background = selectionBrush;
            XElement ele = (XElement)b.Tag;
            XElement desc = FindElementsInElement(ele, "description").First();
            if (desc != null)
                description.Text = desc.Value;
            nextButton.IsEnabled = true;
        }

        private void ResetBackground()
        {
            for (int i = 0; i < radioButtonStackPanel.Children.Count; i++)
            {
                RadioButton b = (RadioButton)radioButtonStackPanel.Children[i];
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
            propertyValueDict.Clear();
            ResetBackground();
            radioButtonStackPanel.Children.Clear();
            choicePath.Clear();
            description.Text = "";
            SetupPage(wizardConfigXML);
        }

        private void SetNextContent(object sender, EventArgs e)
        {
            if (categoryGrid.IsVisible)
            {
                for (int i = 0; i < radioButtonStackPanel.Children.Count; i++)
                {
                    RadioButton b = (RadioButton) radioButtonStackPanel.Children[i];
                    if (b.IsChecked != null && (bool) b.IsChecked)
                    {
                        choicePath.Add(GetElementID((XElement) b.Tag));
                        SetupPage((XElement) b.Tag);
                        break;
                    }
                }
            }
            else if (inputPanel.IsVisible)
            {
                foreach (var child in inputStack.Children)
                {
                    SaveControlContent(child);
                }
                var nextElement = (XElement) inputPanel.Tag;
                choicePath.Add(GetElementID(nextElement));
                SetupPage(nextElement);
            }

            Storyboard mainGridStoryboardLeft = (Storyboard)FindResource("MainGridStoryboardNext2");
            mainGridStoryboardLeft.Begin();
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
            if (categoryGrid.IsVisible && radioButtonStackPanel.Children.Count > 0)
            {
                RadioButton b = (RadioButton) radioButtonStackPanel.Children[0];
                ele = (XElement) b.Tag;
            }
            else if (inputPanel.IsVisible)
            {
                foreach (var child in inputStack.Children)
                {
                    DeleteControlContent(child);
                }

                ele = (XElement) inputPanel.Tag;
            }

            if (ele != null)
            {
                XElement grandParent = ele.Parent.Parent;
                if (grandParent != null)
                    SetupPage(grandParent);
                else
                    SetupPage(wizardConfigXML);
            }

            Storyboard mainGridStoryboardLeft = (Storyboard)FindResource("MainGridStoryboardBack2");
            mainGridStoryboardLeft.Begin();
        }

        private void GuiLogMessage(string message, NotificationLevel loglevel)
        {
            if (OnGuiLogNotificationOccured != null)
                OnGuiLogNotificationOccured(null, new GuiLogEventArgs(message, null, loglevel));
        }
    }

    internal struct PluginPropertyValue
    {
        public string PluginName;
        public string PropertyName;
        public object Value;
    }
}
