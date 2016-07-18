using log4net.Appender;
using log4net.Core;
using LTG.Deployment.DbDeploy.Core;
using LTG.Deployment.DbDeploy.Core.Exceptions;
using LTG.Deployment.DbDeploy.Core.Helpers;
using LTG.Deployment.DbDeploy.Core.Models;
using LTG.Deployment.DbDeploy.Core.Repositories;
using Moq;
using NUnit.Framework;
using System;

namespace LTG.Deployment.DbDeploy.Tests.Unit
{
    [TestFixture(Category = "Unit")]
    public class DbDeployerTests
    {
        public MemoryAppender Logs { get; set; }

        public Mock<ITargetDbRepository> TargetDbRepositoryMock { get; set; }

        public Mock<IChangeScriptRepository> ChangeScriptRepositoryMock { get; set; }

        public Mock<IScriptProcessor> ScriptProcessorMock { get; set; }

        [SetUp]
        public void SetUp()
        {
            Logs = new MemoryAppender();
            log4net.Config.BasicConfigurator.Configure(Logs);
            TargetDbRepositoryMock = new Mock<ITargetDbRepository>();
            ChangeScriptRepositoryMock = new Mock<IChangeScriptRepository>();
            ScriptProcessorMock = new Mock<IScriptProcessor>();
        }

        [Test]
        public void DbDeployShouldAbortOnDuplicateChangeScriptNumbers()
        {
            var scripts = new[]
            {
                new ChangeScript
                {
                    Number = 1,
                },
                new ChangeScript
                {
                    Number = 1,
                }
            };

            ChangeScriptRepositoryMock.Setup(x => x.GetChangeScripts()).Returns(scripts);

            var db = new DbDeployer(TargetDbRepositoryMock.Object, ChangeScriptRepositoryMock.Object, ScriptProcessorMock.Object);

            Assert.That(() => db.GenerateScriptList(), Throws.InstanceOf<DuplicateChangeNumberException>());
        }

        [Test]
        public void DbDeployShouldAbortOnPartiallyAppliedScript()
        {
            var applied = new[]
            {
                new Changelog()
                {
                    Number = 1,
                    AppliedStart = DateTimeOffset.Now,
                    AppliedEnd = null
                }
            };

            TargetDbRepositoryMock.Setup(x => x.GetChangelogs()).Returns(applied);

            var db = new DbDeployer(TargetDbRepositoryMock.Object, ChangeScriptRepositoryMock.Object, ScriptProcessorMock.Object);

            Assert.That(() => db.GenerateScriptList(), Throws.InstanceOf<ScriptPartiallyAppliedException>());
        }

        [Test]
        public void DbDeployShouldWarnOnChangedScripts()
        {
            var scripts = new[]
            {
                new ChangeScript
                {
                    Number = 1,
                    Description = "test script",
                    Md5 = "abc"
                }
            };

            var applied = new[]
            {
                new Changelog()
                {
                    Number = 1,
                    AppliedStart = DateTimeOffset.Now,
                    AppliedEnd = DateTimeOffset.Now,
                    Md5 = "def"
                }
            };

            ChangeScriptRepositoryMock.Setup(x => x.GetChangeScripts()).Returns(scripts);
            TargetDbRepositoryMock.Setup(x => x.GetChangelogs()).Returns(applied);

            var db = new DbDeployer(TargetDbRepositoryMock.Object, ChangeScriptRepositoryMock.Object, ScriptProcessorMock.Object);

            db.GenerateScriptList();

            Assert.That(Logs.GetEvents(), Has.Exactly(1).Matches<LoggingEvent>(le =>
                     le.Level == Level.Warn
                     && le.RenderedMessage.Contains("MD5")
                     && le.RenderedMessage.Contains(scripts[0].Name)));
        }
    }
}