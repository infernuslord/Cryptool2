using System;
using System.Collections.Generic;
using System.IO;
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

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for LastOpenedFilesList.xaml
    /// </summary>
    public partial class LastOpenedFilesList : UserControl
    {
        private List<RecentFileInfo> recentFileInfos = new List<RecentFileInfo>();

        public LastOpenedFilesList()
        {
            ReadRecentFileList();
            InitializeComponent();
            RecentFileListBox.DataContext = recentFileInfos;
        }

        private void ReadRecentFileList()
        {
            var recentFileList = new Cryptool.Core.RecentFileList();
            foreach (var rfile in recentFileList.GetRecentFiles())
            {
                var file = new FileInfo(rfile);
                bool cte = (file.Extension.ToLower() == ".cte");
                var title = file.Name.Remove(file.Name.Length - 4).Replace("-", " ").Replace("_", " ");
                Type editorType = cte ? typeof(AnotherEditor.AnotherEditor) : typeof(WorkspaceManager.WorkspaceManager);
                var icon = editorType.GetImage(0).Source;

                recentFileInfos.Add(new RecentFileInfo() {File = rfile, Title = title, Icon = icon});
            }
        }
    }

    struct RecentFileInfo
    {
        public string File { get; set; }
        public string Title { get; set; }
        public ImageSource Icon { get; set; }
    }
}
