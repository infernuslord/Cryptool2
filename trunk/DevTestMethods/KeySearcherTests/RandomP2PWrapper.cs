using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.P2P.Internal;
using KeySearcher.P2P.Exceptions;

namespace KeySearcher.P2P.UnitTests
{
    /// <summary>
    /// Simulates a very error prone P2P connection (DHT Operations)
    /// </summary>
    class RandomP2PWrapper : P2PManagerWrapper
    {
        private Dictionary<string, byte[]> dht = new Dictionary<string, byte[]>();
        private Random random = new Random();

        private bool OperationSuccess()
        {
            //return true;
            //1% error probability
            return random.Next(0, 1000) > 1;
        }

        public override RequestResult Retrieve(string key)
        {
            if (OperationSuccess())
            {
                if (dht.ContainsKey(key))
                {
                    var rr = new RequestResult();
                    rr.Status = RequestResultType.Success;
                    rr.RequestType = RequestType.Retrieve;
                    rr.Data = dht[key];
                    return rr;
                }
                else
                {
                    var rr = new RequestResult();
                    rr.Status = RequestResultType.KeyNotFound;
                    rr.RequestType = RequestType.Retrieve;
                    rr.Data = null;
                    return rr;
                }
            }
            else
            {
                //Simulate some kind of error:
                switch (random.Next(0, 4))
                {
                    case 0:
                        return null;
                    case 1:
                        var rr = new RequestResult();
                        rr.RequestType = RequestType.Retrieve;
                        rr.Status = RequestResultType.Failure;
                        return rr;
                    case 2:
                        rr = new RequestResult();
                        rr.RequestType = RequestType.Retrieve;
                        rr.Status = RequestResultType.Unknown;
                        return rr;
                    case 3:
                        ThrowRandomException();
                        break;
                    //case 4:
                    //    rr = new RequestResult();
                    //    rr.RequestType = RequestType.Retrieve;
                    //    rr.Status = RequestResultType.Success;
                    //    rr.Data = new byte[random.Next(10, 100)];
                    //    return rr;
                    default:
                        throw new Exception();
                }
            }
            return null;
        }

        public override RequestResult Store(string key, byte[] data)
        {
            if (OperationSuccess())
            {
                dht[key] = data;
                //dht.Add(key, data);
                var rr = new RequestResult();
                rr.Status = RequestResultType.Success;
                rr.RequestType = RequestType.Store;
                return rr;
            }
            else
            {
                //Simulate some kind of error:
                switch (random.Next(0, 4))
                {
                    case 0:
                        return null;
                    case 1:
                        var rr = new RequestResult();
                        rr.RequestType = RequestType.Store;
                        rr.Status = RequestResultType.VersionMismatch;
                        return rr;
                    case 2:
                        rr = new RequestResult();
                        rr.RequestType = RequestType.Store;
                        rr.Status = RequestResultType.Unknown;
                        return rr;
                    case 3:
                        ThrowRandomException();
                        break;
                    default:
                        throw new Exception();
                }
            }
            return null;
        }

        public override RequestResult Remove(string key)
        {
            if (OperationSuccess())
            {
                if (dht.ContainsKey(key))
                {
                    var rr = new RequestResult();
                    rr.Status = RequestResultType.Success;
                    rr.RequestType = RequestType.Remove;
                    if (OperationSuccess())
                        dht.Remove(key);
                    return rr;
                }
                else
                {
                    var rr = new RequestResult();
                    rr.Status = RequestResultType.KeyNotFound;
                    rr.RequestType = RequestType.Remove;
                    rr.Data = null;
                    return rr;
                }
            }
            else
            {
                //Simulate some kind of error:
                switch (random.Next(0, 4))
                {
                    case 0:
                        return null;
                    case 1:
                        var rr = new RequestResult();
                        rr.RequestType = RequestType.Remove;
                        rr.Status = RequestResultType.Failure;
                        return rr;
                    case 2:
                        rr = new RequestResult();
                        rr.RequestType = RequestType.Remove;
                        rr.Status = RequestResultType.KeyNotFound;
                        return rr;
                    case 3:
                        ThrowRandomException();
                        break;
                    default:
                        throw new Exception();
                }
            }
            return null;
        }

        private void ThrowRandomException()
        {
            switch (random.Next(0, 2))
            {
                case 0:
                    throw new P2POperationFailedException("");
                case 1:
                    throw new NotConnectedException();
                //case 2:
                //    throw new ReservationRemovedException("");
            }
        }
    }
}
