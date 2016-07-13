using System;

namespace LTG.Deployment.DbDeploy.Core.Exceptions
{
    public class ScriptPartiallyAppliedException : Exception
    {
        public int Number { get; }

        public string Description { get; }

        public ScriptPartiallyAppliedException(int number, string description)
        {
            Number = number;
            Description = description;
        }
    }
}