using SqlScriptRunner.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlScriptRunner.ScriptHandler
{
    internal class ScriptSection
    {
        public Script Script { get; set; }
        public string OriginalContent { get; private set; }
        public string Content { get; private set; }
        public string FilePath { get; private set; }
        public int SectionId { get; private set; }
        public int SectionOffset { get; private set; }
        public ExecutionStatusEnum? Status { get; set; }
        public List<string> MessageLog { get; set; }
        public string ObjectSchema { get; private set; }
        public string ObjectName { get; private set; }
        public ObjectTypeNameEnum ObjectTypeName { get; private set; }

        public bool IsOpenBlockComment { get; set; }

        public List<string> DependentObjectNames { get; private set; }
        public bool ToBeSkipped { get; set; }

        public ScriptSection()
        {
            MessageLog = new List<string>();
        }
        public ScriptSection(string filePath, string originalContent) : this()
        {
            FilePath = filePath;
            OriginalContent = originalContent;
            Content = originalContent.CleanComment();            
            ProcessScript();
        }
        public ScriptSection(string filePath, string originalContent, int sectionId, int sectionOffset): this()
        {
            FilePath = filePath;
            OriginalContent = originalContent;
            Content = originalContent.CleanComment();
            SectionId = sectionId;
            SectionOffset = sectionOffset;
            ProcessScript();
        }

        // The procedure is slicing the incoming file content over GO. Make Sure it is not commented out.
        public static List<ScriptSection> ParseSections(string filePath, string originalContent)
        {
            bool isBlockComment = false;
            bool isLineComment = false;
            var scriptSections = new List<ScriptSection>();
            var scriptSection = new StringWriter();
            int sectionCount = 0;
            int sectionStart = 0;
            int contentLenght = originalContent.Length;
            for (int i = 0; i < contentLenght; i++)
            {
                string fragment = originalContent.Substring(i, Math.Min(contentLenght-i, 2));
                if (fragment == "/*" && !isLineComment) isBlockComment = true;                
                if (fragment == "*/" && !isLineComment && isBlockComment) isBlockComment = false;
                if (fragment == "--" && !isBlockComment) isLineComment = true;
                if (fragment.StartsWith("\n") && !isBlockComment && isLineComment) isLineComment = false;
                if (string.Compare(fragment ,"GO", ignoreCase:true) == 0 && !(isBlockComment || isLineComment))
                {
                    // GO must be surrounded with white space or new line.
                    var startIndex = Math.Max(0, i-2);
                    var endIndex = Math.Min(i + 2, contentLenght - 1);
                    var fragmentGo = originalContent.Substring(startIndex, endIndex - startIndex);
                    if (Regex.IsMatch(fragmentGo, @"(?:\W|^)(?<!\[)GO(?!])(?:\W|$)", RegexOptions.IgnoreCase))
                    {
                        scriptSections.Add(new ScriptSection(filePath, scriptSection.ToString().TrimNewLine(), sectionCount, sectionStart) { IsOpenBlockComment = isBlockComment});
                        sectionCount++;                       
                        scriptSection = new StringWriter();
                        i++;
                        sectionStart = i;
                        continue;
                    }
                }
                scriptSection.Write(originalContent[i]);
            }
            var lastBlock = scriptSection.ToString();
            if (!string.IsNullOrWhiteSpace(lastBlock))
            {
                scriptSections.Add(new ScriptSection(filePath, lastBlock, sectionCount, sectionStart) { IsOpenBlockComment = isBlockComment });
            }
            return scriptSections;
        }

        // TODO: find out if the script is a creation / alter script and what type. If it has a dependency.
        private void ProcessScript()
        {
            // Create Table script but not temporary!
            if (Regex.IsMatch(this.Content, @"(?:\s|^)(?:CREATE|ALTER)\s+TABLE\s+\[?[\w\.\[\]]+?\s?\("
                , RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                // this section contains a Table creation alteration script
                ProcessTable();
            }
            else if (Regex.IsMatch(this.Content, @"(?:\s|^)(?:CREATE|ALTER)\s+VIEW\s+\[?[\w\.\[\]]+?\s"
                , RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                ProcessView();
            }
            else if (Regex.IsMatch(this.Content, @"(?:\s|^)(?:CREATE|ALTER)\s+PROC(EDURE)?\s+\[?[\w\.\[\]]+?\s"
                , RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                // this script contains a stored procedure definition.
                ProcessProcedure();
            }
            else if (Regex.IsMatch(this.Content, @"(?:\s|^)CREATE\s+TYPE\s+\[?[\w\.\[\]]+?\s"
                , RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                // this script contains a type definition.
                ProcessType();
            }
            else if (Regex.IsMatch(this.Content, @"(?:\s|^)(?:CREATE|ALTER)\s+FUNCTION\s+\[?[\w\.\[\]]+?[\s\(]"
                , RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                // this is a function definition
                ProcessFunction();
            }
            else
            {
                // it is a generic script. It could have reference to types, tables, procedures and functions.
                ProcessScriptSection();
            }
        }
        private void ProcessScriptSection()
        {
            // TODO: Check for parameter declarations using user type
            // TODO: Check table references
            // TODO: Check SP/Function calls
        }
        private void ProcessFunction()
        {
            var match = Regex.Match(this.Content, @"(?<=(?:CREATE|ALTER)\s+FUNCTION\s+)[\w\.\[\]]+?(?=[\s\(]+?)"
                , RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                SetProcessedProperties(match, ObjectTypeNameEnum.Function);
            }
            // look for parameter definitions.
            // TODO: Check the bracket matching... we need only the first closing bracket. I probably need to give up regex hire.
            match = Regex.Match(this.Content, @"(?<=(?:CREATE|ALTER)\s+FUNCTION\s+[\w\.\[\]]+?(?=[\s\(]+?)).+(?=\))(?!\()"
                , RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                ProcessParameters(match, true);
            }
        }

        private void SetProcessedProperties(Match match, ObjectTypeNameEnum typeName)
        {
            var nameElements = match?.Value.Replace("[", "").Replace("]", "").Split('.');
            var objectName = nameElements?.LastOrDefault();
            this.ObjectName = objectName.Trim();
            this.ObjectTypeName = typeName;
            if (nameElements != null)
            {
                this.ObjectSchema = (nameElements.Count() <= 1 ? "dbo" : nameElements[nameElements.Count() - 2]).Trim();
            }
        }

        private void ProcessType()
        {
            var match = Regex.Match(this.Content, @"(?<=CREATE\s+TYPE\s+)[\w\.\[\]]+?(?=\s+?)"
                , RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                SetProcessedProperties(match, ObjectTypeNameEnum.Type);
            }
            // it can be a value type but the base type can be only standard SQL type
            match = Regex.Match(this.Content, @"(?<=CREATE\s+TYPE\s+[\w\.\[\]]+?\s+?AS\s+?TABLE(?:\s?\()).+(?=\))(?!\()"
                , RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                ProcessTableColumns(match);
            }
        }
        private void ProcessProcedure()
        {
            // get out the procedure name and check for the parameter types
            var match = Regex.Match(this.Content, @"(?<=(?:CREATE|ALTER)\s+PROC(EDURE)?\s+)[\w\.\[\]]+?(?=\s+?)"
                , RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                SetProcessedProperties(match, ObjectTypeNameEnum.Procedure);
            }
            
            // look for parameter definitions.
            match = Regex.Match(this.Content, @"(?<=(?:CREATE|ALTER)\s+PROC(EDURE)?\s+[\w\.\[\]]+?\s+?).+(?=\s+?AS\s+?)(?!AS)"
                , RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                ProcessParameters(match, false);
            }
        }

        private void ProcessParameters(Match match, bool isBrackets)
        {
            // parameters are in brackets
            string bracketedParams;
            if (isBrackets)
            {
                bracketedParams = match.Value.FirstBracketContent();
            }
            else
            {
                bracketedParams = match.Value;
            }
            var paramLines = Regex.Split(bracketedParams, @"(?!(?<=\(.*?[^\)]),(?=.*?\))),");
            if (paramLines.Length > 0)
            {
                foreach (string pline in paramLines)
                {
                    // consider the word AS between the parameter name and type value
                    var m = Regex.Match(pline, @"(?<name>(?<=\s|^)@\w+?(?=\s))(?:\s+?(AS\s+?)?)(?<type>((?<=\s+?)\w[\w\.]+?(?=\(|\s|$)))"
                        , RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (m.Success
                        && !string.IsNullOrWhiteSpace(m.Groups["type"].Value)
                        && !SqlTypes.IsSQLType(m.Groups["type"].Value))
                    {
                        // The type definition is a user type. Save it for look up
                        if (DependentObjectNames == null)
                        {
                            DependentObjectNames = new List<string>();
                        }
                        this.DependentObjectNames.Add($"{m.Groups["type"].Value};{m.Groups["name"].Value}");
                    }
                }
            }
        }
        private void ProcessView()
        {
            var match = Regex.Match(this.Content, @"(?<=(?:CREATE|ALTER)\s+VIEW\s+)[\w\.\[\]]+?(?=\s+?)"
                , RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                SetProcessedProperties(match, ObjectTypeNameEnum.View);
            }
        }
        private void ProcessTable()
        {
            // this is a table creation inside. Get the table name out.
            var match = Regex.Match(this.Content, @"(?<=(?:CREATE|ALTER)\s+TABLE\s+)[\w\.\[\]]+?(?=\s?\()"
                , RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                // get rid of [ ] brackets and split
                var nameElements = match.Value.Replace("[", "").Replace("]", "").Split('.');
                var objectName = nameElements.LastOrDefault();

                if (!objectName.StartsWith("#"))
                {
                    // it is not a temporary table
                    this.ObjectName = objectName.Trim();
                    this.ObjectTypeName = ObjectTypeNameEnum.Table;
                    this.ObjectSchema = (nameElements.Count() <= 1 ? "dbo" : nameElements[nameElements.Count() - 2]).Trim();
                }
            }
            // look for column definitions what are not standard types.
            match = Regex.Match(this.Content, @"(?<=(?:CREATE|ALTER)\s+TABLE\s+[\w\.\[\]]+?\s?\().+(?=\))(?!\()"
                , RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                // we got all parameter definitions inside the brackets separated with coma but not bracketed coma (1,1)
                ProcessTableColumns(match);
            }
        }

        private void ProcessTableColumns(Match match)
        {
            var paramLines = Regex.Split(match.Value, @"(?!(?<=\(.*?[^\)]),(?=.*?\))),");
            // Need to find all parameters an see if they are not among the standard one.
            if (paramLines.Length > 0)
            {
                // we got a list of parameter names and type definitions. we need only those what is not standard.
                foreach (string pline in paramLines)
                {
                    // let retrieve the name and the type definition. Name could be in [] with space
                    var m = Regex.Match(pline, @"(?<name>((?<=\s|^)\w+?(?=\s))|((?<=\s|^)\[\w[\w ]+?\](?=\s)))(?:\s+?)(?<type>((?<=\s+?)\w+(?=\(|\s|$)))"
                       , RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (m.Success
                        && !string.IsNullOrWhiteSpace(m.Groups["type"].Value)
                        && !SqlTypes.IsSQLType(m.Groups["type"].Value))
                    {
                        // The type definition is a user type. Save it for look up
                        if (DependentObjectNames == null)
                        {
                            DependentObjectNames = new List<string>();
                        }
                        this.DependentObjectNames.Add($"{m.Groups["type"].Value};{m.Groups["name"].Value}");
                    }
                }
            }
        }
    }
}
