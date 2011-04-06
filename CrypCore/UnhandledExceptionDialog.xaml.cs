using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Cryptool.Core
{
    /// <summary>
    /// Interaction logic for UnhandledExceptionDialog.xaml
    /// </summary>
    public partial class UnhandledExceptionDialog : Window
    {
        private readonly Exception _e;
        private readonly Version _version;
        private readonly string _buildType;
        private readonly string _productName;

        public UnhandledExceptionDialog(Exception e, Version version, string buildType, string productName)
        {
            _e = e;
            _version = version;
            _buildType = buildType;
            _productName = productName;
            InitializeComponent();
            ExceptionNameLabel.Content = e.GetType().FullName;
            ExceptionMessageLabel.Text = e.Message;
            StackTraceBox.Text = e.StackTrace;
        }

        private void Button_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var button = MessageBoxButton.YesNo;
            var icon = MessageBoxImage.Question;
            var res = MessageBox.Show("Do you want to report these error informations to the CrypTool 2.0 developers? This is fully anonymous!", "Report error", button, icon);
            if (res == MessageBoxResult.Yes)
            {
                var sb = new StringBuilder();
                sb.AppendLine(string.Format("Exception at {0} (UTC time):", DateTime.UtcNow));
                sb.AppendLine(_e.ToString());
                sb.AppendLine("");
                sb.AppendLine("-");
                sb.AppendLine("System infos:");
                sb.AppendLine(GetSystemInfos());

                try
                {
                    Mailer.SendMailToCoreDevs("Crash report!", sb.ToString());
                    MessageBox.Show("The error has been reported. Thank you!", "Reporting done");
                }
                catch (Exception)
                {
                    MessageBox.Show("Error trying to report!", "Reporting failed");
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs routedEventArgs)
        {
            Close();
        }

        private string GetSystemInfos()
        {
            var pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            var hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

            var sb = new StringBuilder();
            sb.AppendLine(string.Format("User Name: {0}", System.Environment.UserName));
            sb.AppendLine(string.Format("Operating System: {0}", System.Environment.OSVersion.ToString()));
            sb.AppendLine(string.Format("Plattform: {0}", Environment.OSVersion.Platform));
            sb.AppendLine(string.Format("Machine Name: {0}", System.Environment.MachineName));
            sb.AppendLine(string.Format("Processors: {0}", System.Environment.ProcessorCount));
            sb.AppendLine(string.Format("Process Info: {0}", (System.Environment.Is64BitProcess ? "64 Bit" : "32 Bit")));
            sb.AppendLine(string.Format("Administrative Rights: {0}", hasAdministrativeRight));
            sb.AppendLine(string.Format("Current culture: {0}", CultureInfo.CurrentCulture.Name));
            sb.AppendLine(string.Format("CrypTool version: {0}", _version));
            sb.AppendLine(string.Format("Build type: {0}", _buildType));
            sb.AppendLine(string.Format("Product name: {0}", _productName));
            sb.AppendLine(string.Format("Common language runtime version: {0}", Environment.Version));

            return sb.ToString();
        }

        public static void ShowModalDialog(Exception e, Version version, string buildType, string productName)
        {
            var unhandledExceptionDialog = new UnhandledExceptionDialog(e, version, buildType, productName);
            unhandledExceptionDialog.ShowDialog();
        }
    }
}
