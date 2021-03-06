﻿using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities
{
    public class TaskMetaUpdatedEvent
    {
        public TaskMetaUpdatedEvent([NotNull] string taskId, long ticks)
        {
            if (string.IsNullOrWhiteSpace(taskId))
                throw new InvalidOperationException(string.Format("TaskId is empty for ticks: {0}", ticks));
            TaskId = taskId;
            Ticks = ticks;
        }

        [NotNull]
        public string TaskId { get; private set; }

        public long Ticks { get; private set; }

        public override string ToString()
        {
            return string.Format("TaskId: {0}, Timestamp: {1}", TaskId, new Timestamp(Ticks));
        }
    }
}