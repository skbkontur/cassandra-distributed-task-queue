using System;
using System.Collections.Generic;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using MoreLinq;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class LegacyBlobStorage<T>
    {
        public LegacyBlobStorage(ICassandraCluster cassandraCluster, ISerializer serializer, string keyspaceName, string columnFamilyName, TimeSpan ttl)
        {
            this.cassandraCluster = cassandraCluster;
            this.serializer = serializer;
            this.keyspaceName = keyspaceName;
            this.columnFamilyName = columnFamilyName;
            this.ttl = ttl;
        }

        public void Write([NotNull] string id, [NotNull] T element, long timestamp)
        {
            var connection = RetrieveColumnFamilyConnection();
            connection.AddColumn(id, new Column
                {
                    Name = dataColumnName,
                    Timestamp = timestamp,
                    Value = serializer.Serialize(element),
                    TTL = (int) ttl.TotalSeconds,
                });
        }

        [CanBeNull]
        public T Read([NotNull] string id)
        {
            var connection = RetrieveColumnFamilyConnection();
            Column column;
            if(connection.TryGetColumn(id, dataColumnName, out column))
                return serializer.Deserialize<T>(column.Value);
            return default(T);
        }

        /// <remarks>
        ///     Result does NOT contain entries for non existing blobs
        /// </remarks>
        [NotNull]
        public Dictionary<string, T> Read([NotNull] List<string> ids)
        {
            return ids
                .Distinct()
                .Batch(1000, Enumerable.ToArray)
                .SelectMany(batch => MakeInConnection(connection => connection.GetRows(batch, new[] {dataColumnName})))
                .Where(x => x.Value != null && x.Value.Length > 0)
                .ToDictionary(pair => pair.Key, pair => serializer.Deserialize<T>(pair.Value.First().Value));
        }

        [NotNull]
        public IEnumerable<Tuple<string, T>> ReadAll(int batchSize)
        {
            var connection = RetrieveColumnFamilyConnection();
            string exclusiveStartKey = null;
            while(true)
            {
                var keys = connection.GetKeys(exclusiveStartKey, batchSize);
                if(keys.Length == 0)
                    yield break;
                exclusiveStartKey = keys.Last();
                var objects = TryReadInternal(keys);
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

        [NotNull]
        private IEnumerable<Tuple<string, T>> TryReadInternal([NotNull] string[] ids)
        {
            if(ids.Length == 0) return new Tuple<string, T>[0];
            var rows = new List<KeyValuePair<string, Column[]>>();
            ids
                .Batch(1000, Enumerable.ToArray)
                .ForEach(batchIds => MakeInConnection(connection => rows.AddRange(connection.GetRows(batchIds, new[] {dataColumnName}))));
            var rowsDict = rows.ToDictionary(row => row.Key);

            return ids.Where(rowsDict.ContainsKey)
                      .Select(id => Tuple.Create(id, Read(rowsDict[id].Value)))
                      .Where(obj => obj.Item2 != null);
        }

        [CanBeNull]
        private T Read(IEnumerable<Column> columns)
        {
            return columns.Where(column => column.Name == dataColumnName).Select(column => serializer.Deserialize<T>(column.Value)).FirstOrDefault();
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

        private IColumnFamilyConnection RetrieveColumnFamilyConnection()
        {
            return cassandraCluster.RetrieveColumnFamilyConnection(keyspaceName, columnFamilyName);
        }

        private const int maximalColumnsCount = 1000;
        private const string dataColumnName = "Data";
        private readonly ICassandraCluster cassandraCluster;
        private readonly ISerializer serializer;
        private readonly string keyspaceName;
        private readonly string columnFamilyName;
        private readonly TimeSpan ttl;
    }
}