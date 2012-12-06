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

        public IEnumerable<string> GetAllTasksInStates(long ticks, params TaskState[] states)
        {
            IEnumerable<string> res = new List<string>();
            var idGroups = states.Select(state => minimalStartTicksIndex.GetTaskIds(state, ticks));
            return idGroups.Aggregate(res, (current, idGroup) => current.Concat(idGroup));
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

        private readonly ITaskMetaInformationBlobStorage storage;
        private readonly ITaskMinimalStartTicksIndex minimalStartTicksIndex;
    }
}