using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlScriptRunner.ScriptHandler
{
    internal class SqlTypes
    {
        static readonly List<string> SqlTypesList = 
            Enum.GetValues(typeof(SqlDbType))
                .Cast<SqlDbType>()
                .Select(x => @"\b" + x.ToString() + @"\b")
                .ToList();

        public static bool IsSQLType(string typeName)
        {            
            return SqlTypesList.Any(p => Regex.IsMatch(typeName, p, RegexOptions.IgnoreCase));
        }
    }
}
