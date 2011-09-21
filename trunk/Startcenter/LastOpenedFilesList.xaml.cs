﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Cryptool.Core;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Editor;
using Path = System.IO.Path;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for LastOpenedFilesList.xaml
    /// </summary>
    public partial class LastOpenedFilesList : UserControl
    {
        public event OpenEditorHandler OnOpenEditor;
        private readonly List<RecentFileInfo> _recentFileInfos = new List<RecentFileInfo>();
        private readonly RecentFileList _recentFileList = RecentFileList.GetSingleton();

        public LastOpenedFilesList()
        {
            ReadRecentFileList();
            InitializeComponent();
            RecentFileListBox.DataContext = _recentFileInfos;

            _recentFileList.ListChanged += delegate
                                               {
                                                   try
                                                   {
                                                       ReadRecentFileList();
                                                       RecentFileListBox.DataContext = null;
                                                       RecentFileListBox.DataContext = _recentFileInfos;
                                                   }
                                                   catch (Exception)
                                                   {
                                                       //Not critical.. Do nothing
                                                   }
                                               };
        }

        private void ReadRecentFileList()
        {
            var recentFiles = _recentFileList.GetRecentFiles();
            _recentFileInfos.Clear();

            foreach (var rfile in recentFiles)
            {
                var file = new FileInfo(rfile);
                var fileExt = file.Extension.ToLower().Substring(1);
                if (ComponentInformations.EditorExtension != null && ComponentInformations.EditorExtension.ContainsKey(fileExt))
                {
                    bool cte = (fileExt == "cte");
                    Type editorType = ComponentInformations.EditorExtension[fileExt];
                    string xmlFile = Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + ".xml");
                    string iconFile = null;
                    Inline description = null;
                    string title = null;

                    if (File.Exists(xmlFile))
                    {
                        try
                        {
                            XElement xml = XElement.Load(xmlFile);
                            var titleElement = Helper.GetGlobalizedElementFromXML(xml, "title");
                            if (titleElement != null)
                                title = titleElement.Value;

                            var descriptionElement = Helper.GetGlobalizedElementFromXML(xml, "description");
                            if (descriptionElement != null)
                            {
                                description = Helper.ConvertFormattedXElement(descriptionElement);
                            }

                            if (xml.Element("icon") != null && xml.Element("icon").Attribute("file") != null)
                                iconFile = Path.Combine(file.Directory.FullName, xml.Element("icon").Attribute("file").Value);
                        }
                        catch (Exception)
                        {
                            //we do nothing if the loading of an description xml fails => this is not a hard error
                        }
                    }

                    if ((title == null) || (title.Trim() == ""))
                    {
                        title = Path.GetFileNameWithoutExtension(file.Name).Replace("-", " ").Replace("_", " ");
                    }
                    if (description == null)
                    {
                        string desc;
                        if (cte)
                            desc = Properties.Resources.This_is_an_AnotherEditor_file_;
                        else
                            desc = Properties.Resources.This_is_a_WorkspaceManager_file_;
                        description = new Run(desc);
                    }

                    if (iconFile == null || !File.Exists(iconFile))
                        iconFile = Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + ".png");
                    var image = File.Exists(iconFile) ? new BitmapImage(new Uri(iconFile)) : editorType.GetImage(0).Source;
                    
                    _recentFileInfos.Add(new RecentFileInfo()
                        {
                            File = rfile,
                            Title = title,
                            Description = new TextBlock(description) { TextWrapping = TextWrapping.Wrap, MaxWidth = 400},
                            Icon = image, 
                            EditorType = editorType
                        });
                }
            }
            _recentFileInfos.Reverse();
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = (RecentFileInfo)RecentFileListBox.SelectedItem;
            IEditor editor = OnOpenEditor(selectedItem.EditorType, null, null);
            editor.Open(selectedItem.File);
            _recentFileList.AddRecentFile(selectedItem.File);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _recentFileList.Clear();
        }
    }

    struct RecentFileInfo
    {
        public string File { get; set; }
        public string Title { get; set; }
        public TextBlock Description { get; set; }
        public ImageSource Icon { get; set; }
        public Type EditorType { get; set; }
    }
}
