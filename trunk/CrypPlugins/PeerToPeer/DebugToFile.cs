using System;
using System.IO;
using System.Text;

namespace Cryptool.Plugins.PeerToPeer.Internal
{
    public static class DebugToFile
    {
        private static StreamWriter sw;
        private static string sPath = @"c:\p2p_debug";
        private static string sFileName = "p2p_debug";

        private static readonly string sDateTime = DateTime.Now.Year + DateTime.Now.Month.ToString()
                                                   + DateTime.Now.Day + "_" + DateTime.Now.Hour
                                                   + DateTime.Now.Minute;

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
            var fileInfo = new FileInfo(Path + @"\" + FileName + "_" + sDateTime + ".txt");
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
                catch (Exception)
                {
                }
                sw.Close();
                sw.Dispose();
                sw = null;
            }
        }

        // method only necessary for evaluation issues
        public static string GetTimeStamp()
        {
            DateTime now = DateTime.Now;
            var sbTimeStamp = new StringBuilder();

            if (now.Hour <= 9)
                sbTimeStamp.Append("0");
            sbTimeStamp.Append(now.Hour + ".");
            if (now.Minute <= 9)
                sbTimeStamp.Append("0");
            sbTimeStamp.Append(now.Minute + ".");
            if (now.Second <= 9)
                sbTimeStamp.Append("0");
            sbTimeStamp.Append(now.Second + ":");
            if (now.Millisecond <= 9)
                sbTimeStamp.Append("00");
            else if (now.Millisecond <= 99)
                sbTimeStamp.Append("0");
            sbTimeStamp.Append(now.Millisecond.ToString());

            return sbTimeStamp.ToString();
        }
    }
}