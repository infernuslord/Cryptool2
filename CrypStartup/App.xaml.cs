using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace CrypStartup
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Cryptool.CrypWin.MainWindow wnd = new Cryptool.CrypWin.MainWindow();
            wnd.Show();
        }
    }
}
