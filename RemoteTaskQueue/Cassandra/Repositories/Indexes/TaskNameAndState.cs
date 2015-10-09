using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.Indexes
{
    public class TaskNameAndState : IEquatable<TaskNameAndState>
    {
        public TaskNameAndState([NotNull] string taskName, TaskState taskState)
        {
            TaskName = taskName;
            TaskState = taskState;
        }

        [NotNull]
        public string TaskName { get; private set; }

        public TaskState TaskState { get; private set; }

        [NotNull]
        public string ToCassandraKey()
        {
            var taskStateCassandraName = TaskState.GetCassandraName();
            return TaskName == anyTaskName ? taskStateCassandraName : string.Format("{0}_{1}", TaskName, taskStateCassandraName);
        }

        public bool Equals(TaskNameAndState other)
        {
            if(ReferenceEquals(null, other)) return false;
            if(ReferenceEquals(this, other)) return true;
            return string.Equals(TaskName, other.TaskName) && TaskState == other.TaskState;
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj)) return false;
            if(ReferenceEquals(this, obj)) return true;
            if(obj.GetType() != this.GetType()) return false;
            return Equals((TaskNameAndState)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (TaskName.GetHashCode() * 397) ^ (int)TaskState;
            }
        }

        public override string ToString()
        {
            return string.Format("TaskName: {0}, TaskState: {1}", TaskName, TaskState);
        }

        [NotNull]
        public static TaskNameAndState AnyTaskName(TaskState taskState)
        {
            return new TaskNameAndState(anyTaskName, taskState);
        }

        private const string anyTaskName = "AnyTaskName";
    }
}