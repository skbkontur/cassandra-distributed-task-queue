using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using JetBrains.Annotations;

using MoreLinq;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;

using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.Objects;

using Vostok.Logging.Abstractions;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class TimeBasedBlobStorage
    {
        public TimeBasedBlobStorage(TimeBasedBlobStorageSettings settings, ICassandraCluster cassandraCluster, ILog logger)
        {
            this.settings = settings;
            this.cassandraCluster = cassandraCluster;
            this.logger = logger.ForContext(nameof(TimeBasedBlobStorage));
        }

        [NotNull]
        public static BlobId GenerateNewBlobId(int blobSize)
        {
            var id = TimeGuid.NowGuid();
            return new BlobId(id, blobSize > TimeBasedBlobStorageSettings.MaxRegularBlobSize ? BlobType.Large : BlobType.Regular);
        }

        public void Write([NotNull] BlobId id, [NotNull] byte[] value, long timestamp, TimeSpan? ttl)
        {
            if (value == null)
                throw new InvalidProgramStateException(string.Format("value is NULL for id: {0}", id));
            if (id.Type == BlobType.Regular && value.Length > TimeBasedBlobStorageSettings.MaxRegularBlobSize)
                logger.Error(string.Format("Writing large blob with id={0} of size={1} into time-based cf: {2}", id.Id, value.Length, settings.RegularBlobsCfName));
            if (value.Length > TimeBasedBlobStorageSettings.MaxBlobSize)
                logger.Warn(string.Format("Writing extra large blob with id={0} of size={1} into time-based cf: {2}", id.Id, value.Length, settings.LargeBlobsCfName));
            var columnAddress = GetColumnAddress(id);
            var connection = cassandraCluster.RetrieveColumnFamilyConnection(settings.KeyspaceName, columnAddress.CfName);
            connection.AddColumn(columnAddress.RowKey, new Column
                {
                    Name = columnAddress.ColumnName,
                    Value = value,
                    Timestamp = timestamp,
                    TTL = ttl.HasValue ? (int)ttl.Value.TotalSeconds : (int?)null,
                });
        }

        public void Delete([NotNull] BlobId id, long timestamp)
        {
            var columnAddress = GetColumnAddress(id);
            var connection = cassandraCluster.RetrieveColumnFamilyConnection(settings.KeyspaceName, columnAddress.CfName);
            connection.DeleteColumn(columnAddress.RowKey, columnAddress.ColumnName, timestamp);
        }

        [CanBeNull]
        public byte[] Read([NotNull] BlobId id)
        {
            var columnAddress = GetColumnAddress(id);
            var connection = cassandraCluster.RetrieveColumnFamilyConnection(settings.KeyspaceName, columnAddress.CfName);
            Column column;
            if (!connection.TryGetColumn(columnAddress.RowKey, columnAddress.ColumnName, out column) || column.Value == null)
                return null;
            return column.Value;
        }

        /// <remarks>
        ///     Result does NOT contain entries for non existing blobs
        /// </remarks>
        [NotNull]
        public Dictionary<BlobId, byte[]> Read([NotNull] BlobId[] ids)
        {
            var distinctIds = ids.DistinctBy(x => x.Id).ToArray();
            return ReadRegular(distinctIds, defaultBatchSize)
                   .Concat(ReadLarge(distinctIds))
                   .Where(x => x.Column.Value != null)
                   .ToDictionary(x => x.BlobId, x => x.Column.Value);
        }

        [NotNull, ItemNotNull]
        private IEnumerable<ColumnWithId> ReadRegular([NotNull] BlobId[] ids, int batchSize)
        {
            var connection = cassandraCluster.RetrieveColumnFamilyConnection(settings.KeyspaceName, settings.RegularBlobsCfName);
            return ids.Where(x => x.Type == BlobType.Regular)
                      .OrderBy(blobId => blobId.Id)
                      .Select(blobId => new {BlobId = blobId, ColumnAddress = GetColumnAddress(blobId)})
                      .GroupBy(x => x.ColumnAddress.RowKey)
                      .SelectMany(gByRow => gByRow.Batch(batchSize, Enumerable.ToArray)
                                                  .SelectMany(batch =>
                                                      {
                                                          var columnNameToBlobIdMap = batch.ToDictionary(x => x.ColumnAddress.ColumnName, x => x.BlobId);
                                                          var columns = connection.GetColumns(gByRow.Key, columnNameToBlobIdMap.Keys.ToArray());
                                                          return columns.Select(column => new ColumnWithId {BlobId = columnNameToBlobIdMap[column.Name], Column = column});
                                                      }));
        }

        [NotNull, ItemNotNull]
        private IEnumerable<ColumnWithId> ReadLarge([NotNull] BlobId[] blobIds)
        {
            var connection = cassandraCluster.RetrieveColumnFamilyConnection(settings.KeyspaceName, settings.LargeBlobsCfName);
            return blobIds.Where(x => x.Type == BlobType.Large)
                          .Select(blobId =>
                              {
                                  var columnAddress = GetColumnAddress(blobId);
                                  if (!connection.TryGetColumn(columnAddress.RowKey, columnAddress.ColumnName, out var column))
                                      return null;
                                  return new ColumnWithId {BlobId = blobId, Column = column};
                              })
                          .Where(x => x != null);
        }

        [NotNull]
        public IEnumerable<Tuple<BlobId, byte[]>> ReadAll(int batchSize)
        {
            return ReadAllRegular(batchSize).Concat(ReadAllLarge(batchSize));
        }

        [NotNull]
        private IEnumerable<Tuple<BlobId, byte[]>> ReadAllRegular(int batchSize)
        {
            var connection = cassandraCluster.RetrieveColumnFamilyConnection(settings.KeyspaceName, settings.RegularBlobsCfName);
            string exclusiveStartKey = null;
            while (true)
            {
                var keys = connection.GetKeys(exclusiveStartKey, count : batchSize);
                if (keys.Length == 0)
                    yield break;
                foreach (var key in keys)
                {
                    string exclusiveStartColumnName = null;
                    while (true)
                    {
                        var columns = connection.GetColumns(key, exclusiveStartColumnName, count : batchSize);
                        if (columns.Length == 0)
                            break;
                        foreach (var column in columns)
                        {
                            if (column.Value != null)
                                yield return Tuple.Create(new BlobId(GetTimeGuidFromColumnName(column.Name), BlobType.Regular), column.Value);
                        }
                        exclusiveStartColumnName = columns.Last().Name;
                    }
                }
                exclusiveStartKey = keys.Last();
            }
        }

        [NotNull]
        private IEnumerable<Tuple<BlobId, byte[]>> ReadAllLarge(int batchSize)
        {
            var connection = cassandraCluster.RetrieveColumnFamilyConnection(settings.KeyspaceName, settings.LargeBlobsCfName);
            string exclusiveStartKey = null;
            while (true)
            {
                var keys = connection.GetKeys(exclusiveStartKey, count : batchSize);
                if (keys.Length == 0)
                    yield break;
                var blobIds = keys.Select(x => new BlobId(GetTimeGuidFromRowKey(x), BlobType.Large)).ToArray();
                foreach (var columnWithId in ReadLarge(blobIds))
                {
                    if (columnWithId.Column.Value != null)
                        yield return Tuple.Create(columnWithId.BlobId, columnWithId.Column.Value);
                }
                exclusiveStartKey = keys.Last();
            }
        }

        [NotNull]
        private ColumnAddress GetColumnAddress([NotNull] BlobId id)
        {
            var timeGuid = id.Id;
            switch (id.Type)
            {
            case BlobType.Regular:
                var ticks = timeGuid.GetTimestamp().Ticks;
                return new ColumnAddress
                    {
                        CfName = settings.RegularBlobsCfName,
                        RowKey = string.Format("{0}_{1}", ticks / TimeBasedBlobStorageSettings.TickPartition, ShardingHelpers.GetShard(timeGuid.GetHashCode(), TimeBasedBlobStorageSettings.SplittingFactor)),
                        ColumnName = string.Format("{0}_{1}", ticks.ToString("D20", CultureInfo.InvariantCulture), timeGuid.ToGuid()),
                    };
            case BlobType.Large:
                return new ColumnAddress
                    {
                        CfName = settings.LargeBlobsCfName,
                        RowKey = timeGuid.ToGuid().ToString(),
                        ColumnName = largeBlobColumnName,
                    };
            default:
                throw new InvalidProgramStateException(string.Format("Invalid BlobType in id: {0}", id));
            }
        }

        [NotNull]
        private static TimeGuid GetTimeGuidFromColumnName([NotNull] string columnName)
        {
            TimeGuid timeGuid;
            if (!TimeGuid.TryParse(columnName.Split('_')[1], out timeGuid))
                throw new InvalidProgramStateException(string.Format("Invalid regular column name: {0}", columnName));
            return timeGuid;
        }

        [NotNull]
        private static TimeGuid GetTimeGuidFromRowKey([NotNull] string rowKey)
        {
            TimeGuid timeGuid;
            if (!TimeGuid.TryParse(rowKey, out timeGuid))
                throw new InvalidProgramStateException(string.Format("Invalid rowKey: {0}", rowKey));
            return timeGuid;
        }

        private const int defaultBatchSize = 1000;
        private const string largeBlobColumnName = "Data";

        private readonly TimeBasedBlobStorageSettings settings;
        private readonly ICassandraCluster cassandraCluster;
        private readonly ILog logger;

        private class ColumnAddress
        {
            public string CfName { get; set; }
            public string RowKey { get; set; }
            public string ColumnName { get; set; }
        }

        private class ColumnWithId
        {
            public BlobId BlobId { get; set; }
            public Column Column { get; set; }
        }
    }
}