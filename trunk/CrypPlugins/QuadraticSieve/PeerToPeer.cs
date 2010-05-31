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
using System.Windows.Threading;
using System.Runtime.Serialization.Formatters.Binary;

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
        private QuadraticSievePresentation quadraticSieveQuickWatchPresentation;

        public PeerToPeer(QuadraticSievePresentation presentation)
        {
            quadraticSieveQuickWatchPresentation = presentation;
        }

        private byte[] ReadYield(int index)
        {
            byte[] yield = P2PManager.Retrieve(YieldIdentifier(index));
            if (yield == null)
                return null;

            MemoryStream memStream = new MemoryStream();            
            DeflateStream defStream = new DeflateStream(memStream, CompressionMode.Decompress);
            memStream.Write(yield, 0, yield.Length);
            memStream.Position = 0;
            MemoryStream memStream2 = new MemoryStream();
            defStream.CopyTo(memStream2);
            byte[] decompressedYield = memStream2.ToArray();
            
            return decompressedYield;
        }

        private void LoadStoreThreadProc()
        {
            int loadEnd = head; //we know, that the DHT entries from 1 to "loadEnd" are unknown to us, so we will load these when we have no other things to do
            int loadIndex = 0;

            while (!stopLoadStoreThread)
            {                
                if (storequeue.Count != 0)  //storing has priority
                {
                    byte[] yield = (byte[])storequeue.Dequeue();
                    P2PManager.Store(YieldIdentifier(head), yield);
                    //TODO: If versioning sytem tells us, that there is already a newer entry here, we load this value, enqueue it in loadqueue and try again with head++
                    SetProgressYield(head, YieldStatus.Ours);
                    head++;
                    P2PManager.Store(HeadIdentifier(), System.Text.ASCIIEncoding.ASCII.GetBytes(head.ToString()));
                    //TODO: If versioning system tells us, that there is already a newer head entry, we ignore this and don't store ours
                }
                else                      //if there is nothing to store, we can load the yields up to "loadEnd".
                {
                    if (loadIndex < loadEnd)
                    {
                        byte[] yield = ReadYield(loadIndex);
                        loadqueue.Enqueue(yield);
                        SetProgressYield(loadIndex, YieldStatus.OthersLoaded);
                        loadIndex++;
                    }
                    else                //if there is nothing left to load, we can slow down.
                    {
                        Thread.Sleep(10000);        //wait 10 seconds
                    }
                }
            }
        }

        private void SetProgressYield(int index, YieldStatus status)
        {
            quadraticSieveQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.ProgressYields[index] = status;
            }, null);
        }

        private void ClearProgressYields()
        {
            quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.ProgressYields.Clear();
            }, null);
        }

        private string HeadIdentifier()
        {
            return channel + "#" + factor + "HEAD";
        }

        private string FactorListIdentifier()
        {
            return channel + "#" + number + "FACTORLIST";
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

            ClearProgressYields();
            byte[] h = P2PManager.Retrieve(HeadIdentifier());
            if (h != null)
            {                
                head = int.Parse(System.Text.ASCIIEncoding.ASCII.GetString(h));
                SetProgressYield(head-1, YieldStatus.OthersNotLoaded);
            }
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
            FactorManager dhtFactorManager = null;
            //load DHT Factor Manager:
            byte[] dhtFactorManagerBytes = P2PManager.Retrieve(FactorListIdentifier());
            if (dhtFactorManagerBytes != null)
            {
                MemoryStream memstream = new MemoryStream();
                memstream.Write(dhtFactorManagerBytes, 0, dhtFactorManagerBytes.Length);
                BinaryFormatter bformatter = new BinaryFormatter();
                try
                {
                    dhtFactorManager = (FactorManager)bformatter.Deserialize(memstream);
                }
                catch (System.Runtime.Serialization.SerializationException)
                {
                    //TODO: GuiLogMessage here
                    P2PManager.Remove(FactorListIdentifier());
                    return SyncFactorManager(factorManager);
                }
            }

            //Synchronize DHT Factor Manager with our Factor List
            if (dhtFactorManager == null || factorManager.Synchronize(dhtFactorManager))
            {
                //Our Factor Manager has more informations, so let's store it in the DHT:
                MemoryStream memstream = new MemoryStream();
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(memstream, factorManager);
                P2PManager.Store(FactorListIdentifier(), memstream.ToArray());
            }

            return factorManager.ContainsComposite(this.factor);
        }
    }
}
