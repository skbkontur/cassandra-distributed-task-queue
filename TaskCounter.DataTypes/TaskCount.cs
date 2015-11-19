namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.DataTypes
{
    public class TaskCount
    {
        public int Count { get; set; }
        public long OldWaitingTaskCount { get; set; }
        public int[] Counts { get; set; }
        public long UpdateTicks { get; set; }
        public long StartTicks { get; set; }
    }
}