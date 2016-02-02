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
        }

        public LocalTaskProcessingResult RunTask()
        {
            if(taskMeta == null)
            {
                logger.ErrorFormat("Удаляем запись индекса, для которой не записалась мета: {0}", taskIndexRecord);
                taskMinimalStartTicksIndex.RemoveRecord(taskIndexRecord, globalTime.UpdateNowTicks());
                return LocalTaskProcessingResult.Undefined;
            }
            var nowTicks = Timestamp.Now.Ticks;
            if(taskMeta.State == TaskState.Finished || taskMeta.State == TaskState.Fatal || taskMeta.State == TaskState.Canceled)
            {
                logger.InfoFormat("Даже не пытаемся обработать таску '{0}', потому что она уже находится в состоянии '{1}'", taskIndexRecord.TaskId, taskMeta.State);
                if(taskIndexRecord.MinimalStartTicks < nowTicks - maxAllowedIndexInconsistencyDuration.Ticks)
                {
                    logger.ErrorFormat("Удаляем зависшую запись индекса: {0}", taskIndexRecord);
                    taskMinimalStartTicksIndex.RemoveRecord(taskIndexRecord, globalTime.UpdateNowTicks());
                }
                return LocalTaskProcessingResult.Undefined;
            }
            if(taskMeta.MinimalStartTicks > nowTicks && taskIndexRecord.MinimalStartTicks > nowTicks - maxAllowedIndexInconsistencyDuration.Ticks)
            {
                logger.InfoFormat("MinimalStartTicks ({0}) задачи '{1}' в состоянии {2} больше, чем nowTicks ({3}), поэтому не берем задачу в обработку, ждем; taskIndexRecord: {4}",
                                  taskMeta.MinimalStartTicks, taskMeta.Id, taskMeta.State, nowTicks, taskIndexRecord);
                return LocalTaskProcessingResult.Undefined;
            }
            IRemoteLock taskGroupRemoteLock = null;
            if(!string.IsNullOrEmpty(taskMeta.TaskGroupLock) && !remoteLockCreator.TryGetLock(taskMeta.TaskGroupLock, out taskGroupRemoteLock))
            {
                logger.InfoFormat("Не смогли взять блокировку на задачу '{0}', так как выполняется другая задача из группы {1}.", taskIndexRecord.TaskId, taskMeta.TaskGroupLock);
                return LocalTaskProcessingResult.Undefined;
            }
            try
            {
                IRemoteLock remoteLock;
                if(!remoteLockCreator.TryGetLock(taskIndexRecord.TaskId, out remoteLock))
                {
                    logger.InfoFormat("Не смогли взять блокировку на задачу '{0}', пропускаем её.", taskIndexRecord.TaskId);
                    return LocalTaskProcessingResult.Undefined;
                }
                LocalTaskProcessingResult result;
                using(remoteLock)
                    result = ProcessTask();
                logger.InfoFormat("Завершили выполнение задачи '{0}' с результатом '{1}'. Отпустили блокировку ('{2}').", taskMeta.Id, result, taskIndexRecord.TaskId);
                return result;
            }
            finally
            {
                if(taskGroupRemoteLock != null)
                {
                    taskGroupRemoteLock.Dispose();
                    logger.InfoFormat("Отпустили блокировку '{0}' в процессе завершения задачи '{1}'.", taskMeta.TaskGroupLock, taskMeta.Id);
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
                logger.Error(string.Format("Ошибка во время чтения задачи '{0}'", taskIndexRecord.TaskId), e);
                return LocalTaskProcessingResult.Undefined;
            }

            if(oldMeta.State == TaskState.Finished || oldMeta.State == TaskState.Fatal || oldMeta.State == TaskState.Canceled)
            {
                logger.InfoFormat("Другая очередь успела обработать задачу: {0}", taskIndexRecord);
                return LocalTaskProcessingResult.Undefined;
            }

            var nowTicks = Timestamp.Now.Ticks;
            if(oldMeta.MinimalStartTicks > nowTicks)
            {
                if(taskIndexRecord.MinimalStartTicks > nowTicks - maxAllowedIndexInconsistencyDuration.Ticks)
                {
                    logger.InfoFormat("После перечитывания меты под локом MinimalStartTicks ({0}) задачи '{1}' в состоянии {2} больше, чем nowTicks ({3}), поэтому не берем задачу в обработку, ждем; taskIndexRecord: {4}",
                                      oldMeta.MinimalStartTicks, oldMeta.Id, oldMeta.State, nowTicks, taskIndexRecord);
                }
                else
                {
                    var newIndexRecord = handleTasksMetaStorage.FormatIndexRecord(oldMeta);
                    logger.ErrorFormat("После перечитывания меты под локом MinimalStartTicks ({0}) задачи '{1}' в состоянии {2} больше, чем nowTicks ({3}), поэтому не берем задачу в обработку и чиним индекс; oldIndexRecord: {4}; newIndexRecord: {5}",
                                       oldMeta.MinimalStartTicks, oldMeta.Id, oldMeta.State, nowTicks, taskIndexRecord, newIndexRecord);
                    var globalNowTicks = globalTime.UpdateNowTicks();
                    taskMinimalStartTicksIndex.AddRecord(newIndexRecord, globalNowTicks);
                    taskMinimalStartTicksIndex.RemoveRecord(taskIndexRecord, globalNowTicks);
                }
                return LocalTaskProcessingResult.Undefined;
            }

            logger.InfoFormat("Начинаем обрабатывать задачу [{0}]. Reason = {1}", oldMeta, reason);

            var inProcessMeta = TrySwitchToInProcessState(oldMeta);
            if(inProcessMeta == null)
            {
                logger.ErrorFormat("Не удалось начать обработку задачи: {0}", oldMeta);
                return LocalTaskProcessingResult.Undefined;
            }

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
        private static readonly TimeSpan maxAllowedIndexInconsistencyDuration = TimeSpan.FromMinutes(1);
    }
}