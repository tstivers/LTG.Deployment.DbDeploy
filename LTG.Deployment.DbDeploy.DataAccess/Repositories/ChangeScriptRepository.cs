using JetBrains.Annotations;
using log4net;
using LTG.Deployment.DbDeploy.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace LTG.Deployment.DbDeploy.DataAccess.Repositories
{
    public class ChangeScriptRepository
    {
        [UsedImplicitly]
        private static ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string ScriptPath { get; }

        public const string ScriptNameRegex = @"^(?<number>\d+)(?<pre>pre)?[\s\-]*(?<description>.*)\.sql$";

        public IFileSystem FileSystem { get; }

        public ChangeScriptRepository(string scriptPath, IFileSystem fileSystem = null)
        {
            ScriptPath = scriptPath;
            FileSystem = fileSystem ?? new FileSystem();
        }

        public IEnumerable<ChangeScript> GetChangeScripts()
        {
            if (!FileSystem.Directory.Exists(ScriptPath))
                throw new ArgumentException($"Cannot open directory {ScriptPath}");

            var changeScripts = new List<ChangeScript>();

            foreach (var filePath in FileSystem.Directory.EnumerateFiles(ScriptPath))
            {
                var fileInfo = FileSystem.FileInfo.FromFileName(filePath);
                var filename = fileInfo.Name;

                var match = Regex.Match(filename, ScriptNameRegex);

                if (!match.Success)
                {
                    _logger.Warn($"Skipping file {filename} as it does not appear to be a change script");
                    continue;
                }

                var script = new ChangeScript();
                script.Filename = filename;

                script.Number = int.Parse(match.Groups["number"].Value);
                script.Description = match.Groups["description"].Value;
                script.IsPre = match.Groups["pre"].Success;

                changeScripts.Add(script);
            }

            return changeScripts;
        }

        public string CalculateMd5Hash(string input)
        {
            // step 1, calculate MD5 hash from input

            MD5 md5 = MD5.Create();

            byte[] inputBytes = Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)

            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }
    }
}