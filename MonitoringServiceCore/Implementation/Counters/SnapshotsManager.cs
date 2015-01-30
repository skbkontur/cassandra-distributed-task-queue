using System;
using System.Collections.Generic;
using System.Security.Cryptography;

using GroBuf;

using log4net;

using SKBKontur.Catalogue.Core.SQL;
using SKBKontur.Catalogue.Expressions.ExpressionTrees;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters
{
    public class SnapshotsManager
    {
        public SnapshotsManager(MetaProvider metaProvider, IProcessedTasksCounter processedTasksCounter, ISqlDatabase sqlDatabase, ISerializer serializer)
        {
            this.metaProvider = metaProvider;
            this.processedTasksCounter = processedTasksCounter;
            this.sqlDatabase = sqlDatabase;
            this.serializer = serializer;
        }

        public void Init()
        {
            sqlDatabase.CreateTableWithIndexes(tableName, new[]
                {
                    new SqlColumn()
                        {
                            DataType = SqlDataType.LongBlob, Name = hashColumn,
                        },
                    new SqlColumn()
                        {
                            DataType = SqlDataType.LongBlob, Name = dataColumn,
                        },
                });
        }

        public void LoadSnapshot()
        {
            logger.InfoFormat("LoadSnapshot begin");
            var bytes = ReadOrNull();
            if(bytes == null)
            {
                logger.InfoFormat("No snapshot found");
                return;
            }
            var internalSnapshot = serializer.Deserialize<InternalSnapshot>(bytes);
            var isBad = IsSnapshotBad(internalSnapshot);
            logger.InfoFormat("Snapshot found. version={0}. isBad={1}", internalSnapshot.Version, isBad);
            if(!isBad)
                LoadSnapshots(internalSnapshot);

            logger.InfoFormat("LoadSnapshot end");
        }

        public void SaveSnapshot()
        {
            var metaProviderSnapshot = metaProvider.GetSnapshotOrNull(CounterSettings.MaxStoredSnapshotLength);
            var counterSnapshot = processedTasksCounter.GetSnapshotOrNull(CounterSettings.MaxStoredSnapshotLength);
            if(metaProviderSnapshot == null)
            {
                logger.WarnFormat("MetaProvider snapshot is BIG - do nothing");
                return;
            }
            if(counterSnapshot == null)
            {
                logger.WarnFormat("ProcessedTasksCounter snapshot is BIG - do nothing");
                return;
            }
            var internalSnapshot = new InternalSnapshot(version, metaProviderSnapshot, counterSnapshot);
            SaveBytes(serializer.Serialize(internalSnapshot));
            logger.InfoFormat("SaveSnapshot time={0} ok", DateTimeFormatter.FormatWithMsAndTicks(metaProviderSnapshot.LastUpdateTicks));
        }

        private void LoadSnapshots(InternalSnapshot internalSnapshot)
        {
            metaProvider.Stop();
            try
            {
                metaProvider.LoadSnapshot(internalSnapshot.MetaProviderSnapshot, MetaProvider.GetMaxHistoryDepthTicks());
                processedTasksCounter.LoadSnapshot(internalSnapshot.CounterSnapshot);
            }
            finally
            {
                metaProvider.Start();
            }
        }

        private static bool IsSnapshotBad(InternalSnapshot internalSnapshot)
        {
            return internalSnapshot.Version != version || internalSnapshot.CounterSnapshot == null || internalSnapshot.MetaProviderSnapshot == null;
        }

        private byte[] ReadOrNull()
        {
            var selectQuery = new SqlSelectQuery
                {
                    TableName = tableName,
                    Criterion = ExpressionTree.Constant(true),
                    NeedColumns = new[] {hashColumn, dataColumn},
                };
            var rows = sqlDatabase.GetRows(selectQuery);

            if(rows.Count <= 0 || rows[0].Length < 2)
                return null;
            if(rows.Count > 1)
                logger.WarnFormat("Trash in table - many rows.");
            var hash = ExtractBytes(rows[0][0]);
            var data = ExtractBytes(rows[0][1]);
            var actualHash = GetHash(data);
            var readHash = Convert.ToBase64String(hash);
            var actualHashStr = Convert.ToBase64String(actualHash);
            if(readHash != actualHashStr)
            {
                logger.WarnFormat("Snapshot Currupted. expected hash {0} but was {1}", readHash, actualHashStr);
                return null;
            }
            return data;
        }

        private static byte[] ExtractBytes(object o)
        {
            if(ReferenceEquals(o, DBNull.Value))
                return null;
            return (byte[])o;
        }

        private void SaveBytes(byte[] data)
        {
            sqlDatabase.ReplaceRows(tableName, new[]
                {
                    new[]
                        {
                            new KeyValuePair<string, object>(SqlConstants.SqlIdColumnName, "1"),
                            new KeyValuePair<string, object>(hashColumn, GetHash(data)),
                            new KeyValuePair<string, object>(dataColumn, data),
                        }
                });
        }

        private static byte[] GetHash(byte[] data)
        {
            return new MD5CryptoServiceProvider().ComputeHash(data);
        }

        private readonly MetaProvider metaProvider;
        private readonly IProcessedTasksCounter processedTasksCounter;
        private readonly ISqlDatabase sqlDatabase;
        private readonly ISerializer serializer;

        private readonly ILog logger = LogManager.GetLogger("SnapshotsManager");

        private class InternalSnapshot
        {
            public InternalSnapshot(int version, MetaProvider.MetaProviderSnapshot metaProviderSnapshot, ProcessedTasksCounter.CounterSnapshot counterSnapshot)
            {
                Version = version;
                MetaProviderSnapshot = metaProviderSnapshot;
                CounterSnapshot = counterSnapshot;
            }

            public int Version { get; set; }
            public MetaProvider.MetaProviderSnapshot MetaProviderSnapshot { get; set; }
            public ProcessedTasksCounter.CounterSnapshot CounterSnapshot { get; set; }
        }

        private const string dataColumn = "Data";
        private const string hashColumn = "Hash";
        private const string tableName = "Snapshots";

        private const int version = 1;
    }
}