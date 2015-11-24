using JetBrains.Annotations;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Profiling
{
    public interface IGraphiteRemoteTaskQueueProfilerSettings
    {
        [CanBeNull]
        string KeyNamePrefix { get; }
    }
}