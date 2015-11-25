using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes
{
    public class TaskIndexRecord
    {
        public TaskIndexRecord([NotNull] string taskId, long minimalStartTicks, [NotNull] TaskIndexShardKey taskIndexShardKey)
        {
            TaskId = taskId;
            MinimalStartTicks = minimalStartTicks;
            TaskIndexShardKey = taskIndexShardKey;
        }

        [NotNull]
        public string TaskId { get; private set; }

        public long MinimalStartTicks { get; private set; }

        [NotNull]
        public TaskIndexShardKey TaskIndexShardKey { get; private set; }

        public override string ToString()
        {
            return string.Format("TaskId: {0}, MinimalStartTicks: {1}, TaskIndexShardKey: {2}", TaskId, MinimalStartTicks, TaskIndexShardKey);
        }
    }
}