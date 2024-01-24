using SqlScriptRunner.Database;
using SqlScriptRunner.Extensions;
using SqlScriptRunner.Forms;
using SqlScriptRunner.Logger;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SqlScriptRunner.ScriptHandler.ScriptLoader;

namespace SqlScriptRunner
{
    public partial class SqlScriptRunnerMain : Form
    {
        private string lastUsedFolderName;
        private bool expandingTreeNode;
        private bool checkingTreeNode;
        private bool isInitializing;
        private delegate void LogWriterDelegate(string message, LogLevelEnum? logLevel);
        internal DbContext dbContext;
        private StatusCallBackDelegate scriptStatusCallBack;
        private CancellationTokenSource cancellationTokenSource;
        private UsersGuideForm userGuideForm;

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

                this.ShowErrorMessage(ex);
            }finally
            {
                isInitializing = false;
            }
        }

        private void buttonBrowsFolder_Click(object sender, EventArgs e)
        {
            try
            {
                 var dialoge = new FolderBrowserDialog() { RootFolder = Environment.SpecialFolder.MyComputer, SelectedPath = lastUsedFolderName};
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

                this.ShowErrorMessage(ex);
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

                this.ShowErrorMessage(ex);
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

                this.ShowErrorMessage(ex);
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
            if (isInitializing || checkingTreeNode) return;
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

                this.ShowErrorMessage(ex);
            }
            finally
            {
            }
        }

        private void treeViewFileStructure_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (isInitializing || checkingTreeNode) return;
            try
            {
                if (e.Node != null && e.Node.Tag == null && e.Node.Nodes.Count > 0)
                {
                    AllowTreeNodeEvents(false);
                    checkingTreeNode = true;
                    treeViewFileStructure.BeginUpdate();
                    SetCheckAllTreeNode(e.Node);
                }
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
            finally
            {
                treeViewFileStructure.EndUpdate();

                AllowTreeNodeEvents();
                // Application.DoEvents();
                checkingTreeNode = false;
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
            if (isInitializing || checkingTreeNode) return;
            try
            {
                // Get the full path and the base path and reinitialize the node structure
                // expandingTreeNode = true;
                AllowTreeNodeEvents(false);
                checkingTreeNode = true;
                treeViewFileStructure.BeginUpdate();
                e.Node.Nodes.Clear();
                PopulateSubFolders(Path.Combine(lastUsedFolderName, e.Node.FullPath), e.Node, 0);
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
            finally
            {
                treeViewFileStructure.EndUpdate();
                // Application.DoEvents();
                AllowTreeNodeEvents();
                checkingTreeNode = false;
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

                this.ShowErrorMessage(ex);
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

                this.ShowErrorMessage(ex);
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

                this.ShowErrorMessage(ex);
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

                this.ShowErrorMessage(ex);
            }
        }
        private void LogWriter(string message)
        {
            LogWriter(message, LogLevelEnum.Debug);
        }
        private void LogWriter(string message, LogLevelEnum? logLevel )
        {
            try
            {
                if (this.InvokeRequired)
                {
                    var op = new LogWriterDelegate(LogWriter);
                    this.Invoke(op, message, logLevel);
                }
                else
                {
                    toolStripStatusLabel.Text = message;
                }
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }
        private async Task<ScriptLoader> PrepareScripts()
        {
            ScriptLoader scriptLoader = null;
            var fileList = CollectAllCheckedFiles();

            if (fileList.Count > 0)
            {
                // create a script collection
                scriptLoader = new ScriptLoader(fileList, (m,level) => LogWriter(m,level));
                await scriptLoader.LoadingTask;
                // Look inside those file and try to determine if they has specific content.
                await scriptLoader.ProcessScripts();                
            }

            return scriptLoader;
        }
        private async void buttonMakeScript_Click(object sender, EventArgs e)
        {
            try
            {
                buttonMakeScript.Enabled = false;
                var scriptLoader = await PrepareScripts();
                var fileList = CollectAllCheckedFiles();
                if (scriptLoader != null)
                {
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

                this.ShowErrorMessage(ex);
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

                this.ShowErrorMessage(ex);
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

                this.ShowErrorMessage(ex);
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

        private async void buttonInsertToDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                var scriptLoader = await PrepareScripts();
                // Open the script execution monitor for monitoring the script execution
                if (scriptLoader != null)
                {
                    var monitor = new ScriptExecutionMonitorForm(scriptLoader, DatabaseInsertionCommands);
                    cancellationTokenSource = new CancellationTokenSource();
                    // Add call back references to the monitor to start up the script execution
                    scriptStatusCallBack = new StatusCallBackDelegate(monitor.StatusUpdate);
                    monitor.ShowDialog(this);
                }
                else
                {
                    throw new Exception("There is no file selected'");
                }
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }

        private void DatabaseInsertionCommands(ScriptLoader scriptLoader, string command)
        {
            //implement Start, Cancel commands, Setting settings separated with (:) colon.
            var subCommand = command.Split(':');
            
            switch (subCommand[0])
            {
                case "Start":
                    // TODO: Start the asynchronous procedure to apply DB scripts into database. Use Cancellation token.
                    richTextBoxGeneratedContent.Clear();
                    ExecutionLogAction($"Start executing sequence with{(scriptLoader.WithTransaction ? "" : "out")} transaction", LogLevelEnum.Info);
                    scriptLoader.ApplyScriptsToDatabase(dbContext, ExecutionLogAction, scriptStatusCallBack, cancellationTokenSource.Token);
                    break;
                case "Cancel":
                    // TODO: Cancel and roll back
                    cancellationTokenSource.Cancel(throwOnFirstException: true);
                    break;
                case "Transaction":
                    scriptLoader.WithTransaction = subCommand.Length > 1 ? bool.Parse(subCommand[1]) : true;
                    break;
                default:
                    break;
            }
        }
        private void ExecutionLogAction(string str)
        {
            ExecutionLogAction(str, LogLevelEnum.Debug);
        }
        private void ExecutionLogAction(string str, LogLevelEnum? logLevel)
        {
            if (InvokeRequired)
            {
                LogWriterDelegate op = new LogWriterDelegate(ExecutionLogAction);
                Invoke(op, str, logLevel);
                Application.DoEvents();
            }
            else
            {
                var textColor = Color.Black;
                if (logLevel != null)
                {
                    switch (logLevel.Value)
                    {
                        case LogLevelEnum.Debug:
                            textColor = Color.Black;
                            break;
                        case LogLevelEnum.Info:
                            textColor = Color.Blue;
                            break;
                        case LogLevelEnum.Warn:
                            textColor = Color.Orange;
                            break;
                        case LogLevelEnum.Error:
                            textColor = Color.Red;
                            break;
                        default:
                            textColor = Color.Black;
                            break;
                    }
                }
                richTextBoxGeneratedContent.AppendText($"{DateTime.Now:u}: {str}\n",textColor);
                richTextBoxGeneratedContent.ScrollToCaret();
                richTextBoxGeneratedContent.ForeColor = Color.Black;
            }
        }

        private void usersGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UsersGuide.html");
                if (File.Exists(path))
                {
                    if (userGuideForm == null)
                    {
                        userGuideForm = new UsersGuideForm(path);
                        userGuideForm.Show(this);
                    }
                    else
                    {
                        if (userGuideForm.IsDisposed)
                        {
                            userGuideForm = new UsersGuideForm(path);
                            userGuideForm.Show(this);
                        }
                    }
                    userGuideForm?.Focus();
                }
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string message = string.Format("{0} v.{1}\nCopyright (c) 2024 Csaba Nagy\nMIT license"
                    , Application.ProductName
                    , Application.ProductVersion);
                MessageBox.Show(this,message, "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage(ex);
            }
        }
    }
}
