using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.Indexes
{
    public class TaskIndexShardKey : IEquatable<TaskIndexShardKey>
    {
        public TaskIndexShardKey([NotNull] string taskTopic, TaskState taskState)
        {
            TaskTopic = taskTopic;
            TaskState = taskState;
        }

        [NotNull]
        public string TaskTopic { get; private set; }

        public TaskState TaskState { get; private set; }

        [NotNull]
        public string ToCassandraKey()
        {
            var taskStateCassandraName = TaskState.GetCassandraName();
            return TaskTopic == anyTaskTopic ? taskStateCassandraName : string.Format("{0}_{1}", TaskTopic, taskStateCassandraName);
        }

        public bool Equals(TaskIndexShardKey other)
        {
            if(ReferenceEquals(null, other)) return false;
            if(ReferenceEquals(this, other)) return true;
            return string.Equals(TaskTopic, other.TaskTopic) && TaskState == other.TaskState;
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj)) return false;
            if(ReferenceEquals(this, obj)) return true;
            if(obj.GetType() != this.GetType()) return false;
            return Equals((TaskIndexShardKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (TaskTopic.GetHashCode() * 397) ^ (int)TaskState;
            }
        }

        public override string ToString()
        {
            return string.Format("TaskTopic: {0}, TaskState: {1}", TaskTopic, TaskState);
        }

        [NotNull]
        [Obsolete("Will be removed after migration to sharded queue")]
        public static TaskIndexShardKey AnyTaskTopic(TaskState taskState)
        {
            return new TaskIndexShardKey(anyTaskTopic, taskState);
        }

        private const string anyTaskTopic = "AnyTaskTopic";
    }
}