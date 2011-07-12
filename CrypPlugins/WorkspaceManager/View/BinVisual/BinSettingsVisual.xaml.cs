
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Validation;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using System;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls.Primitives;
using Cryptool.PluginBase.Editor;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using Microsoft.Windows.Controls;


namespace WorkspaceManager.View.BinVisual
{
    public partial class BinSettingsVisual : UserControl
    {
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private readonly Thickness CONTROL_DEFAULT_MARGIN = new Thickness(4, 0, 0, 0);
        private Dictionary<ISettings, Dictionary<string, List<RadioButton>>> dicRadioButtons = new Dictionary<ISettings, Dictionary<string, List<RadioButton>>>();
        private IPlugin plugin;


        public BinSettingsVisual(IPlugin plugin)
        {
            InitializeComponent();
            this.plugin = plugin;
            this.SizeChanged += sizeChanged;
            drawList(createContentSettings(plugin));

            //this.IsVisibleChanged += isVisibleChanged;
        }

        private void sizeChanged(Object sender, EventArgs eventArgs)
        {
            //mycanvas.Width = this.ActualWidth;
            //mycanvas.Height = this.ActualHeight;
            //this.blupp.RenderTransform = new ScaleTransform(this.ActualWidth / this.blupp.ActualWidth,
              //                                              this.ActualHeight / this.blupp.ActualHeight);
          //textBoxTooltip.Text = ""+this.ActualHeight;
        }

        List<String> groups = new List<String>();

        private void isVisibleChanged(Object sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            
        }

