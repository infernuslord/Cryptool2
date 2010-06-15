using System.Numerics;
using KeySearcher.Helper;

namespace KeySearcher.P2P.Nodes
{
    static class NodeFactory
    {
        public static NodeBase CreateNode(P2PHelper p2PHelper, KeyQualityHelper keyQualityHelper, Node parentNode, BigInteger from, BigInteger to, string distributedJobIdentifier)
        {
            NodeBase newNode;
            
            if (from == to)
            {
                newNode = new Leaf(p2PHelper, keyQualityHelper, parentNode, from, distributedJobIdentifier);
            } else
            {
                newNode = new Node(p2PHelper, keyQualityHelper, parentNode, from, to, distributedJobIdentifier);
            }

            return newNode;
        }
    }
}
