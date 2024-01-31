using SqlScriptRunner.Database;
using SqlScriptRunner.Logger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace SqlScriptRunner.ScriptHandler
{
    internal class ScriptLoader
    {
        private List<Script> scriptFileList;
        private ConcurrentBag<ScriptSection> scriptSections = new ConcurrentBag<ScriptSection>();
        private ConcurrentDictionary<string, string> scriptMap = new ConcurrentDictionary<string, string>();
        private static List<ObjectTypeNameEnum> executionSequence = new List<ObjectTypeNameEnum>()
            {
                ObjectTypeNameEnum.Type, ObjectTypeNameEnum.Function, ObjectTypeNameEnum.Table,
                ObjectTypeNameEnum.View, ObjectTypeNameEnum.Procedure, ObjectTypeNameEnum.Script
            };

        public Task LoadingTask { get; set; }
        public List<Script> ScriptFileList => scriptFileList;
        public bool WithTransaction { get; set; }
        public bool IsCompleted => !scriptSections?.Any(ss => ss.Status == null || ss.Status == ExecutionStatusEnum.Running) ?? true;

        public bool SkipObjectIfExists { get; set; }

        private Action<string, LogLevelEnum?> LogAction;
        internal delegate void StatusCallBackDelegate(ScriptSection script);

        public ScriptLoader(IEnumerable<string> scriptFiles, Action<string, LogLevelEnum?> logAction)
        {
            // scriptFileList = scriptFiles;
            scriptFileList = new List<Script>();
            LogAction = logAction;
            WithTransaction = true;
            LoadingTask = Task.Run(() =>
            {
                var result = Parallel.ForEach(scriptFiles, scriptPath =>
                {
                    scriptMap.TryAdd(scriptPath, File.ReadAllText(scriptPath));
                    Log($"File '{scriptPath}' is loaded", LogLevelEnum.Debug);
                });
                return result;
            });
            
        }
        /// <summary>
        /// Asynchronously processing all files to cut them to basic scripts and identifying them.
        /// </summary>
        public Task ProcessScripts()
        {
            return Task.Run(() => 
            {
                return Parallel.ForEach(scriptMap, scriptItem => 
                {
                    var scriptSectionRange = ScriptSection.ParseSections(scriptItem.Key, scriptItem.Value);
                    scriptFileList.Add(new Script(scriptItem.Key, scriptSectionRange));
                    scriptSectionRange.ForEach(scriptSections.Add);
                    Log($"File '{scriptItem.Key}' has been parsed.", LogLevelEnum.Debug);

                });
            });
        }
        /// <summary>
        /// After process script completed the execution, it creates an execution order list based on predefined orders and type references.
        /// 1: type definitions, 2: Functions, 3: Tables, 4: Procedures, 5: Views, 6: General scripts
        /// The file path could influence the execution order.
        /// </summary>
        /// <returns>The sorted script sections</returns>
        public IEnumerable<ScriptSection> ScriptExecutionSequence()
        {
            // Sort based on file name first and look dependent object for sorting! Only change order based on that.
            var resultSequnce = new List<ScriptSection>();
            var scriptSequence = SortedFileList();
            // TODO: inspect the possibility if we may change the internal sequence compare as it appears in the file
            scriptSequence.ToList().ForEach(s => resultSequnce.AddRange(s.ScriptSections));
            return resultSequnce;
        }
        /// <summary>
        /// Returns the file list sorted by execution sequence and by file FilePath
        /// </summary>
        /// <returns>File execution order of Script list</returns>
        public IEnumerable<Script> SortedFileList()
        {
            var scriptSequence = new List<Script>();
            executionSequence.ForEach(ot =>
                 scriptSequence.AddRange(scriptFileList
                    .Where(s => !scriptSequence.Contains(s) && s.ScriptSections.Any(ss => ss.ObjectTypeName == ot))
                    .OrderBy(f => f.FilePath))
                );
            return scriptSequence;
        }
        /// <summary>
        /// Running all script segments on the configured SQL server instance Asynchronously. Updating Logger and presenter over the progress.
        /// </summary>
        /// <param name="db">Database context where the scripts may need to run</param>
        /// <param name="ExecutionLogAction">Action delegate for logging execution events from SQL server</param>
        /// <param name="statusCallBack">Updating the Status about the current progress</param>
        /// <param name="cancellationToken">User requested cancellation</param>
        /// <returns></returns>
        public Task ApplyScriptsToDatabase (DbContext db, Action<string,LogLevelEnum?> ExecutionLogAction, StatusCallBackDelegate statusCallBack, CancellationToken cancellationToken)
        {
            return Task.Run(() => 
            {                
                IEnumerable<ScriptSection> scriptSequence = ScriptExecutionSequence();
                // Reset the status of each scripts to null
                foreach (var item in scriptSequence)
                {
                    item.Status = null;
                    statusCallBack.Invoke(item);
                }
                var scriptApplyer = new ApplyScript() { WithTransaction = WithTransaction, LogFunction = ExecutionLogAction };
                ScriptSection currScriptSection = null;
                try
                {
                    string message;
                    foreach (var scriptSection in scriptSequence)
                    {
                        currScriptSection = scriptSection;
                        currScriptSection.MessageLog.Clear();
                        message = $"Start Executing: '{currScriptSection.FilePath}', GO#{currScriptSection.SectionId}";
                        ExecutionLogAction?.Invoke(message, LogLevelEnum.Debug);
                        currScriptSection.MessageLog.Add(message);
                        currScriptSection.Status = ExecutionStatusEnum.Running;
                        statusCallBack.Invoke(currScriptSection);
                        if (scriptSection.ToBeSkipped)
                        {
                            currScriptSection.Status = ExecutionStatusEnum.Skipped;
                            message = $"Script section skipped by user: '{currScriptSection.FilePath}', GO#{currScriptSection.SectionId}";
                            currScriptSection.MessageLog?.Add(message);
                            ExecutionLogAction?.Invoke(message, LogLevelEnum.Warn);
                        }
                        else
                        {
                            currScriptSection.Status = (ExecutionStatusEnum)scriptApplyer.Execute(db, currScriptSection, cancellationToken);
                            if (currScriptSection.Status != ExecutionStatusEnum.Completed)
                            {
                                // There could be several scenario why we should not abort the operation.
                                // TODO: Inspect the error message from the script applier and inject a drop section or empty body creation before retry function and procedure creation.
                                // TODO: Table types in use needs special handling to replace!
                                // Example: Cannot drop type 'dbo.MatrixType' because it is being referenced by object 'CreateXmlFromMatrix'. There may be other objects that reference this type.
                                if (SkipObjectIfExists)
                                {
                                    // check the log for specific error message look for error for existing object.
                                    if (IsObjectAllreadyExistsError(currScriptSection))
                                    {
                                        currScriptSection.Status = ExecutionStatusEnum.Skipped;
                                        message = $"Execution skipped: '{currScriptSection.FilePath}', GO#{currScriptSection.SectionId}";
                                        currScriptSection.MessageLog?.Add(message);
                                        ExecutionLogAction?.Invoke(message, LogLevelEnum.Warn);
                                        statusCallBack.Invoke(currScriptSection);
                                    }
                                }
                                if (currScriptSection.Status != ExecutionStatusEnum.Skipped)
                                {
                                    message = $"Execution failed: '{currScriptSection.FilePath}', GO#{currScriptSection.SectionId}";
                                    currScriptSection.MessageLog?.Add(message);
                                    ExecutionLogAction?.Invoke(message, LogLevelEnum.Error);
                                    if (WithTransaction)
                                    {
                                        scriptApplyer.RollBackTransaction();
                                        message = "Rolling back all actions";
                                        currScriptSection.MessageLog?.Add(message);
                                        ExecutionLogAction?.Invoke(message, LogLevelEnum.Warn);
                                    }
                                    statusCallBack.Invoke(currScriptSection);
                                    break;
                                }
                            }
                        }
                        message = $"Execution completed: '{currScriptSection.FilePath}', GO#{currScriptSection.SectionId}";
                        currScriptSection.MessageLog?.Add(message);
                        ExecutionLogAction?.Invoke(message, LogLevelEnum.Debug);
                        statusCallBack.Invoke(scriptSection);
                    }
                    scriptApplyer.CommitTransaction();
                }
                catch (ThreadAbortException) { }
                catch (OperationCanceledException) 
                {
                    if (cancellationToken.IsCancellationRequested)
                    {

                        scriptApplyer.RollBackTransaction();
                        if (currScriptSection != null)
                        {
                            currScriptSection.Status = ExecutionStatusEnum.Failed;
                            var message = $"Cancellation Requested: '{currScriptSection.FilePath}', GO#{currScriptSection.SectionId}";
                            currScriptSection.MessageLog?.Add(message);
                            ExecutionLogAction?.Invoke(message, LogLevelEnum.Warn);
                            statusCallBack.Invoke(currScriptSection);
                        }
                    }
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    if (scriptSequence.Any(ss => ss.Status == null))
                    {
                        foreach (var scriptSection in scriptSequence.Where(ss => ss.Status == null))
                        {
                            scriptSection.Status = ExecutionStatusEnum.Canceled;
                            scriptSection.MessageLog.Add("Canceled");
                            statusCallBack.Invoke(scriptSection);
                        }
                    }
                    ExecutionLogAction?.Invoke("Finished executing sequence", LogLevelEnum.Info);
                }
            }, cancellationToken
            );
        }
        /// <summary>
        /// Finding and setting the execution state of a script section
        /// </summary>
        /// <param name="guid">generated internal guid of the script object</param>
        /// <param name="sectionId">the section id of the script section</param>
        /// <param name="skipState">true for skip, false for not...</param>
        public void SetScriptSectionSkipState(Guid guid, int sectionId, bool skipState)
        {
            var scriptSection = scriptSections.FirstOrDefault(ss => ss.Script?.Guid == guid && ss.SectionId == sectionId);
            if (scriptSection != null)
            {
                scriptSection.ToBeSkipped = skipState;
            }
        }
        /// <summary>
        /// Creates the full script for reference and with log?
        /// </summary>
        /// <returns></returns>
        public string CreateScript()
        {
            var contentWriter = new StringBuilder();
            bool isFirst = true;
            bool endsWithNewLine = true;
            // TODO: Create a descriptive header to script user and tool information.
            // TODO: Create a dependency check log warning for missing dependency.
            var sequence = ScriptExecutionSequence();
            foreach (var item in sequence)
            {
                if (!string.IsNullOrEmpty(item?.OriginalContent))
                {
                    if (!isFirst)
                    {
                        if (!endsWithNewLine)
                        {
                            contentWriter.AppendLine();
                        }
                        contentWriter.AppendLine("GO");
                    }
                    contentWriter.Append(item.OriginalContent);
                    if (item.IsOpenBlockComment)
                    {
                        if (!endsWithNewLine)
                        {
                            contentWriter.AppendLine();
                        }
                        contentWriter.AppendLine("*/");
                    }
                    endsWithNewLine = item.OriginalContent.EndsWith("\n");
                    if (isFirst)
                        isFirst = false;
                }
            }

            return contentWriter.ToString();
        }

        private void Log(string message, LogLevelEnum logLevel)
        {
            LogAction?.Invoke(message, logLevel);
        }

        private bool IsObjectAllreadyExistsError(ScriptSection scriptSection)
        {
            bool result = false;

            if (scriptSection != null && scriptSection.MessageLog.Any(m => m.StartsWith("ERROR: ")))
            {

                Regex regEx;
                if (string.IsNullOrEmpty(scriptSection.ObjectName))
                {
                    regEx = new Regex(@"((There is already an object named)|(The type)) '.+?' ((in the database)|(already exists))", RegexOptions.Compiled);
                    result = scriptSection.MessageLog.Any(m => regEx.IsMatch(m));
                }
                else
                {
                    // make sure it is the same object
                    regEx = new Regex(@"(?<=((There is already an object named)|(The type)) ')(.+?)(?=' ((in the database)|(already exists)))", RegexOptions.Compiled);
                    var match = scriptSection.MessageLog.Select(ml => regEx.Match(ml)).FirstOrDefault(m => m.Success);
                    if (match != null)
                    {
                        var parts = match.Value.Split('.');
                        string schema = string.Empty;
                        string dbObject = string.Empty;
                        if (parts.Length == 1)
                        {
                            dbObject = parts[0].Trim().Trim('[', ']');
                        }
                        else if (parts.Length >= 2)
                        {
                            dbObject = parts[parts.Length - 1].Trim().Trim('[', ']');
                            schema = parts[parts.Length - 2].Trim().Trim('[', ']');
                        }
                        result = string.Compare(scriptSection.ObjectName, dbObject, ignoreCase: true) == 0 
                            && (string.IsNullOrEmpty(schema) 
                                || string.Compare(scriptSection.ObjectSchema, schema, ignoreCase: true) == 0);
                    }
                }
                
            }

            return result;
        }
    }
}
