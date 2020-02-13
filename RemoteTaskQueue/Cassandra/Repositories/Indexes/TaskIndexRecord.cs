using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes
{
    public class TaskIndexRecord : IEquatable<TaskIndexRecord>
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
            var minimalStartTicks = MinimalStartTicks >= Timestamp.MinValue.Ticks && MinimalStartTicks <= Timestamp.MaxValue.Ticks ? new Timestamp(MinimalStartTicks).ToString() : MinimalStartTicks.ToString();
            return string.Format("TaskId: {0}, MinimalStartTicks: {1}, TaskIndexShardKey: {2}", TaskId, minimalStartTicks, TaskIndexShardKey);
        }

        public bool Equals(TaskIndexRecord other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(TaskId, other.TaskId) && MinimalStartTicks == other.MinimalStartTicks && TaskIndexShardKey.Equals(other.TaskIndexShardKey);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((TaskIndexRecord)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TaskId.GetHashCode();
                hashCode = (hashCode * 397) ^ MinimalStartTicks.GetHashCode();
                hashCode = (hashCode * 397) ^ TaskIndexShardKey.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(TaskIndexRecord left, TaskIndexRecord right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TaskIndexRecord left, TaskIndexRecord right)
        {
            return !Equals(left, right);
        }
    }
}