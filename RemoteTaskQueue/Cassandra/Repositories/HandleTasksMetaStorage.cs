using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Cassandra.Repositories.Indexes.ChildTaskIndex;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Configuration;

using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteQueue.Cassandra.Repositories
{
    public class HandleTasksMetaStorage : IHandleTasksMetaStorage
    {
        public HandleTasksMetaStorage(
            ITaskMetaStorage taskMetaStorage,
            ITaskMinimalStartTicksIndex minimalStartTicksIndex,
            IEventLogRepository eventLogRepository,
            IGlobalTime globalTime,
            IChildTaskIndex childTaskIndex,
            ITaskDataRegistry taskDataRegistry)
        {
            this.taskMetaStorage = taskMetaStorage;
            this.minimalStartTicksIndex = minimalStartTicksIndex;
            this.eventLogRepository = eventLogRepository;
            this.globalTime = globalTime;
            this.childTaskIndex = childTaskIndex;
            this.taskDataRegistry = taskDataRegistry;
        }

        [CanBeNull]
        public LiveRecordTicksMarkerState TryGetCurrentLiveRecordTicksMarker([NotNull] TaskIndexShardKey taskIndexShardKey)
        {
            return minimalStartTicksIndex.TryGetCurrentLiveRecordTicksMarker(taskIndexShardKey);
        }

        [NotNull]
        public TaskIndexRecord[] GetIndexRecords(long toTicks, [NotNull] TaskIndexShardKey[] taskIndexShardKeys)
        {
            var liveRecordsByKey = new Dictionary<TaskIndexShardKey, TaskIndexRecord[]>();
            foreach(var taskIndexShardKey in taskIndexShardKeys)
            {
                var liveRecords = minimalStartTicksIndex.GetRecords(taskIndexShardKey, toTicks, batchSize : 2000).ToArray();
                liveRecordsByKey.Add(taskIndexShardKey, liveRecords);
                Log.For(this).InfoFormat("Got {0} live minimalStartTicksIndex records for taskIndexShardKey: {1}; Oldest live record: {2}", liveRecords.Length, taskIndexShardKey, liveRecords.FirstOrDefault());
            }
            return liveRecordsByKey.SelectMany(x => x.Value).OrderBy(x => x.MinimalStartTicks).ToArray();
        }

        [NotNull]
        public TaskIndexRecord AddMeta([NotNull] TaskMetaInformation taskMeta, [CanBeNull] TaskIndexRecord oldTaskIndexRecord)
        {
            var globalNowTicks = globalTime.UpdateNowTicks();
            var nowTicks = Math.Max((taskMeta.LastModificationTicks ?? 0) + 1, globalNowTicks);
            taskMeta.LastModificationTicks = nowTicks;
            eventLogRepository.AddEvent(taskMeta.Id, nowTicks);
            var newIndexRecord = FormatIndexRecord(taskMeta);
            minimalStartTicksIndex.AddRecord(newIndexRecord);
            if(taskMeta.State == TaskState.New)
                childTaskIndex.AddMeta(taskMeta);
            taskMetaStorage.Write(taskMeta, globalNowTicks);
            if(oldTaskIndexRecord != null)
                minimalStartTicksIndex.RemoveRecord(oldTaskIndexRecord);
            return newIndexRecord;
        }

        [NotNull]
        public TaskIndexRecord FormatIndexRecord([NotNull] TaskMetaInformation taskMeta)
        {
            var taskTopic = taskDataRegistry.GetTaskTopic(taskMeta.Name);
            var taskIndexShardKey = new TaskIndexShardKey(taskTopic, taskMeta.State);
            return new TaskIndexRecord(taskMeta.Id, taskMeta.MinimalStartTicks, taskIndexShardKey);
        }

        [NotNull]
        public TaskMetaInformation GetMeta([NotNull] string taskId)
        {
            var meta = taskMetaStorage.Read(taskId);
            if(meta == null)
                throw new InvalidProgramStateException(string.Format("TaskMeta not found for: {0}", taskId));
            return meta;
        }

        [NotNull]
        public Dictionary<string, TaskMetaInformation> GetMetas([NotNull] string[] taskIds)
        {
            return taskMetaStorage.Read(taskIds);
        }

        private readonly ITaskMetaStorage taskMetaStorage;
        private readonly ITaskMinimalStartTicksIndex minimalStartTicksIndex;
        private readonly IEventLogRepository eventLogRepository;
        private readonly IGlobalTime globalTime;
        private readonly IChildTaskIndex childTaskIndex;
        private readonly ITaskDataRegistry taskDataRegistry;
    }
}