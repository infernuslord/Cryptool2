/* 
   Copyright 2011 Corinna John

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

#region Using directives

using System;
using System.Drawing;
using System.Collections.ObjectModel;

#endregion

namespace Cryptool.Plugins.StegoLeastSignificantBit
{
    /// <summary>Describes a carrier image that is being used by a SteganoDotNet.Action.BitmapFileUtility></summary>
    public class ImageInfo
    {
        public const int EffectiveSize = 41;

        private string destinationFileName = string.Empty;
        private string textMessage = string.Empty;
        private Collection<RegionInfo> regionInfo;
        private Image image;
        
        /// <summary>Returns the image.</summary>
        public Image Image
        {
            get { return image; }
        }

        /// <summary>Returns the selected regions or sets the extracted regions.</summary>
        public Collection<RegionInfo> RegionInfo
        {
            get { return regionInfo; }
            set { regionInfo = value; }
        }

        /// <summary>Returns or sets the destination file name.</summary>
        public string DestinationFileName
        {
            get { return destinationFileName; }
            set { destinationFileName = value; }
        }

        /// <summary>Returns or sets the plaintext that has been extracted from this image.</summary>
        public string TextMessage
        {
            get { return textMessage; }
            set { textMessage = value; }
        }

        /// <summary>Constructor.</summary>
        /// <param name="image">The image.</param>
        /// <param name="regionInfo">The selected or extracted regions.</param>
        public ImageInfo(Image image, Collection<RegionInfo> regionInfo)
        {
            this.image = image;
            this.regionInfo = regionInfo;
        }
    }
}
