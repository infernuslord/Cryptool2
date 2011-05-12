using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Cryptool.Core
{
    public static class Mailer
    {
        public const int MINIMUM_DIFF = 5;

        public const string ACTION_DEVMAIL = "DEVMAIL"; // server will send mail to coredevs
        public const string ACTION_TICKET = "TICKET"; // server will create a trac ticket (and send a mail to coredevs)

        private static DateTime lastMailTime;

        /// <summary>
        /// Send mail to developers via CT2 DEVMAIL web interface.
        /// Server-side spam protection is not handled differently from other server errors.
        /// </summary>
        /// <exception cref="SpamException">Thrown when client-side spam protection triggers</exception>
        /// <param name="title">Subject (without any "CrypTool" prefixes, will be added at server-side)</param>
        /// <param name="text">Message body</param>
        public static void SendMailToCoreDevs(string action, string title, string text)
        {
            // Client-side spam check. Will fail if client changes system time.
            TimeSpan diff = DateTime.Now - lastMailTime;
            if (diff < TimeSpan.FromSeconds(MINIMUM_DIFF))
            {
                // +1 to avoid confusing "0 seconds" text message
                throw new SpamException(string.Format("Please wait {0} seconds before trying again", Math.Round(MINIMUM_DIFF - diff.TotalSeconds + 1)));
            }

            var client = new WebClient();
            client.Headers["User-Agent"] = "CrypTool";
            var stream = client.OpenWrite("http://www.cryptool.org/cgi/ct2devmail");

            var postMessage = Encoding.ASCII.GetBytes(string.Format("action={0}&title={1}&text={2}", Uri.EscapeDataString(action), Uri.EscapeDataString(title), Uri.EscapeDataString(text)));
            stream.Write(postMessage, 0, postMessage.Length);
            stream.Close();

            lastMailTime = DateTime.Now;
        }

        public class SpamException : Exception
        {
            public SpamException(string message) : base(message)
            {
            }
        }
    }
}
