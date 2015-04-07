using System.Linq;

using GroboContainer.Core;

using JetBrains.Annotations;

using NUnit.Framework;

using RemoteQueue.Configuration;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.Core.Configuration.Settings;
using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers.ForSuite;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.FunctionalTests
{
    [EdiTestSuite, WithCassandra("CatalogueCluster", "QueueKeyspace"), WithDefaultSerializer, WithRemoteLock("remoteLock")]
    public class TaskSearchTestUtils
    {
        [Test, Ignore]
        public void TestDeleteRemoteLock()
        {
            cassandraCluster.RetrieveColumnFamilyConnection("QueueKeyspace", "remoteLock").Truncate();
        }
        //private void ConfigureRemoteLock(IContainer container)
        //{
        //    var keyspaceName = container.Get<IApplicationSettings>().GetString("KeyspaceName");
        //    const string columnFamilyName = "remoteLock";
        //    container.Configurator.ForAbstraction<IRemoteLockImplementation>().UseInstances(container.Create<ColumnFamilyFullName, CassandraRemoteLockImplementation>(new ColumnFamilyFullName(keyspaceName, columnFamilyName)));
        //}
        [Test, Ignore]
        public void TestCreateCassandraSchema()
        {
            DropAndCreateDatabase(columnFamilyRegistry.GetAllColumnFamilyNames().Concat(new[]
                {
                    new ColumnFamily
                        {
                            Name = "columnFamilyName",
                        },
                    new ColumnFamily
                        {
                            Name = "remoteLock",
                        }
                }).ToArray());
        }

        private void DropAndCreateDatabase(ColumnFamily[] columnFamilies)
        {
            var settings = container.Get<ICassandraSettings>();
            var clusterConnection = cassandraCluster.RetrieveClusterConnection();
            var keyspaceConnection = cassandraCluster.RetrieveKeyspaceConnection(settings.QueueKeyspace);

            var keyspaces = clusterConnection.RetrieveKeyspaces();
            if(keyspaces.All(x => x.Name != settings.QueueKeyspace))
            {
                clusterConnection.AddKeyspace(
                    new Keyspace
                        {
                            Name = settings.QueueKeyspace,
                            ReplicaPlacementStrategy = "org.apache.cassandra.locator.SimpleStrategy",
                            ReplicationFactor = 1
                        });
            }

            var cassandraColumnFamilies = keyspaceConnection.DescribeKeyspace().ColumnFamilies;
            foreach(var columnFamily in columnFamilies)
            {
                if(!cassandraColumnFamilies.Any(x => x.Key == columnFamily.Name))
                    keyspaceConnection.AddColumnFamily(columnFamily);
            }

            foreach(var columnFamily in columnFamilies)
            {
                var columnFamilyConnection = cassandraCluster.RetrieveColumnFamilyConnection(settings.QueueKeyspace, columnFamily.Name);
                columnFamilyConnection.Truncate();
            }
        }

        // ReSharper disable UnassignedReadonlyField.Compiler
        [Injected]
        private readonly IColumnFamilyRegistry columnFamilyRegistry;

        [Injected]
        private readonly IContainer container;

        [Injected]
        private readonly ICassandraCluster cassandraCluster;

        // ReSharper restore UnassignedReadonlyField.Compiler
    }
}