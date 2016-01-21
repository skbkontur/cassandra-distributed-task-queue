using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Cassandra.Repositories.Indexes.ChildTaskIndex;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Configuration;

using SKBKontur.Catalogue.Objects;

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
        public TaskIndexRecord[] GetIndexRecords(long toTicks, [NotNull] params TaskIndexShardKey[] taskIndexShardKeys)
        {
            return taskIndexShardKeys.SelectMany(x => minimalStartTicksIndex.GetRecords(x, toTicks, batchSize : 2000))
                                     .OrderBy(x => x.MinimalStartTicks)
                                     .ToArray();
        }

        [NotNull]
        public TaskIndexRecord AddMeta([NotNull] TaskMetaInformation taskMeta)
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

            var oldMeta = taskMeta.TryGetSnapshot();
            if(oldMeta != null)
            {
                if(oldMeta.State != taskMeta.State || oldMeta.MinimalStartTicks != taskMeta.MinimalStartTicks)
                    minimalStartTicksIndex.RemoveRecord(FormatIndexRecord(oldMeta));
            }

            taskMeta.MakeSnapshot();
            return newIndexRecord;
        }

        [NotNull]
        private TaskIndexRecord FormatIndexRecord([NotNull] TaskMetaInformation taskMeta)
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
            meta.MakeSnapshot();
            return meta;
        }

        [NotNull]
        public Dictionary<string, TaskMetaInformation> GetMetas([NotNull] string[] taskIds)
        {
            var metas = taskMetaStorage.Read(taskIds);
            metas.Values.ForEach(x => x.MakeSnapshot());
            return metas;
        }

        private readonly ITaskMetaStorage taskMetaStorage;
        private readonly ITaskMinimalStartTicksIndex minimalStartTicksIndex;
        private readonly IEventLogRepository eventLogRepository;
        private readonly IGlobalTime globalTime;
        private readonly IChildTaskIndex childTaskIndex;
        private readonly ITaskDataRegistry taskDataRegistry;
    }
}