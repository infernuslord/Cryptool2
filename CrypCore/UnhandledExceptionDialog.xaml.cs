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
using System.Windows.Shapes;

namespace Cryptool.Core
{
    /// <summary>
    /// Interaction logic for UnhandledExceptionDialog.xaml
    /// </summary>
    public partial class UnhandledExceptionDialog : Window
    {
        public UnhandledExceptionDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not implemented yet!");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void ShowModalDialog(Exception e)
        {
            var unhandledExceptionDialog = new UnhandledExceptionDialog();
            unhandledExceptionDialog.ExceptionNameLabel.Content = e.GetType().FullName;
            unhandledExceptionDialog.ExceptionMessageLabel.Content = e.Message;
            unhandledExceptionDialog.StackTraceBox.Text = e.StackTrace;
            unhandledExceptionDialog.ShowDialog();
        }
    }
}
