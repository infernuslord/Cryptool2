using System;
using System.IO;

namespace Cryptool.P2P.Helper
{
    internal static class SettingsHelper
    {
        public static void ValidateSettings()
        {
            if (String.IsNullOrEmpty(P2PSettings.Default.WorkspacePath))
            {
                var tempForUser = Path.Combine(Path.GetTempPath(), "CrypTool2");
                Directory.CreateDirectory(tempForUser);
                P2PSettings.Default.WorkspacePath = tempForUser;
                P2PSettings.Default.Save();
            }
        }
    }
}
