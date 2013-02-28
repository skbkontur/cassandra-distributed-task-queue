using System.Collections.Generic;
using System.Linq;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;

using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace RemoteQueue.Configuration
{
    public class ColumnFamilyRegistry : IColumnFamilyRegistry
    {
        public ColumnFamilyRegistry()
        {
            Register(ColumnFamilyRepositoryParameters.LockColumnFamily);

            Register(TaskDataBlobStorage.columnFamilyName);
            Register(TaskExceptionInfoBlobStorage.columnFamilyName);
            Register(TaskMetaInformationBlobStorage.columnFamilyName);
            Register(TicksHolder.columnFamilyName);
            Register(TaskMinimalStartTicksIndex.columnFamilyName);
            Register(EventLogRepository.columnFamilyName);
        }

        public ColumnFamily[] GetAllColumnFamilyNames()
        {
            return set.ToArray();
        }

        private void Register(string columnFamilyName)
        {
            set.Add(new ColumnFamily
                {
                    Name = columnFamilyName
                });
        }

        private readonly HashSet<ColumnFamily> set = new HashSet<ColumnFamily>();
    }
}