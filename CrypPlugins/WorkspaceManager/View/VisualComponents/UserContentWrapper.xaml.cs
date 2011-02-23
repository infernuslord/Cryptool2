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
using WorkspaceManager.View.Interface;
using WorkspaceManagerModel.Model.Operations;
using WorkspaceManagerModel.Model.Interfaces;

namespace WorkspaceManager.View.VisualComponents
{

    /// <summary>
    /// Interaction logic for UserContentWrapper.xaml
    /// </summary>
    public partial class UserContentWrapper : UserControl, IUpdateableView
    {
        public WorkspaceModel Model;
        public List<ImageWrapper> ImageList { get; set; }
        public List<TextInputWrapper> TextInputList { get; set; }

        private UserControl selectedItem;
        public UserControl SelectedItem 
        { 
            get
            {
                return selectedItem;
            }
            set
            {
                selectedItem = value;
                foreach (ImageWrapper img in ImageList)
                {
                    if (img == selectedItem)
                        continue;

                    img.IsSelected = false;
                }

                foreach (TextInputWrapper txt in TextInputList)
                {
                    if (txt == selectedItem)
                        continue;

                    txt.IsSelected = false;
                }
            } 
        }

        public UserContentWrapper(WorkspaceModel WorkspaceModel, BottomBox Box)
        {
            InitializeComponent();
            Box.ImageSelected += new EventHandler<ImageSelectedEventArgs>(UserContentWrapper_ImageSelected);
            Box.AddText += new EventHandler<AddTextEventArgs>(UserContentWrapper_AddText);
            ImageList = new List<ImageWrapper>();
            TextInputList = new List<TextInputWrapper>();
            Model = WorkspaceModel;
            foreach (ImageModel ImageModel in WorkspaceModel.GetAllImageModels())
            {
                AddImage(ImageModel);
            }

            foreach (TextModel TextModel in WorkspaceModel.GetAllTextModels())
            {
                AddText(TextModel);
            }
        }

        void UserContentWrapper_AddText(object sender, AddTextEventArgs e)
        {
            AddText(new Point(50, 50));
        }

        void UserContentWrapper_ImageSelected(object sender, ImageSelectedEventArgs e)
        {
            AddImage(e.uri, new Point(0, 0));
        }

        public void AddImage(Uri imgUri, Point point)
        {
            try
            {
                ImageModel model = (ImageModel)Model.ModifyModel(new NewImageModelOperation(imgUri));                
                ImageWrapper imgWrap = new ImageWrapper(model, point, this);
                model.UpdateableView = imgWrap;
                imgWrap.Delete += new EventHandler<ImageDeleteEventArgs>(imgWrap_Delete);
                ImageList.Add(imgWrap);
                ContentRoot.Children.Add(imgWrap);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.ToString());
            }
        }

        public void AddImage(ImageModel model)
        {
            ImageWrapper imgWrap = new ImageWrapper(model, model.GetPosition(), this);
            model.UpdateableView = imgWrap;
            imgWrap.Delete += new EventHandler<ImageDeleteEventArgs>(imgWrap_Delete);
            ImageList.Add(imgWrap);
            ContentRoot.Children.Add(imgWrap);
        }

        public void AddText(Point point)
        {
            TextModel model = (TextModel)Model.ModifyModel(new NewTextModelOperation());            
            TextInputWrapper txtWrap = new TextInputWrapper(model, point, this);
            model.UpdateableView = txtWrap;
            txtWrap.Delete += new EventHandler<TextInputDeleteEventArgs>(txtWrap_Delete);
            TextInputList.Add(txtWrap);
            ContentRoot.Children.Add(txtWrap);
        }

        public void AddText(TextModel model)
        {
            TextInputWrapper txtWrap = new TextInputWrapper(model, model.GetPosition(), this);
            model.UpdateableView = txtWrap;
            txtWrap.Delete += new EventHandler<TextInputDeleteEventArgs>(txtWrap_Delete);
            TextInputList.Add(txtWrap);
            ContentRoot.Children.Add(txtWrap);
        }

        void imgWrap_Delete(object sender, ImageDeleteEventArgs e)
        {
            ContentRoot.Children.Remove(e.img);
            ImageList.Remove(e.img);
            Model.ModifyModel(new DeleteImageModelOperation(e.img.Model));
        }

        void txtWrap_Delete(object sender, TextInputDeleteEventArgs e)
        {
            ContentRoot.Children.Remove(e.txt);
            TextInputList.Remove(e.txt);
            Model.ModifyModel(new DeleteTextModelOperation(e.txt.Model));
        }

        #region UpdateableView Members

        public void update()
        {
        }

        #endregion
    }
}
