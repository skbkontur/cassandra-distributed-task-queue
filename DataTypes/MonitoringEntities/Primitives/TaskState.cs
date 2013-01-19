namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives
{
    public enum TaskState
    {
        Unknown,

        New,
        InProcess,
        WaitingForRerun,
        WaitingForRerunAfterError,
        
        Finished,
        Fatal,
        Canceled,
    }
}