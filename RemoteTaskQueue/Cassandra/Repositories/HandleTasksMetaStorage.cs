using System;
using System.Collections.Generic;
using System.Linq;

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
            ITaskMetaInformationBlobStorage storage,
            ITaskMinimalStartTicksIndex minimalStartTicksIndex,
            IEventLogRepository eventLogRepository,
            IGlobalTime globalTime,
            IChildTaskIndex childTaskIndex)
        {
            this.storage = storage;
            this.minimalStartTicksIndex = minimalStartTicksIndex;
            this.eventLogRepository = eventLogRepository;
            this.globalTime = globalTime;
            this.childTaskIndex = childTaskIndex;
        }

        public IEnumerable<Tuple<string, ColumnInfo>> GetAllTasksInStates(long toTicks, params TaskState[] states)
        {
            return states.SelectMany(
                state =>
                    {
                        var ids = minimalStartTicksIndex.GetTaskIds(state, toTicks).ToArray();
                        return ids;
                    });
        }

        public ColumnInfo AddMeta(TaskMetaInformation meta)
        {
            var nowTicks = Math.Max((meta.LastModificationTicks ?? 0) + 1, globalTime.GetNowTicks());
            meta.LastModificationTicks = nowTicks;
            eventLogRepository.AddEvent(meta.Id, nowTicks);
            var columnInfo = minimalStartTicksIndex.IndexMeta(meta);
            if(meta.State == TaskState.New)
                childTaskIndex.AddMeta(meta);
            storage.Write(meta.Id, meta);

            var oldMeta = meta.GetSnapshot();
            if(oldMeta != null)
            {
                var oldColumnInfo = TicksNameHelper.GetColumnInfo(oldMeta);
                if(!oldColumnInfo.Equals(columnInfo))
                    minimalStartTicksIndex.UnindexMeta(meta.Id, oldColumnInfo);
            }

            meta.MakeSnapshot();
            return columnInfo;
        }

        public TaskMetaInformation GetMeta(string taskId)
        {
            var meta = storage.Read(taskId);
            if(meta != null)
                meta.MakeSnapshot();
            return meta;
        }

        public TaskMetaInformation[] GetMetas(string[] taskIds)
        {
            var metas = storage.Read(taskIds);
            metas.ForEach(x => x.MakeSnapshot());
            return metas;
        }

        public TaskMetaInformation[] GetMetasQuiet(string[] taskIds)
        {
            var metas = storage.ReadQuiet(taskIds);
            metas.Where(x => x != null).ForEach(x => x.MakeSnapshot());
            return metas;
        }

        private readonly ITaskMetaInformationBlobStorage storage;
        private readonly ITaskMinimalStartTicksIndex minimalStartTicksIndex;
        private readonly IEventLogRepository eventLogRepository;
        private readonly IGlobalTime globalTime;
        private readonly IChildTaskIndex childTaskIndex;
    }
}