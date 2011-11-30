using System;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Management;

namespace Cryptool.P2P
{
    public class UniqueIdentifier
    {
        private static Int64? id = null;

        /// <summary>
        /// Returns a globally unique identifier for a user on a computer.
        /// </summary>
        /// <returns>Unique ID</returns>
        /// <exception cref="Exception">This method can throw any kind of exception.</exception>
        public static Int64 GetID()
        {
            if (id.HasValue)
                return id.Value;

            string username = WindowsIdentity.GetCurrent().Name;

            ManagementClass man = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = man.GetInstances();
            string cpuids = "";
            foreach (ManagementObject mob in moc)
            {
                var cpuid = mob.Properties["processorID"].Value;
                if (cpuid != null)
                {
                    cpuids += cpuid.ToString();
                }
            }


            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] idBytes = md5.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(username + cpuids));
            long ID = BitConverter.ToInt64(idBytes, 0);
            ID = Math.Abs(ID);

            id = ID;

            return ID;
        }

        /// <summary>
        /// Returns a globally unique identifier for an external client connected
        /// to this computer.
        /// </summary>
        /// <param name="externalClient"></param>
        /// <returns>Unique ID</returns>
        /// <exception cref="Exception">This method can throw any kind of exception.</exception>
        public static Int64 GetID(String externalClient)
        {
            Int64 localId = GetID();

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] idBytes = md5.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(externalClient + localId));
            long ID = BitConverter.ToInt64(idBytes, 0);
            ID = Math.Abs(ID);

            return ID;
        }

        /// <summary>
        /// Returns an identifier that depends on the MAC addresses of this system
        /// </summary>
        /// <exception cref="Exception">This method can throw any kind of exception.</exception>
        private static string GetMacIdentifier()
        {
            string MacID = "";
            ManagementClass MC = new ManagementClass("Win32_NetworkAdapter");
            ManagementObjectCollection MOCol = MC.GetInstances();
            foreach (ManagementObject MO in MOCol)
                if (MO != null)
                    if (MO["MacAddress"] != null)
                        MacID += MO["MACAddress"].ToString();
            return MacID;
        }

        /// <summary>
        /// Returns the (not unique) host name of this computer.
        /// </summary>
        /// <returns></returns>
        public static string GetHostName()
        {
            return System.Net.Dns.GetHostName();
        }
    }
}
