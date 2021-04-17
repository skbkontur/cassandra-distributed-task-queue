using System.Reflection;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using JetBrains.Annotations;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests
{
    public class WithLogging : GroboTestSuiteWrapperAttribute
    {
        public override sealed void SetUp([NotNull] string suiteName, [NotNull] Assembly testAssembly, [NotNull] IEditableGroboTestContext suiteContext)
        {
            var defaultLog = TestLoggingConfigurator.SetUpLoggingOnce();
            suiteContext.Container.Configurator.ForAbstraction<ILog>().UseInstances(defaultLog);
        }

        public override sealed void TearDown([NotNull] string suiteName, [NotNull] Assembly testAssembly, [NotNull] IEditableGroboTestContext suiteContext)
        {
            TestLoggingConfigurator.FlushAll();
        }
    }
}