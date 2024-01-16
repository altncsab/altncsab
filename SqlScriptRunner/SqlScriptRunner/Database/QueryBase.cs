using SqlScriptRunner.Logger;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqlScriptRunner.Database
{
    internal abstract class QueryBase : IQueryBase
    {
        internal protected DataTable resultTable = null;
        public QueryBase()
        {
            Messages = new List<string>();
            Errors = null;
        }
        public virtual DataTable ResultTable 
        { 
            get => resultTable; 
            protected private set => resultTable = value; 
        } 

        public virtual int ReturnCode 
        { 
            get; 
            protected private set; 
        }
        public virtual IEnumerable<SqlError> Errors { get; protected private set; }

        public virtual List<string> Messages
        {
            get;
            protected private set;
        }
        public virtual SqlCommand SqlCommand { get; protected set; }
        public virtual bool WithTransaction {  get; set; }

        public virtual SqlTransaction Transaction { get; set; }

        private SqlConnection _SqlConnection;
        public virtual SqlConnection SqlConnection 
        { 
            get => _SqlConnection; 
            set 
            { 
                if (_SqlConnection == null)
                {
                    value.InfoMessage += SqlConnection_InfoMessage;
                }
                else if (_SqlConnection != value)
                {
                    _SqlConnection.InfoMessage -= SqlConnection_InfoMessage;
                    value.InfoMessage += SqlConnection_InfoMessage;
                }
                value.FireInfoMessageEventOnUserErrors = true;
                _SqlConnection = value;
            } 
        }

        public virtual Action<string, LogLevelEnum?> LogFunction
        {
            get;
            set;
        }

        public virtual object Execute(DbContext dbContext)
        {
            return Execute(dbContext, string.Empty);
        }
        public virtual object Execute(DbContext dbContext, params object[] args)
        {
            return Execute(dbContext, cmdText: "", args: args);
        }
        public abstract object Execute(DbContext dbContext, string cmdText, params object[] args);

        // This is working only with BeginExecuteNonQuery - EndExecuteNonQuery! No exception is fired!
        internal virtual void SqlConnection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            Messages.Add(e.Message);
            Errors = e.Errors.Cast<SqlError>().Where(se => se.Number != 0);
            if (Errors != null && Errors.Count() > 0)
            {
                Errors.ToList().ForEach(err => LogFunction?.Invoke($"ERROR: {err.Message}", LogLevelEnum.Error));
            }
            else
            {
                LogFunction?.Invoke($"INFO: {e.Message}", LogLevelEnum.Info);
            }
        }
        public virtual void RollBackTransaction()
        {
            // roll back all
            if (Transaction?.Connection != null)
            {
                Transaction?.Rollback();
            }
            SqlConnection?.Close();
        }
        public virtual void CommitTransaction() 
        {
            if (Transaction?.Connection != null)
            {
                Transaction?.Commit();
            }
            SqlConnection?.Close();
        }
    }
}
