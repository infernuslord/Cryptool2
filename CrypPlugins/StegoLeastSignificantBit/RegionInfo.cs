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
using System.Collections;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;

#endregion

namespace Cryptool.Plugins.StegoLeastSignificantBit
{
	/// <summary>Describes an image region.</summary>
	public class RegionInfo
    {
        private Region region;
        private Point[] points;
        private ArrayList pixels = new ArrayList();
        private Size imageSize;
        private int capacity = 1;
        private byte countUsedBitsPerPixel = 1;

		/// <summary>Return the region.</summary>
		public Region Region
        {
            get { return region; }
        }

		/// <summary>Returns all points in the region.</summary>
		public Point[] Points
        {
            get { return points; }
        }

		/// <summary>Returns or sets the region's capacity.</summary>
		public int Capacity
        {
            get { return capacity; }
            set { capacity = value; }
        }

		/// <summary>Returns or sets the usable bits per pixel for this region.</summary>
		public byte CountUsedBitsPerPixel
        {
            get { return countUsedBitsPerPixel; }
            set { countUsedBitsPerPixel = value; }
        }

		/// <summary>Returns the number of pixels in the region.</summary>
		public long CountPixels
        {
            get { return pixels.Count; }
        }

		/// <summary>Returns the indices of all pixels in the region.</summary>
		public ArrayList PixelIndices
        {
            get { return pixels; }
        }

		/// <summary>Returns the percent of the image covered by this region.</summary>
        public decimal PercentOfImage
        {
            get
            {
                return (100 * (decimal)pixels.Count / (imageSize.Width * imageSize.Height));
            }
        }

		/// <summary>Constructor.</summary>
		public RegionInfo(GraphicsPath path, Point[] points, Size imageSize)
        {
            this.region = new Region(path);
            this.points = points;
            this.imageSize = imageSize;
            UpdateCountPixels();
        }

		/// <summary>Constructor.</summary>
		public RegionInfo(Region region, int capacity, byte bitsPerPixel, Size imageSize)
        {
            this.region = region;
            this.capacity = capacity;
            this.countUsedBitsPerPixel = bitsPerPixel;
            this.imageSize = imageSize;
            UpdateCountPixels();
        }

		/// <summary>Adds points to the region.</summary>
		public void AddPoints(Point[] addPoints)
        {
            Point[] newPoints = new Point[points.Length + addPoints.Length];
            points.CopyTo(newPoints, 0);
            addPoints.CopyTo(newPoints, points.Length);
            points = newPoints;
        }

		/// <summary>Re-builds the list of pixels.</summary>
		public void UpdateCountPixels()
        {
            pixels.Clear();
            for (int y = 0; y < imageSize.Height; y++)
            {
                for (int x = 0; x < imageSize.Width; x++)
                {
                    if (region.IsVisible(x, y))
                    {
                        pixels.Add(GetPixelIndex(x, y));
                    }
                }
            }
			pixels.Sort();
        }

        private int GetPixelIndex(int x, int y)
        {
            return x + (y * imageSize.Width);
        }

    }
}
