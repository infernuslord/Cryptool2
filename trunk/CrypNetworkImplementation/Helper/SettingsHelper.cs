using System;
using System.IO;
using Cryptool.PluginBase.IO;

namespace Cryptool.P2PDLL.Helper
{
    internal static class SettingsHelper
    {
        public static void ValidateSettings()
        {
            if (String.IsNullOrEmpty(P2P.P2PSettings.Default.WorkspacePath))
            {
                Directory.CreateDirectory(DirectoryHelper.DirectoryLocalTemp);
                P2P.P2PSettings.Default.WorkspacePath = DirectoryHelper.DirectoryLocalTemp;
                P2P.P2PSettings.Default.Save();
            }
        }
    }
}
