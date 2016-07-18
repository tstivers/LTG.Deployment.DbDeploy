namespace LTG.Deployment.DbDeploy.Core.Exceptions
{
    public class ScriptFolderNotFoundException : DbDeployException
    {
        public string ScriptPath { get; }

        public ScriptFolderNotFoundException(string scriptPath)
        {
            ScriptPath = scriptPath;
        }

        public override string Message => $"Could not open folder: {ScriptPath}.";
    }
}