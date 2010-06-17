using System;
using System.Collections.Generic;
using System.Numerics;
using KeySearcher.Helper;
using KeySearcher.P2P.Exceptions;

namespace KeySearcher.P2P.Nodes
{
    class Leaf : NodeBase
    {
        internal DateTime LastReservationDate;

        public Leaf(P2PHelper p2PHelper, KeyQualityHelper keyQualityHelper, Node parentNode, BigInteger id, string distributedJobIdentifier)
            : base(p2PHelper, keyQualityHelper, parentNode, id, id, distributedJobIdentifier)
        {
        }

        public void HandleResults(LinkedList<KeySearcher.ValueKey> result)
        {
            Result = result;
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
                throw new AlreadyCalculatedException();
            }

            return this;
        }

        public override bool IsCalculated()
        {
            return Result.Count > 0;
        }

        public override void Reset()
        {
        }

        public bool ReserveLeaf()
        {
            LastReservationDate = DateTime.UtcNow;
            return P2PHelper.UpdateInDht(this).IsSuccessful();
        }

        public override bool IsReserverd()
        {
            var dateFiveMinutesBefore = DateTime.UtcNow.Subtract(new TimeSpan(0, 5, 0));
            var isReserverd = dateFiveMinutesBefore < LastReservationDate;
            return isReserverd;
        }

        public override string ToString()
        {
            return base.ToString() + ", last reservation date " + LastReservationDate;
        }
    }
}
