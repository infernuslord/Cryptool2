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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Editor;
using StartCenter;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for Startcenter.xaml
    /// </summary>
    public partial class Startcenter : UserControl
    {
        public string TemplatesDir
        {
            set 
            {
                ((Panels)panels.Children[0]).TemplatesDir = value;
            }
        }

        public event OpenEditorHandler OnOpenEditor;
        public event OpenTabHandler OnOpenTab;
        public event StartcenterEditor.StartupBehaviourChangedHandler StartupBehaviourChanged;

        public Startcenter()
        {
            InitializeComponent();
            ((Buttons)buttons.Content).OnOpenEditor += (content, title) => OnOpenEditor(content, title);
            ((Panels)panels.Children[0]).OnOpenEditor += (content, title) => OnOpenEditor(content, title);
            ((Panels)panels.Children[0]).OnOpenTab += (content, title, parent) => OnOpenTab(content, title, parent);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (StartupBehaviourChanged != null && StartupCheckbox.IsChecked.HasValue)
                StartupBehaviourChanged(!StartupCheckbox.IsChecked.Value);
        }
    }
}
