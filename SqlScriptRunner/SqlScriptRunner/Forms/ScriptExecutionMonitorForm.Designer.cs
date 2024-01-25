namespace SqlScriptRunner.Forms
{
    partial class ScriptExecutionMonitorForm
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
            this.panelHeader = new System.Windows.Forms.Panel();
            this.panelOptions = new System.Windows.Forms.Panel();
            this.checkBoxAllowTransaction = new System.Windows.Forms.CheckBox();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.labelStatusText = new System.Windows.Forms.Label();
            this.labelHeader = new System.Windows.Forms.Label();
            this.panelFooter = new System.Windows.Forms.Panel();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.listViewExecution = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelHeader.SuspendLayout();
            this.panelOptions.SuspendLayout();
            this.panelStatus.SuspendLayout();
            this.panelFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.Controls.Add(this.panelOptions);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(288, 50);
            this.panelHeader.TabIndex = 1;
            // 
            // panelOptions
            // 
            this.panelOptions.Controls.Add(this.checkBoxAllowTransaction);
            this.panelOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelOptions.Location = new System.Drawing.Point(0, 0);
            this.panelOptions.Name = "panelOptions";
            this.panelOptions.Size = new System.Drawing.Size(288, 50);
            this.panelOptions.TabIndex = 2;
            // 
            // checkBoxAllowTransaction
            // 
            this.checkBoxAllowTransaction.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxAllowTransaction.Checked = true;
            this.checkBoxAllowTransaction.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAllowTransaction.Dock = System.Windows.Forms.DockStyle.Top;
            this.checkBoxAllowTransaction.Location = new System.Drawing.Point(0, 0);
            this.checkBoxAllowTransaction.Name = "checkBoxAllowTransaction";
            this.checkBoxAllowTransaction.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.checkBoxAllowTransaction.Size = new System.Drawing.Size(288, 16);
            this.checkBoxAllowTransaction.TabIndex = 0;
            this.checkBoxAllowTransaction.Text = "Allow Transaction";
            this.checkBoxAllowTransaction.UseVisualStyleBackColor = true;
            this.checkBoxAllowTransaction.CheckedChanged += new System.EventHandler(this.checkBoxAllowTransaction_CheckedChanged);
            // 
            // panelStatus
            // 
            this.panelStatus.Controls.Add(this.labelStatusText);
            this.panelStatus.Controls.Add(this.labelHeader);
            this.panelStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelStatus.Location = new System.Drawing.Point(0, 0);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Size = new System.Drawing.Size(166, 31);
            this.panelStatus.TabIndex = 3;
            // 
            // labelStatusText
            // 
            this.labelStatusText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelStatusText.Location = new System.Drawing.Point(94, 0);
            this.labelStatusText.Name = "labelStatusText";
            this.labelStatusText.Size = new System.Drawing.Size(72, 31);
            this.labelStatusText.TabIndex = 1;
            this.labelStatusText.Text = "Not Started";
            this.labelStatusText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelHeader
            // 
            this.labelHeader.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelHeader.Location = new System.Drawing.Point(0, 0);
            this.labelHeader.Name = "labelHeader";
            this.labelHeader.Size = new System.Drawing.Size(94, 31);
            this.labelHeader.TabIndex = 0;
            this.labelHeader.Text = "Execution Status:";
            this.labelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelFooter
            // 
            this.panelFooter.Controls.Add(this.panelStatus);
            this.panelFooter.Controls.Add(this.buttonStart);
            this.panelFooter.Controls.Add(this.buttonCancel);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFooter.Location = new System.Drawing.Point(0, 511);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Size = new System.Drawing.Size(288, 31);
            this.panelFooter.TabIndex = 1;
            // 
            // buttonStart
            // 
            this.buttonStart.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonStart.Location = new System.Drawing.Point(166, 0);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(61, 31);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonCancel.Location = new System.Drawing.Point(227, 0);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(61, 31);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Close";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // listViewExecution
            // 
            this.listViewExecution.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listViewExecution.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewExecution.FullRowSelect = true;
            this.listViewExecution.GridLines = true;
            this.listViewExecution.HideSelection = false;
            this.listViewExecution.Location = new System.Drawing.Point(0, 50);
            this.listViewExecution.MultiSelect = false;
            this.listViewExecution.Name = "listViewExecution";
            this.listViewExecution.ShowItemToolTips = true;
            this.listViewExecution.Size = new System.Drawing.Size(288, 461);
            this.listViewExecution.TabIndex = 2;
            this.listViewExecution.UseCompatibleStateImageBehavior = false;
            this.listViewExecution.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "#";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Object";
            // 
            // ScriptExecutionMonitorForm
            // 
            this.AcceptButton = this.buttonStart;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(288, 542);
            this.Controls.Add(this.listViewExecution);
            this.Controls.Add(this.panelFooter);
            this.Controls.Add(this.panelHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "ScriptExecutionMonitorForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Script Execution Monitor";
            this.Load += new System.EventHandler(this.ScriptExecutionMonitorForm_Load);
            this.panelHeader.ResumeLayout(false);
            this.panelOptions.ResumeLayout(false);
            this.panelStatus.ResumeLayout(false);
            this.panelFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.ListView listViewExecution;
        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Label labelStatusText;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Panel panelOptions;
        private System.Windows.Forms.Panel panelStatus;
        private System.Windows.Forms.CheckBox checkBoxAllowTransaction;
    }
}