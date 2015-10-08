using System;

using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.Indexes
{
    public class TaskColumnInfo : IEquatable<TaskColumnInfo>
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

        public bool Equals(TaskColumnInfo other)
        {
            if(ReferenceEquals(null, other)) return false;
            if(ReferenceEquals(this, other)) return true;
            return string.Equals(RowKey, other.RowKey) && string.Equals(ColumnName, other.ColumnName);
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj)) return false;
            if(ReferenceEquals(this, obj)) return true;
            if(obj.GetType() != this.GetType()) return false;
            return Equals((TaskColumnInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (RowKey.GetHashCode() * 397) ^ ColumnName.GetHashCode();
            }
        }

        public override string ToString()
        {
            return string.Format("RowKey: {0}, ColumnName: {1}", RowKey, ColumnName);
        }
    }
}