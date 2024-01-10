using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptRunner.Database
{
    internal abstract class QueryBase : IQueryBase
    {
        internal protected DataTable resultTable = null;

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

        public virtual string[] Messages
        {
            get;
            protected private set;
        }

        public virtual Action<string> LogFunction
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
            return Execute(dbContext, cmdText: null, args: args);
        }
        public abstract object Execute(DbContext dbContext, string cmdText, params object[] args);
    }
}
