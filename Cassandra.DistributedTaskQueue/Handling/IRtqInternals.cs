using System;

using GroBuf;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedLock;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.BlobStorages;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using SkbKontur.Cassandra.DistributedTaskQueue.LocalTasks.TaskQueue;
using SkbKontur.Cassandra.DistributedTaskQueue.Profiling;
using SkbKontur.Cassandra.GlobalTimestamp;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    internal interface IRtqInternals
    {
        TimeSpan TaskTtl { get; }
        ILog Logger { get; }
        ISerializer Serializer { get; }
        IGlobalTime GlobalTime { get; }
        ITaskMinimalStartTicksIndex TaskMinimalStartTicksIndex { get; }
        IHandleTasksMetaStorage HandleTasksMetaStorage { get; }
        IHandleTaskCollection HandleTaskCollection { get; }
        ITaskExceptionInfoStorage TaskExceptionInfoStorage { get; }
        IRemoteLockCreator RemoteLockCreator { get; }
        IRtqProfiler Profiler { get; }
        IRtqTaskProducer TaskProducer { get; }

        void AttachLocalTaskQueue([NotNull] LocalTaskQueue localTaskQueue);
        void ResetTicksHolderInMemoryState();
        void ChangeTaskTtl(TimeSpan ttl);
    }
}