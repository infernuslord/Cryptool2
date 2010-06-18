using System;
using System.IO;
using Cryptool.P2P;
using Cryptool.P2P.Internal;
using KeySearcher.P2P.Tree;

namespace KeySearcher.P2P.Storage
{
    class StorageHelper
    {
        private readonly KeySearcher keySearcher;

        public StorageHelper(KeySearcher keySearcher)
        {
            this.keySearcher = keySearcher;
        }

        internal static RequestResult UpdateInDht(NodeBase nodeToUpdate)
        {
            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);

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
            }

            return P2PManager.Store(KeyInDht(nodeToUpdate), memoryStream.ToArray());
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
        }

        internal RequestResult UpdateFromDht(NodeBase nodeToUpdate, bool forceUpdate = false)
        {
            if (!forceUpdate && nodeToUpdate.LastUpdate > DateTime.Now.Subtract(new TimeSpan(0, 0, 5)))
            {
                return new RequestResult { Status = RequestResultType.Success };
            }

            nodeToUpdate.LastUpdate = DateTime.Now;

            var requestResult = P2PManager.Retrieve(KeyInDht(nodeToUpdate));
            var nodeBytes = requestResult.Data;

            if (nodeBytes == null)
            {
                return requestResult;
            }

            var binaryReader = new BinaryReader(new MemoryStream(nodeBytes));

            if (nodeToUpdate is Node)
            {
                UpdateNodeFromDht((Node) nodeToUpdate, binaryReader);
            } else
            {
                UpdateLeafFromDht((Leaf) nodeToUpdate, binaryReader);
            }

            // Load results
            var resultCount = binaryReader.ReadInt32();
            for (var i = 0; i < resultCount; i++)
            {
                var newResult = new KeySearcher.ValueKey
                                    {
                                        key = binaryReader.ReadString(),
                                        value = binaryReader.ReadDouble(),
                                        decryption = binaryReader.ReadBytes(binaryReader.ReadInt32())
                                    };
                nodeToUpdate.Result.AddLast(newResult);
            }

            if (resultCount > 0)
            {
                keySearcher.IntegrateNewResults(nodeToUpdate.Result);
            }

            return requestResult;
        }

        private static void UpdateNodeFromDht(Node nodeToUpdate, BinaryReader binaryReader)
        {
            nodeToUpdate.LeftChildFinished = binaryReader.ReadBoolean() || nodeToUpdate.LeftChildFinished;
            nodeToUpdate.RightChildFinished = binaryReader.ReadBoolean() || nodeToUpdate.RightChildFinished;
        }

        private static void UpdateLeafFromDht(Leaf nodeToUpdate, BinaryReader binaryReader)
        {
            var date = DateTime.FromBinary(binaryReader.ReadInt64());
            if (date > nodeToUpdate.LastReservationDate)
            {
                nodeToUpdate.LastReservationDate = date;
            }
        }

        internal static string KeyInDht(NodeBase node)
        {
            return string.Format("{0}_node_{1}_{2}", node.DistributedJobIdentifier, node.From, node.To);
        }
    }
}
