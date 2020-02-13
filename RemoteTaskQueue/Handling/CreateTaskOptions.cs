namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    public class CreateTaskOptions
    {
        public string ParentTaskId { get; set; }
        public string TaskGroupLock { get; set; }
    }
}