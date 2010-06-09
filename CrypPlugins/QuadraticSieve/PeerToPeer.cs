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
        private AutoResetEvent yieldEvent;

        public delegate void P2PWarningHandler(String warning);
        public event P2PWarningHandler P2PWarning;

        public PeerToPeer(QuadraticSievePresentation presentation, AutoResetEvent yieldEvent)
        {
            quadraticSieveQuickWatchPresentation = presentation;
            this.yieldEvent = yieldEvent;
        }

        private byte[] ReadYield(int index)
        {
            byte[] yield = P2PManager.Retrieve(YieldIdentifier(index));
            if (yield == null)
                return null;

            byte[] decompressedYield = DecompressYield(yield);            
            return decompressedYield;
        }

        private static byte[] DecompressYield(byte[] yield)
        {
            MemoryStream memStream = new MemoryStream();
            DeflateStream defStream = new DeflateStream(memStream, CompressionMode.Decompress);
            memStream.Write(yield, 0, yield.Length);
            memStream.Position = 0;
            MemoryStream memStream2 = new MemoryStream();
            defStream.CopyTo(memStream2);
            defStream.Close();
            byte[] decompressedYield = memStream2.ToArray();
            return decompressedYield;
        }

        private void LoadStoreThreadProc()
        {
            int loadIndex = 0;
            uint downloaded = 0;
            uint uploaded = 0;
            HashSet<int> ourIndices = new HashSet<int>();   //Stores all the indices which belong to our packets
            //Stores all the indices (together with there check date) which belong to lost packets (i.e. packets that can't be load anymore):
            Queue<KeyValuePair<int, DateTime>> lostIndices = new Queue<KeyValuePair<int, DateTime>>();

            try
            {
                while (!stopLoadStoreThread)
                {
                    SynchronizeHead();

                    if (storequeue.Count != 0)  //storing has priority
                    {
                        byte[] yield = (byte[])storequeue.Dequeue();
                        bool success = P2PManager.Store(YieldIdentifier(head), yield);
                        while (!success)
                        {
                            SynchronizeHead();
                            success = P2PManager.Store(YieldIdentifier(head), yield);
                        }
                        SetProgressYield(head, YieldStatus.Ours);
                        ourIndices.Add(head);
                        
                        head++;

                        //show informations about the uploaded yield:
                        uploaded += (uint)yield.Length;
                        ShowTransfered(downloaded, uploaded);
                    }
                    else                      //if there is nothing to store, we can load the other yields.
                    {
                        while (ourIndices.Contains(loadIndex))
                            loadIndex++;

                        if (loadIndex < head)
                        {
                            downloaded = TryReadAndEnqueueYield(loadIndex, downloaded, uploaded, lostIndices);

                            loadIndex++;
                        }
                        else
                        {
                            //check all lost indices which are last checked longer than 1 minutes ago:
                            //TODO: Maybe we should throw away those indices, which have been checked more than several times.
                            while (lostIndices.Peek().Value.CompareTo(DateTime.Now.Subtract(new TimeSpan(0,1,0))) < 0)
                            {
                                var e = lostIndices.Dequeue();
                                downloaded = TryReadAndEnqueueYield(loadIndex, downloaded, uploaded, lostIndices);
                            }
                            Thread.Sleep(5000);    //Wait 5 seconds
                        }
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                return;
            }
        }

        /// <summary>
        /// Tries to read and enqueue yield on position "loadIndex".
        /// If it fails, it stores the index in lostIndices queue.
        /// </summary>
        private uint TryReadAndEnqueueYield(int loadIndex, uint downloaded, uint uploaded, Queue<KeyValuePair<int, DateTime>> lostIndices)
        {
            uint res = ReadAndEnqueueYield(loadIndex, downloaded, uploaded);
            if (res != 0)
                downloaded = res;
            else
            {
                var e = new KeyValuePair<int, DateTime>(loadIndex, DateTime.Now);
                lostIndices.Enqueue(e);
            }
            return downloaded;
        }

        /// <summary>
        /// Loads head from DHT. If ours is greater, we store ours.
        /// </summary>
        private void SynchronizeHead()
        {
            byte[] h = P2PManager.Retrieve(HeadIdentifier());
            if (h != null)
            {
                int dhthead = int.Parse(System.Text.ASCIIEncoding.ASCII.GetString(h));
                if (head > dhthead)
                {
                    bool success = P2PManager.Store(HeadIdentifier(), System.Text.ASCIIEncoding.ASCII.GetBytes(head.ToString()));
                    if (!success)
                        SynchronizeHead();
                }
                else
                    head = dhthead;
            }            
        }

        private uint ReadAndEnqueueYield(int loadIndex, uint downloaded, uint uploaded)
        {
            byte[] yield = ReadYield(loadIndex);
            if (yield != null)
            {
                downloaded += (uint)yield.Length;
                ShowTransfered(downloaded, uploaded);
                loadqueue.Enqueue(yield);
                SetProgressYield(loadIndex, YieldStatus.OthersLoaded);
                yieldEvent.Set();
                return downloaded;
            }
            return 0;
        }

        private void ShowTransfered(uint downloaded, uint uploaded)
        {
            string s1 = ((downloaded / 1024.0) / 1024).ToString();
            string size1 = s1.Substring(0, (s1.Length < 3) ? s1.Length : 3);
            string s2 = ((uploaded / 1024.0) / 1024).ToString();
            string size2 = s2.Substring(0, (s2.Length < 3) ? s2.Length : 3);
            quadraticSieveQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.relationsInfo.Content = "Downloaded " + size1 + " MB! Uploaded " + size2 + " MB!";
            }, null);
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
            return channel + "#" + number + "-" + factor + "HEAD";
        }

        private string FactorListIdentifier()
        {
            return channel + "#" + number + "FACTORLIST";
        }

        private string YieldIdentifier(int index)
        {
            return channel + "#" + number + "-" + factor + "!" + index;
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
            loadStoreThread.Interrupt();
            loadStoreThread.Join();
            loadStoreThread = null;
        }

        public Queue GetLoadedYieldsQueue()
        {
            return loadqueue;
        }

        /// <summary>
        /// Compresses the yield and puts it in the DHT.
        /// </summary>
        public void Put(byte[] serializedYield)
        {
            //Compress:
            MemoryStream memStream = new MemoryStream();
            DeflateStream defStream = new DeflateStream(memStream, CompressionMode.Compress);
            defStream.Write(serializedYield, 0, serializedYield.Length);
            defStream.Close();
            byte[] compressedYield = memStream.ToArray();

            //Debug stuff:
            byte[] decompr = DecompressYield(compressedYield);
            Debug.Assert(decompr.Length == serializedYield.Length);

            //store in queue, so the LoadStoreThread can store it in the DHT later:
            storequeue.Enqueue(compressedYield);            
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
                memstream.Position = 0;
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
                try
                {
                    dhtFactorManager = (FactorManager)bformatter.Deserialize(memstream);
                }
                catch (System.Runtime.Serialization.SerializationException)
                {
                    P2PWarning("DHT factor list is broken!");
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
                bformatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
                bformatter.Serialize(memstream, factorManager);
                bool success = P2PManager.Store(FactorListIdentifier(), memstream.ToArray());
                if (!success)
                {
                    Thread.Sleep(1000);
                    return SyncFactorManager(factorManager);       //Just try again
                }
            }

            return factorManager.ContainsComposite(this.factor);
        }
    }
}
