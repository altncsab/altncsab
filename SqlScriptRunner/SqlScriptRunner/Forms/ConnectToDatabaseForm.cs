using SqlScriptRunner.Database;
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
    internal partial class ConnectToDatabaseForm : Form
    {
        private DbContext Db;

        public ConnectToDatabaseForm()
        {
            InitializeComponent();
        }
        public ConnectToDatabaseForm(DbContext db):this()
        {
            this.Db = db;
        }
        private bool isContentValid() 
        {
            var isValid = false;

            if (!string.IsNullOrWhiteSpace(textBoxServer.Text)
                    && !string.IsNullOrEmpty(comboBoxDatabase.Text))
            {
                if (!checkBoxIsTrusted.Checked)
                {
                    if (!string.IsNullOrWhiteSpace(textBoxUserName.Text)
                        && !string.IsNullOrEmpty(textBoxPassword.Text))
                    {
                        isValid = true;
                    }
                }
                else
                {
                    isValid = true;
                }
            }
        
            return isValid;
        }

        private DbContext BuildDbContext()
        {
            DbContext result = null;

            if (!string.IsNullOrWhiteSpace(textBoxServer.Text)
                    && !string.IsNullOrEmpty(comboBoxDatabase.Text))
            {
                if (!checkBoxIsTrusted.Checked)
                {
                    if (!string.IsNullOrWhiteSpace(textBoxUserName.Text)
                        && !string.IsNullOrEmpty(textBoxPassword.Text))
                    {
                        result = new DbContext(textBoxServer.Text, checkBoxIsTrusted.Checked,
                            comboBoxDatabase.Text, textBoxUserName.Text, textBoxPassword.Text);
                    }
                }
                else
                {
                    result = new DbContext(textBoxServer.Text, comboBoxDatabase.Text);
                }
            }
            return result;
        }
        private void ConnectToDatabaseForm_Load(object sender, EventArgs e)
        {
            try
            {
                if (Db != null)
                {                    
                    textBoxServer.Text = Db.ConnectionStringBuilder.DataSource;
                    checkBoxIsTrusted.Checked = Db.ConnectionStringBuilder.IntegratedSecurity;
                    if (!checkBoxIsTrusted.Checked)
                    {
                        textBoxUserName.Text = Db.ConnectionStringBuilder.UserID;
                        textBoxPassword.Text = Db.ConnectionStringBuilder.Password;
                    }
                    comboBoxDatabase.BeginUpdate();
                    comboBoxDatabase.Items.Clear();
                    comboBoxDatabase.Items.Add(Db.ConnectionStringBuilder.InitialCatalog);
                    comboBoxDatabase.SelectedIndex = 0;
                    comboBoxDatabase.EndUpdate();
                }
                buttonSave.Enabled = isContentValid();
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                this.Tag = BuildDbContext();
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }

        private void buttonQueryDB_Click(object sender, EventArgs e)
        {
            try
            {
                var context = new DbContext(textBoxServer.Text, checkBoxIsTrusted.Checked,
                            "master", textBoxUserName.Text, textBoxPassword.Text);
                var currentItem = comboBoxDatabase.Text;
                // ToDo: call the QueryBase for the list
                var catalogListQuery = new ListDatabases();
                string[] catalogList = (string[])catalogListQuery.Execute(context);
                // Test: Add master as default for testing
                comboBoxDatabase.BeginUpdate();
                comboBoxDatabase.Items.Clear();
                comboBoxDatabase.Items.AddRange(catalogList);
                if (string.IsNullOrEmpty(currentItem)
                   || (!string.IsNullOrEmpty(currentItem) 
                        && !catalogList.Any(it => string.Compare(currentItem, it, ignoreCase: true) == 0)))
                {
                    comboBoxDatabase.SelectedIndex = 0;
                }

                comboBoxDatabase.EndUpdate();

            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }

        private void checkBoxIsTrusted_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                textBoxPassword.Enabled = !checkBoxIsTrusted.Checked;
                textBoxUserName.Enabled = !checkBoxIsTrusted.Checked;
                labelPassword.Enabled = !checkBoxIsTrusted.Checked;
                labelUserName.Enabled = !checkBoxIsTrusted.Checked;
                buttonSave.Enabled = isContentValid();
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }

        private void textBoxServer_TextChanged(object sender, EventArgs e)
        {
            try
            {
                buttonSave.Enabled = isContentValid();
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }

        private void textBoxUserName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                buttonSave.Enabled = isContentValid();
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }

        private void textBoxPassword_TextChanged(object sender, EventArgs e)
        {
            try
            {
                buttonSave.Enabled = isContentValid();
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }

        private void comboBoxDatabase_TextChanged(object sender, EventArgs e)
        {
            try
            {
                buttonSave.Enabled = isContentValid();
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }
    }
}
