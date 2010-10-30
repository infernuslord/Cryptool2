﻿/*                              
   Copyright 2010 Sven Rech (svenrech at googlemail dot com), Uni Duisburg-Essen

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

namespace Cryptool.PluginBase.Control
{
    public interface KeyTranslator
    {
        /// <summary>
        /// The parameter "keys" determines the keys this KeyTranslator should iterate over.
        /// The parameter is almost always a KeyPattern.
        /// </summary>
        /// <param name="keys">The keys</param>
        void SetKeys(object keys);

        /// <summary>
        /// Returns the current key as an array.
        /// </summary>
        /// <returns>current key</returns>
        byte[] GetKey();

        /// <summary>
        ///  This is used for iterating over the keys.
        /// </summary>
        /// <returns>if there are keys left</returns>
        bool NextKey();

        /// <summary>
        /// Returns the string representation of the current key.
        /// </summary>
        /// <returns>string representation of the current key</returns>
        string GetKeyRepresentation();

        /// <summary>
        /// Returns the amount of keys that where given by the "nextKey()" method after "getProgress()" was called last time.
        /// This method should have the side-effect, that the progress is synchronized with the underlying keypattern.
        /// This is important, because the synchronization is necessary for splitting the keypattern.
        /// </summary>
        /// <returns>The progress</returns>
        int GetProgress();

        //TODO: Add OpenCL stuff here
    }
}