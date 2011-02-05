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
using Cryptool.Core;
using CTWin.Components.Misc;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.Generator;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Tool;
using Cryptool.PluginBase.Editor;

namespace CTWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region CryptCore Related
        private Dictionary<string, Type> loadedTypes;
        public Dictionary<string, Type> LoadedTypes
        { 
            get 
            {
                return loadedTypes;
            } 
            set 
            {
                loadedTypes = value;
            } 
        }

        private PluginManager pluginManager;
        public PluginManager PluginManager 
        {
            get 
            {
                return pluginManager;
            }
            set 
            {
                pluginManager = value;
            }
        }
        #endregion

        #region DataModel
        public DataModel Model { get; private set; }
        #endregion

        #region Properties
        public Dictionary<string, List<PluginInfo>> PluginInfoCollection { get; set; }
        #endregion

        #region Constructor
        public MainWindow(Dictionary<string, Type> loadedTypes, PluginManager pluginManager)
        {
            InitializeComponent();
            this.LoadedTypes = loadedTypes;
            this.PluginManager = pluginManager;
            this.Model = new DataModel(this);
            this.PluginInfoCollection = loadPlugins();
        }
        #endregion
        #region private
        private Dictionary<string, List<PluginInfo>> loadPlugins()
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

                        PluginInfo p = new PluginInfo()
                        {
                            Model = this.Model.GetPluginInfoModel(),
                            Caption = attr.Caption,
                            DescriptionURL = attr.DescriptionUrl,
                            Icon = pluginType.GetImage(0),
                            PluginType = pluginType,
                            EncryptionType = pluginType.GetEncryptionTypeAttribute(),
                            Category = PluginInfo.GetName(interfaceName),
                        };

                        pluginInfos[interfaceName].Add(p);

                        if (interfaceName == typeof(Cryptool.PluginBase.Editor.IEditor).FullName)
                        {
                            if (pluginType.Name == "Wizard")
                            {
                                this.PlaceHolderEditor = (IEditor)Activator.CreateInstance(pluginType);
                                TabItem item = new TabItem();
                                item.Content = (object)PlaceHolderEditor.Presentation;
                                this.PlaceHolderTabControl.Items.Add(item);
                            }

                        }
                    }
                }
            }
            return pluginInfos;
        }
        #endregion

        public IEditor PlaceHolderEditor { get; set; }
    }

    public class PluginInfo
    {
        public string Category { get; set; }
        public ChildDataModel Model { get; set; }
        public string Caption { get; set; }
        public string DescriptionURL { get; set; }
        public Image Icon { get; set; }
        public Type PluginType { get; set; }
        public EncryptionTypeAttribute EncryptionType { get; set; }

        public static string GetName(string interfaceName)
        {
            if (interfaceName == typeof(ITool).FullName) 
                return "Tools Standalone";
            else if (interfaceName == typeof(IInput).FullName)
                return "Tools Input";
            else if (interfaceName == typeof(IOutput).FullName)
                return "Tools Output";
            else if (interfaceName == typeof(IIOMisc).FullName)
                return "Tools Misc";
            else if (interfaceName == typeof(IThroughput).FullName)
                return "Tools Throughput";
            else if (interfaceName == typeof(IGeneratorMisc).FullName)
                return "Tools Generators";
            else if (interfaceName == typeof(IKeyGenerator).FullName)
                return "Tools Generators";
            else if (interfaceName == typeof(IRandomNumberGenerator).FullName)
                return "Tools Generators";
            else if (interfaceName == typeof(IEncryption).FullName)
            {
                //switch (encryptionTypeAttribute.EncryptionType)
                //{
                //    case EncryptionType.Asymmetric:
                //        return this.ModernAsymmetric;
                //    case EncryptionType.Classic:
                //        return this.ClassicCiphers;   
                //    case EncryptionType.SymmetricBlock:
                //        return ModernSymmetric;
                //    case EncryptionType.SymmetricStream:
                //        return ModernSymmetric;
                //    case EncryptionType.Hybrid:
                //        return ModernHybrid;
                //}
                return "Encryption";
            }
            else if (interfaceName == typeof(ICryptographyMisc).FullName)
                return "Modern Misc";
            else if (interfaceName == typeof(ICryptographicHash).FullName)
                return "Hash Cryptographic";
            else if (interfaceName == typeof(ICheckSumHash).FullName)
                return "Hash Checksum";
            return null;
        }
    }
}
