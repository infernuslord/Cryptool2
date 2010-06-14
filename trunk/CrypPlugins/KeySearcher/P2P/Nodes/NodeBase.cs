using System;
using System.Collections.Generic;
using System.Numerics;
using Cryptool.P2P;
using KeySearcher.Helper;

namespace KeySearcher.P2P.Nodes
{
    abstract class NodeBase
    {
        protected internal readonly BigInteger From;
        protected internal readonly BigInteger To;
        protected internal readonly string DistributedJobIdentifier;
        protected readonly P2PHelper P2PHelper;
        protected readonly KeyQualityHelper KeyQualityHelper;

        protected internal DateTime LastUpdate;

        private readonly KeySearcher _keySearcher;
        public readonly Node ParentNode;
        public LinkedList<KeySearcher.ValueKey> Result;

        protected NodeBase(P2PHelper p2PHelper, KeyQualityHelper keyQualityHelper, Node parentNode, BigInteger @from, BigInteger to, string distributedJobIdentifier)
        {
            P2PHelper = p2PHelper;
            KeyQualityHelper = keyQualityHelper;
            ParentNode = parentNode;
            From = @from;
            To = to;
            DistributedJobIdentifier = distributedJobIdentifier;

            LastUpdate = DateTime.MinValue;
            Result = new LinkedList<KeySearcher.ValueKey>();

            P2PHelper.UpdateFromDht(this);
        }

        protected void UpdateDht()
        {
            // If this instance is the root node, we need a special handling
            if (ParentNode == null)
            {
                UpdateDhtForRootNode();
                return;
            }

            // Compare with refreshed parent node
            P2PHelper.UpdateFromDht(ParentNode);

            var bestValue = double.MinValue;
            if (ParentNode.Result.Count > 0)
            {
                bestValue = ParentNode.Result.First.Value.value;
            }

            var revertedResults = new LinkedList<KeySearcher.ValueKey>();
            foreach (var valueKey in Result)
            {
                revertedResults.AddFirst(valueKey);
            }

            // TODO eventuell wird nur der beste Eintrag übernommen?
            foreach (var valueKey in revertedResults)
            {
                if (!KeyQualityHelper.IsBetter(valueKey.value, bestValue)) continue;

                if (ParentNode.Result.Contains(valueKey)) continue;

                ParentNode.Result.AddFirst(valueKey);
                bestValue = valueKey.value;

                if (ParentNode.Result.Count > 10)
                {
                    ParentNode.Result.RemoveLast();
                }
            }

            ParentNode.ChildFinished(this);

            P2PHelper.UpdateInDht(ParentNode);
            P2PManager.Retrieve(P2PHelper.KeyInDht(this));
            P2PManager.Remove(P2PHelper.KeyInDht(this));

            if (ParentNode.IsCalculated)
            {
                ParentNode.UpdateDht();
            }
        }

        private void UpdateDhtForRootNode()
        {
            P2PHelper.UpdateFromDht(this);
            P2PHelper.UpdateInDht(this);
        }

        public abstract bool IsReserverd();

        public abstract NodeBase CalculatableNode(bool useReservedNodes);
    }
}
