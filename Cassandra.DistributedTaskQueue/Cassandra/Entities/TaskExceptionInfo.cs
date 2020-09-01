using System;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities
{
    public class TaskExceptionInfo
    {
        public TaskExceptionInfo([NotNull] Exception exception)
        {
            ExceptionMessageInfo = exception.ToString();
        }

        public string ExceptionMessageInfo { get; private set; }
    }
}