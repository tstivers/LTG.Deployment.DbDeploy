using System.Collections.Generic;

namespace LTG.Deployment.DbDeploy.Core.Exceptions
{
    public class DuplicateChangeNumberException : DbDeployException
    {
        public IEnumerable<int> DuplicateScriptNumbers { get; }

        public DuplicateChangeNumberException(IEnumerable<int> numbers)
        {
            DuplicateScriptNumbers = numbers;
        }

        public override string Message
            => $"Found duplicate script numbers in changescripts folder: {string.Join(", ", DuplicateScriptNumbers)}";
    }
}