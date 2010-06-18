using System.Numerics;
using KeySearcher.Helper;
using KeySearcher.P2P.Storage;

namespace KeySearcher.P2P.Tree
{
    internal static class NodeFactory
    {
        public static NodeBase CreateNode(StorageHelper storageHelper, KeyQualityHelper keyQualityHelper,
                                          Node parentNode, BigInteger from, BigInteger to,
                                          string distributedJobIdentifier)
        {
            NodeBase newNode;

            if (from == to)
            {
                newNode = new Leaf(storageHelper, keyQualityHelper, parentNode, from, distributedJobIdentifier);
            }
            else
            {
                newNode = new Node(storageHelper, keyQualityHelper, parentNode, from, to, distributedJobIdentifier);
            }

            return newNode;
        }
    }
}