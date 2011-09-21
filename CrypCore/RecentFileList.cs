using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace Cryptool.Core
{
    public class RecentFileList
    {
        private static RecentFileList _recentFileList = null;
        private List<string> recentFiles = new List<string>();
        private string RegistryKey = "Software\\CrypTool2.0";
        private string valueKey = "recentFileList";

        public int ListLength { get; private set; }

        public delegate void ListChangedEventHandler(List<string> recentFiles);
        public event ListChangedEventHandler ListChanged;

        public static RecentFileList GetSingleton()
        {
            if (_recentFileList == null)
            {
                _recentFileList = new RecentFileList(Properties.Settings.Default.RecentFileListSize);
            }
            return _recentFileList;
        }

        private RecentFileList() : this(10)
        {            
        }

        public void ChangeListLength(int listLength)
        {
            Properties.Settings.Default.RecentFileListSize = listLength;
            Properties.Settings.Default.Save();
            ListLength = listLength;
            if (ListLength < recentFiles.Count)
            {
                recentFiles.RemoveRange(ListLength, recentFiles.Count - ListLength);
            }
            Store();
            ListChanged(recentFiles);
        }

        private RecentFileList(int listLength)
        {
            ListLength = listLength;
            Load();
        }

        public void AddRecentFile(string recentFile)
        {
            if (Path.GetFileName(recentFile).StartsWith("."))
                return;

            recentFiles.Remove(recentFile);
            recentFiles.Add(recentFile);
            if (recentFiles.Count > ListLength)
                recentFiles.RemoveAt(0);

            Store();
            ListChanged(recentFiles);
        }

        public void RemoveFile(string fileName)
        {
            recentFiles.Remove(fileName);
            Store();
            ListChanged(recentFiles);
        }

        public List<string> GetRecentFiles()
        {
            return recentFiles;
        }

        public void Clear()
        {
            recentFiles.Clear();
            ListChanged(recentFiles);
        }

        private void Store()
        {            
            RegistryKey k = Registry.CurrentUser.OpenSubKey(RegistryKey);
            if (k == null)
                k = Registry.CurrentUser.CreateSubKey(RegistryKey);
            k = Registry.CurrentUser.OpenSubKey(RegistryKey, true);

            k.SetValue(valueKey, recentFiles.ToArray());
        }

        private void Load()
        {
            RegistryKey k = Registry.CurrentUser.OpenSubKey(RegistryKey);
            if (k == null)
                k = Registry.CurrentUser.CreateSubKey(RegistryKey);

            if (k.GetValue(valueKey) != null && k.GetValueKind(valueKey) == RegistryValueKind.MultiString)
            {
                string[] list = (string[])(k.GetValue(valueKey));
                for (int i = list.Length-ListLength; i < list.Length; i++)
                {
                    if ((i >= 0) && File.Exists(list[i]))
                    {
                        recentFiles.Add(list[i]);
                    }
                }
            }
        }
    }
}
