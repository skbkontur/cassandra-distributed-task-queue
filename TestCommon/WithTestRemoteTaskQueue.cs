using System.Reflection;

using GroboContainer.Core;

using RemoteQueue.Configuration;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;

namespace TestCommon
{
    [WithCassandra("CatalogueCluster", "QueueKeyspace")]
    public class WithTestRemoteTaskQueue : EdiTestSuiteWrapperAttribute
    {
        public override sealed void SetUp(string suiteName, Assembly testAssembly, IEditableEdiTestContext suiteContext)
        {
            SetUpRemoteTaskQueue(suiteContext.Container);
        }

        public static void SetUpRemoteTaskQueue(IContainer container)
        {
            var remoteQueueTestsCassandraSettings = new RemoteQueueTestsCassandraSettings();
            container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseInstances(remoteQueueTestsCassandraSettings);
            container.Configurator.ForAbstraction<IRemoteTaskQueueSettings>().UseInstances(remoteQueueTestsCassandraSettings);
            container.Configurator.ForAbstraction<ITaskDataRegistry>().UseType<TaskDataRegistry>();
        }
    }
}