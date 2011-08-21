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
using Cryptool.PluginBase.Attributes;
using Wizard.Properties;

namespace Wizard
{
    /// <summary>
    /// Interaction logic for WizardSettingsTab.xaml
    /// </summary>
    [Localization("Wizard.Resources.settingsRes")]
    [SettingsTab("WizardSettings", "/MainSettings/")]
    public partial class WizardSettingsTab : UserControl
    {
        public WizardSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();

            Cryptool.PluginBase.Properties.Settings.Default.PropertyChanged += delegate { Cryptool.PluginBase.Properties.Settings.Default.Save(); };
        }
    }
}
