using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class LiveRecordTicksMarkerState
    {
        public LiveRecordTicksMarkerState([NotNull] TaskIndexShardKey taskIndexShardKey, long currentTicks)
        {
            TaskIndexShardKey = taskIndexShardKey;
            CurrentTicks = currentTicks;
        }

        [NotNull]
        public TaskIndexShardKey TaskIndexShardKey { get; private set; }

        public long CurrentTicks { get; private set; }

        public override string ToString()
        {
            return string.Format("TaskIndexShardKey: {0}, CurrentTicks: {1}", TaskIndexShardKey, CurrentTicks);
        }
    }
}