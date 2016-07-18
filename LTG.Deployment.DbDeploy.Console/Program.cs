using JetBrains.Annotations;
using log4net;
using LTG.Deployment.DbDeploy.Core;
using LTG.Deployment.DbDeploy.Core.Exceptions;
using LTG.Deployment.DbDeploy.Core.Helpers;
using LTG.Deployment.DbDeploy.Core.Repositories;
using StructureMap;
using System.Data;
using System.Data.SqlClient;
using System.IO.Abstractions;
using System.Reflection;

namespace LTG.Deployment.DbDeploy.Console
{
    internal class Program
    {
        [UsedImplicitly]
        private static ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static int Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                return -1;
            }

            _logger.Info("Starting DbDeploy");

            var connection = new SqlConnection(options.ConnectionString);
            try
            {
                connection.Open();
            }
            catch (SqlException sqlEx)
            {
                _logger.Error($"Unable to connect to the target database. Error message: {sqlEx.Message}");
                return -2;
            }

            var container = new Container(_ =>
            {
                _.For<IFileSystem>().Use<FileSystem>();
                _.For<ITargetDbRepository>().Use<TargetDbRepository>();
                _.For<IChangeScriptRepository>().Use<ChangeScriptRepository>().Ctor<string>("scriptPath").Is(options.ScriptFolderPath);
                _.For<IScriptProcessor>().Use<ScriptProcessor>().Ctor<string>("environment").Is(options.Environment);
                _.For<IDbDeployer>().Use<DbDeployer>().Ctor<bool>("isPreDeployment").Is(options.PreRun);
                _.For<IDbConnection>().Use(connection);
                _.For<IMd5Service>().Use<Md5Service>();
            });

            var deployer = container.GetInstance<IDbDeployer>();
            try
            {
                deployer.Execute();
            }
            catch (DbDeployException dbdEx) // don't need to log stack trace for these as they are logical errors
            {
                _logger.Error(dbdEx.Message);
                return -3;
            }

            return 0;
        }
    }
}