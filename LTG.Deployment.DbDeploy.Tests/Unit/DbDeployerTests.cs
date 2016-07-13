using log4net.Appender;
using log4net.Core;
using LTG.Deployment.DbDeploy.Core;
using LTG.Deployment.DbDeploy.Core.Exceptions;
using LTG.Deployment.DbDeploy.Core.Models;
using LTG.Deployment.DbDeploy.Core.Repositories;
using LTG.Deployment.DbDeploy.DataAccess.Repositories;
using Moq;
using NUnit.Framework;
using System;

namespace LTG.Deployment.DbDeploy.Tests.Unit
{
    [TestFixture(Category = "Unit")]
    public class DbDeployerTests
    {
        public MemoryAppender Logs { get; set; }

        [SetUp]
        public void SetUp()
        {
            Logs = new MemoryAppender();
            log4net.Config.BasicConfigurator.Configure(Logs);
        }

        [Test]
        public void DbDeployShouldAbortOnDuplicateChangeScriptNumbers()
        {
            var tr = new Mock<ITargetDbRepository>();
            var sr = new Mock<IChangeScriptRepository>();

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

            sr.Setup(x => x.GetChangeScripts()).Returns(scripts);

            var db = new DbDeployer(tr.Object, sr.Object);

            Assert.That(() => db.GenerateScriptList(), Throws.InstanceOf<DuplicateChangeNumberException>());
        }

        [Test]
        public void DbDeployShouldAbortOnPartiallyAppliedScript()
        {
            var tr = new Mock<ITargetDbRepository>();
            var sr = new Mock<IChangeScriptRepository>();

            var applied = new[]
            {
                new Changelog()
                {
                    Number = 1,
                    AppliedStart = DateTimeOffset.Now,
                    AppliedEnd = null
                }
            };

            tr.Setup(x => x.GetChangelogs()).Returns(applied);

            var db = new DbDeployer(tr.Object, sr.Object);

            Assert.That(() => db.GenerateScriptList(), Throws.InstanceOf<ScriptPartiallyAppliedException>());
        }

        [Test]
        public void DbDeployShouldWarnOnChangedScripts()
        {
            var tr = new Mock<ITargetDbRepository>();
            var sr = new Mock<IChangeScriptRepository>();

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

            sr.Setup(x => x.GetChangeScripts()).Returns(scripts);
            tr.Setup(x => x.GetChangelogs()).Returns(applied);

            var db = new DbDeployer(tr.Object, sr.Object);

            db.GenerateScriptList();

            Assert.That(Logs.GetEvents(), Has.Exactly(1).Matches<LoggingEvent>(le =>
                     le.Level == Level.Warn
                     && le.RenderedMessage.Contains("MD5")
                     && le.RenderedMessage.Contains(scripts[0].Name)));
        }
    }
}