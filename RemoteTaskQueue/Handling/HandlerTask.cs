using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using Metrics;

using SkbKontur.Cassandra.DistributedLock;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.BlobStorages;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling.ExecutionContext;
using SkbKontur.Cassandra.DistributedTaskQueue.LocalTasks.TaskQueue;
using SkbKontur.Cassandra.DistributedTaskQueue.Profiling;
using SkbKontur.Cassandra.GlobalTimestamp;
using SkbKontur.Cassandra.TimeBasedUuid;

using SKBKontur.Catalogue.GrobufExtensions;
using SKBKontur.Catalogue.Objects;

using Vostok.Logging.Abstractions;

using MetricsContext = SkbKontur.Cassandra.DistributedTaskQueue.Profiling.MetricsContext;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    internal class HandlerTask
    {
        public HandlerTask(
            [NotNull] TaskIndexRecord taskIndexRecord,
            TaskQueueReason reason,
            [CanBeNull] TaskMetaInformation taskMeta,
            IRtqTaskHandlerRegistry taskHandlerRegistry,
            IRtqInternals rtqInternals)
        {
            this.taskIndexRecord = taskIndexRecord;
            this.reason = reason;
            this.taskMeta = taskMeta;
            this.taskHandlerRegistry = taskHandlerRegistry;
            serializer = rtqInternals.Serializer;
            taskProducer = rtqInternals.TaskProducer;
            handleTaskCollection = rtqInternals.HandleTaskCollection;
            remoteLockCreator = rtqInternals.RemoteLockCreator;
            taskExceptionInfoStorage = rtqInternals.TaskExceptionInfoStorage;
            handleTasksMetaStorage = rtqInternals.HandleTasksMetaStorage;
            taskMinimalStartTicksIndex = rtqInternals.TaskMinimalStartTicksIndex;
            rtqProfiler = rtqInternals.Profiler;
            globalTime = rtqInternals.GlobalTime;
            taskTtl = rtqInternals.TaskTtl;
            logger = rtqInternals.Logger.ForContext(nameof(HandlerTask));
            taskShardMetricsContext = MetricsContext.For($"Shards.{taskIndexRecord.TaskIndexShardKey.TaskTopic}.{taskIndexRecord.TaskIndexShardKey.TaskState}.Tasks");
        }

        public LocalTaskProcessingResult RunTask()
        {
            taskShardMetricsContext.Meter("Started").Mark();
            if (taskMeta == null)
            {
                taskShardMetricsContext.Meter("NoMeta").Mark();
                logger.Error("Удаляем запись индекса, для которой мета так и не записалась: {RtqTaskIndexRecord}", new {RtqTaskIndexRecord = taskIndexRecord});
                taskMinimalStartTicksIndex.RemoveRecord(taskIndexRecord, globalTime.UpdateNowTimestamp().Ticks);
                return LocalTaskProcessingResult.Undefined;
            }
            var localNow = Timestamp.Now;
            if (taskIndexRecord != handleTasksMetaStorage.FormatIndexRecord(taskMeta) && taskIndexRecord.MinimalStartTicks > localNow.Ticks - MaxAllowedIndexInconsistencyDuration.Ticks)
            {
                taskShardMetricsContext.Meter("InconsistentIndexRecord").Mark();
                logger.Debug("taskIndexRecord != IndexRecord(taskMeta), поэтому ждем; taskMeta: {RtqTaskMeta}; taskIndexRecord: {RtqTaskIndexRecord}; localNow: {LocalNow}",
                             new {RtqTaskMeta = taskMeta, RtqTaskIndexRecord = taskIndexRecord, LocalNow = localNow});
                return LocalTaskProcessingResult.Undefined;
            }
            var metricsContext = MetricsContext.For(taskMeta).SubContext(nameof(HandlerTask));
            return TryProcessTaskExclusively(metricsContext);
        }

        private LocalTaskProcessingResult TryProcessTaskExclusively([NotNull] MetricsContext metricsContext)
        {
            metricsContext = metricsContext.SubContext(nameof(TryProcessTaskExclusively));
            using (metricsContext.Timer("Total").NewContext())
            {
                IRemoteLock taskGroupRemoteLock = null;
                if (!string.IsNullOrEmpty(taskMeta.TaskGroupLock))
                {
                    using (metricsContext.Timer("TryGetLock_TaskGroupLock").NewContext())
                    {
                        if (!remoteLockCreator.TryGetLock(taskMeta.TaskGroupLock, out taskGroupRemoteLock))
                        {
                            taskShardMetricsContext.Meter("DidNotGetTaskGroupLock").Mark();
                            logger.Debug("Не смогли взять групповую блокировку {RtqTaskGroupLock} на задачу: {RtqTaskId}", new {RtqTaskGroupLock = taskMeta.TaskGroupLock, RtqTaskId = taskIndexRecord.TaskId});
                            return LocalTaskProcessingResult.Undefined;
                        }
                    }
                    taskShardMetricsContext.Meter("GotTaskGroupLock").Mark();
                }
                try
                {
                    var sw = Stopwatch.StartNew();
                    IRemoteLock remoteLock;
                    using (metricsContext.Timer("TryGetLock_TaskId").NewContext())
                    {
                        if (!remoteLockCreator.TryGetLock(taskIndexRecord.TaskId, out remoteLock))
                        {
                            taskShardMetricsContext.Meter("DidNotGetTaskLock").Mark();
                            logger.Debug("Не смогли взять блокировку на задачу, пропускаем её: {RtqTaskIndexRecord}", new {RtqTaskIndexRecord = taskIndexRecord});
                            return LocalTaskProcessingResult.Undefined;
                        }
                    }
                    taskShardMetricsContext.Meter("GotTaskLock").Mark();
                    LocalTaskProcessingResult result;
                    using (remoteLock)
                        result = ProcessTask(metricsContext);
                    sw.Stop();
                    var isLongRunningTask = sw.Elapsed > longRunningTaskDurationThreshold;
                    logger.Log(new LogEvent(
                                       level : isLongRunningTask ? LogLevel.Warn : LogLevel.Debug,
                                       timestamp : DateTimeOffset.Now,
                                       messageTemplate : "Завершили выполнение задачи {RtqTaskId} с результатом {Result}. Отпустили блокировку {LockId}. Время работы с учетом взятия лока: {Elapsed}")
                                   .WithObjectProperties(new
                                       {
                                           RtqTaskId = taskMeta.Id,
                                           Result = result,
                                           LockId = taskIndexRecord.TaskId,
                                           Elapsed = sw.Elapsed,
                                           IsLongRunningTask = isLongRunningTask,
                                       }));
                    return result;
                }
                finally
                {
                    if (taskGroupRemoteLock != null)
                    {
                        taskGroupRemoteLock.Dispose();
                        logger.Debug("Отпустили групповую блокировку {RtqTaskGroupLock} в процессе завершения задачи {RtqTaskId}", new {RtqTaskGroupLock = taskMeta.TaskGroupLock, RtqTaskId = taskMeta.Id});
                    }
                }
            }
        }

        private LocalTaskProcessingResult ProcessTask([NotNull] MetricsContext metricsContext)
        {
            metricsContext = metricsContext.SubContext(nameof(ProcessTask));
            using (metricsContext.Timer("Total").NewContext())
            {
                byte[] taskData;
                TaskMetaInformation oldMeta;
                try
                {
                    Task task;
                    using (metricsContext.Timer("GetTask").NewContext())
                        task = handleTaskCollection.GetTask(taskIndexRecord.TaskId);
                    oldMeta = task.Meta;
                    taskData = task.Data;
                    if (oldMeta.NeedTtlProlongation())
                        logger.Error("oldMeta.NeedTtlProlongation(oldMeta.GetExpirationTimestamp()) == true for: {RtqTaskMeta}", new {RtqTaskMeta = oldMeta});
                }
                catch (Exception e)
                {
                    taskShardMetricsContext.Meter("ReadTaskException_UnderLock").Mark();
                    logger.Error(e, "Ошибка во время чтения задачи: {RtqTaskIndexRecord}", new {RtqTaskIndexRecord = taskIndexRecord});
                    return LocalTaskProcessingResult.Undefined;
                }

                var localNow = Timestamp.Now;
                var indexRecordConsistentWithActualMeta = handleTasksMetaStorage.FormatIndexRecord(oldMeta);
                if (taskIndexRecord != indexRecordConsistentWithActualMeta)
                {
                    if (taskIndexRecord.MinimalStartTicks > localNow.Ticks - MaxAllowedIndexInconsistencyDuration.Ticks)
                    {
                        taskShardMetricsContext.Meter("InconsistentIndexRecord_UnderLock").Mark();
                        logger.Debug("После перечитывания меты под локом taskIndexRecord != IndexRecord(oldMeta), поэтому ждем; oldMeta: {RtqTaskMeta}; taskIndexRecord: {RtqTaskIndexRecord}; localNow: {LocalNow}",
                                     new {RtqTaskMeta = oldMeta, RtqTaskIndexRecord = taskIndexRecord, LocalNow = localNow});
                    }
                    else
                    {
                        if (oldMeta.State == TaskState.Finished || oldMeta.State == TaskState.Fatal || oldMeta.State == TaskState.Canceled)
                        {
                            taskShardMetricsContext.Meter("TaskAlreadyFinished_UnderLock").Mark();
                            logger.Error("После перечитывания меты под локом taskIndexRecord != IndexRecord(oldMeta) в течение {MaxAllowedIndexInconsistencyDuration} и задача уже находится в терминальном состоянии, " +
                                         "поэтому просто удаляем зависшую запись из индекса; oldMeta: {RtqTaskMeta}; taskIndexRecord: {RtqTaskIndexRecord}; localNow: {LocalNow}",
                                         new {MaxAllowedIndexInconsistencyDuration, RtqTaskMeta = oldMeta, RtqTaskIndexRecord = taskIndexRecord, LocalNow = localNow});
                            using (metricsContext.Timer("RemoveIndexRecord_Terminal").NewContext())
                                taskMinimalStartTicksIndex.RemoveRecord(taskIndexRecord, globalTime.UpdateNowTimestamp().Ticks);
                        }
                        else
                        {
                            logger.Error("После перечитывания меты под локом taskIndexRecord != IndexRecord(oldMeta) в течение {MaxAllowedIndexInconsistencyDuration}, поэтому чиним индекс; " +
                                         "oldMeta: {RtqTaskMeta}; taskIndexRecord: {RtqTaskIndexRecord}; indexRecordConsistentWithActualMeta: {IndexRecordConsistentWithActualMeta}; localNow: {LocalNow}",
                                         new {MaxAllowedIndexInconsistencyDuration, RtqTaskMeta = oldMeta, RtqTaskIndexRecord = taskIndexRecord, IndexRecordConsistentWithActualMeta = indexRecordConsistentWithActualMeta, LocalNow = localNow});
                            taskShardMetricsContext.Meter("FixIndex_UnderLock").Mark();
                            var globalNowTicks = globalTime.UpdateNowTimestamp().Ticks;
                            using (metricsContext.Timer("AddIndexRecord_FixIndex").NewContext())
                                taskMinimalStartTicksIndex.AddRecord(indexRecordConsistentWithActualMeta, globalNowTicks, oldMeta.GetTtl());
                            using (metricsContext.Timer("RemoveIndexRecord_FixIndex").NewContext())
                                taskMinimalStartTicksIndex.RemoveRecord(taskIndexRecord, globalNowTicks);
                        }
                    }
                    return LocalTaskProcessingResult.Undefined;
                }

                var metricsContextForTaskName = MetricsContext.For(oldMeta);
                if (oldMeta.Attempts > 0)
                    metricsContextForTaskName.Meter("RerunTask").Mark();
                var waitedInQueue = Timestamp.Now - new Timestamp(oldMeta.FinishExecutingTicks ?? oldMeta.Ticks);
                if (waitedInQueue < TimeSpan.Zero)
                    waitedInQueue = TimeSpan.Zero;
                metricsContextForTaskName.Timer("TimeWaitingForExecution").Record((long)waitedInQueue.TotalMilliseconds, TimeUnit.Milliseconds);

                logger.Debug("Начинаем обрабатывать задачу oldMeta: {RtqTaskMeta}; Reason: {Reason}; taskIndexRecord: {RtqTaskIndexRecord}",
                             new {RtqTaskMeta = oldMeta, Reason = reason, RtqTaskIndexRecord = taskIndexRecord});
                TaskMetaInformation inProcessMeta;
                using (metricsContext.Timer("TrySwitchToInProcessState").NewContext())
                    inProcessMeta = TrySwitchToInProcessState(oldMeta);
                if (inProcessMeta == null)
                {
                    taskShardMetricsContext.Meter("StartProcessingFailed_UnderLock").Mark();
                    logger.Error("Не удалось начать обработку задачи. OldMeta: {RtqTaskMeta}", new {RtqTaskMeta = oldMeta});
                    return LocalTaskProcessingResult.Undefined;
                }

                var processTaskResult = DoProcessTask(inProcessMeta, taskData, metricsContext);
                taskShardMetricsContext.Meter("Processed").Mark();

                var newMeta = processTaskResult.NewMeta;
                if (newMeta != null && newMeta.NeedTtlProlongation())
                {
                    logger.Debug("Продлеваем время жизни задачи после обработки. NewMeta: {RtqTaskMeta}", new {RtqTaskMeta = newMeta});
                    try
                    {
                        newMeta.SetOrUpdateTtl(taskTtl);
                        using (metricsContext.Timer("ProlongTaskTtl").NewContext())
                            handleTaskCollection.ProlongTaskTtl(newMeta, taskData);
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Ошибка во время продления времени жизни задачи. NewMeta: {RtqTaskMeta}", new {RtqTaskMeta = newMeta});
                    }
                }

                return processTaskResult.ProcessingResult;
            }
        }

        [NotNull]
        private ProcessTaskResult DoProcessTask([NotNull] TaskMetaInformation inProcessMeta, [NotNull] byte[] taskData, [NotNull] MetricsContext metricsContext)
        {
            metricsContext = metricsContext.SubContext(nameof(DoProcessTask));
            using (metricsContext.Timer("Total").NewContext())
            {
                IRtqTaskHandler taskHandler;
                try
                {
                    using (metricsContext.Timer("CreateHandlerFor").NewContext())
                        taskHandler = taskHandlerRegistry.CreateHandlerFor(inProcessMeta.Name);
                }
                catch (Exception e)
                {
                    var newExceptionInfoIds = TryLogError(e, inProcessMeta);
                    using (metricsContext.Timer("TrySwitchToTerminalState").NewContext())
                        return new ProcessTaskResult(LocalTaskProcessingResult.Error, TrySwitchToTerminalState(inProcessMeta, TaskState.Fatal, newExceptionInfoIds));
                }

                var task = new Task(inProcessMeta, taskData);
                using (TaskExecutionContext.ForTask(task))
                {
                    var sw = Stopwatch.StartNew();
                    try
                    {
                        HandleResult handleResult;
                        using (metricsContext.Timer("HandleTask").NewContext())
                            handleResult = taskHandler.HandleTask(taskProducer, serializer, task);
                        rtqProfiler.ProcessTaskExecutionFinished(inProcessMeta, handleResult, sw.Elapsed);
                        MetricsContext.For(inProcessMeta).Meter("TasksExecuted").Mark();
                        using (metricsContext.Timer("UpdateTaskMetaByHandleResult").NewContext())
                            return UpdateTaskMetaByHandleResult(inProcessMeta, handleResult);
                    }
                    catch (Exception e)
                    {
                        rtqProfiler.ProcessTaskExecutionFailed(inProcessMeta, sw.Elapsed);
                        MetricsContext.For(inProcessMeta).Meter("TasksExecutionFailed").Mark();
                        var taskExceptionInfoId = TryLogError(e, inProcessMeta);
                        using (metricsContext.Timer("TrySwitchToTerminalState").NewContext())
                            return new ProcessTaskResult(LocalTaskProcessingResult.Error, TrySwitchToTerminalState(inProcessMeta, TaskState.Fatal, taskExceptionInfoId));
                    }
                }
            }
        }

        [NotNull]
        private ProcessTaskResult UpdateTaskMetaByHandleResult([NotNull] TaskMetaInformation inProcessMeta, [NotNull] HandleResult handleResult)
        {
            List<TimeGuid> newExceptionInfoIds;
            switch (handleResult.FinishAction)
            {
            case FinishAction.Finish:
                return new ProcessTaskResult(LocalTaskProcessingResult.Success, TrySwitchToTerminalState(inProcessMeta, TaskState.Finished, newExceptionInfoIds : null));
            case FinishAction.Fatal:
                newExceptionInfoIds = TryLogError(handleResult.Error, inProcessMeta);
                return new ProcessTaskResult(LocalTaskProcessingResult.Error, TrySwitchToTerminalState(inProcessMeta, TaskState.Fatal, newExceptionInfoIds));
            case FinishAction.RerunAfterError:
                newExceptionInfoIds = TryLogError(handleResult.Error, inProcessMeta);
                return new ProcessTaskResult(LocalTaskProcessingResult.Rerun, TrySwitchToWaitingForRerunState(inProcessMeta, TaskState.WaitingForRerunAfterError, handleResult.RerunDelay, newExceptionInfoIds));
            case FinishAction.Rerun:
                return new ProcessTaskResult(LocalTaskProcessingResult.Rerun, TrySwitchToWaitingForRerunState(inProcessMeta, TaskState.WaitingForRerun, handleResult.RerunDelay, newExceptionInfoIds : null));
            default:
                throw new InvalidProgramStateException($"Invalid FinishAction: {handleResult.FinishAction}");
            }
        }

        [CanBeNull]
        private List<TimeGuid> TryLogError([NotNull] Exception e, [NotNull] TaskMetaInformation inProcessMeta)
        {
            logger.Debug(e, "Ошибка во время обработки задачи: {RtqTaskMeta}", new {RtqTaskMeta = inProcessMeta});
            try
            {
                if (taskExceptionInfoStorage.TryAddNewExceptionInfo(inProcessMeta, e, out var newExceptionInfoIds))
                    return newExceptionInfoIds;
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Не смогли записать ошибку для задачи: {RtqTaskMeta}. Ошибка: {Error}", new {RtqTaskMeta = inProcessMeta, Error = e});
            }
            return null;
        }

        [CanBeNull]
        private TaskMetaInformation TrySwitchToInProcessState([NotNull] TaskMetaInformation oldMeta)
        {
            var nowTicks = Timestamp.Now.Ticks;
            var newMinimalStartTicks = nowTicks + CassandraNameHelper.TaskMinimalStartTicksIndexTicksPartition;
            return TryUpdateTaskState(oldMeta, taskIndexRecord, newMinimalStartTicks, nowTicks, null, oldMeta.Attempts + 1, TaskState.InProcess, newExceptionInfoIds : null);
        }

        [CanBeNull]
        private TaskMetaInformation TrySwitchToTerminalState([NotNull] TaskMetaInformation inProcessMeta, TaskState terminalState, [CanBeNull] List<TimeGuid> newExceptionInfoIds)
        {
            var nowTicks = Timestamp.Now.Ticks;
            var inProcessTaskIndexRecord = handleTasksMetaStorage.FormatIndexRecord(inProcessMeta);
            return TryUpdateTaskState(inProcessMeta, inProcessTaskIndexRecord, nowTicks, inProcessMeta.StartExecutingTicks, nowTicks, inProcessMeta.Attempts, terminalState, newExceptionInfoIds);
        }

        [CanBeNull]
        private TaskMetaInformation TrySwitchToWaitingForRerunState([NotNull] TaskMetaInformation inProcessMeta, TaskState waitingForRerunState, TimeSpan rerunDelay, [CanBeNull] List<TimeGuid> newExceptionInfoIds)
        {
            var nowTicks = Timestamp.Now.Ticks;
            var inProcessTaskIndexRecord = handleTasksMetaStorage.FormatIndexRecord(inProcessMeta);
            return TryUpdateTaskState(inProcessMeta, inProcessTaskIndexRecord, nowTicks + rerunDelay.Ticks, inProcessMeta.StartExecutingTicks, nowTicks, inProcessMeta.Attempts, waitingForRerunState, newExceptionInfoIds);
        }

        [CanBeNull]
        private TaskMetaInformation TryUpdateTaskState([NotNull] TaskMetaInformation oldMeta, [NotNull] TaskIndexRecord oldTaskIndexRecord, long newMinimalStartTicks, long? startExecutingTicks, long? finishExecutingTicks, int attempts, TaskState newState, [CanBeNull] List<TimeGuid> newExceptionInfoIds)
        {
            var newMeta = GrobufSerializers.AllFieldsSerializer.Copy(oldMeta);
            if (newState == oldMeta.State)
                newMinimalStartTicks = Math.Max(newMinimalStartTicks, oldMeta.MinimalStartTicks + PreciseTimestampGenerator.TicksPerMicrosecond);
            newMeta.MinimalStartTicks = newMinimalStartTicks;
            newMeta.StartExecutingTicks = startExecutingTicks;
            newMeta.FinishExecutingTicks = finishExecutingTicks;
            newMeta.Attempts = attempts;
            newMeta.State = newState;
            if (newExceptionInfoIds != null && newExceptionInfoIds.Any())
                newMeta.TaskExceptionInfoIds = newExceptionInfoIds;
            try
            {
                handleTasksMetaStorage.AddMeta(newMeta, oldTaskIndexRecord);
                logger.Debug("Changed task state. NewMeta: {RtqTaskMeta}", new {RtqTaskMeta = newMeta});
                return newMeta;
            }
            catch (Exception e)
            {
                logger.Error(e, "Can't update task state. OldMeta: {RtqTaskMeta}", new {RtqTaskMeta = oldMeta});
                return null;
            }
        }

        private readonly TaskIndexRecord taskIndexRecord;
        private readonly TaskQueueReason reason;
        private readonly TaskMetaInformation taskMeta;
        private readonly IRtqTaskHandlerRegistry taskHandlerRegistry;
        private readonly ISerializer serializer;
        private readonly IRtqTaskProducer taskProducer;
        private readonly IHandleTaskCollection handleTaskCollection;
        private readonly IRemoteLockCreator remoteLockCreator;
        private readonly ITaskExceptionInfoStorage taskExceptionInfoStorage;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ITaskMinimalStartTicksIndex taskMinimalStartTicksIndex;
        private readonly IRtqProfiler rtqProfiler;
        private readonly IGlobalTime globalTime;
        private readonly TimeSpan taskTtl;
        private readonly ILog logger;
        private static readonly TimeSpan longRunningTaskDurationThreshold = TimeSpan.FromMinutes(1);
        public static readonly TimeSpan MaxAllowedIndexInconsistencyDuration = TimeSpan.FromMinutes(1);
        private readonly MetricsContext taskShardMetricsContext;

        private class ProcessTaskResult
        {
            public ProcessTaskResult(LocalTaskProcessingResult processingResult, [CanBeNull] TaskMetaInformation newMeta)
            {
                ProcessingResult = processingResult;
                NewMeta = newMeta;
            }

            public LocalTaskProcessingResult ProcessingResult { get; }

            [CanBeNull]
            public TaskMetaInformation NewMeta { get; }
        }
    }
}