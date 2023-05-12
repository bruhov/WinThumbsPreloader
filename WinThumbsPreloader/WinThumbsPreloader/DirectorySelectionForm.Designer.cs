namespace WinThumbsPreloader
{
    partial class DirectorySelectionForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.AppNameLabel = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.selectDir = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.AppNameLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.richTextBox1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.listBox1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.selectDir, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20.14925F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 79.85075F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 211F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 450);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // AppNameLabel
            // 
            this.AppNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.AppNameLabel.AutoSize = true;
            this.AppNameLabel.Font = new System.Drawing.Font("Hack Nerd Font Mono", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.AppNameLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.AppNameLabel.Location = new System.Drawing.Point(296, 0);
            this.AppNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.AppNameLabel.Name = "AppNameLabel";
            this.AppNameLabel.Size = new System.Drawing.Size(208, 22);
            this.AppNameLabel.TabIndex = 7;
            this.AppNameLabel.Text = "WinThumbsPreloader";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Cursor = System.Windows.Forms.Cursors.No;
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Font = new System.Drawing.Font("Hack Nerd Font Mono", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.richTextBox1.Location = new System.Drawing.Point(3, 43);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(794, 156);
            this.richTextBox1.TabIndex = 9;
            this.richTextBox1.Text = "Please hit the button below to select the folder you would like to scan for and c" +
    "reate thumbnails.";
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Location = new System.Drawing.Point(3, 205);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(794, 205);
            this.listBox1.TabIndex = 10;
            // 
            // selectDir
            // 
            this.selectDir.AutoSize = true;
            this.selectDir.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.selectDir.Dock = System.Windows.Forms.DockStyle.Top;
            this.selectDir.Font = new System.Drawing.Font("Hack Nerd Font Mono", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.selectDir.Location = new System.Drawing.Point(3, 416);
            this.selectDir.Name = "selectDir";
            this.selectDir.Size = new System.Drawing.Size(794, 25);
            this.selectDir.TabIndex = 8;
            this.selectDir.Text = "Select Directory";
            this.selectDir.UseVisualStyleBackColor = true;
            this.selectDir.Click += new System.EventHandler(this.selectDir_Click);
            // 
            // DirectorySelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DirectorySelectionForm";
            this.Text = "DirectorySelectionForm";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Label AppNameLabel;
        private Button selectDir;
        private RichTextBox richTextBox1;
        private OpenFileDialog openFileDialog1;
        private ListBox listBox1;
        private FolderBrowserDialog folderBrowserDialog1;
    }
}