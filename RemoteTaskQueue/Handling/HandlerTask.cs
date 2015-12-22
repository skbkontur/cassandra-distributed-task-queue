﻿using System;
using System.Diagnostics;

using GroBuf;
using GroBuf.DataMembersExtracters;

using JetBrains.Annotations;

using log4net;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Configuration;
using RemoteQueue.Handling.ExecutionContext;
using RemoteQueue.LocalTasks.TaskQueue;
using RemoteQueue.Profiling;

using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;
using SKBKontur.Catalogue.Objects;

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
        }

        public LocalTaskProcessingResult RunTask()
        {
            if(taskMeta == null)
            {
                logger.InfoFormat("Удаляем запись индекса, для которой не записалась мета: {0}", taskIndexRecord);
                taskMinimalStartTicksIndex.RemoveRecord(taskIndexRecord);
                return LocalTaskProcessingResult.Undefined;
            }
            if(taskMeta.MinimalStartTicks > taskIndexRecord.MinimalStartTicks)
            {
                logger.InfoFormat("Удаляем зависшую запись индекса: {0}", taskIndexRecord);
                taskMinimalStartTicksIndex.RemoveRecord(taskIndexRecord);
            }
            if(taskMeta.State == TaskState.Finished || taskMeta.State == TaskState.Fatal || taskMeta.State == TaskState.Canceled)
            {
                logger.InfoFormat("Даже не пытаемся обработать таску '{0}', потому что она уже находится в состоянии '{1}'", taskIndexRecord.TaskId, taskMeta.State);
                taskMinimalStartTicksIndex.RemoveRecord(taskIndexRecord);
                return LocalTaskProcessingResult.Undefined;
            }
            var nowTicks = DateTime.UtcNow.Ticks;
            if(taskMeta.MinimalStartTicks != 0 && (taskMeta.MinimalStartTicks > nowTicks))
            {
                logger.InfoFormat("MinimalStartTicks ({0}) задачи '{1}' больше, чем nowTicks ({2}), поэтому не берем задачу в обработку, ждем.", taskMeta.MinimalStartTicks, taskMeta.Id, nowTicks);
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
                using(remoteLock)
                    return ProcessTask();
            }
            finally
            {
                if(taskGroupRemoteLock != null)
                    taskGroupRemoteLock.Dispose();
            }
        }

        [CanBeNull]
        private TaskMetaInformation TryUpdateTaskState([NotNull] TaskMetaInformation oldMeta, long newMinimalStartTicks, long? startExecutingTicks, long? finishExecutingTicks, int attempts, TaskState newState)
        {
            var newMeta = allFieldsSerializer.Copy(oldMeta);
            newMeta.MinimalStartTicks = Math.Max(newMinimalStartTicks, oldMeta.MinimalStartTicks + 1);
            newMeta.StartExecutingTicks = startExecutingTicks;
            newMeta.FinishExecutingTicks = finishExecutingTicks;
            newMeta.Attempts = attempts;
            newMeta.State = newState;
            try
            {
                handleTasksMetaStorage.AddMeta(newMeta);
                logger.InfoFormat("Changed task state. Task = {0}", newMeta);
                return newMeta;
            }
            catch(Exception e)
            {
                logger.Error(string.Format("Can't update task state for: {0}", oldMeta), e);
                return null;
            }
        }

        private LocalTaskProcessingResult ProcessTask()
        {
            byte[] taskData;
            TaskMetaInformation oldMeta;
            try
            {
                //note тут обязательно надо перечитывать мету
                var task = handleTaskCollection.GetTask(taskIndexRecord.TaskId);
                oldMeta = task.Meta;
                taskData = task.Data;
            }
            catch(Exception e)
            {
                logger.Error(string.Format("Ошибка во время чтения задачи '{0}'", taskIndexRecord.TaskId), e);
                return LocalTaskProcessingResult.Undefined;
            }

            var nowTicks = DateTime.UtcNow.Ticks;
            if(oldMeta.MinimalStartTicks != 0 && (oldMeta.MinimalStartTicks > nowTicks))
            {
                logger.InfoFormat("MinimalStartTicks ({0}) задачи '{1}' больше, чем nowTicks ({2}), поэтому не берем задачу в обработку, ждем.", oldMeta.MinimalStartTicks, oldMeta.Id, nowTicks);
                return LocalTaskProcessingResult.Undefined;
            }

            if(oldMeta.State == TaskState.Finished || oldMeta.State == TaskState.Fatal || oldMeta.State == TaskState.Canceled)
            {
                logger.InfoFormat("Другая очередь успела обработать задачу '{0}'", taskIndexRecord.TaskId);
                taskMinimalStartTicksIndex.RemoveRecord(taskIndexRecord);
                return LocalTaskProcessingResult.Undefined;
            }

            logger.InfoFormat("Начинаем обрабатывать задачу [{0}]. Reason = {1}", oldMeta, reason);

            var inProcessMeta = TrySwitchToInProcessState(oldMeta);
            if(inProcessMeta == null)
            {
                logger.ErrorFormat("Не удалось обновить метаинформацию у задачи '{0}'.", oldMeta);
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
                LogError(e, inProcessMeta);
                TrySwitchToTerminalState(inProcessMeta, TaskState.Fatal);
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
                    LogError(e, inProcessMeta);
                    TrySwitchToTerminalState(inProcessMeta, TaskState.Fatal);
                }
            }
            return localTaskProcessingResult;
        }

        private LocalTaskProcessingResult UpdateTaskMetaByHandleResult([NotNull] TaskMetaInformation inProcessMeta, [NotNull] HandleResult handleResult)
        {
            switch(handleResult.FinishAction)
            {
            case FinishAction.Finish:
                TrySwitchToTerminalState(inProcessMeta, TaskState.Finished);
                return LocalTaskProcessingResult.Success;
            case FinishAction.Fatal:
                LogError(handleResult.Error, inProcessMeta);
                TrySwitchToTerminalState(inProcessMeta, TaskState.Fatal);
                return LocalTaskProcessingResult.Error;
            case FinishAction.RerunAfterError:
                LogError(handleResult.Error, inProcessMeta);
                TrySwitchToWaitingForRerunState(inProcessMeta, TaskState.WaitingForRerunAfterError, handleResult.RerunDelay);
                return LocalTaskProcessingResult.Rerun;
            case FinishAction.Rerun:
                TrySwitchToWaitingForRerunState(inProcessMeta, TaskState.WaitingForRerun, handleResult.RerunDelay);
                return LocalTaskProcessingResult.Rerun;
            default:
                throw new InvalidProgramStateException(string.Format("Invalid FinishAction: {0}", handleResult.FinishAction));
            }
        }

        private void LogError([NotNull] Exception e, [NotNull] TaskMetaInformation inProcessMeta)
        {
            logger.Error(string.Format("Ошибка во время обработки задачи: {0}", inProcessMeta), e);
            try
            {
                BlobId taskExceptionInfoId;
                if(taskExceptionInfoStorage.TryAddNewExceptionInfo(inProcessMeta, e, out taskExceptionInfoId))
                    inProcessMeta.AddTaskExceptionInfoId(taskExceptionInfoId);
            }
            catch
            {
                logger.Error(string.Format("Не смогли записать ошибку для задачи: {0}", inProcessMeta), e);
            }
        }

        [CanBeNull]
        private TaskMetaInformation TrySwitchToInProcessState([NotNull] TaskMetaInformation oldMeta)
        {
            var nowTicks = DateTime.UtcNow.Ticks;
            var inProcessMeta = TryUpdateTaskState(oldMeta, nowTicks, nowTicks, null, oldMeta.Attempts + 1, TaskState.InProcess);
            return inProcessMeta;
        }

        private void TrySwitchToTerminalState([NotNull] TaskMetaInformation inProcessMeta, TaskState terminalState)
        {
            var nowTicks = DateTime.UtcNow.Ticks;
            TryUpdateTaskState(inProcessMeta, nowTicks, inProcessMeta.StartExecutingTicks, nowTicks, inProcessMeta.Attempts, terminalState);
        }

        private void TrySwitchToWaitingForRerunState([NotNull] TaskMetaInformation inProcessMeta, TaskState waitingForRerunState, TimeSpan rerunDelay)
        {
            var nowTicks = DateTime.UtcNow.Ticks;
            TryUpdateTaskState(inProcessMeta, nowTicks + rerunDelay.Ticks, inProcessMeta.StartExecutingTicks, nowTicks, inProcessMeta.Attempts, waitingForRerunState);
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
        private static readonly ILog logger = LogManager.GetLogger(typeof(HandlerTask));
        private static readonly ISerializer allFieldsSerializer = new Serializer(new AllFieldsExtractor());
    }
}