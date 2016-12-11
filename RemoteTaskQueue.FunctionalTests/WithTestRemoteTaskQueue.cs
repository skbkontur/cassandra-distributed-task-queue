using System.Reflection;

using RemoteTaskQueue.FunctionalTests.Common;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;

namespace RemoteTaskQueue.FunctionalTests
{
    [WithDefaultSerializer, WithCassandra("CatalogueCluster", "QueueKeyspace")]
    public class WithTestRemoteTaskQueue : EdiTestSuiteWrapperAttribute
    {
        public override sealed void SetUp(string suiteName, Assembly testAssembly, IEditableEdiTestContext suiteContext)
        {
            suiteContext.Container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseType<RemoteQueueTestsCassandraSettings>();
            suiteContext.Container.ConfigureRemoteTaskQueue();
            suiteContext.Container.ConfigureLockRepository();
        }
    }
}