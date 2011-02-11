/*
   Copyright 2008 Martin Saternus, Arno Wacker, Thomas Schmid, Sebastian Przybylski

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
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
using System.Text.RegularExpressions;

namespace WorkspaceManager.View.VisualComponents
{
    public partial class TaskPaneCtrl : UserControl
    {
        private const int CONTROL_OFFSET = 10;
        private const int SETTINGS_FORMAT_INDENT_OFFSET = 15;
        private const int EXPANDER_GROUP_LEFT_RIGHT_OFFSET = 5;
        private readonly Thickness CONTROL_DEFAULT_MARGIN = new Thickness(4, 0, 0, 0);
        private Dictionary<IPlugin, List<UIElement>> dicPluginSettings = new Dictionary<IPlugin, List<UIElement>>();
        private Dictionary<IPlugin, List<UIElement>> dicPluginSettingsToDisable = new Dictionary<IPlugin, List<UIElement>>();
        private Dictionary<ISettings, Dictionary<string, TaskPaneSettingsForPlugins>> dicAllPluginSettings = new Dictionary<ISettings, Dictionary<string, TaskPaneSettingsForPlugins>>();
        // private List<TaskPaneSettingsForPlugins> listAllPluginSettings = new List<TaskPaneSettingsForPlugins>();
        private List<IPlugin> listAlwaysDisabled = new List<IPlugin>();
        private IPlugin activePlugin;
        private Dictionary<ISettings, Dictionary<string, List<RadioButton>>> dicRadioButtons = new Dictionary<ISettings, Dictionary<string, List<RadioButton>>>();

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public TaskPaneCtrl()
        {
            InitializeComponent();
            this.Loaded += TaskPaneCtrl_Loaded;
            this.Height = double.NaN;
            this.Width = double.NaN;
        }

        void TaskPaneCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            // Hide the native expand button of naviPane, because we use resize/hide functions of SplitPanel Element
            //Button naviPaneExpandButton = navPaneSettings.Template.FindName("ExpandButton", navPaneSettings) as Button;
            //if (naviPaneExpandButton != null) naviPaneExpandButton.Visibility = Visibility.Collapsed;
        }

        public void ClearCache()
        {
            dicPluginSettings.Clear();
            dicPluginSettingsToDisable.Clear();

            // mwander 20100224
            dicAllPluginSettings.Clear();
            listAlwaysDisabled.Clear();
            dicRadioButtons.Clear();
            ClearCurrentChildren();
        }

        private void ClearCurrentChildren()
        {
            //stackPanelContent.Children.Clear();
        }

        private bool isChangeable = true;
        public bool IsChangeable
        {
            get { return isChangeable; }
            set
            {
                isChangeable = value;
                if (activePlugin != null) SetPluginEnabled(activePlugin, isChangeable);
            }
        }

        public void SetPluginEnabled(IPlugin plugin, bool isEnabled)
        {
            try
            {
                if (dicPluginSettingsToDisable.ContainsKey(plugin))
                {
                    foreach (UIElement ui in dicPluginSettingsToDisable[plugin])
                    {
                        ui.IsEnabled = isEnabled && !listAlwaysDisabled.Contains(plugin);
                    }
                    this.isChangeable = isEnabled;
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        public void DisplayPluginSettings(IPlugin plugin, string title, DisplayPluginMode mode)
        {
            try
            {
                ClearCurrentChildren();
                activePlugin = plugin;

                if (plugin != null)
                {
                    if (title == null) title = string.Empty;

                    ISettings settings = plugin.Settings;
                    if (settings != null && !dicPluginSettings.ContainsKey(plugin))
                    {
                        if (mode == DisplayPluginMode.Disabled && !listAlwaysDisabled.Contains(plugin))
                            listAlwaysDisabled.Add(plugin);

                        List<BindingInfo> bindingList = new List<BindingInfo>();
                        foreach (TaskPaneAttribute tpa in settings.GetSettingsProperties(plugin))
                        {
                            BindingInfo bInfo = null;
                            bInfo = new BindingInfo(tpa);
                            bindingList.Add(bInfo);
                            bInfo.Settings = settings;
                            if (bInfo != null && settings.GetSettingsFormat(bInfo.TaskPaneSettingsAttribute.PropertyName) != null)
                                bInfo.SettingFormat = settings.GetSettingsFormat(bInfo.TaskPaneSettingsAttribute.PropertyName);
                        }
                        bindingList.Sort(new BindingInfoComparer());
                        AddOutputControls(bindingList, settings, plugin, title);

                        // save elements to list, on next display request they won't be recreated and the old group-expand-state still exists              
                        List<UIElement> list = new List<UIElement>();
                        foreach (UIElement uIElement in stackPanelContent.Children)
                        {
                            list.Add(uIElement);
                        }
                        dicPluginSettings.Add(plugin, list);
                    }
                    else if (dicPluginSettings.ContainsKey(plugin))
                    {
                        ClearCurrentChildren();
                        foreach (UIElement ui in dicPluginSettings[plugin])
                        {
                            // baseStackPanel.Children.Add(ui);
                            stackPanelContent.Children.Add(ui);
                        }
                    }
                    this.SetPluginEnabled(plugin, isChangeable);
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }



        /// <summary>
        /// All the GUI elements used to display the options are created here.
        /// </summary>
        /// <param name="bindingList">The binding list.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="plugin">The plugin.</param>
        /// <param name="title">The title.</param>
        private void AddOutputControls(List<BindingInfo> bindingList, ISettings settings, IPlugin plugin, string title)
        {
            try
            {
                ClearCurrentChildren();

                string emptyGroup = Guid.NewGuid().ToString();
                Dictionary<string, List<UIElement>> dicGroupedElements = new Dictionary<string, List<UIElement>>();
                dicGroupedElements.Add(emptyGroup, new List<UIElement>());
                Dictionary<string, List<KeyValuePair<BindingInfo, UIElement>>> dicVerticalSubGroups = new Dictionary<string, List<KeyValuePair<BindingInfo, UIElement>>>();
                bool taskPaneAttributesCanChange = plugin.Settings.GetTaskPaneAttributeChanged() != null;
                foreach (BindingInfo bInfo in bindingList)
                {
                    Binding dataBinding = new Binding(bInfo.TaskPaneSettingsAttribute.PropertyName);
                    dataBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    dataBinding.Mode = BindingMode.TwoWay;
                    dataBinding.Source = settings;

                    UIElement inputControl = null;

                    switch (bInfo.TaskPaneSettingsAttribute.ControlType)
                    {
                        # region TextBox
                        case ControlType.TextBox:
                            TextBox textbox = new TextBox();
                            textbox.Tag = bInfo.TaskPaneSettingsAttribute.ToolTip;
                            textbox.MouseEnter += Control_MouseEnter;
                            if (
                                bInfo.TaskPaneSettingsAttribute.RegularExpression != null &&
                                bInfo.TaskPaneSettingsAttribute.RegularExpression != string.Empty)
                            {
                                ControlTemplate validationTemplate = Application.Current.Resources["validationTemplate"] as ControlTemplate;
                                Style tbStyle = Application.Current.Resources["textStyleTextBox"] as Style;
                                textbox.Style = tbStyle;
                                RegExRule regExRule = new RegExRule();
                                regExRule.RegExValue = bInfo.TaskPaneSettingsAttribute.RegularExpression;
                                Validation.SetErrorTemplate(textbox, validationTemplate);
                                dataBinding.ValidationRules.Add(regExRule);
                                dataBinding.NotifyOnValidationError = true;
                            }
                            // this flag is set to true here, because only then the OnPropertyChanged events in 
                            // the property setter will result in an GUI update of this textBox. 
                            // TODO: add IMultiValueConverter option to TaskPane settings. 
                            // dataBinding.IsAsync = true;                                     
                            textbox.SetBinding(TextBox.TextProperty, dataBinding);
                            inputControl = textbox;
                            break;
                        # endregion TextBox
                        # region NumericUpDown
                        case ControlType.NumericUpDown:
                            if (bInfo.TaskPaneSettingsAttribute.ValidationType == ValidationType.RangeInteger)
                            {
                                TextBlock intInput = new TextBlock();
                                intInput.Tag = bInfo.TaskPaneSettingsAttribute.ToolTip;
                                intInput.MouseEnter += Control_MouseEnter;
                                //intInput.MaxValue = bInfo.TaskPaneSettingsAttribute.IntegerMaxValue;
                                //intInput.MinValue = bInfo.TaskPaneSettingsAttribute.IntegerMinValue;
                                intInput.SetBinding(TextBlock.TextProperty, dataBinding);
                                inputControl = intInput;
                                bInfo.CaptionGUIElement = intInput;
                            }
                            else if (bInfo.TaskPaneSettingsAttribute.ValidationType == ValidationType.RangeDouble)
                            {
                                TextBlock doubleInput = new TextBlock();
                                doubleInput.Tag = bInfo.TaskPaneSettingsAttribute.ToolTip;
                                doubleInput.MouseEnter += Control_MouseEnter;
                                //doubleInput.MaxValue = bInfo.TaskPaneSettingsAttribute.DoubleMaxValue;
                                //doubleInput.MinValue = bInfo.TaskPaneSettingsAttribute.DoubleMaxValue;
                                doubleInput.SetBinding(TextBlock.TextProperty, dataBinding);
                                inputControl = doubleInput;
                                bInfo.CaptionGUIElement = doubleInput;
                            }
                            break;
                        # endregion NumericUpDown
                        # region ComboBox
                        case ControlType.ComboBox:
                            ComboBox comboBox = new ComboBox();
                            comboBox.Margin = new Thickness(0);
                            comboBox.Tag = bInfo.TaskPaneSettingsAttribute.ToolTip;
                            comboBox.MouseEnter += Control_MouseEnter;

                            object value = bInfo.Settings.GetType().GetProperty(bInfo.TaskPaneSettingsAttribute.PropertyName).GetValue(bInfo.Settings, null);
                            bool isEnum = value is Enum;

                            if (isEnum) // use generic enum<->int converter
                                dataBinding.Converter = EnumToIntConverter.GetInstance();

                            if (bInfo.TaskPaneSettingsAttribute.ControlValues != null) // show manually passed entries in ComboBox
                                comboBox.ItemsSource = bInfo.TaskPaneSettingsAttribute.ControlValues;
                            else if (isEnum) // show automatically derived enum entries in ComboBox
                                comboBox.ItemsSource = Enum.GetValues(value.GetType());
                            else // nothing to show
                                GuiLogMessage("No ComboBox entries given", NotificationLevel.Error);

                            comboBox.SetBinding(ComboBox.SelectedIndexProperty, dataBinding);
                            inputControl = comboBox;
                            bInfo.CaptionGUIElement = comboBox;
                            break;
                        # endregion ComboBox
                        # region RadioButton
                        case ControlType.RadioButton:
                            try
                            {
                                bInfo.Settings.PropertyChanged += RadioButton_PropertyChanged;
                                if (!dicRadioButtons.ContainsKey(bInfo.Settings))
                                {
                                    dicRadioButtons.Add(bInfo.Settings, new Dictionary<string, List<RadioButton>>());
                                }
                                List<RadioButton> list = new List<RadioButton>();
                                StackPanel panelRadioButtons = new StackPanel();
                                panelRadioButtons.ToolTip = bInfo.TaskPaneSettingsAttribute.ToolTip;
                                panelRadioButtons.MouseEnter += Control_MouseEnter;
                                panelRadioButtons.Margin = CONTROL_DEFAULT_MARGIN;

                                int selectedRadioButton = (int)bInfo.Settings.GetType().GetProperty(bInfo.TaskPaneSettingsAttribute.PropertyName).GetValue(bInfo.Settings, null);
                                string groupNameExtension = Guid.NewGuid().ToString();
                                foreach (string stringValue in bInfo.TaskPaneSettingsAttribute.ControlValues)
                                {
                                    RadioButton radio = new RadioButton();
                                    radio.GroupName = bInfo.TaskPaneSettingsAttribute.PropertyName + groupNameExtension;
                                    radio.Content = stringValue;
                                    if (panelRadioButtons.Children.Count == selectedRadioButton)
                                    {
                                        radio.IsChecked = true;
                                    }
                                    radio.Tag = new RadioButtonListAndBindingInfo(list, bInfo);
                                    radio.Checked += RadioButton_Checked;
                                    panelRadioButtons.Children.Add(radio);
                                    list.Add(radio);
                                }
                                dicRadioButtons[bInfo.Settings].Add(bInfo.TaskPaneSettingsAttribute.PropertyName, list);
                                inputControl = panelRadioButtons;
                                bInfo.CaptionGUIElement = panelRadioButtons;
                            }
                            catch (Exception ex)
                            {
                                GuiLogMessage(ex.Message, NotificationLevel.Error);
                            }
                            break;
                        #endregion RadioButton
                        # region DynamicComboBox
                        case ControlType.DynamicComboBox:
                            PropertyInfo pInfo = settings.GetType().GetProperty(bInfo.TaskPaneSettingsAttribute.ControlValues[0]);
                            ObservableCollection<string> coll = pInfo.GetValue(settings, null) as ObservableCollection<string>;
                            if (coll != null)
                            {
                                ComboBox comboBoxDyn = new ComboBox();
                                comboBoxDyn.Tag = bInfo.TaskPaneSettingsAttribute.ToolTip;
                                comboBoxDyn.MouseEnter += Control_MouseEnter;
                                comboBoxDyn.ItemsSource = coll;
                                comboBoxDyn.SetBinding(ComboBox.SelectedIndexProperty, dataBinding);
                                inputControl = comboBoxDyn;
                                bInfo.CaptionGUIElement = comboBoxDyn;
                            }
                            break;
                        # endregion DynamicComboBox
                        # region CheckBox
                        case ControlType.CheckBox:
                            CheckBox checkBox = new CheckBox();
                            checkBox.Margin = CONTROL_DEFAULT_MARGIN;
                            checkBox.Content = bInfo.TaskPaneSettingsAttribute.Caption;
                            checkBox.Tag = bInfo.TaskPaneSettingsAttribute.ToolTip;
                            checkBox.MouseEnter += Control_MouseEnter;
                            checkBox.SetBinding(CheckBox.IsCheckedProperty, dataBinding);
                            inputControl = checkBox;
                            bInfo.CaptionGUIElement = checkBox;
                            break;
                        # endregion CheckBox
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

                            fileTextBox.Tag = bInfo.TaskPaneSettingsAttribute;
                            fileTextBox.MouseEnter += fileTextBox_MouseEnter;
                            sp.Children.Add(fileTextBox);

                            Button btn = new Button();
                            btn.Tag = fileTextBox;
                            if (bInfo.TaskPaneSettingsAttribute.ControlType == ControlType.SaveFileDialog) btn.Content = "Save file";
                            else btn.Content = "Open file";
                            btn.Click += FileDialogClick;
                            sp.Children.Add(btn);
                            inputControl = sp;
                            bInfo.CaptionGUIElement = fileTextBox;
                            break;
                        # endregion FileDialog
                        # region Button
                        case ControlType.Button:
                            Button taskPaneButton = new Button();
                            taskPaneButton.Margin = new Thickness(0);
                            taskPaneButton.Tag = bInfo;
                            taskPaneButton.MouseEnter += TaskPaneButton_MouseEnter;
                            taskPaneButton.Content = bInfo.TaskPaneSettingsAttribute.Caption;
                            taskPaneButton.Click += TaskPaneButton_Click;
                            inputControl = taskPaneButton;
                            bInfo.CaptionGUIElement = taskPaneButton;
                            break;
                        # endregion Button
                        # region Slider
                        case ControlType.Slider:
                            Slider slider = new Slider();
                            slider.Margin = CONTROL_DEFAULT_MARGIN;
                            slider.Orientation = Orientation.Horizontal;
                            slider.Minimum = bInfo.TaskPaneSettingsAttribute.DoubleMinValue;
                            slider.Maximum = bInfo.TaskPaneSettingsAttribute.DoubleMaxValue;
                            slider.Tag = bInfo.TaskPaneSettingsAttribute.ToolTip;
                            slider.MouseEnter += Control_MouseEnter;
                            slider.SetBinding(Slider.ValueProperty, dataBinding);
                            inputControl = slider;
                            bInfo.CaptionGUIElement = slider;
                            break;
                        # endregion Slider
                        # region TextBoxReadOnly
                        case ControlType.TextBoxReadOnly:
                            TextBox textBoxReadOnly = new TextBox();
                            textBoxReadOnly.IsReadOnly = true;
                            textBoxReadOnly.BorderThickness = new Thickness(0);
                            textBoxReadOnly.Background = Brushes.Transparent;
                            textBoxReadOnly.Tag = bInfo.TaskPaneSettingsAttribute.ToolTip;
                            textBoxReadOnly.MouseEnter += Control_MouseEnter;
                            textBoxReadOnly.SetBinding(TextBox.TextProperty, dataBinding);
                            inputControl = textBoxReadOnly;
                            bInfo.CaptionGUIElement = textBoxReadOnly;
                            break;
                        # endregion TextBoxReadOnly
                        #region TextBoxHidden
                        case ControlType.TextBoxHidden:
                            PasswordBox passwordBox = new PasswordBox();
                            passwordBox.Tag = bInfo;
                            passwordBox.MouseEnter += Control_MouseEnter;
                            passwordBox.Password = bInfo.Settings.GetType().GetProperty(bInfo.TaskPaneSettingsAttribute.PropertyName).GetValue(bInfo.Settings, null) as string;
                            passwordBox.PasswordChanged += TextBoxHidden_Changed;
                            inputControl = passwordBox;
                            break;
                        #endregion TextBoxHidden
                    }
                    inputControl.MouseLeave += Control_MouseLeave;

                    // elements in this list will be disabled while chain is running or if "disabled" state is requested
                    if (!bInfo.TaskPaneSettingsAttribute.ChangeableWhileExecuting && bInfo.TaskPaneSettingsAttribute.ControlType != ControlType.TextBoxReadOnly)
                    {
                        if (!dicPluginSettingsToDisable.ContainsKey(plugin))
                            dicPluginSettingsToDisable.Add(plugin, new List<UIElement>());
                        dicPluginSettingsToDisable[plugin].Add(inputControl);
                    }

                    // only some elemetns need a "headline". If true element and headline have to be grouped in a stackpanel
                    StackPanel stackPanelInputControl = null;
                    if (bInfo.TaskPaneSettingsAttribute.ControlType != ControlType.Button && bInfo.TaskPaneSettingsAttribute.ControlType != ControlType.OpenFileDialog &&
                        bInfo.TaskPaneSettingsAttribute.ControlType != ControlType.SaveFileDialog && bInfo.TaskPaneSettingsAttribute.ControlType != ControlType.TextBoxReadOnly &&
                        bInfo.TaskPaneSettingsAttribute.ControlType != ControlType.CheckBox && bInfo.TaskPaneSettingsAttribute.Caption != "")
                    {
                        TextBox textBlock = new TextBox();
                        textBlock.Background = Brushes.Transparent;
                        textBlock.IsReadOnly = true;
                        textBlock.BorderThickness = new Thickness(0);
                        textBlock.Background = Brushes.Transparent;
                        textBlock.Text = bInfo.TaskPaneSettingsAttribute.Caption;
                        textBlock.ToolTip = plugin.GetPluginInfoAttribute().ToolTip;
                        textBlock.Height += textBlock.Height * 1.5;
                        stackPanelInputControl = new StackPanel();
                        stackPanelInputControl.Margin = new Thickness(0);

                        // this textBox maybe accessed later on TaskPaneSettingChanged event to set a new caption
                        bInfo.CaptionGUIElement = textBlock;

                        if (bInfo.SettingFormat != null)
                        {
                            textBlock.FontWeight = bInfo.SettingFormat.FontWeight;
                            textBlock.FontStyle = bInfo.SettingFormat.FontStyle;
                            textBlock.Foreground = bInfo.SettingFormat.ForeGround;
                            textBlock.Background = bInfo.SettingFormat.BackGround;
                            stackPanelInputControl.Background = bInfo.SettingFormat.BackGround;
                        }

                        // create a grid for vertical layout with definded column width
                        if (bInfo.SettingFormat != null && bInfo.SettingFormat.Orientation == Orientation.Horizontal)
                        {
                            Grid grid = new Grid();
                            grid.RowDefinitions.Add(new RowDefinition());
                            ColumnDefinition colDef1 = new ColumnDefinition();
                            grid.ColumnDefinitions.Add(colDef1);
                            ColumnDefinition colDef2 = new ColumnDefinition();
                            grid.ColumnDefinitions.Add(colDef2);
                            colDef1.Width = bInfo.SettingFormat.WidthCol1;
                            colDef2.Width = bInfo.SettingFormat.WidthCol2;
                            grid.Children.Add(textBlock);
                            grid.Children.Add(inputControl);
                            Grid.SetRow(textBlock, 0);
                            Grid.SetRow(inputControl, 0);
                            Grid.SetColumn(textBlock, 0);
                            Grid.SetColumn(inputControl, 1);

                            stackPanelInputControl.Children.Add(grid);
                        }
                        else
                        {
                            // Just put headline and item into vertical stack panel
                            stackPanelInputControl.Children.Add(textBlock);
                            stackPanelInputControl.Children.Add(inputControl);
                        }
                        inputControl = stackPanelInputControl;
                        stackPanelContent.Children.Add(stackPanelInputControl);
                    }

                    if (bInfo.SettingFormat != null)
                    {
                        if (bInfo.SettingFormat.Ident > 0)
                        {
                            StackPanel stackPanel = new StackPanel();
                            stackPanel.Margin = new Thickness(bInfo.SettingFormat.Ident * SETTINGS_FORMAT_INDENT_OFFSET, 0, 0, 0);
                            stackPanel.Children.Add(inputControl);
                            inputControl = stackPanel;
                        }
                        // check for vertical groups
                        if (bInfo.SettingFormat.HasVerticalGroup)
                        {
                            dicVerticalSubGroups.GetOrCreate(bInfo.SettingFormat.VerticalGroup).Add(new KeyValuePair<BindingInfo, UIElement>(bInfo, inputControl));
                        }
                    }

                    // check for groups
                    if (bInfo.TaskPaneSettingsAttribute.GroupName != null && bInfo.TaskPaneSettingsAttribute.GroupName != "")
                    {
                        if (!dicGroupedElements.ContainsKey(bInfo.TaskPaneSettingsAttribute.GroupName))
                            dicGroupedElements.Add(bInfo.TaskPaneSettingsAttribute.GroupName, new List<UIElement>());

                        dicGroupedElements[bInfo.TaskPaneSettingsAttribute.GroupName].Add(inputControl);
                    }
                    else
                    {
                        dicGroupedElements[emptyGroup].Add(inputControl);
                    }

                    bInfo.GUIElement = inputControl;

                    string key = bInfo.TaskPaneSettingsAttribute.PropertyName != null ? bInfo.TaskPaneSettingsAttribute.PropertyName : bInfo.TaskPaneSettingsAttribute.PropertyName;

                    dicAllPluginSettings.GetOrCreate(plugin.Settings).Add(
                      bInfo.TaskPaneSettingsAttribute.PropertyName,
                      new TaskPaneSettingsForPlugins(bInfo.TaskPaneSettingsAttribute.PropertyName, bInfo, Visibility.Visible));
                }

                #region vertical_sub_groups
                // first create the sub groups and remove items from main dictionary if sub group is selected
                foreach (KeyValuePair<string, List<KeyValuePair<BindingInfo, UIElement>>> kvpVerticalGrops in dicVerticalSubGroups)
                {
                    // TODO: place group name in vertical stack panel?

                    // create grid for vertical group
                    Grid grid = new Grid();
                    grid.RowDefinitions.Add(new RowDefinition());
                    // we place the group item on the position of the first vertical group item
                    int replaceIndex = -1;

                    foreach (KeyValuePair<BindingInfo, UIElement> kvp in kvpVerticalGrops.Value)
                    {
                        // remove from main items dic, because we create new group items
                        if (kvp.Key.TaskPaneSettingsAttribute.HasGroupName)
                        {
                            if (replaceIndex == -1)
                                replaceIndex = dicGroupedElements[kvp.Key.TaskPaneSettingsAttribute.GroupName].IndexOf(kvp.Value);
                            dicGroupedElements[kvp.Key.TaskPaneSettingsAttribute.GroupName].Remove(kvp.Value);
                        }
                        else
                        {
                            if (replaceIndex == -1)
                                replaceIndex = dicGroupedElements[emptyGroup].IndexOf(kvp.Value);
                            dicGroupedElements[emptyGroup].Remove(kvp.Value);
                        }
                        // create new column for current item and place it there
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                        grid.Children.Add(kvp.Value);
                        Grid.SetRow(kvp.Value, 0);
                        Grid.SetColumn(kvp.Value, grid.ColumnDefinitions.Count - 1);
                    }

                    // plugin developer has to make sure, that vertical sub group items belong to the main top group, so we just take to main group of the first element
                    // and place the grid with all items in that group
                    if (kvpVerticalGrops.Value[0].Key.TaskPaneSettingsAttribute.HasGroupName)
                    {
                        dicGroupedElements[kvpVerticalGrops.Value[0].Key.TaskPaneSettingsAttribute.GroupName].Insert(replaceIndex, grid);
                    }
                    else
                    {
                        dicGroupedElements[emptyGroup].Insert(replaceIndex, grid);
                    }
                }
                #endregion vertical_sub_groups

                #region main_groups
                // add all elements without groups. TODO: order is lost here            
                //stackPanelSettingsDefault.Margin = new Thickness(EXPANDER_GROUP_LEFT_RIGHT_OFFSET, 0, EXPANDER_GROUP_LEFT_RIGHT_OFFSET, 0);
                //foreach (UIElement ui in dicGroupedElements[emptyGroup])
                //{
                //    Rectangle r1 = new Rectangle();
                //    r1.Height = CONTROL_OFFSET;
                //    stackPanelSettingsDefault.Children.Add(r1);
                //    stackPanelSettingsDefault.Children.Add(ui);
                //}
                dicGroupedElements.Remove(emptyGroup);

                // now add groups with elements
                foreach (KeyValuePair<string, List<UIElement>> kvp in dicGroupedElements)
                {
                    Expander exp = new Expander();
                    exp.SetResourceReference(Expander.StyleProperty, new ComponentResourceKey(typeof(Panel), "Expander"));
                    exp.Header = kvp.Key;
                    StackPanel stackPanel = new StackPanel();
                    // some offset for the settings elements just for optical reasons
                    stackPanel.Margin = new Thickness(EXPANDER_GROUP_LEFT_RIGHT_OFFSET, 0, EXPANDER_GROUP_LEFT_RIGHT_OFFSET, 0);
                    exp.Content = stackPanel;
                    foreach (UIElement ui in kvp.Value)
                    {
                        Rectangle r1 = new Rectangle();
                        r1.Height = CONTROL_OFFSET;
                        stackPanel.Children.Add(r1);
                        stackPanel.Children.Add(ui);
                    }

                    Rectangle rect = new Rectangle();
                    rect.Height = CONTROL_OFFSET;
                    stackPanelContent.Children.Add(rect);
                    stackPanelContent.Children.Add(exp);
                }
                #endregion main_groups

                if (plugin.Settings.GetTaskPaneAttributeChanged() != null)
                {
                    plugin.Settings.GetTaskPaneAttributeChanged().AddEventHandler(plugin.Settings, new TaskPaneAttributeChangedHandler(TaskPaneAttributeChanged));
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        #region event_handler_methods
        private void TaskPaneAttributeChanged(ISettings settings, TaskPaneAttributeChangedEventArgs args)
        {
            try
            {
                // plugin was display already
                if (dicAllPluginSettings.ContainsKey(settings))
                {
                    foreach (TaskPaneAttribteContainer tpac in args.ListTaskPaneAttributeContainer)
                    {
                        if (dicAllPluginSettings[settings].ContainsKey(tpac.Property) && dicAllPluginSettings[settings][tpac.Property].BindingInfo != null)
                        {
                            dicAllPluginSettings[settings][tpac.Property].BindingInfo.GUIElement.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                dicAllPluginSettings[settings][tpac.Property].BindingInfo.GUIElement.Visibility = tpac.Visibility;

                                if (tpac.TaskPaneAttribute != null)
                                {
                                    setCaptionAndTooltip(dicAllPluginSettings[settings][tpac.Property].BindingInfo, tpac);
                                }
                            }, null);
                            dicAllPluginSettings[settings][tpac.Property].Visibility = tpac.Visibility;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
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
                        rbl.BInfo.Settings.GetType().GetProperty(rbl.BInfo.TaskPaneSettingsAttribute.PropertyName).SetValue(rbl.BInfo.Settings, rbl.List.IndexOf(radio), null);
                    }
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        // propagate changes of TextBoxHidden to ISettings instance
        private void TextBoxHidden_Changed(object sender, RoutedEventArgs e)
        {
            try
            {
                PasswordBox pwBox = sender as PasswordBox;
                if (pwBox != null)
                {
                    BindingInfo bInfo = pwBox.Tag as BindingInfo;
                    if (bInfo != null)
                    {
                        bInfo.Settings.GetType().GetProperty(bInfo.TaskPaneSettingsAttribute.PropertyName).SetValue(bInfo.Settings, pwBox.Password, null);
                    }
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        private void RadioButton_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (sender is ISettings)
                {
                    ISettings settings = sender as ISettings;
                    PropertyInfo pInfo = settings.GetType().GetProperty(e.PropertyName);
                    if (pInfo == null)
                    {
                        GuiLogMessage("The property \"" + e.PropertyName + "\" does not exist in your plugin settings.", NotificationLevel.Error);
                        return;
                    }
                    TaskPaneAttribute[] attributes = (TaskPaneAttribute[])pInfo.GetCustomAttributes(typeof(TaskPaneAttribute), false);
                    if (attributes.Length == 1 && attributes[0].ControlType == ControlType.RadioButton)
                    {
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            int value = (int)settings.GetType().GetProperty(e.PropertyName).GetValue(settings, null);

                            // Avoid event loops: if plugin develop does not check that if value is != current value and fires
                            // an event allways we have a loop. So we break this loop at this central position by reassigning
                            // the value only if not set already.
                            if (dicRadioButtons[settings][e.PropertyName][value].IsChecked == false || dicRadioButtons[settings][e.PropertyName][value].IsChecked == null)
                            {
                                foreach (RadioButton radio in dicRadioButtons[settings][e.PropertyName])
                                {
                                    radio.IsChecked = false;
                                }
                                dicRadioButtons[settings][e.PropertyName][value].IsChecked = true;
                            }
                        }, null);
                    }
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        private void infoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(((TextBlock)((ContextMenu)((MenuItem)sender).Parent).Tag).Text.Trim());
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

        private void hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                string navigateUri = ((Hyperlink)sender).NavigateUri.ToString();
                Process.Start(navigateUri);
                e.Handled = true;
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private void TaskPaneButton_Click(object sender, RoutedEventArgs e)
        {
            BindingInfo bInfo = (sender as Button).Tag as BindingInfo;
            if (bInfo != null && bInfo.Settings != null && bInfo.TaskPaneSettingsAttribute.Method != null)
            {
                bInfo.TaskPaneSettingsAttribute.Method.Invoke(bInfo.Settings, null);
            }
        }

        private void Control_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender is TextBox) SetHelpText((sender as TextBox).Tag as string);
                if (sender is CheckBox) SetHelpText((sender as CheckBox).Tag as string);
                if (sender is ComboBox) SetHelpText((sender as ComboBox).Tag as string);
                if (sender is Slider) SetHelpText((sender as Slider).Tag as string);
                if (sender is Button) SetHelpText((sender as Button).Tag as string);
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        private void Control_MouseLeave(object sender, MouseEventArgs e)
        {
            textBoxTooltip.Foreground = Brushes.Transparent;
        }

        private void SetHelpText(string text)
        {
            try
            {
                textBoxTooltip.Text = text;
                textBoxTooltip.Foreground = Brushes.Black;
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

        private void TaskPaneButton_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                SetHelpText(((sender as Button).Tag as BindingInfo).TaskPaneSettingsAttribute.ToolTip);
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

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, new GuiLogEventArgs(message, null, logLevel));
        }


        //private void buttonShowDescription_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (OnShowPluginDescription != null)
        //            OnShowPluginDescription(this);
        //    }
        //    catch (Exception ex)
        //    {
        //        GuiLogMessage(ex.Message, NotificationLevel.Error);
        //    }
        //}
        #endregion event_handler_methods

        #region privateMethods
        private void setCaptionAndTooltip(BindingInfo bInfo, TaskPaneAttribteContainer tpac)
        {
            try
            {

                ((FrameworkElement)bInfo.GUIElement).ToolTip = tpac.TaskPaneAttribute.ToolTip;
                switch (tpac.TaskPaneAttribute.ControlType)
                {
                    // all these elements have a TextBlock "headline", that shows the caption
                    case ControlType.TextBox:
                    case ControlType.NumericUpDown:
                    case ControlType.ComboBox:
                    case ControlType.RadioButton:
                    case ControlType.DynamicComboBox:
                    case ControlType.Slider:
                        ((TextBlock)bInfo.CaptionGUIElement).Text = tpac.TaskPaneAttribute.Caption;
                        break;
                    case ControlType.CheckBox:
                        ((CheckBox)bInfo.CaptionGUIElement).Content = tpac.TaskPaneAttribute.Caption;
                        break;
                    // no handling of these two necessary, because of hard coded name
                    // case ControlType.SaveFileDialog:
                    // case ControlType.OpenFileDialog:
                    case ControlType.Button:
                        ((Button)bInfo.CaptionGUIElement).Content = tpac.TaskPaneAttribute.Caption;
                        break;
                    // no change for TextBoxReadOnly needed because the property itself contains the text - makes no sense to set the value by using the attribute
                    // case ControlType.TextBoxReadOnly:
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }
        #endregion privateMethods
    }

    /// <summary>
    /// Used for enums in comboboxes
    /// </summary>
    public class EnumToIntConverter : IValueConverter
    {
        private static EnumToIntConverter instance;

        private EnumToIntConverter() { }

        // singleton
        public static EnumToIntConverter GetInstance()
        {
            if (instance == null)
                instance = new EnumToIntConverter();

            return instance;
        }

        // enum -> int
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value;
        }

        // int -> enum
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Enum.ToObject(targetType, value);
        }
    }

    # region helper_classes
    public class TaskPaneSettingsForPlugins
    {
        public string PropertyName;
        public BindingInfo BindingInfo;
        public Visibility Visibility;
        public TaskPaneSettingsForPlugins(string propertyName, BindingInfo bindingInfo, Visibility visibility)
        {
            if (propertyName == null || propertyName == string.Empty) throw new ArgumentException("propertyName has to be set");
            this.PropertyName = propertyName;
            this.BindingInfo = bindingInfo;
        }
    }

    public class BindingInfo
    {
        public readonly TaskPaneAttribute TaskPaneSettingsAttribute;

        public ISettings Settings;

        public SettingsFormatAttribute SettingFormat;

        public UIElement GUIElement;

        public UIElement CaptionGUIElement;

        public BindingInfo(TaskPaneAttribute taskPaneSettingsAttribute)
        {
            this.TaskPaneSettingsAttribute = taskPaneSettingsAttribute;
        }

        public BindingInfo(TaskPaneAttribute taskPaneSettingsAttribute, ISettings settings)
        {
            this.TaskPaneSettingsAttribute = taskPaneSettingsAttribute;
            this.Settings = settings;
        }

    }

    public class BindingInfoRibbon
    {
        public RibbonBarAttribute RibbonBarAttribute;
        public ISettings Settings;
        public string PropertyPath;

        public BindingInfoRibbon(RibbonBarAttribute ribbonBarAttribute, string propertyPath, ISettings settings)
        {
            this.RibbonBarAttribute = ribbonBarAttribute;
            this.PropertyPath = propertyPath;
            this.Settings = settings;
        }
    }

    public class BindingInfoComparer : IComparer<BindingInfo>
    {
        public int Compare(BindingInfo x, BindingInfo y)
        {
            if (x.TaskPaneSettingsAttribute.Order != y.TaskPaneSettingsAttribute.Order)
                return x.TaskPaneSettingsAttribute.Order.CompareTo(y.TaskPaneSettingsAttribute.Order);
            else
                return x.TaskPaneSettingsAttribute.Caption.CompareTo(y.TaskPaneSettingsAttribute.Caption);
        }
    }

    public class BindingInfoRibbonComparer : IComparer<BindingInfoRibbon>
    {
        public int Compare(BindingInfoRibbon x, BindingInfoRibbon y)
        {
            if (x.RibbonBarAttribute.Order != y.RibbonBarAttribute.Order)
                return x.RibbonBarAttribute.Order.CompareTo(y.RibbonBarAttribute.Order);
            else
                return x.RibbonBarAttribute.Caption.CompareTo(y.RibbonBarAttribute.Caption);
        }
    }

    public class RadioButtonListAndBindingInfo
    {
        public readonly List<RadioButton> List = null;
        public readonly BindingInfo BInfo = null;

        public RadioButtonListAndBindingInfo(List<RadioButton> list, BindingInfo bInfo)
        {
            if (list == null) throw new ArgumentException("list");
            if (bInfo == null) throw new ArgumentException("bInfo");
            this.List = list;
            this.BInfo = bInfo;
        }
    }

    public static class PasswordBoxAssistant
    {
        public static readonly DependencyProperty BoundPassword =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBoxAssistant), new FrameworkPropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static readonly DependencyProperty BindPassword = DependencyProperty.RegisterAttached(
            "BindPassword", typeof(bool), typeof(PasswordBoxAssistant), new PropertyMetadata(false, OnBindPasswordChanged));

        private static readonly DependencyProperty UpdatingPassword =
            DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool), typeof(PasswordBoxAssistant));

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox box = d as PasswordBox;

            // only handle this event when the property is attached to a PasswordBox  
            // and when the BindPassword attached property has been set to true  
            if (d == null || !GetBindPassword(d))
            {
                return;
            }

            // avoid recursive updating by ignoring the box's changed event  
            box.PasswordChanged -= HandlePasswordChanged;

            string newPassword = (string)e.NewValue;

            if (!GetUpdatingPassword(box))
            {
                box.Password = newPassword;
            }

            box.PasswordChanged += HandlePasswordChanged;
        }

        private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            // when the BindPassword attached property is set on a PasswordBox,  
            // start listening to its PasswordChanged event  

            PasswordBox box = dp as PasswordBox;

            if (box == null)
            {
                return;
            }

            bool wasBound = (bool)(e.OldValue);
            bool needToBind = (bool)(e.NewValue);

            if (wasBound)
            {
                box.PasswordChanged -= HandlePasswordChanged;
            }

            if (needToBind)
            {
                box.PasswordChanged += HandlePasswordChanged;
            }
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox box = sender as PasswordBox;

            // set a flag to indicate that we're updating the password  
            SetUpdatingPassword(box, true);
            // push the new password into the BoundPassword property  
            SetBoundPassword(box, box.Password);
            SetUpdatingPassword(box, false);
        }

        public static void SetBindPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(BindPassword, value);
        }

        public static bool GetBindPassword(DependencyObject dp)
        {
            return (bool)dp.GetValue(BindPassword);
        }

        public static string GetBoundPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(BoundPassword);
        }

        public static void SetBoundPassword(DependencyObject dp, string value)
        {
            dp.SetValue(BoundPassword, value);
        }

        private static bool GetUpdatingPassword(DependencyObject dp)
        {
            return (bool)dp.GetValue(UpdatingPassword);
        }

        private static void SetUpdatingPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(UpdatingPassword, value);
        }

    }

    public static class DictionaryHelper
    {
        /// <summary>
        /// Gets or create an Dic entry. 
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns>The value for the given key.</returns>
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : new()
        {
            TValue ret;
            if (!dictionary.TryGetValue(key, out ret))
            {
                ret = new TValue();
                dictionary[key] = ret;
            }
            return ret;
        }

    }


    public static class StringExtensions
    {
        public static bool IsValidEmailAddress(this string s)
        {
            Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return regex.IsMatch(s);
        }

        public static bool IsValidURL(this string s)
        {
            Regex regex = new Regex("(([a-zA-Z][0-9a-zA-Z+\\-\\.]*:)?/{0,2}[0-9a-zA-Z;/?:@&=+$\\.\\-_!~*'()%]+)?(#[0-9a-zA-Z;/?:@&=+$\\.\\-_!~*'()%]+)?");
            return regex.IsMatch(s);
        }
    }

    #endregion helper_classes
}