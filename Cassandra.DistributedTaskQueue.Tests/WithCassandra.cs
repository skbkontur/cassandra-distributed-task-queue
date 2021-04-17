using System.Reflection;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using JetBrains.Annotations;

using RemoteTaskQueue.FunctionalTests.Common;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests
{
    [WithLogging]
    public class WithCassandra : GroboTestSuiteWrapperAttribute
    {
        public override sealed void SetUp([NotNull] string suiteName, [NotNull] Assembly testAssembly, [NotNull] IEditableGroboTestContext suiteContext)
        {
            suiteContext.Container.ConfigureCassandra();
        }
    }
}