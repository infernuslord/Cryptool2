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
using Cryptool.Plugins.PeerToPeer.Jobs;
using Cryptool.PluginBase.Miscellaneous;
using System.Numerics;

namespace Cryptool.PluginBase.Control
{
    /// <summary>
    /// Every PlugIn, which derives from this interface have to implement
    /// the logics for processing a distributed JobPart
    /// </summary>
    public interface IControlWorker
    {
        event ProcessingSuccessfullyEnded OnProcessingSuccessfullyEnded;
        event ProcessingCanceled OnProcessingCanceled;
        event InfoText OnInfoTextReceived;

        string TopicName { get; set; }

        /// <summary>
        /// tries to deserialize byte representation of a job. If this is possible,
        /// JobAccepted event will be thrown, otherwise JobDeclined event will be thrown.
        /// Catch OnProcessingStarted-Event to get result of this operation!
        /// </summary>
        /// <param name="job">byte representation of a job</param>
        bool StartProcessing(byte[] job, out BigInteger jobId);
        void StopProcessing();
    }
}
