using System.Collections.Generic;
using Microsoft.Win32;

namespace Cryptool.Core
{
    public class RecentFileList
    {
        private static RecentFileList _recentFileList = null;
        private List<string> recentFiles = new List<string>();
        private string RegistryKey = "Software\\CrypTool2.0";
        private string valueKey = "recentFileList";

        public int ListLength { get; set; }

        public delegate void ListChangedEventHandler(List<string> recentFiles);
        public event ListChangedEventHandler ListChanged;

        public static RecentFileList GetSingleton()
        {
            if (_recentFileList == null)
            {
                _recentFileList = new RecentFileList();
            }
            return _recentFileList;
        }

        private RecentFileList() : this(10)
        {            
        }

        private RecentFileList(int listLength)
        {
            ListLength = listLength;
            Load();
        }

        public void AddRecentFile(string recentFile)
        {
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
                recentFiles = new List<string>(list);
            }
        }
    }
}
