using System;

namespace RemoteQueue.Cassandra.Entities
{
    public class TaskMetaUpdatedEvent : IComparable<TaskMetaUpdatedEvent>
    {
        public long Ticks { get; set; }
        public string TaskId { get; set; }

        public int CompareTo(TaskMetaUpdatedEvent other)
        {
            int result = Ticks.CompareTo(other.Ticks);
            if (result != 0) return result;
            return (TaskId ?? "").CompareTo(other.TaskId ?? "");
        }


        public bool Equals(TaskMetaUpdatedEvent other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.TaskId, TaskId) && other.Ticks == Ticks;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(TaskMetaUpdatedEvent)) return false;
            return Equals((TaskMetaUpdatedEvent)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = TaskId != null ? TaskId.GetHashCode() : 0;
                result = (result * 397) ^ Ticks.GetHashCode();
                return result;
            }
        }
    }
}