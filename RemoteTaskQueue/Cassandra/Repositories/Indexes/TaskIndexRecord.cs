using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes
{
    public class TaskIndexRecord
    {
        public TaskIndexRecord([NotNull] string taskId, long minimalStartTicks, [NotNull] TaskNameAndState taskNameAndState)
        {
            TaskId = taskId;
            MinimalStartTicks = minimalStartTicks;
            TaskNameAndState = taskNameAndState;
        }

        [NotNull]
        public string TaskId { get; private set; }

        public long MinimalStartTicks { get; private set; }

        [NotNull]
        public TaskNameAndState TaskNameAndState { get; private set; }

        public override string ToString()
        {
            return string.Format("TaskId: {0}, MinimalStartTicks: {1}, TaskNameAndState: {2}", TaskId, MinimalStartTicks, TaskNameAndState);
        }
    }
}