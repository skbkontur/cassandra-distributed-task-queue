using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.Core.EventFeeds.Building;
using SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages;
using SKBKontur.Catalogue.Objects.Json;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter
{
    // NB! RtqTaskCounterEventFeeder is essentially single-threaded because of WithSingleLeaderElectionKey() configuration
    public class RtqTaskCounterStateManager
    {
        public RtqTaskCounterStateManager(ILog logger,
                                          ISerializer serializer,
                                          IRtqTaskDataRegistry taskDataRegistry,
                                          IRtqTaskCounterStateStorage stateStorage,
                                          RtqTaskCounterSettings settings,
                                          RtqEventLogOffsetInterpreter offsetInterpreter,
                                          RtqMonitoringPerfGraphiteReporter perfGraphiteReporter)
        {
            this.logger = logger;
            this.serializer = serializer;
            this.taskDataRegistry = taskDataRegistry;
            this.stateStorage = stateStorage;
            this.settings = settings;
            this.offsetInterpreter = offsetInterpreter;
            this.perfGraphiteReporter = perfGraphiteReporter;
            Blades = settings.BladeDelays.Select((delay, index) => new BladeId($"{CompositeFeedKey}_Blade{index}", delay)).ToArray();
        }

        [NotNull, ItemNotNull]
        public BladeId[] Blades { get; }

        [NotNull]
        public string CompositeFeedKey { get; } = "RtqTaskCounter";

        public bool EventFeedIsRunning { get; private set; }

        [NotNull]
        public IOffsetStorage<string> CreateOffsetStorage([NotNull] BladeId bladeId)
        {
            var offsetStorage = new InMemoryOffsetStorage<string>();
            bladeOffsetStorages.Add(bladeId.BladeKey, offsetStorage);
            return offsetStorage;
        }

        public void ResetLocalState()
        {
            if (++resetLocalStateCalls % Blades.Length != 1)
                return;

            EventFeedIsRunning = !EventFeedIsRunning;
            if (EventFeedIsRunning)
            {
                var persistedLastBladeOffset = LoadPersistedState();
                AdjustBladeOffsets(persistedLastBladeOffset);
            }
            logger.Info($"EventFeedIsRunning flag is switched to: {EventFeedIsRunning}");
        }

        private void AdjustBladeOffsets([CanBeNull] string persistedLastBladeOffset)
        {
            foreach (var bladeId in Blades)
            {
                var bladeOffset = offsetInterpreter.GetMaxOffsetForTimestamp(Timestamp.Now - bladeId.Delay);
                if (bladeId == Blades.Last() && !string.IsNullOrEmpty(persistedLastBladeOffset))
                    bladeOffset = persistedLastBladeOffset;
                bladeOffsetStorages[bladeId.BladeKey].Write(bladeOffset);
                logger.Info($"Blade {bladeId} is moved to offset: {offsetInterpreter.Format(bladeOffset)}");
            }
        }

        [CanBeNull]
        private string LoadPersistedState()
        {
            var (persistedLastBladeOffset, persistedTaskMetasCount) = perfGraphiteReporter.ReportTiming("LoadPersistedState", DoLoadPersistedState, out var timer);
            logger.Info($"Loaded state with LastBladeOffset: {offsetInterpreter.Format(persistedLastBladeOffset)}, TaskMetas.Count: {persistedTaskMetasCount} in {timer.Elapsed}");
            return persistedLastBladeOffset;
        }

        private ( /*[CanBeNull]*/ string PersistedLastBladeOffset, int PersistedTaskMetasCount) DoLoadPersistedState()
        {
            var serializedState = stateStorage.TryRead();
            if (serializedState == null)
                return (PersistedLastBladeOffset : null, PersistedTaskMetasCount : 0);

            var persistedState = serializer.Deserialize<RtqTaskCounterState>(serializedState);
            var result = (persistedState.LastBladeOffset, persistedState.TaskMetas.Count);

            lock (state)
            {
                state.TaskMetas = persistedState.TaskMetas;
                state.LastBladeOffset = persistedState.LastBladeOffset;
            }

            return result;
        }

        public void MaybePersistState()
        {
            var now = Timestamp.Now;
            if (lastStatePersistedTimestamp != null && now - lastStatePersistedTimestamp < settings.StatePersistingInterval)
                return;

            var garbageTaskMetasCount = perfGraphiteReporter.ReportTiming("CollectGarbageInState", () => CollectGarbageInState(now), out var timer);
            perfGraphiteReporter.Increment("GarbageTaskMetas", garbageTaskMetasCount);
            logger.Info($"Collected garbage in state with garbageTaskMetasCount: {garbageTaskMetasCount} in {timer.Elapsed}");

            var lastBladeOffset = bladeOffsetStorages[Blades.Last().BladeKey].Read();
            if (string.IsNullOrEmpty(lastBladeOffset))
                return;

            var persistedState = perfGraphiteReporter.ReportTiming("PersistState", () => DoPersistState(lastBladeOffset), out timer);
            perfGraphiteReporter.Increment("PersistedTaskMetas", persistedState.TaskMetas.Count);
            logger.Info($"Persisted state with lastBladeOffset: {offsetInterpreter.Format(persistedState.LastBladeOffset)}, persistedTaskMetasCount: {persistedState.TaskMetas.Count} in {timer.Elapsed}");

            lastStatePersistedTimestamp = Timestamp.Now;
        }

        private int CollectGarbageInState([NotNull] Timestamp now)
        {
            var taskIdsToRemove = new List<string>();
            lock (state)
            {
                foreach (var kvp in state.TaskMetas)
                {
                    var taskMeta = kvp.Value;
                    if (IsTaskStateTerminal(taskMeta.State) && taskMeta.LastStateUpdateTimestamp + settings.StateGarbageTtl < now)
                        taskIdsToRemove.Add(kvp.Key);
                }
                foreach (var taskId in taskIdsToRemove)
                    state.TaskMetas.Remove(taskId);
            }
            return taskIdsToRemove.Count;
        }

        private static bool IsTaskStateTerminal(TaskState taskState)
        {
            return taskState == TaskState.Finished || taskState == TaskState.Fatal || taskState == TaskState.Canceled;
        }

        [NotNull]
        private RtqTaskCounterState DoPersistState([NotNull] string lastBladeOffset)
        {
            var stateToPersist = new RtqTaskCounterState {LastBladeOffset = lastBladeOffset};
            lock (state)
            {
                foreach (var kvp in state.TaskMetas)
                {
                    var taskMeta = kvp.Value;
                    if (!IsTaskStateTerminal(taskMeta.State))
                        stateToPersist.TaskMetas.Add(kvp.Key, taskMeta);
                }
            }

            var serializedState = serializer.Serialize(stateToPersist);
            stateStorage.Write(serializedState);

            return stateToPersist;
        }

        public bool NeedToProcessEvent([NotNull] TaskMetaUpdatedEvent @event)
        {
            lock (state)
                return !state.TaskMetas.TryGetValue(@event.TaskId, out var taskMeta) || taskMeta.LastModificationTicks < @event.Ticks;
        }

        public void UpdateTaskState([NotNull] TaskMetaInformation taskMetaInformation)
        {
            lock (state)
            {
                if (!state.TaskMetas.TryGetValue(taskMetaInformation.Id, out var taskMeta))
                {
                    taskMeta = new RtqTaskCounterStateTaskMeta(taskMetaInformation.Name);
                    state.TaskMetas.Add(taskMetaInformation.Id, taskMeta);
                }
                taskMeta.State = taskMetaInformation.State;
                taskMeta.MinimalStartTimestamp = taskMetaInformation.GetMinimalStartTimestamp();
                taskMeta.LastModificationTicks = taskMetaInformation.LastModificationTicks;
                taskMeta.LastStateUpdateTimestamp = Timestamp.Now;
            }
        }

        [NotNull]
        public RtqTaskCounters GetTaskCounters([NotNull] Timestamp now)
        {
            var pendingTaskMetas = GetPendingTaskMetas();

            var lostTasks = pendingTaskMetas.Where(x => x.MinimalStartTimestamp < now - settings.PendingTaskExecutionUpperBound).ToArray();
            if (lostTasks.Length > 0)
                logger.Warn($"Probably {lostTasks.Length} lost tasks detected: {lostTasks.ToPrettyJson()}");

            var taskCounters = new RtqTaskCounters
                {
                    LostTasksCount = lostTasks.Length,
                    PendingTaskCountsTotal = pendingTaskMetas.GroupBy(x => x.State)
                                                             .ToDictionary(g => g.Key, g => g.Count()),
                    PendingTaskCountsByName = pendingTaskMetas.GroupBy(x => x.Name)
                                                              .ToDictionary(g => g.Key, g => g.GroupBy(x => x.State)
                                                                                              .ToDictionary(gg => gg.Key, gg => gg.Count()))
                };
            NormalizeTaskCounters(taskCounters);

            return taskCounters;
        }

        private void NormalizeTaskCounters([NotNull] RtqTaskCounters taskCounters)
        {
            foreach (var taskName in taskDataRegistry.GetAllTaskNames())
            {
                if (!taskCounters.PendingTaskCountsByName.ContainsKey(taskName))
                    taskCounters.PendingTaskCountsByName.Add(taskName, new Dictionary<TaskState, int>());
            }
            foreach (var taskState in Enum.GetValues(typeof(TaskState)).Cast<TaskState>().Where(x => !IsTaskStateTerminal(x)))
            {
                NormalizeTaskCounters(taskCounters.PendingTaskCountsTotal, taskState);
                foreach (var kvp in taskCounters.PendingTaskCountsByName)
                    NormalizeTaskCounters(kvp.Value, taskState);
            }

            void NormalizeTaskCounters(Dictionary<TaskState, int> taskCountsByState, TaskState taskState)
            {
                if (!taskCountsByState.ContainsKey(taskState))
                    taskCountsByState.Add(taskState, 0);
            }
        }

        [NotNull]
        private List<RtqTaskCounterStateTaskMeta> GetPendingTaskMetas()
        {
            int taskMetasCount;
            var pendingTaskMetas = new List<RtqTaskCounterStateTaskMeta>();
            var timer = Stopwatch.StartNew();
            try
            {
                lock (state)
                {
                    taskMetasCount = state.TaskMetas.Count;
                    foreach (var kvp in state.TaskMetas)
                    {
                        var taskMeta = kvp.Value;
                        if (!IsTaskStateTerminal(taskMeta.State))
                            pendingTaskMetas.Add(taskMeta);
                    }
                }
            }
            finally
            {
                timer.Stop();
            }
            logger.Info($"Selected {pendingTaskMetas.Count} pendingTaskMetas from {taskMetasCount} taskMetas in {timer.Elapsed}");
            return pendingTaskMetas;
        }

        private int resetLocalStateCalls;
        private Timestamp lastStatePersistedTimestamp;
        private readonly ILog logger;
        private readonly ISerializer serializer;
        private readonly IRtqTaskDataRegistry taskDataRegistry;
        private readonly IRtqTaskCounterStateStorage stateStorage;
        private readonly RtqTaskCounterSettings settings;
        private readonly RtqEventLogOffsetInterpreter offsetInterpreter;
        private readonly RtqMonitoringPerfGraphiteReporter perfGraphiteReporter;
        private readonly RtqTaskCounterState state = new RtqTaskCounterState();
        private readonly Dictionary<string, InMemoryOffsetStorage<string>> bladeOffsetStorages = new Dictionary<string, InMemoryOffsetStorage<string>>();
    }
}