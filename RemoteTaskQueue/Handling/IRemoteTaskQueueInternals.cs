using System;

using GroBuf;

using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Profiling;

using SkbKontur.Cassandra.DistributedLock;

using Vostok.Logging.Abstractions;

namespace RemoteQueue.Handling
{
    internal interface IRemoteTaskQueueInternals
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
        IRemoteTaskQueueProfiler RemoteTaskQueueProfiler { get; }
        IRemoteTaskQueue RemoteTaskQueue { get; }
    }
}