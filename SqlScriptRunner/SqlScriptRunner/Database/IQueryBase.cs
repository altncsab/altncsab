using SqlScriptRunner.Logger;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptRunner.Database
{
    internal interface IQueryBase
    {
        DataTable ResultTable { get; }
        
        int ReturnCode { get; }

        List<string> Messages { get; }

        Action<string, LogLevelEnum?> LogFunction { get; set; }

        bool WithTransaction { get; set; }

        SqlTransaction Transaction { get; set; }

        SqlConnection SqlConnection { get; set; }

        object Execute(DbContext dbContext, string cmdText, params object[] args);

        object Execute(DbContext dbContext, params object[] args);

        object Execute(DbContext dbContext);
    }
}
