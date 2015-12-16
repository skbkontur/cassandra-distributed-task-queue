using System.Collections.Generic;
using System.Linq;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes.ChildTaskIndex;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;

using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace RemoteQueue.Configuration
{
    public class ColumnFamilyRegistry : IColumnFamilyRegistry
    {
        public ColumnFamilyRegistry()
        {
            foreach(var cfName in TaskMetaStorage.GetColumnFamilyNames())
                cfNames.Add(cfName);
            foreach(var cfName in TaskDataStorage.GetColumnFamilyNames())
                cfNames.Add(cfName);
            foreach(var cfName in TaskExceptionInfoStorage.GetColumnFamilyNames())
                cfNames.Add(cfName);
            cfNames.Add(ColumnFamilyRepositoryParameters.LockColumnFamily);
            cfNames.Add(TicksHolder.columnFamilyName);
            cfNames.Add(TaskMinimalStartTicksIndex.columnFamilyName);
            cfNames.Add(EventLogRepository.columnFamilyName);
            cfNames.Add(ChildTaskIndex.columnFamilyName);
        }

        public ColumnFamily[] GetAllColumnFamilyNames()
        {
            return cfNames.Select(cfName => new ColumnFamily
                {
                    Name = cfName
                }).ToArray();
        }

        private readonly HashSet<string> cfNames = new HashSet<string>();
    }
}