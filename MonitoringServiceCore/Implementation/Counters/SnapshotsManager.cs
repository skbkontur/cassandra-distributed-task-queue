using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

using log4net;

using SKBKontur.Catalogue.Core.SQL;
using SKBKontur.Catalogue.Expressions.ExpressionTrees;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters
{
    public class SnapshotsManager
    {
        public SnapshotsManager(MetaProvider metaProvider, IProcessedTasksCounter processedTasksCounter, ISqlDatabase sqlDatabase, SnapshotConverter snapshotConverter)
        {
            this.metaProvider = metaProvider;
            this.processedTasksCounter = processedTasksCounter;
            this.sqlDatabase = sqlDatabase;
            this.snapshotConverter = snapshotConverter;
        }

        public void Init()
        {
            ActualizeDatabaseScheme();
        }

        public void LoadSnapshot()
        {
            logger.InfoFormat("LoadSnapshot begin");
            int version;
            var bytes = ReadOrNull(out version);
            InternalSnapshot internalSnapshot;
            var isBad = false;
            if(bytes == null)
            {
                logger.WarnFormat("No snapshot found");
                internalSnapshot = snapshotConverter.emptyOldSnapshot;
            }
            else
            {
                internalSnapshot = snapshotConverter.ConvertFromBytes(version, bytes);
                isBad = IsSnapshotBad(internalSnapshot);
                logger.InfoFormat("Snapshot check: isBad={0}", isBad);
            }
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
                logger.WarnFormat("SaveSnapshot: MetaProvider snapshot is BIG - do nothing");
                return;
            }
            if(counterSnapshot == null)
            {
                logger.WarnFormat("SaveSnapshot: ProcessedTasksCounter snapshot is BIG - do nothing");
                return;
            }
            var internalSnapshot = new InternalSnapshot(metaProviderSnapshot, counterSnapshot);

            SaveBytes(snapshotConverter.ConvertToBytes(internalSnapshot));

            logger.InfoFormat("SaveSnapshot: NotFinishedTasks={1} NotReadEvents={2} ReadEvents={3} time={0} done",
                              DateTimeFormatter.FormatWithMsAndTicks(metaProviderSnapshot.LastUpdateTicks),
                              SafeCollectionLength(counterSnapshot.NotFinishedTasks),
                              SafeDictionaryCount(metaProviderSnapshot.NotReadEvents),
                              SafeDictionaryCount(metaProviderSnapshot.ReadEvents));
        }

        private void ActualizeDatabaseScheme()
        {
            sqlDatabase.CreateTableWithIndexes(tableName, columnDefinitions);
            var columns = sqlDatabase.GetColumns(tableName);
            foreach(var columnDefinition in columnDefinitions)
            {
                if(!columns.Contains(columnDefinition.Name))
                {
                    logger.InfoFormat("Init: add missing column '{0}'", columnDefinition.Name);
                    sqlDatabase.AddColumns(tableName, new[] {columnDefinition});
                }
            }
        }

        private static int SafeDictionaryCount(IDictionary dictionary)
        {
            if(dictionary == null)
                return 0;
            return dictionary.Count;
        }

        private static int SafeCollectionLength<T>(ICollection<T> arr)
        {
            if(arr == null)
                return 0;
            return arr.Count;
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
            return internalSnapshot.CounterSnapshot == null || internalSnapshot.MetaProviderSnapshot == null;
        }

        private byte[] ReadOrNull(out int version)
        {
            version = 0;
            var selectQuery = new SqlSelectQuery
                {
                    TableName = tableName,
                    Criterion = ExpressionTree.Constant(true),
                    NeedColumns = new[] {hashColumn, dataColumn, versionColumn},
                };
            var rows = sqlDatabase.GetRows(selectQuery);

            if(rows.Count <= 0 || rows[0].Length < 3)
                return null;
            if(rows.Count > 1)
                logger.WarnFormat("Trash in table - many rows.");
            var hash = ExtractBytes(rows[0][0]);
            var data = ExtractBytes(rows[0][1]);
            version = ExtractInt(rows[0][2]);
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

        private static int ExtractInt(object o)
        {
            if (ReferenceEquals(o, DBNull.Value) || o == null)
                return 0;
            return (int)o;
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
                            new KeyValuePair<string, object>(versionColumn, SnapshotConverter.CurrentVersion),
                            new KeyValuePair<string, object>(hashColumn, GetHash(data)),
                            new KeyValuePair<string, object>(dataColumn, data),
                        }
                });
        }

        private static byte[] GetHash(byte[] data)
        {
            return new MD5CryptoServiceProvider().ComputeHash(data);
        }

        private readonly SqlColumn[] columnDefinitions = new SqlColumn[]
            {
                new SqlColumn
                    {
                        DataType = SqlDataType.LongBlob, Name = hashColumn,
                    },
                new SqlColumn
                    {
                        DataType = SqlDataType.LongBlob, Name = dataColumn,
                    },
                new SqlColumn()
                    {
                        DataType = SqlDataType.Integer,
                        Name = versionColumn
                    },
            };

        private readonly MetaProvider metaProvider;
        private readonly IProcessedTasksCounter processedTasksCounter;
        private readonly ISqlDatabase sqlDatabase;
        private readonly SnapshotConverter snapshotConverter;

        private static readonly ILog logger = LogManager.GetLogger("SnapshotsManager");

        private const string dataColumn = "Data";
        private const string hashColumn = "Hash";
        private const string versionColumn = "Version";
        private const string tableName = "Snapshots";
    }
}