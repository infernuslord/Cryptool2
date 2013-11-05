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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

#endregion

namespace Cryptool.Plugins.StegoLeastSignificantBit
{
	/// <summary>Represents an item in the image regions list.</summary>
	public partial class RegionInfoListItem : UserControl
    {
        private System.Windows.Forms.NumericUpDown numCountUsedBitsPerPixel;
        private System.Windows.Forms.NumericUpDown numCapacity;
        private System.Windows.Forms.Label lblCountPixels;
        private System.Windows.Forms.Label lblRegionName;
        private System.Windows.Forms.Label lblPercentPixels;
        private System.Windows.Forms.Label lblRegionCapacity;
        private System.Windows.Forms.Button btnDelete;

        private RegionInfo regionInfo;

		/// <summary>Returns the region.</summary>
		public RegionInfo RegionInfo
        {
            get { return regionInfo; }
        }

        private int index = 0;

		/// <summary>Returns the index.</summary>
		public int Index
        {
            get { return index; }
            set { index = value; }
        }

        private bool isUpdating; //the control is updating its fields - don't send Select events

		/// <summary>The item is removed from the list.</summary>
		public event EventHandler Delete;

		/// <summary>The item is selected.</summary>
		public event EventHandler Selected;

		/// <summary>Default constructor, only for VS Forms Designer.</summary>
		public RegionInfoListItem() { }

		/// <summary>Constructor.</summary>
		public RegionInfoListItem(RegionInfo regionInfo)
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            this.btnDelete.Text = Properties.Resources.RegionDelete;

            this.regionInfo = regionInfo;

            UpdateContent();
        }

		/// <summary>Updates the list item.</summary>
		public void UpdateContent()
        {
            isUpdating = true;

            lblRegionName.Text = "Region " + (index + 1).ToString();
            lblCountPixels.Text = regionInfo.CountPixels.ToString();
            lblPercentPixels.Text = String.Format("{0:F2} %", regionInfo.PercentOfImage);
            numCountUsedBitsPerPixel.Value = regionInfo.CountUsedBitsPerPixel;
            UpdateCapacity();
            lblRegionCapacity.Text = numCapacity.Maximum.ToString();

            isUpdating = false;
        }

        private void UpdateCapacity()
        {
            numCapacity.Maximum = regionInfo.MaximumCapacity;

            if (numCapacity.Maximum == 0) {
                //the region is too small to hide any data
                numCapacity.Minimum = 0;
                numCapacity.Maximum = 0;
                numCapacity.Value = 0;
                numCapacity.Enabled = false;
            } else {
                numCapacity.Enabled = true;
                numCapacity.Minimum = 0;
                if (regionInfo.Capacity <= numCapacity.Maximum) {
                    numCapacity.Value = regionInfo.Capacity;
                } else {
                    numCapacity.Value = numCapacity.Maximum;
                    regionInfo.Capacity = (int)numCapacity.Maximum;
                }
            }
        }

		/// <summary>Deselects the list item.</summary>
		public void DeselectItem()
        {
            this.BackColor = SystemColors.Control;
        }

		/// <summary>Selects the list item.</summary>
		public void SelectItem()
        {
            this.BackColor = Color.White;
        }

        private void OnDelete()
        {
            if (Delete != null)
            {
                Delete(this, new EventArgs());
            }
        }

        private void OnSelected()
        {
            if (!isUpdating) {
                if (Selected != null) {
                    Selected(this, new EventArgs());
                }
                SelectItem();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            OnDelete();
        }

        private void Label_Click(object sender, System.EventArgs e)
        {
            OnSelected();
        }

        private void NumCapacityEnter(object sender, System.EventArgs e)
        {
            OnSelected();
        }

        private void NumCapacityValueChanged(object sender, System.EventArgs e)
        {
            regionInfo.Capacity = (int)numCapacity.Value;
            OnSelected();
        }

        private void NumCountUsedBitsPerPixelValueChanged(object sender, System.EventArgs e)
        {
            regionInfo.CountUsedBitsPerPixel = (byte)numCountUsedBitsPerPixel.Value;
            UpdateCapacity();
            OnSelected();
        }

        private void numCapacity_KeyPress(object sender, KeyPressEventArgs e)
        {
            regionInfo.Capacity = (int)numCapacity.Value;
            OnSelected();
        }

    }
}
