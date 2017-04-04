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
        public TaskMetaStorage(ICassandraCluster cassandraCluster, ISerializer serializer, IRemoteTaskQueueSettings remoteTaskQueueSettings)
        {
            this.serializer = serializer;
            var settings = new TimeBasedBlobStorageSettings(remoteTaskQueueSettings.QueueKeyspace, largeBlobsCfName, regularBlobsCfName);
            timeBasedBlobStorage = new TimeBasedBlobStorage(settings, cassandraCluster);
        }

        public void Write([NotNull] TaskMetaInformation taskMeta, long timestamp)
        {
            TimeGuid timeGuid;
            if(!TimeGuid.TryParse(taskMeta.Id, out timeGuid))
                throw new InvalidProgramStateException(string.Format("TaskMeta is not time-based: {0}", taskMeta));
            var blobId = new BlobId(timeGuid, BlobType.Regular);
            var taskMetaBytes = serializer.Serialize(taskMeta);
            timeBasedBlobStorage.Write(blobId, taskMetaBytes, timestamp, taskMeta.GetTtl());
        }

        public void Delete([NotNull] string taskId, long timestamp)
        {
            TimeGuid timeGuid;
            if(!TimeGuid.TryParse(taskId, out timeGuid))
                throw new InvalidProgramStateException(string.Format("Task is not time-based: {0}", taskId));
            timeBasedBlobStorage.Delete(new BlobId(timeGuid, BlobType.Regular), timestamp);
        }

        [CanBeNull]
        public TaskMetaInformation Read([NotNull] string taskId)
        {
            TimeGuid timeGuid;
            if(!TimeGuid.TryParse(taskId, out timeGuid))
                throw new InvalidProgramStateException(string.Format("Task is not time-based: {0}", taskId));
            var taskMetaBytes = timeBasedBlobStorage.Read(new BlobId(timeGuid, BlobType.Regular));
            if(taskMetaBytes == null)
                return null;
            return serializer.Deserialize<TaskMetaInformation>(taskMetaBytes);
        }

        [NotNull]
        public Dictionary<string, TaskMetaInformation> Read([NotNull] string[] taskIds)
        {
            var blobIdToTaskIdMap = new Dictionary<BlobId, string>();
            foreach(var taskId in taskIds.Distinct())
            {
                TimeGuid timeGuid;
                if(!TimeGuid.TryParse(taskId, out timeGuid))
                    throw new InvalidProgramStateException(string.Format("Task is not time-based: {0}", taskId));
                blobIdToTaskIdMap.Add(new BlobId(timeGuid, BlobType.Regular), taskId);
            }
            var taskMetas = timeBasedBlobStorage.Read(blobIdToTaskIdMap.Keys.ToArray())
                                                .ToDictionary(x => blobIdToTaskIdMap[x.Key], x => serializer.Deserialize<TaskMetaInformation>(x.Value));
            return taskMetas;
        }

        [NotNull]
        public IEnumerable<Tuple<string, TaskMetaInformation>> ReadAll(int batchSize)
        {
            return timeBasedBlobStorage.ReadAll(batchSize).Select(x => Tuple.Create(x.Item1.Id.ToGuid().ToString(), serializer.Deserialize<TaskMetaInformation>(x.Item2)));
        }

        [NotNull]
        public static string[] GetColumnFamilyNames()
        {
            return new[] {largeBlobsCfName, regularBlobsCfName};
        }

        private const string largeBlobsCfName = "largeTaskMetas";
        private const string regularBlobsCfName = "regularTaskMetas";

        private readonly ISerializer serializer;
        private readonly TimeBasedBlobStorage timeBasedBlobStorage;
    }
}