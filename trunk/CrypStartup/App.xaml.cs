﻿using System;
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
            long x, y;
            x = DateTime.Now.Ticks;
            Cryptool.CrypWin.MainWindow wnd = new Cryptool.CrypWin.MainWindow();
            try
            {
                wnd.Show();
            }
            catch (Exception)
            {
                //This window has already been closed
            }
            y = DateTime.Now.Ticks - x;
            Console.WriteLine(y.ToString());
        }
    }
}
