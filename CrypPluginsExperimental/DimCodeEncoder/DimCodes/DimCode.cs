/*
   Copyright 2008-2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Cryptool.Plugins.DimCodeEncoder;

namespace DimCodeEncoder.DimCodes
{
    class DimCode
    {
        /// <summary>
        /// extern reachable encode methode
        /// </summary>
        /// <returns> creates based on the given settings an byte array of the image and returns it</returns>
        public byte[] Encode(byte[] input, DimCodeEncoderSettings settings)
        {
            input = EnrichInput(input, settings);
            var error = VerifyInput(input, settings);
            if( error != null)
             throw new Exception(error);
            return EncodeInput(input, settings);
        }

      

        /// <summary>
        /// the origin encode methode each child should override with the dim-code's specific encode methode.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="settings"></param>
        /// <returns>returns the bytearray of the dim-Code</returns>
        protected virtual byte[] EncodeInput(byte[] input, DimCodeEncoderSettings settings)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  each child should override this to avoid errors inside of the EncodeInput 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="settings"></param>
        /// <returns> Internationalization error message or if valid null  </returns>
        protected virtual string VerifyInput (byte[] input, DimCodeEncoderSettings settings)
        {
            throw new NotImplementedException();
        }
   

        /// <summary>
        /// gives the child class the ability to enrich the input before validating.
        // this may be some kind of integrity check value, or, if your code has a fixed size ,some pad
        /// </summary>
        /// <param name="input"></param>
        /// <returns>enriched Input</returns>
        protected virtual byte[] EnrichInput(byte[] input, DimCodeEncoderSettings settings)
        {
            return input;
        }
        #region helper

      
     
        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }

        #endregion helper
    }
}
