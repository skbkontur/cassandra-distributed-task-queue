using GroboContainer.Core;

using RemoteQueue.Configuration;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Scheme;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;

namespace TestCommon
{
    public static class CassandraHelpers
    {
        public static void DropAndCreateDatabase(this IContainer container, ColumnFamily[] columnFamilies)
        {
            var settings = container.Get<ICassandraSettings>();
            var cassandraCluster = container.Get<ICassandraCluster>();
            cassandraCluster.ActualizeKeyspaces(new[]
                {
                    new KeyspaceScheme
                        {
                            Name = settings.QueueKeyspace,
                            Configuration =
                                {
                                    ReplicationStrategy = SimpleReplicationStrategy.Create(1),
                                    ColumnFamilies = columnFamilies,
                                }
                        },
                });
            foreach(var columnFamily in columnFamilies)
                cassandraCluster.RetrieveColumnFamilyConnection(settings.QueueKeyspace, columnFamily.Name).Truncate();
        }

        public static void ConfigureRemoteTaskQueue(this IContainer container)
        {
            var remoteQueueTestsCassandraSettings = new RemoteQueueTestsCassandraSettings();
            container.Configurator.ForAbstraction<IRemoteTaskQueueSettings>().UseInstances(remoteQueueTestsCassandraSettings);
            container.Configurator.ForAbstraction<ITaskDataRegistry>().UseType<TaskDataRegistry>();
        }
    }
}