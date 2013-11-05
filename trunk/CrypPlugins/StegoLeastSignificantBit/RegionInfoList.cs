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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

#endregion

namespace Cryptool.Plugins.StegoLeastSignificantBit
{
	/// <summary>Represents an image regions list.</summary>
	public partial class RegionInfoList : UserControl
    {
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;

		/// <summary>Arguments for RegionEventHandler.</summary>
        public class RegionInfoListEventArgs : EventArgs
        {
            private RegionInfoListItem item;

			/// <summary>Returns the affected item.</summary>
			public RegionInfoListItem Item
            {
                get { return item; }
            }

			/// <summary>Constructor.</summary>
			public RegionInfoListEventArgs(RegionInfoListItem item)
            {
                this.item = item;
            }
        }

		/// <summary>Delegate for tegion list events.</summary>
		public delegate void RegionEventHandler(object sender, RegionInfoListEventArgs e);

		/// <summary>An item has been removed.</summary>
		public event RegionEventHandler Delete;

		/// <summary>An item has been selected.</summary>
		public event RegionEventHandler SelectionChanged;

        private Collection<RegionInfoListItem> items = new Collection<RegionInfoListItem>();

		/// <summary>Constructor.</summary>
        public RegionInfoList()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();

            this.label1.Text = Properties.Resources.RegionListLabel1;
            this.label2.Text = Properties.Resources.RegionListLabel2;
            this.label3.Text = Properties.Resources.RegionListLabel3;
            this.label4.Text = Properties.Resources.RegionListLabel4;
            this.label5.Text = Properties.Resources.RegionListLabel5;
        }

        /// <summary>Returns the combined maximum capacity of all regions.</summary>
        public int MaximumCapacity
        {
            get
            {
                int maximumCapacity = 0;

                foreach (RegionInfoListItem item in items)
                    maximumCapacity += item.RegionInfo.MaximumCapacity;

                return maximumCapacity;
            }
        }        
        
        /// <summary>Returns the combined number of hidden bytes in all regions.</summary>
        public int HiddenBytes
        {
            get
            {
                int hiddenBytes = 0;

                foreach (RegionInfoListItem item in items)
                    hiddenBytes += item.RegionInfo.Capacity;

                return hiddenBytes;
            }
        }

		/// <summary>Adds an item to the list.</summary>
		public void Add(RegionInfoListItem item)
        {
            items.Add(item);
            item.Location = new Point(0, items.Count * item.Height);
            this.Controls.Add(item);
            item.Delete += new EventHandler(item_Delete);
            item.Selected += new EventHandler(item_Selected);
            item.Index = items.Count - 1;
            item.UpdateContent();
        }

		/// <summary>Removes an item  from the list.</summary>
		public void DeleteItem(RegionInfo info)
        {
            foreach (RegionInfoListItem item in items)
            {
                if (item.RegionInfo == info)
                {
                    DeleteRegion(item);
                    break;
                }
            }

            UpdateContent();
        }

		/// <summary>Selects an item.</summary>
		public void SelectItem(RegionInfo info)
        {
            foreach (RegionInfoListItem item in items)
            {
                if (item.RegionInfo == info)
                    item.SelectItem();
                else
                    item.DeselectItem();
            }
        }

        /// <summary>Updates all items.</summary>
        public void UpdateContent()
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Index = i;
                items[i].Location = new Point(0, (i + 1) * items[i].Height);
                items[i].UpdateContent();
            }
        }

		/// <summary>Updates one item.</summary>
		public void UpdateContent(RegionInfo info)
        {
            foreach (RegionInfoListItem item in items)
            {
                if (item.RegionInfo == info)
                {
                    item.UpdateContent();
                    break;
                }
            }
        }

		/// <summary>Removes all items.</summary>
		public void Clear()
        {
            foreach (RegionInfoListItem item in items)
            {
                this.Controls.Remove(item);
            }
            items.Clear();
        }

        private void item_Delete(object sender, EventArgs e)
        {
            RegionInfoListItem senderItem = (RegionInfoListItem)sender;
            DeleteRegion(senderItem);
            UpdateContent();
            OnDelete(senderItem);
        }

        private void item_Selected(object sender, EventArgs e)
        {
            RegionInfoListItem senderItem = (RegionInfoListItem)sender;
            OnSelectionChanged(senderItem);
        }

        private void DeleteRegion(RegionInfoListItem item)
        {
            items.Remove(item);
            this.Controls.Remove(item);
        }

        private void OnDelete(RegionInfoListItem senderItem)
        {
            if (Delete != null)
            {
                Delete(this, new RegionInfoListEventArgs(senderItem));
            }
        }

        private void OnSelectionChanged(RegionInfoListItem senderItem)
        {

            foreach (RegionInfoListItem item in items)
            {
                item.DeselectItem();
            }

            if (SelectionChanged != null)
            {
                SelectionChanged(this, new RegionInfoListEventArgs(senderItem));
            }
        }

    }
}
