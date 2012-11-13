using System;

namespace Cryptool.Plugins.NetworkOutput
{
    public class PresentationPackage
    {
        public PresentationPackage()
        {
            this.TimeOfReceiving = DateTime.Now.ToString("HH:mm:ss:ffff");
        }

        public string TimeOfReceiving { get; set; }
        public string IPFrom { get; set; }
        public string Payload { get; set; }
    }

}
