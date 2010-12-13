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

//-----------------------------------------------------------------------------------
/*
        //Dictionary Tests
        public String Avatarname = "ct2";
        public Dictionary<String, Dictionary<long, int>> Activity;
*/ 
//-----------------------------------------------------------------------------------

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
//            Activity = new Dictionary<string, Dictionary<long, int>>();

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

            // TODO add check, if we retrieved our lock (e.g. by comparing the lock date or the future client identifier
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

//Integration of the Dictionary into the ParentNode
//-------------------------------------------------------------------------------------------------------
/*
            //Collection of all avatarnames in activity of this node
            Dictionary<String, Dictionary<long, int>>.KeyCollection keyColl = Activity.Keys;

            foreach (string avname in keyColl)
            {
                //taking the dictionary in this avatarname
                Dictionary<long, int> MaschCount = Activity[avname];

                //collecting der maschinID's for this avatarname
                Dictionary<long, int>.KeyCollection maschColl = MaschCount.Keys;

                //if the avatarname already exists in the parentnode.activity
                if (ParentNode.Activity.ContainsKey(avname))
                {
                    foreach (long id in maschColl)
                    {
                        //get the parent maschcount for this avatarname
                        Dictionary<long, int> ParentMaschCount = ParentNode.Activity[avname];

                        //if the id of the Maschine already exists for this avatarname
                        if (ParentMaschCount.ContainsKey(id))
                        {
                            ParentMaschCount[id] = ParentMaschCount[id] + MaschCount[id];

                            ParentNode.Activity[avname] = ParentMaschCount;
                        }
                        else
                        {
                            //add a new id,count value for this avatarname
                            ParentNode.Activity[avname].Add(id, MaschCount[id]);
                        }
                    }
                }
                else
                {   //add the maschinecount dictionary to this avatarname
                    ParentNode.Activity[avname] = MaschCount;
                }                
            }
*/
//------------------------------------------------------------------------------------------------------
            
        }

        private void UpdateDhtForRootNode()
        {
            StorageHelper.UpdateFromDht(this, true);
            StorageHelper.UpdateInDht(this);
        }
//-------------------------------------------------------------------------------------------------------
/*
        protected void UpdateActivity()
        {
            Dictionary<long, int> Maschine = new Dictionary<long, int> { { Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetID(), 1 } };
            Activity.Add(Avatarname, Maschine);
        }
*/
//--------------------------------------------------------------------------------------------------------
        public abstract bool IsReserved();

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
