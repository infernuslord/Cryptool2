
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
using System.Windows.Media.Animation;
using System.Globalization;
using System.Collections.Specialized;



namespace WorkspaceManager.View.BinVisual
{
    public partial class BinSettingsVisual : UserControl
    {
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private readonly Thickness CONTROL_DEFAULT_MARGIN = new Thickness(4, 0, 0, 0);
        private Dictionary<ISettings, Dictionary<string, List<RadioButton>>> dicRadioButtons = new Dictionary<ISettings, Dictionary<string, List<RadioButton>>>();
        private IPlugin plugin;
        private EntryGroup entgrou;
        private BinComponentVisual bcv;
        private TabControl tbC;
        public String myConnectorName;
        private Boolean isSideBar;

        public BinSettingsVisual(IPlugin plugin, BinComponentVisual bcv, Boolean isMaster, Boolean isSideBar)
        {
            this.isSideBar = isSideBar;
            this.bcv = bcv;
            this.plugin = plugin;
            entgrou = new EntryGroup();
            this.entgrou = createContentSettings(plugin);

           

            ((WorkspaceManagerClass)bcv.Model.WorkspaceModel.MyEditor).executeEvent += new EventHandler(excuteEventHandler);

           
            //plugin.Settings.PropertyChanged += myTaskPaneAttributeChangedHandler;
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
                MyScrollViewer.Margin = new Thickness(-5, -5, -5, -5);
                
            }
            
            drawList(this.entgrou);
           
            


            /*
            for(int i = 0 ; i< bcv.IControlCollection.Count ; i++)
            {
                this.entgrou.Add(createContentSettings(bcv.IControlCollection[i].PluginModel.Plugin));
                drawList(this.entgrou[i+1]);
                Console.WriteLine("Hallo" + i);
            }
            Console.WriteLine("Hallo" );*/
            
            
            
            
        }

