using JetBrains.Annotations;
using log4net;
using log4net.Config;
using LTG.Deployment.DbDeploy.Core;
using LTG.Deployment.DbDeploy.Core.Exceptions;
using LTG.Deployment.DbDeploy.Core.Helpers;
using LTG.Deployment.DbDeploy.Core.Repositories;
using LTG.Deployment.DbDeploy.DataAccess.Repositories;
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

        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            _logger.Info("Starting DbDeploy");

            var connection =
                new SqlConnection(
                    "Data Source=.;Initial Catalog=LTG.Warehouse;Integrated Security=SSPI;Trusted_Connection=True");
            connection.Open();

            var container = new Container(_ =>
            {
                _.For<IFileSystem>().Use<FileSystem>();
                _.For<ITargetDbRepository>().Use<TargetDbRepository>();
                _.For<IChangeScriptRepository>().Use<ChangeScriptRepository>().Ctor<string>().Is(@"c:\Users\tstivers\Scripts");
                _.For<IDbConnection>().Use(connection);
                _.For<IMd5Service>().Use<Md5Service>();
            });

            var deployer = container.GetInstance<DbDeployer>();
            try
            {
                deployer.Execute();
            }
            catch (DbDeployException) // don't need to log stack trace for these as they are logical errors
            {
            }

            System.Console.ReadKey();
        }
    }
}