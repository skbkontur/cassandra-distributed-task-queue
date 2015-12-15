﻿namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
{
    public interface ITaskWriteDynamicSettings
    {
        bool EnableDestructiveActions { get; }
        string CurrentIndexNameFormat { get; }
        string OldIndexNameFormat { get; }
        string LastTicksIndex { get; }
        long CalculatedIndexStartTimeTicks { get; }
        string GraphitePrefixOrNull { get; }
        string RemoteLockId { get; }
        long? MaxTicks { get; }
        int MaxBatch { get; }
    }
}