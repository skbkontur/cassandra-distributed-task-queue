using System;
using System.Diagnostics;

using GroBuf;
using GroBuf.DataMembersExtracters;

using JetBrains.Annotations;

using log4net;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
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
            handleTaskExceptionInfoStorage = remoteTaskQueueInternals.HandleTaskExceptionInfoStorage;
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
                logger.InfoFormat("MinimalStartTicks ({0}) задачи '{1}' больше, чем  startTicks ({2}), поэтому не берем задачу в обработку, ждем.", taskMeta.MinimalStartTicks, taskMeta.Id, nowTicks);
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

        private bool TryUpdateTaskState([NotNull] Task task, long newMinimalStartTicks, long? startExecutingTicks, long? finishExecutingTicks, int attempts, TaskState state)
        {
            var newMeta = allFieldsSerializer.Copy(task.Meta);
            newMeta.MinimalStartTicks = Math.Max(newMinimalStartTicks, task.Meta.MinimalStartTicks + 1);
            newMeta.StartExecutingTicks = startExecutingTicks;
            newMeta.FinishExecutingTicks = finishExecutingTicks;
            newMeta.Attempts = attempts;
            newMeta.State = state;
            try
            {
                handleTasksMetaStorage.AddMeta(newMeta);
                logger.InfoFormat("Changed task state. Task = {0}", newMeta);
                task.Meta = newMeta;
                return true;
            }
            catch(Exception e)
            {
                logger.Error("Can't update task state " + e);
                return false;
            }
        }

        private LocalTaskProcessingResult ProcessTask()
        {
            Task task;
            try
            {
                //note тут обязательно надо перечитывать мету
                task = handleTaskCollection.GetTask(taskIndexRecord.TaskId);
                if(task.Meta == null)
                {
                    logger.Warn(string.Format("Ошибка во время чтения задачи '{0}'. Отсутствует метаинформация.", taskIndexRecord.TaskId));
                    return LocalTaskProcessingResult.Undefined;
                }
            }
            catch(Exception e)
            {
                logger.Error(string.Format("Ошибка во время чтения задачи '{0}'", taskIndexRecord.TaskId), e);
                return LocalTaskProcessingResult.Undefined;
            }

            var nowTicks = DateTime.UtcNow.Ticks;
            if(task.Meta.MinimalStartTicks != 0 && (task.Meta.MinimalStartTicks > nowTicks))
            {
                logger.InfoFormat("MinimalStartTicks ({0}) задачи '{1}' больше, чем  startTicks ({2}), поэтому не берем задачу в обработку, ждем.", task.Meta.MinimalStartTicks, task.Meta.Id, nowTicks);
                return LocalTaskProcessingResult.Undefined;
            }

            if(task.Meta.State == TaskState.Finished || task.Meta.State == TaskState.Fatal || task.Meta.State == TaskState.Canceled)
            {
                logger.InfoFormat("Другая очередь успела обработать задачу '{0}'", taskIndexRecord.TaskId);
                taskMinimalStartTicksIndex.RemoveRecord(taskIndexRecord);
                return LocalTaskProcessingResult.Undefined;
            }

            logger.InfoFormat("Начинаем обрабатывать задачу [{0}]. Reason = {1}", task.Meta, reason);

            nowTicks = DateTime.UtcNow.Ticks;
            if(!TryUpdateTaskState(task, nowTicks, nowTicks, null, task.Meta.Attempts + 1, TaskState.InProcess))
            {
                logger.ErrorFormat("Не удалось обновить метаинформацию у задачи '{0}'.", task.Meta);
                return LocalTaskProcessingResult.Undefined;
            }

            ITaskHandler taskHandler;
            try
            {
                taskHandler = taskHandlerRegistry.CreateHandlerFor(task.Meta.Name);
            }
            catch(Exception e)
            {
                LogError(e, task.Meta);
                nowTicks = DateTime.UtcNow.Ticks;
                TryUpdateTaskState(task, nowTicks, task.Meta.StartExecutingTicks, nowTicks, task.Meta.Attempts, TaskState.Fatal);
                return LocalTaskProcessingResult.Error;
            }

            LocalTaskProcessingResult localTaskProcessingResult;
            using(TaskExecutionContext.ForTask(task))
            {
                try
                {
                    var sw = Stopwatch.StartNew();
                    remoteTaskQueueProfiler.ProcessTaskDequeueing(task.Meta);
                    var handleResult = taskHandler.HandleTask(remoteTaskQueue, serializer, remoteLockCreator, task);
                    TimeSpan taskExecutionTime = sw.Elapsed;
                    remoteTaskQueueProfiler.ProcessTaskExecutionFinished(task.Meta, handleResult, taskExecutionTime);
                    localTaskProcessingResult = UpdateTaskMetaByHandleResult(task, handleResult);
                }
                catch(Exception e)
                {
                    remoteTaskQueueProfiler.ProcessTaskExecutionFailed(task.Meta, e);
                    localTaskProcessingResult = LocalTaskProcessingResult.Error;
                    LogError(e, task.Meta);
                    nowTicks = DateTime.UtcNow.Ticks;
                    TryUpdateTaskState(task, nowTicks, task.Meta.StartExecutingTicks, nowTicks, task.Meta.Attempts, TaskState.Fatal);
                }
            }
            return localTaskProcessingResult;
        }

        private LocalTaskProcessingResult UpdateTaskMetaByHandleResult([NotNull] Task task, [NotNull] HandleResult handleResult)
        {
            var nowTicks = DateTime.UtcNow.Ticks;
            switch(handleResult.FinishAction)
            {
            case FinishAction.Finish:
                TryUpdateTaskState(task, nowTicks, task.Meta.StartExecutingTicks, nowTicks, task.Meta.Attempts, TaskState.Finished);
                return LocalTaskProcessingResult.Success;
            case FinishAction.Fatal:
                TryUpdateTaskState(task, nowTicks, task.Meta.StartExecutingTicks, nowTicks, task.Meta.Attempts, TaskState.Fatal);
                LogError(handleResult.Error, task.Meta);
                return LocalTaskProcessingResult.Error;
            case FinishAction.RerunAfterError:
                TryUpdateTaskState(task, nowTicks + handleResult.RerunDelay.Ticks, task.Meta.StartExecutingTicks, nowTicks, task.Meta.Attempts, TaskState.WaitingForRerunAfterError);
                LogError(handleResult.Error, task.Meta);
                return LocalTaskProcessingResult.Rerun;
            case FinishAction.Rerun:
                TryUpdateTaskState(task, nowTicks + handleResult.RerunDelay.Ticks, task.Meta.StartExecutingTicks, nowTicks, task.Meta.Attempts, TaskState.WaitingForRerun);
                return LocalTaskProcessingResult.Rerun;
            default:
                throw new InvalidProgramStateException(string.Format("Invalid FinishAction: {0}", handleResult.FinishAction));
            }
        }

        private void LogError(Exception e, TaskMetaInformation meta)
        {
            TaskExceptionInfo previousExceptionInfo;
            if(!handleTaskExceptionInfoStorage.TryGetExceptionInfo(taskIndexRecord.TaskId, out previousExceptionInfo))
                previousExceptionInfo = new TaskExceptionInfo();
            if(!previousExceptionInfo.EqualsToException(e))
                logger.Error(string.Format("Ошибка во время обработки задачи '{0}'.", meta), e);
            handleTaskExceptionInfoStorage.TryAddExceptionInfo(taskIndexRecord.TaskId, e);
        }

        private readonly TaskIndexRecord taskIndexRecord;
        private readonly TaskQueueReason reason;
        private readonly TaskMetaInformation taskMeta;
        private readonly ITaskHandlerRegistry taskHandlerRegistry;
        private readonly ISerializer serializer;
        private readonly IRemoteTaskQueue remoteTaskQueue;
        private readonly IHandleTaskCollection handleTaskCollection;
        private readonly IRemoteLockCreator remoteLockCreator;
        private readonly IHandleTaskExceptionInfoStorage handleTaskExceptionInfoStorage;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ITaskMinimalStartTicksIndex taskMinimalStartTicksIndex;
        private readonly IRemoteTaskQueueProfiler remoteTaskQueueProfiler;
        private static readonly ILog logger = LogManager.GetLogger(typeof(HandlerTask));
        private static readonly ISerializer allFieldsSerializer = new Serializer(new AllFieldsExtractor());
    }
}