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
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.PeerToPeer.Jobs
{
    /// <summary>
    /// please implement additionally a static Deserialize method and a constructor
    /// with a parameter byte[], where you can Deserialize a byte[] representation
    /// of this class. The static Deserialize Method could call than the byte[] constructor!!!
    /// </summary>
    /// <typeparam name="JobType"></typeparam>
    public interface IJobPart
    {
        BigInteger JobId { get; set; }
        byte[] Serialize();
        //IJobPart<JobType> Deserialize(byte[] serializedJobPart);
    }
}
