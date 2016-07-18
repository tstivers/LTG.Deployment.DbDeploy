using JetBrains.Annotations;
using log4net;
using LTG.Deployment.DbDeploy.Core.Exceptions;
using LTG.Deployment.DbDeploy.Core.Helpers;
using LTG.Deployment.DbDeploy.Core.Models;
using LTG.Deployment.DbDeploy.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace LTG.Deployment.DbDeploy.Core
{
    public interface IDbDeployer
    {
        void Execute();
    }

    public class DbDeployer : IDbDeployer
    {
        [UsedImplicitly]
        private static ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ITargetDbRepository TargetDbRepository { get; }

        public IChangeScriptRepository ChangeScriptRepository { get; }

        public IScriptProcessor ScriptProcessor { get; }

        public bool IsPreDeployment { get; }

        public DbDeployer(ITargetDbRepository targetDbRepository, IChangeScriptRepository changeScriptRepository, IScriptProcessor scriptProcessor, bool isPreDeployment = false)
        {
            TargetDbRepository = targetDbRepository;
            ChangeScriptRepository = changeScriptRepository;
            ScriptProcessor = scriptProcessor;
            IsPreDeployment = isPreDeployment;
        }

        public void Execute()
        {
            _logger.Info("Initializing target database.");
            TargetDbRepository.InitializeTargetDb();
            var scriptsToApply = GenerateScriptList();
            if (scriptsToApply.Any())
            {
                ApplyScripts(scriptsToApply);
            }
            else
            {
                _logger.Info("No scripts need to be applied.");
            }
        }

        public IList<ChangeScript> GenerateScriptList()
        {
            var appliedScripts = TargetDbRepository.GetChangelogs().ToDictionary(x => x.Number);

            var partiallyApplied = appliedScripts.Values.Where(x => x.AppliedEnd == null).ToList();
            if (partiallyApplied.Any())
            {
                var applied = partiallyApplied.First();
                throw new ScriptPartiallyAppliedException(applied.Name);
            }

            if (appliedScripts.Any())
                _logger.Info($"Target database has {appliedScripts.Count} applied scripts. Max applied number is {appliedScripts.Max(x => x.Value.Number)}.");

            var existingScripts = ChangeScriptRepository.GetChangeScripts().ToList();

            if (existingScripts.Any())
                _logger.Info($"Loaded {existingScripts.Count} pending scripts. Max pending number is {existingScripts.Max(x => x.Number)}.");

            var duplicates = existingScripts.GroupBy(x => x.Number).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            if (duplicates.Any())
            {
                throw new DuplicateChangeNumberException(duplicates);
            }

            var scriptsToApply = new List<ChangeScript>();

            foreach (var script in existingScripts)
            {
                Changelog applied;
                if (!appliedScripts.TryGetValue(script.Number, out applied))
                {
                    if (IsPreDeployment && !script.IsPre)
                        continue;

                    _logger.Info($"Script {{{script.Name}}} needs to be applied.");
                    scriptsToApply.Add(script);
                }
                else
                {
                    if (applied.Md5 != script.Md5)
                    {
                        _logger.Warn($"Script {{{script.Name}}} has been changed since it was applied. New MD5 is {script.Md5}.");
                    }
                }
            }

            return scriptsToApply;
        }

        public void ApplyScripts(IList<ChangeScript> scripts)
        {
            _logger.Info($"Applying {scripts.Count} pending script{(scripts.Count > 1 ? "s" : "")}.");
            foreach (var script in scripts)
            {
                ApplyScript(script);
            }
            _logger.Info("All scripts applied.");
        }

        public void ApplyScript(ChangeScript script)
        {
            var logEntry = new Changelog
            {
                Number = script.Number,
                Description = script.Description,
                Md5 = script.Md5,
                AppliedStart = DateTimeOffset.Now
            };

            _logger.Info($"Applying script {{{script.Name}}}.");

            // create changelog entry
            logEntry = TargetDbRepository.CreateChangelog(logEntry);

            // execute sql
            var contents = ChangeScriptRepository.GetScriptContents(script);
            var sql = ScriptProcessor.ProcessScript(contents);

            try
            {
                TargetDbRepository.ExecuteScript(sql);
            }
            catch (SqlException sqlEx)
            {
                throw new ScriptFailedException(script, sql, sqlEx);
            }

            // update changelog entry finish time
            logEntry.AppliedEnd = DateTimeOffset.Now;
            TargetDbRepository.UpdateChangelog(logEntry);

            _logger.Info($"Finished script {{{script.Name}}}.");
        }
    }
}