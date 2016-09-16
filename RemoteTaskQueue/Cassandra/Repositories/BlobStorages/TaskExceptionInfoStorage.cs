using System;
using System.Collections.Generic;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.CassandraPrimitives.Storages.Primitives;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class TaskExceptionInfoStorage : ITaskExceptionInfoStorage
    {
        public TaskExceptionInfoStorage(ICassandraCluster cassandraCluster, ISerializer serializer, IRemoteTaskQueueSettings remoteTaskQueueSettings)
        {
            this.serializer = serializer;
            timeBasedBlobStorage = new SinglePartitionTimeBasedBlobStorage(new ColumnFamilyFullName(remoteTaskQueueSettings.QueueKeyspace, timeBasedCfName), cassandraCluster);
            legacyBlobStorage = new LegacyBlobStorage<TaskExceptionInfo>(cassandraCluster, serializer, remoteTaskQueueSettings.QueueKeyspace, legacyCfName);
        }

        public bool TryAddNewExceptionInfo([NotNull] TaskMetaInformation taskMeta, [NotNull] Exception exception, out List<TimeGuid> newExceptionInfoIds)
        {
            newExceptionInfoIds = null;
            var newExceptionInfo = new TaskExceptionInfo(exception);
            var lastExceptionInfo = TryGetLastExceptionInfo(taskMeta);
            if(lastExceptionInfo != null && lastExceptionInfo.ExceptionMessageInfo == newExceptionInfo.ExceptionMessageInfo)
                return false;
            var newExceptionInfoId = TimeGuid.NowGuid();
            var timestamp = newExceptionInfoId.GetTimestamp().Ticks;
            TimeGuid oldExceptionInfoId;
            newExceptionInfoIds = taskMeta.AddExceptionInfoId(newExceptionInfoId, out oldExceptionInfoId);
            if(!taskMeta.IsTimeBased())
                legacyBlobStorage.Write(taskMeta.Id, newExceptionInfo, timestamp, taskMeta.GetTtl());
            else
            {
                var newExceptionInfoBytes = serializer.Serialize(newExceptionInfo);
                timeBasedBlobStorage.Write(taskMeta.Id, newExceptionInfoId, newExceptionInfoBytes, timestamp, taskMeta.GetTtl());
                if(oldExceptionInfoId != null)
                    timeBasedBlobStorage.Delete(taskMeta.Id, oldExceptionInfoId, timestamp);
            }
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
            var lastExceptionInfoBytes = timeBasedBlobStorage.Read(taskMeta.Id, lastExceptionInfoId);
            if(lastExceptionInfoBytes == null)
                return null;
            return serializer.Deserialize<TaskExceptionInfo>(lastExceptionInfoBytes);
        }

        public void Delete([NotNull] TaskMetaInformation taskMeta)
        {
            var timestamp = Timestamp.Now.Ticks;
            if(!taskMeta.IsTimeBased())
                legacyBlobStorage.Delete(taskMeta.Id, timestamp);
            else
            {
                foreach(var blobId in taskMeta.GetTaskExceptionInfoIds())
                    timeBasedBlobStorage.Delete(taskMeta.Id, blobId, timestamp);
            }
        }

        [NotNull]
        public Dictionary<string, TaskExceptionInfo[]> Read([NotNull] TaskMetaInformation[] taskMetas)
        {
            var legacyBlobIds = new List<string>();
            var timeBasedBlobsByTaskId = new Dictionary<string, Dictionary<TimeGuid, byte[]>>();
            var distinctTaskMetas = taskMetas.DistinctBy(x => x.Id).ToArray();
            foreach(var taskMeta in distinctTaskMetas)
            {
                if(taskMeta.IsTimeBased())
                {
                    var columnIds = taskMeta.GetTaskExceptionInfoIds().ToArray();
                    var blobs = timeBasedBlobStorage.Read(taskMeta.Id, columnIds);
                    timeBasedBlobsByTaskId.Add(taskMeta.Id, blobs);
                }
                else
                    legacyBlobIds.Add(taskMeta.Id);
            }
            var legacyBlobs = legacyBlobIds.Any()
                                  ? legacyBlobStorage.Read(legacyBlobIds)
                                  : new Dictionary<string, TaskExceptionInfo>();
            var result = new Dictionary<string, TaskExceptionInfo[]>();
            foreach(var taskId in distinctTaskMetas.Select(x => x.Id))
            {
                TaskExceptionInfo[] taskExceptionInfos;
                TaskExceptionInfo legacyExceptionInfo;
                Dictionary<TimeGuid, byte[]> timeBasedExceptionInfos;
                if(timeBasedBlobsByTaskId.TryGetValue(taskId, out timeBasedExceptionInfos))
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
            return new[] {legacyCfName, timeBasedCfName};
        }

        private const string legacyCfName = "taskExceptionInfo";
        private const string timeBasedCfName = "timeBasedTaskExceptionInfos";

        private readonly ISerializer serializer;
        private readonly SinglePartitionTimeBasedBlobStorage timeBasedBlobStorage;
        private readonly LegacyBlobStorage<TaskExceptionInfo> legacyBlobStorage;
    }
}