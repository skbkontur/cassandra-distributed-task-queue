using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes
{
    public class TaskColumnInfo
    {
        public TaskColumnInfo([NotNull] string rowKey, [NotNull] string columnName)
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