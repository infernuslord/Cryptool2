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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using System.Collections.ObjectModel;

#endregion

namespace Cryptool.Plugins.StegoLeastSignificantBit
{
    partial class RegionHideForm : Form
    {
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ContextMenu contextmenuImage;
        private System.Windows.Forms.PictureBox picMap;
        private System.Windows.Forms.MenuItem mnuDeleteRegion;
        private System.Windows.Forms.PictureBox picImage;
        private System.Windows.Forms.Button btnNext;

        /// <summary>true: a region is being drawn at the moment</summary>
        private bool isDrawing = false;

        /// <summary>Contains the new image when drawing a new region</summary>
        private Image bufferImage;
        /// <summary>Contains the old image when drawing a new region</summary>
        private Image cleanImage;
        /// <summary>The unchanged source image</summary>
        private Image baseImage;

        /// <summary>Points in the currently drawn path</summary>
        private ArrayList drawingPoints;

        /// <summary>List of finished regions</summary>
        private Collection<RegionInfo> drawnRegions = new Collection<RegionInfo>();

        /// <summary>Selected region. Regions can be selected in [picImage] with the right mouse button, or in [ctlRegions].</summary>
        private RegionInfo selectedRegionInfo;

        /// <summary>Length of the message that will be hidden in [baseImage]</summary>
        private int messageLength;

		internal int MessageLength
		{
			get
			{
				return messageLength;
			}
			set
			{
				messageLength = value;
				lblMessageSize.Text = messageLength.ToString() + (messageLength==1?" Byte":" Bytes");
				UpdateSummary();
			}
		}

		public RegionHideForm(Bitmap bitmap, int messageLength)
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();

            this.Text = Properties.Resources.RegionHideTitle;
            this.label2.Text = Properties.Resources.RegionLabel2;
            this.label3.Text = Properties.Resources.RegionLabel3;
            this.label4.Text = Properties.Resources.RegionLabel4;
            this.labelHeaderLength.Text = Properties.Resources.RegionHeaderSize;
            this.labelHeaderSpace.Text = Properties.Resources.RegionHeaderSpace;
            this.labelMessageLength.Text = Properties.Resources.RegionMessageSize;
            this.labelSummary.Text = Properties.Resources.RegionSummary;
            this.btnCancel.Text = Properties.Resources.RegionCancel;
            this.btnMaxRegion.Text = Properties.Resources.RegionMaxRegion;
            this.mnuDeleteRegion.Text = Properties.Resources.RegionDelete;

			OpenImage(bitmap);
            this.MessageLength = messageLength;
        }

        private RegionInfo FindRegion(int x, int y)
        {
            foreach (RegionInfo info in drawnRegions)
                if (info.Region.IsVisible(x,y)) 
                    return info;

            return null;
        }

        private void SelectRegion(RegionInfo region)
        {
            selectedRegionInfo = region;
            ctlRegions.SelectItem(region);
            ReDrawImages(false);
        }

        private void picImage_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = e as MouseEventArgs;

            RegionInfo info = FindRegion(me.X, me.Y);   // did we click inside a region?

