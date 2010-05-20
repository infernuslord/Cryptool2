/* Copyright 2010 Team CrypTool (Christian Arnold), Uni Duisburg-Essen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeySearcher;
using Cryptool.PluginBase.Miscellaneous;
using System.Numerics;

namespace Cryptool.Plugins.PeerToPeer.Jobs
{
    public class DistributableKeyPatternJob : IDistributableJob
    {
        public event LastJobAllocated OnLastJobAllocated;
        public event LastResultReceived OnLastResultReceived;

        #region Variables and Properties

        byte[] encryptData;
        public byte[] EncryptData 
        {
            get { return this.encryptData ; }
            set { this.encryptData = value; }
        }

        byte[] initVector;
        public byte[] InitVector 
        { 
            get { return this.initVector; }
            set { this.initVector = value; }
        }

        /// <summary>
        /// increment this counter every time you produce a NEW job. When you pop a 
        /// already created Job from the pattern Buffer, don't increment this value. 
        /// Use the JobId existing in the KeyPatternJobPart-Element.
        /// </summary>
        BigInteger jobIdCounter = 0;
        /// <summary>
        /// main pattern stack
        /// </summary>
        KeyPatternPool patternPool;

        /// <summary>
        /// buffer for patterns which were popped from the patternPool, but were declined by the worker
        /// </summary>
        Stack<KeyPatternJobPart> patternBuffer;
        /// <summary>
        /// when a new Pattern will be requested, it will be copied
        /// in this list. When a JobAccepted message arrived, move this
        /// entry to the processingPatterns Dictionary. When a JobDeclined
        /// Message arrived, remove the pattern from this Dictionary and
        /// add the Job to the patternBuffer.
        /// </summary>
        Dictionary<BigInteger, KeyPatternJobPart> allocatedPatterns;
        /// <summary>
        /// Not before receiving the JobAccepted message from the worker,
        /// the allocatedPattern entry will be moved to this list
        /// </summary>
        Dictionary<BigInteger, KeyPatternJobPart> patternsInProgress;
        Dictionary<BigInteger, KeyPatternJobResult> finishedPatterns;

        private LinkedList<KeySearcher.KeySearcher.ValueKey> globalResultList;
        public LinkedList<KeySearcher.KeySearcher.ValueKey> GlobalResultList 
        {
            get { return this.globalResultList;}
            private set { this.globalResultList = value; }
        }

        public BigInteger TotalAmount
        {
            get { return this.patternPool.Count() + this.patternBuffer.Count + this.patternsInProgress.Count + this.allocatedPatterns.Count + this.finishedPatterns.Count; }
            set { throw new NotImplementedException(); }
        }

        public BigInteger AllocatedAmount
        {
            get { return this.allocatedPatterns.Count + this.patternsInProgress.Count; }
            set { throw new NotImplementedException(); }
        }

        public BigInteger FinishedAmount
        {
            get { return this.finishedPatterns.Count; }
            set { throw new NotImplementedException(); }
        }

        #endregion

        public DistributableKeyPatternJob(KeyPattern pattern, BigInteger partSize, byte[] encryptData, byte[] initVector)
        {
            this.EncryptData = encryptData;
            this.InitVector = initVector;

            this.patternPool = new KeyPatternPool(pattern, partSize);
            this.patternBuffer = new Stack<KeyPatternJobPart>();
            this.allocatedPatterns = new Dictionary<BigInteger, KeyPatternJobPart>();
            this.patternsInProgress = new Dictionary<BigInteger, KeyPatternJobPart>();
            this.finishedPatterns = new Dictionary<BigInteger, KeyPatternJobResult>();

            this.GlobalResultList = new LinkedList<KeySearcher.KeySearcher.ValueKey>();
        }

        #region IDistributableJob Members

        public byte[] Pop(out BigInteger jobId)
        {
            byte[] serializedJob = null;
            jobId = -1;

            if (this.patternBuffer.Count > 0)
            {
                KeyPatternJobPart jobPart = this.patternBuffer.Pop();
                jobId = jobPart.JobId;
                serializedJob = jobPart.Serialize();
                this.allocatedPatterns.Add(jobId, jobPart);
            }
            else
            {
                KeyPattern poppedPattern = this.patternPool.Pop();
                if (poppedPattern != null)
                {
                    // create a new JobPart element
                    jobId = jobIdCounter++;
                    KeyPatternJobPart jobPart = new KeyPatternJobPart(jobId, poppedPattern, this.EncryptData, this.InitVector);
                    serializedJob = jobPart.Serialize();
                    this.allocatedPatterns.Add(jobId, jobPart);

                    if (this.patternPool.Count() == 0)
                        if (OnLastJobAllocated != null)
                            OnLastJobAllocated(jobId);
                }
            }
            return serializedJob;
        }

        public void Push(BigInteger jobId)
        {
            if (this.allocatedPatterns.ContainsKey(jobId))
            {
                this.patternBuffer.Push(this.allocatedPatterns[jobId]);
                //when a job is pushed on the "patternBuffer", this indicates, that worker leaves the network
                this.allocatedPatterns.Remove(jobId);
            }

            if (this.patternsInProgress.ContainsKey(jobId))
            {
                this.patternBuffer.Push(this.patternsInProgress[jobId]);
                //when a job is pushed on the "patternBuffer", this indicates, that worker leaves the network
                this.patternsInProgress.Remove(jobId);
            }
        }

        public double ProcessProgress()
        {
            throw new NotImplementedException();
        }

        public void JobAccepted(BigInteger jobId)
        {
            if (this.allocatedPatterns.ContainsKey(jobId))
            {
                this.patternsInProgress.Add(jobId, this.allocatedPatterns[jobId]);
                this.allocatedPatterns.Remove(jobId);
            }
            //dirty workaround because P2PJobAdmin sends the accepted/declined msg twice...
            //else
            //    throw (new Exception("The job-accepted-message for jobId '"+ jobId.ToString() + "' isn't valid"));
        }

        public void JobDeclined(BigInteger jobId)
        {
            if (this.allocatedPatterns.ContainsKey(jobId))
            {
                this.patternBuffer.Push(this.allocatedPatterns[jobId]);
                this.allocatedPatterns.Remove(jobId);
            }
            //dirty workaround because P2PJobAdmin sends the accepted/declined msg twice...
            //else
            //    throw (new Exception("The job-declined-message for jobId '" + jobId.ToString() + "' isn't valid"));
        }

        public TimeSpan SetResult(BigInteger jobId, byte[] result)
        {
            TimeSpan returnTimeSpan = new TimeSpan(0);
            if (this.patternsInProgress.ContainsKey(jobId))
            {
                KeyPatternJobResult deserializedJobResult = new KeyPatternJobResult(result);
                
                MergeGlobalList(deserializedJobResult.Result);

                returnTimeSpan = deserializedJobResult.ProcessingTime;
                this.finishedPatterns.Add(jobId, deserializedJobResult);
                this.patternsInProgress.Remove(jobId);
            }
            //dirty workaround because P2PJobAdmin sends the result msg twice...
            //else
            //    throw(new Exception("Received result from a job, which isn't in 'patternsInProgress' List."));
            if (this.patternPool.Count() == 0 && this.patternsInProgress.Count == 0 && this.allocatedPatterns.Count == 0 && this.patternBuffer.Count == 0)
            {
                if (OnLastResultReceived != null)
                    OnLastResultReceived(jobId);
            }

            return returnTimeSpan;
        }

        #endregion

        #region Special Global Result List

        private LinkedList<KeySearcher.KeySearcher.ValueKey> MergeGlobalList(LinkedList<KeySearcher.KeySearcher.ValueKey> listToCompare)
        {
            if (this.GlobalResultList.Count == 0)
            {
                this.GlobalResultList = listToCompare;
                return this.GlobalResultList;
            }

            if (this.GlobalResultList.Last().value >= listToCompare.First().value)
                return this.GlobalResultList;

            LinkedListNode<KeySearcher.KeySearcher.ValueKey> globalNode = this.GlobalResultList.First;
            LinkedListNode<KeySearcher.KeySearcher.ValueKey> temp_node = this.GlobalResultList.First;
            LinkedListNode<KeySearcher.KeySearcher.ValueKey> localNode = listToCompare.First;

            if (this.GlobalResultList.Count != listToCompare.Count)
                throw (new Exception("The two lists, which you want to merge haven't the same length (GlobalList: " + this.GlobalResultList.Count + ", LocalList: " + listToCompare.Count));

            while(localNode != null)
            {
                temp_node = globalNode;
                while (globalNode != null)
                {
                    if (localNode.Value.value >= globalNode.Value.value)
                    {
                        this.GlobalResultList.AddBefore(globalNode, localNode.Value);
                        this.GlobalResultList.RemoveLast();
                        break;
                    }
                    globalNode = globalNode.Next;
                }
                globalNode = temp_node;
                localNode = localNode.Next;
            }

            return this.GlobalResultList;
        }

        #endregion
    }
}
