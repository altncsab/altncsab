using SqlScriptRunner.ScriptHandler;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqlScriptRunner.Database
{
    internal class ApplyScript : QueryBase
    {
        private bool isDone=false;
        public override object Execute(DbContext dbContext, string cmdText, params object[] args)
        {
            if (SqlConnection == null)
            {
                SqlConnection = dbContext.Connection;                
            }
            CancellationToken? cancellationToken = null;
            if (args != null && args.Length >= 1 && args[0] is CancellationToken token) 
            {
                cancellationToken = token;
            }
            ExecutionStatusEnum resultCode;
            if (SqlConnection.State != System.Data.ConnectionState.Open)
            {
                SqlConnection.Open();
                SqlCommand = null;
            }
            if (WithTransaction && Transaction == null)
            {
                Transaction = SqlConnection.BeginTransaction();
            }
            if (SqlCommand ==  null)
            {
                SqlCommand = new SqlCommand(cmdText, SqlConnection, Transaction) 
                { 
                    CommandTimeout = 0,
                    CommandType = System.Data.CommandType.Text,                    
                };
            }
            else
            {
                SqlCommand.CommandText = cmdText;
            }
            Messages.Clear();
            try
            {
                AsyncCallback callback = new AsyncCallback(HandleCallback);
                isDone = false;
                var result = SqlCommand.BeginExecuteNonQuery(callback, SqlCommand);
                while (!isDone)
                {
                    if (cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
                    {
                        LogFunction?.Invoke($"CANCELED: User requested a cancel", Logger.LogLevelEnum.Warn);
                        SqlCommand.Cancel();
                    }
                    Thread.Sleep(100);
                }
                var effected = SqlCommand.EndExecuteNonQuery(result);
                if (Errors != null && Errors.Count() > 0)
                {
                    resultCode = ExecutionStatusEnum.Failed;
                }
                else
                {
                    resultCode = ExecutionStatusEnum.Completed;
                }
            }
            catch (Exception ex)
            {
                resultCode = ExecutionStatusEnum.Failed;
                LogFunction?.Invoke($"ERROR: {ex.Message}", Logger.LogLevelEnum.Error);
                Messages.Add(ex.Message);
            }
            if (!WithTransaction && Transaction != null)
            {
                Transaction.Commit();
                SqlConnection.Close();
            }
            
            return resultCode;
        }
        private void HandleCallback(IAsyncResult result)
        {
            isDone = result.IsCompleted;            
        }
    }
}
