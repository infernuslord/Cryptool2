namespace FileOutput.WindowsFormsUserControl
{
  partial class HexBoxContainer
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
      this.statusStrip = new System.Windows.Forms.StatusStrip();
      this.toolStripLblCharPosition = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripStatusLblFileSieze = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripStatusLblFile = new System.Windows.Forms.ToolStripStatusLabel();
      this.hexBox = new Be.Windows.Forms.HexBox();
      this.statusStrip.SuspendLayout();
      this.SuspendLayout();
      // 
      // statusStrip
      // 
      this.statusStrip.GripMargin = new System.Windows.Forms.Padding(0);
      this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLblCharPosition,
            this.toolStripStatusLblFileSieze,
            this.toolStripStatusLblFile});
      this.statusStrip.Location = new System.Drawing.Point(2, 2);
      this.statusStrip.Name = "statusStrip";
      this.statusStrip.Size = new System.Drawing.Size(0, 22);
      this.statusStrip.SizingGrip = false;
      this.statusStrip.TabIndex = 1;
      this.statusStrip.Text = "statusStrip1";
      // 
      // toolStripLblCharPosition
      // 
      this.toolStripLblCharPosition.AutoSize = false;
      this.toolStripLblCharPosition.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.toolStripLblCharPosition.Name = "toolStripLblCharPosition";
      this.toolStripLblCharPosition.Size = new System.Drawing.Size(150, 17);
      // 
      // toolStripStatusLblFileSieze
      // 
      this.toolStripStatusLblFileSieze.AutoSize = false;
      this.toolStripStatusLblFileSieze.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.toolStripStatusLblFileSieze.Name = "toolStripStatusLblFileSieze";
      this.toolStripStatusLblFileSieze.Size = new System.Drawing.Size(150, 17);
      // 
      // toolStripStatusLblFile
      // 
      this.toolStripStatusLblFile.Name = "toolStripStatusLblFile";
      this.toolStripStatusLblFile.Size = new System.Drawing.Size(0, 17);
      // 
      // hexBox
      // 
      this.hexBox.AllowDrop = true;
      this.hexBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.hexBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.hexBox.HexCasing = Be.Windows.Forms.HexCasing.Lower;
      this.hexBox.LineInfoForeColor = System.Drawing.Color.Gray;
      this.hexBox.LineInfoVisible = true;
      this.hexBox.Location = new System.Drawing.Point(2, 2);
      this.hexBox.Name = "hexBox";
      this.hexBox.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
      this.hexBox.Size = new System.Drawing.Size(0, 0);
      this.hexBox.StringViewVisible = true;
      this.hexBox.TabIndex = 0;
      this.hexBox.UseFixedBytesPerLine = true;
      this.hexBox.VScrollBarVisible = true;
      this.hexBox.CurrentPositionInLineChanged += new System.EventHandler(this.Position_Changed);
      this.hexBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.hexBox_DragEnter);
      this.hexBox.CurrentLineChanged += new System.EventHandler(this.Position_Changed);
      this.hexBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.hexBox_DragDrop);
      // 
      // HexBoxContainer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.hexBox);
      this.Controls.Add(this.statusStrip);
      this.Name = "HexBoxContainer";
      this.Padding = new System.Windows.Forms.Padding(2);
      this.Size = new System.Drawing.Size(4, 26);
      this.statusStrip.ResumeLayout(false);
      this.statusStrip.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private Be.Windows.Forms.HexBox hexBox;
    private System.Windows.Forms.StatusStrip statusStrip;
    private System.Windows.Forms.ToolStripStatusLabel toolStripLblCharPosition;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLblFileSieze;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLblFile;
  }
}
