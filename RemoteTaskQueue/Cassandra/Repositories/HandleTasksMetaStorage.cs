using System;
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

namespace RemoteQueue.Cassandra.Repositories
{
    public class HandleTasksMetaStorage : IHandleTasksMetaStorage
    {
        public HandleTasksMetaStorage(
            ITaskMetaInformationBlobStorage metaStorage,
            ITaskMinimalStartTicksIndex minimalStartTicksIndex,
            IEventLogRepository eventLogRepository,
            IGlobalTime globalTime,
            IChildTaskIndex childTaskIndex,
            ITaskDataRegistry taskDataRegistry)
        {
            this.metaStorage = metaStorage;
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
            return taskIndexShardKeys.SelectMany(x => minimalStartTicksIndex.GetRecords(x, toTicks, batchSize : 2000)).ToArray();
        }

        [NotNull]
        public TaskIndexRecord AddMeta([NotNull] TaskMetaInformation taskMeta)
        {
            var nowTicks = Math.Max((taskMeta.LastModificationTicks ?? 0) + 1, globalTime.GetNowTicks());
            taskMeta.LastModificationTicks = nowTicks;
            eventLogRepository.AddEvent(taskMeta.Id, nowTicks);
            var newIndexRecord = FormatIndexRecord(taskMeta);
            minimalStartTicksIndex.AddRecord(newIndexRecord);
            if(taskMeta.State == TaskState.New)
                childTaskIndex.AddMeta(taskMeta);
            metaStorage.Write(taskMeta.Id, taskMeta);

            var oldMeta = taskMeta.TryGetSnapshot();
            if(oldMeta != null)
            {
                if(oldMeta.State != taskMeta.State || oldMeta.MinimalStartTicks != taskMeta.MinimalStartTicks)
                {
                    minimalStartTicksIndex.RemoveRecord(FormatIndexRecord(oldMeta));
                    minimalStartTicksIndex.RemoveRecord(new TaskIndexRecord(oldMeta.Id, oldMeta.MinimalStartTicks, TaskIndexShardKey.AnyTaskTopic(oldMeta.State)));
                }
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

        public TaskMetaInformation GetMeta(string taskId)
        {
            var meta = metaStorage.Read(taskId);
            if(meta != null)
                meta.MakeSnapshot();
            return meta;
        }

        public TaskMetaInformation[] GetMetas(string[] taskIds)
        {
            var metas = metaStorage.Read(taskIds);
            metas.ForEach(x => x.MakeSnapshot());
            return metas;
        }

        public TaskMetaInformation[] GetMetasQuiet(string[] taskIds)
        {
            var metas = metaStorage.ReadQuiet(taskIds);
            metas.Where(x => x != null).ForEach(x => x.MakeSnapshot());
            return metas;
        }

        private readonly ITaskMetaInformationBlobStorage metaStorage;
        private readonly ITaskMinimalStartTicksIndex minimalStartTicksIndex;
        private readonly IEventLogRepository eventLogRepository;
        private readonly IGlobalTime globalTime;
        private readonly IChildTaskIndex childTaskIndex;
        private readonly ITaskDataRegistry taskDataRegistry;
    }
}