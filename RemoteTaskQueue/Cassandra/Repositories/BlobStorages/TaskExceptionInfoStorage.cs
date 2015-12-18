using System;
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
    public class TaskExceptionInfoStorage : ITaskExceptionInfoStorage
    {
        public TaskExceptionInfoStorage(ICassandraCluster cassandraCluster, ISerializer serializer, ICassandraSettings cassandraSettings)
        {
            this.serializer = serializer;
            var settings = new TimeBasedBlobStorageSettings(cassandraSettings.QueueKeyspace, largeBlobsCfName, regularBlobsCfName);
            timeBasedBlobStorage = new TimeBasedBlobStorage(settings, cassandraCluster);
            legacyBlobStorage = new LegacyBlobStorage<TaskExceptionInfo>(cassandraCluster, serializer, cassandraSettings.QueueKeyspace, legacyCfName);
        }

        public bool TryAddNewExceptionInfo([NotNull] TaskMetaInformation taskMeta, [NotNull] Exception exception, out BlobId newExceptionInfoId)
        {
            newExceptionInfoId = null;
            var newExceptionInfo = new TaskExceptionInfo(exception);
            var lastExceptionInfo = TryGetLastExceptionInfo(taskMeta);
            if(lastExceptionInfo != null && lastExceptionInfo.ExceptionMessageInfo == newExceptionInfo.ExceptionMessageInfo)
                return false;
            var newExceptionInfoBytes = serializer.Serialize(newExceptionInfo);
            newExceptionInfoId = TimeBasedBlobStorage.GenerateNewBlobId(newExceptionInfoBytes.Length);
            var timestamp = newExceptionInfoId.Id.GetTimestamp().Ticks;
            legacyBlobStorage.Write(taskMeta.Id, newExceptionInfo, timestamp);
            timeBasedBlobStorage.Write(newExceptionInfoId, newExceptionInfoBytes, timestamp);
            return true;
        }

        [CanBeNull]
        private TaskExceptionInfo TryGetLastExceptionInfo([NotNull] TaskMetaInformation taskMeta)
        {
            if(!taskMeta.IsTimeBased())
                return legacyBlobStorage.Read(taskMeta.Id);
            var lastExceptionInfoId = taskMeta.GetTaskExceptionInfoIds().LastOrDefault();
            if(lastExceptionInfoId == null)
                return null;
            var lastExceptionInfoBytes = timeBasedBlobStorage.Read(lastExceptionInfoId);
            if(lastExceptionInfoBytes == null)
                return null;
            return serializer.Deserialize<TaskExceptionInfo>(lastExceptionInfoBytes);
        }

        public void Delete([NotNull] TaskMetaInformation taskMeta)
        {
            var timestamp = Timestamp.Now.Ticks;
            legacyBlobStorage.Delete(taskMeta.Id, timestamp);
            if(taskMeta.IsTimeBased())
            {
                foreach(var blobId in taskMeta.GetTaskExceptionInfoIds())
                    timeBasedBlobStorage.Delete(blobId, timestamp);
            }
        }

        [NotNull]
        public Dictionary<string, TaskExceptionInfo[]> Read([NotNull] TaskMetaInformation[] taskMetas)
        {
            var legacyBlobIds = new List<string>();
            var blobIdToTaskIdMap = new Dictionary<BlobId, string>();
            var distinctTaskMetas = taskMetas.DistinctBy(x => x.Id).ToArray();
            foreach(var taskMeta in distinctTaskMetas)
            {
                if(taskMeta.IsTimeBased())
                {
                    foreach(var blobId in taskMeta.GetTaskExceptionInfoIds())
                        blobIdToTaskIdMap.Add(blobId, taskMeta.Id);
                }
                else
                    legacyBlobIds.Add(taskMeta.Id);
            }
            var timeBasedBlobs = timeBasedBlobStorage.Read(blobIdToTaskIdMap.Keys.ToArray())
                                                     .GroupBy(x => blobIdToTaskIdMap[x.Key])
                                                     .ToDictionary(x => x.Key);
            var legacyBlobs = legacyBlobIds.Any()
                                  ? legacyBlobStorage.Read(legacyBlobIds)
                                  : new Dictionary<string, TaskExceptionInfo>();
            var result = new Dictionary<string, TaskExceptionInfo[]>();
            foreach(var taskId in distinctTaskMetas.Select(x => x.Id))
            {
                TaskExceptionInfo[] taskExceptionInfos;
                TaskExceptionInfo legacyExceptionInfo;
                IGrouping<string, KeyValuePair<BlobId, byte[]>> timeBasedExceptionInfos;
                if(timeBasedBlobs.TryGetValue(taskId, out timeBasedExceptionInfos))
                    taskExceptionInfos = timeBasedExceptionInfos.Select(x => serializer.Deserialize<TaskExceptionInfo>(x.Value)).ToArray();
                else if(legacyBlobs.TryGetValue(taskId, out legacyExceptionInfo))
                    taskExceptionInfos = new[] {legacyExceptionInfo};
                else
                    taskExceptionInfos = new TaskExceptionInfo[0];
                result.Add(taskId, taskExceptionInfos);
            }
            return result;
        }

        [NotNull]
        public static string[] GetColumnFamilyNames()
        {
            return new[] {legacyCfName, largeBlobsCfName, regularBlobsCfName};
        }

        private const string legacyCfName = "taskExceptionInfo";
        private const string largeBlobsCfName = "largeTaskExceptionInfos";
        private const string regularBlobsCfName = "regularTaskExceptionInfos";

        private readonly ISerializer serializer;
        private readonly TimeBasedBlobStorage timeBasedBlobStorage;
        private readonly LegacyBlobStorage<TaskExceptionInfo> legacyBlobStorage;
    }
}