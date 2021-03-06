﻿using System.Collections.Generic;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories
{
    public interface IHandleTaskCollection
    {
        [NotNull]
        TaskIndexRecord AddTask([NotNull] Task task);

        void ProlongTaskTtl([NotNull] TaskMetaInformation taskMeta, [NotNull] byte[] taskData);

        [NotNull]
        Task GetTask([NotNull] string taskId);

        [CanBeNull]
        Task TryGetTask([NotNull] string taskId);

        [NotNull]
        List<Task> GetTasks([NotNull] string[] taskIds);
    }
}