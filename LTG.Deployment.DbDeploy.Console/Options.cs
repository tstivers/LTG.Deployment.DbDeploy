using CommandLine;
using CommandLine.Text;

namespace LTG.Deployment.DbDeploy.Console
{
    public class Options
    {
        [Option('c', "connection-string", Required = true, HelpText = "Connection string for target database.")]
        public string ConnectionString { get; set; }

        [Option('s', "scripts-folder", DefaultValue = "Scripts", HelpText = "Path to the scripts folder.")]
        public string ScriptFolderPath { get; set; }

        [Option('p', "pre-scripts", HelpText = "Execute pre-deployment scripts only.")]
        public bool PreRun { get; set; }

        [Option('e', "environment", DefaultValue = "null", HelpText = "Environment code to use for environment-dependant scripts.")]
        public string Environment { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}