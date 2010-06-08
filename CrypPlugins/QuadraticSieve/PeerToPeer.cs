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
            int loadEnd = head; //we know, that the DHT entries from 1 to "loadEnd" are unknown to us, so we will load these when we have no other things to do
            int loadIndex = 0;
            uint downloaded = 0;
            uint uploaded = 0;

            try
            {
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
                        uploaded += (uint)yield.Length;
                        ShowTransfered(downloaded, uploaded);
                    }
                    else                      //if there is nothing to store, we can load the yields up to "loadEnd".
                    {
                        if (loadIndex < loadEnd)
                        {
                            byte[] yield = ReadYield(loadIndex);
                            downloaded += (uint)yield.Length;
                            ShowTransfered(downloaded, uploaded);
                            loadqueue.Enqueue(yield);
                            SetProgressYield(loadIndex, YieldStatus.OthersLoaded);
                            loadIndex++;
                            yieldEvent.Set();
                        }
                        else                //if there is nothing left to load, we can slow down.
                        {
                            Thread.Sleep(10000);        //wait 10 seconds
                        }
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                return;
            }
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
                P2PManager.Store(FactorListIdentifier(), memstream.ToArray());
            }

            return factorManager.ContainsComposite(this.factor);
        }
    }
}
