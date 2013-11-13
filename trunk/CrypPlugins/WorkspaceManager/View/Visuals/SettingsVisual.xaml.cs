
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

using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using System.Windows.Controls.Primitives;
using Cryptool.PluginBase.Editor;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using Microsoft.Windows.Controls;
using System.Windows.Media.Animation;
using System.Globalization;
using System.Collections.Specialized;
using System.Collections;
using Cryptool.PluginBase.Validation;
using Cryptool.PluginBase.Miscellaneous;



namespace WorkspaceManager.View.Visuals
{
    public partial class SettingsVisual : UserControl
    {
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private readonly Thickness CONTROL_DEFAULT_MARGIN = new Thickness(4, 0, 0, 0);
        private Dictionary<ISettings, Dictionary<string, List<RadioButton>>> dicRadioButtons = new Dictionary<ISettings, Dictionary<string, List<RadioButton>>>();
        private IPlugin plugin;
        private EntryGroup entgrou;
        private ComponentVisual bcv;
        private TabControl tbC;
        public String myConnectorName;
        public Boolean noSettings;
        private Boolean isSideBar;

        public SettingsVisual(IPlugin plugin, ComponentVisual bcv, Boolean isMaster, Boolean isSideBar)
        {
            bcv.Model.ConnectorPlugstateChanged += new EventHandler<Model.ConnectorPlugstateChangedEventArgs>(Model_ConnectorPlugstateChanged);
            this.Loaded += new RoutedEventHandler(BinSettingsVisual_Loaded);

            noSettings = false;
            this.isSideBar = isSideBar;
            this.Resources.Add("isSideBarResource", this.isSideBar);


            this.bcv = bcv;
            this.plugin = plugin;
            entgrou = new EntryGroup();
            this.entgrou = createContentSettings(plugin);



            if (entgrou.entryList.Count != 0)
            {
                ((WorkspaceManagerClass)bcv.Model.WorkspaceModel.MyEditor).executeEvent += new EventHandler(excuteEventHandler);

                if (plugin.Settings != null && plugin.Settings.GetTaskPaneAttributeChanged() != null)
                {
                    plugin.Settings.GetTaskPaneAttributeChanged().AddEventHandler(plugin.Settings, new TaskPaneAttributeChangedHandler(myTaskPaneAttributeChangedHandler));
                }

                InitializeComponent();

                if (isMaster)
                {
                    bcv.IControlCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(CollectionChangedHandler);

                    tbC = new TabControl();
                    tbC.Name = "TabControl";

                    tbC.Background = Brushes.Transparent;
                    tbC.BorderBrush = Brushes.Transparent;

                    DataTrigger dt = new DataTrigger();
                    dt.Value = 1;

                    Binding dataBinding = new Binding("Items.Count");
                    dataBinding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(TabControl), 1);
                    dt.Binding = dataBinding;

                    Setter sett = new Setter();
                    sett.Property = VisibilityProperty;
                    sett.Value = Visibility.Collapsed;
                    dt.Setters.Add(sett);

                    Style stu = new Style();
                    stu.TargetType = typeof(TabItem);
                    stu.Triggers.Add(dt);

                    tbC.ItemContainerStyle = stu;

                    myGrid.Children.Remove(MyScrollViewer);

                    myGrid.Children.Add(tbC);
                    TabItem tbI = new TabItem();
                    tbI.Header = bcv.Model.PluginType.Name;
                    tbI.Content = MyScrollViewer;

                    tbC.Items.Add(tbI);

                    myConnectorName = "None, I'm the master!";

                }

                else
                {
                    //MyScrollViewer.Margin = new Thickness(-5, -5, -5, -5);

                }

                drawList(this.entgrou);

            }

            else
            {
                InitializeComponent();
                TextBlock tb = new TextBlock();
                tb.Text = Properties.Resources.BinSettingsVisual_BinSettingsVisual_No_Settings_available_;
                MyScrollViewer.Content = tb;
                noSettings = true;
            }

        }

        void BinSettingsVisual_Loaded(object sender, RoutedEventArgs e)
        {
            this.InvalidateVisual();
        }

        private void CollectionChangedHandler(Object sender, NotifyCollectionChangedEventArgs args)
        {
            
            for (int i = 0; i < args.NewItems.Count; i++)
            {
                IControlMasterElement icm = args.NewItems[i] as IControlMasterElement;
                icm.PluginModelChanged += new EventHandler(icm_PluginModelChanged);

            }
        }

        void icm_PluginModelChanged(object sender, EventArgs e)
        {
            IControlMasterElement master = (IControlMasterElement)sender;
            if (master.PluginModel != null)
            {
                Boolean b = true;
                foreach (TabItem vtbI in tbC.Items)
                {
                    if (vtbI.Uid == master.ConnectorModel.PropertyName)
                    {

                        vtbI.Content = new SettingsVisual(master.PluginModel.Plugin, bcv, false, isSideBar);
                        vtbI.Header = master.PluginModel.GetName();
                        b = false;
                    }
                }

                if (b)
                {
                    TabItem tbI = new TabItem();
                    tbI.Uid = master.ConnectorModel.PropertyName;
                    tbI.Content = new SettingsVisual(master.PluginModel.Plugin, bcv, false, isSideBar);
                    tbI.Header = master.PluginModel.GetName();
                    tbC.Items.Add(tbI);
                }
            }
            else
            {
                TabItem tbI = null;
                foreach (TabItem vtbI in tbC.Items)
                {
                    if (vtbI.Uid == master.ConnectorModel.PropertyName)
                    {
                        tbI = vtbI;
                    }
                }
                if (tbI != null)
                    tbC.Items.Remove(tbI);
            }
        }

