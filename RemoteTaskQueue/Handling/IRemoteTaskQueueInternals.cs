using System;

using GroBuf;

using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Profiling;

using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;

namespace RemoteQueue.Handling
{
    public interface IRemoteTaskQueueInternals
    {
        TimeSpan TaskTtl { get; }
        ISerializer Serializer { get; }
        IGlobalTime GlobalTime { get; }
        ITicksHolder TicksHolder { get; }
        ITaskMinimalStartTicksIndex TaskMinimalStartTicksIndex { get; }
        IHandleTasksMetaStorage HandleTasksMetaStorage { get; }
        IHandleTaskCollection HandleTaskCollection { get; }
        ITaskExceptionInfoStorage TaskExceptionInfoStorage { get; }
        IRemoteLockCreator RemoteLockCreator { get; }
        IRemoteTaskQueueProfiler RemoteTaskQueueProfiler { get; }
        IRemoteTaskQueue RemoteTaskQueue { get; }

        void ResetTicksHolderInMemoryState();

        void ChangeTaskTtl(TimeSpan ttl);
    }
}