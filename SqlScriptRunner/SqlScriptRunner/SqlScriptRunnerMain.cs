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
        private delegate void SubFoldersReadyDelegate(TreeNode tootNode ,TreeNode targetRootNode = null);
        private delegate void CopyToTreeViewDelegate(TreeNode rootNode, TreeNode targetRootNode = null);
        internal DbContext dbContext;
        private StatusCallBackDelegate scriptStatusCallBack;
        private CancellationTokenSource cancellationTokenSourceSql;
        private CancellationTokenSource cancellationTokenSourceTreeNodes;
        private Task TreeNodeTask;
        private UsersGuideForm userGuideForm;
        private TreeNode asyncRoot;

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
        private Task PopulateSubFolders(TreeNode rootNode = null)
        {
            treeViewFileStructure.BeginUpdate();
            TreeNode backUp = new TreeNode();
            if (rootNode == null)
            {
                backUp.Nodes.AddRange(
                        treeViewFileStructure.Nodes.Cast<TreeNode>().Select(tn => (TreeNode)tn.Clone()).ToArray()
                    );
                //CopyToTreeView(treeViewFileStructure.Nodes, backUp);
                treeViewFileStructure.Nodes.Clear();
                treeViewFileStructure.Nodes.Add("Initializing folder structure...");
            }
            else
            {
                CopyToTreeView(rootNode, backUp); 
                rootNode.Nodes.Clear();
                rootNode.Nodes.Add("Loading folders...");
            }
            treeViewFileStructure.EndUpdate();
            asyncRoot = new TreeNode();
            if(cancellationTokenSourceTreeNodes != null && TreeNodeTask != null)
            {
                cancellationTokenSourceTreeNodes.Cancel(throwOnFirstException: true);
                // It could happen the application is invoking to Form thread. It could result deadlock situation.
                // That is the reason for DoEvent and timeout for Wait.
                Application.DoEvents();
                TreeNodeTask.Wait(TimeSpan.FromMilliseconds(1000));
            }
            cancellationTokenSourceTreeNodes = new CancellationTokenSource();
            TreeNodeTask = Task.Run(() =>
            {
                try
                {
                    var pathToLookIn = Path.Combine(lastUsedFolderName, (rootNode?.TreeView != null ? rootNode?.FullPath : ""));
                    PopulateSubFolders(pathToLookIn, asyncRoot, 0);
                    cancellationTokenSourceTreeNodes?.Token.ThrowIfCancellationRequested();
                    SubFoldersReady(asyncRoot, rootNode);
                }
                catch (OperationCanceledException)
                {
                    CopyToTreeView(backUp, rootNode);
                }
                finally
                {
                    cancellationTokenSourceTreeNodes = null;
                }
            }, cancellationTokenSourceTreeNodes.Token);
            return TreeNodeTask;
        }
        private void SubFoldersReady(TreeNode rootNode, TreeNode targetRootNode = null)
        {
            if (InvokeRequired)
            {
                var op = new SubFoldersReadyDelegate(SubFoldersReady);
                cancellationTokenSourceTreeNodes?.Token.ThrowIfCancellationRequested();
                Invoke(op, rootNode, targetRootNode);
            }
            else
            {
                AllowTreeNodeEvents(false);
                treeViewFileStructure.BeginUpdate();
                CopyToTreeView(rootNode, targetRootNode);
                treeViewFileStructure.EndUpdate();
                AllowTreeNodeEvents();

            }
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
                    cancellationTokenSourceTreeNodes?.Token.ThrowIfCancellationRequested();
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
                    cancellationTokenSourceTreeNodes?.Token.ThrowIfCancellationRequested();
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

        private void CopyToTreeView(TreeNode rootNode, TreeNode targetRootNode = null)
        {
            if ((targetRootNode == null && treeViewFileStructure.InvokeRequired) ||
                (targetRootNode != null && (targetRootNode.TreeView?.InvokeRequired ?? false)))
            {
                var op = new CopyToTreeViewDelegate(CopyToTreeView);
                _ = Invoke(op, rootNode, targetRootNode);
            }
            else
            {
                // we need only children
                if (rootNode != null)
                {
                    TreeNodeCollection nodeCollection;
                    if (targetRootNode == null)
                    {
                        nodeCollection = treeViewFileStructure.Nodes;
                    }
                    else
                    {
                        nodeCollection = targetRootNode.Nodes;
                    }
                    nodeCollection.Clear();
                    nodeCollection
                        .AddRange(
                            rootNode.Nodes.Cast<TreeNode>().Select(tn => 
                            {
                                var newNode = (TreeNode)tn.Clone();
                                if (tn.IsExpanded)
                                {
                                    newNode.Expand();
                                }
                                return newNode; 
                            }).ToArray()
                        );
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
                e.Node.Expand();
                PopulateSubFolders(e.Node);
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
            finally
            {
                treeViewFileStructure.EndUpdate();
                AllowTreeNodeEvents();
                checkingTreeNode = false;
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
                // do not remove the last part of the path.
                if (!string.IsNullOrWhiteSpace(upperFolder))
                {
                    textBoxRootFolder.Text = upperFolder;
                }
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
                    cancellationTokenSourceSql = new CancellationTokenSource();
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
            ScriptCommandEnum baseCommands = ScriptCommandEnum.Nothing;
            _ = Enum.TryParse(subCommand[0], out baseCommands);
            switch (baseCommands)
            {
                case ScriptCommandEnum.Start:
                    // Start the asynchronous procedure to apply DB scripts into database. Use Cancellation token.
                    richTextBoxGeneratedContent.Clear();
                    ExecutionLogAction($"Start executing sequence with{(scriptLoader.WithTransaction ? "" : "out")} transaction", LogLevelEnum.Info);
                    scriptLoader.ApplyScriptsToDatabase(dbContext, ExecutionLogAction, scriptStatusCallBack, cancellationTokenSourceSql.Token);
                    break;
                case ScriptCommandEnum.Cancel:
                    // Cancel and roll back
                    cancellationTokenSourceSql.Cancel(throwOnFirstException: true);
                    break;
                case ScriptCommandEnum.Transaction:
                    scriptLoader.WithTransaction = subCommand.Length > 1 ? bool.Parse(subCommand[1]) : true;
                    break;
                case ScriptCommandEnum.SetSkip:
                    if (subCommand.Length >= 4)
                    {
                        var scriptName = Guid.Parse(subCommand[1]);
                        var sectionId = int.Parse(subCommand[2]);
                        var ToBeScipped = bool.Parse(subCommand[3]);
                        // Find and set the new status for the addressed script section.
                        scriptLoader.SetScriptSectionSkipState(scriptName, sectionId, ToBeScipped);
                    }
                    break;
                case ScriptCommandEnum.SkipIfExists:
                    scriptLoader.SkipObjectIfExists = subCommand.Length > 1 ? bool.Parse(subCommand[1]) : true;
                    break;
                default:
                    // DO nothing
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
