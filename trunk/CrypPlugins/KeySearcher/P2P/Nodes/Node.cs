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
        }

        private void LoadOrUpdateChildNodes(bool ignoreReservation = false)
        {
            var middle = (From + To)/2;

            if (!LeftChildFinished)
            {
                if (_leftChild == null)
                {
                    _leftChild = NodeFactory.CreateNode(P2PHelper, KeyQualityHelper, this, From, middle,
                                                        DistributedJobIdentifier);
                }
                else
                {
                    P2PHelper.UpdateFromDht(_leftChild);
                }
            }

            // Only load right node, if the left one is finished or reserved
            var leftChildsReservationExistingAndNotIgnored = !LeftChildFinished && _leftChild.IsReserverd() && !ignoreReservation;
            if ((LeftChildFinished || !leftChildsReservationExistingAndNotIgnored) && !RightChildFinished)
            {
                if (_rightChild == null)
                {
                    _rightChild = NodeFactory.CreateNode(P2PHelper, KeyQualityHelper, this, middle + 1, To,
                                                         DistributedJobIdentifier);
                }
                else
                {
                    P2PHelper.UpdateFromDht(_rightChild);
                }
            }
        }

        public override bool IsCalculated()
        { 
            return LeftChildFinished && RightChildFinished;
        }

        public override NodeBase CalculatableNode(bool useReservedNodes)
        {
            if (IsCalculated())
            {
                return null;
            }

            LoadOrUpdateChildNodes(true);

            if ((LeftChildFinished || (_leftChild.IsReserverd() && !useReservedNodes)) && !RightChildFinished)
            {
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
            LoadOrUpdateChildNodes(true);

            var leftChildFinishedOrReserved = LeftChildFinished || _leftChild.IsReserverd();

            if (leftChildFinishedOrReserved && !RightChildFinished)
            {
                return _rightChild.IsReserverd();
            }

            return !LeftChildFinished && _leftChild.IsReserverd();
        }

        public override string ToString()
        {
            return base.ToString() + ", LeftChildFinished " + LeftChildFinished + ",  RightChildFinished " +
                   RightChildFinished;
        }
    }
}
