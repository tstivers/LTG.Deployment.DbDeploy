namespace LTG.Deployment.DbDeploy.Core.Exceptions
{
    public class ScriptPartiallyAppliedException : DbDeployException
    {
        public string Name { get; }

        public ScriptPartiallyAppliedException(string name)
        {
            Name = name;
        }

        public override string Message => $"Script {{{Name}}} was partially applied in a previous execution. Please fix the script and clean up the changelog table before attempting deployment.";
    }
}