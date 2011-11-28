using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
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
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.CrypWin
{
    /// <summary>
    /// Interaction logic for SystemInfos.xaml
    /// </summary>
    [TabColor("pink")]
    [Localization("Cryptool.CrypWin.Properties.Resources")]
    public partial class SystemInfos : UserControl
    {
        private struct Info
        {
            public string Description { get; set; }
            public string Value { get; set; }
        }

        private List<Info> informations = new List<Info>();

        public SystemInfos()
        {
            InitializeComponent();

            var pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            var hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

            string uniqueID;
            try
            {
                uniqueID = UniqueIdentifier.GetID().ToString();
            }
            catch (Exception ex)
            {
                uniqueID = string.Format(Properties.Resources.Can_t_get_unique_ID___0_, ex.Message);
            }

            informations.Add(new Info() { Description = Properties.Resources.SI_User_Name, Value = System.Environment.UserName });
            informations.Add(new Info() { Description = Properties.Resources.SI_Operating_System, Value = System.Environment.OSVersion.ToString() });
            //informations.Add(new Info() { Description = "Plattform", Value = Environment.OSVersion.Platform.ToString() }); // always Win32NT
            informations.Add(new Info() { Description = Properties.Resources.SI_Machine_Name, Value = System.Environment.MachineName });
            informations.Add(new Info() { Description = Properties.Resources.SI_Processors, Value = System.Environment.ProcessorCount.ToString() });
            //informations.Add(new Info() { Description = "Process Info", Value = (System.Environment.Is64BitProcess ? "64 Bit" : "32 Bit") }); // always 32 Bit
            informations.Add(new Info() { Description = Properties.Resources.SI_Administrative_Rights, Value = hasAdministrativeRight.ToString() });
            informations.Add(new Info() { Description = Properties.Resources.SI_Unique_Identifier, Value = uniqueID });
            informations.Add(new Info() { Description = Properties.Resources.SI_Host_Name, Value = UniqueIdentifier.GetHostName() });
            informations.Add(new Info() { Description = Properties.Resources.SI_Current_Culture, Value = CultureInfo.CurrentUICulture.Name });
            informations.Add(new Info() { Description = Properties.Resources.SI_CrypTool_Version, Value = AssemblyHelper.Version.ToString() });
            informations.Add(new Info() { Description = Properties.Resources.SI_Installation_Type, Value = AssemblyHelper.InstallationType.ToString() });
            informations.Add(new Info() { Description = Properties.Resources.SI_Build_Type, Value = AssemblyHelper.BuildType.ToString() });
            informations.Add(new Info() { Description = Properties.Resources.SI_Build_Time, Value = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location).ToString() });
            informations.Add(new Info() { Description = Properties.Resources.SI_Common_Language_Runtime_Version, Value = Environment.Version.ToString() });

            InfoGrid.DataContext = informations;

            Tag = FindResource("Icon");

            InfoGrid.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, new ExecutedRoutedEventHandler(MyExecutedRoutedEventHandler), new CanExecuteRoutedEventHandler(CanExecuteRoutedEventHandler)));
        }

        private void MyExecutedRoutedEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            var info = (Info) InfoGrid.SelectedItem;
            Clipboard.SetText(info.Description + ": " + info.Value);
        }
        
        private void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (InfoGrid.SelectedIndex >= 0);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (var info in informations)
            {
                sb.Append(info.Description);
                sb.Append(": ");
                sb.Append(info.Value);
                sb.AppendLine();
            }
            Clipboard.SetText(sb.ToString());
        }

    }
}
