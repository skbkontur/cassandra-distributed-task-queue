using System.Collections.Generic;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class TaskDataStorage : ITaskDataStorage
    {
        public TaskDataStorage(ICassandraCluster cassandraCluster, ISerializer serializer, string keyspaceName)
        {
            var settings = new TimeBasedBlobStorageSettings(keyspaceName, largeBlobsCfName, regularBlobsCfName);
            timeBasedBlobStorage = new TimeBasedBlobStorage(settings, cassandraCluster);
            legacyBlobStorage = new LegacyBlobStorage<byte[]>(cassandraCluster, serializer, keyspaceName, legacyCfName);
        }

        [NotNull]
        public BlobId Write([NotNull] string taskId, [NotNull] byte[] taskData)
        {
            var blobId = TimeBasedBlobStorage.GenerateNewBlobId(taskData.Length);
            var timestamp = blobId.Id.GetTimestamp().Ticks;
            legacyBlobStorage.Write(taskId, taskData, timestamp);
            timeBasedBlobStorage.Write(blobId, taskData, timestamp);
            return blobId;
        }

        public void Overwrite([NotNull] string taskId, [NotNull] BlobId blobId, [NotNull] byte[] taskData)
        {
            var timestamp = Timestamp.Now.Ticks;
            legacyBlobStorage.Write(taskId, taskData, timestamp);
            timeBasedBlobStorage.Write(blobId, taskData, timestamp);
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