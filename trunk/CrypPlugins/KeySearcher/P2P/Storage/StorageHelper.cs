﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cryptool.P2P;
using Cryptool.P2P.Internal;
using Cryptool.PluginBase;
using KeySearcher.P2P.Exceptions;
using KeySearcher.P2P.Presentation;
using KeySearcher.P2P.Tree;

namespace KeySearcher.P2P.Storage
{
    class StorageHelper
    {
        private readonly KeySearcher keySearcher;
        private readonly StatisticsGenerator statisticsGenerator;
        private readonly StatusContainer statusContainer;

        //VERSIONNUMBER: Important. Set it +1 manually everytime the length of the MemoryStream Changes
        private const int version = 5;
        /*
        -----------------------------Versionnumber Changelog---------------------------------------------
        |   Version 1: Added the Versionnumber to the Stream (Leafs)
        |   Version 2: Added the first User Statistics (Avatar,ID,Count) to the Stream
        |   Version 3: Added version question (in front of results) + more statistic information (hostname,date) to the Stream
        |   Version 4: Reorganisation of the DHT structure. Update fully working now + features from previous versions available
        |   Version 5: Extended the key information by the user who found it. (Username,Date,Maschid,Host)
        -------------------------------------------------------------------------------------------------
         */

        public StorageHelper(KeySearcher keySearcher, StatisticsGenerator statisticsGenerator, StatusContainer statusContainer)
        {
            this.keySearcher = keySearcher;
            this.statisticsGenerator = statisticsGenerator;
            this.statusContainer = statusContainer;
        }

        //-------------------------------------------------------------------------------------------
        //AFTER CHANGING THE FOLLOWING METHODS INCREASE THE VERSION-NUMBER AT THE TOP OF THIS CLASS!
        //-------------------------------------------------------------------------------------------
        internal RequestResult UpdateInDht(NodeBase nodeToUpdate)
        {
            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);

            //Append Updater Version
            binaryWriter.Write('W');
            binaryWriter.Write(version);  

            if (nodeToUpdate is Node)
            {
                UpdateNodeInDht((Node) nodeToUpdate, binaryWriter);
            } else
            {
                UpdateLeafInDht((Leaf) nodeToUpdate, binaryWriter);
            }

            // Append results
            binaryWriter.Write(nodeToUpdate.Result.Count);
            foreach (var valueKey in nodeToUpdate.Result)
            {
                binaryWriter.Write(valueKey.key);
                binaryWriter.Write(valueKey.value);
                binaryWriter.Write(valueKey.decryption.Length);
                binaryWriter.Write(valueKey.decryption);

                //--------------------------------------------------------
                binaryWriter.Write(valueKey.user);
                var buffertime = valueKey.time.ToBinary();
                binaryWriter.Write(buffertime);
                binaryWriter.Write(valueKey.maschid);
                binaryWriter.Write(valueKey.maschname);
                //---------------------------------------------------------
            }                        
             
            //Creating a copy of the activity dictionary
            var copyAct = nodeToUpdate.Activity;

            //Write number of avatarnames
            binaryWriter.Write(copyAct.Keys.Count);
            foreach (string avatar in copyAct.Keys)
            {
                var maschCopy = copyAct[avatar];
                //write avatarname
                binaryWriter.Write(avatar);
                //write the number of maschines for this avatar
                binaryWriter.Write(maschCopy.Keys.Count);

                foreach (long maschID in maschCopy.Keys)
                {
                    //write the maschines and their patterncount
                    binaryWriter.Write(maschID);
                    binaryWriter.Write(maschCopy[maschID].Count); //int 32
                    binaryWriter.Write(maschCopy[maschID].Hostname); //String
                    binaryWriter.Write(maschCopy[maschID].Date.ToBinary()); //DateTime
                }
            }
            return StoreWithStatistic(KeyInDht(nodeToUpdate), memoryStream.ToArray());
        }

        private static void UpdateNodeInDht(Node nodeToUpdate, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(nodeToUpdate.LeftChildFinished);
            binaryWriter.Write(nodeToUpdate.RightChildFinished);
        }

        private static void UpdateLeafInDht(Leaf nodeToUpdate, BinaryWriter binaryWriter)
        {
            binaryWriter.Write('V');
            binaryWriter.Write(version);
            var buffer = nodeToUpdate.LastReservationDate.ToBinary();
            binaryWriter.Write(buffer);
            binaryWriter.Write(nodeToUpdate.getClientIdentifier());
        }

