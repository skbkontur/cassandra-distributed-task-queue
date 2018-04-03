using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using log4net;

using Metrics;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Configuration;
using RemoteQueue.Handling.ExecutionContext;
using RemoteQueue.LocalTasks.TaskQueue;
using RemoteQueue.Profiling;

using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;
using SKBKontur.Catalogue.GrobufExtensions;
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.TimeBasedUuid;

using MetricsContext = RemoteQueue.Profiling.MetricsContext;

namespace RemoteQueue.Handling
{
    internal class HandlerTask
    {
        public HandlerTask(
            [NotNull] TaskIndexRecord taskIndexRecord,
            TaskQueueReason reason,
            [CanBeNull] TaskMetaInformation taskMeta,
            ITaskHandlerRegistry taskHandlerRegistry,
            IRemoteTaskQueueInternals remoteTaskQueueInternals)
        {
            this.taskIndexRecord = taskIndexRecord;
            this.reason = reason;
            this.taskMeta = taskMeta;
            this.taskHandlerRegistry = taskHandlerRegistry;
            serializer = remoteTaskQueueInternals.Serializer;
            remoteTaskQueue = remoteTaskQueueInternals.RemoteTaskQueue;
            handleTaskCollection = remoteTaskQueueInternals.HandleTaskCollection;
            remoteLockCreator = remoteTaskQueueInternals.RemoteLockCreator;
            taskExceptionInfoStorage = remoteTaskQueueInternals.TaskExceptionInfoStorage;
            handleTasksMetaStorage = remoteTaskQueueInternals.HandleTasksMetaStorage;
            taskMinimalStartTicksIndex = remoteTaskQueueInternals.TaskMinimalStartTicksIndex;
            remoteTaskQueueProfiler = remoteTaskQueueInternals.RemoteTaskQueueProfiler;
            globalTime = remoteTaskQueueInternals.GlobalTime;
            taskTtl = remoteTaskQueueInternals.TaskTtl;
            taskShardMetricsContext = MetricsContext.For($"Shards.{taskIndexRecord.TaskIndexShardKey.TaskTopic}.{taskIndexRecord.TaskIndexShardKey.TaskState}.Tasks");
        }

        public LocalTaskProcessingResult RunTask()
        {
            taskShardMetricsContext.Meter("Started").Mark();
            if(taskMeta == null)
            {
                taskShardMetricsContext.Meter("NoMeta").Mark();
                logger.Error($"Удаляем запись индекса, для которой мета так и не записалась: {taskIndexRecord}");
                taskMinimalStartTicksIndex.RemoveRecord(taskIndexRecord, globalTime.UpdateNowTicks());
                return LocalTaskProcessingResult.Undefined;
            }
            var localNow = Timestamp.Now;
            if(taskIndexRecord != handleTasksMetaStorage.FormatIndexRecord(taskMeta) && taskIndexRecord.MinimalStartTicks > localNow.Ticks - MaxAllowedIndexInconsistencyDuration.Ticks)
            {
                taskShardMetricsContext.Meter("InconsistentIndexRecord").Mark();
                logger.Debug($"taskIndexRecord != IndexRecord(taskMeta), поэтому ждем; taskMeta: {taskMeta}; taskIndexRecord: {taskIndexRecord}; localNow: {localNow}");
                return LocalTaskProcessingResult.Undefined;
            }
            var metricsContext = MetricsContext.For(taskMeta).SubContext(nameof(HandlerTask));
            return TryProcessTaskExclusively(metricsContext);
        }

