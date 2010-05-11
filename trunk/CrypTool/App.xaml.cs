using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading;
using Cryptool.Core;

namespace CrypTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            new Thread(LoadPlugins).Start();
        }

        private void LoadPlugins()
        {
            PluginManager pluginMgr = new PluginManager();
            pluginMgr.OnPluginLoaded += new CrypCorePluginLoadedHandler(OnPluginLoaded);
            Dictionary<string, Type> plugins = pluginMgr.LoadTypes(AssemblySigningRequirement.LoadAllAssemblies);
            OpenMainWindow(plugins);
        }

        private void OnPluginLoaded(object sender, PluginLoadedEventArgs args)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action<Object, PluginLoadedEventArgs>(OnPluginLoaded), sender, args);
            }
            else
            {
                SplashScreen screen = (SplashScreen)this.MainWindow;
                screen.Text = args.AssemblyName;
                screen.Value = args.CurrentPluginNumber == 0 ? 0 : (int)((double)args.CurrentPluginNumber / (double)args.NumberPluginsFound * 100);
            }
        }

        private void OpenMainWindow(Dictionary<string, Type> plugins)
        {

            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action<Dictionary<string, Type>>(OpenMainWindow), plugins);
            }
            else
            {
                SplashScreen screen = (SplashScreen)this.MainWindow;
                MainWindow wnd = new CrypTool.MainWindow();
                wnd.LoadedTypes = plugins;
                Application.Current.MainWindow = wnd;
                
                screen.Close();
                Application.Current.MainWindow.Show();
            }
        }
    }
}
