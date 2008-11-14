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

namespace SimpleEditor
{
    /// <summary>
    /// Interaction logic for UserControlSimpleEditor.xaml
    /// </summary>
    public partial class UserControlSimpleEditor : UserControl
    {
        IPlugin plugin;
        List<UserControl> inputControl;
        List<UserControl> outputControl;
        
        public UserControlSimpleEditor(IPlugin plugin, List<UserControl> inputControl, List<UserControl> outputControl)
        {
            InitializeComponent();
            this.plugin = plugin;
            this.inputControl = inputControl;
            this.outputControl = outputControl;
            DisplayControls();
        }

        public void DisplayControls()
        {
            if (this.plugin != null)
            {
                this.pluginTabItem.Header = this.plugin.GetPluginInfo().Name;
                mainStackPanel.Children.Clear();
                foreach (PropertyInformation pInfo in this.plugin.GetInputProperties())
                {
                    InputUsrCtrl inputUsrCtrl = new InputUsrCtrl(pInfo.Caption, pInfo.Description);
                    this.inputControl.Add(inputUsrCtrl);
                    mainStackPanel.Children.Add(inputUsrCtrl);
                }
                foreach (PropertyInformation pInfo in this.plugin.GetOutputProperties())
                {
                    OutputUsrCtrl outputUsrCtrl = new OutputUsrCtrl(pInfo.Caption, pInfo.Description);
                    this.outputControl.Add(outputUsrCtrl);
                    mainStackPanel.Children.Add(outputUsrCtrl);
                }
                pluginTabItem.Visibility = Visibility.Visible;
                mainTabControl.SelectedItem = pluginTabItem;
            }

        }


    }
}
