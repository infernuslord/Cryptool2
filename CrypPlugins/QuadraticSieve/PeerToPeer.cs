﻿/*                              
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
using System.Management;
using System.Security.Principal;
using System.Security.Cryptography;
using Cryptool.P2P.Internal;

namespace Cryptool.Plugins.QuadraticSieve
{
    class PeerToPeer
    {
        /// <summary>
        /// This structure is used to hold all important peer performance and alive informations in a queue
        /// </summary>
        private struct PeerPerformanceInformations
        {
            public DateTime lastChecked;
            public int peerID;
            public double performance;
            public int lastAlive;            

            public PeerPerformanceInformations(DateTime lastChecked, int peerID, double performance, int lastAlive)
            {
                this.lastAlive = lastAlive;
                this.lastChecked = lastChecked;
                this.peerID = peerID;
                this.performance = performance;
            }
        }

        private string channel;
        private BigInteger number;
        private BigInteger factor;
        private int head;
        private Queue storequeue;   //relation packages to store in the DHT
        private Queue loadqueue;    //relation packages that have been loaded from the DHT
        private Thread loadStoreThread;
        private bool stopLoadStoreThread;
        private QuadraticSievePresentation quadraticSieveQuickWatchPresentation;
        private AutoResetEvent newRelationPackageEvent;
        private long ourID;           //Our ID
        private Dictionary<int, string> nameCache;  //associates the ids with the names
        private Queue<PeerPerformanceInformations> peerPerformances;      //A queue of performances from the different peers ordered by the date last checked.
        private HashSet<int> activePeers;                                 //A set of active peers
        private double ourPerformance = 0;
        private int aliveCounter = 0;       //This is stored together with the performance in the DHT
        private string ourName;
        private uint downloaded = 0;
        private uint uploaded = 0;
        private HashSet<int> ourIndices;
        private Queue<KeyValuePair<int, DateTime>> lostIndices;
        private int loadIndex;
        private AutoResetEvent waitForConnection = new AutoResetEvent(false);

        public delegate void P2PWarningHandler(String warning);
        public event P2PWarningHandler P2PWarning;        

        public PeerToPeer(QuadraticSievePresentation presentation, AutoResetEvent newRelationPackageEvent)
        {
            quadraticSieveQuickWatchPresentation = presentation;
            this.newRelationPackageEvent = newRelationPackageEvent;
            SetOurID();
        }

        public int getActivePeers()
        {
            if (activePeers == null)
                return 0;
            return activePeers.Count;
        }

        private void SetOurID()
        {
            ourID = Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetID();
            quadraticSieveQuickWatchPresentation.ProgressRelationPackages.setOurID(ourID);
            ourName = Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetHostName();
        }

        /// <summary>
        /// Reads relation package at position "index" from DHT and returns the ownerID and the decompressed packet itself.
        /// </summary>
        private byte[] ReadRelationPackage(int index, out int ownerID)
        {
            ownerID = 0;
            byte[] relationPackage = P2PManager.Retrieve(RelationPackageIdentifier(index)).Data;
            if (relationPackage == null)
                return null;
            downloaded += (uint)relationPackage.Length;

            byte[] decompressedRelationPackage = DecompressRelationPackage(relationPackage);

            byte[] idbytes = new byte[4];
            Array.Copy(decompressedRelationPackage, idbytes, 4);
            ownerID = BitConverter.ToInt32(idbytes, 0);
            byte[] y = new byte[decompressedRelationPackage.Length - 4];
            Array.Copy(decompressedRelationPackage, 4, y, 0, y.Length);
            
            return y;
        }

        private static byte[] DecompressRelationPackage(byte[] relationPackage)
        {
            MemoryStream memStream = new MemoryStream();
            DeflateStream defStream = new DeflateStream(memStream, CompressionMode.Decompress);
            memStream.Write(relationPackage, 0, relationPackage.Length);
            memStream.Position = 0;
            MemoryStream memStream2 = new MemoryStream();
            defStream.CopyTo(memStream2);
            defStream.Close();
            byte[] decompressedRelationPackage = memStream2.ToArray();
            return decompressedRelationPackage;
        }

        private bool ReadAndEnqueueRelationPackage(int loadIndex, bool checkAlive)
        {
            int ownerID;
            byte[] relationPackage = ReadRelationPackage(loadIndex, out ownerID);
            if (relationPackage != null)
            {
                ShowTransfered(downloaded, uploaded);
                loadqueue.Enqueue(relationPackage);

                string name = null;

                if (ownerID != ourID)
                {
                    //Progress relation package:
                    if (!nameCache.ContainsKey(ownerID))
                    {
                        byte[] n = P2PManager.Retrieve(NameIdentifier(ownerID)).Data;
                        if (n != null)
                            nameCache.Add(ownerID, System.Text.ASCIIEncoding.ASCII.GetString(n));
                    }
                    if (nameCache.ContainsKey(ownerID))
                        name = nameCache[ownerID];
                                        
                    if (checkAlive && !activePeers.Contains(ownerID))
                    {
                        //check performance and alive informations:
                        byte[] performancebytes = P2PManager.Retrieve(PerformanceIdentifier(ownerID)).Data;
                        if (performancebytes != null)
                        {
                            double performance = BitConverter.ToDouble(performancebytes, 0);
                            int peerAliveCounter = BitConverter.ToInt32(performancebytes, 8);
                            peerPerformances.Enqueue(new PeerPerformanceInformations(DateTime.Now, ownerID, performance, peerAliveCounter));
                        }
                        activePeers.Add(ownerID);
                        UpdateActivePeerInformation();
                    }
                }

                //Set progress info:
                SetProgressRelationPackage(loadIndex, ownerID, name);

                newRelationPackageEvent.Set();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to read and enqueue relation package on position "loadIndex".
        /// If it fails, it stores the index in lostIndices queue.
        /// </summary>
        private void TryReadAndEnqueueRelationPackage(int index, bool checkAlive, Queue<KeyValuePair<int, DateTime>> lostIndices)
        {
            bool succ = ReadAndEnqueueRelationPackage(index, checkAlive);
            if (!succ)                
            {
                var e = new KeyValuePair<int, DateTime>(index, DateTime.Now);
                lostIndices.Enqueue(e);
                SetProgressRelationPackage(index, -1, null);
            }
        }

        private void LoadStoreThreadProc()
        {
            loadIndex = 0;
            downloaded = 0;
            uploaded = 0;
            ourIndices = new HashSet<int>();   //Stores all the indices which belong to our packets
            //Stores all the indices (together with there check date) which belong to lost packets (i.e. packets that can't be load anymore):
            lostIndices = new Queue<KeyValuePair<int, DateTime>>();
            double lastPerformance = 0;
            DateTime performanceLastPut = new DateTime();
            UpdateActivePeerInformation();

            try
            {
                WaitTillConnected();

                //store our name:
                P2PManager.Retrieve(NameIdentifier(ourID));     //just to outsmart the versioning system
                P2PManager.Store(NameIdentifier(ourID), System.Text.ASCIIEncoding.ASCII.GetBytes(ourName.ToCharArray()));

                head = 0;
                SynchronizeHead();
                int startHead = head;

                while (!stopLoadStoreThread)
                {
                    try
                    {
                        //Store our performance and our alive counter in the DHT, either if the performance changed or when the last write was more than 1 minute ago:
                        if (ourPerformance != lastPerformance || performanceLastPut.CompareTo(DateTime.Now.Subtract(new TimeSpan(0, 1, 0))) < 0)
                        {
                            P2PManager.Retrieve(PerformanceIdentifier(ourID));      //just to outsmart the versioning system
                            P2PManager.Store(PerformanceIdentifier(ourID), concat(BitConverter.GetBytes(ourPerformance), BitConverter.GetBytes(aliveCounter++)));
                            lastPerformance = ourPerformance;
                            performanceLastPut = DateTime.Now;
                        }

                        //updates all peer performances which have last been checked more than 1 minutes and 20 seconds ago and check if they are still alive:
                        while (peerPerformances.Count != 0 && peerPerformances.Peek().lastChecked.CompareTo(DateTime.Now.Subtract(new TimeSpan(0, 1, 20))) < 0)
                        {
                            var e = peerPerformances.Dequeue();

                            byte[] performancebytes = P2PManager.Retrieve(PerformanceIdentifier(e.peerID)).Data;
                            if (performancebytes != null)
                            {
                                double performance = BitConverter.ToDouble(performancebytes, 0);
                                int peerAliveCounter = BitConverter.ToInt32(performancebytes, 8);
                                if (peerAliveCounter <= e.lastAlive)
                                {
                                    activePeers.Remove(e.peerID);
                                    UpdateActivePeerInformation();
                                }
                                else
                                    peerPerformances.Enqueue(new PeerPerformanceInformations(DateTime.Now, e.peerID, performance, peerAliveCounter));
                            }
                            else
                            {
                                activePeers.Remove(e.peerID);
                                UpdateActivePeerInformation();
                            }                       
                        }
                                        
                        SynchronizeHead();
                        UpdateStoreLoadQueueInformation();

                        bool busy = false;

                        if (storequeue.Count != 0)  //store our packages
                        {
                            byte[] relationPackage = (byte[])storequeue.Dequeue();
                            bool success = P2PManager.Store(RelationPackageIdentifier(head), relationPackage).IsSuccessful();
                            while (!success)
                            {
                                SynchronizeHead();
                                success = P2PManager.Store(RelationPackageIdentifier(head), relationPackage).IsSuccessful();
                            }
                            SetProgressRelationPackage(head, ourID, null);
                            ourIndices.Add(head);
                        
                            head++;

                            //show informations about the uploaded relation package:
                            uploaded += (uint)relationPackage.Length;
                            ShowTransfered(downloaded, uploaded);
                            UpdateStoreLoadQueueInformation();
                            busy = true;
                        }

                        //load the other relation packages:

                        //skip all indices which are uploaded by us:
                        while (ourIndices.Contains(loadIndex))
                            loadIndex++;

                        if (loadIndex < head)
                        {
                            bool checkAlive = loadIndex >= startHead;       //we don't check the peer alive informations of the packages that existed before we joined
                            TryReadAndEnqueueRelationPackage(loadIndex, checkAlive, lostIndices);
                            loadIndex++;
                            UpdateStoreLoadQueueInformation();
                            busy = true;
                        }
                    
                        //check lost indices which are last checked longer than 2 minutes ago (but only if we have nothing else to do):
                        if (!busy)
                        {
                            //TODO: Maybe we should throw away those indices, which have been checked more than several times.
                            if (lostIndices.Count != 0 && lostIndices.Peek().Value.CompareTo(DateTime.Now.Subtract(new TimeSpan(0, 2, 0))) < 0)
                            {
                                var e = lostIndices.Dequeue();
                                TryReadAndEnqueueRelationPackage(loadIndex, true, lostIndices);
                                UpdateStoreLoadQueueInformation();                            
                            }
                            else
                                Thread.Sleep(5000);    //Wait 5 seconds
                        }

                    }
                    catch (NotConnectedException)
                    {
                        WaitTillConnected();
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                return;
            }
            catch (NotConnectedException)
            {
                WaitTillConnected();
                LoadStoreThreadProc();
            }
        }

        private void WaitTillConnected()
        {
            if (!P2PManager.IsConnected)
            {
                P2PManager.ConnectionManager.OnP2PConnectionStateChangeOccurred += HandleConnectionStateChange;
                waitForConnection.WaitOne();
                P2PManager.ConnectionManager.OnP2PConnectionStateChangeOccurred -= HandleConnectionStateChange;
            }
        }

        private void HandleConnectionStateChange(object sender, bool newState)
        {
            if (newState)
                waitForConnection.Set();
        }

        private void UpdateActivePeerInformation()
        {
            quadraticSieveQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.activePeers.Content = (activePeers.Count+1);
            }, null);
        }

        private void UpdateStoreLoadQueueInformation()
        {
            if (storequeue != null && ourIndices != null && lostIndices != null)
            {
                quadraticSieveQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    int upload = storequeue.Count;
                    int download = head - loadIndex - ourIndices.Count(x => x > loadIndex);
                    int lost = this.lostIndices.Count;
                    quadraticSieveQuickWatchPresentation.queueInformation.Content = "Upload " + upload + "! Download " + download + "! Lost " + lost + "!";
                }, null);
            }
        }

        /// <summary>
        /// Loads head from DHT. If ours is greater (or there is no entry yet), we store ours.
        /// </summary>
        private void SynchronizeHead()
        {
            byte[] h = P2PManager.Retrieve(HeadIdentifier()).Data;
            if (h != null)
            {
                int dhthead = int.Parse(System.Text.ASCIIEncoding.ASCII.GetString(h));
                if (head > dhthead)
                {
                    bool success = P2PManager.Store(HeadIdentifier(), System.Text.ASCIIEncoding.ASCII.GetBytes(head.ToString())).IsSuccessful();
                    if (!success)
                        SynchronizeHead();
                }
                else if (head < dhthead)
                {
                    head = dhthead;
                    if (head != 0)
                        SetProgressRelationPackage(head - 1, 0, null);
                }
            }
            else
            {
                bool success = P2PManager.Store(HeadIdentifier(), System.Text.ASCIIEncoding.ASCII.GetBytes(head.ToString())).IsSuccessful();
                if (!success)
                    SynchronizeHead();
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
                quadraticSieveQuickWatchPresentation.downloaded.Content = size1 + " MB";
                quadraticSieveQuickWatchPresentation.uploaded.Content = size2 + " MB";
            }, null);
        }

        private void SetProgressRelationPackage(int index, long id, string name)
        {
            quadraticSieveQuickWatchPresentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.ProgressRelationPackages.Set(index, id, name);
            }, null);
        }

        private void ClearProgressRelationPackages()
        {
            quadraticSieveQuickWatchPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                quadraticSieveQuickWatchPresentation.ProgressRelationPackages.Clear();
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

        private string RelationPackageIdentifier(int index)
        {
            return channel + "#" + number + "-" + factor + "!" + index;
        }

        private string NameIdentifier(long ID)
        {
            return channel + "#" + number + "NAME" + ID.ToString();
        }

        private string PerformanceIdentifier(long ID)
        {
            return channel + "#" + number + "-" + factor + "PERFORMANCE" + ID.ToString();
        }

        public string StatusKey()
        {
            return channel + "#" + number + "-status";
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
            if (loadStoreThread != null)
            {
                loadStoreThread.Interrupt();
                loadStoreThread.Join();
                loadStoreThread = null;
            }
        }

        /// <summary>
        /// Concatenates the two byte arrays a1 and a2 an returns the result array.
        /// </summary>
        private byte[] concat(byte[] a1, byte[] a2)
        {
            byte[] res = new byte[a1.Length + a2.Length];
            System.Buffer.BlockCopy(a1, 0, res, 0, a1.Length);
            System.Buffer.BlockCopy(a2, 0, res, a1.Length, a2.Length);
            return res;
        }
        


        public Queue GetLoadedRelationPackagesQueue()
        {
            return loadqueue;
        }

        /// <summary>
        /// Compresses the relation package and puts it in the DHT.
        /// </summary>
        public void Put(byte[] serializedRelationPackage)
        {
            //Add our ID:
            byte[] idbytes = BitConverter.GetBytes(ourID);
            Debug.Assert(idbytes.Length == 4);
            serializedRelationPackage = concat(idbytes, serializedRelationPackage); ;

            //Compress:
            MemoryStream memStream = new MemoryStream();
            DeflateStream defStream = new DeflateStream(memStream, CompressionMode.Compress);
            defStream.Write(serializedRelationPackage, 0, serializedRelationPackage.Length);
            defStream.Close();
            byte[] compressedRelationPackage = memStream.ToArray();

            //Debug stuff:
            byte[] decompr = DecompressRelationPackage(compressedRelationPackage);
            Debug.Assert(decompr.Length == serializedRelationPackage.Length);

            //store in queue, so the LoadStoreThread can store it in the DHT later:
            storequeue.Enqueue(compressedRelationPackage);

            UpdateStoreLoadQueueInformation();
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

            ClearProgressRelationPackages();
            nameCache = new Dictionary<int, string>();
            peerPerformances = new Queue<PeerPerformanceInformations>();
            activePeers = new HashSet<int>();
            
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
            try
            {
                FactorManager dhtFactorManager = null;
                //load DHT Factor Manager:
                byte[] dhtFactorManagerBytes = P2PManager.Retrieve(FactorListIdentifier()).Data;
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
                    bool success = P2PManager.Store(FactorListIdentifier(), memstream.ToArray()).IsSuccessful();
                    if (!success)
                    {
                        Thread.Sleep(1000);
                        return SyncFactorManager(factorManager);       //Just try again
                    }
                }

                return factorManager.ContainsComposite(this.factor);
            }
            catch (NotConnectedException)
            {
                return true;
            }
        }

        /// <summary>
        /// Sets the performance of this machine, so that this class can write it to the DHT later.
        /// The performance is meassured in relations per ms.
        /// </summary>
        public void SetOurPerformance(double[] relationsPerMS)
        {
            double globalPerformance = 0;
            foreach (double r in relationsPerMS)
                globalPerformance += r;

            ourPerformance = globalPerformance;
        }

        /// <summary>
        /// Returns the performance of all peers (excluding ourselve) in relations per ms.
        /// </summary>
        public double GetP2PPerformance()
        {
            double perf = 0;

            foreach (var p in peerPerformances)
                perf += p.performance;

            return perf;
        }
    }
}
