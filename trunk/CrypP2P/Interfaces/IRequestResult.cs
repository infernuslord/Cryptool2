using Cryptool.P2P.Types;

namespace Cryptool.P2P.Interfaces
{
    public interface IRequestResult
    {
        /// <summary>
        /// Indicates if the request was successful. The value depends on the request type.
        /// 
        /// For Store, this is only true if Status equals Success.
        /// For Retrieve, this is only if Status equals Success or KeyNotFound.
        /// For Remove, this is only true if Status does not equal Failure or VersionMismatch.
        /// </summary>
        /// <returns>bool indicating the success of the request</returns>
        bool IsSuccessful();

        byte[] GetData();

        RequestResultType GetStatus();
        RequestType GetRequestType();
    }
}