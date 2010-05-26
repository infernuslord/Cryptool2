using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.IO;

namespace QuadraticSieve
{
    class PeerToPeer
    {
        /// <summary>
        /// Compresses the yield and puts it in the DHT.
        /// Returns the compressed size
        /// </summary>
        internal int put(byte[] serializedYield)
        {
            MemoryStream memStream = new MemoryStream();
            DeflateStream defStream = new DeflateStream(memStream, CompressionMode.Compress);
            defStream.Write(serializedYield, 0, serializedYield.Length);
            byte[] compressedYield = memStream.ToArray();



            return compressedYield.Length;
        }
    }
}
