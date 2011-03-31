using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Cryptool.Core
{
    public static class Mailer
    {
        public static void SendMailToCoreDevs(string title, string text)
        {
            var client = new WebClient();
            client.Headers["User-Agent"] = "CrypTool";
            var stream = client.OpenWrite("http://www.cryptool.org/cgi/ct2devmail");

            var postMessage = Encoding.ASCII.GetBytes(string.Format("title={0}&text={1}", Uri.EscapeDataString(title), Uri.EscapeDataString(text)));
            stream.Write(postMessage, 0, postMessage.Length);
            stream.Close();
        }
    }
}