        private LocalTaskProcessingResult TryProcessTaskExclusively([NotNull] MetricsContext metricsContext)
        {
            metricsContext = metricsContext.SubContext(nameof(TryProcessTaskExclusively));
            using(metricsContext.Timer("Total").NewContext())
            {
                IRemoteLock taskGroupRemoteLock = null;
                if(!string.IsNullOrEmpty(taskMeta.TaskGroupLock))
                {
                    using(metricsContext.Timer("TryGetLock_TaskGroupLock").NewContext())
                    {
                        if(!remoteLockCreator.TryGetLock(taskMeta.TaskGroupLock, out taskGroupRemoteLock))
                        {
                            taskShardMetricsContext.Meter("DidNotGetTaskGroupLock").Mark();
                            logger.Debug($"Не смогли взять групповую блокировку {taskMeta.TaskGroupLock} на задачу: {taskIndexRecord.TaskId}");
                            return LocalTaskProcessingResult.Undefined;
                        }
                    }
                    taskShardMetricsContext.Meter("GotTaskGroupLock").Mark();
                }
                try
                {
                    var sw = Stopwatch.StartNew();
                    IRemoteLock remoteLock;
                    using(metricsContext.Timer("TryGetLock_TaskId").NewContext())
                    {
                        if(!remoteLockCreator.TryGetLock(taskIndexRecord.TaskId, out remoteLock))
                        {
                            taskShardMetricsContext.Meter("DidNotGetTaskLock").Mark();
                            logger.Debug($"Не смогли взять блокировку на задачу, пропускаем её: {taskIndexRecord}");
                            return LocalTaskProcessingResult.Undefined;
                        }
                    }
                    taskShardMetricsContext.Meter("GotTaskLock").Mark();
                    LocalTaskProcessingResult result;
                    using(remoteLock)
                        result = ProcessTask(metricsContext);
                    sw.Stop();
                    var longRunningFlag = sw.Elapsed > longRunningTaskDurationThreshold ? " [LONG RUNNING]" : string.Empty;
                    var message = $"Завершили выполнение задачи {taskMeta.Id} с результатом {result}. Отпустили блокировку {taskIndexRecord.TaskId}. Время работы с учетом взятия лока: {sw.Elapsed}{longRunningFlag}";
                    if(sw.Elapsed < longRunningTaskDurationThreshold)
                        logger.Debug(message);
                    else
                        logger.Warn(message);
                    return result;
                }
                finally
                {
                    if(taskGroupRemoteLock != null)
                    {
                        taskGroupRemoteLock.Dispose();
                        logger.Debug($"Отпустили групповую блокировку {taskMeta.TaskGroupLock} в процессе завершения задачи {taskMeta.Id}");
                    }
                }
            }
        }

