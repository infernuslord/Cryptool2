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
using KeySearcher.Helper;
using Cryptool.PluginBase;

namespace KeySearcher
{
    /// <summary>
    /// Interaction logic for KeysearcherSettingsTab.xaml
    /// </summary>
    [Localization("KeySearcher.Properties.Resources")]
    [SettingsTab("KeysearcherSettings", "/PluginSettings/")]
    public partial class KeysearcherSettingsTab : UserControl
    {
        private string _realMachName = Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetHostName();

        public static readonly DependencyProperty MachNameToUseProperty =
            DependencyProperty.Register("MachNameToUse",
                                        typeof(string),
                                        typeof(KeysearcherSettingsTab), null);
        public string MachNameToUse
        {
            get
            { 
                return (string)GetValue(MachNameToUseProperty); 
            }
            set
            { 
                SetValue(MachNameToUseProperty, value);
            }
        }

        public KeysearcherSettingsTab(Style settingsStyle)
        {
            MachNameToUse = MachineName.MachineNameToUse;
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();
            
            Cryptool.PluginBase.Properties.Settings.Default.PropertyChanged += delegate
                                                               {
                                                                   Cryptool.PluginBase.Miscellaneous.ApplicationSettingsHelper.SaveApplicationsSettings();
                                                               };

            for (int i = 0; i <= _realMachName.Length; i++)
            {
                NumberOfChars.Items.Add(String.Format(typeof(KeySearcher).GetPluginStringResource("Combo_characters"), i));
            }

            MachineName.OnMachineNameToUseChanged += delegate(string newMachineNameToUse)
                                                        {
                                                            MachNameToUse = newMachineNameToUse;
                                                        };
        }
    }
}
