using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cryptool.Plugins.PeerToPeer
{
    public static class DebugToFile
    {
        private static StreamWriter sw;
        private static string sPath = @"c:\p2p_debug";
        private static string sFileName = "p2p_debug";
        private static string sDateTime = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString()
            + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString()
            + DateTime.Now.Minute.ToString();

        public static string Path 
        {
            get { return sPath; }
            private set 
            { 
                // last character must not be a Backslash!
                if (value.Substring(value.Length - 1, 1) == @"\")
                    sPath = value.Substring(0, value.Length - 2);
                else
                    sPath = value;
            }
        }

        public static string FileName
        {
            get { return sFileName; }
            private set 
            {
                if (value.Substring(value.Length - 3, 1) == ".")
                    sFileName = value.Substring(0, value.Length - 3);
                else
                    sFileName = value;
            }
        }

        public static StreamWriter GetDebugStreamWriter(string path, string filename)
        {
            Path = path;
            FileName = filename;
            return GetDebugStreamWriter();
        }

        public static StreamWriter GetDebugStreamWriter()
        {
            if (sw != null)
                Dispose();


            if (!Directory.Exists(sPath))
                Directory.CreateDirectory(sPath);
            int i = 1;
            while (File.Exists(sPath + @"\" + sFileName + "_" + sDateTime + ".txt"))
            {
                FileName += FileName + "_" + i;
                i++;
            }
            FileInfo fileInfo = new FileInfo(Path + @"\" + FileName + "_" + sDateTime + ".txt");
            sw = fileInfo.CreateText();
            return sw;
        }

        public static void Dispose()
        {
            if (sw != null)
            {
                try
                {
                    sw.Flush();
                }
                catch (Exception ex)
                { }
                sw.Close();
                sw.Dispose();
                sw = null;
            }
        }
    }
}
