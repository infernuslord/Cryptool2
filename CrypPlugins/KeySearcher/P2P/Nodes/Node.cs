using System.Numerics;
using KeySearcher.Helper;
using KeySearcher.P2P.Exceptions;

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

        private void LoadOrUpdateChildNodes()
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
            if ((LeftChildFinished || _leftChild.IsReserverd()) && !RightChildFinished)
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

        public override void Reset()
        {
            _leftChild = null;
            _rightChild = null;
            LeftChildFinished = false;
            RightChildFinished = false;
        }

        public override Leaf CalculatableLeaf(bool useReservedNodes)
        {
            LoadOrUpdateChildNodes();

            // Left child not finished and not reserved (or reserved leafs are allowed)
            if (!LeftChildFinished && (!_leftChild.IsReserverd() || useReservedNodes))
            {
                return _leftChild.CalculatableLeaf(useReservedNodes);
            }

            if (_rightChild == null)
            {
                throw new AlreadyCalculatedException();
            }

            return _rightChild.CalculatableLeaf(useReservedNodes);
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
            LoadOrUpdateChildNodes();

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
