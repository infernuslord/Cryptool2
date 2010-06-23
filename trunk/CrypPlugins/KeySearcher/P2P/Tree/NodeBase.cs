using System;
using System.Collections.Generic;
using System.Numerics;
using Cryptool.P2P;
using Cryptool.P2P.Internal;
using KeySearcher.Helper;
using KeySearcher.P2P.Exceptions;
using KeySearcher.P2P.Storage;

namespace KeySearcher.P2P.Tree
{
    abstract class NodeBase
    {
        protected internal readonly BigInteger From;
        protected internal readonly BigInteger To;
        protected internal readonly string DistributedJobIdentifier;
        protected readonly StorageHelper StorageHelper;
        protected readonly KeyQualityHelper KeyQualityHelper;

        protected internal DateTime LastUpdate;

        public readonly Node ParentNode;
        public LinkedList<KeySearcher.ValueKey> Result;

        protected NodeBase(StorageHelper storageHelper, KeyQualityHelper keyQualityHelper, Node parentNode, BigInteger @from, BigInteger to, string distributedJobIdentifier)
        {
            StorageHelper = storageHelper;
            KeyQualityHelper = keyQualityHelper;
            ParentNode = parentNode;
            From = @from;
            To = to;
            DistributedJobIdentifier = distributedJobIdentifier;

            LastUpdate = DateTime.MinValue;
            Result = new LinkedList<KeySearcher.ValueKey>();

            StorageHelper.UpdateFromDht(this);
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
            var result = StorageHelper.UpdateFromDht(ParentNode, true);
            if (!result.IsSuccessful())
            {
                throw new UpdateFailedException("Parent node could not be updated: " + result.Status);
            }

            IntegrateResultsIntoParent();
            ParentNode.ChildFinished(this);
            ParentNode.UpdateCache();

            if (StorageHelper.RetrieveWithStatistic(StorageHelper.KeyInDht(this)).Status == RequestResultType.KeyNotFound)
            {
                throw new ReservationRemovedException("Before updating parent node, this leaf's reservation was deleted.");
            }

            var updateResult = StorageHelper.UpdateInDht(ParentNode);
            if (!updateResult.IsSuccessful())
            {
                throw new UpdateFailedException("Parent node could not be updated: " + updateResult.Status);
            }

            StorageHelper.RemoveWithStatistic(StorageHelper.KeyInDht(this));

            if (ParentNode.IsCalculated())
            {
                ParentNode.UpdateDht();
            }
        }

        private void IntegrateResultsIntoParent()
        {
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
        }

        private void UpdateDhtForRootNode()
        {
            StorageHelper.UpdateFromDht(this, true);
            StorageHelper.UpdateInDht(this);
        }

        public abstract bool IsReserverd();

        public abstract Leaf CalculatableLeaf(bool useReservedNodes);

        public abstract bool IsCalculated();

        public abstract void Reset();

        public abstract void UpdateCache();

        public override string ToString()
        {
            return "NodeBase " + GetType() + ", from " + From + " to " + To;
        }
    }
}
