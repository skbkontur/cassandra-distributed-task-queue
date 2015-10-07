using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes
{
    public class ColumnInfo
    {
        public ColumnInfo([NotNull] string rowKey, [NotNull] string columnName)
        {
            RowKey = rowKey;
            ColumnName = columnName;
        }

        [NotNull]
        public string RowKey { get; private set; }

        [NotNull]
        public string ColumnName { get; private set; }
    }
}