        internal RequestResult UpdateFromDht(NodeBase nodeToUpdate, bool forceUpdate = false)
        {
            try
            {
                if (!forceUpdate && nodeToUpdate.LastUpdate > DateTime.Now.Subtract(new TimeSpan(0, 0, 5)))
                {
                    return new RequestResult { Status = RequestResultType.Success };
                }

                nodeToUpdate.LastUpdate = DateTime.Now;

                var requestResult = RetrieveWithStatistic(KeyInDht(nodeToUpdate));
                var nodeBytes = requestResult.Data;

                if (nodeBytes == null)
                {
                    return requestResult;
                }

                var binaryReader = new BinaryReader(new MemoryStream(nodeBytes));

                //oldVersionFlag will be used to garantee further changes in the Stream
                var oldVersionFlag = CheckNodeVersion(binaryReader);

                if (nodeToUpdate is Node)
                {
                    UpdateNodeFromDht((Node)nodeToUpdate, binaryReader);
                }
                else
                {
                    UpdateLeafFromDht((Leaf)nodeToUpdate, binaryReader);
                }

                // Load results
                var resultCount = binaryReader.ReadInt32();

                for (var i = 0; i < resultCount; i++)
                {
                    if (oldVersionFlag < 5)
                    {
                        var newResult = new KeySearcher.ValueKey
                                            {
                                                key = binaryReader.ReadString(),
                                                value = binaryReader.ReadDouble(),
                                                decryption = binaryReader.ReadBytes(binaryReader.ReadInt32()),
                                                user = "Unknown",
                                                time = DateTime.MinValue,
                                                maschid = 666,
                                                maschname = "Devil"
                                            };
                        nodeToUpdate.Result.AddLast(newResult);
                    }
                    else
                    {
                        var newResult = new KeySearcher.ValueKey
                                            {
                                                key = binaryReader.ReadString(),
                                                value = binaryReader.ReadDouble(),
                                                decryption = binaryReader.ReadBytes(binaryReader.ReadInt32()),
                                                user = binaryReader.ReadString(),
                                                time = DateTime.FromBinary(binaryReader.ReadInt64()),
                                                maschid = binaryReader.ReadInt64(),
                                                maschname = binaryReader.ReadString()
                                            };
                        nodeToUpdate.Result.AddLast(newResult);
                    }
                }

                if (binaryReader.BaseStream.Length != binaryReader.BaseStream.Position)
                {
                    //Reading the number of avatarnames
                    int avatarcount = binaryReader.ReadInt32();
                    for (int i = 0; i < avatarcount; i++)
                    {
                        //Reading the avatarname and the maschine-count for this name
                        string avatarname = binaryReader.ReadString();
                        int maschcount = binaryReader.ReadInt32();
                        var readMaschcount = new Dictionary<long, Information>();

                        for (int j = 0; j < maschcount; j++)
                        {
                            //reading the IDs and patterncount
                            long maschID = binaryReader.ReadInt64();
                            int count = binaryReader.ReadInt32();
                            string host = binaryReader.ReadString();
                            var date = DateTime.FromBinary(binaryReader.ReadInt64());
                            readMaschcount.Add(maschID, new Information() { Count = count, Hostname = host, Date = date });

                        }

                        if (nodeToUpdate.Activity.ContainsKey(avatarname))
                        {
                            nodeToUpdate.Activity[avatarname] = readMaschcount;
                        }
                        else
                        {
                            nodeToUpdate.Activity.Add(avatarname, readMaschcount);
                        }
                    }
                }

                if (resultCount > 0)
                {
                    keySearcher.IntegrateNewResults(nodeToUpdate.Result, nodeToUpdate.Activity,
                                                    nodeToUpdate.DistributedJobIdentifier);
                    statisticsGenerator.ProcessPatternResults(nodeToUpdate.Result);
                }

                nodeToUpdate.UpdateCache();
                return requestResult;
            }
            catch (Exception e)
            {
                if (e is KeySearcherStopException)
                    throw new KeySearcherStopException();
                else if (e is NotConnectedException)
                    throw new NotConnectedException();

                // client id not available, use default
                keySearcher.GuiLogMessage(e.Message + ": Node causing the failure: " + nodeToUpdate.ToString(), NotificationLevel.Error);
                nodeToUpdate.Reset();
                throw new InvalidOperationException();
            }
        }

        private static void UpdateNodeFromDht(Node nodeToUpdate, BinaryReader binaryReader)
        {
            nodeToUpdate.LeftChildFinished = binaryReader.ReadBoolean() || nodeToUpdate.LeftChildFinished;
            nodeToUpdate.RightChildFinished = binaryReader.ReadBoolean() || nodeToUpdate.RightChildFinished;
        }

