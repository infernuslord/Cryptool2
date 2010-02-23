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
using Cryptool.Plugins.MD5Collider.Algorithm;

namespace Cryptool.Plugins.MD5Collider.Presentation
{
    /// <summary>
    /// Interaktionslogik für QuickWatchPresentation.xaml
    /// </summary>
    public partial class QuickWatchPresentation : UserControl
    {
        public static DependencyProperty ColliderProperty = DependencyProperty.Register("Collider", typeof(IMD5ColliderAlgorithm), typeof(QuickWatchPresentation));
        public IMD5ColliderAlgorithm Collider
        {
            get { return (IMD5ColliderAlgorithm)GetValue(ColliderProperty); }
            set { SetValue(ColliderProperty, value); }
        }

        public QuickWatchPresentation()
        {
            InitializeComponent();
        }
    }
}
