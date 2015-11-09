using System;
using System.Collections.Generic;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using MoreLinq;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class OrderedBlobStorage<T> : ColumnFamilyRepositoryBase, IBlobStorage<T>
    {
        public OrderedBlobStorage(IColumnFamilyRepositoryParameters parameters, ISerializer serializer, IGlobalTime globalTime, string columnFamilyName)
            : base(parameters, columnFamilyName)
        {
            this.serializer = serializer;
            this.globalTime = globalTime;
        }

        public void Write([NotNull] string id, T element)
        {
            var connection = RetrieveColumnFamilyConnection();
            var nowTicks = globalTime.UpdateNowTicks();
            var columnInfo = GetColumnInfo(id);

            connection.AddColumn(columnInfo.RowKey, new Column
                {
                    Name = columnInfo.ColumnName,
                    Timestamp = nowTicks,
                    Value = serializer.Serialize(element)
                });
        }

        public void Write(KeyValuePair<string, T>[] elements)
        {
            var connection = RetrieveColumnFamilyConnection();
            var updateNowTicks = globalTime.UpdateNowTicks();

            connection.BatchInsert(elements.Select(x =>
                {
                    var columnInfo = GetColumnInfo(x.Key);
                    return new KeyValuePair<string, IEnumerable<Column>>(
                        columnInfo.RowKey,
                        new[]
                            {
                                new Column
                                    {
                                        Name = columnInfo.ColumnName,
                                        Timestamp = updateNowTicks,
                                        Value = serializer.Serialize(x.Value)
                                    }
                            });
                }));
        }

        public T Read([NotNull] string id)
        {
            var connection = RetrieveColumnFamilyConnection();
            var columnInfo = GetColumnInfo(id);

            Column column;
            if(connection.TryGetColumn(columnInfo.RowKey, columnInfo.ColumnName, out column))
                return serializer.Deserialize<T>(column.Value);
            return default(T);
        }

        //may be rename to Select?
        public T[] Read([NotNull] string[] ids)
        {
            return SelectColumns(ids).Select(column => serializer.Deserialize<T>(column.Value)).ToArray();
        }

        public T[] ReadQuiet([NotNull] string[] ids)
        {
            var result = new T[ids.Length];
            var objectsMap = SelectColumns(ids).ToDictionary(x => x.Name);
            for(var i = 0; i < ids.Length; i++)
            {
                if(objectsMap.ContainsKey(ids[i]))
                    result[i] = serializer.Deserialize<T>(objectsMap[ids[i]].Value);
            }
            return result;
        }

        public IEnumerable<T> ReadAll(int batchSize = 1000)
        {
            return SelectAll(batchSize, column => serializer.Deserialize<T>(column.Value));
        }

        public IEnumerable<KeyValuePair<string, T>> ReadAllWithIds(int batchSize = 1000)
        {
            return SelectAll(batchSize, column => new KeyValuePair<string, T>(column.Name, serializer.Deserialize<T>(column.Value)));
        }

        public void Delete([NotNull] string id, long timestamp)
        {
            var connection = RetrieveColumnFamilyConnection();
            var columnInfo = GetColumnInfo(id);

            connection.DeleteColumn(columnInfo.RowKey, columnInfo.ColumnName, timestamp);
        }

        public void Delete([NotNull] string[] ids, long? timestamp)
        {
            ids
                .Select(GetColumnInfo)
                .GroupBy(x => x.RowKey)
                .ForEach(group =>
                    {
                        group
                            .Batch(1000, Enumerable.ToArray)
                            .ForEach(columnInfo => MakeInConnection(connection => connection.DeleteBatch(group.Key, columnInfo.Select(x => x.ColumnName).Distinct().ToArray(), timestamp)));
                    });
        }

        private IEnumerable<Column> SelectColumns([NotNull] string[] ids)
        {
            if(!ids.Any())
                return new Column[0];

            return ids
                .Select(GetColumnInfo)
                .GroupBy(x => x.RowKey)
                .SelectMany(group =>
                    {
                        return group
                            .Batch(1000, Enumerable.ToArray)
                            .SelectMany(columnInfo => MakeInConnection(connection => connection.GetColumns(group.Key, columnInfo.Select(x => x.ColumnName).Distinct().ToArray())));
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

        private static ColumnInfo GetColumnInfo(string id)
        {
            TimeGuid timeGuid;
            if(!TryGetTimeGuid(id, out timeGuid))
                throw new ArgumentException("Parameter id should be TimeGuid.");

            var ticks = timeGuid.GetTimestamp().Ticks;
            var rowKey = ticks / tickPartition + "_" + timeGuid.GetHashCode() % splittingFactor;

            return new ColumnInfo {RowKey = rowKey, ColumnName = id};
        }

        private static bool TryGetTimeGuid(string input, out TimeGuid timeGuid)
        {
            timeGuid = null;
            Guid guid;
            if(!Guid.TryParse(input, out guid))
                return false;
            if(TimeGuidFormatter.GetVersion(guid) != GuidVersion.TimeBased)
                return false;

            timeGuid = new TimeGuid(guid);
            return true;
        }

        /// <summary>
        ///     пока сделали константой, в будущем можно будет сделать CF внутри которой хранить по тикам значение splittingFactor,
        ///     при старте загружать все в cache и по таймеру обновлять вновь добавленные.
        /// </summary>
        private const int splittingFactor = 10;

        private static readonly long tickPartition = TimeSpan.FromMinutes(6).Ticks;
        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
    }
}