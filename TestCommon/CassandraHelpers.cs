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
            var cassandraCluster = container.Get<ICassandraCluster>();
            var queueKeyspace = container.Get<IRemoteTaskQueueSettings>().QueueKeyspace;
            cassandraCluster.ActualizeKeyspaces(new[]
                {
                    new KeyspaceScheme
                        {
                            Name = queueKeyspace,
                            Configuration =
                                {
                                    ReplicationStrategy = SimpleReplicationStrategy.Create(1),
                                    ColumnFamilies = columnFamilies,
                                }
                        },
                });
            foreach(var columnFamily in columnFamilies)
                cassandraCluster.RetrieveColumnFamilyConnection(queueKeyspace, columnFamily.Name).Truncate();
        }

        public static void ConfigureRemoteTaskQueue(this IContainer container)
        {
            container.Configurator.ForAbstraction<IRemoteTaskQueueSettings>().UseInstances(new RemoteQueueTestsCassandraSettings());
            container.Configurator.ForAbstraction<ITaskDataRegistry>().UseType<TaskDataRegistry>();
        }
    }
}