        private static void UpdateLeafFromDht(Leaf nodeToUpdate, BinaryReader binaryReader)
        {
            var oldVersionFlag = CheckVersion(binaryReader);

            try
            {   
                var date = DateTime.FromBinary(binaryReader.ReadInt64());
                if (date > nodeToUpdate.LastReservationDate)
                {
                    nodeToUpdate.LastReservationDate = date;
                }
            

                if (binaryReader.BaseStream.Length - binaryReader.BaseStream.Position >= 8)
                {
                    nodeToUpdate.setClientIdentifier(binaryReader.ReadInt64());
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                throw new Exception();
            }
            
        }

        internal static string KeyInDht(NodeBase node)
        {
            return string.Format("{0}_node_{1}_{2}", node.DistributedJobIdentifier, node.From, node.To);
        }

        private static int CheckVersion(BinaryReader binaryReader)
        {           
            try
            {
                //Checking if there's a version in the stream
                int vers = binaryReader.PeekChar();
                if (vers == 86) //V infront of a Leaf
                {
                    //Reading the char and the versionnumber
                    char magic = binaryReader.ReadChar();
                    int versionInUse = binaryReader.ReadInt32();
                    //Check if a newer Version is in use
                    if (versionInUse > version)
                    {
                        throw new KeySearcherStopException();
                    }
                    return versionInUse;
                }
                else
                {
                    return 0;
                }
            }
            catch(KeySearcherStopException)
            {
                throw new KeySearcherStopException();
            }            
        }

        private static int CheckNodeVersion(BinaryReader binaryReader)
        {
            try
            {
                //Checking if there's a version in the stream
                int vers = binaryReader.PeekChar();
                if (vers == 87) //W infront of a Node
                {
                    //Reading the char and the versionnumber
                    char magic = binaryReader.ReadChar();
                    int versionInUse = binaryReader.ReadInt32();
                    //Check if a newer Version is in use
                    if (versionInUse > version)
                    {
                        throw new KeySearcherStopException();
                    }
                    return versionInUse;
                }
                else
                {
                    return 0;
                }
            }
            catch (KeySearcherStopException)
            {
                throw new KeySearcherStopException();
            }
        }

        public DateTime StartDate(String ofJobIdentifier)
        {
            var key = ofJobIdentifier + "_startdate";
            var requestResult = RetrieveWithStatistic(key);

            if (requestResult.IsSuccessful() && requestResult.Data != null)
            {
                var startTimeUtc = DateTime.SpecifyKind(
                    DateTime.FromBinary(BitConverter.ToInt64(requestResult.Data, 0)), DateTimeKind.Utc);
                return startTimeUtc.ToLocalTime();
            }

            StoreWithStatistic(key, BitConverter.GetBytes((DateTime.UtcNow.ToBinary())));
            return DateTime.Now;
        }

        public long SubmitterID(String ofJobIdentifier)
        {
            var key = ofJobIdentifier + "_submitterid";
            var requestResult = RetrieveWithStatistic(key);

            if (requestResult.IsSuccessful() && requestResult.Data != null)
            {
                var submitterid = BitConverter.ToInt64(requestResult.Data, 0);
                return submitterid;
            }

            StoreWithStatistic(key, BitConverter.GetBytes(Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetID()));
            return Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetID();
        }

        public RequestResult RetrieveWithStatistic(string key)
        {
            statusContainer.RetrieveRequests++;
            statusContainer.TotalDhtRequests++;
            var requestResult = P2PManager.Retrieve(key);

            if (requestResult.Data != null)
            {
                statusContainer.RetrievedBytes += requestResult.Data.Length;
                statusContainer.TotalBytes += requestResult.Data.Length;
            }

            return requestResult;
        }

        public RequestResult RemoveWithStatistic(string key)
        {
            statusContainer.RemoveRequests++;
            statusContainer.TotalDhtRequests++;
            return P2PManager.Remove(key);
        }

        public RequestResult StoreWithStatistic(string key, byte[] data)
        {
            statusContainer.StoreRequests++;
            statusContainer.TotalDhtRequests++;
            var requestResult = P2PManager.Store(key, data);

            if (requestResult.Data != null)
            {
                statusContainer.StoredBytes += requestResult.Data.Length;
                statusContainer.TotalBytes += requestResult.Data.Length;
            }

            return requestResult;
        }
    }
}
