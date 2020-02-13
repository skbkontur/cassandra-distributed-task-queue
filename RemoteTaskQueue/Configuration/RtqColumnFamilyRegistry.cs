using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Primitives;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.BlobStorages;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.ChildTaskIndex;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using SkbKontur.Cassandra.ThriftClient.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Configuration
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