        private void myTaskPaneAttributeChangedHandler(Object sender, TaskPaneAttributeChangedEventArgs args)
        {

            plugin.Settings.GetTaskPaneAttributeChanged();



            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                foreach (List<ControlEntry> cel in entgrou.entryList)
                {
                    entgrou.gorupPanel[entgrou.entryList.IndexOf(cel)].Visibility = System.Windows.Visibility.Visible;
                    Boolean allinvisble = true;

                    foreach (ControlEntry ce in cel)
                    {

                        foreach (TaskPaneAttribteContainer tpac in args.ListTaskPaneAttributeContainer)
                        {
                            if (ce.tpa.PropertyName == tpac.Property)
                            {

                                
                                ce.element.Visibility = tpac.Visibility;
                                ce.caption.Visibility = tpac.Visibility;
                                


                            }
                            if (ce.element.Visibility == System.Windows.Visibility.Visible)
                            {
                                allinvisble = false;
                            }
                        }


                    }
                    if (allinvisble)
                    {
                        entgrou.gorupPanel[entgrou.entryList.IndexOf(cel)].Visibility = System.Windows.Visibility.Collapsed;

                    }
                }
            }, null);

        }

        private void excuteEventHandler(Object sender, EventArgs args)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {


                foreach (List<ControlEntry> cel in entgrou.entryList)
                {
                    foreach (ControlEntry ce in cel)
                    {
                        if (((WorkspaceManagerClass)bcv.Model.WorkspaceModel.MyEditor).isExecuting())
                        {
                            if (!ce.tpa.ChangeableWhileExecuting)
                            {
                                ce.element.IsEnabled = false;
                                if (ce.element is IntegerUpDown)
                                {
                                    IntegerUpDown nud = ce.element as IntegerUpDown;
                                    nud.Opacity = 0.80;
                                    nud.Foreground = Brushes.Gray;
                                }
                                if (ce.caption != null)
                                {
                                    TextBlock cap = ce.caption as TextBlock;
                                    cap.Opacity = 0.80;
                                    cap.Foreground = Brushes.Gray;
                                }
                            }
                        }
                        else
                        {
                            if (!ce.tpa.ChangeableWhileExecuting)
                            {
                                ce.element.IsEnabled = true;
                                if (ce.element is IntegerUpDown)
                                {
                                    IntegerUpDown nud = ce.element as IntegerUpDown;
                                    nud.Opacity = 1;
                                    nud.Foreground = Brushes.Black;
                                }
                                if (ce.caption != null)
                                {
                                    TextBlock cap = ce.caption as TextBlock;
                                    cap.Opacity = 1;
                                    cap.Foreground = Brushes.Black;
                                }
                            }
                        }
                    }
                }

            }, null);

        }


        List<String> groups = new List<String>();



        public static double getComboBoxMaxSize(ComboBox child)
        {
            double x = 0;
            ComboBox cb = child as ComboBox;
            for (int i = 0; i < cb.Items.Count; i++)
            {
                String s = cb.Items[i] as String;
                FormattedText ft = new FormattedText(s, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(cb.FontFamily, cb.FontStyle, cb.FontWeight, cb.FontStretch), cb.FontSize, Brushes.Black);
                ft.MaxLineCount = 1;
                if (x < ft.WidthIncludingTrailingWhitespace)
                {
                    x = ft.WidthIncludingTrailingWhitespace;
                }
            }

            return cb.Width = x + 28 ; // 28 pixel are an approximation of the rendersize of the dropdown button
        }


        
        private void drawList(EntryGroup entgrou)
        {
            foreach (List<ControlEntry> cel in entgrou.entryList)
            {
                ParameterPanel parameterPanel;
                ParameterPanel noVerticalGroupParameterPanel;

                Expander testexoander = new Expander();

                Expander noverticalgroupexpander = new Expander();

                noVerticalGroupParameterPanel = new ParameterPanel(isSideBar);

                Border noVerticalGroupBodi = new Border();

                noVerticalGroupBodi.Style = (Style)FindResource("border1");

                noVerticalGroupBodi.Child = noVerticalGroupParameterPanel;

                noverticalgroupexpander.Content = noVerticalGroupBodi;

                Border bodi = new Border();

                testexoander.IsExpanded = true;

                parameterPanel = new ParameterPanel(isSideBar);

                entgrou.gorupPanel.Add(testexoander);

                parameterPanel.Name = "border1";

                parameterPanel.Margin = new Thickness(2);

                Binding dataBinding = new Binding("ActualWidth");
                dataBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                dataBinding.Mode = BindingMode.OneWay;
                dataBinding.Source = parameterPanel;

                if (!string.IsNullOrEmpty(cel[0].tpa.groupName))
                {
                    testexoander.Header = cel[0].tpa.GroupName;
                }

                StackPanel contentPanel = new StackPanel();
                List<String> grouplist = new List<String>();
                List<Grid> gridlist = new List<Grid>();
                List<TextBlock> tebo = new List<TextBlock>();
                
                foreach (ControlEntry ce in cel)
                {
                    addToConnectorSettingsHide(ce);
                    TextBlock title = new TextBlock();
                    ce.caption = title;
                    

                    if (ce.sfa == null)
                    {
                        title.Text = ce.tpa.Caption;
                        title.TextWrapping = TextWrapping.Wrap;

                        if (ce.element is CheckBox || ce.element is Button)
                        {
                            Label l = new Label();
                            l.Height = 0;
                            parameterPanel.Children.Add(ce.element);
                            parameterPanel.Children.Add(l);
                        }

                        else
                        {
                            parameterPanel.Children.Add(title);
                            if (ce.element is ComboBox)
                            {
                                ComboBox cb = ce.element as ComboBox;
                                cb.MaxWidth = getComboBoxMaxSize(cb);
                                parameterPanel.Children.Add(cb);

                            }
                            else
                            {
                                parameterPanel.Children.Add(ce.element);
                            }
                        }
                    }
                    else
                    {
                        if (ce.sfa.VerticalGroup != null)
                        {
                            if (grouplist.Contains(ce.sfa.VerticalGroup))
                            {

                                Grid controlGrid = gridlist[grouplist.IndexOf(ce.sfa.VerticalGroup)];
                                
                                ColumnDefinition coldef1 = new ColumnDefinition();
                                coldef1.Width = ce.sfa.WidthCol1;
                                controlGrid.ColumnDefinitions.Add(coldef1);

                                ColumnDefinition coldef2 = new ColumnDefinition();
                                coldef2.Width = ce.sfa.WidthCol2;
                                controlGrid.ColumnDefinitions.Add(coldef2);
                                
                                title.Text = ce.tpa.Caption;
                                ce.caption = title;
                                title.HorizontalAlignment = HorizontalAlignment.Center;

                                Label space = new Label();
                                space.Width = 0;
                                Grid.SetColumn(title, controlGrid.ColumnDefinitions.Count - 2);

                                controlGrid.Children.Add(title);
                                Grid.SetColumn(ce.element, controlGrid.ColumnDefinitions.Count - 1);

                                if (ce.element is ComboBox)
                                {
                                    ComboBox cb = ce.element as ComboBox;
                                    cb.Width = getComboBoxMaxSize(cb);
                                    controlGrid.Children.Add(cb);
                                    controlGrid.MaxWidth += cb.Width;
                                    controlGrid.MaxWidth += title.DesiredSize.Width; ;
                                }
                                else
                                {
                                    controlGrid.Children.Add(ce.element);
                                }
                            }
                            else
                            {
                                grouplist.Add(ce.sfa.VerticalGroup);

                                Grid controlGrid = new Grid();
                                
                                ColumnDefinition coldef1 = new ColumnDefinition();
                                coldef1.Width = ce.sfa.WidthCol1;
                                controlGrid.ColumnDefinitions.Add(coldef1);

                                ColumnDefinition coldef2 = new ColumnDefinition();
                                coldef2.Width = ce.sfa.WidthCol2;
                                controlGrid.ColumnDefinitions.Add(coldef2);

                                title.Text = ce.tpa.Caption;
                                ce.caption = title;
                                title.HorizontalAlignment = HorizontalAlignment.Center;

                                Grid.SetColumn(title, 0);

                                controlGrid.Children.Add(title);
                                Grid.SetColumn(ce.element, 1);
                                if (ce.element is ComboBox)
                                {
                                    ComboBox cb = ce.element as ComboBox;
                                    cb.Width = getComboBoxMaxSize(cb);
                                    controlGrid.Children.Add(cb);
                                    controlGrid.MaxWidth += cb.Width;
                                    controlGrid.MaxWidth += title.DesiredSize.Width;
                                }
                                else
                                {
                                    controlGrid.Children.Add(ce.element);
                                }
                                controlGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

                                Label dummy = new Label();
                                dummy.Height = 0;

                                controlGrid.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                                controlGrid.Arrange(new Rect(controlGrid.DesiredSize));
                                parameterPanel.Children.Add(controlGrid);
                                parameterPanel.Children.Add(dummy);

                                controlGrid.Width = parameterPanel.Width;

                                gridlist.Add(controlGrid);
                            }
                        }
                        else
                        {
                            if (!parameterPanel.IsAncestorOf(noverticalgroupexpander))
                            {
                                Label l = new Label();
                                l.Width = 1;
                                l.Height = 0;
                                parameterPanel.Children.Add(noverticalgroupexpander);
                                parameterPanel.Children.Add(l);
                            }

                            
                            title.Text = ce.tpa.Caption;
                            ce.caption = title;
                            title.TextWrapping = TextWrapping.Wrap;

                            if (ce.element is CheckBox || ce.element is Button)
                            {
                                Label l = new Label();
                                l.Width = 1;
                                l.Height = 0;
                                noVerticalGroupParameterPanel.Children.Add(ce.element);
                                noVerticalGroupParameterPanel.Children.Add(l);

                            }
                            else if (ce.element is ComboBox)
                            {
                                ComboBox cb = ce.element as ComboBox;
                                noVerticalGroupParameterPanel.Children.Add(cb);
                            }
                            else
                            {
                                noVerticalGroupParameterPanel.Children.Add(title);
                                noVerticalGroupParameterPanel.Children.Add(ce.element);
                            }
                        }
                        
                    }

                }
                parameterPanel.HorizontalAlignment = HorizontalAlignment.Left;

                bodi.Child = parameterPanel;

                bodi.Style = (Style)FindResource("border1");

                testexoander.Content = bodi;
                
                if (isSideBar)
                    myStack.Children.Add(testexoander);
                else
                    myWrap.Children.Add(testexoander);

                parameterPanel.setMaxSizes(true);
                noVerticalGroupParameterPanel.setMaxSizes(true);
            }

            this.BeginInit();

        }



        private EntryGroup createContentSettings(IPlugin plugin)
        {

            EntryGroup entgrou = new EntryGroup();

            
            foreach (TaskPaneAttribute tpa in plugin.Settings.GetSettingsProperties(plugin))
            {
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
                
                bool b = (bcv.Model.GetOutputConnectors().Union(bcv.Model.GetInputConnectors())).Any(x => tpa.PropertyName == x.GetName());
                    switch (tpa.ControlType)
                    {
                        #region TextBox
                        case ControlType.TextBox:

                            TextBox textbox = new TextBox();

                            textbox.Tag = tpa.ToolTip;
                            textbox.ToolTip = tpa.ToolTip;
                            textbox.MouseEnter += Control_MouseEnter;
                            
                            
                            if (tpa.RegularExpression != null && tpa.RegularExpression != string.Empty)
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
                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(textbox, tpa, sfa, b, bcv.Model));
                            break;

                        #endregion TextBox

                        # region NumericUpDown
                        case ControlType.NumericUpDown:
                            if (tpa.ValidationType == ValidationType.RangeInteger)
                            {
                                IntegerUpDown intInput = new IntegerUpDown();
                                
                                intInput.SelectAllOnGotFocus = true;
                                intInput.Tag = tpa.ToolTip;
                                intInput.ToolTip = tpa.ToolTip;
                                intInput.MouseEnter += Control_MouseEnter;
                                intInput.Maximum = tpa.IntegerMaxValue;
                                intInput.Minimum = tpa.IntegerMinValue;

                                String s = tpa.IntegerMaxValue+"";
                                FormattedText ft = new FormattedText(s, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(intInput.FontFamily, intInput.FontStyle, intInput.FontWeight, intInput.FontStretch), intInput.FontSize, Brushes.Black);
                                intInput.MaxWidth = ft.WidthIncludingTrailingWhitespace + 30;
                                intInput.Width = ft.WidthIncludingTrailingWhitespace + 30;
                                intInput.SetBinding(IntegerUpDown.ValueProperty, dataBinding);
                                entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(intInput, tpa, sfa, b, bcv.Model));
                                intInput.IsEnabled = true;
                                
                              
                            }
                            else if (tpa.ValidationType == ValidationType.RangeDouble)
                            {
                                throw new InvalidOperationException("Double types for NumericUpDown are currently not supported");
                                /*NumericUpDown doubleInput = new NumericUpDown();
                                doubleInput.ValueType = typeof(double);
                                doubleInput.SelectAllOnGotFocus = true;
                                doubleInput.Tag = tpa.ToolTip;
                                doubleInput.ToolTip = tpa.ToolTip;
                                doubleInput.MouseEnter += Control_MouseEnter;
                                doubleInput.Maximum = tpa.DoubleMaxValue;
                                doubleInput.Minimum = tpa.DoubleMinValue;
                                doubleInput.SetBinding(NumericUpDown.ValueProperty, dataBinding);
                                entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(doubleInput, tpa, sfa));
                                doubleInput.Background = Brushes.Black;*/
                            }
                            break;
                        # endregion NumericUpDown

                        # region ComboBox
                        case ControlType.ComboBox:
                            ComboBox comboBox = new ComboBox();
   
                            comboBox.Tag = tpa.ToolTip;
                            comboBox.MouseEnter += Control_MouseEnter;

                            object value = plugin.Settings.GetType().GetProperty(tpa.PropertyName).GetValue(plugin.Settings, null);
                            bool isEnum = value is Enum;

                            if (isEnum) // use generic enum<->int converter
                                dataBinding.Converter = EnumToIntConverter.GetInstance();
                            
                            

                            if (tpa.ControlValues != null) // show manually passed entries in ComboBox
                                comboBox.ItemsSource = tpa.ControlValues;
                             else if (isEnum) // show automatically derived enum entries in ComboBox
                               comboBox.ItemsSource = Enum.GetValues(value.GetType());
                            else // nothing to show
                                GuiLogMessage("No ComboBox entries given", NotificationLevel.Error);
                            comboBox.ToolTip = tpa.ToolTip;
                            comboBox.SetBinding(ComboBox.SelectedIndexProperty, dataBinding);
                            //controlList.Add(new ControlEntry(comboBox, tpa, sfa));
                            entgrou.AddNewEntry(tpa.GroupName,new ControlEntry(comboBox, tpa, sfa, b, bcv.Model));
                            break;

                        # endregion ComboBox

                        # region RadioButton
                        
                        case ControlType.RadioButton:
                            if (!dicRadioButtons.ContainsKey(plugin.Settings))
                            {
                                dicRadioButtons.Add(plugin.Settings, new Dictionary<string, List<RadioButton>>());
                            }
                            List<RadioButton> list = new List<RadioButton>();
                            StackPanel panelRadioButtons = new StackPanel();

                            panelRadioButtons.ToolTip = tpa.ToolTip;
                            panelRadioButtons.MouseEnter += Control_MouseEnter;
                            panelRadioButtons.Margin = CONTROL_DEFAULT_MARGIN;

                            string groupNameExtension = Guid.NewGuid().ToString();
                            
                            for (int i = 0; i < tpa.ControlValues.Length; i++)
                                {
                                    RadioButton radio = new RadioButton();
                                    radio.IsChecked = false;
                                    
                                    string stringValue = tpa.ControlValues[i];

                                    Binding dataBinding1 = new Binding(plugin.Settings.GetType().GetProperty(tpa.PropertyName).Name);
                                    dataBinding1.Converter = new RadioBoolToIntConverter();
                                    dataBinding1.Mode = BindingMode.TwoWay;
                                    dataBinding1.Source = plugin.Settings;
                                    dataBinding1.ConverterParameter = (int)i;

                                    radio.GroupName = tpa.PropertyName + groupNameExtension;
                                    radio.Content = stringValue;
                                    
                                    radio.Tag = new RadioButtonListAndBindingInfo(list, plugin, tpa);
                                    radio.ToolTip = tpa.ToolTip;
                                    radio.SetBinding(RadioButton.IsCheckedProperty, dataBinding1);
                                    panelRadioButtons.Children.Add(radio);
                                    list.Add(radio);
                                }
                                dicRadioButtons[plugin.Settings].Add(tpa.PropertyName, list);
                                entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(panelRadioButtons, tpa, sfa, b, bcv.Model));
                           
                            break;
                            
                        #endregion RadioButton

                        # region CheckBox
                        case ControlType.CheckBox:
                            CheckBox checkBox = new CheckBox();
                            
                            checkBox.Margin = CONTROL_DEFAULT_MARGIN;
                            TextBlock wrapBlock = new TextBlock();
                            wrapBlock.Text = tpa.Caption;
                            wrapBlock.TextWrapping = TextWrapping.Wrap;
                            checkBox.Content = wrapBlock;
                            checkBox.Tag = tpa.ToolTip;
                            checkBox.ToolTip = tpa.ToolTip;
                            checkBox.MouseEnter += Control_MouseEnter;
                            checkBox.SetBinding(CheckBox.IsCheckedProperty, dataBinding);
                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(checkBox, tpa, sfa, b, bcv.Model));
                            
                            break;
                        # endregion CheckBox
                            
                        # region DynamicComboBox
                        case ControlType.DynamicComboBox:
                            PropertyInfo pInfo = plugin.Settings.GetType().GetProperty(tpa.ControlValuesNotInterpolated[0]);
                                                    
                            ObservableCollection<string> coll = pInfo.GetValue(plugin.Settings, null) as ObservableCollection<string>;
                                
                            if (coll != null)
                            {
                                ComboBox comboBoxDyn = new ComboBox();
                             
                                comboBoxDyn.Tag = tpa.ToolTip;
                                comboBoxDyn.ToolTip = tpa.ToolTip;
                                comboBoxDyn.MouseEnter += Control_MouseEnter;
                                comboBoxDyn.ItemsSource = coll;
                                comboBoxDyn.SetBinding(ComboBox.SelectedIndexProperty, dataBinding);
                                //inputControl = comboBoxDyn;
                                //bInfo.CaptionGUIElement = comboBoxDyn;

                                //controlList.Add(new ControlEntry(comboBoxDyn, tpa, sfa));
                                entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(comboBoxDyn, tpa, sfa, b, bcv.Model));
                            }
                            break;
                            # endregion DynamicComboBox

                        # region FileDialog
                        case ControlType.SaveFileDialog:
                        case ControlType.OpenFileDialog:
                            StackPanel sp = new StackPanel();
                           
                            sp.Uid = "FileDialog";
                            sp.Orientation = Orientation.Vertical;

                            TextBox fileTextBox = new TextBox();
                            fileTextBox.TextWrapping = TextWrapping.Wrap;
                            fileTextBox.Background = Brushes.LightGray;
                            fileTextBox.IsReadOnly = true;
                            fileTextBox.Margin = new Thickness(0, 0, 0, 5);
                            fileTextBox.TextChanged += fileDialogTextBox_TextChanged;
                            fileTextBox.SetBinding(TextBox.TextProperty, dataBinding);
                            //fileTextBox.SetBinding(TextBox.ToolTipProperty, dataBinding);
                            
                            fileTextBox.Tag = tpa;
                            if (fileTextBox.ToolTip == null || fileTextBox.ToolTip == string.Empty)
                            {
                                fileTextBox.ToolTip = tpa.ToolTip;
                            }
                            fileTextBox.MouseEnter += fileTextBox_MouseEnter;
                            sp.Children.Add(fileTextBox);

                            Button btn = new Button();
                            
                            btn.Tag = fileTextBox;
                            if (tpa.ControlType == ControlType.SaveFileDialog)
                                btn.Content = Properties.Resources.Save_File;
                            else
                                btn.Content = Properties.Resources.Open_File;
                            btn.Click += FileDialogClick;
                            sp.Children.Add(btn);
                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(sp, tpa, sfa, b, bcv.Model));
                           
                            break;
                        # endregion FileDialog

                        # region Button
                        case ControlType.Button:
                            Button taskPaneButton = new Button();
                           
                            taskPaneButton.Margin = new Thickness(0);
                            taskPaneButton.Tag = tpa;
                            taskPaneButton.ToolTip = tpa.ToolTip;
                            taskPaneButton.MouseEnter += TaskPaneButton_MouseEnter;
                            TextBlock contentBlock = new TextBlock();
                            contentBlock.Text = tpa.Caption;
                            contentBlock.TextWrapping = TextWrapping.Wrap;
                            contentBlock.TextAlignment = TextAlignment.Center;
                            taskPaneButton.Content = contentBlock;
                            taskPaneButton.Click += TaskPaneButton_Click;
                            
                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(taskPaneButton, tpa, sfa, b, bcv.Model));
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
                            slider.ToolTip = tpa.ToolTip;
                            slider.MouseEnter += Control_MouseEnter;
                            

                            slider.SetBinding(Slider.ValueProperty, dataBinding);
                            
                            slider.MinWidth = 0;

                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(slider, tpa, sfa, b, bcv.Model));
                            break;
                        # endregion Slider

                        # region TextBoxReadOnly
                        case ControlType.TextBoxReadOnly:
                            TextBox textBoxReadOnly = new TextBox();
                           
                            textBoxReadOnly.MinWidth = 0;
                            textBoxReadOnly.TextWrapping = TextWrapping.Wrap;
                            textBoxReadOnly.IsReadOnly = true;
                            textBoxReadOnly.BorderThickness = new Thickness(0);
                            textBoxReadOnly.Background = Brushes.Transparent;
                            textBoxReadOnly.Tag = tpa.ToolTip;
                            textBoxReadOnly.ToolTip = tpa.ToolTip;
                            textBoxReadOnly.MouseEnter += Control_MouseEnter;
                            dataBinding.Mode = BindingMode.OneWay; // read-only strings do not need a setter
                            textBoxReadOnly.SetBinding(TextBox.TextProperty, dataBinding);
                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(textBoxReadOnly, tpa, sfa, b, bcv.Model));
                            break;
                        # endregion TextBoxReadOnly
                
                        #region TextBoxHidden
                            case ControlType.TextBoxHidden:
                            PasswordBox passwordBox = new PasswordBox();
                            
                            passwordBox.MinWidth = 0; 
                            
                            passwordBox.Tag = tpa;
                            passwordBox.ToolTip = tpa.ToolTip;
                            passwordBox.MouseEnter += Control_MouseEnter;
                            passwordBox.Password = plugin.Settings.GetType().GetProperty(tpa.PropertyName).GetValue(plugin.Settings, null) as string;
                            //textBoxReadOnly.SetBinding(PasswordBox.property , dataBinding);
                            passwordBox.PasswordChanged += TextBoxHidden_Changed;
                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(passwordBox, tpa, sfa, b, bcv.Model));
                        break;
                        #endregion TextBoxHidden

                        #region KeyTextBox
                            case ControlType.KeyTextBox:
                            var keyTextBox = new KeyTextBox.KeyTextBox();
                            
                            var keyManager = plugin.Settings.GetType().GetProperty(tpa.AdditionalPropertyName).GetValue(plugin.Settings, null) as KeyTextBox.IKeyManager;
                            keyTextBox.KeyManager = keyManager;
                            keyTextBox.Tag = tpa;
                            keyTextBox.ToolTip = tpa.ToolTip;
                            keyTextBox.MouseEnter += Control_MouseEnter;
                            keyTextBox.SetBinding(KeyTextBox.KeyTextBox.CurrentKeyProperty, dataBinding);
                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(keyTextBox, tpa, sfa, b, bcv.Model));
                        break;
                        #endregion KeyTextBox

                    }
    
            }
            entgrou.sort();

            return entgrou;

        }

        void checkIfPlugged(Model.PlugState state, Model.ConnectorModel model)
        {
            ControlEntry ele = null;
            connectorSettingElements.TryGetValue(model.GetName(), out ele);
            if (state == Model.PlugState.Plugged)
            {
                if (ele != null)
                {
                    ele.Visibility = System.Windows.Visibility.Collapsed;
                    
                    foreach (Expander expander in myStack.Children)
                    {
                        ((expander.Content as Border).Child as ParameterPanel).setMaxSizes(false);   
                    }
                    foreach (Expander expander in myWrap.Children)
                    {
                        ((expander.Content as Border).Child as ParameterPanel).setMaxSizes(false);
                    }
                }
            }
            if (state == Model.PlugState.Unplugged && model.GetInputConnections().Count == 0)
            {
                if (ele != null)
                {
                    ele.Visibility = System.Windows.Visibility.Visible;

                    foreach (Expander expander in myStack.Children)
                    {
                        ((expander.Content as Border).Child as ParameterPanel).setMaxSizes(true);
                    }
                    foreach (Expander expander in myWrap.Children)
                    {
                        ((expander.Content as Border).Child as ParameterPanel).setMaxSizes(true);
                    }
                }
            }
        }

        void Model_ConnectorPlugstateChanged(object sender, Model.ConnectorPlugstateChangedEventArgs e)
        {
            checkIfPlugged(e.PlugState, e.ConnectorModel);
        }

        void addToConnectorSettingsHide(ControlEntry element)
        {
            if (element.hide)
            {
                connectorSettingElements.Add(element.tpa.PropertyName, element);
            }
        }


        Dictionary<string, ControlEntry> connectorSettingElements = new Dictionary<string, ControlEntry>();
        private void TextBoxHidden_Changed(object sender, RoutedEventArgs e)
        {
            try
            {
                PasswordBox pwBox = sender as PasswordBox;
                if (pwBox != null)
                {
                    TaskPaneAttribute tpa = pwBox.Tag as TaskPaneAttribute;
                    if (tpa != null)
                    {
                        plugin.Settings.GetType().GetProperty(tpa.PropertyName).SetValue(plugin.Settings, pwBox.Password, null);
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
                if (sender is IntegerUpDown) SetHelpText((sender as IntegerUpDown).Tag as string);
                if (sender is TextBox) SetHelpText((sender as TextBox).Tag as string);
                //if (sender is PasswordBox) SetHelpText(((BindingInfo)(sender as PasswordBox).Tag).TaskPaneSettingsAttribute.ToolTip as string);
                if (sender is CheckBox) SetHelpText((sender as CheckBox).Tag as string);
                if (sender is ComboBox) SetHelpText((sender as ComboBox).Tag as string);
                if (sender is Slider) SetHelpText((sender as Slider).Tag as string);
                if (sender is Button) SetHelpText((sender as Button).Tag as string);
                if (sender is KeyTextBox.KeyTextBox) SetHelpText((sender as KeyTextBox.KeyTextBox).Tag as string);
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
        public List<Expander> gorupPanel = new List<Expander>();


        public void AddNewEntry(String groupname, ControlEntry entry)
        {

            if (string.IsNullOrEmpty(groupname))
            { groupname = null; }

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
        public void sort()
        {
            foreach (List<ControlEntry> dummyList in entryList)
                dummyList.Sort(new BindingInfoComparer());
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
        public bool hide;

        private FrameworkElement captionx;
        public FrameworkElement caption
        {
            get 
            { 
                return captionx;
            }
            set 
            { 
                captionx = value;
                captionx.Visibility = element.Visibility;
            }
        }

        private Visibility visibility;
        public Visibility Visibility
        {
            get 
            { 
                return visibility;
            }
            set 
            { 
                visibility = value;
                if(caption != null)
                    caption.Visibility = value;
                element.Visibility = value;
            }
        }

        public ControlEntry(UIElement element, TaskPaneAttribute tpa, SettingsFormatAttribute sfa)
        {
            this.element = element;
            this.sfa = sfa;
            this.tpa = tpa;
            
        }

        public ControlEntry(UIElement element, TaskPaneAttribute tpa, SettingsFormatAttribute sfa, bool hide, Model.PluginModel model)
        {
            this.element = element;
            this.sfa = sfa;
            this.tpa = tpa;
            this.hide = hide;
            if (hide)
            {
                var conModel = model.GetOutputConnectors().Union(model.GetInputConnectors()).First(x => x.GetName() == tpa.PropertyName);
                var a = conModel.GetInputConnections().Count > 0;
                var b = conModel.GetOutputConnections().Count > 0;
                this.Visibility = a || b ? Visibility.Collapsed : Visibility.Visible;
            }

        }

    }

    public class ParameterPanel : Panel
    {
        Boolean isSideBar;

        double maxSize = 0;
        double maxSizeContent = 0;
        double maxSizeCaption = 0;
        double maxSizeCB = 0;

        Grid maxGrid = new Grid();

        public ParameterPanel(Boolean isSideBar)
        {
            this.isSideBar = isSideBar;
            SizeChanged += new SizeChangedEventHandler(TestPanel_SizeChanged);
        }

        public void setMaxSizes(Boolean overRun)
        {
            maxSizeCaption = 0;
            maxSizeContent = 0;
            maxSizeCB = 0;
            maxSize = 0;
            foreach (UIElement child in Children)
            {

                child.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                child.Arrange(new Rect(child.DesiredSize));

                if (child is TextBlock)
                {
                    TextBlock caption = child as TextBlock;
                    if (child.Visibility == Visibility.Visible || overRun)
                    {
                        if (caption != null)
                        {
                            FormattedText formattedText = new FormattedText(
                                "1234567890123456789012345678901234567890",
                                CultureInfo.GetCultureInfo("en-us"),
                                FlowDirection.LeftToRight,
                                new Typeface(caption.FontFamily.ToString()),
                                caption.FontSize,
                                Brushes.Black);

                            if (formattedText.WidthIncludingTrailingWhitespace > maxSizeCaption)
                            {
                                maxSizeCaption = formattedText.WidthIncludingTrailingWhitespace + 10;
                            }
                        }
                    }
                }
                else if (child is KeyTextBox.KeyTextBox)
                {
                    var keyTextBlock = child as KeyTextBox.KeyTextBox;
                    if (child.Visibility == Visibility.Visible || overRun)
                    {
                        if (keyTextBlock != null)
                        {
                            FormattedText formattedText = new FormattedText(
                                "1234567890123456789012345678901234567890",
                                CultureInfo.GetCultureInfo("en-us"),
                                FlowDirection.LeftToRight,
                                new Typeface(keyTextBlock.FontFamily.ToString()),
                                keyTextBlock.FontSize,
                                Brushes.Black);

                            if (formattedText.WidthIncludingTrailingWhitespace > maxSizeCaption)
                            {
                                maxSizeCaption = formattedText.WidthIncludingTrailingWhitespace + 10;
                            }
                        }
                    }
                }
                else if (child is Grid)
                {

                    if (maxGrid.Width < (child as Grid).DesiredSize.Width)
                    {
                        maxGrid = child as Grid;

                    }

                }
                else if (child is CheckBox)
                {
                    if (child.DesiredSize.Width > maxSizeCB)
                    {
                        if (child.DesiredSize.Width != 0)
                            maxSizeCB = child.DesiredSize.Width;
                    }

                }
                else if (child is ComboBox)
                {
                    double comboSize = SettingsVisual.getComboBoxMaxSize(child as ComboBox);
                    if (comboSize > maxSizeContent)
                    {
                        if (comboSize != 0)
                            maxSizeContent = comboSize;
                    }
                }

                else if (child is IntegerUpDown)
                {
                    IntegerUpDown intUD = child as IntegerUpDown;
                    String s = intUD.Maximum + "";
                    int intInput = 0;
                    FormattedText ft = new FormattedText(s, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(intUD.FontFamily, intUD.FontStyle, intUD.FontWeight, intUD.FontStretch), intUD.FontSize, Brushes.Black);
                        
                    if (ft.WidthIncludingTrailingWhitespace > maxSizeContent)
                    {
                        if (ft.WidthIncludingTrailingWhitespace != 0)
                            maxSizeContent = ft.WidthIncludingTrailingWhitespace;
                    }
                }
                else
                {
                    child.Measure(new Size(10, 10));
                    if (child.DesiredSize.Width > maxSizeContent)
                    {
                            
                        if (child.DesiredSize.Width != 0)
                            maxSizeContent = child.DesiredSize.Width;
                    }
                }                
            }

            
            if (maxSizeContent < 20)
            {
                maxSizeContent = 100;
            }

            

            maxSize = maxSizeCaption + maxSizeContent;

            if (maxSizeCB > maxSize)
            {
                maxSize = maxSizeCB;
                maxSizeContent = maxSizeCB - maxSizeCaption - 1;
            }

            
            if (maxSize < maxGrid.DesiredSize.Width)
            {
                maxSize = maxGrid.DesiredSize.Width;
            }
            if (!isSideBar)
            {
                this.MaxWidth = maxSize + 10;
            }


            foreach (UIElement child in Children)
            {
                if (child is IntegerUpDown)
                {
                    IntegerUpDown dummyTextBox = child as IntegerUpDown;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                }

                if (child is ComboBox)
                {
                    ComboBox dummyTextBox = child as ComboBox;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                }

                if (child is TextBox)
                {
                    TextBox dummyTextBox = child as TextBox;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    dummyTextBox.MaxWidth = maxSizeContent;

                }

                if (child is PasswordBox)
                {
                    PasswordBox dummyTextBox = child as PasswordBox;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    dummyTextBox.MaxWidth = maxSizeContent;

                }

                if (child is KeyTextBox.KeyTextBox)
                {
                    var dummyKeyTextBox = child as KeyTextBox.KeyTextBox;
                    dummyKeyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyKeyTextBox.Arrange(new Rect(dummyKeyTextBox.DesiredSize));
                    dummyKeyTextBox.MaxWidth = maxSizeContent;

                }

                if (child is TextBlock)
                {
                    TextBlock dummyTextBox = child as TextBlock;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    dummyTextBox.MaxWidth = maxSizeCaption;

                }

                if (child is Button)
                {
                    Button dummyTextBox = child as Button;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    dummyTextBox.MaxWidth = maxSize;

                }

                if (child is CheckBox)
                {
                    CheckBox dummyTextBox = child as CheckBox;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    dummyTextBox.MaxWidth = maxSize;

                }

                if (child is StackPanel)
                {
                    StackPanel dummyTextBox = child as StackPanel;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    if (dummyTextBox.Uid == "FileDialog")
                        dummyTextBox.MaxWidth = maxSize;
                    else
                        dummyTextBox.MaxWidth = dummyTextBox.DesiredSize.Width;

                }

                if (child is Slider)
                {
                    Slider dummyTextBox = child as Slider;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    dummyTextBox.MaxWidth = maxSizeContent;

                }

                if (child is Expander)
                {
                    
                }
            }



        }

        private void TestPanel_SizeChanged(Object sender, SizeChangedEventArgs args)
        {
            foreach (UIElement child in Children)
            {
                child.Measure(new Size(this.ActualWidth, double.PositiveInfinity));

                if (child is CheckBox)
                {
                    CheckBox dummyTextBox = child as CheckBox;

                    dummyTextBox.MinWidth = 0;

                    dummyTextBox.Width = this.ActualWidth;
                }

                if (child is Slider)
                {
                    Slider dummyTextBox = child as Slider;

                    dummyTextBox.MinWidth = 0;

                    if (this.ActualWidth < maxSizeCaption + maxSizeContent)
                    {
                        dummyTextBox.MaxWidth = Double.MaxValue;
                    }
                    else
                    {
                        dummyTextBox.MaxWidth = maxSizeContent;
                    }

                    dummyTextBox.Width = this.ActualWidth;
                }

                if (child is StackPanel)
                {
                    StackPanel dummyTextBox = child as StackPanel;

                    dummyTextBox.MinWidth = 0;

                    dummyTextBox.Width = this.ActualWidth;
                }

                if (child is IntegerUpDown)
                {
                    IntegerUpDown dummyTextBox = child as IntegerUpDown;

                    dummyTextBox.MinWidth = 0;
                    if (this.ActualWidth < maxSizeCaption + maxSizeContent)
                    {
                        dummyTextBox.MaxWidth = Double.MaxValue;
                    }
                    else
                    {
                        dummyTextBox.MaxWidth = this.ActualWidth - maxSizeCaption;
                    }
                    dummyTextBox.Width = this.ActualWidth;
                }

                if (child is Button)
                {
                    Button dummyTextBox = child as Button;

                    dummyTextBox.MinWidth = 0;

                    dummyTextBox.Width = this.ActualWidth;
                }

                if (child is TextBlock)
                {

                    TextBlock dummyTextBox = child as TextBlock;

                    dummyTextBox.MinWidth = 0;


                    dummyTextBox.Width = this.ActualWidth;
                }

                if (child is TextBox)
                {

                    TextBox dummyTextBox = child as TextBox;
                    dummyTextBox.MinWidth = 0;
                    if (this.ActualWidth < maxSizeCaption + maxSizeContent)
                    {
                        dummyTextBox.MaxWidth = Double.MaxValue;
                    }
                    else
                    {
                        dummyTextBox.MaxWidth = this.ActualWidth - maxSizeCaption;
                    }

                    dummyTextBox.Width = this.ActualWidth;
                }

                if (child is PasswordBox)
                {

                    PasswordBox dummyTextBox = child as PasswordBox;
                    dummyTextBox.MinWidth = 0;

                    if (this.ActualWidth < maxSizeCaption + maxSizeContent)
                    {
                        dummyTextBox.MaxWidth = Double.MaxValue;
                    }
                    else
                    {
                        dummyTextBox.MaxWidth = this.ActualWidth - maxSizeCaption;
                    }

                    dummyTextBox.Width = this.ActualWidth;
                }

                if (child is Grid)
                {

                    Grid dummyTextBox = child as Grid;
                    dummyTextBox.MinWidth = 0;

                    dummyTextBox.Width = this.ActualWidth;
                }

                if (child is KeyTextBox.KeyTextBox)
                {

                    var dummykeyTextBox = child as KeyTextBox.KeyTextBox;
                    dummykeyTextBox.MinWidth = 0;
                    if (this.ActualWidth < maxSizeCaption + maxSizeContent)
                    {
                        dummykeyTextBox.MaxWidth = Double.MaxValue;
                    }
                    else
                    {
                        dummykeyTextBox.MaxWidth = this.ActualWidth - maxSizeCaption;
                    }

                    dummykeyTextBox.Width = this.ActualWidth;
                }

                if (child is Expander)
                {

                    Expander dummyTextBox = child as Expander;
                    dummyTextBox.Width = this.ActualWidth;
                }

                if (child is ComboBox)
                {

                    ComboBox dummyTextBox = child as ComboBox;
                    dummyTextBox.MinWidth = 0;
                    if (this.ActualWidth < maxSizeCaption + maxSizeContent)
                    {
                        dummyTextBox.MaxWidth = Double.MaxValue;
                    }
                    else
                    {
                        dummyTextBox.MaxWidth = this.ActualWidth - maxSizeCaption;
                    }
                    dummyTextBox.Width = this.ActualWidth;
                }



            }
        }


        private TimeSpan _AnimationLength = TimeSpan.FromMilliseconds(200);

        protected override Size MeasureOverride(Size availableSize)
        {
            Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            double curX = 0, curY = 0, curLineHeight = 0;


            bool b = !(availableSize.Width > maxSize);

            foreach (UIElement child in Children)
            {

                child.Measure(infiniteSize);
                if (child is CheckBox)
                {
                    //  b = true;
                }

                if (Children.IndexOf(child) % 2 == 0 || curX + child.DesiredSize.Width > availableSize.Width /*|| curX + child.DesiredSize.Width > maxSize*/ || b)
                { //Wrap to next line

                    curY += curLineHeight + 2;
                    curX = 0;
                    curLineHeight = 0;
                }

                curX += maxSize;
                if (child.DesiredSize.Height > curLineHeight)
                    curLineHeight = child.DesiredSize.Height;
            }



            curY += curLineHeight;
            curY += 0;

            Size resultSize = new Size();
            resultSize.Width = double.IsPositiveInfinity(availableSize.Width) ? curX : availableSize.Width;
            resultSize.Height = double.IsPositiveInfinity(availableSize.Height) ? curY : availableSize.Height;
            this.Height = resultSize.Height;


            return resultSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this.Children == null || this.Children.Count == 0)
                return finalSize;

            TranslateTransform trans = null;
            double curX = 0, curY = 0, curLineHeight = 0;

            bool b = !(finalSize.Width > maxSize);

            foreach (UIElement child in Children)
            {
                trans = child.RenderTransform as TranslateTransform;
                if (trans == null)
                {
                    child.RenderTransformOrigin = new Point(0, 0);
                    trans = new TranslateTransform();
                    child.RenderTransform = trans;
                }

                if (child is CheckBox)
                {
                    // b = true;
                }

                if (Children.IndexOf(child) % 2 == 0 || curX + child.DesiredSize.Width > finalSize.Width /*|| curX + child.DesiredSize.Width > maxSize*/ || b || Children.IndexOf(child) % 2 == 0)
                { //Wrap to next line

                    curY += curLineHeight + 2;
                    curX = 0;
                    curLineHeight = 0;
                }

                child.Arrange(new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height));

                trans.X = curX;
                trans.Y = curY;

                curX += maxSizeCaption;

                if (child.DesiredSize.Height > curLineHeight)
                    curLineHeight = child.DesiredSize.Height;

            }

            curY += curLineHeight;
            curY += 0;

            this.Height = curY;


            return finalSize;

        }
    }



    public class BindingInfoComparer : IComparer<ControlEntry>
    {
        public int Compare(ControlEntry x, ControlEntry y)
        {
            if (x.tpa.Order != y.tpa.Order)
                return x.tpa.Order.CompareTo(y.tpa.Order);
            else
                return x.tpa.Caption.CompareTo(y.tpa.Caption);
        }
    }


    public class RadioBoolToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            int integer = (int)value;
            if (integer == int.Parse(parameter.ToString()))
                return true;
            else
                return false;


        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Boolean checkedBool = (Boolean)value;
            if (checkedBool)
            {
                if (targetType.Name != "Int32")
                {
                    String[] targetlist = targetType.GetEnumNames();
                    return Enum.Parse(targetType, targetlist[(int)parameter]);
                }
                else
                {
                    return parameter;
                }

            }
            else
            {
                return null;
            }
        }
    }

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

    public class EnumBooleanConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
        #endregion
    }


}

