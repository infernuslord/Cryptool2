using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;
using Cryptool.P2P;
using Cryptool.P2P.Internal;
using Cryptool.PluginBase;
using KeySearcher.P2P.Exceptions;
using KeySearcher.P2P.Presentation;
using KeySearcher.P2P.Tree;
using InvalidDataException = KeySearcher.P2P.Exceptions.InvalidDataException;

namespace KeySearcher.P2P.Storage
{
    class StorageHelper
    {
        private readonly KeySearcher keySearcher;
        private readonly StatisticsGenerator statisticsGenerator;
        private readonly StatusContainer statusContainer;

        //VERSIONNUMBER: Important. Set it +1 manually everytime the length of the MemoryStream Changes
        private const int version = 2;

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
            binaryWriter.Write('V');
            binaryWriter.Write(version);  

            // Append results
            binaryWriter.Write(nodeToUpdate.Result.Count);
            foreach (var valueKey in nodeToUpdate.Result)
            {
                binaryWriter.Write(valueKey.key);
                binaryWriter.Write(valueKey.keya.Length);
                binaryWriter.Write(valueKey.keya);
                binaryWriter.Write(valueKey.value);
                binaryWriter.Write(valueKey.decryption.Length);
                binaryWriter.Write(valueKey.decryption);

                binaryWriter.Write(valueKey.user);
                var buffertime = valueKey.time.ToBinary();
                binaryWriter.Write(buffertime);
                binaryWriter.Write(valueKey.maschid);
                binaryWriter.Write(valueKey.maschname);
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
            
            if (nodeToUpdate is Node)
            {
                binaryWriter.Write((byte)1);
                UpdateNodeInDht((Node)nodeToUpdate, binaryWriter);
            }
            else
            {
                binaryWriter.Write((byte)0);
                UpdateLeafInDht((Leaf)nodeToUpdate, binaryWriter);
            }

            //return StoreWithStatistic(KeyInDht(nodeToUpdate), memoryStream.ToArray());
            return StoreWithReplicationAndHashAndStatistic(nodeToUpdate.DistributedJobIdentifier, KeyInDht(nodeToUpdate), memoryStream.ToArray(), 3);
        }

        private RequestResult StoreWithHashAndStatistic(string jobid, string keyInDht, byte[] data)
        {
            SHA256 shaM = new SHA256Managed();
            List<byte> b = new List<byte>();
            b.AddRange(data);
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            b.AddRange(enc.GetBytes(jobid));
            var hash = shaM.ComputeHash(b.ToArray());
            List<byte> result = new List<byte>();
            result.AddRange(data);
            result.AddRange(hash);
            return StoreWithStatistic(keyInDht, result.ToArray());
        }