        private LocalTaskProcessingResult ProcessTask([NotNull] MetricsContext metricsContext)
        {
            metricsContext = metricsContext.SubContext(nameof(ProcessTask));
            using(metricsContext.Timer("Total").NewContext())
            {
                byte[] taskData;
                TaskMetaInformation oldMeta;
                try
                {
                    Task task;
                    using(metricsContext.Timer("GetTask").NewContext())
                        task = handleTaskCollection.GetTask(taskIndexRecord.TaskId);
                    oldMeta = task.Meta;
                    taskData = task.Data;
                    if(oldMeta.NeedTtlProlongation())
                        logger.Error($"oldMeta.NeedTtlProlongation(oldMeta.GetExpirationTimestamp()) == true for: {oldMeta}");
                }
                catch(Exception e)
                {
                    taskShardMetricsContext.Meter("ReadTaskException_UnderLock").Mark();
                    logger.Error($"Ошибка во время чтения задачи: {taskIndexRecord}", e);
                    return LocalTaskProcessingResult.Undefined;
                }

                var localNow = Timestamp.Now;
                var indexRecordConsistentWithActualMeta = handleTasksMetaStorage.FormatIndexRecord(oldMeta);
                if(taskIndexRecord != indexRecordConsistentWithActualMeta)
                {
                    if(taskIndexRecord.MinimalStartTicks > localNow.Ticks - MaxAllowedIndexInconsistencyDuration.Ticks)
                    {
                        taskShardMetricsContext.Meter("InconsistentIndexRecord_UnderLock").Mark();
                        logger.Debug($"После перечитывания меты под локом taskIndexRecord != IndexRecord(oldMeta), поэтому ждем; oldMeta: {oldMeta}; taskIndexRecord: {taskIndexRecord}; localNow: {localNow}");
                    }
                    else
                    {
                        if(oldMeta.State == TaskState.Finished || oldMeta.State == TaskState.Fatal || oldMeta.State == TaskState.Canceled)
                        {
                            taskShardMetricsContext.Meter("TaskAlreadyFinished_UnderLock").Mark();
                            logger.Error($"После перечитывания меты под локом taskIndexRecord != IndexRecord(oldMeta) в течение {MaxAllowedIndexInconsistencyDuration} и задача уже находится в терминальном состоянии, " +
                                         $"поэтому просто удаляем зависшую запись из индекса; oldMeta: {oldMeta}; taskIndexRecord: {taskIndexRecord}; localNow: {localNow}");
                            using(metricsContext.Timer("RemoveIndexRecord_Terminal").NewContext())
                                taskMinimalStartTicksIndex.RemoveRecord(taskIndexRecord, globalTime.UpdateNowTicks());
                        }
                        else
                        {
                            logger.Error($"После перечитывания меты под локом taskIndexRecord != IndexRecord(oldMeta) в течение {MaxAllowedIndexInconsistencyDuration}, поэтому чиним индекс; " +
                                         $"oldMeta: {oldMeta}; taskIndexRecord: {taskIndexRecord}; indexRecordConsistentWithActualMeta: {indexRecordConsistentWithActualMeta}; localNow: {localNow}");
                            taskShardMetricsContext.Meter("FixIndex_UnderLock").Mark();
                            var globalNowTicks = globalTime.UpdateNowTicks();
                            using(metricsContext.Timer("AddIndexRecord_FixIndex").NewContext())
                                taskMinimalStartTicksIndex.AddRecord(indexRecordConsistentWithActualMeta, globalNowTicks, oldMeta.GetTtl());
                            using(metricsContext.Timer("RemoveIndexRecord_FixIndex").NewContext())
                                taskMinimalStartTicksIndex.RemoveRecord(taskIndexRecord, globalNowTicks);
                        }
                    }
                    return LocalTaskProcessingResult.Undefined;
                }

                var metricsContextForTaskName = MetricsContext.For(oldMeta);
                if(oldMeta.Attempts > 0)
                    metricsContextForTaskName.Meter("RerunTask").Mark();
                var waitedInQueue = Timestamp.Now - new Timestamp(oldMeta.FinishExecutingTicks ?? oldMeta.Ticks);
                if(waitedInQueue < TimeSpan.Zero)
                    waitedInQueue = TimeSpan.Zero;
                metricsContextForTaskName.Timer("TimeWaitingForExecution").Record((long)waitedInQueue.TotalMilliseconds, TimeUnit.Milliseconds);

                logger.Debug($"Начинаем обрабатывать задачу {oldMeta}; Reason: {reason}; taskIndexRecord: {taskIndexRecord}");
                TaskMetaInformation inProcessMeta;
                using(metricsContext.Timer("TrySwitchToInProcessState").NewContext())
                    inProcessMeta = TrySwitchToInProcessState(oldMeta);
                if(inProcessMeta == null)
                {
                    taskShardMetricsContext.Meter("StartProcessingFailed_UnderLock").Mark();
                    logger.Error($"Не удалось начать обработку задачи: {oldMeta}");
                    return LocalTaskProcessingResult.Undefined;
                }

                var processTaskResult = DoProcessTask(inProcessMeta, taskData, metricsContext);
                taskShardMetricsContext.Meter("Processed").Mark();

                var newMeta = processTaskResult.NewMeta;
                if(newMeta != null && newMeta.NeedTtlProlongation())
                {
                    logger.Debug($"Продлеваем время жизни задачи после обработки: {newMeta}");
                    try
                    {
                        newMeta.SetOrUpdateTtl(taskTtl);
                        using(metricsContext.Timer("ProlongTaskTtl").NewContext())
                            handleTaskCollection.ProlongTaskTtl(newMeta, taskData);
                    }
                    catch(Exception e)
                    {
                        logger.Error($"Ошибка во время продления времени жизни задачи: {newMeta}", e);
                    }
                }

                return processTaskResult.ProcessingResult;
            }
        }

