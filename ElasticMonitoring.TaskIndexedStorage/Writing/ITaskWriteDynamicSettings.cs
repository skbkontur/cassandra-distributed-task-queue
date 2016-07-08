namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
{
    public interface ITaskWriteDynamicSettings
    {
        string CurrentIndexNameFormat { get; }
        string OldIndexNameFormat { get; }
        string OldDataIndex { get; }
        string SearchAliasFormat { get; }
        string OldDataAliasFormat { get; }
        string LastTicksIndex { get; }

        long CalculatedIndexStartTimeTicks { get; }

        string GraphitePrefixOrNull { get; }
        string RemoteLockId { get; }
        long? MaxTicks { get; }
        int MaxBatch { get; }

        bool EnableDestructiveActions { get; }
    }
}