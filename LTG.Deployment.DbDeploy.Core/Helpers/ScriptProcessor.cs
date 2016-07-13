using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LTG.Deployment.DbDeploy.Core.Helpers
{
    public class ScriptProcessor
    {
        public const string EnvironmentRegex = @"^\s*--\s*@ENV (\S+)\s*$";

        public string ProcessScript(string contents, string env)
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
                        if (currentEnv == "*" || currentEnv == env)
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