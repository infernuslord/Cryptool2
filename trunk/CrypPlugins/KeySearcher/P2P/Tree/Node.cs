using System.Numerics;
using KeySearcher.Helper;
using KeySearcher.P2P.Exceptions;
using KeySearcher.P2P.Storage;

namespace KeySearcher.P2P.Tree
{
    class Node : NodeBase
    {
        internal bool LeftChildFinished;
        internal bool RightChildFinished;

        private NodeBase leftChild;
        private NodeBase rightChild;

        public Node(StorageHelper storageHelper, KeyQualityHelper keyQualityHelper, Node parentNode, BigInteger @from, BigInteger to, string distributedJobIdentifier)
            : base(storageHelper, keyQualityHelper, parentNode, @from, to, distributedJobIdentifier)
        {
        }

        private void LoadOrUpdateChildNodes()
        {
            var middle = (From + To)/2;

            if (!LeftChildFinished)
            {
                if (leftChild == null)
                {
                    leftChild = NodeFactory.CreateNode(StorageHelper, KeyQualityHelper, this, From, middle,
                                                        DistributedJobIdentifier);
                }
                else
                {
                    StorageHelper.UpdateFromDht(leftChild);
                }
            }

            // Only load right node, if the left one is finished or reserved
            if ((LeftChildFinished || leftChild.IsReserverd()) && !RightChildFinished)
            {
                if (rightChild == null)
                {
                    rightChild = NodeFactory.CreateNode(StorageHelper, KeyQualityHelper, this, middle + 1, To,
                                                         DistributedJobIdentifier);
                }
                else
                {
                    StorageHelper.UpdateFromDht(rightChild);
                }
            }
        }

        public override bool IsCalculated()
        {
            return LeftChildFinished && RightChildFinished;
        }

        public override void Reset()
        {
            leftChild = null;
            rightChild = null;
        }

        public override Leaf CalculatableLeaf(bool useReservedNodes)
        {
            LoadOrUpdateChildNodes();

            // Left child not finished and not reserved (or reserved leafs are allowed)
            if (!LeftChildFinished && (!leftChild.IsReserverd() || useReservedNodes))
            {
                return leftChild.CalculatableLeaf(useReservedNodes);
            }

            if (rightChild == null)
            {
                throw new AlreadyCalculatedException();
            }

            return rightChild.CalculatableLeaf(useReservedNodes);
        }

        public void ChildFinished(NodeBase childNode)
        {
            if (childNode == leftChild)
            {
                LeftChildFinished = true;
                leftChild = null;
                return;
            }

            if (childNode == rightChild)
            {
                RightChildFinished = true;
                rightChild = null;
                return;
            }
        }

        public override bool IsReserverd()
        {
            LoadOrUpdateChildNodes();

            var leftChildFinishedOrReserved = LeftChildFinished || leftChild.IsReserverd();

            if (leftChildFinishedOrReserved && !RightChildFinished)
            {
                return rightChild.IsReserverd();
            }

            return !LeftChildFinished && leftChild.IsReserverd();
        }

        public override string ToString()
        {
            return base.ToString() + ", LeftChildFinished " + LeftChildFinished + ",  RightChildFinished " +
                   RightChildFinished;
        }
    }
}
