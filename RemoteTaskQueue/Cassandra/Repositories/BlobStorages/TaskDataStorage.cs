using System.Collections.Generic;
using System.Linq;

using GroBuf;

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
        public TaskDataStorage(ICassandraCluster cassandraCluster, ISerializer serializer, IRemoteTaskQueueSettings remoteTaskQueueSettings)
        {
            var settings = new TimeBasedBlobStorageSettings(remoteTaskQueueSettings.QueueKeyspace, largeBlobsCfName, regularBlobsCfName);
            timeBasedBlobStorage = new TimeBasedBlobStorage(settings, cassandraCluster);
            legacyBlobStorage = new LegacyBlobStorage<byte[]>(cassandraCluster, serializer, remoteTaskQueueSettings.QueueKeyspace, legacyCfName);
        }

        [NotNull]
        public BlobId Write([NotNull] TaskMetaInformation taskMeta, [NotNull] byte[] taskData)
        {
            if(!taskMeta.IsTimeBased())
                throw new InvalidProgramStateException(string.Format("TaskId is not time-based: {0}", taskMeta.Id));
            var blobId = TimeBasedBlobStorage.GenerateNewBlobId(taskData.Length);
            var timestamp = blobId.Id.GetTimestamp().Ticks;
            timeBasedBlobStorage.Write(blobId, taskData, timestamp, taskMeta.GetTtl());
            return blobId;
        }

        public void Delete([NotNull] TaskMetaInformation taskMeta)
        {
            var timestamp = Timestamp.Now.Ticks;
            if(!taskMeta.IsTimeBased())
                legacyBlobStorage.Delete(taskMeta.Id, timestamp);
            else
                timeBasedBlobStorage.Delete(taskMeta.GetTaskDataId(), timestamp);
        }

        public void Overwrite([NotNull] TaskMetaInformation taskMeta, [NotNull] byte[] taskData)
        {
            var timestamp = Timestamp.Now.Ticks;
            if(!taskMeta.IsTimeBased())
                legacyBlobStorage.Write(taskMeta.Id, taskData, timestamp, taskMeta.GetTtl());
            else
                timeBasedBlobStorage.Write(taskMeta.GetTaskDataId(), taskData, timestamp, taskMeta.GetTtl());
        }

        [CanBeNull]
        public byte[] Read([NotNull] TaskMetaInformation taskMeta)
        {
            return taskMeta.IsTimeBased()
                       ? timeBasedBlobStorage.Read(taskMeta.GetTaskDataId())
                       : legacyBlobStorage.Read(taskMeta.Id);
        }

        [NotNull]
        public Dictionary<string, byte[]> Read([NotNull] TaskMetaInformation[] taskMetas)
        {
            var legacyBlobIds = new List<string>();
            var blobIdToTaskIdMap = new Dictionary<BlobId, string>();
            foreach(var taskMeta in taskMetas.DistinctBy(x => x.Id))
            {
                if(taskMeta.IsTimeBased())
                    blobIdToTaskIdMap.Add(taskMeta.GetTaskDataId(), taskMeta.Id);
                else
                    legacyBlobIds.Add(taskMeta.Id);
            }
            var taskDatas = timeBasedBlobStorage.Read(blobIdToTaskIdMap.Keys.ToArray())
                                                .ToDictionary(x => blobIdToTaskIdMap[x.Key], x => x.Value);
            if(legacyBlobIds.Any())
            {
                foreach(var kvp in legacyBlobStorage.Read(legacyBlobIds))
                    taskDatas.Add(kvp.Key, kvp.Value);
            }
            return taskDatas;
        }

        [NotNull]
        public static string[] GetColumnFamilyNames()
        {
            return new[] {legacyCfName, largeBlobsCfName, regularBlobsCfName};
        }

        private const string legacyCfName = "taskDataStorage";
        private const string largeBlobsCfName = "largeTaskDatas";
        private const string regularBlobsCfName = "regularTaskDatas";

        private readonly TimeBasedBlobStorage timeBasedBlobStorage;
        private readonly LegacyBlobStorage<byte[]> legacyBlobStorage;
    }
}