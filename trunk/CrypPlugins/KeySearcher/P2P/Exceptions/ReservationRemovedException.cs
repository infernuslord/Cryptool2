using System;

namespace KeySearcher.P2P.Exceptions
{
    public class ReservationRemovedException : Exception
    {
        public ReservationRemovedException(string msg) : base(msg) {}
    }
}
