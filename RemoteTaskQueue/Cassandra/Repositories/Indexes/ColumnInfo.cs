using System;

namespace RemoteQueue.Cassandra.Repositories.Indexes
{
    public class ColumnInfo : IEquatable<ColumnInfo>
    {
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
                return ((RowKey != null ? RowKey.GetHashCode() : 0) * 397) ^ (ColumnName != null ? ColumnName.GetHashCode() : 0);
            }
        }

        public string RowKey { get; set; }
        public string ColumnName { get; set; }
    }
}