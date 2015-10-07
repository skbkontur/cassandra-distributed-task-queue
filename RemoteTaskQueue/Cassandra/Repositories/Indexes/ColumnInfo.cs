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

        public bool Equals(ColumnInfo other)
        {
            if(ReferenceEquals(null, other)) return false;
            if(ReferenceEquals(this, other)) return true;
            return Equals(other.RowKey, RowKey) && Equals(other.ColumnName, ColumnName);
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj)) return false;
            if(ReferenceEquals(this, obj)) return true;
            if(obj.GetType() != typeof(ColumnInfo)) return false;
            return Equals((ColumnInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (RowKey.GetHashCode() * 397) ^ ColumnName.GetHashCode();
            }
        }
    }
}