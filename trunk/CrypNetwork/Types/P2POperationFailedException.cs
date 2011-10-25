using System;

namespace Cryptool.P2P.Types
{
    public class P2POperationFailedException : Exception
    {
        public P2POperationFailedException(string message) : base(message)
        {
        }
    }
}
