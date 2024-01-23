using SqlScriptRunner.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqlScriptRunner.Forms
{
    public partial class UsersGuideForm : Form
    {
        Uri navigationPath;
        string validAddress;
        public UsersGuideForm()
        {
            InitializeComponent();
        }
        public UsersGuideForm(string path) : this()
        {
            if (!string.IsNullOrEmpty(path)) 
            {
                navigationPath = new Uri(path);

            }
        }

        private void UsersGuideForm_Load(object sender, EventArgs e)
        {
            try
            {
                webBrowserUsersGuide.Visible = false;
                labelLoadingDocument.Visible = true;
                Application.DoEvents();
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
                // Cancel the navigation if it would go outside the page
                if (string.IsNullOrEmpty(validAddress))
                {
                    var validAddressMatch = Regex.Match(e.Url.ToString(), @".+?(?=UsersGuide\.html)");
                    if (validAddressMatch.Success)
                    {
                        validAddress = validAddressMatch.Value;
                    }
                }
                if (!e.Url.ToString().StartsWith(validAddress))
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }

        private void webBrowserUsersGuide_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            try
            {
                webBrowserUsersGuide.Visible = true;
                labelLoadingDocument.Visible = false;
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }
    }
}
