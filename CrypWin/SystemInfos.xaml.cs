using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Cryptool.P2P;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Miscellaneous;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Threading;

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
        private int timeIndex;

        public SystemInfos()
        {
            InitializeComponent();
            
            var pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            var hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

            /*string uniqueID;
            try
            {
                uniqueID = UniqueIdentifier.GetID().ToString();
            }
            catch (Exception ex)
            {
                uniqueID = string.Format(Properties.Resources.Can_t_get_unique_ID___0_, ex.Message);
            }*/

            //informations.Add(new Info() { Description = Properties.Resources.SI_User_Name, Value = System.Environment.UserName });    //personal information
            informations.Add(new Info() { Description = Properties.Resources.SI_Operating_System, Value = System.Environment.OSVersion.ToString() });
            //informations.Add(new Info() { Description = "Plattform", Value = Environment.OSVersion.Platform.ToString() }); // always Win32NT
            //informations.Add(new Info() { Description = Properties.Resources.SI_Machine_Name, Value = System.Environment.MachineName });      //personal information
            informations.Add(new Info() { Description = Properties.Resources.SI_Processors, Value = System.Environment.ProcessorCount.ToString() });
            //informations.Add(new Info() { Description = "Process Info", Value = (System.Environment.Is64BitProcess ? "64 Bit" : "32 Bit") }); // always 32 Bit
            informations.Add(new Info() { Description = Properties.Resources.SI_Administrative_Rights, Value = hasAdministrativeRight ? Properties.Resources.SI_yes : Properties.Resources.SI_no });
            //informations.Add(new Info() { Description = Properties.Resources.SI_Unique_Identifier, Value = uniqueID });       //personal information
            //informations.Add(new Info() { Description = Properties.Resources.SI_Host_Name, Value = UniqueIdentifier.GetHostName() });     //personal information
            informations.Add(new Info() { Description = Properties.Resources.SI_Current_Culture, Value = CultureInfo.CurrentUICulture.Name });
            informations.Add(new Info() { Description = Properties.Resources.SI_CrypTool_Version, Value = AssemblyHelper.Version.ToString() });
            informations.Add(new Info() { Description = Properties.Resources.SI_Installation_Type, Value = AssemblyHelper.InstallationType.ToString() });
            informations.Add(new Info() { Description = Properties.Resources.SI_Build_Type, Value = AssemblyHelper.BuildType.ToString() });
            informations.Add(new Info() { Description = Properties.Resources.SI_Build_Time, Value = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location).ToString(CultureInfo.CurrentUICulture.DateTimeFormat)});
            informations.Add(new Info() { Description = Properties.Resources.SI_Product_Name, Value = AssemblyHelper.ProductName });
            informations.Add(new Info() { Description = Properties.Resources.SI_Common_Language_Runtime_Version, Value = Environment.Version.ToString() });
            informations.Add(new Info() { Description = Properties.Resources.SI_Runtime_Path, Value = AppDomain.CurrentDomain.BaseDirectory });
            
            // system time row with hacky workaround to update time when tab becomes visible
            timeIndex = informations.Count;
            informations.Add(new Info() { Description = Properties.Resources.SI_System_Time, Value = DateTime.Now.ToShortTimeString() });
            InfoGrid.IsVisibleChanged += UpdateTime;

            InfoGrid.DataContext = informations;

            Tag = FindResource("Icon");

            InfoGrid.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, new ExecutedRoutedEventHandler(MyExecutedRoutedEventHandler), new CanExecuteRoutedEventHandler(CanExecuteRoutedEventHandler)));

            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                string exe = asm.Location;
                X509Certificate2 executingCert = new X509Certificate2(X509Certificate2.CreateFromSignedFile(exe));

                if (executingCert != null)
                {
                    informations.Add(new Info() { Description = Properties.Resources.SI_IsSigned, Value = Properties.Resources.SI_yes });
                    informations.Add(new Info() { Description = Properties.Resources.SI_ValidCertificate, Value = (executingCert.Verify() ? Properties.Resources.SI_yes : Properties.Resources.SI_no) });                    
                    informations.Add(new Info() { Description = Properties.Resources.SI_Subject, Value = executingCert.Subject });
                    informations.Add(new Info() { Description = Properties.Resources.SI_IssuerName, Value = executingCert.Issuer });
                    informations.Add(new Info() { Description = Properties.Resources.SI_KeyAlgorithm, Value = executingCert.GetKeyAlgorithmParametersString() });
                    informations.Add(new Info() { Description = Properties.Resources.SI_PublicKey, Value = executingCert.GetPublicKeyString() });
                    informations.Add(new Info() { Description = Properties.Resources.SI_SerialNumber, Value = executingCert.GetSerialNumberString() });                    
                    informations.Add(new Info() { Description = Properties.Resources.SI_CertHash, Value = executingCert.GetCertHashString() });
                    informations.Add(new Info() { Description = Properties.Resources.SI_EffectiveDate, Value = executingCert.GetEffectiveDateString() });
                    informations.Add(new Info() { Description = Properties.Resources.SI_ExpirationDate, Value = executingCert.GetExpirationDateString() });                    
                }
                else
                {
                    informations.Add(new Info() { Description = Properties.Resources.SI_IsSigned, Value = Properties.Resources.SI_no });
                    informations.Add(new Info() { Description = Properties.Resources.SI_ValidCertificate, Value = Properties.Resources.SI_no });                    
                    informations.Add(new Info() { Description = Properties.Resources.SI_Subject, Value = Properties.Resources.SI_no });
                    informations.Add(new Info() { Description = Properties.Resources.SI_IssuerName, Value = Properties.Resources.SI_no });
                    informations.Add(new Info() { Description = Properties.Resources.SI_KeyAlgorithm, Value = Properties.Resources.SI_no });
                    informations.Add(new Info() { Description = Properties.Resources.SI_PublicKey, Value = Properties.Resources.SI_no });
                    informations.Add(new Info() { Description = Properties.Resources.SI_SerialNumber, Value = Properties.Resources.SI_no });
                    informations.Add(new Info() { Description = Properties.Resources.SI_CertHash, Value = Properties.Resources.SI_no });
                    informations.Add(new Info() { Description = Properties.Resources.SI_EffectiveDate, Value = Properties.Resources.SI_no });
                    informations.Add(new Info() { Description = Properties.Resources.SI_ExpirationDate, Value = Properties.Resources.SI_no });
                }
            }
            catch (Exception ex)
            {
                //wtf?
            }

        }

        public static string ByteArrayToHexString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex;
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

        // update system time row, refresh grid
        private void UpdateTime(object sender, DependencyPropertyChangedEventArgs args)
        {
            // sanity checks
            if (args.Property == null || args.Property.Name != "IsVisible" || !(args.OldValue is bool) || !(args.NewValue is bool) || timeIndex > informations.Count)
                return;

            if (!(bool)args.OldValue && (bool)args.NewValue) // was false, becomes true
            {
                informations[timeIndex] = new Info() { Description = Properties.Resources.SI_System_Time, Value = DateTime.Now.ToShortTimeString() };
                InfoGrid.Items.Refresh();
            }
        }
    }
}
