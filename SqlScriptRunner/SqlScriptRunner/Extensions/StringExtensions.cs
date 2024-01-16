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
        /// <summary>
        /// Removes SQL commented text from the input string
        /// </summary>
        /// <param name="str">The SQL script to clean</param>
        /// <returns></returns>
        internal static string CleanComment(this string str)
        {
            var result =  new StringWriter();
            bool isComment = false;
            bool isLineComment = false;
            var commentPair = new string[] { "/*", "*/" };
            var lineCommentPair = new string[] { "--", "\n", "\r\n"};

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
                            i += 1;
                            continue;
                        }
                    }
                    else
                    {
                        if (fregment.StartsWith(commentPair[0]))
                        {
                            isComment = true;
                            i += 1; 
                            continue;
                        }
                    }
                    if (isLineComment)
                    {
                        if (fregment.StartsWith(lineCommentPair[2]))
                        {
                            isLineComment = false;
                            i += 1;
                            continue;
                        }
                        else if (fregment.StartsWith(lineCommentPair[1]))
                        {
                            isLineComment = false;
                            continue;
                        }
                    }
                    else
                    {
                        if (fregment.StartsWith(lineCommentPair[0]))
                        {
                            isLineComment = true;
                            i += 1;
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
        /// <summary>
        /// It removes the starting and ending new line: \r \n and space
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static string TrimNewLine(this string str)
        {
            return str.TrimRightNewLine().TrimLeftNewLine();
        }
        /// <summary>
        /// Removes new line characters and space from the end of the string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static string TrimRightNewLine(this string str)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(str))
            {
                int i = str.Length - 1;
                for (; i >= 0; i--)
                {
                    if (!(str[i] == '\n' || str[i] == '\r' || str[i] == ' ')) break;
                }
                result = str.Substring(0, i + 1);
            }
            return result;
        }
        /// <summary>
        /// Remove new line characters and space from the beginning of the string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static string TrimLeftNewLine(this string str)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(str))
            {
                int i = 0;
                for (; i < str.Length; i++)
                {
                    if (!(str[i] == '\n' || str[i] == '\r' || str[i] == ' ')) break;
                }
                result = str.Substring(i);
            }
            return result;
        }
        /// <summary>
        /// It returns the content inside a bracket. It can handle multiple citation marks.
        /// Default brackets are opening bracket "(" and closing bracket ")".
        /// Default citation mark is single apostrophe: '.
        /// </summary>
        /// <param name="str">the source string to look for the brackets</param>
        /// <returns>inside content of the bracketed string</returns>
        internal static string FirstBracketContent(this string str)
        {
            return FirstBracketContent(str,'(', ')', '\'');
        }
        /// <summary>
        /// handle multiple citation marks to generalize this function for the generic use. 
        /// </summary>
        /// <param name="str">the source string to look for the brackets</param>
        /// <param name="openBracket">Opening bracket character. Default value: "("</param>
        /// <param name="closeBracket">Closing bracket character. Default value: ")"</param>
        /// <param name="citationMarks"></param>
        /// <returns></returns>
        // 
        internal static string FirstBracketContent(this string str, char openBracket = '(', char closeBracket = ')', params char[] citationMarks)
        {
            if (str == null) return null;
            if (str.IndexOf(openBracket) == -1 || str.IndexOf(closeBracket) == -1) return string.Empty;
            var result = new StringBuilder();
            var bracketLevel = 0;
            Dictionary<char, bool> citationHit = null;
            if (citationMarks != null)
            {
                citationHit = citationMarks.ToDictionary(i => i, i => false);
            }
            for(int i = 0; i < str.Length; i++)
            {
                var charHit = str[i];
                if (citationHit?.ContainsKey(charHit) ?? false)
                {
                    // only one citation mark counting: the firs one... the rest must be ignored after one gets active
                    if (!(citationHit?.Any(hit => hit.Value == true && hit.Key != charHit) ?? false))
                    {
                        citationHit[charHit] = !citationHit[charHit];
                    }
                }
                if (charHit == openBracket)
                {
                    if (!(citationHit?.Values.Any(hit => hit == true) ?? false))
                    {
                        bracketLevel++;
                        if (bracketLevel == 1) continue;
                    }
                }
                else if(charHit == closeBracket)
                {
                    if (!(citationHit?.Values.Any(hit => hit == true) ?? false))
                    {
                        bracketLevel--;
                    }
                    // when the level 0 hit again we are done.
                    if (bracketLevel == 0) break;
                }
                if (bracketLevel >= 1)
                {
                    result.Append(str[i]);
                }
            }            
            return result.ToString();
        }
    }
}
