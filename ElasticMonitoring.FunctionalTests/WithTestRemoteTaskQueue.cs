using System.Linq;
using System.Reflection;

using ExchangeService.UserClasses;

using RemoteQueue.Configuration;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;

using TestCommon;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.FunctionalTests
{
    [WithCassandra("CatalogueCluster", "QueueKeyspace")]
    public class WithTestRemoteTaskQueue : EdiTestSuiteWrapperAttribute
    {
        public override sealed void SetUp(string suiteName, Assembly testAssembly, IEditableEdiTestContext suiteContext)
        {
            var remoteQueueTestsCassandraSettings = new RemoteQueueTestsCassandraSettings();
            suiteContext.Container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseInstances(remoteQueueTestsCassandraSettings);
            suiteContext.Container.Configurator.ForAbstraction<IRemoteTaskQueueSettings>().UseInstances(remoteQueueTestsCassandraSettings);

            suiteContext.Container.Configurator.ForAbstraction<ITaskDataRegistry>().UseType<TaskDataRegistry>();

            var columnFamilies = suiteContext.Container.Get<IColumnFamilyRegistry>().GetAllColumnFamilyNames().Concat(new[]
                {
                    new ColumnFamily
                        {
                            Name = TestCassandraCounterBlobRepository.columnFamilyName,
                        },
                    new ColumnFamily
                        {                            
                            Name = "remoteLock",
                        },
                    new ColumnFamily
                        {
                            Name = CassandraTestTaskLogger.columnFamilyName
                        }
                }).ToArray();
            suiteContext.Container.DropAndCreateDatabase(columnFamilies);
        }
    }
}