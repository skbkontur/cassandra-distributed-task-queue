using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue
{
    public class RemoteTaskQueuePlugin
    {
        public static Type cassandraSettings = typeof(CassandraSettings);
        public static Type taskDataRegistry = typeof(TaskDataRegistry);
    }
}