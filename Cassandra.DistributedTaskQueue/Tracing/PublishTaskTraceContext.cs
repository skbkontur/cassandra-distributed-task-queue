namespace SkbKontur.Cassandra.DistributedTaskQueue.Tracing
{
    public class PublishTaskTraceContext : PrimitiveTaskTraceContext
    {
        public PublishTaskTraceContext()
            : base("Publish")
        {
        }
    }
}