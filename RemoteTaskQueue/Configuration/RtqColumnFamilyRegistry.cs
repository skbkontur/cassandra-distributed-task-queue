using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes.ChildTaskIndex;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;

using SkbKontur.Cassandra.ThriftClient.Abstractions;

namespace RemoteQueue.Configuration
{
    public class RtqColumnFamilyRegistry
    {
        public RtqColumnFamilyRegistry()
        {
            foreach (var cfName in TaskMetaStorage.GetColumnFamilyNames())
                cfNames.Add(cfName);
            foreach (var cfName in TaskDataStorage.GetColumnFamilyNames())
                cfNames.Add(cfName);
            foreach (var cfName in TaskExceptionInfoStorage.GetColumnFamilyNames())
                cfNames.Add(cfName);
            cfNames.Add(RemoteTaskQueueLockConstants.LockColumnFamily);
            cfNames.Add(RtqMinTicksHolder.ColumnFamilyName);
            cfNames.Add(TicksHolder.ColumnFamilyName);
            cfNames.Add(TaskMinimalStartTicksIndex.ColumnFamilyName);
            cfNames.Add(EventLogRepository.ColumnFamilyName);
            cfNames.Add(ChildTaskIndex.ColumnFamilyName);
        }

        [NotNull, ItemNotNull]
        public ColumnFamily[] GetAllColumnFamilyNames()
        {
            return cfNames.Select(cfName => new ColumnFamily {Name = cfName}).ToArray();
        }

        private readonly HashSet<string> cfNames = new HashSet<string>();
    }
}