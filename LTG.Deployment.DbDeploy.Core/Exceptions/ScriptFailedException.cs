using LTG.Deployment.DbDeploy.Core.Models;
using System;

namespace LTG.Deployment.DbDeploy.Core.Exceptions
{
    public class ScriptFailedException : DbDeployException
    {
        public ChangeScript ChangeScript { get; }

        public string Contents { get; }

        public ScriptFailedException(ChangeScript script, string contents, Exception innerException)
            : base(null, innerException)
        {
            ChangeScript = script;
            Contents = contents;
        }

        public override string Message => $"Failed to apply script {{{ChangeScript.Name}}}. Error was: {InnerException?.Message}";
    }
}