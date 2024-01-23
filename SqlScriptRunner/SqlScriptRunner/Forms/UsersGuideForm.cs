using SqlScriptRunner.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqlScriptRunner.Forms
{
    public partial class UsersGuideForm : Form
    {
        string navigationPath;
        public UsersGuideForm()
        {
            InitializeComponent();
        }
        public UsersGuideForm(string path) : this()
        {
            navigationPath = path;
        }

        private void UsersGuideForm_Load(object sender, EventArgs e)
        {
            try
            {
                if (navigationPath != null)
                {
                    webBrowserUsersGuide.Navigate(navigationPath);
                }
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }

        private void webBrowserUsersGuide_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            try
            {
                // TODO: Cancel the navigation if it would go outside we page
                // MessageBox.Show(this, e.TargetFrameName, "Target Frame Name", MessageBoxButtons.OK, MessageBoxIcon.Information); // only temporary!
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }
    }
}
