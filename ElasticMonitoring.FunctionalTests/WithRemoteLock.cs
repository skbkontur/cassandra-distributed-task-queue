using System.Reflection;

using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.FunctionalTests
{
    [WithDefaultSerializer]
    [WithCassandra("CatalogueCluster", "QueueKeyspace")]
    public class WithRemoteLock : EdiTestSuiteWrapperAttribute
    {
        private readonly string remoteLockColumnFamly;

        public WithRemoteLock(string remoteLockColumnFamly)
        {
            this.remoteLockColumnFamly = remoteLockColumnFamly;
        }

        public override sealed void SetUp(string suiteName, Assembly testAssembly, IEditableEdiTestContext suiteContext)
        {
            suiteContext.Container.ConfigureLockRepository(remoteLockColumnFamly);
        }
    }
}