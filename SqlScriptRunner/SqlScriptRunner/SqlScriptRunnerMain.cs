using SqlScriptRunner.Database;
using SqlScriptRunner.Forms;
using SqlScriptRunner.ScriptHandler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqlScriptRunner
{
    public partial class SqlScriptRunnerMain : Form
    {
        private string lastUsedFolderName;
        private bool expandingTreeNode;
        private bool checkingTreeNode;
        private bool isInitializing;
        private delegate void LogWriterDelegate(string message);
        internal DbContext dbContext;

        public SqlScriptRunnerMain()
        {
            InitializeComponent();
        }

        private void SqlScriptRunnerMain_Load(object sender, EventArgs e)
        {
            try
            {
                isInitializing = true;
                lastUsedFolderName = Environment.CurrentDirectory;
                if (string.IsNullOrEmpty(lastUsedFolderName)) {
                    lastUsedFolderName = AppDomain.CurrentDomain.BaseDirectory;
                }
                string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!string.IsNullOrEmpty(userHome)) {
                    lastUsedFolderName = userHome;
                }
                textBoxRootFolder.Text = lastUsedFolderName;
                treeViewFileStructure.PathSeparator = $"{Path.DirectorySeparatorChar}";
                EnableUpperButton();
                PopulateSubFolders();
            }
            catch (Exception ex)
            {

                ShowErrorMessage(ex);
            }finally
            {
                isInitializing = false;
            }
        }

        private void buttonBrowsFolder_Click(object sender, EventArgs e)
        {
            try
            {
                 var dialoge = new FolderBrowserDialog() { RootFolder = Environment.SpecialFolder.UserProfile, SelectedPath = lastUsedFolderName};
                if( dialoge.ShowDialog(this) == DialogResult.OK )
                {
                    lastUsedFolderName = dialoge.SelectedPath;
                    EnableUpperButton();
                    textBoxRootFolder.Text = lastUsedFolderName;
                    PopulateSubFolders();
                }
            }
            catch (Exception ex)
            {

                ShowErrorMessage(ex);
            }

        }
        private void EnableUpperButton()
        {
            buttonUp.Enabled = lastUsedFolderName.Split(Path.DirectorySeparatorChar).Length > 1;
        }
        private void PopulateSubFolders()
        {
            AllowTreeNodeEvents(false);
            treeViewFileStructure.BeginUpdate();
            treeViewFileStructure.Nodes.Clear();
            PopulateSubFolders(lastUsedFolderName, null, 0);
            AllowTreeNodeEvents();
            treeViewFileStructure.EndUpdate();
        }

        // This procedure could take a bit longer...
        private void PopulateSubFolders(string lastRootFolder, TreeNode rootNode, int level)
        {
            if (lastRootFolder != null
                && Directory.Exists(lastRootFolder))
            {
                try
                {
                    var files = Directory.GetFiles(lastRootFolder, "*.sql").OrderBy(n => n).ToArray();
                    foreach (var file in files)
                    {
                        var treeNode = new TreeNode(Path.GetFileName(file)) { Tag = file, Checked=true };
                        AppendToRootNode(rootNode, treeNode);
                    }
                    if (rootNode != null && rootNode.Nodes.Count > 0)
                    {
                        rootNode.Checked = true;
                        rootNode.Expand();
                    }
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }                
                try
                {
                    var folders = Directory.GetDirectories(lastRootFolder).OrderBy(n => n).ToArray();
                    foreach (var folder in folders)
                    {
                        var folderNode = new TreeNode(Path.GetFileName(folder));
                        AppendToRootNode(rootNode, folderNode);
                        if (level < 1 || rootNode.Checked)
                        {
                            PopulateSubFolders(folder, folderNode, level + 1);
                        }
                    }
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { };
                if (rootNode != null)
                {
                    rootNode.Checked = rootNode.Nodes.Cast<TreeNode>().Any(n => n.Checked);
                    if (rootNode.Checked)
                    {
                        rootNode.Expand();
                    }
                }
            }
        }

        private void AppendToRootNode(TreeNode rootNode, TreeNode treeNode)
        {
            if (rootNode == null)
            {
                treeViewFileStructure.Nodes.Add(treeNode);
            }
            else
            {
                rootNode.Nodes.Add(treeNode);
            }
        }

        private void ShowErrorMessage(Exception ex)
        {
            MessageBox.Show(this, ex.ToString(), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void treeViewFileStructure_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (e.Node?.Tag != null)
                    {
                        richTextBoxGeneratedContent.Text = File.ReadAllText(e.Node.Tag.ToString());
                    }
                }

            }
            catch (Exception ex)
            {

                ShowErrorMessage(ex);
            }
        }

        private void treeViewFileStructure_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (e.Node?.Tag == null)
                    {
                        // TODO: schedule a reinitialization with the selected node
                        textBoxRootFolder.Text = Path.Combine(lastUsedFolderName, e.Node.FullPath);
                    }
                    else
                    {
                        textBoxRootFolder.Text = Path.Combine(lastUsedFolderName, e.Node.Parent.FullPath);
                    }                    
                }
            }
            catch (Exception ex)
            {

                ShowErrorMessage(ex);
            }
        }
        private void AllowTreeNodeEvents(bool allowed = true)
        {
            if (allowed) 
            {
                treeViewFileStructure.BeforeCheck += TreeViewFileStructure_BeforeCheck;
                treeViewFileStructure.AfterCheck += treeViewFileStructure_AfterCheck;
                treeViewFileStructure.BeforeExpand += treeViewFileStructure_BeforeExpand;
            }
            else
            {
                treeViewFileStructure.BeforeCheck -= TreeViewFileStructure_BeforeCheck;
                treeViewFileStructure.AfterCheck -= treeViewFileStructure_AfterCheck;
                treeViewFileStructure.BeforeExpand -= treeViewFileStructure_BeforeExpand;
            }
        }

        private void TreeViewFileStructure_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            if (isInitializing) return;
            try
            {
                if (e.Node != null && e.Node.Tag == null && e.Node.Nodes.Count > 0)
                {
                    if (!IsFileInNodeTree(e.Node))
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {

                ShowErrorMessage(ex);
            }
            finally
            {
            }
        }

        private void treeViewFileStructure_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (isInitializing) return;
            try
            {
                if (e.Node != null && e.Node.Tag == null && e.Node.Nodes.Count > 0)
                {
                    AllowTreeNodeEvents(false);
                    treeViewFileStructure.BeginUpdate();
                    SetCheckAllTreeNode(e.Node);
                }
            }
            catch (Exception ex)
            {

                ShowErrorMessage(ex);
            }
            finally
            {
                treeViewFileStructure.EndUpdate();
                Application.DoEvents();
                AllowTreeNodeEvents();
            }
        }
        private List<string> CollectAllCheckedFiles()
        {
            var result = new List<string>();
            foreach(TreeNode node in treeViewFileStructure.Nodes)
            {
                if (node.Tag != null && node.Checked)
                {
                    result.Add(node.Tag.ToString());
                }
                else
                {
                    CollectAllCheckedFiles(result, node);
                }
            }
            return result;
        }
        private void CollectAllCheckedFiles(List<string> list, TreeNode root)
        {
            foreach (TreeNode node in root.Nodes)
            {
                if (node.Tag != null && node.Checked)
                {
                    list.Add(node.Tag.ToString());
                }
                else
                {
                    CollectAllCheckedFiles(list, node);
                }
            }
        }
        private void SetCheckAllTreeNode(TreeNode node)
        {
            // We need to consider only where we have files too!
            node.Nodes.Cast<TreeNode>().ToList().ForEach(n => 
            { 
                if (IsFileInNodeTree(n))
                {
                    n.Checked = node.Checked;
                }
                SetCheckAllTreeNode(n);
            });
        }
        private bool IsFileInNodeTree(TreeNode node) 
        {
            bool result = false;
            // Need to check if there is a file in any of the end-node
            if (node != null) 
            {
                if (node.Nodes.Count == 0 && node.Tag != null)
                {
                    // file node
                    result = true;
                }
                else
                {
                    result = node.Nodes.Cast<TreeNode>().Any(IsFileInNodeTree);
                }
            }
            return result;
        }
        private void treeViewFileStructure_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (isInitializing) return;
            try
            {
                // Get the full path and the base path and reinitialize the node structure
                // expandingTreeNode = true;
                AllowTreeNodeEvents(false);
                treeViewFileStructure.BeginUpdate();
                e.Node.Nodes.Clear();
                PopulateSubFolders(Path.Combine(lastUsedFolderName, e.Node.FullPath), e.Node, 0);
            }
            catch (Exception ex)
            {

                ShowErrorMessage(ex);
            }
            finally
            {
                treeViewFileStructure.EndUpdate();
                Application.DoEvents();
                AllowTreeNodeEvents();
                // expandingTreeNode = false;
            }
        }

        private void textBoxRootFolder_TextChanged(object sender, EventArgs e)
        {
            if (isInitializing) return;
            try
            {
                if (!string.IsNullOrEmpty(textBoxRootFolder.Text) 
                    && string.Compare(lastUsedFolderName, textBoxRootFolder.Text, ignoreCase: false) != 0
                    && Directory.Exists(textBoxRootFolder.Text))
                {
                    lastUsedFolderName = textBoxRootFolder.Text;
                    EnableUpperButton();
                    PopulateSubFolders();
                }
            }
            catch (Exception ex)
            {

                ShowErrorMessage(ex);
            }
        }

        private void textBoxRootFolder_Leave(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(textBoxRootFolder.Text)
                    && string.Compare(lastUsedFolderName, textBoxRootFolder.Text, ignoreCase: false) != 0
                    && Directory.Exists(textBoxRootFolder.Text))
                {
                    treeViewFileStructure.Nodes.Clear();
                    lastUsedFolderName = textBoxRootFolder.Text;
                    EnableUpperButton();
                    PopulateSubFolders();
                }
            }
            catch (Exception ex)
            {

                ShowErrorMessage(ex);
            }
        }

        private void saveGeneratedScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(richTextBoxGeneratedContent.Text))
                {
                    var saveFileDialog = new SaveFileDialog() 
                    {
                        DefaultExt = "sql", 
                        Filter = "SQL files (*.sql)|*.sql|Text files (*.txt)|*.txt|All files (*.*)|*.*",
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        Title = "Save Generated Script",
                        OverwritePrompt = true,
                        FileName = richTextBoxGeneratedContent.Tag == null ? $"{treeViewFileStructure.SelectedNode?.Name ?? "NoName.sql"}" : $"{richTextBoxGeneratedContent.Tag}"
                    };
                    
                    if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        var file = saveFileDialog.FileName;
                        File.WriteAllText(file, richTextBoxGeneratedContent.Text, Encoding.UTF8);
                    }
                }
            }
            catch (Exception ex)
            {

                ShowErrorMessage(ex);
            }
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            try
            {
                var upperFolder = Path.GetDirectoryName(lastUsedFolderName);
                textBoxRootFolder.Text = upperFolder;
            }
            catch (Exception ex)
            {

                ShowErrorMessage(ex);
            }
        }
        private void LogWriter(string message)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    var op = new LogWriterDelegate(LogWriter);
                    this.Invoke(op, message);
                }
                else
                {
                    toolStripStatusLabel.Text = message;
                }
            }
            catch (Exception ex)
            {

                ShowErrorMessage(ex);
            }
        }
        private async void buttonMakeScript_Click(object sender, EventArgs e)
        {
            try
            {
                buttonMakeScript.Enabled = false;
                var fileList = CollectAllCheckedFiles();
                if (fileList.Count > 0)
                {
                    // create a script collection
                    var scriptLoader = new ScriptLoader(fileList, m => LogWriter(m));
                    await scriptLoader.LoadingTask;
                    // Look inside those file and try to determine if they has specific content.
                    await scriptLoader.ProcessScripts();
                                        
                    richTextBoxGeneratedContent.Text = scriptLoader.CreateScript();
                }
                else
                {
                    richTextBoxGeneratedContent.Text = "-- There is no file selected!";
                    LogWriter("Loading Files: There is no file selected!");
                }
            }
            catch (Exception ex)
            {

                ShowErrorMessage(ex);
            }
            finally
            {
                buttonMakeScript.Enabled = true;
            }
        }

        private void connectToDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var connectToDB = new ConnectToDatabaseForm(dbContext);

                if (connectToDB.ShowDialog(this) == DialogResult.OK)
                {
                    // Initialize new DB Context with the data and store it.
                    dbContext = (DbContext)connectToDB.Tag;
                    SetHeaderText();
                }
            }
            catch (Exception ex)
            {

                ShowErrorMessage(ex);
            }
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                dbContext = null;
                SetHeaderText();
            }
            catch (Exception ex)
            {

                ShowErrorMessage(ex);
            }
        }

        private void SetHeaderText()
        {
            this.Text = "Sql Script Runner";
            if (dbContext != null)
            {
                this.Text += $" {dbContext.ConnectionStringBuilder.InitialCatalog}@{dbContext.ConnectionStringBuilder.DataSource}";
            }
            buttonInsertToDatabase.Enabled = dbContext != null;
        }

        private void buttonInsertToDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                // TODO: 
            }
            catch (Exception ex)
            {

                ShowErrorMessage(ex);
            }
        }
    }
}
