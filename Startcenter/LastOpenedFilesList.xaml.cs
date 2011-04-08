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
using Cryptool.PluginBase.Editor;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for LastOpenedFilesList.xaml
    /// </summary>
    public partial class LastOpenedFilesList : UserControl
    {
        public event OpenEditorHandler OnOpenEditor;
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

                recentFileInfos.Add(new RecentFileInfo() {File = rfile, Title = title, Icon = icon, Cte = cte});
            }
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = (RecentFileInfo)RecentFileListBox.SelectedItem;
            IEditor editor;
            if (selectedItem.Cte)
                editor = OnOpenEditor(typeof(AnotherEditor.AnotherEditor), null);
            else
                editor = OnOpenEditor(typeof(WorkspaceManager.WorkspaceManager), null);
            editor.Open(selectedItem.File);
        }
    }

    struct RecentFileInfo
    {
        public string File { get; set; }
        public string Title { get; set; }
        public ImageSource Icon { get; set; }
        public bool Cte { get; set; }
    }
}
