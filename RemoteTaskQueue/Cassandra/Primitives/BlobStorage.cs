using System;
using System.Collections.Generic;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using MoreLinq;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Primitives
{
    public class BlobStorage<T> : ColumnFamilyRepositoryBase, IBlobStorage<T>
    {
        public BlobStorage(IColumnFamilyRepositoryParameters parameters, ISerializer serializer, IGlobalTime globalTime, string columnFamilyName)
            : base(parameters, columnFamilyName)
        {
            this.serializer = serializer;
            this.globalTime = globalTime;
        }

        public BlobWriteResult Write([NotNull] string id, T element)
        {
            var connection = RetrieveColumnFamilyConnection();
            var nowTicks = globalTime.UpdateNowTicks();
            connection.AddColumn(id, new Column
                {
                    Name = dataColumnName,
                    Timestamp = nowTicks,
                    Value = serializer.Serialize(element)
                });

            return BlobWriteResult.Success;
        }

        public IBlobsWriteResult Write([NotNull] IEnumerable<KeyValuePair<string, T>> elements)
        {
            var connection = RetrieveColumnFamilyConnection();
            var nowTicks = globalTime.UpdateNowTicks();
            connection.BatchInsert(elements.Select(x => new KeyValuePair<string, IEnumerable<Column>>(
                                                            x.Key,
                                                            new[]
                                                                {
                                                                    new Column
                                                                        {
                                                                            Name = dataColumnName,
                                                                            Timestamp = nowTicks,
                                                                            Value = serializer.Serialize(x.Value)
                                                                        }
                                                                })));
            return SuccessBlobsWriteResult.Instance;
        }

        public T Read([NotNull] string id)
        {
            var connection = RetrieveColumnFamilyConnection();
            Column column;
            if(connection.TryGetColumn(id, dataColumnName, out column))
                return serializer.Deserialize<T>(column.Value);
            return default(T);
        }

        public Dictionary<string, T> Read([NotNull] IEnumerable<string> ids)
        {
            return ids
                .Batch(1000, Enumerable.ToArray)
                .SelectMany(batch => MakeInConnection(connection => connection.GetRows(batch, new[] {dataColumnName})))
                .Where(x => x.Value != null && x.Value.Length > 0)
                .DistinctBy(x => x.Key)
                .Select(pair => new KeyValuePair<string, T>(pair.Key, serializer.Deserialize<T>(pair.Value.First().Value)))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public IEnumerable<T> ReadAll(int batchSize = 1000)
        {
            var connection = RetrieveColumnFamilyConnection();
            var keys = connection.GetKeys(batchSize);
            return TryReadInternal(keys.ToArray());
        }

        public IEnumerable<KeyValuePair<string, T>> ReadAllWithIds(int batchSize = 1000)
        {
            var connection = RetrieveColumnFamilyConnection();
            string exclusiveStartKey = null;
            while(true)
            {
                var keys = connection.GetKeys(exclusiveStartKey, batchSize);
                if(keys.Length == 0)
                    yield break;
                exclusiveStartKey = keys.Last();
                var objects = TryReadInternalWithIds(keys);
                foreach(var @object in objects)
                    yield return @object;
            }
        }

        public void Delete([NotNull] string id, long timestamp)
        {
            MakeInConnection(connection =>
                {
                    var columns = connection.GetColumns(id, null, maximalColumnsCount);
                    connection.DeleteBatch(id, columns.Select(col => col.Name), timestamp);
                });
        }

        public void Delete([NotNull] IEnumerable<string> ids, long? timestamp)
        {
            ids.Batch(1000, Enumerable.ToArray).ForEach(x => DeleteInternal(x, timestamp));
        }

        private T[] TryReadInternal([NotNull] string[] ids)
        {
            if(ids.Length == 0)
                return new T[0];

            return ids
                .Batch(1000, Enumerable.ToArray)
                .SelectMany(batch => MakeInConnection(connection => connection.GetRows(batch, new[] {dataColumnName})))
                .Where(x => x.Value != null && x.Value.Length > 0)
                .DistinctBy(x => x.Key)
                .Select(x => x.Value.First())
                .Select(x => serializer.Deserialize<T>(x.Value))
                .ToArray();
        }

        private KeyValuePair<string, T>[] TryReadInternalWithIds([NotNull] string[] ids)
        {
            if(ids.Length == 0) return new KeyValuePair<string, T>[0];
            var rows = new List<KeyValuePair<string, Column[]>>();
            ids
                .Batch(1000, Enumerable.ToArray)
                .ForEach(batchIds => MakeInConnection(connection => rows.AddRange(connection.GetRows(batchIds, new[] {dataColumnName}))));
            var rowsDict = rows.ToDictionary(row => row.Key);

            return ids.Where(rowsDict.ContainsKey)
                      .Select(id => new KeyValuePair<string, T>(id, Read(rowsDict[id].Value)))
                      .Where(obj => obj.Value != null).ToArray();
        }

        private T Read(IEnumerable<Column> columns)
        {
            return columns.Where(column => column.Name == dataColumnName).Select(column => serializer.Deserialize<T>(column.Value)).FirstOrDefault();
        }

        private void DeleteInternal(string[] ids, long? timestamp)
        {
            MakeInConnection(connection => connection.DeleteRows(ids, timestamp));
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

        private const int maximalColumnsCount = 1000;
        private const string dataColumnName = "Data";
        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
    }
}