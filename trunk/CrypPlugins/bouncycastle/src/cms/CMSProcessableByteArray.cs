using System;
using System.IO;

namespace Org.BouncyCastle.Cms
{
    /**
    * a holding class for a byte array of data to be processed.
    */
    public class CmsProcessableByteArray
        :    CmsProcessable
    {
        private byte[]  bytes;

        public CmsProcessableByteArray(
            byte[]  bytes)
        {
            this.bytes = bytes;
        }

        public void Write(Stream zOut)
        {
            zOut.Write(bytes, 0, bytes.Length);
        }

        public object GetContent()
        {
            return bytes.Clone();
        }
    }
}
