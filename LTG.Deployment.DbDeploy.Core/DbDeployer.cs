using JetBrains.Annotations;
using log4net;
using LTG.Deployment.DbDeploy.DataAccess.Repositories;
using System;
using System.Reflection;

namespace LTG.Deployment.DbDeploy.Core
{
    public class DbDeployer : IDisposable
    {
        [UsedImplicitly]
        private static ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string[] Environments { get; set; }

        public ChangelogRepository ChangelogRepository { get; private set; }

        public DbDeployer(ChangelogRepository changelogRepository)
        {
            ChangelogRepository = changelogRepository;
        }

        public void Execute()
        {
            // connect to database
            // lock changelog table

            // generate script list
            // print scripts to be applied
            // apply scripts
            // unlock changelog table
        }

        public void GenerateScriptList()
        {
            // load scripts from database
            // load scripts from disk
            // figure out which scripts should be applied
        }

        public void ApplyScripts()
        {
            // for each script to be applied
            // apply it
        }

        public void ApplyScript()
        {
            // update changelog entry start time
            // begin transaction
            // execute sql
            // complete/rollback transaction
            // update changelog entry finish time
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}