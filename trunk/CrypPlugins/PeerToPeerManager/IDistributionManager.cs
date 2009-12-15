using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/* At present not used anywhere. 
 * But the classes P2PManagerBase and P2PWorkerBase
 * will be reworked on this base!
 */

namespace Cryptool.Plugins.PeerToPeer
{
    /* INTERFACE INFORMATION:
     * A general interface for a distributed Manager, who has to
     * distribute sub-jobs of a dispersible task.
     * To use the P2P infrastructure, derive from P2PManagerBase,
     * which again is derived from P2PPublisherBase (the publishing
     * part of a p2p-based Publish-Subscribe-infrastructure) */ 

    /* DEPLOYMENT EXAMPLE:
     * Valid Processing Flow: DisperseJob --> (waiting for workerAvailableEvent)
     *             --> AllocateJobToWorker --> (waiting for incoming data) 
     *             --> DeserializeReceivedJobResult --> ProcessReceivedJobResult
     * Remaining management: Wait for WorkerAvailableEvent and handle WorkerRemovedEvent */
    interface IDistributionManager
    {
        /// <summary>
        /// Disperses the common job into sub-jobs which can be distributed to different working peers.
        /// Implement the whole Job-Dispersion here and return the sub jobs in a Queue data structure
        /// </summary>
        /// <returns>the sub jobs in a Queue data structure</returns>
        Queue<object> DisperseJob();
        /// <summary>
        /// If a new worker joins the task-solution-union or an old worker again gets 
        /// free, allocate a new job to it, if any one is left.
        /// </summary>
        /// <param name="workerId">Id of the peer which you want to allocate a job to</param>
        /// <param name="subJob">The sub-job you want to allocate to the peer</param>
        /// <returns>true, if allocating was successful, otherwise false</returns>
        bool AllocateJobToWorker(PeerId workerId, object subJob);
        /// <summary>
        /// The case could happen, that a worker gets down (french leave, etc.), so we
        /// have to handle this case. Check if removed peer was processing a job at present.
        /// In this case enqueue its allocated job to the JobQueue and delete all its 
        /// information in this class, because he is down, but could rejoin the 
        /// task-solution-union again.
        /// </summary>
        /// <param name="workerId">Id of the removed peer</param>
        /// <returns>true, if checking and removing worker was exception-free.</returns>
        bool HandleRemovedWorker(PeerId workerId);
        /// <summary>
        /// The incoming job result is in a serialized form. So we first
        /// have to deserialize the job result, to process it accurately.
        /// </summary>
        /// <param name="sSerializedData">String which contains the job result object</param>
        /// <returns>the deserialized job result object</returns>
        object DeserializeReceivedJobResult(string sSerializedData);
        /// <summary>
        /// After deserializing the incoming sub-job result, we can
        /// process it in this method (e.g. actualize the Top-Result-List,
        /// try to break the task with this result, validate the result)
        /// </summary>
        /// <param name="deserializedJobResult">the deserialized (!) sub-job result</param>
        void ProcessReceivedJobResult(object deserializedJobResult);
    }

    /* INTERFACE INFORMATION:
     * A general interface for a distributed Worker, who has to
     * process distributed sub-jobs of a dispersible task.
     * To use the P2P infrastructure, derive from P2PWorkerBase,
     * which again is derived from P2PSubscriberBase (the subscribing
     * part of a p2p-based Publish-Subscribe-infrastructure) */

    /* DEPLOYMENT EXAMPLE:
     * Valid Processing Flow: (waiting for incoming data) --> ValidateIncomingData 
     *             --> Try to DeserializeIncomingSubJob --> StartProcessing
     *             --> (wait for external processingEnded-Event) --> Processing Ended
     *             --> SerializeSubJobResult --> (send solution to the manager) */
    interface IDistributedWorker
    {
        /// <summary>
        /// Differentiate between nonrelevant Publish-Subscribe-messages and
        /// for job processing relevant data. So try to deserialize the data
        /// and check validation. If data is a sub-job, forward it to the 
        /// processing method, otherwise use the base methods of the inherited
        /// classes
        /// </summary>
        /// <param name="senderId">Id of the sending peer (if necessary the managing peer)</param>
        /// <param name="sData">Either Publish-Subscribe data or a serialized sub-job</param>
        void ValidateIncomingData(PeerId senderId, string sData);
        /// <summary>
        /// Try to deserialize string to a valid sub job. If this is possible,
        /// return the valid object, otherwise return null, so the calling 
        /// method could differ between a job or other relevant information.
        /// </summary>
        /// <param name="sSubJob">serialized sub job</param>
        /// <returns>if serialized string could be shaped to a valid object, return it, otherwise return null</returns>
        object DeserializeIncomingSubJob(string sSubJob);
        /// <summary>
        /// When validity of subJob was tested and the subJob object was deserialized,
        /// this method will start processing the subjob
        /// </summary>
        /// <param name="subJob">deserialized and valid sub-job</param>
        void StartProcessing(object subJob);
        /// <summary>
        /// When processing the sub-job is ended, this method have to be invoked.
        /// Invoking this method does not neccessarily imply, that processing
        /// was successful. It could also be canceled because of an user action or
        /// an exception. So we must check validity of the result seperately.
        /// </summary>
        /// <param name="result"></param>
        void ProcessingEnded(object result);
        /// <summary>
        /// For sending the result to the Manager we have to serialize
        /// the result object. On the managing side you have to implement
        /// a contrary deserialization.
        /// </summary>
        /// <param name="result">the valid sub-job result</param>
        /// <returns>the serialized sub-job result</returns>
        string SerializedSubJobResult(object result);
    }
}
