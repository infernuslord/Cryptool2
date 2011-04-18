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
using System.Windows.Shapes;

namespace Cryptool.Core
{
    /// <summary>
    /// Interaction logic for UnhandledExceptionDialog.xaml
    /// </summary>
    public partial class ReportErrorDialog : Window
    {
        private readonly Exception _e;
        private readonly Version _version;
        private readonly string _buildType;
        private readonly string _productName;
        private string _systemInfos;

        public ReportErrorDialog(Exception e, Version version, string buildType, string productName)
        {
            _e = e;
            _version = version;
            _buildType = buildType;
            _productName = productName;
            _systemInfos = GetSystemInfos();

            InitializeComponent();
            UpdateSendInformationsBox();
        }

        private void SendButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            // User clicked on "report" and then "send" at this point. Attempt to send mail.
            try
            {
                Mailer.SendMailToCoreDevs("Crash report!", SendInformations.Text);
                MessageBox.Show("The error has been reported. Thank you!", "Reporting done");
                Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Reporting failed: {0}. Please try again later.", e.Message), "Reporting failed");
            }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Close();
        }

        private string GetSystemInfos()
        {
            var pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            var hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Operating System: {0}", System.Environment.OSVersion.ToString()));
            sb.AppendLine(string.Format("Plattform: {0}", Environment.OSVersion.Platform));
            sb.AppendLine(string.Format("Processors: {0}", System.Environment.ProcessorCount));
            sb.AppendLine(string.Format("Process Info: {0}", (System.Environment.Is64BitProcess ? "64 Bit" : "32 Bit")));
            sb.AppendLine(string.Format("Administrative Rights: {0}", hasAdministrativeRight));
            sb.AppendLine(string.Format("Current culture: {0}", CultureInfo.CurrentCulture.Name));
            sb.AppendLine(string.Format("CrypTool version: {0}", _version));
            sb.AppendLine(string.Format("Build type: {0}", _buildType));
            sb.AppendLine(string.Format("Build time: {0}", File.GetLastWriteTime(Assembly.GetEntryAssembly().Location)));
            sb.AppendLine(string.Format("Product name: {0}", _productName));
            sb.AppendLine(string.Format("Common language runtime version: {0}", Environment.Version));

            return sb.ToString();
        }

        public static void ShowModalDialog(Exception e, Version version, string buildType, string productName)
        {
            var unhandledExceptionDialog = new UnhandledExceptionDialog(e, version, buildType, productName);
            unhandledExceptionDialog.ShowDialog();
        }

        private void UserMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSendInformationsBox();
        }

        private void UpdateSendInformationsBox()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("Exception at {0} (UTC time).", DateTime.UtcNow));
            sb.AppendLine("User message:");
            sb.AppendLine(UserMessage.Text);
            sb.AppendLine("-");
            sb.AppendLine("Exception:");
            sb.AppendLine(_e.ToString());
            sb.AppendLine("");
            sb.AppendLine("-");
            sb.AppendLine("System infos:");
            sb.AppendLine(_systemInfos);

            SendInformations.Text = sb.ToString();
        }
    }
}
