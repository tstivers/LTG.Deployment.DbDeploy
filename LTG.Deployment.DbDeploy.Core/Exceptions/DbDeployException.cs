using System;

namespace LTG.Deployment.DbDeploy.Core.Exceptions
{
    public class DbDeployException : Exception
    {
        public DbDeployException()
        {
        }

        public DbDeployException(string message) : base(message)
        {
        }

        public DbDeployException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}