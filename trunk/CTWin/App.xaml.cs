using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Cryptool.Core;
using System.Threading;
using System.Windows.Threading;

namespace CTWin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            new Thread(loadCrypTool).Start();
        }

        private void loadCrypTool()
        {
            PluginManager pluginManager = new PluginManager(null);
            pluginManager.OnPluginLoaded += new CrypCorePluginLoadedHandler(OnPluginLoaded);
            Dictionary<string, Type> loadedTypes = pluginManager.LoadTypes(AssemblySigningRequirement.LoadAllAssemblies);
            displayMainWindow(loadedTypes, pluginManager);
        }

        public void OnPluginLoaded(object sender, PluginLoadedEventArgs args)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((SplashScreen)MainWindow).HandleUpdate(args);
            }, null);
            //Thread.Sleep(100);
        }

        private void displayMainWindow(Dictionary<string, Type> loadedTypes, PluginManager pluginManager)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                MainWindow window = new MainWindow(loadedTypes, pluginManager);
                Application.Current.MainWindow.Close();
                Application.Current.MainWindow = window;
                Application.Current.MainWindow.Show();
            }, null);
        }
    }

}
