using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    public class RemoteTaskInfo
    {
        public RemoteTaskInfo([NotNull] TaskMetaInformation context, [NotNull] IRtqTaskData taskData, [NotNull, ItemNotNull] TaskExceptionInfo[] exceptionInfos)
        {
            Context = context;
            TaskData = taskData;
            ExceptionInfos = exceptionInfos;
        }

        [NotNull]
        public TaskMetaInformation Context { get; }

        [NotNull]
        public IRtqTaskData TaskData { get; }

        [NotNull, ItemNotNull]
        public TaskExceptionInfo[] ExceptionInfos { get; private set; }

        [Obsolete("For FakeRemoteTaskQueue impl only")]
        public void SetExceptionInfo([NotNull] TaskExceptionInfo taskExceptionInfo)
        {
            ExceptionInfos = new[] {taskExceptionInfo};
        }
    }

    public class RemoteTaskInfo<T>
        where T : IRtqTaskData
    {
        public RemoteTaskInfo([NotNull] TaskMetaInformation context, [NotNull] T taskData, [NotNull, ItemNotNull] TaskExceptionInfo[] exceptionInfos)
        {
            Context = context;
            TaskData = taskData;
            ExceptionInfos = exceptionInfos;
        }

        [NotNull]
        public TaskMetaInformation Context { get; }

        [NotNull]
        public T TaskData { get; }

        [NotNull, ItemNotNull]
        public TaskExceptionInfo[] ExceptionInfos { get; }
    }
}