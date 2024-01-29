using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SqlScriptRunner.Extensions;

namespace SqlScriptRunner.ScriptHandler
{
    internal class Script
    {
        public string FilePath { get; set; }
        
        public string Name => Path.GetFileName(FilePath);

        private Guid guid = Guid.NewGuid();
        
        public Guid Guid => guid;
        
        public string OriginalContent { get; private set; }

        public string Content {  get; private set; }
        
        public string Status { get; set; }
        
        public IList<ScriptSection> ScriptSections { get; private set; }

        public Script(string filePath, IList<ScriptSection> scriptSections)
        {
            FilePath = filePath;
            ScriptSections = scriptSections;
            foreach (var item in ScriptSections)
            {
                item.Script = this;
            }
        }
    }
}
