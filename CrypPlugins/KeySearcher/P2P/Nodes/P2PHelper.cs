using System;
using System.IO;
using Cryptool.P2P;

namespace KeySearcher.P2P.Nodes
{
    class P2PHelper
    {
        private KeySearcher _keySearcher;

        public P2PHelper(KeySearcher keySearcher)
        {
            _keySearcher = keySearcher;
        }

        internal static bool UpdateInDht(NodeBase nodeToUpdate)
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

        internal void UpdateFromDht(NodeBase nodeToUpdate)
        {
            if (nodeToUpdate.LastUpdate > DateTime.Now.Subtract(new TimeSpan(0, 0, 5)))
            {
                return;
            }

            var nodeBytes = P2PManager.Retrieve(KeyInDht(nodeToUpdate));

            if (nodeBytes == null)
            {
                return;
            }

            nodeToUpdate.LastUpdate = DateTime.Now;

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
                _keySearcher.IntegrateNewResults(nodeToUpdate.Result);
            }
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
            return string.Format("p2pjob_{0}_node_{1}_{2}", node.DistributedJobIdentifier, node.From, node.To);
        }
    }
}
