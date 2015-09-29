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
using RemoteQueue.Handling.ExecutionContext;
using RemoteQueue.Handling.HandlerResults;
using RemoteQueue.LocalTasks.TaskQueue;
using RemoteQueue.Profiling;

using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;
using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Handling
{
    internal class HandlerTask
    {
        public HandlerTask(
            [NotNull] string taskId,
            TaskQueueReason reason,
            [NotNull] ColumnInfo taskInfo,
            [CanBeNull] TaskMetaInformation taskMeta,
            ITaskHandlerCollection taskHandlerCollection,
            IRemoteTaskQueueInternals remoteTaskQueueInternals)
        {
            this.taskId = taskId;
            this.reason = reason;
            this.taskInfo = taskInfo;
            this.taskMeta = taskMeta;
            this.taskHandlerCollection = taskHandlerCollection;
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
                logger.InfoFormat("Удаляем запись индекса, для которой не записалась мета (TaskId = {0}, ColumnName = {1}, RowKey = {2})", taskId, taskInfo.ColumnName, taskInfo.RowKey);
                taskMinimalStartTicksIndex.UnindexMeta(taskId, taskInfo);
                return LocalTaskProcessingResult.Undefined;
            }
            if(taskMeta.MinimalStartTicks > TicksNameHelper.GetTicksFromColumnName(taskInfo.ColumnName))
            {
                logger.InfoFormat("Удаляем зависшую запись индекса (TaskId = {0}, ColumnName = {1}, RowKey = {2})", taskId, taskInfo.ColumnName, taskInfo.RowKey);
                taskMinimalStartTicksIndex.UnindexMeta(taskId, taskInfo);
            }
            if(taskMeta.State == TaskState.Finished || taskMeta.State == TaskState.Fatal || taskMeta.State == TaskState.Canceled)
            {
                logger.InfoFormat("Даже не пытаемся обработать таску '{0}', потому что она уже находится в состоянии '{1}'", taskId, taskMeta.State);
                taskMinimalStartTicksIndex.UnindexMeta(taskId, taskInfo);
                return LocalTaskProcessingResult.Undefined;
            }
            var nowTicks = DateTime.UtcNow.Ticks;
            if(taskMeta.MinimalStartTicks != 0 && (taskMeta.MinimalStartTicks > nowTicks))
            {
                logger.InfoFormat("MinimalStartTicks ({0}) задачи '{1}' больше, чем  startTicks ({2}), поэтому не берем задачу в обработку, ждем.",
                                  taskMeta.MinimalStartTicks, taskMeta.Id, nowTicks);
                return LocalTaskProcessingResult.Undefined;
            }
            IRemoteLock taskGroupRemoteLock = null;
            if(!string.IsNullOrEmpty(taskMeta.TaskGroupLock) && !remoteLockCreator.TryGetLock(taskMeta.TaskGroupLock, out taskGroupRemoteLock))
            {
                logger.InfoFormat("Не смогли взять блокировку на задачу '{0}', так как выполняется другая задача из группы {1}.", taskId, taskMeta.TaskGroupLock);
                return LocalTaskProcessingResult.Undefined;
            }
            try
            {
                IRemoteLock remoteLock;
                if(!remoteLockCreator.TryGetLock(taskId, out remoteLock))
                {
                    logger.InfoFormat("Не смогли взять блокировку на задачу '{0}', пропускаем её.", taskId);
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

        private bool TryUpdateTaskState(Task task, long? minimalStartTicks, long? startExecutingTicks, long? finishExecutingTicks, int attempts, TaskState state)
        {
            var metaForWrite = allFieldsSerializer.Copy(task.Meta);

            metaForWrite.MinimalStartTicks = Math.Max(metaForWrite.MinimalStartTicks, minimalStartTicks ?? 0) + 1;
            metaForWrite.StartExecutingTicks = startExecutingTicks;
            metaForWrite.FinishExecutingTicks = finishExecutingTicks;

            metaForWrite.Attempts = attempts;
            metaForWrite.State = state;
            try
            {
                handleTasksMetaStorage.AddMeta(metaForWrite);
                logger.InfoFormat("Changed task state. Task = {0}", metaForWrite);
                task.Meta = metaForWrite;
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
                task = handleTaskCollection.GetTask(taskId);
                if(task.Meta == null)
                {
                    logger.Warn(string.Format("Ошибка во время чтения задачи '{0}'. Отсутствует метаинформация.", taskId));
                    return LocalTaskProcessingResult.Undefined;
                }
            }
            catch(Exception e)
            {
                logger.Error(string.Format("Ошибка во время чтения задачи '{0}'", taskId), e);
                return LocalTaskProcessingResult.Undefined;
            }

            var nowTicks = DateTime.UtcNow.Ticks;
            if(task.Meta.MinimalStartTicks != 0 && (task.Meta.MinimalStartTicks > nowTicks))
            {
                logger.InfoFormat("MinimalStartTicks ({0}) задачи '{1}' больше, чем  startTicks ({2}), поэтому не берем задачу в обработку, ждем.",
                                  task.Meta.MinimalStartTicks, task.Meta.Id, nowTicks);
                return LocalTaskProcessingResult.Undefined;
            }

            if(task.Meta.State == TaskState.Finished || task.Meta.State == TaskState.Fatal || task.Meta.State == TaskState.Canceled)
            {
                logger.InfoFormat("Другая очередь успела обработать задачу '{0}'", taskId);
                taskMinimalStartTicksIndex.UnindexMeta(taskId, taskInfo);
                return LocalTaskProcessingResult.Undefined;
            }

            logger.InfoFormat("Начинаем обрабатывать задачу [{0}]. Reason = {1}", task.Meta, reason);

            if(!TryUpdateTaskState(task, null, DateTime.UtcNow.Ticks, null, task.Meta.Attempts + 1, TaskState.InProcess))
            {
                logger.ErrorFormat("Не удалось обновить метаинформацию у задачи '{0}'.", task.Meta);
                return LocalTaskProcessingResult.Undefined;
            }

            ITaskHandler taskHandler;
            try
            {
                taskHandler = taskHandlerCollection.CreateHandler(task.Meta.Name);
            }
            catch(Exception e)
            {
                LogError(e, task.Meta);
                TryUpdateTaskState(task, null, task.Meta.StartExecutingTicks, DateTime.UtcNow.Ticks, task.Meta.Attempts, TaskState.Fatal);
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
                    remoteTaskQueueProfiler.RecordTaskExecutionTime(task.Meta, sw.Elapsed);
                    remoteTaskQueueProfiler.RecordTaskExecutionResult(task.Meta, handleResult);
                    localTaskProcessingResult = UpdateTaskMetaByHandleResult(task, handleResult);
                }
                catch(Exception e)
                {
                    localTaskProcessingResult = LocalTaskProcessingResult.Error;
                    LogError(e, task.Meta);
                    TryUpdateTaskState(task, null, task.Meta.StartExecutingTicks, DateTime.UtcNow.Ticks, task.Meta.Attempts, TaskState.Fatal);
                }
            }
            return localTaskProcessingResult;
        }

        private LocalTaskProcessingResult UpdateTaskMetaByHandleResult(Task task, HandleResult handleResult)
        {
            var nowTicks = DateTime.UtcNow.Ticks;
            switch(handleResult.FinishAction)
            {
            case FinishAction.Finish:
                TryUpdateTaskState(task, null, task.Meta.StartExecutingTicks, nowTicks, task.Meta.Attempts, TaskState.Finished);
                return LocalTaskProcessingResult.Success;
            case FinishAction.Fatal:
                TryUpdateTaskState(task, null, task.Meta.StartExecutingTicks, nowTicks, task.Meta.Attempts, TaskState.Fatal);
                LogError(handleResult.Error, task.Meta);
                return LocalTaskProcessingResult.Error;
            case FinishAction.RerunAfterError:
                TryUpdateTaskState(task, handleResult.RerunDelay.Ticks + nowTicks,
                                   task.Meta.StartExecutingTicks, nowTicks, task.Meta.Attempts,
                                   TaskState.WaitingForRerunAfterError);
                LogError(handleResult.Error, task.Meta);
                return LocalTaskProcessingResult.Rerun;
            case FinishAction.Rerun:
                TryUpdateTaskState(task, handleResult.RerunDelay.Ticks + nowTicks,
                                   task.Meta.StartExecutingTicks, nowTicks, task.Meta.Attempts, TaskState.WaitingForRerun);
                return LocalTaskProcessingResult.Rerun;
            default:
                throw new InvalidProgramStateException(string.Format("Invalid FinishAction: {0}", handleResult.FinishAction));
            }
        }

        private void LogError(Exception e, TaskMetaInformation meta)
        {
            TaskExceptionInfo previousExceptionInfo;
            if(!handleTaskExceptionInfoStorage.TryGetExceptionInfo(taskId, out previousExceptionInfo))
                previousExceptionInfo = new TaskExceptionInfo();
            if(!previousExceptionInfo.EqualsToException(e))
                logger.Error(string.Format("Ошибка во время обработки задачи '{0}'.", meta), e);
            handleTaskExceptionInfoStorage.TryAddExceptionInfo(taskId, e);
        }

        private readonly string taskId;
        private readonly TaskQueueReason reason;
        private readonly ColumnInfo taskInfo;
        private readonly TaskMetaInformation taskMeta;
        private readonly ITaskHandlerCollection taskHandlerCollection;
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