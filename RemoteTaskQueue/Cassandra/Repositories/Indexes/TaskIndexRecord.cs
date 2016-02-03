using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

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
            var minimalStartTicks = MinimalStartTicks >= Timestamp.MinValue.Ticks ? new Timestamp(MinimalStartTicks).ToString() : MinimalStartTicks.ToString();
            return string.Format("TaskId: {0}, MinimalStartTicks: {1}, TaskIndexShardKey: {2}", TaskId, minimalStartTicks, TaskIndexShardKey);
        }
    }
}