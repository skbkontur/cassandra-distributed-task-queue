namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.DataTypes
{
    public class TaskCount
    {
        public int Count { get; set; }
        public long UpdateTicks { get; set; }
        public long StartTicks { get; set; }
    }
}