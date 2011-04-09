using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for CTTreeViewItem.xaml
    /// </summary>
    public partial class CTTreeViewItem : TreeViewItem
    {
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
            "Icon",
            typeof(ImageSource),
            typeof(CTTreeViewItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, null));

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set
            {
                SetValue(IconProperty, value);
            }
        }

        public static readonly DependencyProperty IsDirectoryProperty =
            DependencyProperty.Register(
            "IsDirectory",
            typeof(bool),
            typeof(CTTreeViewItem),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, null));

        public bool IsDirectory
        {
            get { return (bool)GetValue(IsDirectoryProperty); }
            set
            {
                SetValue(IsDirectoryProperty, value);
            }
        }

        public static readonly DependencyProperty DirectoryProperty =
            DependencyProperty.Register(
            "Directory",
            typeof(DirectoryInfo),
            typeof(CTTreeViewItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, null));

        public DirectoryInfo Directory
        {
            get { return (DirectoryInfo)GetValue(DirectoryProperty); }
            set
            {
                SetValue(DirectoryProperty, value);
            }
        }
        
        public FileInfo File { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public CTTreeViewItem(FileInfo file, string title, string description, ImageSource image)
        {
            this.File = file;
            this.Title = title;
            this.Description = description;
            this.Icon = image;
            this.Tag = new KeyValuePair<string, string>(file.FullName, title);
            InitializeComponent();
        }

        public CTTreeViewItem(string title, bool isDirectory)
        {
            this.Title = title;
            this.IsDirectory = isDirectory;
            this.Icon = new BitmapImage(new Uri("pack://application:,,,/CrypWIn;component/images/Open32.png"));
            InitializeComponent();
        }
    }
}
