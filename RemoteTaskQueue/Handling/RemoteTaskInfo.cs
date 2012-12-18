using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Handling
{
    public class RemoteTaskInfo
    {
        public TaskMetaInformation Context { get; set; }
        public ITaskData TaskData { get; set; }
        public TaskExceptionInfo ExceptionInfo { get; set; }
    }

    public class RemoteTaskInfo<T>
        where T : ITaskData
    {
        public TaskMetaInformation Context { get; set; }
        public T TaskData { get; set; }
        public TaskExceptionInfo ExceptionInfo { get; set; }
    }
}