            if (me.Button == MouseButtons.Left)
            {
                if (info != null)
                {
                    if (!isDrawing) SelectRegion(info); // select the clicked region
                    return;
                }

                if (!isDrawing)
                {   //start a new region
                    isDrawing = true;
                    drawingPoints = new ArrayList();
                    cleanImage = picImage.Image;
                    bufferImage = new Bitmap(cleanImage.Width, cleanImage.Height);
                }

                AddPoint(me.X, me.Y);
            }
            else if (me.Button == MouseButtons.Right)
            {
                if (info != null)
                {   //the point is inside a region
                    SelectRegion(info);
                    isDrawing = false;
                    contextmenuImage.Show(picImage, new Point(me.X, me.Y));
                }
            }
        }

        private void picImage_DoubleClick(object sender, EventArgs e)
        {
            MouseEventArgs me = e as MouseEventArgs;

            if (!isDrawing) return;
            if (FindRegion(me.X, me.Y) != null) return;

			this.Cursor = Cursors.WaitCursor;

            try
            {
                isDrawing = false;

                if (drawingPoints.Count > 2)
                    AddRegion((Point[])drawingPoints.ToArray(typeof(Point)));

                ReDrawImages(true);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        
        private bool AddRegion(Point[] points)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(points);

            if (!UniteWithIntersectedRegions(path, points))
            {
                RegionInfo info = new RegionInfo(path, points, picImage.Image.Size);
                if (info.CountPixels > 0)
                {
                    int nothidden = Math.Max(messageLength - ctlRegions.HiddenBytes,0);
                    info.Capacity = Math.Min(nothidden, info.MaximumCapacity);
                    drawnRegions.Add(info);
                    ctlRegions.Add(new RegionInfoListItem(info));
                    return true;
                }
            }

            return false;
        }

        private void AddMaximumRegion()
        {
            // how many lines do we need to hide the region info
            int countLines = (int)Math.Ceiling((double)ImageInfo.EffectiveSize * 8 / baseImage.Width);
            Point[] points = new Point[4] { new Point(0, countLines), new Point(baseImage.Width, countLines), new Point(baseImage.Width, baseImage.Height), new Point(0, baseImage.Height) };
            AddRegion(points);
        }

        private void DeleteAllRegions()
        {
            isDrawing = false;
            ctlRegions.Clear();
            drawnRegions.Clear();
            ReDrawImages(true);
        }

        private void mnuDeleteRegion_Click(object sender, EventArgs e)
        {
            isDrawing = false;
            drawnRegions.Remove(selectedRegionInfo);
            ctlRegions.DeleteItem(selectedRegionInfo);
            ReDrawImages(true);
        }

        private void UpdateSummary()
        {
            bool isOkay = true;

            long countPixels = 0;       // count of selected pixels
            int capacity = 0;           // capacity of all regions
            int hiddenbytes = 0;        // number of hidden bytes in all regions
            long mapStreamLength = 65;  // Int32 beginning of first region + Int32 regions length + Byte bits per pixel
            int firstPixelInRegions = baseImage.Width * baseImage.Height; // first pixel inside a region
            RegionInfo firstRegion = null; // topmost region

            foreach (RegionInfo info in drawnRegions)
            {
                countPixels += info.CountPixels;
                capacity += info.MaximumCapacity;
                hiddenbytes += info.Capacity;

                mapStreamLength += 64; //Int32 RegionData Length + Int32 Capacity
                mapStreamLength += info.Points.Length * 8; //length of the points stream

                if (info.CountPixels > 0 && (int)info.PixelIndices[0] < firstPixelInRegions)
                {
                    firstPixelInRegions = (int)info.PixelIndices[0];
                    firstRegion = info;
                }
            }

            //selected pixels
            lblSelectedPixels.Text = countPixels.ToString() + " " + (countPixels == 1 ? Properties.Resources.RegionPixel : Properties.Resources.RegionPixel);

            //percent of image
            lblPercent.Text = String.Format("{0:F2} %", (100.0 * countPixels) / (baseImage.Width * baseImage.Height));

            //hidden bytes
            lblHidden.Text = hiddenbytes.ToString() + (hiddenbytes == 1 ? " Byte" : " Bytes");

            //capacity
            lblCapacity.Text = capacity.ToString() + (capacity == 1 ? " Byte" : " Bytes");

			if(capacity < messageLength)
			{
			    SetControlColor(lblCapacity, true);
                errors.SetError(lblCapacity, Properties.Resources.RegionErrorCapacity);
                isOkay = false;
            }
            else
            {
                SetControlColor(lblCapacity, false);
                errors.SetError(lblCapacity, String.Empty);
            }
            
            if (hiddenbytes < messageLength)
            {
                SetControlColor(lblHidden, true);
                errors.SetError(lblHidden, Properties.Resources.RegionErrorHidden);
                isOkay = false;
            }
            else
            {
                SetControlColor(lblHidden, false);
                errors.SetError(lblHidden, String.Empty);
            }

            //header size
            lblHeaderSize.Text = mapStreamLength.ToString() + (mapStreamLength == 1 ? " Bit" : " Bits");

            //first pixel inside a region
            if (firstRegion != null)
            {
                String firstPixel = firstPixelInRegions.ToString() + (firstPixelInRegions == 1 ? " Bit" : " Bits");

                if (firstPixelInRegions > mapStreamLength) {
                    lblHeaderSpace.Text = firstPixel;
                    SetControlColor(lblHeaderSpace, false);
                } else {
                    isOkay = false;
                    lblHeaderSpace.Text = firstPixel + " - " + Properties.Resources.RegionRemoveTopmost;
                    SetControlColor(lblHeaderSpace, true);
                    selectedRegionInfo = firstRegion;
                    ctlRegions.SelectItem(firstRegion);
                    ReDrawImages(false);
                }
            } else {
                int picsize = baseImage.Width * baseImage.Height;
                lblHeaderSpace.Text = picsize.ToString() + (picsize == 1 ? " Bit" : " Bits") + " - " + Properties.Resources.RegionOneRegion;
                SetControlColor(lblHeaderSpace, true);
            }

            btnNext.Enabled = isOkay;
        }

        private void SetControlColor(Control control, bool isError) {
            if (isError) {
                control.BackColor = Color.DarkRed;
                control.ForeColor = Color.White;
            } else {
                control.BackColor = SystemColors.Control;
                control.ForeColor = SystemColors.ControlText;
            }

        }

        /// <summary>Add the path to an existing region, if there are any intersections</summary>
        /// <param name="path">The path that is added to an intersected region</param>
        /// <param name="points">Point of the path</param>
        /// <returns>true: intersection found, regions united; false: no intersection found</returns>
        private bool UniteWithIntersectedRegions(GraphicsPath path, Point[] points)
		{
            bool returnValue = false; //no intersection found yet

            Graphics graphics = Graphics.FromImage(baseImage);
            Region tempRegion;

            RegionInfo info;
            for (int n = 0; n < drawnRegions.Count; n++)
            {
                info = (RegionInfo)drawnRegions[n];
                tempRegion = new Region(info.Region.GetRegionData());
                tempRegion.Intersect(path);
                if (!tempRegion.IsEmpty(graphics))
                {	
                    return true;
					// Intersect/IsEmpty does not work in mono 2.6
					// Workaround: Check bounds of intersection
					RectangleF bounds = tempRegion.GetBounds(graphics);
					if(bounds.Height > 0 && bounds.Width > 0)
					{
						// Union does something strange on mono 2.6, so this might not work
	                    info.Region.Union(path);
    	                info.AddPoints(points);
        	            info.UpdateCountPixels();
            	        ctlRegions.UpdateContent(info);
                	    returnValue = true;
					}
                }
            }

            return returnValue;
        }

        /// <summary>Display the source image with the regions on it in [picImage], and only the regions in [picMap]</summary>
        /// <param name="updateSummary">true: call UpdateSummary() when finished</param>
        private void ReDrawImages(bool updateSummary)
        {
            //create empty images
            Image bufferImageNoBackground = new Bitmap(baseImage.Width, baseImage.Height);
            Image bufferImageWithBackground = new Bitmap(baseImage.Width, baseImage.Height);

            //get graphics
            Graphics graphicsWithBackground = Graphics.FromImage(bufferImageWithBackground);
            Graphics graphicsNoBackground = Graphics.FromImage(bufferImageNoBackground);
            
            //draw backgrounds
            graphicsNoBackground.Clear(Color.White);
            graphicsWithBackground.DrawImage(baseImage, 0, 0, baseImage.Width, baseImage.Height);

            //draw regions
            foreach (RegionInfo info in drawnRegions)
            {
                PathGradientBrush brush = new PathGradientBrush(info.Points, WrapMode.Clamp);
                brush.CenterColor = Color.Transparent;
                brush.SurroundColors = new Color[1] { info == selectedRegionInfo ?  Color.Green : Color.Red };

                graphicsWithBackground.FillRegion(brush, info.Region);
                graphicsWithBackground.DrawPolygon(new Pen(Color.Black, 4), info.Points);

                graphicsNoBackground.FillRegion(brush, info.Region);
                graphicsNoBackground.DrawPolygon(new Pen(Color.Black, 4), info.Points);
            }

            //clean up
            graphicsWithBackground.Dispose();
            graphicsNoBackground.Dispose();

            //show images
            picImage.Image = bufferImageWithBackground;
            picMap.Image = bufferImageNoBackground;
            picImage.Invalidate();
            picMap.Invalidate();

            //update numbers and errors
            if (updateSummary) { UpdateSummary(); }
        }

        /// <summary>Add a point to the currently dran path</summary>
        /// <param name="x">X-coordinate of the point</param>
        /// <param name="y">Y-coordinate of the point</param>
        private void AddPoint(int x, int y)
        {
            drawingPoints.Add(new Point(x, y));

            Graphics graphics = Graphics.FromImage(bufferImage);

            graphics.Clear(Color.White);
            graphics.DrawImage(cleanImage, 0, 0);

            Pen pen = new Pen(Color.Red);
            pen.Width = 4;

            if (drawingPoints.Count > 1)
            {
                //draw the path
                graphics.DrawPolygon(pen, (Point[])drawingPoints.ToArray(typeof(Point)));
            }
            else
            {
                //the path contains only one point yet - draw a circle
                graphics.DrawEllipse(pen, x - 1, y - 1, 2, 2);
            }
            graphics.Dispose();

            picImage.Image = bufferImage;
            picImage.Invalidate();
        }

        /// <summary>Open the source image, clear [picMap]</summary>
        private void OpenImage(Bitmap bitmap)
        {
			baseImage = bitmap;

            picImage.Size = baseImage.Size;
            picMap.Size = baseImage.Size;

            picMap.Image = new Bitmap(baseImage.Width, baseImage.Height);
            Graphics graphics = Graphics.FromImage(this.picMap.Image);
            graphics.Clear(Color.White);
            graphics.Dispose();

            ReDrawImages(true);
        }

        private void ctlRegions_Delete(object sender, RegionInfoList.RegionInfoListEventArgs e)
        {
            drawnRegions.Remove(e.Item.RegionInfo);
            ReDrawImages(true);
        }


        /// <summary>a region has been selected or modified</summary>
        /// <param name="sender">the list control</param>
        /// <param name="e">the list item</param>
        private void ctlRegions_SelectionChanged(object sender, RegionInfoList.RegionInfoListEventArgs e)
        {
            selectedRegionInfo = e.Item.RegionInfo;
            ReDrawImages(true);
        }

        private void btnNext_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();

            int remaining = messageLength;
            Collection<RegionInfo> usedRegions = new Collection<RegionInfo>();
            foreach (RegionInfo info in drawnRegions)
            {
                if (remaining == 0) break;
                if (remaining < info.Capacity)
                    info.Capacity = remaining;
                remaining -= info.Capacity;
                usedRegions.Add(info);
            }

            drawnRegions = usedRegions;
        }

        /// <summary>Get the source image and all specified regions</summary>
        /// <value>Image and regions with all information needed to hide a message</value>
        public ImageInfo ImageInfo
        {
            get
            {
                return new ImageInfo(baseImage, drawnRegions);
            }
        }

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
            this.Close();
		}

        private void btnMaxRegion_Click(object sender, EventArgs e)
        {
            DeleteAllRegions();
            AddMaximumRegion();
            SelectRegion(drawnRegions[0]);
            ReDrawImages(true);
        }
    }
}