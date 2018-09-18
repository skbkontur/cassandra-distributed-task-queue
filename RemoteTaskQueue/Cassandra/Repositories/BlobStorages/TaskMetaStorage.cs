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

using Vostok.Logging.Abstractions;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class TaskMetaStorage : ITaskMetaStorage
    {
        public TaskMetaStorage(ICassandraCluster cassandraCluster, ISerializer serializer, IRemoteTaskQueueSettings remoteTaskQueueSettings, ILog logger)
        {
            this.serializer = serializer;
            var settings = new TimeBasedBlobStorageSettings(remoteTaskQueueSettings.QueueKeyspace, largeBlobsCfName, regularBlobsCfName);
            timeBasedBlobStorage = new TimeBasedBlobStorage(settings, cassandraCluster, logger);
        }

        public void Write([NotNull] TaskMetaInformation taskMeta, long timestamp)
        {
            var blobId = GetBlobId(taskMeta.Id);
            var taskMetaBytes = serializer.Serialize(taskMeta);
            timeBasedBlobStorage.Write(blobId, taskMetaBytes, timestamp, taskMeta.GetTtl());
        }

        public void Delete([NotNull] string taskId, long timestamp)
        {
            var blobId = GetBlobId(taskId);
            timeBasedBlobStorage.Delete(blobId, timestamp);
        }

        [CanBeNull]
        public TaskMetaInformation Read([NotNull] string taskId)
        {
            var blobId = GetBlobId(taskId);
            var taskMetaBytes = timeBasedBlobStorage.Read(blobId);
            if (taskMetaBytes == null)
                return null;
            return serializer.Deserialize<TaskMetaInformation>(taskMetaBytes);
        }

        [NotNull]
        public Dictionary<string, TaskMetaInformation> Read([NotNull] string[] taskIds)
        {
            var blobIdToTaskIdMap = taskIds.Distinct().ToDictionary(GetBlobId);
            var taskMetas = timeBasedBlobStorage.Read(blobIdToTaskIdMap.Keys.ToArray())
                                                .ToDictionary(x => blobIdToTaskIdMap[x.Key], x => serializer.Deserialize<TaskMetaInformation>(x.Value));
            return taskMetas;
        }

        [NotNull]
        private static BlobId GetBlobId([NotNull] string taskId)
        {
            TimeGuid timeGuid;
            if (!TimeGuid.TryParse(taskId, out timeGuid))
                throw new InvalidProgramStateException(string.Format("Task is not time-based: {0}", taskId));
            return new BlobId(timeGuid, BlobType.Regular);
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