using System;
using System.Collections.Generic;
using System.Linq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;

using MoreLinq;

namespace RemoteQueue.Cassandra.Repositories
{
    public class HandleTasksMetaStorage : IHandleTasksMetaStorage
    {
        public HandleTasksMetaStorage(
            ITaskMetaInformationBlobStorage storage,
            ITaskMinimalStartTicksIndex minimalStartTicksIndex,
            IEventLogRepository eventLogRepository,
            IGlobalTime globalTime)
        {
            this.storage = storage;
            this.minimalStartTicksIndex = minimalStartTicksIndex;
            this.eventLogRepository = eventLogRepository;
            this.globalTime = globalTime;
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

        public void AddMeta(TaskMetaInformation meta)
        {
            var nowTicks = globalTime.GetNowTicks();
            meta.LastModificationTicks = nowTicks;
            eventLogRepository.AddEvent(meta.Id, nowTicks);
            storage.Write(meta.Id, meta);
            minimalStartTicksIndex.IndexMeta(meta);
            meta.MakeSnapshot();
        }

        public TaskMetaInformation GetMeta(string taskId)
        {
            var meta = storage.Read(taskId);
            meta.MakeSnapshot();
            return meta;
        }

        public TaskMetaInformation[] GetMetas(string[] taskIds)
        {
            var metas = storage.Read(taskIds);
            metas.ForEach(x => x.MakeSnapshot());
            return metas;
        }

        private readonly ITaskMetaInformationBlobStorage storage;
        private readonly ITaskMinimalStartTicksIndex minimalStartTicksIndex;
        private readonly IEventLogRepository eventLogRepository;
        private readonly IGlobalTime globalTime;
    }
}