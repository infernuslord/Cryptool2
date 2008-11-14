namespace FileInput
{
  partial class About
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
      this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.logoPictureBox = new System.Windows.Forms.PictureBox();
      this.labelProductName = new System.Windows.Forms.Label();
      this.labelVersion = new System.Windows.Forms.Label();
      this.okButton = new System.Windows.Forms.Button();
      this.rtbAbout = new System.Windows.Forms.RichTextBox();
      this.lablelLink = new System.Windows.Forms.LinkLabel();
      this.tableLayoutPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // tableLayoutPanel
      // 
      this.tableLayoutPanel.ColumnCount = 2;
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 94F));
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel.Controls.Add(this.logoPictureBox, 0, 0);
      this.tableLayoutPanel.Controls.Add(this.labelProductName, 1, 0);
      this.tableLayoutPanel.Controls.Add(this.labelVersion, 1, 1);
      this.tableLayoutPanel.Controls.Add(this.okButton, 1, 4);
      this.tableLayoutPanel.Controls.Add(this.rtbAbout, 1, 3);
      this.tableLayoutPanel.Controls.Add(this.lablelLink, 1, 2);
      this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel.Location = new System.Drawing.Point(9, 9);
      this.tableLayoutPanel.Name = "tableLayoutPanel";
      this.tableLayoutPanel.RowCount = 5;
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel.Size = new System.Drawing.Size(664, 350);
      this.tableLayoutPanel.TabIndex = 0;
      // 
      // logoPictureBox
      // 
      this.logoPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.logoPictureBox.Image = global::FileInput.Properties.Resources.logo;
      this.logoPictureBox.Location = new System.Drawing.Point(3, 3);
      this.logoPictureBox.Name = "logoPictureBox";
      this.tableLayoutPanel.SetRowSpan(this.logoPictureBox, 5);
      this.logoPictureBox.Size = new System.Drawing.Size(88, 344);
      this.logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.logoPictureBox.TabIndex = 12;
      this.logoPictureBox.TabStop = false;
      // 
      // labelProductName
      // 
      this.labelProductName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelProductName.Location = new System.Drawing.Point(94, 0);
      this.labelProductName.Margin = new System.Windows.Forms.Padding(0);
      this.labelProductName.MaximumSize = new System.Drawing.Size(0, 17);
      this.labelProductName.Name = "labelProductName";
      this.labelProductName.Size = new System.Drawing.Size(570, 17);
      this.labelProductName.TabIndex = 19;
      this.labelProductName.Text = "Be.HexEditor  1.3.0";
      this.labelProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelVersion
      // 
      this.labelVersion.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelVersion.Location = new System.Drawing.Point(94, 25);
      this.labelVersion.Margin = new System.Windows.Forms.Padding(0);
      this.labelVersion.MaximumSize = new System.Drawing.Size(0, 17);
      this.labelVersion.Name = "labelVersion";
      this.labelVersion.Size = new System.Drawing.Size(570, 17);
      this.labelVersion.TabIndex = 0;
      this.labelVersion.Text = "Shared Source Licence by Bernhard Elbl ";
      this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.okButton.Location = new System.Drawing.Point(586, 323);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 24);
      this.okButton.TabIndex = 24;
      this.okButton.Text = "&OK";
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // rtbAbout
      // 
      this.rtbAbout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.rtbAbout.Location = new System.Drawing.Point(97, 78);
      this.rtbAbout.Name = "rtbAbout";
      this.rtbAbout.ReadOnly = true;
      this.rtbAbout.Size = new System.Drawing.Size(564, 239);
      this.rtbAbout.TabIndex = 25;
      this.rtbAbout.Text = "";
      // 
      // lablelLink
      // 
      this.lablelLink.AutoSize = true;
      this.lablelLink.Location = new System.Drawing.Point(94, 50);
      this.lablelLink.Margin = new System.Windows.Forms.Padding(0);
      this.lablelLink.Name = "lablelLink";
      this.lablelLink.Size = new System.Drawing.Size(193, 13);
      this.lablelLink.TabIndex = 26;
      this.lablelLink.TabStop = true;
      this.lablelLink.Text = "http://sourceforge.net/projects/hexbox";
      // 
      // About
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.ClientSize = new System.Drawing.Size(682, 368);
      this.Controls.Add(this.tableLayoutPanel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(200, 150);
      this.Name = "About";
      this.Padding = new System.Windows.Forms.Padding(9);
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "About";
      this.tableLayoutPanel.ResumeLayout(false);
      this.tableLayoutPanel.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.PictureBox logoPictureBox;
    private System.Windows.Forms.Label labelProductName;
    private System.Windows.Forms.Label labelVersion;
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.RichTextBox rtbAbout;
    private System.Windows.Forms.LinkLabel lablelLink;
  }
}
