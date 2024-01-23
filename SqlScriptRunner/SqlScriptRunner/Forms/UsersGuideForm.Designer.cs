namespace SqlScriptRunner.Forms
{
    partial class UsersGuideForm
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
            this.webBrowserUsersGuide = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // webBrowserUsersGuide
            // 
            this.webBrowserUsersGuide.AllowWebBrowserDrop = false;
            this.webBrowserUsersGuide.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowserUsersGuide.IsWebBrowserContextMenuEnabled = false;
            this.webBrowserUsersGuide.Location = new System.Drawing.Point(0, 0);
            this.webBrowserUsersGuide.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowserUsersGuide.Name = "webBrowserUsersGuide";
            this.webBrowserUsersGuide.Size = new System.Drawing.Size(800, 450);
            this.webBrowserUsersGuide.TabIndex = 0;
            this.webBrowserUsersGuide.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.webBrowserUsersGuide_Navigating);
            // 
            // UsersGuideForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.webBrowserUsersGuide);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "UsersGuideForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Users Guide";
            this.Load += new System.EventHandler(this.UsersGuideForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowserUsersGuide;
    }
}