namespace Cryptool.Plugins.StegoLeastSignificantBit
{
    partial class RegionHideForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnNext = new System.Windows.Forms.Button();
            this.picImage = new System.Windows.Forms.PictureBox();
            this.mnuDeleteRegion = new System.Windows.Forms.MenuItem();
            this.picMap = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.contextmenuImage = new System.Windows.Forms.ContextMenu();
            this.label2 = new System.Windows.Forms.Label();
            this.labelMessageLength = new System.Windows.Forms.Label();
            this.labelHeaderLength = new System.Windows.Forms.Label();
            this.lblMessageSize = new System.Windows.Forms.Label();
            this.lblSelectedPixels = new System.Windows.Forms.Label();
            this.labelHeaderSpace = new System.Windows.Forms.Label();
            this.lblHeaderSpace = new System.Windows.Forms.Label();
            this.lblHeaderSize = new System.Windows.Forms.Label();
            this.labelSummary = new System.Windows.Forms.Label();
            this.lblPercent = new System.Windows.Forms.Label();
            this.lblHidden = new System.Windows.Forms.Label();
            this.errors = new System.Windows.Forms.ErrorProvider(this.components);
            this.btnCancel = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.lblCapacity = new System.Windows.Forms.Label();
            this.ctlRegions = new Cryptool.Plugins.StegoLeastSignificantBit.RegionInfoList();
            this.btnMaxRegion = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMap)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errors)).BeginInit();
            this.SuspendLayout();
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(472, 543);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 23);
            this.btnNext.TabIndex = 0;
            this.btnNext.Text = "OK";
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // picImage
            // 
            this.picImage.Location = new System.Drawing.Point(0, 0);
            this.picImage.Name = "picImage";
            this.picImage.Size = new System.Drawing.Size(216, 192);
            this.picImage.TabIndex = 2;
            this.picImage.TabStop = false;
            this.picImage.Click += new System.EventHandler(this.picImage_Click);
            this.picImage.DoubleClick += new System.EventHandler(this.picImage_DoubleClick);
            // 
            // mnuDeleteRegion
            // 
            this.mnuDeleteRegion.Index = 0;
            this.mnuDeleteRegion.Text = "Delete";
            this.mnuDeleteRegion.Click += new System.EventHandler(this.mnuDeleteRegion_Click);
            // 
            // picMap
            // 
            this.picMap.Location = new System.Drawing.Point(0, 0);
            this.picMap.Name = "picMap";
            this.picMap.Size = new System.Drawing.Size(216, 192);
            this.picMap.TabIndex = 3;
            this.picMap.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.picImage);
            this.panel1.Location = new System.Drawing.Point(13, 45);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(280, 240);
            this.panel1.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.AutoScroll = true;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel2.Controls.Add(this.picMap);
            this.panel2.Location = new System.Drawing.Point(347, 46);
            this.panel2.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(272, 240);
            this.panel2.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Location = new System.Drawing.Point(346, 12);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(274, 31);
            this.label3.TabIndex = 10;
            this.label3.Text = "Only regions, just for your information:";
            // 
            // contextmenuImage
            // 
            this.contextmenuImage.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuDeleteRegion});
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(14, 12);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(280, 31);
            this.label2.TabIndex = 9;
            this.label2.Text = "Bitmap and regions - Click to draw a region, double-click to finish it:";
            // 
            // labelMessageLength
            // 
            this.labelMessageLength.AutoSize = true;
            this.labelMessageLength.BackColor = System.Drawing.Color.Transparent;
            this.labelMessageLength.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMessageLength.Location = new System.Drawing.Point(14, 500);
            this.labelMessageLength.Name = "labelMessageLength";
            this.labelMessageLength.Size = new System.Drawing.Size(100, 13);
            this.labelMessageLength.TabIndex = 13;
            this.labelMessageLength.Text = "Message length:";
            // 
            // labelHeaderLength
            // 
            this.labelHeaderLength.AutoSize = true;
            this.labelHeaderLength.BackColor = System.Drawing.Color.Transparent;
            this.labelHeaderLength.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHeaderLength.Location = new System.Drawing.Point(14, 520);
            this.labelHeaderLength.Name = "labelHeaderLength";
            this.labelHeaderLength.Size = new System.Drawing.Size(91, 13);
            this.labelHeaderLength.TabIndex = 15;
            this.labelHeaderLength.Text = "Header length:";
            // 
            // lblMessageSize
            // 
            this.lblMessageSize.AutoSize = true;
            this.lblMessageSize.BackColor = System.Drawing.Color.Transparent;
            this.lblMessageSize.Location = new System.Drawing.Point(136, 500);
            this.lblMessageSize.Name = "lblMessageSize";
            this.lblMessageSize.Size = new System.Drawing.Size(80, 13);
            this.lblMessageSize.TabIndex = 17;
            this.lblMessageSize.Text = "lblMessageSize";
            // 
            // lblSelectedPixels
            // 
            this.lblSelectedPixels.BackColor = System.Drawing.Color.Transparent;
            this.lblSelectedPixels.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSelectedPixels.Location = new System.Drawing.Point(105, 462);
            this.lblSelectedPixels.Name = "lblSelectedPixels";
            this.lblSelectedPixels.Size = new System.Drawing.Size(70, 14);
            this.lblSelectedPixels.TabIndex = 18;
            this.lblSelectedPixels.Text = "lblSelectedPixels";
            this.lblSelectedPixels.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelHeaderSpace
            // 
            this.labelHeaderSpace.AutoSize = true;
            this.labelHeaderSpace.BackColor = System.Drawing.Color.Transparent;
            this.labelHeaderSpace.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHeaderSpace.Location = new System.Drawing.Point(14, 540);
            this.labelHeaderSpace.Name = "labelHeaderSpace";
            this.labelHeaderSpace.Size = new System.Drawing.Size(109, 13);
            this.labelHeaderSpace.TabIndex = 19;
            this.labelHeaderSpace.Text = "Space for header:";
            // 
            // lblHeaderSpace
            // 
            this.lblHeaderSpace.AutoSize = true;
            this.lblHeaderSpace.BackColor = System.Drawing.Color.Transparent;
            this.lblHeaderSpace.Location = new System.Drawing.Point(136, 540);
            this.lblHeaderSpace.Name = "lblHeaderSpace";
            this.lblHeaderSpace.Size = new System.Drawing.Size(83, 13);
            this.lblHeaderSpace.TabIndex = 20;
            this.lblHeaderSpace.Text = "lblHeaderSpace";
            // 
            // lblHeaderSize
            // 
            this.lblHeaderSize.AutoSize = true;
            this.lblHeaderSize.BackColor = System.Drawing.Color.Transparent;
            this.lblHeaderSize.Location = new System.Drawing.Point(136, 520);
            this.lblHeaderSize.Name = "lblHeaderSize";
            this.lblHeaderSize.Size = new System.Drawing.Size(72, 13);
            this.lblHeaderSize.TabIndex = 21;
            this.lblHeaderSize.Text = "lblHeaderSize";
            // 
            // labelSummary
            // 
            this.labelSummary.AutoSize = true;
            this.labelSummary.BackColor = System.Drawing.Color.Transparent;
            this.labelSummary.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSummary.Location = new System.Drawing.Point(45, 462);
            this.labelSummary.Name = "labelSummary";
            this.labelSummary.Size = new System.Drawing.Size(61, 13);
            this.labelSummary.TabIndex = 22;
            this.labelSummary.Text = "Summary:";
            // 
            // lblPercent
            // 
            this.lblPercent.BackColor = System.Drawing.Color.Transparent;
            this.lblPercent.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPercent.Location = new System.Drawing.Point(175, 462);
            this.lblPercent.Name = "lblPercent";
            this.lblPercent.Size = new System.Drawing.Size(100, 14);
            this.lblPercent.TabIndex = 23;
            this.lblPercent.Text = "lblPercent";
            this.lblPercent.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblHidden
            // 
            this.lblHidden.BackColor = System.Drawing.Color.Transparent;
            this.lblHidden.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHidden.Location = new System.Drawing.Point(275, 462);
            this.lblHidden.Name = "lblHidden";
            this.lblHidden.Size = new System.Drawing.Size(90, 14);
            this.lblHidden.TabIndex = 24;
            this.lblHidden.Text = "lblHidden";
            this.lblHidden.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // errors
            // 
            this.errors.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errors.ContainerControl = this;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(553, 543);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 25;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Location = new System.Drawing.Point(12, 291);
            this.label4.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(616, 26);
            this.label4.TabIndex = 26;
            this.label4.Text = "Please configure the regions and make sure the sum of the hidden bytes matches th" +
                "e message length:";
            // 
            // lblCapacity
            // 
            this.lblCapacity.BackColor = System.Drawing.Color.Transparent;
            this.lblCapacity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCapacity.Location = new System.Drawing.Point(395, 462);
            this.lblCapacity.Name = "lblCapacity";
            this.lblCapacity.Size = new System.Drawing.Size(80, 14);
            this.lblCapacity.TabIndex = 27;
            this.lblCapacity.Text = "lblCapacity";
            this.lblCapacity.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ctlRegions
            // 
            this.ctlRegions.AutoScroll = true;
            this.ctlRegions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ctlRegions.Location = new System.Drawing.Point(45, 320);
            this.ctlRegions.Name = "ctlRegions";
            this.ctlRegions.Size = new System.Drawing.Size(546, 140);
            this.ctlRegions.TabIndex = 2;
            this.ctlRegions.Delete += new Cryptool.Plugins.StegoLeastSignificantBit.RegionInfoList.RegionEventHandler(this.ctlRegions_Delete);
            this.ctlRegions.SelectionChanged += new Cryptool.Plugins.StegoLeastSignificantBit.RegionInfoList.RegionEventHandler(this.ctlRegions_SelectionChanged);
            // 
            // btnMaxRegion
            // 
            this.btnMaxRegion.Location = new System.Drawing.Point(472, 510);
            this.btnMaxRegion.Name = "btnMaxRegion";
            this.btnMaxRegion.Size = new System.Drawing.Size(156, 23);
            this.btnMaxRegion.TabIndex = 28;
            this.btnMaxRegion.Text = "Maximum Region";
            this.btnMaxRegion.UseVisualStyleBackColor = true;
            this.btnMaxRegion.Click += new System.EventHandler(this.btnMaxRegion_Click);
            // 
            // RegionHideForm
            // 
            this.AcceptButton = this.btnNext;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(636, 574);
            this.Controls.Add(this.btnMaxRegion);
            this.Controls.Add(this.lblCapacity);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblHidden);
            this.Controls.Add(this.lblPercent);
            this.Controls.Add(this.labelSummary);
            this.Controls.Add(this.lblHeaderSize);
            this.Controls.Add(this.lblHeaderSpace);
            this.Controls.Add(this.labelHeaderSpace);
            this.Controls.Add(this.lblSelectedPixels);
            this.Controls.Add(this.lblMessageSize);
            this.Controls.Add(this.labelHeaderLength);
            this.Controls.Add(this.labelMessageLength);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.ctlRegions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "RegionHideForm";
            this.Text = "Define the Regions of the Image";
            ((System.ComponentModel.ISupportInitialize)(this.picImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMap)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errors)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelMessageLength;
        private System.Windows.Forms.Label labelHeaderLength;
        private System.Windows.Forms.Label lblMessageSize;
        private System.Windows.Forms.Label lblSelectedPixels;
        private System.Windows.Forms.Label labelHeaderSpace;
        private System.Windows.Forms.Label lblHeaderSpace;
        private System.Windows.Forms.Label lblHeaderSize;
        private System.Windows.Forms.Label labelSummary;
        private System.Windows.Forms.Label lblPercent;
        private System.Windows.Forms.Label lblHidden;
        private System.Windows.Forms.ErrorProvider errors;
        private RegionInfoList ctlRegions;
		private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblCapacity;
        private System.Windows.Forms.Button btnMaxRegion;
    }
}