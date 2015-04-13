using System.Reflection;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers.ForSuite;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.FunctionalTests
{
    [WithContainerPerSuite]
    public class WithQueueSettings : EdiTestSuiteWrapperAttribute
    {
        public override sealed void SetUp(string suiteName, Assembly testAssembly, IEdiTestContextData suiteContext)
        {
            suiteContext.GetContainer().Configurator.ForAbstraction<ICassandraClusterSettings>().UseInstances(new RemoteQueueTestsCassandraSettings());
        }
    }
}