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
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Tool;
using Cryptool.PluginBase.Generator;
using Cryptool.PluginBase.Cryptography;

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
            Dictionary<string, List<PluginInfo>> pluginInfos = new Dictionary<string, List<PluginInfo>>();
            foreach (Type pluginType in this.LoadedTypes.Values)
            {
                foreach (string interfaceName in PluginExtension.Interfaces)
                {
                    if (pluginType.GetInterface(interfaceName) != null)
                    {
                        if (!pluginInfos.ContainsKey(interfaceName))
                            pluginInfos.Add(interfaceName, new List<PluginInfo>());

                        PluginInfoAttribute attr = pluginType.GetPluginInfoAttribute();

                        PluginInfo p =new PluginInfo()
                        {
                            Caption = attr.Caption,
                            DescriptionURL = attr.DescriptionUrl,
                            Icon = pluginType.GetImage(0),
                            PluginType = pluginType,
                            EncryptionType = pluginType.GetEncryptionTypeAttribute()
                        };

                        pluginInfos[interfaceName].Add(p);

                        AddIt(p, interfaceName);
                        
                    }
                }
            }

        }

        private void AddIt(PluginInfo pluginInfo, string interfaceName)
        {
            Image img = pluginInfo.Icon;
            if (img != null)
            {
                img.Margin = new Thickness(2, 0, 5, 0);
                img.Height = 25;
                img.Width = 25;
                TextBlock textBlock = new TextBlock();
                textBlock.FontWeight = FontWeights.DemiBold;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.Text = pluginInfo.Caption;
                WrapPanel wrapPanel = new WrapPanel();
                wrapPanel.Margin = new Thickness(0, 2, 0, 2);
                wrapPanel.VerticalAlignment = VerticalAlignment.Center;
                wrapPanel.Children.Add(img);
                wrapPanel.Children.Add(textBlock);
                ListBoxItem navItem = new ListBoxItem();
                navItem.Content = wrapPanel;

                ListView view = GetListBox(pluginInfo, interfaceName);
                    if (view != null)
                    view.Items.Add(navItem);
            }
        }

        private ListView GetListBox(PluginInfo pluginInfo, string interfaceName)
        {
            if (interfaceName == typeof(ITool).FullName) 
                return this.ToolsStandalone;
            else if (interfaceName == typeof(IInput).FullName)
                return this.ToolsInput;
            else if (interfaceName == typeof(IOutput).FullName)
                return this.ToolsOutput;
            else if (interfaceName == typeof(IIOMisc).FullName)
                return this.ToolsMisc;
            else if (interfaceName == typeof(IThroughput).FullName)
                return this.ToolsThroughput;
            else if (interfaceName == typeof(IGeneratorMisc).FullName)
                return this.ToolsGenerators;
            else if (interfaceName == typeof(IKeyGenerator).FullName)
                return this.ToolsGenerators;
            else if (interfaceName == typeof(IRandomNumberGenerator).FullName)
                return this.ToolsGenerators;
            else if (interfaceName == typeof(IEncryption).FullName)
            {
                if (pluginInfo.EncryptionType != null)
                {
                    switch (pluginInfo.EncryptionType.EncryptionType)
                    {
                        case EncryptionType.Asymmetric:
                            return this.ModernAsymmetric;
                        case EncryptionType.Classic:
                            return this.ClassicCiphers;   
                        case EncryptionType.SymmetricBlock:
                            return ModernSymmetric;
                        case EncryptionType.SymmetricStream:
                            return ModernSymmetric;
                        case EncryptionType.Hybrid:
                            return ModernHybrid;
                    }
                }
            }
            else if (interfaceName == typeof(ICryptographyMisc).FullName)
                return this.ModernMisc;
            else if (interfaceName == typeof(ICryptographicHash).FullName)
                return this.HashCryptographic;
            else if (interfaceName == typeof(ICheckSumHash).FullName)
                return this.HashChecksum;

            
            
            return null;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPlugins();
        }
    }

    public class PluginInfo
    {
        public string Caption { get; set; }
        public string DescriptionURL { get; set; }
        public Image Icon { get; set; }
        public Type PluginType { get; set; }
        public EncryptionTypeAttribute EncryptionType { get; set; }
    }
}
