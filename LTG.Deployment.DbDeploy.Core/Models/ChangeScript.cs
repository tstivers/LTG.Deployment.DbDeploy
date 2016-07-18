namespace LTG.Deployment.DbDeploy.Core.Models
{
    public class ChangeScript
    {
        public string Path { get; set; }

        public string Filename { get; set; }

        public string Name => $"{Filename}";

        public int Number { get; set; }

        public string Description { get; set; }

        public string Md5 { get; set; }

        public bool IsPre { get; set; }
    }
}