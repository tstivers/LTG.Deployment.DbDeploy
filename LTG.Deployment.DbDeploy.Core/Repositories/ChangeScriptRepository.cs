using JetBrains.Annotations;
using log4net;
using LTG.Deployment.DbDeploy.Core.Exceptions;
using LTG.Deployment.DbDeploy.Core.Helpers;
using LTG.Deployment.DbDeploy.Core.Models;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LTG.Deployment.DbDeploy.Core.Repositories
{
    public interface IChangeScriptRepository
    {
        IEnumerable<ChangeScript> GetChangeScripts();

        string GetScriptContents(ChangeScript script);
    }

    public class ChangeScriptRepository : IChangeScriptRepository
    {
        [UsedImplicitly]
        private static ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string ScriptPath { get; }

        public const string ScriptNameRegex = @"^(?<number>\d+)(?<pre>pre)?[\s\-]*(?<description>.*)\.sql$";

        public IFileSystem FileSystem { get; }

        public IMd5Service Md5Service { get; }

        public ChangeScriptRepository(string scriptPath, IMd5Service md5Service, IFileSystem fileSystem)
        {
            ScriptPath = scriptPath;
            FileSystem = fileSystem;
            Md5Service = md5Service;
        }

        public IEnumerable<ChangeScript> GetChangeScripts()
        {
            if (!FileSystem.Directory.Exists(ScriptPath))
                throw new ScriptFolderNotFoundException(ScriptPath);

            var changeScripts = new List<ChangeScript>();

            foreach (var filePath in FileSystem.Directory.EnumerateFiles(ScriptPath))
            {
                var fileInfo = FileSystem.FileInfo.FromFileName(filePath);
                var filename = fileInfo.Name;

                var match = Regex.Match(filename, ScriptNameRegex);

                if (!match.Success)
                {
                    _logger.Warn($"Skipping file {filename} as it does not appear to be a change script.");
                    continue;
                }

                var script = new ChangeScript
                {
                    Path = filePath,
                    Filename = filename,
                    Number = int.Parse(match.Groups["number"].Value),
                    Description = match.Groups["description"].Value,
                    IsPre = match.Groups["pre"].Success,
                    Md5 = Md5Service.CalculateMd5Hash(filePath)
                };

                changeScripts.Add(script);
            }

            return changeScripts.OrderBy(x => x.Number);
        }

        public string GetScriptContents(ChangeScript script)
        {
            return FileSystem.File.ReadAllText(script.Path);
        }
    }
}