namespace RemoteQueue.Tracing
{
    public class BusinessLogicTaskTraceContext : PrimitiveTaskTraceContext
    {
        public BusinessLogicTaskTraceContext()
            : base("Handle.BusinessLogic")
        {
        }
    }
}