namespace SqlScriptRunner
{
    partial class SqlScriptRunnerMain
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.connectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveGeneratedScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToDBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.usersGuideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel3 = new System.Windows.Forms.Panel();
            this.treeViewFileStructure = new System.Windows.Forms.TreeView();
            this.label2 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.buttonUp = new System.Windows.Forms.Button();
            this.buttonBrowsFolder = new System.Windows.Forms.Button();
            this.textBoxRootFolder = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.buttonInsertToDatabase = new System.Windows.Forms.Button();
            this.buttonMakeScript = new System.Windows.Forms.Button();
            this.richTextBoxGeneratedContent = new System.Windows.Forms.RichTextBox();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectionToolStripMenuItem,
            this.helpToolStripMenuItem,
            this.helpToolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(901, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // connectionToolStripMenuItem
            // 
            this.connectionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveGeneratedScriptToolStripMenuItem});
            this.connectionToolStripMenuItem.Name = "connectionToolStripMenuItem";
            this.connectionToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.connectionToolStripMenuItem.Text = "File";
            // 
            // saveGeneratedScriptToolStripMenuItem
            // 
            this.saveGeneratedScriptToolStripMenuItem.Name = "saveGeneratedScriptToolStripMenuItem";
            this.saveGeneratedScriptToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveGeneratedScriptToolStripMenuItem.Size = new System.Drawing.Size(241, 22);
            this.saveGeneratedScriptToolStripMenuItem.Text = "&Save Generated Content";
            this.saveGeneratedScriptToolStripMenuItem.Click += new System.EventHandler(this.saveGeneratedScriptToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToDBToolStripMenuItem,
            this.disconnectToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(81, 20);
            this.helpToolStripMenuItem.Text = "Connection";
            // 
            // connectToDBToolStripMenuItem
            // 
            this.connectToDBToolStripMenuItem.Name = "connectToDBToolStripMenuItem";
            this.connectToDBToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.connectToDBToolStripMenuItem.Text = "&Connect To DB";
            // 
            // disconnectToolStripMenuItem
            // 
            this.disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
            this.disconnectToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.disconnectToolStripMenuItem.Text = "&Disconnect";
            // 
            // helpToolStripMenuItem1
            // 
            this.helpToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem,
            this.usersGuideToolStripMenuItem});
            this.helpToolStripMenuItem1.Name = "helpToolStripMenuItem1";
            this.helpToolStripMenuItem1.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem1.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // usersGuideToolStripMenuItem
            // 
            this.usersGuideToolStripMenuItem.Name = "usersGuideToolStripMenuItem";
            this.usersGuideToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.usersGuideToolStripMenuItem.Text = "Users Guide";
            // 
            // statusStrip1
            // 
            this.statusStrip1.AutoSize = false;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 494);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(901, 25);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.splitContainer1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(901, 470);
            this.panel1.TabIndex = 2;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel3);
            this.splitContainer1.Panel1.Controls.Add(this.panel2);
            this.splitContainer1.Panel1.Controls.Add(this.panel4);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.richTextBoxGeneratedContent);
            this.splitContainer1.Size = new System.Drawing.Size(901, 470);
            this.splitContainer1.SplitterDistance = 236;
            this.splitContainer1.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.treeViewFileStructure);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 35);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(236, 402);
            this.panel3.TabIndex = 3;
            // 
            // treeViewFileStructure
            // 
            this.treeViewFileStructure.CheckBoxes = true;
            this.treeViewFileStructure.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewFileStructure.Location = new System.Drawing.Point(0, 13);
            this.treeViewFileStructure.Name = "treeViewFileStructure";
            this.treeViewFileStructure.Size = new System.Drawing.Size(236, 389);
            this.treeViewFileStructure.TabIndex = 1;
            this.treeViewFileStructure.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewFileStructure_NodeMouseClick);
            this.treeViewFileStructure.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewFileStructure_NodeMouseDoubleClick);
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(236, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Files To Involve:";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.buttonUp);
            this.panel2.Controls.Add(this.buttonBrowsFolder);
            this.panel2.Controls.Add(this.textBoxRootFolder);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(236, 35);
            this.panel2.TabIndex = 0;
            // 
            // buttonUp
            // 
            this.buttonUp.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonUp.Enabled = false;
            this.buttonUp.Location = new System.Drawing.Point(186, 13);
            this.buttonUp.Name = "buttonUp";
            this.buttonUp.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.buttonUp.Size = new System.Drawing.Size(25, 22);
            this.buttonUp.TabIndex = 3;
            this.buttonUp.Text = "^";
            this.buttonUp.UseVisualStyleBackColor = true;
            this.buttonUp.Click += new System.EventHandler(this.buttonUp_Click);
            // 
            // buttonBrowsFolder
            // 
            this.buttonBrowsFolder.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonBrowsFolder.Location = new System.Drawing.Point(211, 13);
            this.buttonBrowsFolder.Name = "buttonBrowsFolder";
            this.buttonBrowsFolder.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.buttonBrowsFolder.Size = new System.Drawing.Size(25, 22);
            this.buttonBrowsFolder.TabIndex = 1;
            this.buttonBrowsFolder.Text = "...";
            this.buttonBrowsFolder.UseVisualStyleBackColor = true;
            this.buttonBrowsFolder.Click += new System.EventHandler(this.buttonBrowsFolder_Click);
            // 
            // textBoxRootFolder
            // 
            this.textBoxRootFolder.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBoxRootFolder.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.textBoxRootFolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxRootFolder.Location = new System.Drawing.Point(0, 13);
            this.textBoxRootFolder.Name = "textBoxRootFolder";
            this.textBoxRootFolder.Size = new System.Drawing.Size(236, 20);
            this.textBoxRootFolder.TabIndex = 0;
            this.textBoxRootFolder.TextChanged += new System.EventHandler(this.textBoxRootFolder_TextChanged);
            this.textBoxRootFolder.Leave += new System.EventHandler(this.textBoxRootFolder_Leave);
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(236, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Root Folder:";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.buttonInsertToDatabase);
            this.panel4.Controls.Add(this.buttonMakeScript);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(0, 437);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(236, 33);
            this.panel4.TabIndex = 4;
            // 
            // buttonInsertToDatabase
            // 
            this.buttonInsertToDatabase.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonInsertToDatabase.Enabled = false;
            this.buttonInsertToDatabase.Location = new System.Drawing.Point(77, 0);
            this.buttonInsertToDatabase.Name = "buttonInsertToDatabase";
            this.buttonInsertToDatabase.Size = new System.Drawing.Size(77, 33);
            this.buttonInsertToDatabase.TabIndex = 1;
            this.buttonInsertToDatabase.Text = "Apply To DB";
            this.buttonInsertToDatabase.UseVisualStyleBackColor = true;
            // 
            // buttonMakeScript
            // 
            this.buttonMakeScript.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonMakeScript.Location = new System.Drawing.Point(0, 0);
            this.buttonMakeScript.Name = "buttonMakeScript";
            this.buttonMakeScript.Size = new System.Drawing.Size(77, 33);
            this.buttonMakeScript.TabIndex = 0;
            this.buttonMakeScript.Text = "Make Script";
            this.buttonMakeScript.UseVisualStyleBackColor = true;
            this.buttonMakeScript.Click += new System.EventHandler(this.buttonMakeScript_Click);
            // 
            // richTextBoxGeneratedContent
            // 
            this.richTextBoxGeneratedContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxGeneratedContent.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxGeneratedContent.Name = "richTextBoxGeneratedContent";
            this.richTextBoxGeneratedContent.Size = new System.Drawing.Size(661, 470);
            this.richTextBoxGeneratedContent.TabIndex = 0;
            this.richTextBoxGeneratedContent.Text = "";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(46, 20);
            this.toolStripStatusLabel.Text = "Loaded";
            // 
            // SqlScriptRunnerMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(901, 519);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "SqlScriptRunnerMain";
            this.Text = "Sql Script Runner";
            this.Load += new System.EventHandler(this.SqlScriptRunnerMain_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem connectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.RichTextBox richTextBoxGeneratedContent;
        private System.Windows.Forms.ToolStripMenuItem saveGeneratedScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectToDBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disconnectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem usersGuideToolStripMenuItem;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button buttonBrowsFolder;
        private System.Windows.Forms.TextBox textBoxRootFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TreeView treeViewFileStructure;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button buttonMakeScript;
        private System.Windows.Forms.Button buttonInsertToDatabase;
        private System.Windows.Forms.Button buttonUp;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}

