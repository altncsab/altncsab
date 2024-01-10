using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptRunner.Database
{
    internal interface IQueryBase
    {
        DataTable ResultTable { get; }
        
        int ReturnCode { get; }

        string[] Messages { get; }

        Action<string> LogFunction { get; set; }

        object Execute(DbContext dbContext, string cmdText, params object[] args);

        object Execute(DbContext dbContext, params object[] args);

        object Execute(DbContext dbContext);
    }
}
