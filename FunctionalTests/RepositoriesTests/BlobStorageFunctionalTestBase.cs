using GroboContainer.Core;
using GroboContainer.Impl;

using GroBuf;
using GroBuf.DataMembersExtracters;

using NUnit.Framework;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;
using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.TestCore;

using TestCommon;

namespace FunctionalTests.RepositoriesTests
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
            Container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseInstances(Container.Get<RemoteQueueTestsCassandraSettings>());
            Container.DropAndCreateDatabase(GetColumnFamilies());
        }

        protected abstract ColumnFamily[] GetColumnFamilies();

        protected Container Container { get; private set; }
    }
}