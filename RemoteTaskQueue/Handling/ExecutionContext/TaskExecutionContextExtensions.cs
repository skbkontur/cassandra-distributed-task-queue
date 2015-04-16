namespace RemoteQueue.Handling.ExecutionContext
{
    internal static class TaskExecutionContextExtensions
    {
        public static ReadOnlyTaskExecutionContext ToReadOnly(this ITaskExecutionContext context)
        {
            return context == null ? null : new ReadOnlyTaskExecutionContext(context);
        }
    }
}