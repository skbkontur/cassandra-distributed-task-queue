using System;
using System.Collections.Generic;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class TaskMetaStorage : ITaskMetaStorage
    {
        public TaskMetaStorage(ICassandraCluster cassandraCluster, ISerializer serializer, ICassandraSettings cassandraSettings)
        {
            this.serializer = serializer;
            var settings = new TimeBasedBlobStorageSettings(cassandraSettings.QueueKeyspace, largeBlobsCfName, regularBlobsCfName);
            timeBasedBlobStorage = new TimeBasedBlobStorage(settings, cassandraCluster);
            legacyBlobStorage = new LegacyBlobStorage<TaskMetaInformation>(cassandraCluster, serializer, cassandraSettings.QueueKeyspace, legacyCfName);
        }

        public void Write([NotNull] TaskMetaInformation taskMeta, long timestamp)
        {
            TimeGuid timeGuid;
            if(!TimeGuid.TryParse(taskMeta.Id, out timeGuid))
                throw new InvalidProgramStateException(string.Format("TaskMeta.Id is not a TimeGuid for: {0}", taskMeta));
            var blobId = new BlobId(timeGuid, BlobType.Regular);
            var taskMetaBytes = serializer.Serialize(taskMeta);
            legacyBlobStorage.Write(taskMeta.Id, taskMeta, timestamp);
            timeBasedBlobStorage.Write(blobId, taskMetaBytes, timestamp);
        }

        public void Delete([NotNull] string taskId, long timestamp)
        {
            legacyBlobStorage.Delete(taskId, timestamp);
            TimeGuid timeGuid;
            if(TimeGuid.TryParse(taskId, out timeGuid))
                timeBasedBlobStorage.Delete(new BlobId(timeGuid, BlobType.Regular), timestamp);
        }

        [CanBeNull]
        public TaskMetaInformation Read([NotNull] string taskId)
        {
            TimeGuid timeGuid;
            if(!TimeGuid.TryParse(taskId, out timeGuid))
                return legacyBlobStorage.Read(taskId);
            var taskMetaBytes = timeBasedBlobStorage.Read(new BlobId(timeGuid, BlobType.Regular));
            if(taskMetaBytes == null)
                return null;
            return serializer.Deserialize<TaskMetaInformation>(taskMetaBytes);
        }

        [NotNull]
        public Dictionary<string, TaskMetaInformation> Read([NotNull] string[] taskIds)
        {
            var legacyBlobIds = new List<string>();
            var blobIdToTaskIdMap = new Dictionary<BlobId, string>();
            foreach(var taskId in taskIds.Distinct())
            {
                TimeGuid timeGuid;
                if(TimeGuid.TryParse(taskId, out timeGuid))
                    blobIdToTaskIdMap.Add(new BlobId(timeGuid, BlobType.Regular), taskId);
                else
                    legacyBlobIds.Add(taskId);
            }
            var taskMetas = timeBasedBlobStorage.Read(blobIdToTaskIdMap.Keys.ToArray())
                                                .ToDictionary(x => blobIdToTaskIdMap[x.Key], x => serializer.Deserialize<TaskMetaInformation>(x.Value));
            if(legacyBlobIds.Any())
            {
                foreach(var kvp in legacyBlobStorage.Read(legacyBlobIds))
                    taskMetas.Add(kvp.Key, kvp.Value);
            }
            return taskMetas;
        }

        [NotNull]
        public IEnumerable<Tuple<string, TaskMetaInformation>> ReadAll(int batchSize)
        {
            return legacyBlobStorage.ReadAll(batchSize);
        }

        [NotNull]
        public static string[] GetColumnFamilyNames()
        {
            return new[] {legacyCfName, largeBlobsCfName, regularBlobsCfName};
        }

        private const string legacyCfName = "taskMetaInformation";
        private const string largeBlobsCfName = "largeTaskMetas";
        private const string regularBlobsCfName = "regularTaskMetas";

        private readonly ISerializer serializer;
        private readonly TimeBasedBlobStorage timeBasedBlobStorage;
        private readonly LegacyBlobStorage<TaskMetaInformation> legacyBlobStorage;
    }
}