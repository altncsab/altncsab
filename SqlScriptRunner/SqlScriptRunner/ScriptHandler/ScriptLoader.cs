using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptRunner.ScriptHandler
{
    internal class ScriptLoader
    {
        private List<Script> scriptFileList;
        private ConcurrentBag<ScriptSection> scriptSections = new ConcurrentBag<ScriptSection>();
        private ConcurrentDictionary<string, string> scriptMap = new ConcurrentDictionary<string, string>();
        public Task LoadingTask { get; set; }
        private Action<string> LogAction;
        public ScriptLoader(IEnumerable<string> scriptFiles, Action<string> logAction)
        {
            // scriptFileList = scriptFiles;
            scriptFileList = new List<Script>();
            LogAction = logAction;
            LoadingTask = Task.Run(() =>
            {
                var result = Parallel.ForEach(scriptFiles, scriptPath =>
                {
                    scriptMap.TryAdd(scriptPath, File.ReadAllText(scriptPath));
                    Log($"File '{scriptPath}' is loaded");
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
                    Log($"File '{scriptItem.Key}' has been parsed.");

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
            // TODO: Sort based on file name first and look dependent object for sorting! Only change order based on that.
            var scriptSequence = new List<Script>();
            var resultSequnce = new List<ScriptSection>();
            var executionSequence = new List<ObjectTypeNameEnum>() 
            { 
                ObjectTypeNameEnum.Type, ObjectTypeNameEnum.Function, ObjectTypeNameEnum.Table, 
                ObjectTypeNameEnum.View, ObjectTypeNameEnum.Procedure, ObjectTypeNameEnum.Script 
            };
            executionSequence.ForEach(ot =>
                 scriptSequence.AddRange(scriptFileList
                    .Where(s => !scriptSequence.Contains(s) && s.ScriptSections.Any(ss => ss.ObjectTypeName == ot))
                    .OrderBy(f => f.FilePath))
                );
            scriptSequence.ForEach(s => resultSequnce.AddRange(s.ScriptSections));
            return resultSequnce;
        }

        public string CreateScript()
        {
            var contentWriter = new StringBuilder();
            bool isFirst = true;
            bool endsWithNewLine = true;
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

        private void Log(string message)
        {
            LogAction?.Invoke(message);
        }
    }
}