        private RequestResult StoreWithReplicationAndHashAndStatistic(string jobid, string keyInDht, byte[] data, int replications)
        {
            try
            {
                //store the "real" entry:
                var res = StoreWithHashAndStatistic(jobid, keyInDht, data);

                //store all replications:
                for (int c = 1; c < replications; c++)
                {
                    var rkey = keyInDht + "_" + c;
                    var req = StoreWithHashAndStatistic(jobid, rkey, data);

                    if (req.Status == RequestResultType.VersionMismatch)
                    {
                        //Version mismatch, so try again... if it still fails, ignore it...
                        P2PManager.Retrieve(rkey);
                        StoreWithHashAndStatistic(jobid, rkey, data);
                    }
                }

                return res;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool InvalidHash(string jobid, byte[] nodeBytes)
        {
            try
            {
                if (nodeBytes == null || nodeBytes.Length < 32)
                    return true;

                List<byte> b = new List<byte>();
                b.AddRange(nodeBytes);
                byte[] hash = b.GetRange(nodeBytes.Length - 32, 32).ToArray();

                SHA256 shaM = new SHA256Managed();
                List<byte> c = b.GetRange(0, nodeBytes.Length - 32);
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                c.AddRange(enc.GetBytes(jobid));
                byte[] packethash = shaM.ComputeHash(c.ToArray());

                for (int i = 0; i < 32; i++)
                {
                    if (hash[i] != packethash[i])
                        return true;
                }

                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private static void UpdateNodeInDht(Node nodeToUpdate, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(nodeToUpdate.LeftChildFinished);
            binaryWriter.Write(nodeToUpdate.RightChildFinished);
        }

        private static void UpdateLeafInDht(Leaf nodeToUpdate, BinaryWriter binaryWriter)
        {
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

                var requestResult = RetrieveWithReplicationAndHashAndStatistic(KeyInDht(nodeToUpdate), nodeToUpdate.DistributedJobIdentifier, 3);
                if (requestResult == null)
                    return null;

                var nodeBytes = requestResult.Data;
                if (nodeBytes == null)
                {
                    return requestResult;
                }

                var binaryReader = new BinaryReader(new MemoryStream(nodeBytes));

                //oldVersionFlag will be used to garantee further changes in the Stream
                var oldVersionFlag = CheckNodeVersion(binaryReader);

                // Load results
                var resultCount = binaryReader.ReadInt32();

                for (var i = 0; i < resultCount; i++)
                {
                    var key = binaryReader.ReadString();
                    int keyaLength = binaryReader.ReadInt32();
                    var newResult = new KeySearcher.ValueKey
                                        {
                                                key = key,
                                                keya = binaryReader.ReadBytes(keyaLength),
                                                value = binaryReader.ReadDouble(),
                                                decryption = binaryReader.ReadBytes(binaryReader.ReadInt32()),
                                                user = binaryReader.ReadString(),
                                                time = DateTime.FromBinary(binaryReader.ReadInt64()),
                                                maschid = binaryReader.ReadInt64(),
                                                maschname = binaryReader.ReadString()
                                            };
                    nodeToUpdate.PushToResults(newResult);
                }

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

                byte type = binaryReader.ReadByte();

                if (type == 1 && nodeToUpdate is Node)
                {
                    UpdateNodeFromDht((Node)nodeToUpdate, binaryReader);
                }
                else if (type == 0 && nodeToUpdate is Leaf)
                {
                    UpdateLeafFromDht((Leaf)nodeToUpdate, binaryReader);
                }
                else
                {
                    throw new Exception("Inconsistent type");
                }

                
                if (resultCount > 0 && keySearcher != null && statisticsGenerator != null)
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
                //There are some kinds of exceptions, which should not cause removal of DHT entries:
                if (e is KeySearcherStopException || e is NotConnectedException || e is P2POperationFailedException)
                    throw;

                //For all others, delete the problematic subtree:
                if (keySearcher != null)
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
            try
            {   
                var date = DateTime.FromBinary(binaryReader.ReadInt64());
                if (date > nodeToUpdate.LastReservationDate)
                {
                    nodeToUpdate.LastReservationDate = date;
                }
                nodeToUpdate.setClientIdentifier(binaryReader.ReadInt64());
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

        internal static string KeyInDht(string distributedJobIdentifier, BigInteger from, BigInteger to)
        {
            return string.Format("{0}_node_{1}_{2}", distributedJobIdentifier, from, to);
        }

        private static int CheckNodeVersion(BinaryReader binaryReader)
        {
            try
            {
                //Reading the char and the versionnumber
                char magic = binaryReader.ReadChar();
                if (magic != 86) //V infront of a Node
                {
                    throw new Exception();
                }
                var versionInUse = binaryReader.ReadInt32();
                if (versionInUse > version) //Check if a newer Version is in use
                {
                    throw new KeySearcherStopException();
                }
                return versionInUse;
            }
            catch (KeySearcherStopException)
            {
                throw new KeySearcherStopException();
            }
        }

        public DateTime StartDate(String ofJobIdentifier)
        {
            var key = ofJobIdentifier + "_startdate";
            try
            {
                var requestResult = RetrieveWithReplicationAndHashAndStatistic(key, ofJobIdentifier, 3);

                if (requestResult != null && requestResult.IsSuccessful() && requestResult.Data != null && requestResult.Data.Length >= 8)
                {
                    var startTimeUtc = DateTime.SpecifyKind(DateTime.FromBinary(BitConverter.ToInt64(requestResult.Data, 0)), DateTimeKind.Utc);
                    return startTimeUtc.ToLocalTime();
                }
            }
            catch
            {
                //No valid startdate available
            }

            StoreWithReplicationAndHashAndStatistic(ofJobIdentifier, key, BitConverter.GetBytes((DateTime.UtcNow.ToBinary())), 3);
            return DateTime.Now;
        }

        public long SubmitterID(String ofJobIdentifier)
        {
            var key = ofJobIdentifier + "_submitterid";

            try
            {
                var requestResult = RetrieveWithReplicationAndHashAndStatistic(key, ofJobIdentifier, 3);

                if (requestResult != null && requestResult.IsSuccessful() && requestResult.Data != null && requestResult.Data.Length >= 8)
                {
                    var submitterid = BitConverter.ToInt64(requestResult.Data, 0);
                    return submitterid;
                }
            }
            catch
            {
                //No valid SubmitterID available
            }

            StoreWithReplicationAndHashAndStatistic(ofJobIdentifier, key, BitConverter.GetBytes(Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetID()), 3);
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

        public RequestResult RetrieveWithHashAndStatistic(string key, string distributedJobIdentifier)
        {
            var result = RetrieveWithStatistic(key);

            if (result == null || result.Data == null)
            {
                return null;
            }
            if (InvalidHash(distributedJobIdentifier, result.Data))
            {
                throw new InvalidDataException("Invalid Hash");
            }
            return result;
        }

        public RequestResult RetrieveWithReplicationAndHashAndStatistic(string key, string distributedJobIdentifier, int replications)
        {
            bool invalid = false;

            if (replications > 0)
            {
                int c = 0;
                while (c < replications)
                {
                    try
                    {
                        string ext = "";
                        if (c > 0)
                            ext = "_" + c;
                        var req = RetrieveWithHashAndStatistic(key + ext, distributedJobIdentifier);
                        if (req != null)
                        {
                            if (c > 0)
                            {
                                //The primary replication seems to be broken, so refresh it:
                                StoreWithReplicationAndHashAndStatistic(distributedJobIdentifier, key, req.Data,
                                                                        replications);
                            }
                            return req;
                        }
                    }
                    catch (InvalidDataException)
                    {
                        invalid = true;
                    }
                    c++;
                }
            }

            if (invalid)
                throw new InvalidDataException("Invalid data found when trying to retrieve packet.");

            //no valid replication found :(
            return null;
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
