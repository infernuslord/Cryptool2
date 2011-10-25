using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.P2P.Internal
{
    public class P2POperationFailedException : Exception
    {
        public P2POperationFailedException(string message) : base(message)
        {
        }
    }
}