        private void test_ContextMenuOpening(Object sender, EventArgs e)
        {
            double x = 0;
            try
            { 
                Expander temp = sender as Expander;
                if (temp.Content != null)
                {
                    StackPanel sp = temp.Content as StackPanel;
                    foreach (WrapPanel wp in sp.Children)
                    {
                        foreach (UIElement el in wp.Children)
                        {
                            if (el is TextBlock)
                            {
                                
                                if (x < el.RenderSize.Width)
                                    x = el.RenderSize.Width;
                            }
                        }
                    }
                }
            }

            catch (Exception){ }

            try
            {

                Expander temp = sender as Expander;
                if (temp.Content != null)
                {
                    StackPanel sp = temp.Content as StackPanel;
                    foreach (WrapPanel wp in sp.Children)
                    {
                        foreach (UIElement el in wp.Children)
                        {
                            if (el is TextBlock)
                            {
                                TextBlock test = el as TextBlock;
                                test.Width = x;
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        private void drawList(EntryGroup entgrou) 
        {
            List<StackPanel> gridList = new List<StackPanel>();
            
           

                foreach (List<ControlEntry> cel in entgrou.entryList)
                {
                    Expander test = new Expander();
                   
                    test.IsExpanded = true;


                    test.Expanded += test_ContextMenuOpening;

                    test.Margin = new Thickness(10);
                    test.VerticalAlignment = VerticalAlignment.Stretch;
                    
                    Binding dataBinding = new Binding("window");
                    dataBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    dataBinding.Mode = BindingMode.OneWay;
                    dataBinding.Source = this;


                    if (cel[0].tpa.groupName != null)
                    {
                        test.Header = cel[0].tpa.groupName;
                    }
                    else
                        test.Header = "Main";

                    test.Style = (Style)FindResource("GroupBoxExpander");
                    
                    //test.Background = brushlist[entgrou.entryList.IndexOf(cel)];
                    
                    

                    //test.Background = brushlist[entgrou.entryList.IndexOf(cel)];
                    StackPanel contentPanel = new StackPanel();
                    List<String> grouplist = new List<String>();
                    List<Grid> gridlist = new List<Grid>();
                    List<TextBlock> tebo = new List<TextBlock>();

                    double maxlength = 0;
                    
                    //test.Width = 400;      
                    


                    foreach (ControlEntry ce in cel) 
                    {
                        if (ce.sfa == null)
                        {
                            WrapPanel controlGrid = new WrapPanel();
                            controlGrid.Orientation = Orientation.Horizontal;
                            controlGrid.Margin = new Thickness(5);
                            TextBlock title = new TextBlock();
                            title.Text = ce.tpa.Caption;
                            title.TextWrapping = TextWrapping.Wrap;

                            //controlGrid.SetBinding(WidthProperty,dataBinding);

//                            ColumnDefinition coldef = new ColumnDefinition();
  //                          ColumnDefinition coldef2 = new ColumnDefinition();

                            //controlGrid.ColumnDefinitions.Add(coldef);
                            //controlGrid.ColumnDefinitions.Add(coldef2);


                            Grid.SetColumn(title, 0);

                            controlGrid.Children.Add(title);
                            Grid.SetColumn(ce.element, 1);

                            Label space = new Label();
                            space.Width = 5;

                            controlGrid.Children.Add(space);
                            controlGrid.Children.Add(ce.element);

                            contentPanel.Children.Add(controlGrid);

                           

                            tebo.Add(title);

                            if (maxlength < title.Text.Length)
                            {
                                maxlength = title.Text.Length;
                            }
                        }
                        else 
                        {
                            if (ce.sfa.VerticalGroup != null)
                            {
                                if (grouplist.Contains(ce.sfa.VerticalGroup))
                                {

                                    Grid controlGrid = gridlist[grouplist.IndexOf(ce.sfa.VerticalGroup)];
                                    //controlGrid.Margin = new Thickness(10);

                                    controlGrid.ColumnDefinitions.Add(new ColumnDefinition());
                                    controlGrid.ColumnDefinitions.Add(new ColumnDefinition());
                                    controlGrid.ColumnDefinitions.Add(new ColumnDefinition());

                                    TextBlock title = new TextBlock();
                                    title.Text = ce.tpa.Caption;

                                    Label space = new Label();
                                    space.Width = 5;
                                    Grid.SetColumn(title, controlGrid.ColumnDefinitions.Count - 2);

                                    controlGrid.Children.Add(space);

                                    Grid.SetColumn(space, controlGrid.ColumnDefinitions.Count - 3);

                                    controlGrid.Children.Add(title);
                                    Grid.SetColumn(ce.element, controlGrid.ColumnDefinitions.Count - 1);

                                    

                                    controlGrid.Children.Add(ce.element);

                                }
                                else
                                {
                                    grouplist.Add(ce.sfa.VerticalGroup);

                                    Grid controlGrid = new Grid();
                                    //controlGrid.Margin = new Thickness(10);


                                    controlGrid.ColumnDefinitions.Add(new ColumnDefinition());
                                    controlGrid.ColumnDefinitions.Add(new ColumnDefinition());

                                    TextBlock title = new TextBlock();
                                    title.Text = ce.tpa.Caption;

                                    Grid.SetColumn(title, 0);

                                    controlGrid.Children.Add(title);
                                    Grid.SetColumn(ce.element, 1);
                                    controlGrid.Children.Add(ce.element);

                                    contentPanel.Children.Add(controlGrid);

                                    gridlist.Add(controlGrid);
                                }
                            }
                            else 
                            {
                                WrapPanel controlGrid = new WrapPanel();
                                controlGrid.Orientation = Orientation.Horizontal;
                                controlGrid.Margin = new Thickness(5);
                                TextBlock title = new TextBlock();
                                title.Text = ce.tpa.Caption;
                                title.TextWrapping = TextWrapping.Wrap;



                                //controlGrid.SetBinding(WidthProperty,dataBinding);

                                ColumnDefinition coldef = new ColumnDefinition();
                                ColumnDefinition coldef2 = new ColumnDefinition();



                                //controlGrid.ColumnDefinitions.Add(coldef);
                                //controlGrid.ColumnDefinitions.Add(coldef2);


                                Grid.SetColumn(title, 0);

                                controlGrid.Children.Add(title);
                                Grid.SetColumn(ce.element, 1);

                                controlGrid.Children.Add(ce.element);

                                contentPanel.Children.Add(controlGrid);
                                
                                tebo.Add(title);

                                if (maxlength < title.Text.Length) ;
                                {
                                    maxlength = title.Text.Length;
                                }
                                
                            }
                        }
                    }
                   /* foreach (TextBlock te in tebo) 
                    {
                        te.Width = maxlength * 5;
                    }*/
                    test.Content = contentPanel;

                    
                    blupp.Children.Add(test);

                }
            
        }

        private EntryGroup createContentSettings(IPlugin plugin)
        {

            EntryGroup entgrou = new EntryGroup();

            

            foreach (TaskPaneAttribute tpa in plugin.Settings.GetSettingsProperties(plugin))
            //for (int i = 0; i < plugin.Settings.GetSettingsProperties(plugin).Length;i++ )
            {
                //TaskPaneAttribute tpa = plugin.Settings.GetSettingsProperties(plugin)[i];
                SettingsFormatAttribute sfa = plugin.Settings.GetSettingsFormat(tpa.PropertyName);
                if(sfa!=null)
                if (!groups.Contains(sfa.VerticalGroup))
                {
                    groups.Add(sfa.VerticalGroup);
                }

                

                Binding dataBinding = new Binding(tpa.PropertyName);
                dataBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                dataBinding.Mode = BindingMode.TwoWay;
                dataBinding.Source = plugin.Settings;

                try
                {
                    switch (tpa.ControlType)
                    {
                        #region TextBox
                        case ControlType.TextBox:

                            TextBox textbox = new TextBox();
                            textbox.MinWidth = 20;
                            textbox.Tag = tpa.ToolTip;
                            textbox.MouseEnter += Control_MouseEnter;

                            if (
                                    tpa.RegularExpression != null && tpa.RegularExpression != string.Empty)
                            {
                                ControlTemplate validationTemplate = Application.Current.Resources["validationTemplate"] as ControlTemplate;
                                RegExRule regExRule = new RegExRule();
                                regExRule.RegExValue = tpa.RegularExpression;
                                Validation.SetErrorTemplate(textbox, validationTemplate);
                                dataBinding.ValidationRules.Add(regExRule);
                                dataBinding.NotifyOnValidationError = true;
                            }

                            textbox.SetBinding(TextBox.TextProperty, dataBinding);
                            textbox.TextWrapping = TextWrapping.Wrap;
                            //controlList.Add(new ControlEntry(textbox,tpa,sfa));
                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(textbox, tpa, sfa));
                            break;

                        #endregion TextBox

                        # region NumericUpDown
                        case ControlType.NumericUpDown:
                            if (tpa.ValidationType == ValidationType.RangeInteger)
                            {
                                NumericUpDown intInput = new NumericUpDown();
                                intInput.ValueType = typeof(int);
                                intInput.SelectAllOnGotFocus = true;
                                intInput.Tag = tpa.ToolTip;
                                intInput.MouseEnter += Control_MouseEnter;
                                intInput.Maximum = tpa.IntegerMaxValue;
                                intInput.Minimum = tpa.IntegerMinValue;
                                intInput.SetBinding(NumericUpDown.ValueProperty, dataBinding);
                                entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(intInput, tpa, sfa));
                                
                            }
                            else if (tpa.ValidationType == ValidationType.RangeDouble)
                            {
                                NumericUpDown doubleInput = new NumericUpDown();
                                doubleInput.ValueType = typeof(double);
                                doubleInput.SelectAllOnGotFocus = true;
                                doubleInput.Tag = tpa.ToolTip;
                                doubleInput.MouseEnter += Control_MouseEnter;
                                doubleInput.Maximum = tpa.DoubleMaxValue;
                                doubleInput.Minimum = tpa.DoubleMaxValue;
                                doubleInput.SetBinding(NumericUpDown.ValueProperty, dataBinding);
                                entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(doubleInput, tpa, sfa));
                                
                            }
                            break;
                        # endregion NumericUpDown

                        # region ComboBox
                        case ControlType.ComboBox:
                            ComboBox comboBox = new ComboBox();

                            comboBox.Tag = tpa.ToolTip;
                            comboBox.MouseEnter += Control_MouseEnter;

                            //object value = bInfo.Settings.GetType().GetProperty(bInfo.TaskPaneSettingsAttribute.PropertyName).GetValue(bInfo.Settings, null);
                            //bool isEnum = value is Enum;

                            //if (isEnum) // use generic enum<->int converter
                            //dataBinding.Converter = EnumToIntConverter.GetInstance();

                            if (tpa.ControlValues != null) // show manually passed entries in ComboBox
                                comboBox.ItemsSource = tpa.ControlValues;
                            // else if (isEnum) // show automatically derived enum entries in ComboBox
                            //   comboBox.ItemsSource = Enum.GetValues(value.GetType());
                            else // nothing to show
                                GuiLogMessage("No ComboBox entries given", NotificationLevel.Error);

                            comboBox.SetBinding(ComboBox.SelectedIndexProperty, dataBinding);
                            //controlList.Add(new ControlEntry(comboBox, tpa, sfa));
                            entgrou.AddNewEntry(tpa.GroupName,new ControlEntry(comboBox, tpa, sfa));
                            break;

                        # endregion ComboBox

                        # region RadioButton
                        
                        case ControlType.RadioButton:
                            try
                            {
                                dataBinding = new Binding("IsChecked");
                                //bInfo.Settings.PropertyChanged += RadioButton_PropertyChanged;
                                if (!dicRadioButtons.ContainsKey(plugin.Settings))
                                {
                                    dicRadioButtons.Add(plugin.Settings, new Dictionary<string, List<RadioButton>>());
                                }
                                List<RadioButton> list = new List<RadioButton>();
                                StackPanel panelRadioButtons = new StackPanel();
                                panelRadioButtons.ToolTip = tpa.ToolTip;
                                panelRadioButtons.MouseEnter += Control_MouseEnter;
                                panelRadioButtons.Margin = CONTROL_DEFAULT_MARGIN;

                                int selectedRadioButton = (int)plugin.Settings.GetType().GetProperty(tpa.PropertyName).GetValue(plugin.Settings, null);
                                string groupNameExtension = Guid.NewGuid().ToString();
                                foreach (string stringValue in tpa.ControlValues)
                                {
                                    RadioButton radio = new RadioButton();
                                    radio.GroupName = tpa.PropertyName + groupNameExtension;
                                    radio.Content = stringValue;
                                    if (panelRadioButtons.Children.Count == selectedRadioButton)
                                    {
                                        radio.IsChecked = true;
                                    }

                                    radio.Tag = new RadioButtonListAndBindingInfo(list, plugin,tpa);
                                    radio.Checked += RadioButton_Checked;
                                  // radio.SetBinding(RadioButton.IsCheckedProperty, dataBinding);
                                    panelRadioButtons.Children.Add(radio);
                                    list.Add(radio);
                                }
                                dicRadioButtons[plugin.Settings].Add(tpa.PropertyName, list);
                                entgrou.AddNewEntry(tpa.GroupName,new ControlEntry(panelRadioButtons, tpa, sfa));
                             //   bInfo.CaptionGUIElement = panelRadioButtons;
                            }
                            catch (Exception ex)
                            {
                                GuiLogMessage(ex.Message, NotificationLevel.Error);
                            }
                            break;
                            
                        #endregion RadioButton

                        # region CheckBox
                        case ControlType.CheckBox:
                            CheckBox checkBox = new CheckBox();
                            checkBox.Margin = CONTROL_DEFAULT_MARGIN;
                            checkBox.Content = tpa.Caption;
                            checkBox.Tag = tpa.ToolTip;
                            checkBox.MouseEnter += Control_MouseEnter;
                            checkBox.SetBinding(CheckBox.IsCheckedProperty, dataBinding);
                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(checkBox, tpa, sfa));
                            
                            break;
                        # endregion CheckBox
                            
                        # region DynamicComboBox
                        case ControlType.DynamicComboBox:
                            PropertyInfo pInfo = plugin.Settings.GetType().GetProperty(tpa.ControlValues[0]);
                                                    
                            ObservableCollection<string> coll = pInfo.GetValue(plugin.Settings, null) as ObservableCollection<string>;
                                
                            if (coll != null)
                            {
                                ComboBox comboBoxDyn = new ComboBox();
                                comboBoxDyn.Tag = tpa.ToolTip;
                                comboBoxDyn.MouseEnter += Control_MouseEnter;
                                comboBoxDyn.ItemsSource = coll;
                                comboBoxDyn.SetBinding(ComboBox.SelectedIndexProperty, dataBinding);
                                //inputControl = comboBoxDyn;
                                //bInfo.CaptionGUIElement = comboBoxDyn;

                                //controlList.Add(new ControlEntry(comboBoxDyn, tpa, sfa));
                                entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(comboBoxDyn, tpa, sfa));
                            }
                            break;
                            # endregion DynamicComboBox

                        # region FileDialog
                        case ControlType.SaveFileDialog:
                        case ControlType.OpenFileDialog:
                            StackPanel sp = new StackPanel();
                            sp.Orientation = Orientation.Vertical;

                            TextBox fileTextBox = new TextBox();
                            fileTextBox.Background = Brushes.LightGray;
                            fileTextBox.IsReadOnly = true;
                            fileTextBox.Margin = new Thickness(0, 0, 0, 5);
                            fileTextBox.TextChanged += fileDialogTextBox_TextChanged;
                            fileTextBox.SetBinding(TextBox.TextProperty, dataBinding);
                            fileTextBox.SetBinding(TextBox.ToolTipProperty, dataBinding);

                            fileTextBox.Tag = tpa;
                            fileTextBox.MouseEnter += fileTextBox_MouseEnter;
                            sp.Children.Add(fileTextBox);

                            Button btn = new Button();
                            btn.Tag = fileTextBox;
                            if (tpa.ControlType == ControlType.SaveFileDialog)
                                //btn.Content = Properties.Resources.Save_file;
                                btn.Content = "Save File";
                            else
                                btn.Content = "Open File";
                            btn.Click += FileDialogClick;
                            sp.Children.Add(btn);
                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(sp, tpa, sfa));
                           
                            break;
                        # endregion FileDialog

                        # region Button
                        case ControlType.Button:
                            Button taskPaneButton = new Button();
                            taskPaneButton.Margin = new Thickness(0);
                            taskPaneButton.Tag = tpa;
                            taskPaneButton.MouseEnter += TaskPaneButton_MouseEnter;
                            taskPaneButton.Content = tpa.Caption;
                            taskPaneButton.Click += TaskPaneButton_Click;
                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(taskPaneButton, tpa, sfa));
                            break;
                        # endregion Button
                          
                        # region Slider
                        case ControlType.Slider:
                            Slider slider = new Slider();
                            slider.Margin = CONTROL_DEFAULT_MARGIN;
                            slider.Orientation = Orientation.Horizontal;
                            slider.Minimum = tpa.DoubleMinValue;
                            slider.Maximum = tpa.DoubleMaxValue;
                            slider.Tag = tpa.ToolTip;
                            slider.MouseEnter += Control_MouseEnter;
                            slider.SetBinding(Slider.ValueProperty, dataBinding);

                            slider.Width = 100;

                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(slider, tpa, sfa));
                            break;
                        # endregion Slider

                        # region TextBoxReadOnly
                        case ControlType.TextBoxReadOnly:
                            TextBox textBoxReadOnly = new TextBox();
                            textBoxReadOnly.IsReadOnly = true;
                            textBoxReadOnly.BorderThickness = new Thickness(0);
                            textBoxReadOnly.Background = Brushes.Transparent;
                            textBoxReadOnly.Tag = tpa.ToolTip;
                            textBoxReadOnly.MouseEnter += Control_MouseEnter;
                            textBoxReadOnly.SetBinding(TextBox.TextProperty, dataBinding);
                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(textBoxReadOnly, tpa, sfa));
                            break;
                        # endregion TextBoxReadOnly

                    }

                     
                }

