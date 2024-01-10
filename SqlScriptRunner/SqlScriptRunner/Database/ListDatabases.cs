using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptRunner.Database
{
    internal class ListDatabases : QueryBase
    {

        private static string commandTemplate =
            "select Name from sys.databases where state = 0 and not name in ('tempdb','model','msdb')";
        
        public override object Execute(DbContext dbContext, string cmdText, params object[] args)
        {
            string commandText;

            commandText = commandTemplate;

            var conn = dbContext.Connection;
            DataTable dt = new DataTable();
            using (var cmd = new SqlCommand(commandText, conn))
            {
                conn.Open();
                var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                
                dt.Load(reader);                
            }
            base.ResultTable = dt;
            return dt.Rows.Cast<DataRow>().Select(dr => dr["name"].ToString()).ToArray();
        }
    }
}
