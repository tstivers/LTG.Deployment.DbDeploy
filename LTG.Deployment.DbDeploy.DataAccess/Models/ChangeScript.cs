﻿namespace LTG.Deployment.DbDeploy.DataAccess.Models
{
    public class ChangeScript
    {
        public string Path { get; set; }

        public string Name => $"{Number}{(IsPre ? "pre" : "")} - {Description}";

        public int Number { get; set; }

        public string Description { get; set; }

        public string Md5 { get; set; }

        public bool IsPre { get; set; }
    }
}