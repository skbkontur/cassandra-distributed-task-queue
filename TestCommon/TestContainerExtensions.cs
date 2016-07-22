using GroboContainer.Core;

using RemoteQueue.Configuration;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue;

namespace TestCommon
{
    public static class TestContainerExtensions
    {
        public static void ConfigureForTests(this IContainer container)
        {
            container.Configurator.ForAbstraction<ICassandraClusterSettings>().UseType<RemoteQueueTestsCassandraSettings>();
            container.Configurator.ForAbstraction<IRemoteTaskQueueSettings>().UseType<RemoteQueueTestsCassandraSettings>();
            container.Configurator.ForAbstraction<ITaskDataRegistry>().UseType<TaskDataRegistry>();
        }
    }
}
