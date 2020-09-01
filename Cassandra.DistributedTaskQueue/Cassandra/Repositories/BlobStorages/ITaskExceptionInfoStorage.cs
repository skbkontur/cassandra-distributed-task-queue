using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.BlobStorages
{
    public interface ITaskExceptionInfoStorage
    {
        bool TryAddNewExceptionInfo([NotNull] TaskMetaInformation taskMeta, [NotNull] Exception exception, out List<TimeGuid> newExceptionInfoIds);

        void ProlongExceptionInfosTtl([NotNull] TaskMetaInformation taskMeta);

        void Delete([NotNull] TaskMetaInformation taskMeta);

        /// <remarks>
        ///     Return TaskId -> TaskExceptionInfo[] map. Result does contain entries for ALL taskMetas
        /// </remarks>
        [NotNull]
        Dictionary<string, TaskExceptionInfo[]> Read([NotNull] TaskMetaInformation[] taskMetas);
    }
}