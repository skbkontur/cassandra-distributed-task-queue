using System.Collections.Concurrent;

using JetBrains.Annotations;

using Metrics;

using RemoteQueue.Cassandra.Repositories.Indexes;

namespace RemoteQueue.Handling
{
    internal class TaskShardMetrics
    {
        private TaskShardMetrics([NotNull] TaskIndexShardKey shardKey)
        {
            var context = Metric.Context("RemoteTaskQueue").Context("Shards").Context(shardKey.TaskTopic).Context(shardKey.TaskState.ToString()).Context("Tasks");
            Started = context.Meter("Started", Unit.Events, TimeUnit.Minutes);
            NoMeta = context.Meter("NoMeta", Unit.Events, TimeUnit.Minutes);
            InconsistentIndexRecord = context.Meter("InconsistentIndexRecord", Unit.Events, TimeUnit.Minutes);
            InconsistentIndexRecord_UnderLock = context.Meter("InconsistentIndexRecord_UnderLock", Unit.Events, TimeUnit.Minutes);
            TaskAlreadyFinished_UnderLock = context.Meter("TaskAlreadyFinished_UnderLock", Unit.Events, TimeUnit.Minutes);
            FixIndex_UnderLock = context.Meter("FixIndex_UnderLock", Unit.Events, TimeUnit.Minutes);
            StartProcessingFailed_UnderLock = context.Meter("StartProcessingFailed_UnderLock", Unit.Events, TimeUnit.Minutes);
            DidNotGetTaskGroupLock = context.Meter("DidNotGetTaskGroupLock", Unit.Events, TimeUnit.Minutes);
            GotTaskGroupLock = context.Meter("GotTaskGroupLock", Unit.Events, TimeUnit.Minutes);
            DidNotGetTaskLock = context.Meter("DidNotGetTaskLock", Unit.Events, TimeUnit.Minutes);
            GotTaskLock = context.Meter("GotTaskLock", Unit.Events, TimeUnit.Minutes);
            ReadTaskException_UnderLock = context.Meter("Started", Unit.Events, TimeUnit.Minutes);
            Processed = context.Meter("Processed", Unit.Events, TimeUnit.Minutes);
        }

        public static TaskShardMetrics ForShard([NotNull] TaskIndexShardKey shardKey)
        {
            return shardKeyToMetrics.GetOrAdd(shardKey, s => new TaskShardMetrics(shardKey));
        }

        [NotNull]
        public Meter Started { get; private set; }

        [NotNull]
        public Meter NoMeta { get; private set; }

        [NotNull]
        public Meter InconsistentIndexRecord { get; private set; }

        [NotNull]
        public Meter InconsistentIndexRecord_UnderLock { get; private set; }

        [NotNull]
        public Meter TaskAlreadyFinished_UnderLock { get; private set; }

        [NotNull]
        public Meter FixIndex_UnderLock { get; private set; }

        [NotNull]
        public Meter StartProcessingFailed_UnderLock { get; private set; }

        [NotNull]
        public Meter DidNotGetTaskGroupLock { get; private set; }

        [NotNull]
        public Meter GotTaskGroupLock { get; private set; }

        [NotNull]
        public Meter DidNotGetTaskLock { get; private set; }

        [NotNull]
        public Meter GotTaskLock { get; private set; }

        [NotNull]
        public Meter ReadTaskException_UnderLock { get; private set; }

        [NotNull]
        public Meter Processed { get; private set; }

        private static readonly ConcurrentDictionary<TaskIndexShardKey, TaskShardMetrics> shardKeyToMetrics = new ConcurrentDictionary<TaskIndexShardKey, TaskShardMetrics>();
    }
}