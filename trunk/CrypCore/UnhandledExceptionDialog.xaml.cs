﻿using System;
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
            var reportErrorDialog = new ReportErrorDialog(_e, _version, _buildType, _productName);
            reportErrorDialog.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs routedEventArgs)
        {
            Close();
        }

        public static void ShowModalDialog(Exception e, Version version, string buildType, string productName)
        {
            var unhandledExceptionDialog = new UnhandledExceptionDialog(e, version, buildType, productName);
            unhandledExceptionDialog.ShowDialog();
        }
    }
}