        [NotNull]
        private ProcessTaskResult DoProcessTask([NotNull] TaskMetaInformation inProcessMeta, [NotNull] byte[] taskData, [NotNull] MetricsContext metricsContext)
        {
            metricsContext = metricsContext.SubContext(nameof(DoProcessTask));
            using(metricsContext.Timer("Total").NewContext())
            {
                ITaskHandler taskHandler;
                try
                {
                    using(metricsContext.Timer("CreateHandlerFor").NewContext())
                        taskHandler = taskHandlerRegistry.CreateHandlerFor(inProcessMeta.Name);
                }
                catch(Exception e)
                {
                    var newExceptionInfoIds = TryLogError(e, inProcessMeta);
                    using(metricsContext.Timer("TrySwitchToTerminalState").NewContext())
                        return new ProcessTaskResult(LocalTaskProcessingResult.Error, TrySwitchToTerminalState(inProcessMeta, TaskState.Fatal, newExceptionInfoIds));
                }

                var task = new Task(inProcessMeta, taskData);
                using(TaskExecutionContext.ForTask(task))
                {
                    var sw = Stopwatch.StartNew();
                    try
                    {
                        HandleResult handleResult;
                        using(metricsContext.Timer("HandleTask").NewContext())
                            handleResult = taskHandler.HandleTask(remoteTaskQueue, serializer, remoteLockCreator, task);
                        remoteTaskQueueProfiler.ProcessTaskExecutionFinished(inProcessMeta, handleResult, sw.Elapsed);
                        MetricsContext.For(inProcessMeta).Meter("TasksExecuted").Mark();
                        using(metricsContext.Timer("UpdateTaskMetaByHandleResult").NewContext())
                            return UpdateTaskMetaByHandleResult(inProcessMeta, handleResult);
                    }
                    catch(Exception e)
                    {
                        remoteTaskQueueProfiler.ProcessTaskExecutionFailed(inProcessMeta, sw.Elapsed);
                        MetricsContext.For(inProcessMeta).Meter("TasksExecutionFailed").Mark();
                        var taskExceptionInfoId = TryLogError(e, inProcessMeta);
                        using(metricsContext.Timer("TrySwitchToTerminalState").NewContext())
                            return new ProcessTaskResult(LocalTaskProcessingResult.Error, TrySwitchToTerminalState(inProcessMeta, TaskState.Fatal, taskExceptionInfoId));
                    }
                }
            }
        }

        [NotNull]
        private ProcessTaskResult UpdateTaskMetaByHandleResult([NotNull] TaskMetaInformation inProcessMeta, [NotNull] HandleResult handleResult)
        {
            List<TimeGuid> newExceptionInfoIds;
            switch(handleResult.FinishAction)
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
            logger.Error($"Ошибка во время обработки задачи: {inProcessMeta}", e);
            try
            {
                if(taskExceptionInfoStorage.TryAddNewExceptionInfo(inProcessMeta, e, out var newExceptionInfoIds))
                    return newExceptionInfoIds;
            }
            catch
            {
                logger.Error($"Не смогли записать ошибку для задачи: {inProcessMeta}", e);
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
            if(newState == oldMeta.State)
                newMinimalStartTicks = Math.Max(newMinimalStartTicks, oldMeta.MinimalStartTicks + PreciseTimestampGenerator.TicksPerMicrosecond);
            newMeta.MinimalStartTicks = newMinimalStartTicks;
            newMeta.StartExecutingTicks = startExecutingTicks;
            newMeta.FinishExecutingTicks = finishExecutingTicks;
            newMeta.Attempts = attempts;
            newMeta.State = newState;
            if(newExceptionInfoIds != null && newExceptionInfoIds.Any())
                newMeta.TaskExceptionInfoIds = newExceptionInfoIds;
            try
            {
                handleTasksMetaStorage.AddMeta(newMeta, oldTaskIndexRecord);
                logger.Debug($"Changed task state. Task = {newMeta}");
                return newMeta;
            }
            catch(Exception e)
            {
                logger.Error($"Can't update task state for: {oldMeta}", e);
                return null;
            }
        }

        private readonly TaskIndexRecord taskIndexRecord;
        private readonly TaskQueueReason reason;
        private readonly TaskMetaInformation taskMeta;
        private readonly ITaskHandlerRegistry taskHandlerRegistry;
        private readonly ISerializer serializer;
        private readonly IRemoteTaskQueue remoteTaskQueue;
        private readonly IHandleTaskCollection handleTaskCollection;
        private readonly IRemoteLockCreator remoteLockCreator;
        private readonly ITaskExceptionInfoStorage taskExceptionInfoStorage;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ITaskMinimalStartTicksIndex taskMinimalStartTicksIndex;
        private readonly IRemoteTaskQueueProfiler remoteTaskQueueProfiler;
        private readonly IGlobalTime globalTime;
        private readonly TimeSpan taskTtl;
        private static readonly ILog logger = LogManager.GetLogger(typeof(HandlerTask));
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