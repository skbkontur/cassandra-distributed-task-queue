using NUnit.Framework;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.TestEnvironments.Cassandra;
using SKBKontur.Catalogue.NUnit.Extensions.TestEnvironments.Container;
using SKBKontur.Catalogue.NUnit.Extensions.TestEnvironments.PropertyInjection;
using SKBKontur.Catalogue.NUnit.Extensions.TestEnvironments.Serializer;
using SKBKontur.Catalogue.NUnit.Extensions.TestEnvironments.Settings;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.FunctionalTests.Environment;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.FunctionalTests
{
    [ContainerEnvironment, Cassandra, DefaultSettings(FileName = "functionalTests.csf"), DefaultSerializer, InjectProperties, RemoteTaskQueueRemoteLock]
    public class TaskSearchTestUtils
    {
        [Test, Ignore]
        public void TestDeleteRemoteLock()
        {
            CassandraCluster.RetrieveColumnFamilyConnection("QueueKeyspace", "remoteLock").Truncate();
        }

        [Injected]
        public ICassandraCluster CassandraCluster { get; set; }
    }
}