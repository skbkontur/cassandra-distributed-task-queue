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

namespace RemoteQueue.Cassandra.Repositories
{
    public class HandleTasksMetaStorage : IHandleTasksMetaStorage
    {
        public HandleTasksMetaStorage(
            ITaskMetaInformationBlobStorage metaStorage,
            ITaskMinimalStartTicksIndex minimalStartTicksIndex,
            IEventLogRepository eventLogRepository,
            IGlobalTime globalTime,
            IChildTaskIndex childTaskIndex)
        {
            this.metaStorage = metaStorage;
            this.minimalStartTicksIndex = minimalStartTicksIndex;
            this.eventLogRepository = eventLogRepository;
            this.globalTime = globalTime;
            this.childTaskIndex = childTaskIndex;
        }

        [NotNull]
        public IEnumerable<TaskIndexRecord> GetIndexRecords(long toTicks, [NotNull] params TaskNameAndState[] taskNameAndStates)
        {
            return taskNameAndStates.SelectMany(x => minimalStartTicksIndex.GetRecords(x, toTicks, batchSize : 2000).ToArray());
        }

        [NotNull]
        public TaskIndexRecord AddMeta([NotNull] TaskMetaInformation taskMeta)
        {
            var nowTicks = Math.Max((taskMeta.LastModificationTicks ?? 0) + 1, globalTime.GetNowTicks());
            taskMeta.LastModificationTicks = nowTicks;
            eventLogRepository.AddEvent(taskMeta.Id, nowTicks);
            var newIndexRecord = taskMeta.FormatIndexRecord();
            minimalStartTicksIndex.AddRecord(newIndexRecord);
            if(taskMeta.State == TaskState.New)
                childTaskIndex.AddMeta(taskMeta);
            metaStorage.Write(taskMeta.Id, taskMeta);

            var oldMeta = taskMeta.TryGetSnapshot();
            if(oldMeta != null)
            {
                if(oldMeta.State != taskMeta.State || oldMeta.MinimalStartTicks != taskMeta.MinimalStartTicks)
                {
                    minimalStartTicksIndex.RemoveRecord(oldMeta.FormatIndexRecord());
                    minimalStartTicksIndex.RemoveRecord(new TaskIndexRecord(oldMeta.Id, oldMeta.MinimalStartTicks, TaskNameAndState.AnyTaskName(oldMeta.State)));
                }
            }

            taskMeta.MakeSnapshot();
            return newIndexRecord;
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
    }
}