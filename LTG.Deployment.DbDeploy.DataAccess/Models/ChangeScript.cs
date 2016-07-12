namespace LTG.Deployment.DbDeploy.DataAccess.Models
{
    public class ChangeScript
    {
        public string Filename { get; set; }

        public int Number { get; set; }

        public string Description { get; set; }

        public string Md5 { get; set; }

        public bool IsPre { get; set; }
    }
}