namespace SKBKontur.Catalogue.RemoteTaskQueue.Profiling
{
    public class GraphiteRemoteTaskQueueProfilerSettings : IGraphiteRemoteTaskQueueProfilerSettings
    {
        public string KeyNamePrefix { get { return "EDI.services.RemoteTaskQueueTasks"; } }
    }
}