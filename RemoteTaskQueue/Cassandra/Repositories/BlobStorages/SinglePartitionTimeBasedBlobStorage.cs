using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using JetBrains.Annotations;

using SkbKontur.Cassandra.ThriftClient.Abstractions;
using SkbKontur.Cassandra.ThriftClient.Clusters;
using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.Objects;

using Vostok.Logging.Abstractions;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class SinglePartitionTimeBasedBlobStorage
    {
        public SinglePartitionTimeBasedBlobStorage([NotNull] string keyspaceName,
                                                   [NotNull] string columnFamilyName,
                                                   ICassandraCluster cassandraCluster,
                                                   ILog logger)
        {
            this.keyspaceName = keyspaceName;
            this.columnFamilyName = columnFamilyName;
            this.cassandraCluster = cassandraCluster;
            this.logger = logger.ForContext(nameof(SinglePartitionTimeBasedBlobStorage));
        }

        public void Write([NotNull] string rowKey, [NotNull] TimeGuid columnId, [NotNull] byte[] value, long timestamp, TimeSpan? ttl)
        {
            if (value == null)
                throw new InvalidProgramStateException(string.Format("value is NULL for id: {0}", columnId));
            if (value.Length > TimeBasedBlobStorageSettings.MaxBlobSize)
                logger.Warn(string.Format("Writing extra large blob with rowKey={0} and columnId={1} of size={2} into cf: {3}.{4}", rowKey, columnId, value.Length, keyspaceName, columnFamilyName));
            var connection = cassandraCluster.RetrieveColumnFamilyConnection(keyspaceName, columnFamilyName);
            connection.AddColumn(rowKey, new Column
                {
                    Name = FormatColumnName(columnId),
                    Value = value,
                    Timestamp = timestamp,
                    TTL = ttl.HasValue ? (int)ttl.Value.TotalSeconds : (int?)null,
                });
        }

        public void Delete([NotNull] string rowKey, [NotNull] TimeGuid columnId, long timestamp)
        {
            var connection = cassandraCluster.RetrieveColumnFamilyConnection(keyspaceName, columnFamilyName);
            connection.DeleteColumn(rowKey, FormatColumnName(columnId), timestamp);
        }

        [CanBeNull]
        public byte[] Read([NotNull] string rowKey, [NotNull] TimeGuid columnId)
        {
            var connection = cassandraCluster.RetrieveColumnFamilyConnection(keyspaceName, columnFamilyName);
            Column column;
            if (!connection.TryGetColumn(rowKey, FormatColumnName(columnId), out column) || column.Value == null)
                return null;
            return column.Value;
        }

        /// <remarks>
        ///     Result does NOT contain entries for non existing blobs
        /// </remarks>
        [NotNull]
        public Dictionary<TimeGuid, byte[]> Read([NotNull] string rowKey, [NotNull] TimeGuid[] columnIds)
        {
            var columnNameToIdMap = columnIds.Distinct().ToDictionary(FormatColumnName, x => x);
            var connection = cassandraCluster.RetrieveColumnFamilyConnection(keyspaceName, columnFamilyName);
            var columns = connection.GetColumns(rowKey, columnNameToIdMap.Keys.ToArray());
            return columns.Select(x => new ColumnWithId {ColumnId = columnNameToIdMap[x.Name], Column = x})
                          .Where(x => x.Column.Value != null)
                          .ToDictionary(x => x.ColumnId, x => x.Column.Value);
        }

        [NotNull]
        private static string FormatColumnName([NotNull] TimeGuid columnId)
        {
            return string.Format("{0}_{1}", columnId.GetTimestamp().Ticks.ToString("D20", CultureInfo.InvariantCulture), columnId.ToGuid());
        }

        private readonly string keyspaceName;
        private readonly string columnFamilyName;
        private readonly ICassandraCluster cassandraCluster;
        private readonly ILog logger;

        private class ColumnWithId
        {
            public TimeGuid ColumnId { get; set; }
            public Column Column { get; set; }
        }
    }
}