                catch (Exception) { }
            }
            return entgrou;

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is RadioButton)
                {
                    RadioButton radio = sender as RadioButton;
                    if (radio.Tag is RadioButtonListAndBindingInfo)
                    {
                        RadioButtonListAndBindingInfo rbl = radio.Tag as RadioButtonListAndBindingInfo;
                        rbl.plugin.Settings.GetType().GetProperty(rbl.tpa.PropertyName).SetValue(rbl.plugin.Settings, rbl.List.IndexOf(radio), null);
                    }
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        private void fileDialogTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ((TextBox)sender).ScrollToHorizontalOffset(int.MaxValue);
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private void fileTextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender is TextBox) SetHelpText(((sender as TextBox).Tag as TaskPaneAttribute).ToolTip);
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private void FileDialogClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                TextBox tb = btn.Tag as TextBox;
                TaskPaneAttribute tpAtt = tb.Tag as TaskPaneAttribute;

                if (tpAtt.ControlType == ControlType.OpenFileDialog)
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Filter = tpAtt.FileExtension;
                    ofd.Multiselect = false;
                    bool? test = ofd.ShowDialog();
                    if (test.HasValue && test.Value) tb.Text = ofd.FileName;
                }
                else if (tpAtt.ControlType == ControlType.SaveFileDialog)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = tpAtt.FileExtension;
                    bool? test = saveFileDialog.ShowDialog();
                    if (test.HasValue && test.Value) tb.Text = saveFileDialog.FileName;
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private void Control_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender is NumericUpDown) SetHelpText((sender as NumericUpDown).Tag as string);
                if (sender is NumericUpDown) SetHelpText((sender as NumericUpDown).Tag as string);
                if (sender is TextBox) SetHelpText((sender as TextBox).Tag as string);
                //if (sender is PasswordBox) SetHelpText(((BindingInfo)(sender as PasswordBox).Tag).TaskPaneSettingsAttribute.ToolTip as string);
                if (sender is CheckBox) SetHelpText((sender as CheckBox).Tag as string);
                if (sender is ComboBox) SetHelpText((sender as ComboBox).Tag as string);
                if (sender is Slider) SetHelpText((sender as Slider).Tag as string);
                if (sender is Button) SetHelpText((sender as Button).Tag as string);
            }
            catch (Exception)
            {
                // GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private void SetHelpText(string text)
        {
            try
            {
                textBoxTooltip.Text = text;
                textBoxTooltip.Foreground = Brushes.Black;
            }
            catch (Exception)
            {
                //GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, new GuiLogEventArgs(message, null, logLevel));
        }

        private void TaskPaneButton_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                SetHelpText(((sender as Button).Tag as TaskPaneAttribute).ToolTip);
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private void TaskPaneButton_Click(object sender, RoutedEventArgs e)
        {
            TaskPaneAttribute tpa = (sender as Button).Tag as TaskPaneAttribute;
            if (tpa != null && plugin.Settings != null && tpa.Method != null)
            {
                tpa.Method.Invoke(plugin.Settings, null);
            }
        }
        
    }

    public class EntryGroup 
    {
        
        public List<String> listAdmin = new List<String>();
        public List<List<ControlEntry>> entryList = new List<List<ControlEntry>>();



        public void AddNewEntry(String groupname , ControlEntry entry)
        {
            if (listAdmin.Contains(groupname))
            {
                listAdmin.IndexOf(groupname);
                entryList[listAdmin.IndexOf(groupname)].Add(entry);
            }
            else 
            {
                List<ControlEntry> dummyList = new List<ControlEntry>();
                dummyList.Add(entry);
                listAdmin.Add(groupname);
                entryList.Add(dummyList);
            }

        }

    }

    public class RadioButtonListAndBindingInfo
    {
        public readonly List<RadioButton> List = null;
        public readonly IPlugin plugin = null;
        public readonly TaskPaneAttribute tpa = null;

        public RadioButtonListAndBindingInfo(List<RadioButton> list, IPlugin plugin, TaskPaneAttribute tpa)
        {
            if (list == null) throw new ArgumentException("list");
            if (plugin == null) throw new ArgumentException("bInfo");
            if (tpa == null) throw new ArgumentException("tpa");
            this.tpa = tpa;
            this.List = list;
            this.plugin = plugin;
        }
    }

    public class ControlEntry
    {
        public UIElement element;
        public TaskPaneAttribute tpa;
        public SettingsFormatAttribute sfa;


       public ControlEntry (UIElement element, TaskPaneAttribute tpa, SettingsFormatAttribute sfa)
        {
            this.element = element;
            this.sfa = sfa;
            this.tpa = tpa;
        }
    }
}

