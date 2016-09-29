using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
                var liveRecords = minimalStartTicksIndex.GetRecords(taskIndexShardKey, toTicks, batchSize : 2000).Take(10000).ToArray();
                liveRecordsByKey.Add(taskIndexShardKey, liveRecords);
                if(liveRecords.Any())
                    Log.For(this).InfoFormat("Got {0} live minimalStartTicksIndex records for taskIndexShardKey: {1}; Oldest live record: {2}", liveRecords.Length, taskIndexShardKey, liveRecords.First());
            }
            return Shuffle(liveRecordsByKey.SelectMany(x => x.Value).ToArray());
        }

        [NotNull]
        public TaskIndexRecord AddMeta([NotNull] TaskMetaInformation taskMeta, [CanBeNull] TaskIndexRecord oldTaskIndexRecord)
        {
            var globalNowTicks = globalTime.UpdateNowTicks();
            var nowTicks = Math.Max((taskMeta.LastModificationTicks ?? 0) + 1, globalNowTicks);
            taskMeta.LastModificationTicks = nowTicks;
            eventLogRepository.AddEvent(taskMeta, nowTicks);
            var newIndexRecord = FormatIndexRecord(taskMeta);
            minimalStartTicksIndex.AddRecord(newIndexRecord, globalNowTicks, taskMeta.GetTtl());
            if(taskMeta.State == TaskState.New)
                childTaskIndex.WriteIndexRecord(taskMeta, globalNowTicks);
            taskMetaStorage.Write(taskMeta, globalNowTicks);
            if(oldTaskIndexRecord != null)
                minimalStartTicksIndex.RemoveRecord(oldTaskIndexRecord, globalNowTicks);
            return newIndexRecord;
        }

        public void ProlongMetaTtl([NotNull] TaskMetaInformation taskMeta)
        {
            var globalNowTicks = globalTime.UpdateNowTicks();
            minimalStartTicksIndex.WriteRecord(FormatIndexRecord(taskMeta), globalNowTicks, taskMeta.GetTtl());
            childTaskIndex.WriteIndexRecord(taskMeta, globalNowTicks);
            taskMetaStorage.Write(taskMeta, globalNowTicks);
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

        [NotNull]
        private T[] Shuffle<T>([NotNull] T[] array)
        {
            for(var i = 0; i < array.Length; i++)
            {
                var r = i + (int)(random.Value.NextDouble() * (array.Length - i));
                var t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
            return array;
        }

        private readonly ITaskMetaStorage taskMetaStorage;
        private readonly ITaskMinimalStartTicksIndex minimalStartTicksIndex;
        private readonly IEventLogRepository eventLogRepository;
        private readonly IGlobalTime globalTime;
        private readonly IChildTaskIndex childTaskIndex;
        private readonly ITaskDataRegistry taskDataRegistry;
        private readonly ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));
    }
}