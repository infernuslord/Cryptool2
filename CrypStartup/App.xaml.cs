using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Cryptool.Core;
using Cryptool.PluginBase.Miscellaneous;
using Application = System.Windows.Application;

namespace CrypStartup
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            System.Windows.Forms.Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                UnhandledExceptionDialog.ShowModalDialog((Exception)e.ExceptionObject, AssemblyHelper.Version, AssemblyHelper.InstallationType.ToString(), AssemblyHelper.BuildType.ToString(), AssemblyHelper.ProductName);
            }, null);
        }



        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            UnhandledExceptionDialog.ShowModalDialog(e.Exception, AssemblyHelper.Version, AssemblyHelper.InstallationType.ToString(), AssemblyHelper.BuildType.ToString(), AssemblyHelper.ProductName);
        }

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
