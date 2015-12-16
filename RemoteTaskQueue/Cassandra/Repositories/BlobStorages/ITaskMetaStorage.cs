using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public interface ITaskMetaStorage
    {
        void Write([NotNull] TaskMetaInformation taskMeta, long globalNowTicks);

        [CanBeNull]
        TaskMetaInformation Read([NotNull] string taskId);

        /// <remarks>
        ///     Return TaskId -> TaskMetaInformation map. Result does NOT contain entries for non existing metas
        /// </remarks>
        [NotNull]
        Dictionary<string, TaskMetaInformation> Read([NotNull] string[] taskIds);
    }
}