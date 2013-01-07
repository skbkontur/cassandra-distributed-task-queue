using System;
using System.Collections.Generic;
using System.Linq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;

namespace RemoteQueue.Cassandra.Repositories
{
    public class HandleTasksMetaStorage : IHandleTasksMetaStorage
    {
        public HandleTasksMetaStorage(
            ITaskMetaInformationBlobStorage storage,
            ITaskMinimalStartTicksIndex minimalStartTicksIndex,
            IEventLogRepository eventLogRepository)
        {
            this.storage = storage;
            this.minimalStartTicksIndex = minimalStartTicksIndex;
            this.eventLogRepository = eventLogRepository;
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
            eventLogRepository.AddEvent(new TaskMetaUpdatedEvent {TaskId = meta.Id});
            storage.Write(meta.Id, meta);
            minimalStartTicksIndex.IndexMeta(meta);
        }

        public TaskMetaInformation GetMeta(string taskId)
        {
            return storage.Read(taskId);
        }

        private readonly ITaskMetaInformationBlobStorage storage;
        private readonly ITaskMinimalStartTicksIndex minimalStartTicksIndex;
        private readonly IEventLogRepository eventLogRepository;
    }
}