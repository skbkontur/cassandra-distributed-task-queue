namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities
{
    public class TaskCount
    {
        public int Count { get; set; }
        public long UpdateTicks { get; set; }
        public long StartTicks { get; set; }
    }
}