using Dapper;
using LTG.Deployment.DbDeploy.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace LTG.Deployment.DbDeploy.DataAccess.Repositories
{
    public class ChangelogRepository : IDisposable
    {
        public IDbConnection Connection { get; set; }

        private IDbTransaction _readLockTransaction;

        public IDbTransaction ReadLockTransaction => _readLockTransaction ??
                                                     (_readLockTransaction = Connection.BeginTransaction(IsolationLevel.RepeatableRead));

        public string ChangelogTableName { get; set; } = "[dbo].[Changelog]";

        public IEnumerable<Changelog> GetChangelogs()
        {
            return Connection.Query<Changelog>($@"
                SELECT [Id]
                    ,[Name]
                    ,[Description]
                    ,[AppliedStart]
                    ,[AppliedEnd]
                    ,[Md5]
                FROM {ChangelogTableName}
            ", transaction: ReadLockTransaction);
        }

        public void UpdateChangelog(Changelog changelog)
        {
            Connection.Execute($@"
                UPDATE {ChangelogTableName}
                SET [AppliedEnd] = @AppliedEnd
                WHERE [Id] = @Id
            ", changelog);
        }

        public void CreateChangelog(Changelog changelog)
        {
            var id = Connection.Query<int>($@"
                INSERT INTO {ChangelogTableName} (
                    [Name]
                    ,[Description]
                    ,[AppliedStart]
                    ,[Md5]
                    )
                VALUES (
                    @Name
                    ,@Description
                    ,@AppliedStart
                    ,@Md5
                    );
                SELECT CAST(SCOPE_IDENTITY() as int);
            ", changelog).Single();

            changelog.Id = id;
        }

        public void Dispose()
        {
            _readLockTransaction?.Rollback();
        }
    }
}