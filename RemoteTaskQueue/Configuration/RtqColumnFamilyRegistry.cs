using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

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
            cfNames.Add(RtqMinTicksHolder.ColumnFamilyName);
            cfNames.Add(TaskMinimalStartTicksIndex.ColumnFamilyName);
            cfNames.Add(EventLogRepository.ColumnFamilyName);
            cfNames.Add(ChildTaskIndex.ColumnFamilyName);
        }

        [NotNull, ItemNotNull]
        public ColumnFamily[] GetAllColumnFamilyNamesExceptLocks()
        {
            return cfNames.Select(cfName => new ColumnFamily {Name = cfName}).ToArray();
        }

        public const string LocksColumnFamilyName = "Locks";

        [Obsolete("// todo (andrew, 01.03.2020): remove after avk/rtqLock release")]
        public const string LegacyLocksColumnFamilyName = "RemoteTaskQueueLock";

        private readonly HashSet<string> cfNames = new HashSet<string>();
    }
}