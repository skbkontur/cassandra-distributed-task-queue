using System;
using System.Collections.Generic;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;

using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.Objects;

using Vostok.Logging.Abstractions;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class TaskExceptionInfoStorage : ITaskExceptionInfoStorage
    {
        public TaskExceptionInfoStorage(ICassandraCluster cassandraCluster, ISerializer serializer, IRemoteTaskQueueSettings remoteTaskQueueSettings, ILog logger)
        {
            this.serializer = serializer;
            timeBasedBlobStorage = new SinglePartitionTimeBasedBlobStorage(remoteTaskQueueSettings.QueueKeyspace, timeBasedCfName, cassandraCluster, logger.ForContext(nameof(TaskExceptionInfoStorage)));
        }

        public bool TryAddNewExceptionInfo([NotNull] TaskMetaInformation taskMeta, [NotNull] Exception exception, out List<TimeGuid> newExceptionInfoIds)
        {
            if (!taskMeta.IsTimeBased())
                throw new InvalidProgramStateException(string.Format("TaskMeta is not time-based: {0}", taskMeta));
            newExceptionInfoIds = null;
            var newExceptionInfo = new TaskExceptionInfo(exception);
            var lastExceptionInfo = TryGetLastExceptionInfo(taskMeta);
            if (lastExceptionInfo != null && lastExceptionInfo.ExceptionMessageInfo == newExceptionInfo.ExceptionMessageInfo)
                return false;
            var newExceptionInfoId = TimeGuid.NowGuid();
            var timestamp = newExceptionInfoId.GetTimestamp().Ticks;
            TimeGuid oldExceptionInfoId;
            newExceptionInfoIds = taskMeta.AddExceptionInfoId(newExceptionInfoId, out oldExceptionInfoId);
            var newExceptionInfoBytes = serializer.Serialize(newExceptionInfo);
            timeBasedBlobStorage.Write(taskMeta.Id, newExceptionInfoId, newExceptionInfoBytes, timestamp, taskMeta.GetTtl());
            if (oldExceptionInfoId != null)
                timeBasedBlobStorage.Delete(taskMeta.Id, oldExceptionInfoId, timestamp);
            return true;
        }

        public void ProlongExceptionInfosTtl([NotNull] TaskMetaInformation taskMeta)
        {
            if (!taskMeta.IsTimeBased())
                throw new InvalidProgramStateException(string.Format("TaskMeta is not time-based: {0}", taskMeta));
            var oldExceptionInfos = timeBasedBlobStorage.Read(taskMeta.Id, taskMeta.GetTaskExceptionInfoIds().ToArray());
            var timestamp = Timestamp.Now.Ticks;
            foreach (var exceptionInfo in oldExceptionInfos)
                timeBasedBlobStorage.Write(taskMeta.Id, exceptionInfo.Key, exceptionInfo.Value, timestamp, taskMeta.GetTtl());
        }

        [CanBeNull]
        private TaskExceptionInfo TryGetLastExceptionInfo([NotNull] TaskMetaInformation taskMeta)
        {
            if (!taskMeta.IsTimeBased())
                throw new InvalidProgramStateException(string.Format("TaskMeta is not time-based: {0}", taskMeta));
            var lastExceptionInfoId = taskMeta.GetTaskExceptionInfoIds().LastOrDefault();
            if (lastExceptionInfoId == null)
                return null;
            var lastExceptionInfoBytes = timeBasedBlobStorage.Read(taskMeta.Id, lastExceptionInfoId);
            if (lastExceptionInfoBytes == null)
                return null;
            return serializer.Deserialize<TaskExceptionInfo>(lastExceptionInfoBytes);
        }

        public void Delete([NotNull] TaskMetaInformation taskMeta)
        {
            if (!taskMeta.IsTimeBased())
                throw new InvalidProgramStateException(string.Format("TaskMeta is not time-based: {0}", taskMeta));
            var timestamp = Timestamp.Now.Ticks;
            foreach (var blobId in taskMeta.GetTaskExceptionInfoIds())
                timeBasedBlobStorage.Delete(taskMeta.Id, blobId, timestamp);
        }

        [NotNull]
        public Dictionary<string, TaskExceptionInfo[]> Read([NotNull, ItemNotNull] TaskMetaInformation[] taskMetas)
        {
            var result = new Dictionary<string, TaskExceptionInfo[]>();
            foreach (var taskMeta in taskMetas.DistinctBy(x => x.Id))
            {
                if (!taskMeta.IsTimeBased())
                    throw new InvalidProgramStateException(string.Format("TaskMeta is not time-based: {0}", taskMeta));
                var columnIds = taskMeta.GetTaskExceptionInfoIds().ToArray();
                var blobs = timeBasedBlobStorage.Read(taskMeta.Id, columnIds);
                var taskExceptionInfos = blobs.Select(x => serializer.Deserialize<TaskExceptionInfo>(x.Value)).ToArray();
                result.Add(taskMeta.Id, taskExceptionInfos);
            }
            return result;
        }

        [NotNull, ItemNotNull]
        public static string[] GetColumnFamilyNames()
        {
            return new[] {timeBasedCfName};
        }

        private const string timeBasedCfName = "timeBasedTaskExceptionInfos";

        private readonly ISerializer serializer;
        private readonly SinglePartitionTimeBasedBlobStorage timeBasedBlobStorage;
    }
}