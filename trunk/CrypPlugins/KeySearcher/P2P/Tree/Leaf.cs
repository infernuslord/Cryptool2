using System;
using System.Collections.Generic;
using System.Numerics;
using Cryptool.P2P;
using KeySearcher.Helper;
using KeySearcher.P2P.Exceptions;
using KeySearcher.P2P.Storage;
using KeySearcher.Properties;

namespace KeySearcher.P2P.Tree
{
    class Leaf : NodeBase
    {
        internal DateTime LastReservationDate;
        private bool isLeafReserved;
        private const int RESERVATIONTIMEOUT = 30;
        private long clientIdentifier = -1;

        public Leaf(StorageHelper storageHelper, KeyQualityHelper keyQualityHelper, Node parentNode, BigInteger id, string distributedJobIdentifier)
            : base(storageHelper, keyQualityHelper, parentNode, id, id, distributedJobIdentifier)
        {
        }

        public void HandleResults(LinkedList<KeySearcher.ValueKey> result, Int64 id, String hostname)
        {
            Result = result;
            this.id = id;
            this.hostname = hostname;
            UpdateDht();
        }

        public BigInteger PatternId()
        {
            return From;
        }

        public override Leaf CalculatableLeaf(bool useReservedNodes)
        {
            if (IsCalculated())
            {
                Reset();
            }

            return this;
        }

        public override bool IsCalculated()
        {
            return Result.Count > 0;
        }

        public override void Reset()
        {
            //ParentNode.Reset();
            Result.Clear();
            Activity.Clear();
            var reqRes = StorageHelper.UpdateInDht(this);
            if (reqRes == null || !reqRes.IsSuccessful())
                throw new InvalidOperationException(string.Format("Writing leaf {0} failed!", this));
        }

        public override void UpdateCache()
        {
            var dateSomeMinutesBefore = DateTime.UtcNow.Subtract(new TimeSpan(0, RESERVATIONTIMEOUT, 0));
            isLeafReserved = dateSomeMinutesBefore < LastReservationDate;
        }

        public bool ReserveLeaf()
        {
            LastReservationDate = DateTime.UtcNow;
            clientIdentifier = Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetID();
            var reqRes = StorageHelper.UpdateInDht(this);
            return (reqRes != null) && (reqRes.IsSuccessful());
        }

        public void GiveLeafFree()
        {
            StorageHelper.UpdateFromDht((this));          
            //Only give leaf free, if the reservation is still ours:
            if (clientIdentifier == Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetID())
            {
                LastReservationDate = new DateTime(0);
                clientIdentifier = -1;
                isLeafReserved = false;
                StorageHelper.UpdateInDht(this);
            }
        }

        public override bool IsReserved()
        {
            return isLeafReserved;
        }

        public override string ToString()
        {
            return base.ToString() + Resources.__last_reservation_date_ + LastReservationDate;
        }

        public long getClientIdentifier()
        {
            return clientIdentifier;
        }

        public void setClientIdentifier(long id)
        {
            clientIdentifier = id;
        }
    }
}
