namespace Cryptool.Plugins.StegoLeastSignificantBit
{
    partial class RegionExtractForm
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
			this.btnClose = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.picImage = new System.Windows.Forms.PictureBox();
			this.txtMessage = new System.Windows.Forms.TextBox();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.picImage)).BeginInit();
			this.SuspendLayout();
			// 
			// btnClose
			// 
			this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClose.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.btnClose.Location = new System.Drawing.Point(365, 436);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(75, 23);
			this.btnClose.TabIndex = 3;
			this.btnClose.Text = "Close";
			this.btnClose.UseVisualStyleBackColor = false;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.AutoScroll = true;
			this.panel1.Controls.Add(this.picImage);
			this.panel1.Location = new System.Drawing.Point(8, 16);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(432, 230);
			this.panel1.TabIndex = 1;
			// 
			// picImage
			// 
			this.picImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.picImage.Location = new System.Drawing.Point(0, 0);
			this.picImage.Name = "picImage";
			this.picImage.Size = new System.Drawing.Size(200, 222);
			this.picImage.TabIndex = 0;
			this.picImage.TabStop = false;
			// 
			// txtMessage
			// 
			this.txtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtMessage.Location = new System.Drawing.Point(8, 262);
			this.txtMessage.Multiline = true;
			this.txtMessage.Name = "txtMessage";
			this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtMessage.Size = new System.Drawing.Size(432, 168);
			this.txtMessage.TabIndex = 2;
			// 
			// RegionExtractForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(448, 467);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.txtMessage);
			this.Controls.Add(this.panel1);
			this.Name = "RegionExtractForm";
			this.Text = "Content of a cover medium";
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.picImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

    }
}