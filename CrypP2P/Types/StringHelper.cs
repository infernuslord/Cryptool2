using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Cryptool.P2P.Types
{
    public static class StringHelper
    {
        /// <summary>
        /// Encrypts the given string using the current windows user password and converts
        /// this to a base64 string
        /// </summary>
        /// <param name="s"></param>
        /// <returns>encrypted base64 string</returns>
        public static string EncryptString(string s)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(s);
            byte[] encBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encBytes);
        }

        /// <summary>
        /// Decrypts the given base64 string using the current windows user password
        /// </summary>
        /// <param name="s"></param>
        /// <returns>decrypted string</returns>
        public static string DecryptString(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "";
            }
            try
            {
                byte[] encBytes = Convert.FromBase64String(s);
                byte[] bytes = ProtectedData.Unprotect(encBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.Unicode.GetString(bytes);
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