        private void CollectionChangedHandler(Object sender, NotifyCollectionChangedEventArgs args)
        {
            //Console.WriteLine(args.Action);
            
            for (int i = 0; i < args.NewItems.Count;i++ )
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
                //  Console.WriteLine(master.PluginModel.GetName());
                Boolean b = true;
                foreach (TabItem vtbI in tbC.Items)
                {
                    if (vtbI.Uid == master.ConnectorModel.PropertyName)
                    {

                        vtbI.Content = new BinSettingsVisual(master.PluginModel.Plugin, bcv, false, isSideBar);
                        vtbI.Header = master.PluginModel.GetName();
                        b = false;
                    }
                }

                if (b)
                {
                    TabItem tbI = new TabItem();
                    tbI.Uid = master.ConnectorModel.PropertyName;
                    tbI.Content = new BinSettingsVisual(master.PluginModel.Plugin, bcv, false,isSideBar);
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
                if(tbI!=null)
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

                                if (ce.element is NumericUpDown)
                                {
                                    if (tpac.Visibility == System.Windows.Visibility.Collapsed)
                                    {
                                        ce.element.Visibility = System.Windows.Visibility.Hidden;
                                        ce.caption.Visibility = System.Windows.Visibility.Hidden;
                                    }
                                    else 
                                    {
                                        ce.element.Visibility = tpac.Visibility;
                                        ce.caption.Visibility = tpac.Visibility;
                                        
                                    }
                                }
                                else
                                {
                                    ce.element.Visibility = tpac.Visibility;
                                    ce.caption.Visibility = tpac.Visibility;
                                }
                                    
                                
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
                                ce.element.IsEnabled = false;
                            if (ce.element is NumericUpDown)
                            {
                                NumericUpDown nud =  ce.element as NumericUpDown;
                                nud.Opacity = 0.80;
                                nud.Foreground = Brushes.Gray;
                            }
                        }
                        else 
                        {
                            if (!ce.tpa.ChangeableWhileExecuting)
                                ce.element.IsEnabled = true;
                            if (ce.element is NumericUpDown)
                            {
                                NumericUpDown nud = ce.element as NumericUpDown;
                                nud.Opacity = 1;
                                nud.Foreground = Brushes.Black;
                            }
                        }
                    }
                }
                
            }, null);
            
        }


        List<String> groups = new List<String>();

       

        private double getComboBoxMaxSize(ComboBox child) 
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

            return cb.Width = x + 30;
        }


        private void drawList(EntryGroup entgrou) 
        {
                foreach (List<ControlEntry> cel in entgrou.entryList)
                {
                    Expander testexoander = new Expander();

                    Expander noverticalgroupexpander = new Expander();
                    
                    TestPanel noVerticalGroup = new TestPanel();

                    Border noVerticalGroupBodi = new Border();

                    noVerticalGroupBodi.Child = noVerticalGroup;

                    noverticalgroupexpander.Content = noVerticalGroupBodi;

                    Border bodi = new Border();
                    
                    testexoander.IsExpanded = true;
                    
                    TestPanel test = new TestPanel();

                    entgrou.gorupPanel.Add(testexoander);

                    test.Name = "border1";

                    //test.Background = Brushes.AliceBlue;

                    //test.IsExpanded = true;


 //                   test.Expanded += test_ContextMenuOpening;

                    test.Margin = new Thickness(10);
                    //test.VerticalAlignment = VerticalAlignment.Stretch;
                    
                    Binding dataBinding = new Binding("ActualWidth");
                    dataBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    dataBinding.Mode = BindingMode.OneWay;
                    dataBinding.Source = test;

//                    bodi.SetBinding(Border.WidthProperty, dataBinding);


                    if ( !string.IsNullOrEmpty(cel[0].tpa.groupName) )
                    {
                          testexoander.Header = cel[0].tpa.GroupName;
                    }
                    
                          

                    

                    //testexoander.Style = (Style)FindResource("Expander");
                    

                    
                    StackPanel contentPanel = new StackPanel();
                    List<String> grouplist = new List<String>();
                    List<Grid> gridlist = new List<Grid>();
                    List<TextBlock> tebo = new List<TextBlock>();

                    
                    
                    //test.Width = 400;      




                    foreach (ControlEntry ce in cel) 
                    {
                        
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
                                test.Children.Add(ce.element);
                                test.Children.Add(l);
                            }

                            else
                            {
                                test.Children.Add(title);
                                if (ce.element is ComboBox)
                                {
                                    ComboBox cb = ce.element as ComboBox;
                                    cb.MaxWidth = getComboBoxMaxSize(cb);
                                    test.Children.Add(cb);

                                }
                                else
                                {
                                    test.Children.Add(ce.element);
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
                                    //controlGrid.Margin = new Thickness(10);

                                    ColumnDefinition coldef1 = new ColumnDefinition();
                                    coldef1.Width = ce.sfa.WidthCol1;
                                    controlGrid.ColumnDefinitions.Add(coldef1);

                                    ColumnDefinition coldef2 = new ColumnDefinition();
                                    coldef2.Width = ce.sfa.WidthCol2;
                                    controlGrid.ColumnDefinitions.Add(coldef2);
                                    //controlGrid.ColumnDefinitions.Add(new ColumnDefinition());

                                    //TextBlock title = new TextBlock();
                                    title.Text = ce.tpa.Caption;
                                    ce.caption = title;
                                    title.HorizontalAlignment = HorizontalAlignment.Center;

                                    Label space = new Label();
                                    space.Width = 0;
                                    Grid.SetColumn(title, controlGrid.ColumnDefinitions.Count - 2);

                                    //controlGrid.Children.Add(space);

//                                    Grid.SetColumn(space, controlGrid.ColumnDefinitions.Count - 3);
                                    
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
                                    //controlGrid.Margin = new Thickness(10);

                                    ColumnDefinition coldef1 = new ColumnDefinition();
                                    coldef1.Width = ce.sfa.WidthCol1;
                                    controlGrid.ColumnDefinitions.Add(coldef1 );

                                    ColumnDefinition coldef2 = new ColumnDefinition();
                                    coldef2.Width = ce.sfa.WidthCol2;
                                    controlGrid.ColumnDefinitions.Add(coldef2);

                                    //TextBlock title = new TextBlock();
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
                                    //controlGrid.MaxWidth = controlGrid.DesiredSize.Width;
                                    test.Children.Add(controlGrid);
                                    test.Children.Add(dummy);

                                    controlGrid.Width = test.Width;

                                    gridlist.Add(controlGrid);
                                }
                            }
                            else 
                            {
                                if (!test.IsAncestorOf(noverticalgroupexpander))
                                {
                                    Label l = new Label();
                                    l.Width = 1;
                                    l.Height = 0;
                                    test.Children.Add(noverticalgroupexpander);
                                    test.Children.Add(l);
                                }

                               /* WrapPanel controlGrid = new WrapPanel();
                                controlGrid.Orientation = Orientation.Horizontal;
                                //controlGrid.Margin = new Thickness(0);
                                TextBlock title = new TextBlock();
                                title.Text = ce.tpa.Caption;
                                title.TextWrapping = TextWrapping.Wrap;


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

                                if (maxlength < title.Text.Length)
                                {
                                    maxlength = title.Text.Length;
                                }
                                */

                                //TextBlock title = new TextBlock();
                                title.Text = ce.tpa.Caption;
                                ce.caption = title;
                                title.TextWrapping = TextWrapping.Wrap;

                                if (ce.element is CheckBox || ce.element is Button)
                                {
                                    Label l = new Label();
                                    l.Width = 1;
                                    l.Height = 0;
                                    noVerticalGroup.Children.Add(ce.element);
                                    test.Children.Add(l);
                                   
                                }
                                else if (ce.element is ComboBox )
                                {
                                    ComboBox cb = ce.element as ComboBox;
                                    //cb.Width = getComboBoxMaxSize(cb);
                                    noVerticalGroup.Children.Add(cb);
                                }
                                else
                                {
                                    noVerticalGroup.Children.Add(title);
                                    noVerticalGroup.Children.Add(ce.element);
                                }
                            }
                        }
                        
                    }
                    test.HorizontalAlignment = HorizontalAlignment.Left;
                   
                    bodi.Child = test;
                    testexoander.Content = bodi;
                    if (isSideBar)
                        myStack.Children.Add(testexoander);
                    else
                        myWrap.Children.Add(testexoander);
                    test.setMaxSizes();
                    noVerticalGroup.setMaxSizes();
                }

                this.BeginInit();

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
                
               // try
                //{
                    switch (tpa.ControlType)
                    {
                        #region TextBox
                        case ControlType.TextBox:

                            TextBox textbox = new TextBox();
                            
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
                                intInput.IsEnabled = true;
                                
                                /*
                                InputNumericControl intInput = new InputNumericControl();
                                intInput.ShowCheckBox = false;
                                intInput.ShowClearButton = true;
                                intInput.ShowUpDown = true;
                                intInput.Tag = tpa.ToolTip;
                                intInput.MouseEnter += Control_MouseEnter;
                                //intInput.MaxValue = tpa.IntegerMaxValue;
                                //intInput.MinValue = tpa.IntegerMinValue;
                                //intInput.SetBinding(InputNumericControl.prop, dataBinding);
                                entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(intInput, tpa, sfa));
                                //inputControl = intInput;
                                //bInfo.CaptionGUIElement = intInput;*/
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
                                doubleInput.Background = Brushes.Black;
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
                            entgrou.AddNewEntry(tpa.GroupName,new ControlEntry(comboBox, tpa, sfa));
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
                                    
                                    radio.SetBinding(RadioButton.IsCheckedProperty, dataBinding1);
                                    panelRadioButtons.Children.Add(radio);
                                    list.Add(radio);
                                }
                                dicRadioButtons[plugin.Settings].Add(tpa.PropertyName, list);
                                entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(panelRadioButtons, tpa, sfa));
                           
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
                            sp.Uid = "FileDialog";
                            sp.Orientation = Orientation.Vertical;

                            TextBox fileTextBox = new TextBox();
                            fileTextBox.TextWrapping = TextWrapping.Wrap;
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
                            TextBlock contentBlock = new TextBlock();
                            contentBlock.Text = tpa.Caption;
                            contentBlock.TextWrapping = TextWrapping.Wrap;
                            contentBlock.TextAlignment = TextAlignment.Center;
                            taskPaneButton.Content = contentBlock;
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
                            
                            slider.MinWidth = 0;

                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(slider, tpa, sfa));
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
                            textBoxReadOnly.MouseEnter += Control_MouseEnter;
                            textBoxReadOnly.SetBinding(TextBox.TextProperty, dataBinding);
                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(textBoxReadOnly, tpa, sfa));
                            break;
                        # endregion TextBoxReadOnly
                
                        #region TextBoxHidden
                            case ControlType.TextBoxHidden:
                            PasswordBox passwordBox = new PasswordBox();

                            passwordBox.MinWidth = 0; 
                            
                            passwordBox.Tag = tpa;
                            passwordBox.MouseEnter += Control_MouseEnter;
                            passwordBox.Password = plugin.Settings.GetType().GetProperty(tpa.PropertyName).GetValue(plugin.Settings, null) as string;
                            //textBoxReadOnly.SetBinding(PasswordBox.property , dataBinding);
                            passwordBox.PasswordChanged += TextBoxHidden_Changed;
                            entgrou.AddNewEntry(tpa.GroupName, new ControlEntry(passwordBox, tpa, sfa));
                        break;
                        #endregion TextBoxHidden
              
                    }

                     
             //   }

           //     catch (Exception) { }
                
            }
            entgrou.sort();

            return entgrou;

        }

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
            foreach(List<ControlEntry> dummyList in entryList)
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

        public UIElement caption
        {
            get;
            set;
        }

       

       public ControlEntry (UIElement element, TaskPaneAttribute tpa, SettingsFormatAttribute sfa)
        {
            this.element = element;
            this.sfa = sfa;
            this.tpa = tpa;
        }

    }

    public class TestPanel : Panel
    {

        double maxSize = 0;
        double maxSizeContent = 0;
        double maxSizeCaption = 0;

        double maxSizeCB = 0;

        Grid maxGrid = new Grid();

        public TestPanel()
        {
            SizeChanged += new SizeChangedEventHandler(TestPanel_SizeChanged);
        }

        public void setMaxSizes() 
        {
            foreach (UIElement child in Children)
            {


                child.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                child.Arrange(new Rect(child.DesiredSize));



                if (true)
                {
                    if (child is TextBlock || child is Expander)
                    {
                        if (child.DesiredSize.Width > maxSizeCaption)
                        {
                            maxSizeCaption = child.DesiredSize.Width;

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
                    else 
                    {
                        if (child.DesiredSize.Width > maxSizeContent)
                        {
                            if (child.DesiredSize.Width != 0)
                                maxSizeContent = child.DesiredSize.Width;


                        }
                    }
                }
            }

            if (maxSizeContent < 20)
                maxSizeContent = 200;

            maxSizeCaption += 5;
            maxSizeContent += 5;
            maxSize = maxSizeCaption  + maxSizeContent ;

            if (maxSizeCB > maxSize)
            {
                maxSize = maxSizeCB;
                maxSizeContent = maxSizeCB - maxSizeCaption - 1;
            }

                //if (maxSizeCaption > maxSizeContent)
                //  this.MinWidth = maxSizeCaption;
                //else
                // this.MinWidth = maxSizeContent;

            if (maxSize < maxGrid.DesiredSize.Width)
                {
                    maxSize = maxGrid.DesiredSize.Width;
                    //  this.MinWidth = maxGrid.Width;
                }

            this.MaxWidth = maxSize + 10;
            
            foreach (UIElement child in Children)
            {
                if (child is NumericUpDown)
                {
                    NumericUpDown dummyTextBox = child as NumericUpDown;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    //dummyTextBox.MaxWidth = dummyTextBox.DesiredSize.Width;
                    dummyTextBox.MaxWidth = maxSizeContent;

                }

                if (child is ComboBox)
                {
                    ComboBox dummyTextBox = child as ComboBox;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    //dummyTextBox.MaxWidth = dummyTextBox.DesiredSize.Width;
                    dummyTextBox.MaxWidth = maxSizeContent;

                }

                if (child is TextBox)
                {
                    TextBox dummyTextBox = child as TextBox;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    //dummyTextBox.MaxWidth = dummyTextBox.DesiredSize.Width;
                    
                    dummyTextBox.MaxWidth = maxSizeContent;

                }

                if (child is PasswordBox)
                {
                    PasswordBox dummyTextBox = child as PasswordBox;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    //dummyTextBox.MaxWidth = dummyTextBox.DesiredSize.Width;

                    dummyTextBox.MaxWidth = maxSizeContent;

                }

                if (child is TextBlock)
                {
                    TextBlock dummyTextBox = child as TextBlock;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    //dummyTextBox.MaxWidth = dummyTextBox.DesiredSize.Width;
                    dummyTextBox.MaxWidth = maxSizeCaption;

                }

                if (child is Button)
                {
                    Button dummyTextBox = child as Button;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    //dummyTextBox.MaxWidth = dummyTextBox.DesiredSize.Width;
                    dummyTextBox.MaxWidth = maxSize;

                }

                if (child is CheckBox)
                {
                    CheckBox dummyTextBox = child as CheckBox;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    //dummyTextBox.MaxWidth = dummyTextBox.DesiredSize.Width;
                    dummyTextBox.MaxWidth = maxSize;

                }

                if (child is StackPanel)
                {
                    StackPanel dummyTextBox = child as StackPanel;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    //dummyTextBox.MaxWidth = dummyTextBox.DesiredSize.Width;
                    if(dummyTextBox.Uid == "FileDialog")
                        dummyTextBox.MaxWidth = maxSize;
                    else
                        dummyTextBox.MaxWidth = dummyTextBox.DesiredSize.Width;

                }

                if (child is Slider)
                {
                    Slider dummyTextBox = child as Slider;
                    dummyTextBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    dummyTextBox.Arrange(new Rect(dummyTextBox.DesiredSize));
                    //dummyTextBox.MaxWidth = dummyTextBox.DesiredSize.Width;
                    dummyTextBox.MaxWidth = maxSizeContent-10;

                }

                if (child is Expander)
                {
                    //Expander dummyTextBox = child as Expander;
                    //dummyTextBox.MaxWidth = maxSizeContent;

                }



                
            }



        }

        private void TestPanel_SizeChanged(Object sender, SizeChangedEventArgs args) 
        {
            //Console.WriteLine(this.ActualWidth);
            foreach (UIElement child in Children)
            {
                child.Measure(new Size(this.ActualWidth,double.PositiveInfinity));

                if (child is CheckBox)
                {
                    CheckBox dummyTextBox = child as CheckBox;

                    dummyTextBox.MinWidth = 0;

                    dummyTextBox.Width = this.ActualWidth;
                }

                if (child is Slider)
                {
                    Slider dummyTextBox = child as Slider;
                    dummyTextBox.Width = this.ActualWidth;
                }

                if (child is StackPanel) 
                {
                    StackPanel dummyTextBox = child as StackPanel;

                    dummyTextBox.MinWidth = 0;

                    dummyTextBox.Width = this.ActualWidth;
                }

                if (child is NumericUpDown) 
                {
                    NumericUpDown dummyTextBox = child as NumericUpDown;

                    dummyTextBox.MinWidth = 0;

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
                    
                    dummyTextBox.Width = this.ActualWidth;
                }

                if (child is PasswordBox)
                {

                    PasswordBox dummyTextBox = child as PasswordBox;
                    dummyTextBox.MinWidth = 0;

                    dummyTextBox.Width = this.ActualWidth;
                }

                if (child is Grid)
                {

                    Grid dummyTextBox = child as Grid;
                    dummyTextBox.MinWidth = 0;
                    
                    dummyTextBox.Width = this.ActualWidth;
                }



                if (child is Expander)
                {

                    Expander dummyTextBox = child as Expander;
                    //dummyTextBox.MinWidth = 0;

                    dummyTextBox.Width = this.ActualWidth;
                }

                if (child is ComboBox)
                {

                    ComboBox dummyTextBox = child as ComboBox;
                    dummyTextBox.MinWidth = 0;
                    
                    dummyTextBox.Width = this.ActualWidth;
                }

                

            }
        }


        private TimeSpan _AnimationLength = TimeSpan.FromMilliseconds(200);

        protected override Size MeasureOverride(Size availableSize)
        {
            Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            double curX = 0, curY = 0, curLineHeight = 0;
            /*
            double maxSize = 0;
            double maxSizeContent = 0;
            double maxSizeCaption = 0;

            Grid maxGrid = new Grid();

            

            foreach (UIElement child in Children)
            {
                if (true)
                {
                    if (child is TextBlock || child is CheckBox || child is Expander )
                    {
                        if (child.DesiredSize.Width > maxSizeCaption)
                        {
                            maxSizeCaption = child.DesiredSize.Width;
                            
                        }
                    }
                    else if (child is Grid) 
                    {
                        if (maxGrid.Width < (child as Grid).Width)
                        {
                            maxGrid = child as Grid;
                            
                        }
                        
                    }
                    else
                    {
                        if (child.DesiredSize.Width > maxSizeContent)
                        {
                            maxSizeContent = child.DesiredSize.Width;
                        }
                    }
                }
            }

           

            maxSizeCaption += 5;
            maxSize = maxSizeCaption + maxSizeContent;
            

            //if (maxSizeCaption > maxSizeContent)
              //  this.MinWidth = maxSizeCaption;
            //else
               // this.MinWidth = maxSizeContent;
            
            if (maxSize < maxGrid.Width)
            {
                maxSize = maxGrid.Width;
              //  this.MinWidth = maxGrid.Width;
            }

            this.MaxWidth = maxSize + 10;
            */

            Boolean b = true;

            if (availableSize.Width > maxSize)
                b = false;

            foreach (UIElement child in Children)
            {

                child.Measure(infiniteSize);
                /*
                if (child is Grid)
                {
                    Grid dummy = child as Grid;
                    if (this.ActualWidth != 0)
                    {
                        if(0>this.ActualWidth -10)
                            dummy.Width = this.ActualWidth; 
                        else
                            dummy.Width = this.ActualWidth - 10; 
                    }
                    else
                    {
                        if (0 > dummy.DesiredSize.Width - 10)
                            dummy.Width = dummy.DesiredSize.Width;
                        else
                            dummy.Width = dummy.DesiredSize.Width - 10;
                    }
                }*/

               
                

                if (Children.IndexOf(child) % 2 == 0 || curX + child.DesiredSize.Width > availableSize.Width || curX + child.DesiredSize.Width > maxSize || b)
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
            /*
            double maxSize = 0;
            double maxSizeContent = 0;
            double maxSizeCaption = 0;

            
            Grid maxGrid = new Grid();



            foreach (UIElement child in Children)
            {
                if (true)
                {
                    if (child is TextBlock || child is CheckBox || child is Expander )
                    {
                        if (child.DesiredSize.Width > maxSizeCaption)
                        {
                            maxSizeCaption = child.DesiredSize.Width;
                            
                        }
                    }
                    else if (child is Grid)
                    {
                        if (maxGrid.Width < (child as Grid).Width)
                        {
                            maxGrid = child as Grid;
                            
                        }
                       
                    }
                    else
                    {
                        if (child.DesiredSize.Width > maxSizeContent)
                        {
                            maxSizeContent = child.DesiredSize.Width;
                        }
                    }
                }
            }
            maxSizeCaption += 5;
            maxSize = maxSizeCaption + maxSizeContent;

            if (maxSizeContent > maxSizeCaption)
            {
                //this.MinWidth = maxSizeContent;
            }
            else 
            {
               // this.MinWidth = maxSizeCaption;
            }

            if (maxSize < maxGrid.Width)
            {
                maxSize = maxGrid.Width;
                //this.MinWidth = maxGrid.Width;
            }

            this.MaxWidth = maxSize + 10;
            */
            /*
            if (maxSizeCaption > maxSizeContent)
                this.MinWidth = maxSizeCaption;
            else
                this.MinWidth = maxSizeContent;
            */
            if (this.Children == null || this.Children.Count == 0)
                return finalSize;

            TranslateTransform trans = null;
            double curX = 0, curY = 0, curLineHeight = 0;

            Boolean b = true;

            if (finalSize.Width > maxSize)
                b = false;

            foreach (UIElement child in Children)
            {
                trans = child.RenderTransform as TranslateTransform;
                if (trans == null)
                {
                    child.RenderTransformOrigin = new Point(0, 0);
                    trans = new TranslateTransform();
                    child.RenderTransform = trans;
                }
                
                if (Children.IndexOf(child) % 2 == 0 || curX + child.DesiredSize.Width > finalSize.Width || curX + child.DesiredSize.Width > maxSize || b || Children.IndexOf(child) % 2 == 0 )
                { //Wrap to next line
                    
                    curY += curLineHeight + 2;
                    curX = 0;
                    curLineHeight = 0;
                }

                child.Arrange(new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height));

                //trans.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(curX, _AnimationLength), HandoffBehavior.Compose);
                //trans.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(curY, _AnimationLength), HandoffBehavior.Compose);

                trans.X = curX;
                trans.Y = curY;

                curX += maxSizeCaption;

                if (child.DesiredSize.Height > curLineHeight )
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
                if(targetType.Name != "Int32")
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

