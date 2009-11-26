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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Cryptool.PluginBase;
using System.Windows.Threading;
using System.Threading;

namespace SimpleEditor
{
    /// <summary>
    /// Interaction logic for UserControlSimpleEditor.xaml
    /// </summary>
    public partial class UserControlSimpleEditor : UserControl
    {
        private IPlugin plugin;
        private Dictionary<PropertyInfoAttribute, UserCtrlInput> controls;

        public UserControlSimpleEditor()
        {
            InitializeComponent();
            controls = new Dictionary<PropertyInfoAttribute, UserCtrlInput>();
        }

        public void DisplayControls(IPlugin plugin, List<PropertyInfoAttribute> inputProps, List<PropertyInfoAttribute> outputProps)
        {
            this.plugin = plugin;

            if (plugin == null)
                return;

            this.pluginTabItem.Header = this.plugin.GetPluginInfoAttribute().Caption;
            mainStackPanel.Children.Clear();
            controls.Clear();

            addProps(inputProps);
            addProps(outputProps);

            pluginTabItem.Visibility = Visibility.Visible;
            mainTabControl.SelectedItem = pluginTabItem;
        }

        private void addProps(List<PropertyInfoAttribute> props)
        {
            foreach (PropertyInfoAttribute pInfo in props)
            {
                UserCtrlInput usrCtrl = new UserCtrlInput(pInfo.Caption, pInfo.ToolTip);
                controls[pInfo] = usrCtrl;
                mainStackPanel.Children.Add(usrCtrl);
            }
        }

        private bool? dispatchIsChecked(RadioButton radio)
        {
            bool? isChecked = null;

            radio.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                isChecked = radio.IsChecked;
            }, null);

            return isChecked;
        }

        private String dispatchGetBoxText(TextBox box)
        {
            String text = null;

            box.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                text = box.Text;
            }, null);

            return text;
        }

        private void dispatchSetBoxText(TextBox box, String text)
        {
            box.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                box.Text = text;
            }, null);
        }

        public bool IsUsingTextBox(PropertyInfoAttribute pInfo)
        {
            UserCtrlInput ctrl = controls[pInfo];

            if (dispatchIsChecked(ctrl.radioButtonString) == true)
                return true;
            else if (dispatchIsChecked(ctrl.radioButtonFile) == true)
                return false;
            else
                throw new InvalidOperationException("Neither string nor file radio button is checked");
        }

        public String GetBoxText(PropertyInfoAttribute pInfo)
        {
            return dispatchGetBoxText(controls[pInfo].textBoxString);
        }

        public void SetBoxText(PropertyInfoAttribute pInfo, String text)
        {
            dispatchSetBoxText(controls[pInfo].textBoxString, text);
        }
    }
}
