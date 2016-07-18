using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LTG.Deployment.DbDeploy.Core.Helpers
{
    public interface IScriptProcessor
    {
        string ProcessScript(string contents);
    }

    public class ScriptProcessor : IScriptProcessor
    {
        public const string EnvironmentRegex = @"^\s*--\s*@ENV (\S+)\s*$";

        public string Environment { get; }

        public ScriptProcessor(string environment)
        {
            Environment = environment;
        }

        public string ProcessScript(string contents)
        {
            var sb = new StringBuilder();
            var currentEnv = "*";

            using (var reader = new StringReader(contents))
            {
                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    Debug.Assert(line != null, "line != null");

                    var match = Regex.Match(line, EnvironmentRegex);

                    if (match.Success)
                    {
                        currentEnv = match.Groups[1].Value.ToUpper();
                    }
                    else
                    {
                        if (currentEnv == "*" || currentEnv == Environment)
                        {
                            sb.AppendLine(line);
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }
}