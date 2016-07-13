using LTG.Deployment.DbDeploy.Core.Models;
using System;

namespace LTG.Deployment.DbDeploy.Core.Exceptions
{
    public class ScriptFailedException : DbDeployException
    {
        public ChangeScript ChangeScript { get; }

        public string Contents { get; }

        public ScriptFailedException(ChangeScript script, string contents, Exception ex)
            : base("Script failed to apply", ex)
        {
            ChangeScript = script;
            Contents = contents;
        }
    }
}