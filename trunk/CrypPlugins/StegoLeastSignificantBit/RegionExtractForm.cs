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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

#endregion

namespace Cryptool.Plugins.StegoLeastSignificantBit
{
    /// <summary>Displays extracted regions and plain text for one carrier image.</summary>
    public partial class RegionExtractForm : Form
    {
        private System.Windows.Forms.PictureBox picImage;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnClose;

		/// <summary>Constructor.</summary>
		public RegionExtractForm(ImageInfo imageInfo)
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();

            DrawRegions(imageInfo);
            txtMessage.Text = imageInfo.TextMessage;
        }

        /// <summary>Draw the image and the carrier regions</summary>
        /// <param name="imageInfo">Image and regions</param>
        private void DrawRegions(ImageInfo imageInfo)
        {
            picImage.Image = (Image)imageInfo.Image;
            picImage.Size = picImage.Image.Size;
            Graphics graphics = Graphics.FromImage(picImage.Image);
            
            foreach (RegionInfo info in imageInfo.RegionInfo)
            {
                graphics.FillRegion(new SolidBrush(Color.Red), info.Region);
            }

            graphics.Dispose();
            picImage.Invalidate();
        }

        private void btnClose_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }
    }
}