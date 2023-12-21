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
        private IEnumerable<string> scriptFileList;
        private ConcurrentBag<ScriptSection> scriptSections = new ConcurrentBag<ScriptSection>();
        private ConcurrentDictionary<string, string> scriptMap = new ConcurrentDictionary<string, string>();
        public Task LoadingTask { get; set; }
        private Action<string> LogAction;
        public ScriptLoader(IEnumerable<string> scriptFiles, Action<string> logAction)
        { 
            scriptFileList = scriptFiles;
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

        public async void ProcessScripts()
        {
            await Task.Run(() => 
            {
                return Parallel.ForEach(scriptMap, scriptItem => 
                {
                    var scriptSectionRange = ScriptSection.ParseSections(scriptItem.Key, scriptItem.Value);
                    scriptSectionRange.ForEach(scriptSections.Add);
                    Log($"File '{scriptItem.Key}' has been parsed.");

                });
            });
        }
        
        private void Log(string message)
        {
            LogAction?.Invoke(message);
        }
    }
}
