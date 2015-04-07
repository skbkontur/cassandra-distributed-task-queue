using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using GroboContainer.Infection;

using log4net;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.Utils;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public class TaskIndexController : ITaskIndexController
    {
        public TaskIndexController(IEventLogRepository eventLogRepository,
                                   IMetaCachedReader reader,
                                   ITaskMetaProcessor taskMetaProcessor,
                                   LastReadTicksStorage lastReadTicksStorage,
                                   IGlobalTime globalTime,
                                   IRemoteLockCreator remoteLockCreator,
                                   int maxBatch)
        {
            this.eventLogRepository = eventLogRepository;
            this.reader = reader;
            this.taskMetaProcessor = taskMetaProcessor;
            this.globalTime = globalTime;
            this.remoteLockCreator = remoteLockCreator;
            this.lastReadTicksStorage = lastReadTicksStorage;
            this.maxBatch = maxBatch;
            unstableZoneTicks = eventLogRepository.UnstableZoneLength.Ticks;
            unprocessedEventsMap = new UnprocessedEventsMap(unstableZoneTicks * 2);
            lastTicks = long.MinValue;
        }

        [ContainerConstructor]
        public TaskIndexController(IEventLogRepository eventLogRepository,
                                   IMetaCachedReader reader,
                                   ITaskMetaProcessor taskMetaProcessor,
                                   LastReadTicksStorage lastReadTicksStorage,
                                   IGlobalTime globalTime, IRemoteLockCreator remoteLockCreator
            )
            : this(eventLogRepository,
                   reader,
                   taskMetaProcessor,
                   lastReadTicksStorage,
                   globalTime,
                   remoteLockCreator,
                   TaskIndexSettings.MaxBatch)
        {
        }

        private long GetNow()
        {
            return globalTime.GetNowTicks();
        }

        public void Dispose()
        {
            if(distributedLock != null)
            {
                //NOTE close lock if shutdown by container
                distributedLock.Dispose();
                distributedLock = null;
                logger.InfoFormat("Distributed lock released");
            }
        }

        private TaskMetaInformation[] CutMetas(TaskMetaInformation[] metas)
        {
            var ticks = MinTicksHack;
            if(ticks <= 0)
                return metas;
            var list = new List<TaskMetaInformation>();
            foreach(var taskMetaInformation in metas)
            {
                if(taskMetaInformation.Ticks > ticks)
                    list.Add(taskMetaInformation);
            }
            return list.ToArray();
        }

        public void ProcessNewEvents()
        {
            lock(lockObject)
            {
                if(!DistributedLockAcquired())
                    return;
                var now = GetNow();

                if(lastTicks == long.MinValue)
                    LoadLastState();

                var hasEvents = false;

                var unprocessedEvents = unprocessedEventsMap.GetUnprocessedEvents(now);
                var newEvents = GetEvents(lastTicks);

                unprocessedEvents.Concat(newEvents)
                                 .Batch(maxBatch, Enumerable.ToArray)
                                 .ForEach(events =>
                                     {
                                         hasEvents = true;
                                         ProcessEventsBatch(events, now);
                                     });

                if(!hasEvents)
                    ProcessEventsBatch(new TaskMetaUpdatedEvent[0], now);

                lastTicks = now;
            }
        }

        private void LoadLastState()
        {
            logger.LogInfoFormat("Loading saved state");
            var lastReadTicks = lastReadTicksStorage.GetLastReadTicks();
            Interlocked.Exchange(ref lastTicks, lastReadTicks);
            logger.InfoFormat("Last state loaded. LastTicks={0}", DateTimeFormatter.FormatWithMsAndTicks(lastReadTicks));
        }

        private void ProcessEventsBatch(TaskMetaUpdatedEvent[] taskMetaUpdatedEvents, long now)
        {
            var actualMetas = ReadActualMetas(taskMetaUpdatedEvents, now);

            actualMetas = CutMetas(actualMetas);

            taskMetaProcessor.IndexMetas(actualMetas);

            var ticks = GetSafeTimeForSnapshot(now, actualMetas);
            SaveSnapshot(ticks);
        }

        private TaskMetaInformation[] ReadActualMetas(TaskMetaUpdatedEvent[] taskMetaUpdatedEvents, long now)
        {
            var taskMetaInformations = reader.ReadActualMetasQuiet(taskMetaUpdatedEvents, now);
            var actualMetas = new List<TaskMetaInformation>();
            for(var i = 0; i < taskMetaInformations.Length; i++)
            {
                var taskMetaInformation = taskMetaInformations[i];
                if(taskMetaInformation != null)
                {
                    actualMetas.Add(taskMetaInformation);
                    unprocessedEventsMap.RemoveEvent(taskMetaUpdatedEvents[i]);
                }
                else
                    unprocessedEventsMap.AddEvent(taskMetaUpdatedEvents[i]);
            }

            var actualMetasArray = actualMetas.ToArray();
            return actualMetasArray;
        }

        public long MinTicksHack { get { return Interlocked.Read(ref minTicksHack); } }

        public void SetMinTicksHack(long minTicks)
        {
            Interlocked.Exchange(ref minTicksHack, minTicks);
        }

        private long GetSafeTimeForSnapshot(long now, TaskMetaInformation[] taskMetaInformations)
        {
            var lastTicksEstimation = GetMinTicks(taskMetaInformations, now);
            var oldestEventTime = unprocessedEventsMap.GetOldestEventTime();
            if(oldestEventTime != null)
                lastTicksEstimation = Math.Min(lastTicksEstimation, oldestEventTime.Value);
            return lastTicksEstimation;
        }

        private void SaveSnapshot(long ticks)
        {
            lastReadTicksStorage.SetLastReadTicks(ticks);
        }

        private static long GetMinTicks(TaskMetaInformation[] taskMetaInformations, long now)
        {
            if(taskMetaInformations.Length <= 0)
                return now;
            var minTicks = taskMetaInformations.Min(x => x.LastModificationTicks.Value);
            return minTicks;
        }

        private IEnumerable<TaskMetaUpdatedEvent> GetEvents(long fromTicks)
        {
            //todo do not read event twice
            return eventLogRepository.GetEvents(fromTicks - unstableZoneTicks, maxBatch);
        }

        public bool IsDistributedLockAcquired()
        {
            return distributedLock != null;
        }

        private const string lockId = "TaskSearch_Loading_Lock";

        private bool DistributedLockAcquired()
        {
            if(IsDistributedLockAcquired())
                return true;
            IRemoteLock @lock;
            if(remoteLockCreator.TryGetLock(lockId, out @lock))
            {
                distributedLock = @lock;
                logger.InfoFormat("Distributed lock acquired.");
                return true;
            }
            return false;
        }

        private static readonly ILog logger = LogManager.GetLogger("TaskIndexController");

        private long minTicksHack;
        private long lastTicks;
        private volatile IRemoteLock distributedLock;

        private readonly ITaskMetaProcessor taskMetaProcessor;
        private readonly LastReadTicksStorage lastReadTicksStorage;
        private readonly IRemoteLockCreator remoteLockCreator;
        private readonly IGlobalTime globalTime;
        private readonly IEventLogRepository eventLogRepository;
        private readonly IMetaCachedReader reader;
        private readonly UnprocessedEventsMap unprocessedEventsMap;

        private readonly object lockObject = new object();

        private readonly long unstableZoneTicks;
        private readonly int maxBatch;
    }
}