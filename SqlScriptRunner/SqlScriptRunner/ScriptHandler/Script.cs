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

        public string OriginalContent { get; private set; }

        public string Content {  get; private set; }
        public IList<ScriptSection> ScriptSections { get; private set; }

        public Script(string filePath, string originalContent)
        {
            FilePath = filePath;
            OriginalContent = originalContent;
            Content = originalContent.CleanComment();
        }

        private void CleanScript()
        {
            // TODO: remove comments and slice the content over GO
            using (var sw = new StringWriter())
            {

            }
        }
    }
}
