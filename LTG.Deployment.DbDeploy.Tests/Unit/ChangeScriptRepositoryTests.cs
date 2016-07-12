using FluentAssertions;
using log4net.Appender;
using log4net.Core;
using LTG.Deployment.DbDeploy.DataAccess.Models;
using LTG.Deployment.DbDeploy.DataAccess.Repositories;
using NUnit.Framework;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;

namespace LTG.Deployment.DbDeploy.Tests.Unit
{
    [TestFixture(Category = "Unit")]
    public class ChangeScriptRepositoryTests
    {
        public MemoryAppender Logs { get; set; }

        public string ScriptPath { get; set; }

        [SetUp]
        public void SetUp()
        {
            Logs = new MemoryAppender();
            log4net.Config.BasicConfigurator.Configure(Logs);
            ScriptPath = "Scripts";
        }

        [Test]
        public void GetChangeScriptsThrowsOnNonExistantDirectory()
        {
            var fileSystem = new MockFileSystem();

            var repo = new ChangeScriptRepository(ScriptPath, fileSystem);

            Assert.That(() => repo.GetChangeScripts(), Throws.ArgumentException.And.Message.Contains(ScriptPath));
        }

        [Test]
        public void ScriptNameRegexTest()
        {
            var scripts = new[]
            {
                new ChangeScript
                {
                    Filename = "001 - Standard.sql",
                    Number = 1,
                    Description = "Standard"
                },
                new ChangeScript
                {
                    Filename = "002.sql",
                    Number = 2,
                    Description = ""
                },
                new ChangeScript
                {
                    Filename = "3  - - --- some stuff.sql",
                    Number = 3,
                    Description = "some stuff"
                },
                new ChangeScript
                {
                    Filename = "004pre - pre script.sql",
                    Number = 4,
                    Description = "pre script",
                    IsPre = true
                },
            };

            var fileSystem = new MockFileSystem(scripts.ToDictionary(x => Path.Combine(ScriptPath, x.Filename), y => new MockFileData("")));

            var repo = new ChangeScriptRepository(ScriptPath, fileSystem);

            var results = repo.GetChangeScripts();

            results.ShouldBeEquivalentTo(scripts);
        }

        [Test]
        public void GetChangeScriptsSkipsInvalidFilenames()
        {
            var badFiles = new[] { "nonumber.sql", "two2numbers3.sql", "nonumber - description has 001 number.sql", "001 - wrongextension.txt" };

            var fileSystem = new MockFileSystem(badFiles.ToDictionary(x => Path.Combine(ScriptPath, x), y => new MockFileData("")));

            var repo = new ChangeScriptRepository(ScriptPath, fileSystem);

            repo.GetChangeScripts();

            foreach (var file in badFiles)
            {
                Assert.That(Logs.GetEvents(), Has.Exactly(1).Matches<LoggingEvent>(le =>
                     le.Level == Level.Warn
                     && le.RenderedMessage.Contains("Skipping")
                     && le.RenderedMessage.Contains(file)));
            }
        }
    }
}