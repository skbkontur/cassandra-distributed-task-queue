using System;
using System.Collections.Generic;
using System.Linq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;

namespace RemoteQueue.Cassandra.Repositories
{
    public class HandleTasksMetaStorage : IHandleTasksMetaStorage
    {
        public HandleTasksMetaStorage(
            ITaskMetaInformationBlobStorage storage,
            ITaskMinimalStartTicksIndex minimalStartTicksIndex)
        {
            this.storage = storage;
            this.minimalStartTicksIndex = minimalStartTicksIndex;
        }

        public IEnumerable<string> GetAllTasksInStates(long toTicks, params TaskState[] states)
        {
            return GetAllTasksInStateBase(toTicks, states);
        }

        public IEnumerable<string> GetAllTasksInStatesFromTicks(long fromTicks, params TaskState[] states)
        {
            return GetAllTasksInStateBase(DateTime.UtcNow.Ticks, states, fromTicks);
        }

        public IEnumerable<string> GetReverseAllTasksInStatesOrder(long ticks, params TaskState[] states)
        {
            return GetAllTasksInStateBase(ticks, states, 0, true);
        }

        public void AddMeta(TaskMetaInformation meta)
        {
            storage.Write(meta.Id, meta);
            minimalStartTicksIndex.IndexMeta(meta);
        }

        public TaskMetaInformation GetMeta(string taskId)
        {
            return storage.Read(taskId);
        }

        private IEnumerable<string> GetAllTasksInStateBase(long ticks, IEnumerable<TaskState> states, long fromTicks = 0, bool reverseOrder = false)
        {
            IEnumerable<string> res = new List<string>();
            var idGroups = states.Select(state => minimalStartTicksIndex.GetTaskIds(state, ticks, fromTicks, reverseOrder));
            return idGroups.Aggregate(res, (current, idGroup) => current.Concat(idGroup));
        }

        private readonly ITaskMetaInformationBlobStorage storage;
        private readonly ITaskMinimalStartTicksIndex minimalStartTicksIndex;
    }
}