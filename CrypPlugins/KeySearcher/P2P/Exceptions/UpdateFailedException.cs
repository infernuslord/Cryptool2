using System;

namespace KeySearcher.P2P.Exceptions
{
    class UpdateFailedException : Exception
    {
        public UpdateFailedException(string msg) : base(msg) {}
    }
}
