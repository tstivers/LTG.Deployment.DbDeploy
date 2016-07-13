using Dapper;
using LTG.Deployment.DbDeploy.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace LTG.Deployment.DbDeploy.DataAccess.Repositories
{
    public interface ITargetDbRepository : IDisposable
    {
        void InitializeTargetDb();

        IEnumerable<Changelog> GetChangelogs();

        Changelog CreateChangelog(Changelog changelog);

        void UpdateChangelog(Changelog changelog);

        void ExecuteScript(string sql);
    }

    public class TargetDbRepository : ITargetDbRepository
    {
        public TargetDbRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        public IDbConnection Connection { get; }

        private IDbTransaction _readLockTransaction;

        public IDbTransaction ReadLockTransaction => _readLockTransaction ??
                                                     (_readLockTransaction = Connection.BeginTransaction(IsolationLevel.RepeatableRead));

        public string ChangelogTableName { get; set; } = "[dbo].[Changelog]";

        public void InitializeTargetDb()
        {
            Connection.Execute($@"
                IF object_id(N'{ChangelogTableName}', N'U') IS NULL
                BEGIN
                    CREATE TABLE {ChangelogTableName} (
                        [Id] INT NOT NULL PRIMARY KEY IDENTITY
                        ,[Number] INT NOT NULL
                        ,[Description] VARCHAR(250) NOT NULL
                        ,[AppliedStart] DATETIME2(7) NOT NULL
                        ,[AppliedEnd] DATETIME2(7) NULL
                        ,[Md5] NVARCHAR(100) NOT NULL
                        );

                    CREATE UNIQUE INDEX [IX_Changelog] ON {ChangelogTableName} ([Number]);
                END
            ");
        }

        public IEnumerable<Changelog> GetChangelogs()
        {
            return Connection.Query<Changelog>($@"
                SELECT [Id]
                    ,[Number]
                    ,[Description]
                    ,[AppliedStart]
                    ,[AppliedEnd]
                    ,[Md5]
                FROM {ChangelogTableName}
            ");
        }

        public void UpdateChangelog(Changelog changelog)
        {
            Connection.Execute($@"
                UPDATE {ChangelogTableName}
                SET [AppliedEnd] = @AppliedEnd
                WHERE [Id] = @Id
            ", changelog);
        }

        public Changelog CreateChangelog(Changelog changelog)
        {
            var id = Connection.Query<int>($@"
                INSERT INTO {ChangelogTableName} (
                    [Number]
                    ,[Description]
                    ,[AppliedStart]
                    ,[Md5]
                    )
                VALUES (
                    @Number
                    ,@Description
                    ,@AppliedStart
                    ,@Md5
                    );
                SELECT CAST(SCOPE_IDENTITY() as int);
            ", changelog).Single();

            changelog.Id = id;

            return changelog;
        }

        public void Dispose()
        {
            _readLockTransaction?.Rollback();
            _readLockTransaction?.Dispose();
        }

        public void ExecuteScript(string sql)
        {
            Connection.Execute(sql);
        }
    }
}