using System;
using System.Reflection;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests
{
    public class WithRtqElasticsearchClient : GroboTestSuiteWrapperAttribute
    {
        public override sealed void SetUp([NotNull] string suiteName, [NotNull] Assembly testAssembly, [NotNull] IEditableGroboTestContext suiteContext)
        {
            suiteContext.Container.Configurator.ForAbstraction<IRtqElasticsearchClient>().UseInstances(new RtqElasticsearchClient(new Uri("http://localhost:9205")));
        }
    }
}