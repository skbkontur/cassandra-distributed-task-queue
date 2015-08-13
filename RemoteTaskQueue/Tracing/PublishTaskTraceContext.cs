namespace RemoteQueue.Tracing
{
    public class PublishTaskTraceContext : PrimitiveTaskTraceContext
    {
        public PublishTaskTraceContext()
            : base("Publish")
        {
        }
    }
}