using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Common.RemoteTaskQueue
{
    public class RemoteTaskQueuePlugin
    {
        public static Type cassandraSettings = typeof(RemoteQueueTestsCassandraSettings);
        public static Type taskDataRegistry = typeof(TaskDataRegistry);
    }
}