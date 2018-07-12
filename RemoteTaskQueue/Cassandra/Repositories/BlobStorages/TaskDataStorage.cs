using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class TaskDataStorage : ITaskDataStorage
    {
        public TaskDataStorage(ICassandraCluster cassandraCluster, IRemoteTaskQueueSettings remoteTaskQueueSettings)
        {
            var settings = new TimeBasedBlobStorageSettings(remoteTaskQueueSettings.QueueKeyspace, largeBlobsCfName, regularBlobsCfName);
            timeBasedBlobStorage = new TimeBasedBlobStorage(settings, cassandraCluster);
        }

        [NotNull]
        public BlobId Write([NotNull] TaskMetaInformation taskMeta, [NotNull] byte[] taskData)
        {
            if (!taskMeta.IsTimeBased())
                throw new InvalidProgramStateException(string.Format("TaskId is not time-based: {0}", taskMeta.Id));
            var blobId = TimeBasedBlobStorage.GenerateNewBlobId(taskData.Length);
            var timestamp = blobId.Id.GetTimestamp().Ticks;
            timeBasedBlobStorage.Write(blobId, taskData, timestamp, taskMeta.GetTtl());
            return blobId;
        }

        public void Delete([NotNull] TaskMetaInformation taskMeta)
        {
            if (!taskMeta.IsTimeBased())
                throw new InvalidProgramStateException(string.Format("TaskMeta is not time-based: {0}", taskMeta));
            timeBasedBlobStorage.Delete(taskMeta.GetTaskDataId(), timestamp : Timestamp.Now.Ticks);
        }

        public void Overwrite([NotNull] TaskMetaInformation taskMeta, [NotNull] byte[] taskData)
        {
            if (!taskMeta.IsTimeBased())
                throw new InvalidProgramStateException(string.Format("TaskMeta is not time-based: {0}", taskMeta));
            timeBasedBlobStorage.Write(taskMeta.GetTaskDataId(), taskData, timestamp : Timestamp.Now.Ticks, ttl : taskMeta.GetTtl());
        }

        [CanBeNull]
        public byte[] Read([NotNull] TaskMetaInformation taskMeta)
        {
            if (!taskMeta.IsTimeBased())
                throw new InvalidProgramStateException(string.Format("TaskMeta is not time-based: {0}", taskMeta));
            return timeBasedBlobStorage.Read(taskMeta.GetTaskDataId());
        }

        [NotNull]
        public Dictionary<string, byte[]> Read([NotNull] TaskMetaInformation[] taskMetas)
        {
            var blobIdToTaskIdMap = new Dictionary<BlobId, string>();
            foreach (var taskMeta in taskMetas.DistinctBy(x => x.Id))
            {
                if (!taskMeta.IsTimeBased())
                    throw new InvalidProgramStateException(string.Format("TaskMeta is not time-based: {0}", taskMeta));
                blobIdToTaskIdMap.Add(taskMeta.GetTaskDataId(), taskMeta.Id);
            }
            var taskDatas = timeBasedBlobStorage.Read(blobIdToTaskIdMap.Keys.ToArray())
                                                .ToDictionary(x => blobIdToTaskIdMap[x.Key], x => x.Value);
            return taskDatas;
        }

        [NotNull]
        public static string[] GetColumnFamilyNames()
        {
            return new[] {largeBlobsCfName, regularBlobsCfName};
        }

        private const string largeBlobsCfName = "largeTaskDatas";
        private const string regularBlobsCfName = "regularTaskDatas";

        private readonly TimeBasedBlobStorage timeBasedBlobStorage;
    }
}