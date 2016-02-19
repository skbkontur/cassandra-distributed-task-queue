using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using GroBuf;
using GroBuf.DataMembersExtracters;

using JetBrains.Annotations;

using log4net;

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
using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.Objects.TimeBasedUuid;

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
            taskShardMetrics = TaskShardMetrics.ForShard(taskIndexRecord.TaskIndexShardKey);
        }

        public LocalTaskProcessingResult RunTask()
        {
            taskShardMetrics.Started.Mark();
            if(taskMeta == null)
            {
                taskShardMetrics.NoMeta.Mark();
                logger.ErrorFormat("Удаляем запись индекса, для которой мета так и не записалась: {0}", taskIndexRecord);
                taskMinimalStartTicksIndex.RemoveRecord(taskIndexRecord, globalTime.UpdateNowTicks());
                return LocalTaskProcessingResult.Undefined;
            }
            var localNow = Timestamp.Now;
            if(taskIndexRecord != handleTasksMetaStorage.FormatIndexRecord(taskMeta) && taskIndexRecord.MinimalStartTicks > localNow.Ticks - MaxAllowedIndexInconsistencyDuration.Ticks)
            {
                taskShardMetrics.InconsistentIndexRecord.Mark();
                logger.InfoFormat("taskIndexRecord != IndexRecord(taskMeta), поэтому ждем; taskMeta: {0}; taskIndexRecord: {1}; localNow: {2}", taskMeta, taskIndexRecord, localNow);
                return LocalTaskProcessingResult.Undefined;
            }
            return TryProcessTaskExclusively();
        }

        private LocalTaskProcessingResult TryProcessTaskExclusively()
        {
            IRemoteLock taskGroupRemoteLock = null;
            if(!string.IsNullOrEmpty(taskMeta.TaskGroupLock))
            {
                if(!remoteLockCreator.TryGetLock(taskMeta.TaskGroupLock, out taskGroupRemoteLock))
                {
                    taskShardMetrics.DidNotGetTaskGroupLock.Mark();
                    logger.InfoFormat("Не смогли взять групповую блокировку {0} на задачу: {1}", taskMeta.TaskGroupLock, taskIndexRecord.TaskId);
                    return LocalTaskProcessingResult.Undefined;
                }
                taskShardMetrics.GotTaskGroupLock.Mark();
            }
            try
            {
                IRemoteLock remoteLock;
                if(!remoteLockCreator.TryGetLock(taskIndexRecord.TaskId, out remoteLock))
                {
                    taskShardMetrics.DidNotGetTaskLock.Mark();
                    logger.InfoFormat("Не смогли взять блокировку на задачу, пропускаем её: {0}", taskIndexRecord);
                    return LocalTaskProcessingResult.Undefined;
                }
                taskShardMetrics.GotTaskLock.Mark();
                LocalTaskProcessingResult result;
                using(remoteLock)
                    result = ProcessTask();
                logger.InfoFormat("Завершили выполнение задачи {0} с результатом {1}. Отпустили блокировку {2}", taskMeta.Id, result, taskIndexRecord.TaskId);
                return result;
            }
            finally
            {
                if(taskGroupRemoteLock != null)
                {
                    taskGroupRemoteLock.Dispose();
                    logger.InfoFormat("Отпустили групповую блокировку {0} в процессе завершения задачи {1}", taskMeta.TaskGroupLock, taskMeta.Id);
                }
            }
        }

        private LocalTaskProcessingResult ProcessTask()
        {
            byte[] taskData;
            TaskMetaInformation oldMeta;
            try
            {
                var task = handleTaskCollection.GetTask(taskIndexRecord.TaskId);
                oldMeta = task.Meta;
                taskData = task.Data;
            }
            catch(Exception e)
            {
                taskShardMetrics.ReadTaskException_UnderLock.Mark();
                logger.Error(string.Format("Ошибка во время чтения задачи: {0}", taskIndexRecord), e);
                return LocalTaskProcessingResult.Undefined;
            }

            var localNow = Timestamp.Now;
            var indexRecordConsistentWithActualMeta = handleTasksMetaStorage.FormatIndexRecord(oldMeta);
            if(taskIndexRecord != indexRecordConsistentWithActualMeta)
            {
                if(taskIndexRecord.MinimalStartTicks > localNow.Ticks - MaxAllowedIndexInconsistencyDuration.Ticks)
                {
                    taskShardMetrics.InconsistentIndexRecord_UnderLock.Mark();
                    logger.InfoFormat("После перечитывания меты под локом taskIndexRecord != IndexRecord(oldMeta), поэтому ждем; oldMeta: {0}; taskIndexRecord: {1}; localNow: {2}", oldMeta, taskIndexRecord, localNow);
                }
                else
                {
                    if(oldMeta.State == TaskState.Finished || oldMeta.State == TaskState.Fatal || oldMeta.State == TaskState.Canceled)
                    {
                        taskShardMetrics.TaskAlreadyFinished_UnderLock.Mark();
                        logger.ErrorFormat("После перечитывания меты под локом taskIndexRecord != IndexRecord(oldMeta) в течение {0} и задача уже находится в терминальном состоянии, поэтому просто удаляем зависшую запись из индекса; oldMeta: {1}; taskIndexRecord: {2}; localNow: {3}",
                                           MaxAllowedIndexInconsistencyDuration, oldMeta, taskIndexRecord, localNow);
                        taskMinimalStartTicksIndex.RemoveRecord(taskIndexRecord, globalTime.UpdateNowTicks());
                    }
                    else
                    {
                        logger.ErrorFormat("После перечитывания меты под локом taskIndexRecord != IndexRecord(oldMeta) в течение {0}, поэтому чиним индекс; oldMeta: {1}; taskIndexRecord: {2}; indexRecordConsistentWithActualMeta: {3}; localNow: {4}",
                                           MaxAllowedIndexInconsistencyDuration, oldMeta, taskIndexRecord, indexRecordConsistentWithActualMeta, localNow);
                        taskShardMetrics.FixIndex_UnderLock.Mark();
                        var globalNowTicks = globalTime.UpdateNowTicks();
                        taskMinimalStartTicksIndex.AddRecord(indexRecordConsistentWithActualMeta, globalNowTicks);
                        taskMinimalStartTicksIndex.RemoveRecord(taskIndexRecord, globalNowTicks);
                    }
                }
                return LocalTaskProcessingResult.Undefined;
            }

            logger.InfoFormat("Начинаем обрабатывать задачу {0}; Reason: {1}; taskIndexRecord: {2}", oldMeta, reason, taskIndexRecord);

            var inProcessMeta = TrySwitchToInProcessState(oldMeta);
            if(inProcessMeta == null)
            {
                taskShardMetrics.StartProcessingFailed_UnderLock.Mark();
                logger.ErrorFormat("Не удалось начать обработку задачи: {0}", oldMeta);
                return LocalTaskProcessingResult.Undefined;
            }

            taskShardMetrics.Processed.Mark();
            return ProcessTask(inProcessMeta, taskData);
        }

        private LocalTaskProcessingResult ProcessTask([NotNull] TaskMetaInformation inProcessMeta, [NotNull] byte[] taskData)
        {
            ITaskHandler taskHandler;
            try
            {
                taskHandler = taskHandlerRegistry.CreateHandlerFor(inProcessMeta.Name);
            }
            catch(Exception e)
            {
                var newExceptionInfoIds = TryLogError(e, inProcessMeta);
                TrySwitchToTerminalState(inProcessMeta, TaskState.Fatal, newExceptionInfoIds);
                return LocalTaskProcessingResult.Error;
            }

            LocalTaskProcessingResult localTaskProcessingResult;
            var task = new Task(inProcessMeta, taskData);
            using(TaskExecutionContext.ForTask(task))
            {
                try
                {
                    var sw = Stopwatch.StartNew();
                    remoteTaskQueueProfiler.ProcessTaskDequeueing(inProcessMeta);
                    var handleResult = taskHandler.HandleTask(remoteTaskQueue, serializer, remoteLockCreator, task);
                    remoteTaskQueueProfiler.ProcessTaskExecutionFinished(inProcessMeta, handleResult, sw.Elapsed);
                    localTaskProcessingResult = UpdateTaskMetaByHandleResult(inProcessMeta, handleResult);
                }
                catch(Exception e)
                {
                    localTaskProcessingResult = LocalTaskProcessingResult.Error;
                    remoteTaskQueueProfiler.ProcessTaskExecutionFailed(inProcessMeta, e);
                    var taskExceptionInfoId = TryLogError(e, inProcessMeta);
                    TrySwitchToTerminalState(inProcessMeta, TaskState.Fatal, taskExceptionInfoId);
                }
            }
            return localTaskProcessingResult;
        }

        private LocalTaskProcessingResult UpdateTaskMetaByHandleResult([NotNull] TaskMetaInformation inProcessMeta, [NotNull] HandleResult handleResult)
        {
            List<TimeGuid> newExceptionInfoIds;
            switch(handleResult.FinishAction)
            {
            case FinishAction.Finish:
                TrySwitchToTerminalState(inProcessMeta, TaskState.Finished, newExceptionInfoIds : null);
                return LocalTaskProcessingResult.Success;
            case FinishAction.Fatal:
                newExceptionInfoIds = TryLogError(handleResult.Error, inProcessMeta);
                TrySwitchToTerminalState(inProcessMeta, TaskState.Fatal, newExceptionInfoIds);
                return LocalTaskProcessingResult.Error;
            case FinishAction.RerunAfterError:
                newExceptionInfoIds = TryLogError(handleResult.Error, inProcessMeta);
                TrySwitchToWaitingForRerunState(inProcessMeta, TaskState.WaitingForRerunAfterError, handleResult.RerunDelay, newExceptionInfoIds);
                return LocalTaskProcessingResult.Rerun;
            case FinishAction.Rerun:
                TrySwitchToWaitingForRerunState(inProcessMeta, TaskState.WaitingForRerun, handleResult.RerunDelay, newExceptionInfoIds : null);
                return LocalTaskProcessingResult.Rerun;
            default:
                throw new InvalidProgramStateException(string.Format("Invalid FinishAction: {0}", handleResult.FinishAction));
            }
        }

        [CanBeNull]
        private List<TimeGuid> TryLogError([NotNull] Exception e, [NotNull] TaskMetaInformation inProcessMeta)
        {
            logger.Error(string.Format("Ошибка во время обработки задачи: {0}", inProcessMeta), e);
            try
            {
                List<TimeGuid> newExceptionInfoIds;
                if(taskExceptionInfoStorage.TryAddNewExceptionInfo(inProcessMeta, e, out newExceptionInfoIds))
                    return newExceptionInfoIds;
            }
            catch
            {
                logger.Error(string.Format("Не смогли записать ошибку для задачи: {0}", inProcessMeta), e);
            }
            return null;
        }

        [CanBeNull]
        private TaskMetaInformation TrySwitchToInProcessState([NotNull] TaskMetaInformation oldMeta)
        {
            var nowTicks = Timestamp.Now.Ticks;
            var newMinimalStartTicks = nowTicks + CassandraNameHelper.TaskMinimalStartTicksIndexTicksPartition;
            var inProcessMeta = TryUpdateTaskState(oldMeta, taskIndexRecord, newMinimalStartTicks, nowTicks, null, oldMeta.Attempts + 1, TaskState.InProcess, newExceptionInfoIds : null);
            return inProcessMeta;
        }

        private void TrySwitchToTerminalState([NotNull] TaskMetaInformation inProcessMeta, TaskState terminalState, [CanBeNull] List<TimeGuid> newExceptionInfoIds)
        {
            var nowTicks = Timestamp.Now.Ticks;
            var inProcessTaskIndexRecord = handleTasksMetaStorage.FormatIndexRecord(inProcessMeta);
            TryUpdateTaskState(inProcessMeta, inProcessTaskIndexRecord, nowTicks, inProcessMeta.StartExecutingTicks, nowTicks, inProcessMeta.Attempts, terminalState, newExceptionInfoIds);
        }

        private void TrySwitchToWaitingForRerunState([NotNull] TaskMetaInformation inProcessMeta, TaskState waitingForRerunState, TimeSpan rerunDelay, [CanBeNull] List<TimeGuid> newExceptionInfoIds)
        {
            var nowTicks = Timestamp.Now.Ticks;
            var inProcessTaskIndexRecord = handleTasksMetaStorage.FormatIndexRecord(inProcessMeta);
            TryUpdateTaskState(inProcessMeta, inProcessTaskIndexRecord, nowTicks + rerunDelay.Ticks, inProcessMeta.StartExecutingTicks, nowTicks, inProcessMeta.Attempts, waitingForRerunState, newExceptionInfoIds);
        }

        [CanBeNull]
        private TaskMetaInformation TryUpdateTaskState([NotNull] TaskMetaInformation oldMeta, [NotNull] TaskIndexRecord oldTaskIndexRecord, long newMinimalStartTicks, long? startExecutingTicks, long? finishExecutingTicks, int attempts, TaskState newState, [CanBeNull] List<TimeGuid> newExceptionInfoIds)
        {
            var newMeta = allFieldsSerializer.Copy(oldMeta);
            if(newState == oldMeta.State)
                newMinimalStartTicks = Math.Max(newMinimalStartTicks, oldMeta.MinimalStartTicks + 1);
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
                logger.InfoFormat("Changed task state. Task = {0}", newMeta);
                return newMeta;
            }
            catch(Exception e)
            {
                logger.Error(string.Format("Can't update task state for: {0}", oldMeta), e);
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
        private static readonly ILog logger = LogManager.GetLogger(typeof(HandlerTask));
        private static readonly ISerializer allFieldsSerializer = new Serializer(new AllFieldsExtractor());        
        public static readonly TimeSpan MaxAllowedIndexInconsistencyDuration = TimeSpan.FromMinutes(1);        
        private readonly TaskShardMetrics taskShardMetrics;
    }
}