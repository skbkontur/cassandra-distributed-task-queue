using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.BlobStorages
{
    public class BlobId : IEquatable<BlobId>
    {
        public BlobId(TimeGuid id, BlobType type)
        {
            Id = id;
            Type = type;
        }

        [NotNull]
        public TimeGuid Id { get; private set; }

        public BlobType Type { get; private set; }

        public override string ToString()
        {
            return string.Format("Id: {0}, Type: {1}", Id, Type);
        }

        public bool Equals(BlobId other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Id.Equals(other.Id) && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((BlobId)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode() * 397) ^ (int)Type;
            }
        }
    }
}