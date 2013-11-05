namespace Cryptool.Plugins.StegoLeastSignificantBit
{
    partial class RegionInfoListItem
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnDelete = new System.Windows.Forms.Button();
            this.lblPercentPixels = new System.Windows.Forms.Label();
            this.lblRegionName = new System.Windows.Forms.Label();
            this.lblCountPixels = new System.Windows.Forms.Label();
            this.numCapacity = new System.Windows.Forms.NumericUpDown();
            this.numCountUsedBitsPerPixel = new System.Windows.Forms.NumericUpDown();
            this.lblRegionCapacity = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numCapacity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCountUsedBitsPerPixel)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnDelete.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnDelete.Location = new System.Drawing.Point(450, 0);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(70, 21);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // lblPercentPixels
            // 
            this.lblPercentPixels.Location = new System.Drawing.Point(130, 0);
            this.lblPercentPixels.Name = "lblPercentPixels";
            this.lblPercentPixels.Size = new System.Drawing.Size(100, 23);
            this.lblPercentPixels.TabIndex = 2;
            this.lblPercentPixels.Text = "0";
            this.lblPercentPixels.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPercentPixels.Click += new System.EventHandler(this.Label_Click);
            // 
            // lblRegionName
            // 
            this.lblRegionName.Location = new System.Drawing.Point(0, 0);
            this.lblRegionName.Name = "lblRegionName";
            this.lblRegionName.Size = new System.Drawing.Size(60, 23);
            this.lblRegionName.TabIndex = 0;
            this.lblRegionName.Text = "Region 1";
            this.lblRegionName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblRegionName.Click += new System.EventHandler(this.Label_Click);
            // 
            // lblCountPixels
            // 
            this.lblCountPixels.Location = new System.Drawing.Point(60, 0);
            this.lblCountPixels.Name = "lblCountPixels";
            this.lblCountPixels.Size = new System.Drawing.Size(70, 23);
            this.lblCountPixels.TabIndex = 1;
            this.lblCountPixels.Text = "0";
            this.lblCountPixels.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblCountPixels.Click += new System.EventHandler(this.Label_Click);
            // 
            // numCapacity
            // 
            this.numCapacity.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.numCapacity.Location = new System.Drawing.Point(235, 2);
            this.numCapacity.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numCapacity.Name = "numCapacity";
            this.numCapacity.Size = new System.Drawing.Size(80, 20);
            this.numCapacity.TabIndex = 3;
            this.numCapacity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numCapacity.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numCapacity.ValueChanged += new System.EventHandler(this.NumCapacityValueChanged);
            this.numCapacity.Click += new System.EventHandler(this.Label_Click);
            this.numCapacity.Enter += new System.EventHandler(this.NumCapacityEnter);
            this.numCapacity.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.numCapacity_KeyPress);
            // 
            // numCountUsedBitsPerPixel
            // 
            this.numCountUsedBitsPerPixel.Location = new System.Drawing.Point(456, 3);
            this.numCountUsedBitsPerPixel.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numCountUsedBitsPerPixel.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numCountUsedBitsPerPixel.Name = "numCountUsedBitsPerPixel";
            this.numCountUsedBitsPerPixel.Size = new System.Drawing.Size(10, 20);
            this.numCountUsedBitsPerPixel.TabIndex = 5;
            this.numCountUsedBitsPerPixel.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numCountUsedBitsPerPixel.Visible = false;
            this.numCountUsedBitsPerPixel.ValueChanged += new System.EventHandler(this.NumCountUsedBitsPerPixelValueChanged);
            this.numCountUsedBitsPerPixel.Enter += new System.EventHandler(this.NumCapacityEnter);
            // 
            // lblRegionCapacity
            // 
            this.lblRegionCapacity.Location = new System.Drawing.Point(350, 0);
            this.lblRegionCapacity.Name = "lblRegionCapacity";
            this.lblRegionCapacity.Size = new System.Drawing.Size(80, 23);
            this.lblRegionCapacity.TabIndex = 6;
            this.lblRegionCapacity.Text = "0";
            this.lblRegionCapacity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblRegionCapacity.Click += new System.EventHandler(this.Label_Click);
            // 
            // RegionInfoListItem
            // 
            this.Controls.Add(this.lblRegionCapacity);
            this.Controls.Add(this.numCountUsedBitsPerPixel);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.numCapacity);
            this.Controls.Add(this.lblPercentPixels);
            this.Controls.Add(this.lblCountPixels);
            this.Controls.Add(this.lblRegionName);
            this.Name = "RegionInfoListItem";
            this.Size = new System.Drawing.Size(536, 24);
            ((System.ComponentModel.ISupportInitialize)(this.numCapacity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCountUsedBitsPerPixel)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

    }
}
