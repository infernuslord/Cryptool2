using System.Numerics;
using KeySearcher.Helper;

namespace KeySearcher.P2P.Nodes
{
    class Node : NodeBase
    {
        internal bool LeftChildFinished;
        internal bool RightChildFinished;

        private NodeBase _leftChild;
        private NodeBase _rightChild;

        public Node(P2PHelper p2PHelper, KeyQualityHelper keyQualityHelper, Node parentNode, BigInteger @from, BigInteger to, string distributedJobIdentifier)
            : base(p2PHelper, keyQualityHelper, parentNode, @from, to, distributedJobIdentifier)
        {
            if (!LeftChildFinished && !RightChildFinished)
            {
                FindChildNodes();
            }
        }

        private void FindChildNodes()
        {
            var middle = (From + To)/2;
            if (_leftChild == null)
            {
                _leftChild = NodeFactory.CreateNode(P2PHelper, KeyQualityHelper, this, From, middle, DistributedJobIdentifier);
            } 
            else
            {
                if (!LeftChildFinished)
                    P2PHelper.UpdateFromDht(_leftChild);
            }

            // Only load right node, if the left one is finished or reserved
            if (LeftChildFinished || _leftChild.IsReserverd())
            {
                if (_rightChild == null)
                {
                    _rightChild = NodeFactory.CreateNode(P2PHelper, KeyQualityHelper, this, middle + 1, To, DistributedJobIdentifier);
                }
                else
                {
                    if (!RightChildFinished)
                        P2PHelper.UpdateFromDht(_rightChild);
                }
            }
        }

        public bool IsCalculated
        { 
            get
            {
                return LeftChildFinished && RightChildFinished;
            }
        }

        public override NodeBase CalculatableNode(bool useReservedNodes)
        {
            if (LeftChildFinished && RightChildFinished)
            {
                return null;
            }

            if (LeftChildFinished || (_leftChild.IsReserverd() && !useReservedNodes))
            {
                if (_rightChild == null)
                {
                    FindChildNodes();
                }

                return _rightChild.CalculatableNode(useReservedNodes);
            }

            return _leftChild.CalculatableNode(useReservedNodes);
        }

        

        public void ChildFinished(NodeBase childNode)
        {
            if (childNode == _leftChild)
            {
                LeftChildFinished = true;
                _leftChild = null;
                return;
            }

            if (childNode == _rightChild)
            {
                RightChildFinished = true;
                _rightChild = null;
                return;
            }
        }


        public override bool IsReserverd()
        {
            if (LeftChildFinished || _leftChild.IsReserverd())
            {
                if (_rightChild == null)
                {
                    FindChildNodes();
                }

                return _rightChild.IsReserverd();
            }

            return false;
        }
    }
}
