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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Odyssey.Controls;
using Odyssey.Controls.Classes;

namespace CrypTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        private Dictionary<string, List<Type>> currentPlugins = new Dictionary<string, List<Type>>();

        public Dictionary<string, Type> LoadedTypes { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {
            if (SkinManager.SkinId == SkinId.OfficeBlue)
                SkinManager.SkinId = SkinId.OfficeBlack;
            else
                SkinManager.SkinId = SkinId.OfficeBlue;
        }

        private void LoadPlugins()
        {
            //foreach (Type pluginType in this.LoadedTypes.Values)
            //{
            //    if (pluginType.GetEditorSpecificPluginAttribute() != null)
            //        continue;

            //    foreach (string interfaceName in PluginExtension.Interfaces)
            //    {
            //        if (pluginType.GetInterface(interfaceName) != null)
            //        {
            //            if (!this.currentPlugins.ContainsKey(interfaceName))
            //                this.currentPlugins.Add(interfaceName, new List<Type>());

            //            this.currentPlugins[interfaceName].Add(pluginType);
            //        }
            //    }
            //}
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPlugins();
        }
    }
}
