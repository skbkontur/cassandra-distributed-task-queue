using System;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling.ExecutionContext
{
    internal class TaskExecutionContext : IDisposable, ITaskExecutionContext
    {
        private TaskExecutionContext(Task task)
        {
            CurrentTask = task;
        }

        public static TaskExecutionContext ForTask(Task task)
        {
            return TaskStarted(task);
        }

        public static ITaskExecutionContext Current { get { return current; } }
        public Task CurrentTask { get; private set; }

        public void Dispose()
        {
            TaskFinished();
        }

        private static TaskExecutionContext TaskStarted(Task task)
        {
            current = new TaskExecutionContext(task);
            return current;
        }

        private static void TaskFinished()
        {
            current = null;
        }

        [ThreadStatic]
        private static TaskExecutionContext current;
    }
}