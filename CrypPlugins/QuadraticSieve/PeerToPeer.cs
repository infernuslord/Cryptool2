using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.IO;
using Cryptool.P2P;
using System.Numerics;
using System.Diagnostics;

namespace Cryptool.Plugins.QuadraticSieve
{
    class PeerToPeer
    {
        private string channel;
        private BigInteger number;
        private BigInteger factor;
        private int head;

        private byte[] ReadYield(int index)
        {
            byte[] yield = P2PManager.Retrieve(channel + "#" + factor + "!" + index);
            if (yield == null)
                return null;

            MemoryStream memStream = new MemoryStream();
            DeflateStream defStream = new DeflateStream(memStream, CompressionMode.Decompress);
            defStream.Read(yield, 0, yield.Length);
            byte[] decompressedYield = memStream.ToArray();

            return decompressedYield;
        }

        /// <summary>
        /// Compresses the yield and puts it in the DHT.
        /// Returns the compressed size
        /// </summary>
        public int Put(byte[] serializedYield)
        {
            MemoryStream memStream = new MemoryStream();
            DeflateStream defStream = new DeflateStream(memStream, CompressionMode.Compress);
            defStream.Write(serializedYield, 0, serializedYield.Length);
            byte[] compressedYield = memStream.ToArray();

            P2PManager.Store(channel + "#" + factor + "!" + head, compressedYield);
            head++;
            P2PManager.Store(channel + "#" + factor + "HEAD:" + head, head.ToString());

            return compressedYield.Length;
        }

        /// <summary>
        /// Sets the channel in which we want to sieve
        /// </summary>
        public void SetChannel(string channel)
        {
            this.channel = channel;
        }

        /// <summary>
        /// Sets the number to sieve
        /// </summary>
        public void SetNumber(BigInteger number)
        {
            this.number = number;
        }

        /// <summary>
        /// Sets the factor to sieve next
        /// </summary>
        public void SetFactor(BigInteger factor)
        {
            this.factor = factor;
            Debug.Assert(this.number % this.factor == 0);

            byte[] h = P2PManager.Retrieve(channel + "#" + factor + "HEAD:" + head);
            if (h != null)
                head = int.Parse(h.ToString());
            else
                head = 0;
        }        

        /// <summary>
        /// Synchronizes the factorManager with the DHT.
        /// Return false if this.factor is not a composite factor in the DHT (which means, that another peer already finished sieving this.factor).
        /// </summary>
        public bool SyncFactorManager(FactorManager factorManager)
        {
            FactorManager dhtFactorManager = new FactorManager(null, null);
            //TODO: add factors from dht to dhtFactorManager here.

            if (dhtFactorManager == null || factorManager.Synchronize(dhtFactorManager))
            {
                //TODO: store factorManager to DHT
            }

            return factorManager.ContainsComposite(this.factor);
        }
    }
}
