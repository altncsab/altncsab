using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqlScriptRunner.ScriptHandler
{
    internal enum ScriptCommandEnum
    {
        Nothing,
        Start,
        Cancel,
        Transaction,
        SetSkip,
        SkipIfExists,
    }
}
