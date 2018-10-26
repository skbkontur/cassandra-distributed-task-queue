using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Configuration;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.Core.EventFeeds.Building;
using SKBKontur.Catalogue.Core.EventFeeds.OffsetStorages;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.Json;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteTaskQueue.Monitoring.TaskCounter
{
    [SuppressMessage(
        "ReSharper",
        "InconsistentlySynchronizedField",
        Justification = "RtqTaskCounterEventFeeder is essentially single-threaded because of WithSingleLeaderElectionKey() configuration, so there is no need in locks around RtqTaskCounterState.LastBladeOffset")]
    public class RtqTaskCounterStateManager
    {
        public RtqTaskCounterStateManager(ISerializer serializer,
                                          ITaskDataRegistry taskDataRegistry,
                                          IRtqTaskCounterStateStorage stateStorage,
                                          RtqTaskCounterSettings settings,
                                          RtqEventLogOffsetInterpreter offsetInterpreter,
                                          RtqMonitoringPerfGraphiteReporter perfGraphiteReporter)
        {
            this.serializer = serializer;
            this.taskDataRegistry = taskDataRegistry;
            this.stateStorage = stateStorage;
            this.settings = settings;
            this.offsetInterpreter = offsetInterpreter;
            this.perfGraphiteReporter = perfGraphiteReporter;
            Blades = settings.BladeDelays.Select((delay, index) => new BladeId($"{CompositeFeedKey}_Blade{index}", delay)).ToArray();
            state = perfGraphiteReporter.ReportTiming("LoadPersistedState", LoadPersistedState, out var timer);
            Log.For(this).Info($"Loaded state with LastBladeOffset: {state.LastBladeOffset}, TaskMetas.Count: {state.TaskMetas.Count} in {timer.Elapsed}");
        }

        [NotNull, ItemNotNull]
        public BladeId[] Blades { get; }

        [NotNull]
        public string CompositeFeedKey { get; } = "RtqTaskCounterFeed";

        public bool EventFeedIsRunning { get; private set; }

        public void Initialize()
        {
            EventFeedIsRunning = true;
        }

        [NotNull]
        private RtqTaskCounterState LoadPersistedState()
        {
            var serializedState = stateStorage.TryRead();
            return serializedState == null ? new RtqTaskCounterState() : serializer.Deserialize<RtqTaskCounterState>(serializedState);
        }

        public void MaybePersistState()
        {
            if (lastStatePersistedTimestamp != null && Timestamp.Now - lastStatePersistedTimestamp < settings.StatePersistingInterval)
                return;

            var lastBladeOffset = lastBladeOffsetStorage.Read();
            if (string.IsNullOrEmpty(lastBladeOffset))
                return;

            state.LastBladeOffset = lastBladeOffset;

            perfGraphiteReporter.ReportTiming("PersistState", DoPersistState, out var timer);
            Log.For(this).Info($"Persisted state with LastBladeOffset: {state.LastBladeOffset}, TaskMetas.Count: {state.TaskMetas.Count} in {timer.Elapsed}");

            lastStatePersistedTimestamp = Timestamp.Now;
        }

        private void DoPersistState()
        {
            var serializedState = serializer.Serialize(state);
            stateStorage.Write(serializedState);
        }

        [NotNull]
        public IOffsetStorage<string> CreateOffsetStorage([NotNull] BladeId bladeId)
        {
            var initialOffset = offsetInterpreter.GetMaxOffsetForTimestamp(Timestamp.Now - bladeId.Delay);
            if (bladeId.BladeKey != Blades.Last().BladeKey)
                return new InMemoryOffsetStorage<string>(initialOffset);
            if (!string.IsNullOrEmpty(state.LastBladeOffset))
                initialOffset = state.LastBladeOffset;
            lastBladeOffsetStorage = new InMemoryOffsetStorage<string>(initialOffset);
            return lastBladeOffsetStorage;
        }

        public bool NeedToProcessEvent([NotNull] TaskMetaUpdatedEvent @event)
        {
            lock (taskMetasLocker)
                return !state.TaskMetas.TryGetValue(@event.TaskId, out var taskMeta) || taskMeta.LastModificationTicks < @event.Ticks;
        }

        public void UpdateTaskState([NotNull] TaskMetaInformation taskMetaInformation)
        {
            lock (taskMetasLocker)
            {
                if (IsTaskStateTerminal(taskMetaInformation.State))
                    state.TaskMetas.Remove(taskMetaInformation.Id);
                else
                {
                    if (!state.TaskMetas.TryGetValue(taskMetaInformation.Id, out var taskMeta))
                    {
                        taskMeta = new RtqTaskCounterStateTaskMeta(taskMetaInformation.Name);
                        state.TaskMetas.Add(taskMetaInformation.Id, taskMeta);
                    }
                    taskMeta.State = taskMetaInformation.State;
                    taskMeta.MinimalStartTimestamp = taskMetaInformation.GetMinimalStartTimestamp();
                    taskMeta.LastModificationTicks = taskMetaInformation.LastModificationTicks;
                }
            }
        }

        private static bool IsTaskStateTerminal(TaskState taskState)
        {
            return taskState == TaskState.Finished || taskState == TaskState.Fatal || taskState == TaskState.Canceled;
        }

        [NotNull]
        public RtqTaskCounters GetTaskCounters([NotNull] Timestamp now)
        {
            var taskMetas = CloneState();

            var lostTasks = taskMetas.Where(x => x.Value.MinimalStartTimestamp < now - settings.PendingTaskExecutionUpperBound).ToArray();
            if (lostTasks.Length > 0)
                Log.For(this).Warn($"Probably {lostTasks.Length} lost tasks detected: {lostTasks.ToPrettyJson()}");

            var taskCounters = new RtqTaskCounters
                {
                    LostTasksCount = lostTasks.Length,
                    PendingTaskCountsTotal = taskMetas.GroupBy(x => x.Value.State)
                                                      .ToDictionary(g => g.Key, g => g.Count()),
                    PendingTaskCountsByName = taskMetas.GroupBy(x => x.Value.Name)
                                                       .ToDictionary(g => g.Key, g => g.GroupBy(x => x.Value.State)
                                                                                       .ToDictionary(gg => gg.Key, gg => g.Count()))
                };
            NormalizeTaskCounters(taskCounters);

            return taskCounters;
        }

        private void NormalizeTaskCounters([NotNull] RtqTaskCounters taskCounters)
        {
            foreach (var taskState in Enum.GetValues(typeof(TaskState)).Cast<TaskState>().Where(x => !IsTaskStateTerminal(x)))
            {
                NormalizeTaskCounters(taskCounters.PendingTaskCountsTotal, taskState);
                foreach (var taskName in taskDataRegistry.GetAllTaskNames())
                {
                    if (!taskCounters.PendingTaskCountsByName.ContainsKey(taskName))
                        taskCounters.PendingTaskCountsByName.Add(taskName, new Dictionary<TaskState, int>());
                }
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
        private Dictionary<string, RtqTaskCounterStateTaskMeta> CloneState()
        {
            Dictionary<string, RtqTaskCounterStateTaskMeta> taskMetas;
            var timer = Stopwatch.StartNew();
            try
            {
                lock (taskMetasLocker)
                {
                    var serializedState = serializer.Serialize(state.TaskMetas);
                    taskMetas = serializer.Deserialize<Dictionary<string, RtqTaskCounterStateTaskMeta>>(serializedState);
                }
            }
            finally
            {
                timer.Stop();
            }
            Log.For(this).Info($"Cloned state with TaskMetas.Count: {taskMetas.Count} in {timer.Elapsed}");
            return taskMetas;
        }

        private readonly ISerializer serializer;
        private readonly ITaskDataRegistry taskDataRegistry;
        private readonly IRtqTaskCounterStateStorage stateStorage;
        private readonly RtqTaskCounterSettings settings;
        private readonly RtqEventLogOffsetInterpreter offsetInterpreter;
        private readonly RtqMonitoringPerfGraphiteReporter perfGraphiteReporter;
        private readonly RtqTaskCounterState state;
        private InMemoryOffsetStorage<string> lastBladeOffsetStorage;
        private Timestamp lastStatePersistedTimestamp;
        private readonly object taskMetasLocker = new object();
    }
}