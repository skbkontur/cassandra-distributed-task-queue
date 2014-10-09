using System;
using System.Collections.Generic;
using System.Linq;

using GroBuf;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
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
            long nowTicks = globalTime.UpdateNowTicks();
            if(element is TaskMetaInformation)
                (element as TaskMetaInformation).LastModificationTicksFuckup = nowTicks;
            connection.AddColumn(id, new Column
                {
                    Name = dataColumnName,
                    Timestamp = nowTicks,
                    Value = serializer.Serialize(element)
                });
        }

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
            if(ids.Length == 0)
                return new T[0];
            return TryReadInternal(ids);
        }

        public T[] ReadQuiet(string[] cassandraIds)
        {
            CheckObjectIdentitiesValidness(cassandraIds);

            var rows = new List<KeyValuePair<string, Column[]>>();
            cassandraIds.Distinct()
                        .Batch(1000, Enumerable.ToArray)
                        .ForEach(batchIds => MakeInConnection(connection => rows.AddRange(connection.GetRowsExclusive(batchIds, null, 1000))));

            var rowsDict = rows.ToDictionary(row => row.Key);
            var result = new T[cassandraIds.Length];
            for(var i = 0; i < cassandraIds.Length; i++)
            {
                var id = cassandraIds[i];
                KeyValuePair<string, Column[]> row;
                if(rowsDict.TryGetValue(id, out row))
                    result[i] = Read(row.Value);
            }
            return result;
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

        private T[] TryReadInternal(string[] ids)
        {
            if(ids == null) throw new ArgumentNullException("ids");
            if(ids.Length == 0) return new T[0];
            var rows = new List<KeyValuePair<string, Column[]>>();
            ids
                .Batch(1000, Enumerable.ToArray)
                .ForEach(batchIds => MakeInConnection(connection => rows.AddRange(connection.GetRowsExclusive(batchIds, null, 1000))));
            var rowsDict = rows.ToDictionary(row => row.Key);

            return ids.Where(rowsDict.ContainsKey)
                      .Select(id => Read(rowsDict[id].Value))
                      .Where(obj => obj != null).ToArray();
        }

        private KeyValuePair<string, T>[] TryReadInternalWithIds(string[] ids)
        {
            if(ids == null) throw new ArgumentNullException("ids");
            if(ids.Length == 0) return new KeyValuePair<string, T>[0];
            var rows = new List<KeyValuePair<string, Column[]>>();
            ids
                .Batch(1000, Enumerable.ToArray)
                .ForEach(batchIds => MakeInConnection(connection => rows.AddRange(connection.GetRowsExclusive(batchIds, null, 1000))));
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