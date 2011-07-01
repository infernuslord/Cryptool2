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
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblMessageSize = new System.Windows.Forms.Label();
            this.lblSelectedPixels = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblHeaderSpace = new System.Windows.Forms.Label();
            this.lblHeaderSize = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblPercent = new System.Windows.Forms.Label();
            this.lblCapacity = new System.Windows.Forms.Label();
            this.errors = new System.Windows.Forms.ErrorProvider(this.components);
            this.ctlRegions = new Cryptool.Plugins.StegoLeastSignificantBit.RegionInfoList();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMap)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errors)).BeginInit();
            this.SuspendLayout();
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(454, 513);
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
            this.picImage.DoubleClick += new System.EventHandler(this.picImage_DoubleClick);
            this.picImage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picImage_MouseDown);
            this.picImage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picImage_MouseUp);
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
            this.picMap.Size = new System.Drawing.Size(216, 200);
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
            this.label3.Location = new System.Drawing.Point(345, 13);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(252, 23);
            this.label3.TabIndex = 10;
            this.label3.Text = "Only regions, just four your information:";
            // 
            // contextmenuImage
            // 
            this.contextmenuImage.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuDeleteRegion});
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(13, 13);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(240, 31);
            this.label2.TabIndex = 9;
            this.label2.Text = "Bitmap and regions - Click to draw a region, double-click to finish it:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(22, 472);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Message length:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Location = new System.Drawing.Point(22, 492);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Header length:";
            // 
            // lblMessageSize
            // 
            this.lblMessageSize.AutoSize = true;
            this.lblMessageSize.BackColor = System.Drawing.Color.Transparent;
            this.lblMessageSize.Location = new System.Drawing.Point(122, 471);
            this.lblMessageSize.Name = "lblMessageSize";
            this.lblMessageSize.Size = new System.Drawing.Size(80, 13);
            this.lblMessageSize.TabIndex = 17;
            this.lblMessageSize.Text = "lblMessageSize";
            // 
            // lblSelectedPixels
            // 
            this.lblSelectedPixels.BackColor = System.Drawing.Color.Transparent;
            this.lblSelectedPixels.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSelectedPixels.Location = new System.Drawing.Point(108, 424);
            this.lblSelectedPixels.Name = "lblSelectedPixels";
            this.lblSelectedPixels.Size = new System.Drawing.Size(94, 14);
            this.lblSelectedPixels.TabIndex = 18;
            this.lblSelectedPixels.Text = "lblSelectedPixels";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Location = new System.Drawing.Point(22, 513);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(92, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "Space for header:";
            // 
            // lblHeaderSpace
            // 
            this.lblHeaderSpace.AutoSize = true;
            this.lblHeaderSpace.BackColor = System.Drawing.Color.Transparent;
            this.lblHeaderSpace.Location = new System.Drawing.Point(122, 513);
            this.lblHeaderSpace.Name = "lblHeaderSpace";
            this.lblHeaderSpace.Size = new System.Drawing.Size(83, 13);
            this.lblHeaderSpace.TabIndex = 20;
            this.lblHeaderSpace.Text = "lblHeaderSpace";
            // 
            // lblHeaderSize
            // 
            this.lblHeaderSize.AutoSize = true;
            this.lblHeaderSize.BackColor = System.Drawing.Color.Transparent;
            this.lblHeaderSize.Location = new System.Drawing.Point(122, 492);
            this.lblHeaderSize.Name = "lblHeaderSize";
            this.lblHeaderSize.Size = new System.Drawing.Size(72, 13);
            this.lblHeaderSize.TabIndex = 21;
            this.lblHeaderSize.Text = "lblHeaderSize";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(45, 424);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(61, 13);
            this.label7.TabIndex = 22;
            this.label7.Text = "Summary:";
            // 
            // lblPercent
            // 
            this.lblPercent.BackColor = System.Drawing.Color.Transparent;
            this.lblPercent.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPercent.Location = new System.Drawing.Point(212, 425);
            this.lblPercent.Name = "lblPercent";
            this.lblPercent.Size = new System.Drawing.Size(57, 14);
            this.lblPercent.TabIndex = 23;
            this.lblPercent.Text = "lblPercent";
            // 
            // lblCapacity
            // 
            this.lblCapacity.BackColor = System.Drawing.Color.Transparent;
            this.lblCapacity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCapacity.Location = new System.Drawing.Point(327, 424);
            this.lblCapacity.Name = "lblCapacity";
            this.lblCapacity.Size = new System.Drawing.Size(72, 14);
            this.lblCapacity.TabIndex = 24;
            this.lblCapacity.Text = "lblCapacity";
            // 
            // errors
            // 
            this.errors.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errors.ContainerControl = this;
            // 
            // ctlRegions
            // 
            this.ctlRegions.AutoScroll = true;
            this.ctlRegions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ctlRegions.Location = new System.Drawing.Point(45, 313);
            this.ctlRegions.Name = "ctlRegions";
            this.ctlRegions.Size = new System.Drawing.Size(546, 104);
            this.ctlRegions.TabIndex = 2;
            this.ctlRegions.Delete += new Cryptool.Plugins.StegoLeastSignificantBit.RegionInfoList.RegionEventHandler(this.ctlRegions_Delete);
            this.ctlRegions.SelectionChanged += new Cryptool.Plugins.StegoLeastSignificantBit.RegionInfoList.RegionEventHandler(this.ctlRegions_SelectionChanged);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(535, 513);
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
            this.label4.Size = new System.Drawing.Size(530, 23);
            this.label4.TabIndex = 26;
            this.label4.Text = "Please configure the regions and make sure the sum of hidden bytes matches the me" +
                "ssage\'s length:";
            // 
            // RegionHideForm
            // 
            this.AcceptButton = this.btnNext;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(636, 539);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblCapacity);
            this.Controls.Add(this.lblPercent);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.lblHeaderSize);
            this.Controls.Add(this.lblHeaderSpace);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lblSelectedPixels);
            this.Controls.Add(this.lblMessageSize);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
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

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblMessageSize;
        private System.Windows.Forms.Label lblSelectedPixels;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblHeaderSpace;
        private System.Windows.Forms.Label lblHeaderSize;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblPercent;
        private System.Windows.Forms.Label lblCapacity;
        private System.Windows.Forms.ErrorProvider errors;
        private RegionInfoList ctlRegions;
		private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label4;
    }
}