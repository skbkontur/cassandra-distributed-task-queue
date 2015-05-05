namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
{
    public interface ITaskWriteDynamicSettings
    {
        bool EnableDestructiveActions { get; }
        string CurrentIndexNameFormat { get; }
        string OldIndexNameFormat { get; }
        string LastTicksIndex { get; }
        long CalculatedIndexStartTimeTicks { get; }
    }
}