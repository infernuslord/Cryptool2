using System;
using System.Threading;
using PeersAtPlay.P2PStorage.DHT;

namespace Cryptool.P2P.Internal
{
    public class RequestResult
    {
        /// <summary>
        /// Key used for querying the DHT
        /// </summary>
        public string Key;

        /// <summary>
        /// Type of the request
        /// </summary>
        public RequestType RequestType;

        /// <summary>
        /// Result of the request as reported by Peers@Play
        /// 
        /// The result is an enum. If new result types are added by Peers@Play, 
        /// this field will be set to Unknown.
        /// </summary>
        public RequestResultType Status;

        /// <summary>
        /// Data used in the request.
        /// 
        /// For Store requests, this is the data that was attempted to store.
        /// For Retrieve requests, this is the data that was received.
        /// For Remove results, this is always <code>null</code>.
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// Unique identifier for each request. Is assigned by Peers@Play.
        /// </summary>
        public Guid Guid;

        /// <summary>
        /// Indicates if the request was successful. The value depends on the request type.
        /// 
        /// For Store, this is only true if Status equals Success.
        /// For Retrieve, this is only if Status equals Success or KeyNotFound.
        /// For Remove, this is only true if Status does not equal Failure or VersionMismatch.
        /// </summary>
        /// <returns>bool indicating the success of the request</returns>
        public bool IsSuccessful()
        {
            switch (RequestType)
            {
                case RequestType.Store:
                    return Status == RequestResultType.Success;
                case RequestType.Retrieve:
                    return Status == RequestResultType.Success || Status == RequestResultType.KeyNotFound;
                case RequestType.Remove:
                    return Status != RequestResultType.Failure && Status != RequestResultType.VersionMismatch;
                default:
                    return false;
            }
        }

        internal AutoResetEvent WaitHandle;

        internal void Parse(RemoveResult removeResult)
        {
            RequestType = RequestType.Remove;
            Guid = removeResult.Guid;
            ParseResultStatus(removeResult.Status);
        }

        internal void Parse(RetrieveResult retrieveResult)
        {
            RequestType = RequestType.Retrieve;
            Guid = retrieveResult.Guid;
            ParseResultStatus(retrieveResult.Status);

            if (Status == RequestResultType.Success)
                Data = retrieveResult.Data;
        }

        internal void Parse(StoreResult storeResult)
        {
            RequestType = RequestType.Store;
            Guid = storeResult.Guid;
            ParseResultStatus(storeResult.Status);
        }

        private void ParseResultStatus(OperationStatus status)
        {
            switch (status)
            {
                case OperationStatus.Failure:
                    Status = RequestResultType.Failure;
                    return;
                case OperationStatus.KeyNotFound:
                    Status = RequestResultType.KeyNotFound;
                    return;
                case OperationStatus.Success:
                    Status = RequestResultType.Success;
                    return;
                case OperationStatus.VersionMismatch:
                    Status = RequestResultType.VersionMismatch;
                    return;
                default:
                    Status = RequestResultType.Unknown;
                    return;
            }
        }
    }
}
