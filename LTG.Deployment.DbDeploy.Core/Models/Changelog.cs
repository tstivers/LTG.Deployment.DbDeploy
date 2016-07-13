using System;

namespace LTG.Deployment.DbDeploy.Core.Models
{
    public class Changelog
    {
        public int Id { get; set; }

        public int Number { get; set; }

        public string Description { get; set; }

        public DateTimeOffset AppliedStart { get; set; }

        public DateTimeOffset? AppliedEnd { get; set; }

        public string Md5 { get; set; }
    }
}