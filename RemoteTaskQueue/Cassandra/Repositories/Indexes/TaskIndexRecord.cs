using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes
{
    public class TaskIndexRecord
    {
        public TaskIndexRecord([NotNull] string taskId, long minimalStartTicks, [NotNull] TaskTopicAndState taskTopicAndState)
        {
            TaskId = taskId;
            MinimalStartTicks = minimalStartTicks;
            TaskTopicAndState = taskTopicAndState;
        }

        [NotNull]
        public string TaskId { get; private set; }

        public long MinimalStartTicks { get; private set; }

        [NotNull]
        public TaskTopicAndState TaskTopicAndState { get; private set; }

        public override string ToString()
        {
            return string.Format("TaskId: {0}, MinimalStartTicks: {1}, TaskTopicAndState: {2}", TaskId, MinimalStartTicks, TaskTopicAndState);
        }
    }
}