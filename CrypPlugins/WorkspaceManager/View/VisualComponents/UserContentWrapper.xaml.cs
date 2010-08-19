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

namespace WorkspaceManager.View.VisualComponents
{
    /// <summary>
    /// Interaction logic for UserContentWrapper.xaml
    /// </summary>
    public partial class UserContentWrapper : UserControl
    {
        public List<ImageWrapper> ImageList { get; set; }
        public List<TextBox> TextList { get; set; }

        public UserContentWrapper()
        {
            InitializeComponent();
            (BottomBoxParent.Child as BottomBox).ImageSelected += new EventHandler<ImageSelectedEventArgs>(UserContentWrapper_ImageSelected);
            ImageList = new List<ImageWrapper>();
            TextList = new List<TextBox>();
        }

        void UserContentWrapper_ImageSelected(object sender, ImageSelectedEventArgs e)
        {
            AddImage(e.uri, new Point(0, 0));
        }

        public void AddImage(Uri imgUri, Point point)
        {
            ImageWrapper imgWrap = new ImageWrapper(imgUri, point);
            ImageList.Add(imgWrap);
            ContentRoot.Children.Add(imgWrap);
        }
    }
}
