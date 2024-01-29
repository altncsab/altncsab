using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptRunner.ScriptHandler
{
    internal enum ExecutionStatusEnum
    {
        Running,
        Completed,
        Failed,
        Canceled,
        Skipped,
    }
}
