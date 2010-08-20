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
using WorkspaceManager.Model;

namespace WorkspaceManager.View.VisualComponents
{
    /// <summary>
    /// Interaction logic for UserContentWrapper.xaml
    /// </summary>
    public partial class UserContentWrapper : UserControl
    {
        public WorkspaceModel Model;
        public List<ImageWrapper> ImageList { get; set; }
        public List<TextBox> TextList { get; set; }

        public UserContentWrapper(WorkspaceModel WorkspaceModel)
        {
            InitializeComponent();
            (BottomBoxParent.Child as BottomBox).ImageSelected += new EventHandler<ImageSelectedEventArgs>(UserContentWrapper_ImageSelected);
            ImageList = new List<ImageWrapper>();
            TextList = new List<TextBox>();
            this.Model = WorkspaceModel;
            foreach (ImageModel ImageModel in WorkspaceModel.AllImageModels)
            {
                AddImage(ImageModel);
            }
        }

        void UserContentWrapper_ImageSelected(object sender, ImageSelectedEventArgs e)
        {
            AddImage(e.uri, new Point(0, 0));
        }

        public void AddImage(Uri imgUri, Point point)
        {
            ImageModel model = Model.newImageModel(imgUri);
            ImageWrapper imgWrap = new ImageWrapper(model, point);
            ImageList.Add(imgWrap);
            ContentRoot.Children.Add(imgWrap);
        }

        public void AddImage(ImageModel model)
        {
            ImageWrapper imgWrap = new ImageWrapper(model, model.Position);
            ImageList.Add(imgWrap);
            ContentRoot.Children.Add(imgWrap);
        }
    }
}
