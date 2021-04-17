using System.Reflection;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Profiling;
using SkbKontur.Graphite.Client;

namespace RemoteTaskQueue.FunctionalTests
{
    public class WithNoProfiling : GroboTestSuiteWrapperAttribute
    {
        public override sealed void SetUp([NotNull] string suiteName, [NotNull] Assembly testAssembly, [NotNull] IEditableGroboTestContext suiteContext)
        {
            suiteContext.Container.Configurator.ForAbstraction<IStatsDClient>().UseInstances(NoOpStatsDClient.Instance);
            suiteContext.Container.Configurator.ForAbstraction<IGraphiteClient>().UseInstances(NoOpGraphiteClient.Instance);
            suiteContext.Container.Configurator.ForAbstraction<IRtqProfiler>().UseType<NoOpRtqProfiler>();
        }
    }
}