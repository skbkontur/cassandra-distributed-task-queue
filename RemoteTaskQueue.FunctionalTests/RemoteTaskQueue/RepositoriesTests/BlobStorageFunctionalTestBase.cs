using GroboContainer.Core;
using GroboContainer.Impl;

using GroBuf;
using GroBuf.DataMembersExtracters;

using NUnit.Framework;

using RemoteTaskQueue.FunctionalTests.Common;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.TestCore;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.RepositoriesTests
{
    [TestFixture]
    public abstract class BlobStorageFunctionalTestBase
    {
        [SetUp]
        public void BlobStorageFunctionalTestBase_SetUp()
        {
            Log4NetHelper.SetUpLoggingOnce("BlobStorageFunctionalTestBase");
            Container = new Container(new ContainerConfiguration(AssembliesLoader.Load()));
            Container.Configurator.ForAbstraction<ISerializer>().UseInstances(new Serializer(new AllPropertiesExtractor(), null, GroBufOptions.MergeOnRead));
            Container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseType<RemoteQueueTestsCassandraSettings>();
            Container.ConfigureRemoteTaskQueue();
            Container.DropAndCreateDatabase(GetColumnFamilies());
        }

        protected abstract ColumnFamily[] GetColumnFamilies();

        protected Container Container { get; private set; }
    }
}