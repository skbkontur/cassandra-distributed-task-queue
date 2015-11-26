using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using log4net;

using MoreLinq;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;
using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class TimeBasedBlobStorage<T> : ColumnFamilyRepositoryBase, IBlobStorage<T, TimeGuid>
    {
        public TimeBasedBlobStorage(IColumnFamilyRepositoryParameters parameters, ISerializer serializer, IGlobalTime globalTime, string columnFamilyName)
            : base(parameters, columnFamilyName)
        {
            this.serializer = serializer;
            this.globalTime = globalTime;
        }

        public BlobWriteResult Write([NotNull] TimeGuid id, T element)
        {
            var value = serializer.Serialize(element);
            if(value.Length > TimeBasedBlobStorageSettings.BlobSizeLimit)
            {
                logger.WarnFormat("Blob with id={0} has size={1} bytes. Cannot write to columnFamily={2} in keyspace={3}", id.ToGuid(), value.Length, ColumnFamilyName, Keyspace);
                return BlobWriteResult.OutOfSizeLimit;
            }

            var connection = RetrieveColumnFamilyConnection();
            var nowTicks = globalTime.UpdateNowTicks();
            var columnInfo = GetColumnInfo(id);

            connection.AddColumn(columnInfo.RowKey, new Column
                {
                    Name = columnInfo.ColumnName,
                    Timestamp = nowTicks,
                    Value = value
                });
            return BlobWriteResult.Success;
        }

        public T Read([NotNull] TimeGuid id)
        {
            var connection = RetrieveColumnFamilyConnection();
            var columnInfo = GetColumnInfo(id);

            Column column;
            if(connection.TryGetColumn(columnInfo.RowKey, columnInfo.ColumnName, out column))
                return serializer.Deserialize<T>(column.Value);
            return default(T);
        }

        public Dictionary<TimeGuid, T> Read([NotNull] IEnumerable<TimeGuid> ids)
        {
            return SelectColumns(ids).ToDictionary(column => GetIdFromColumnName(column.Name), column => serializer.Deserialize<T>(column.Value));
        }

        public IEnumerable<T> ReadAll(int batchSize = 1000)
        {
            return SelectAll(batchSize, column => serializer.Deserialize<T>(column.Value));
        }

        public IEnumerable<KeyValuePair<TimeGuid, T>> ReadAllWithIds(int batchSize = 1000)
        {
            return SelectAll(batchSize, column => new KeyValuePair<TimeGuid, T>(GetIdFromColumnName(column.Name), serializer.Deserialize<T>(column.Value)));
        }

        public void Delete([NotNull] TimeGuid id, long timestamp)
        {
            var connection = RetrieveColumnFamilyConnection();
            var columnInfo = GetColumnInfo(id);

            connection.DeleteColumn(columnInfo.RowKey, columnInfo.ColumnName, timestamp);
        }

        public void Delete([NotNull] IEnumerable<TimeGuid> ids, long? timestamp)
        {
            ids
                .Select(GetColumnInfo)
                .GroupBy(x => x.RowKey)
                .ForEach(group =>
                    {
                        group
                            .Distinct()
                            .Batch(1000, Enumerable.ToArray)
                            .ForEach(columnInfo => MakeInConnection(connection => connection.DeleteBatch(group.Key, columnInfo.Select(x => x.ColumnName).ToArray(), timestamp)));
                    });
        }

        private IEnumerable<Column> SelectColumns([NotNull] IEnumerable<TimeGuid> ids)
        {
            return ids
                .OrderBy(x => x)
                .Select(GetColumnInfo)
                .GroupBy(x => x.RowKey)
                .SelectMany(group =>
                    {
                        return group
                            .Distinct()
                            .Batch(1000, Enumerable.ToArray)
                            .SelectMany(columnInfo => MakeInConnection(connection => connection.GetColumns(group.Key, columnInfo.Select(x => x.ColumnName).ToArray())));
                    })
                .Where(x => x.Value != null);
        }

        private IEnumerable<TResult> SelectAll<TResult>(int batchSize, Func<Column, TResult> createResult)
        {
            string exclusiveStartKey = null;
            while(true)
            {
                var keys = RetrieveColumnFamilyConnection().GetKeys(exclusiveStartKey, batchSize);
                if(keys.Length == 0)
                    yield break;

                foreach(var key in keys)
                {
                    string exclusiveStartColumnName = null;
                    while(true)
                    {
                        var columns = RetrieveColumnFamilyConnection().GetColumns(key, exclusiveStartColumnName, batchSize);
                        if(columns.Length == 0)
                            break;

                        foreach(var column in columns)
                            yield return createResult(column);

                        exclusiveStartColumnName = columns.Last().Name;
                    }
                }
                exclusiveStartKey = keys.Last();
            }
        }

        private void MakeInConnection(Action<IColumnFamilyConnection> action)
        {
            var connection = RetrieveColumnFamilyConnection();
            action(connection);
        }

        private TResult MakeInConnection<TResult>(Func<IColumnFamilyConnection, TResult> action)
        {
            var connection = RetrieveColumnFamilyConnection();
            return action(connection);
        }

        private static ColumnInfo GetColumnInfo(TimeGuid id)
        {
            var ticks = id.GetTimestamp().Ticks;
            var rowKey = ticks / TimeBasedBlobStorageSettings.TickPartition + "_" + ticks % TimeBasedBlobStorageSettings.SplittingFactor;

            return new ColumnInfo {RowKey = rowKey, ColumnName = ticks.ToString("D20", CultureInfo.InvariantCulture) + "_" + id.ToGuid()};
        }

        private static TimeGuid GetIdFromColumnName(string columnName)
        {
            var parts = columnName.Split('_');
            return new TimeGuid(Guid.Parse(parts[1]));
        }

        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
        private static readonly ILog logger = LogManager.GetLogger("TimeBasedBlobStorage");
    }
}