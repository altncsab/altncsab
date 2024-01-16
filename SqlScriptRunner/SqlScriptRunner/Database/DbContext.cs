using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptRunner.Database
{
    public class DbContext
    {
        private SqlConnectionStringBuilder connectionStringBuilder;

        internal SqlConnectionStringBuilder ConnectionStringBuilder => connectionStringBuilder;
        public SqlConnection Connection => new SqlConnection(connectionStringBuilder.ConnectionString);

        public DbContext() : this("localhost", "master")
        {              
        }
        public DbContext(string server, string database) : this(server, isTrusted: true, database, userName: "", passWord: "")
        {
        }
        public DbContext(string server, bool isTrusted, string database, string userName, string passWord)
        {
            connectionStringBuilder = new SqlConnectionStringBuilder()
            {
                DataSource = server,
                InitialCatalog = database,
                IntegratedSecurity = isTrusted,
                UserID = userName,
                Password = passWord,
                PersistSecurityInfo = true
            };
        }
    }
}
