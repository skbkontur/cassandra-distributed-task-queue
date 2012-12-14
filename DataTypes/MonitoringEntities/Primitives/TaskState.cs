namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives
{
    public enum TaskState
    {
        Unknown,
        New,
        WaitingForRerun,
        WaitingForRerunAfterError,
        Finished,
        InProcess,
        Fatal,
        Canceled,
    }
}