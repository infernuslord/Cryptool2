/*                              
   Copyright 2010 Sven Rech, Uni Duisburg-Essen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.IO;
using Cryptool.P2P;
using System.Numerics;
using System.Diagnostics;
using System.Collections;
using System.Threading;

namespace Cryptool.Plugins.QuadraticSieve
{
    class PeerToPeer
    {
        private string channel;
        private BigInteger number;
        private BigInteger factor;
        private int head;
        private Queue storequeue;   //yields to store in the DHT
        private Queue loadqueue;    //yields that have been loaded from the DHT
        private Thread loadStoreThread;
        private bool stopLoadStoreThread;

        private byte[] ReadYield(int index)
        {
            byte[] yield = P2PManager.Retrieve(YieldIdentifier(index));
            if (yield == null)
                return null;

            MemoryStream memStream = new MemoryStream();
            DeflateStream defStream = new DeflateStream(memStream, CompressionMode.Decompress);
            defStream.Read(yield, 0, yield.Length);
            byte[] decompressedYield = memStream.ToArray();
            
            return decompressedYield;
        }

        private void LoadStoreThreadProc()
        {
            int loadEnd = head; //we know, that the DHT entries from 1 to "loadEnd" are unknown to us, so we will load these when we have no other things to do
            int loadIndex = 0;

            while (!stopLoadStoreThread)
            {
                if (loadIndex >= loadEnd)   //if we already loaded the yields up to "loadEnd", we don't have too much to do anymore, so we can slow down :)
                    Thread.Sleep(1000);

                if (storequeue.Count != 0)  //storing has priority
                {
                    byte[] yield = (byte[])storequeue.Dequeue();
                    P2PManager.Store(YieldIdentifier(head), yield);
                    //TODO: If versioning sytem tells us, that there is already a newer entry here, we load this value, enqueue it in loadqueue and try again with head++
                    head++;
                    P2PManager.Store(HeadIdentifier(), head.ToString());
                    //TODO: If versioning system tells us, that there is already a newer head entry, we ignore this and don't store ours
                }
                else                      //if there is nothing to store, we can load the yields up to "loadEnd"
                {
                    if (loadIndex < loadEnd)
                    {
                        byte[] yield = ReadYield(loadIndex);
                        loadqueue.Enqueue(yield);
                        loadIndex++;
                    }
                }
            }
        }

        private string HeadIdentifier()
        {
            return channel + "#" + factor + "HEAD";
        }

        private string YieldIdentifier(int index)
        {
            return channel + "#" + factor + "!" + index;
        }

        private void StartLoadStoreThread()
        {
            storequeue = Queue.Synchronized(new Queue());
            loadqueue = Queue.Synchronized(new Queue());

            stopLoadStoreThread = false;
            loadStoreThread = new Thread(LoadStoreThreadProc);
            loadStoreThread.Start();
        }

        public void StopLoadStoreThread()
        {
            stopLoadStoreThread = true;
            loadStoreThread.Join();
            loadStoreThread = null;
        }

        public Queue GetLoadedYieldsQueue()
        {
            return loadqueue;
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

            storequeue.Enqueue(compressedYield);
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
        /// Sets the factor to sieve next and starts reading informations from the DHT.
        /// </summary>
        public void SetFactor(BigInteger factor)
        {
            this.factor = factor;
            Debug.Assert(this.number % this.factor == 0);

            byte[] h = P2PManager.Retrieve(HeadIdentifier());
            if (h != null)
                head = int.Parse(h.ToString());
            else
                head = 0;

            if (loadStoreThread != null)
                throw new Exception("LoadStoreThread already started");
            StartLoadStoreThread();
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
