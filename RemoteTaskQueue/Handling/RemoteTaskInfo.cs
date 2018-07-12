using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Handling
{
    public class RemoteTaskInfo
    {
        public RemoteTaskInfo([NotNull] TaskMetaInformation context, [NotNull] ITaskData taskData, [NotNull] TaskExceptionInfo[] exceptionInfos)
        {
            Context = context;
            TaskData = taskData;
            ExceptionInfos = exceptionInfos;
        }

        [NotNull]
        public TaskMetaInformation Context { get; private set; }

        [NotNull]
        public ITaskData TaskData { get; private set; }

        [NotNull]
        public TaskExceptionInfo[] ExceptionInfos { get; private set; }

        [Obsolete("For FakeRemoteTaskQueue impl only")]
        public void SetExceptionInfo([NotNull] TaskExceptionInfo taskExceptionInfo)
        {
            ExceptionInfos = new[] {taskExceptionInfo};
        }
    }

    public class RemoteTaskInfo<T>
        where T : ITaskData
    {
        public RemoteTaskInfo([NotNull] TaskMetaInformation context, [NotNull] T taskData, [NotNull] TaskExceptionInfo[] exceptionInfos)
        {
            Context = context;
            TaskData = taskData;
            ExceptionInfos = exceptionInfos;
        }

        [NotNull]
        public TaskMetaInformation Context { get; private set; }

        [NotNull]
        public T TaskData { get; private set; }

        [NotNull]
        public TaskExceptionInfo[] ExceptionInfos { get; private set; }
    }
}