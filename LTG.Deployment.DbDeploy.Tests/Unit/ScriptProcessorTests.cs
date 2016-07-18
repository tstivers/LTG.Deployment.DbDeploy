using LTG.Deployment.DbDeploy.Core.Helpers;
using NUnit.Framework;

namespace LTG.Deployment.DbDeploy.Tests.Unit
{
    [TestFixture(Category = "Unit")]
    public class ScriptProcessorTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void ScriptProcessorShouldFilterOnEnvironment()
        {
            const string script = @"
            -- @ENV PROD
            update prod stuff;
            -- @ENV TEST
            update test stuff;
            ";

            var sp = new ScriptProcessor("TEST");

            var results = sp.ProcessScript(script);

            Assert.That(results, Does.Match(@"^\s*update test stuff;\s*$"));
            Assert.That(results, Does.Not.Match(@"^\s*update prod stuff;\s*$"));
            Assert.That(results, Does.Not.Match(@"-- @ENV"));
        }

        [Test]
        public void ScriptProcessorShouldSupportWildcardEnvironment()
        {
            const string script = @"
            update everything;
            -- @ENV PROD
            update prod stuff;
            -- @ENV TEST
            update test stuff;
            -- @ENV *
            update everything else;
            ";

            var sp = new ScriptProcessor("TEST");

            var results = sp.ProcessScript(script);

            Assert.That(results, Does.Match(@"^\s*update everything;\s*\s*update test stuff;\s*update everything else;\s*$"));
            Assert.That(results, Does.Not.Match(@"^\s*update prod stuff;\s*$"));
            Assert.That(results, Does.Not.Match(@"-- @ENV"));
        }
    }
}