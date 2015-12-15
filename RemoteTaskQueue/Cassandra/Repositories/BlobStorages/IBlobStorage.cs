using System.Collections.Generic;

using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public interface IBlobStorage<T, TId>
    {
        void Write([NotNull] TId id, [NotNull] T element);
        bool TryWrite([NotNull] T element, out TId id);

        [CanBeNull]
        T Read([NotNull] TId id);

        /// <remarks>
        /// Result does NOT contain entries for non existing or empty blobs
        /// </remarks>
        [NotNull]
        Dictionary<TId, T> Read([NotNull] TId[] ids);

        [NotNull]
        IEnumerable<KeyValuePair<TId, T>> ReadAll(int batchSize = 1000);

        void Delete([NotNull] TId id, long timestamp);
        void Delete([NotNull] IEnumerable<TId> ids, long? timestamp);
    }

    public interface IBlobStorage<T> : IBlobStorage<T, string>
    {
    }
}