using System;
using System.Collections.Generic;
using System.Linq;

using GroBuf;

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

        public void Write(string id, T element)
        {
            var connection = RetrieveColumnFamilyConnection();
            connection.AddColumn(id, new Column
                {
                    Name = dataColumnName,
                    Timestamp = globalTime.UpdateNowTicks(),
                    Value = serializer.Serialize(element)
                });
        }

        public T Read(string id)
        {
            var connection = RetrieveColumnFamilyConnection();
            Column column;
            if(connection.TryGetColumn(id, dataColumnName, out column))
                return serializer.Deserialize<T>(column.Value);
            return default(T);
        }

        public T[] Read(string[] ids)
        {
            if (ids.Length == 0)
                return new T[0];
            return TryReadInternal(ids);
        }

        public IEnumerable<T> ReadAll(int batchSize = 1000)
        {
            var connection = RetrieveColumnFamilyConnection();
            var keys = connection.GetKeys(batchSize);
            return TryReadInternal(keys.ToArray());
        }

        public void Delete(string id, long timestamp)
        {
            CheckObjectIdentityValidness(id);

            MakeInConnection(connection =>
                {
                    var columns = connection.GetColumns(id, null, maximalColumnsCount);
                    connection.DeleteBatch(id, columns.Select(col => col.Name), timestamp);
                });
        }

        public void Delete(string[] ids, long? timestamp)
        {
            CheckObjectIdentitiesValidness(ids);
            ids.Batch(1000, Enumerable.ToArray).ForEach(x => DeleteInternal(x, timestamp));
        }

        [Obsolete("для конвертаций")]
        public void Write(KeyValuePair<string, T>[] elements)
        {
            var connection = RetrieveColumnFamilyConnection();
            var updateNowTicks = globalTime.UpdateNowTicks();
            connection.BatchInsert(elements.Select(x => new KeyValuePair<string, IEnumerable<Column>>(
                                                            x.Key,
                                                            new[]
                                                                {
                                                                    new Column
                                                                        {
                                                                            Name = dataColumnName,
                                                                            Timestamp = updateNowTicks,
                                                                            Value = serializer.Serialize(x.Value)
                                                                        }
                                                                })));
        }

        private T[] TryReadInternal(string[] ids)
        {
            if (ids == null) throw new ArgumentNullException("ids");
            if (ids.Length == 0) return new T[0];
            var rows = new List<KeyValuePair<string, Column[]>>();
            ids
                .Batch(1000, Enumerable.ToArray)
                .ForEach(batchIds => MakeInConnection(connection => rows.AddRange(connection.GetRowsExclusive(batchIds, null, 1000))));
            var rowsDict = rows.ToDictionary(row => row.Key);

            return ids.Where(rowsDict.ContainsKey)
                .Select(id => Read(rowsDict[id].Value))
                .Where(obj => obj != null).ToArray();
        }

        private T Read(IEnumerable<Column> columns)
        {
            return columns.Where(column => column.Name == dataColumnName).Select(column => serializer.Deserialize<T>(column.Value)).FirstOrDefault();
        }

        private void DeleteInternal(string[] ids, long? timestamp)
        {
            MakeInConnection(
                connection => connection.DeleteRows(ids, timestamp.HasValue ? timestamp.Value : (long?)null));
        }

        private void MakeInConnection(Action<IColumnFamilyConnection> action)
        {
            var connection = RetrieveColumnFamilyConnection();
            action(connection);
        }

        private static void CheckObjectIdentityValidness(string id)
        {
            if(id == null)
                throw new ArgumentNullException("id");
        }

        private static void CheckObjectIdentitiesValidness(string[] ids)
        {
            if(ids == null)
                throw new ArgumentNullException("ids");
        }

        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
        private const int maximalColumnsCount = 1000;
        private const string dataColumnName = "Data";
    }
}