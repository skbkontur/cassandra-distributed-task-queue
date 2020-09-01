namespace SkbKontur.Cassandra.DistributedTaskQueue.Tracing
{
    public class BusinessLogicTaskTraceContext : PrimitiveTaskTraceContext
    {
        public BusinessLogicTaskTraceContext()
            : base("Handle.BusinessLogic")
        {
        }
    }
}