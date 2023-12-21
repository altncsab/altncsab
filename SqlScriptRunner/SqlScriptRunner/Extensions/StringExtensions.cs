using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptRunner.Extensions
{
    internal static class StringExtensions
    {
        internal static string CleanComment(this string str)
        {
            var result =  new StringWriter();
            bool isComment = false;
            bool isLineComment = false;
            var commentPair = new string[] { "/*", "*/" };
            var lineCommentPair = new string[] { "--", "\n"};

            if (str != null)
            {
                // looking for two characters always
                for(int i = 0; i < str.Length; i++)
                {
                    var fregment = str.Substring(i, Math.Min(str.Length - i, 2));
                    if (isComment)
                    {
                        if (fregment.StartsWith(commentPair[1]))
                        {
                            isComment = false;
                            i += 2;
                            continue;
                        }
                    }
                    else
                    {
                        if (fregment.StartsWith(commentPair[0]))
                        {
                            isComment = true;
                            i += 2; 
                            continue;
                        }
                    }
                    if (isLineComment)
                    {
                        if (fregment.StartsWith(lineCommentPair[1]))
                        {
                            isLineComment = false;
                            i += 2;
                            continue;
                        }
                    }
                    else
                    {
                        if (fregment.StartsWith(lineCommentPair[0]))
                        {
                            isLineComment = true;
                            i += 2;
                            continue;
                        }
                    }
                    if (!(isComment || isLineComment))
                    {
                        result.Write(str[i]);
                    }
                }
            }

            return result.ToString();
        }
    }
}
