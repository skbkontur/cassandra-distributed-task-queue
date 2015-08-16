using System;
using System.Diagnostics;

using GroBuf;
using GroBuf.DataMembersExtracters;

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
    public class HandlerTask
    {
        public HandlerTask(
            string taskId,
            TaskQueueReason reason,
            Tuple<string, ColumnInfo> taskInfo,
            TaskMetaInformation meta,
            long startProcessingTicks,
            ITaskCounter taskCounter,
            ISerializer serializer,
            IRemoteTaskQueue remoteTaskQueue,
            IHandleTaskCollection handleTaskCollection,
            IRemoteLockCreator remoteLockCreator,
            IHandleTaskExceptionInfoStorage handleTaskExceptionInfoStorage,
            ITaskHandlerCollection taskHandlerCollection,
            IHandleTasksMetaStorage handleTasksMetaStorage,
            ITaskMinimalStartTicksIndex taskMinimalStartTicksIndex,
            IRemoteTaskQueueProfiler remoteTaskQueueProfiler)
        {
            this.taskId = taskId;
            this.reason = reason;
            this.taskInfo = taskInfo;
            taskMetaInformation = meta;
            this.startProcessingTicks = startProcessingTicks;
            this.taskCounter = taskCounter;
            this.serializer = serializer;
            this.remoteTaskQueue = remoteTaskQueue;
            this.handleTaskCollection = handleTaskCollection;
            this.remoteLockCreator = remoteLockCreator;
            this.handleTaskExceptionInfoStorage = handleTaskExceptionInfoStorage;
            this.taskHandlerCollection = taskHandlerCollection;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.taskMinimalStartTicksIndex = taskMinimalStartTicksIndex;
            this.remoteTaskQueueProfiler = remoteTaskQueueProfiler;
        }

        public LocalTaskProcessingResult RunTask()
        {
            var meta = taskMetaInformation;
            if(meta == null)
            {
                logger.InfoFormat("Мета для задачи TaskId = {0} еще не записана, ждем", taskId);
                if(TicksNameHelper.GetTicksFromColumnName(taskInfo.Item2.ColumnName) < (DateTime.UtcNow - TimeSpan.FromHours(1)).Ticks)
                {
                    logger.InfoFormat("Удаляем запись индекса, для которой не записалась мета (TaskId = {0}, ColumnName = {1}, RowKey = {2})", taskInfo.Item1, taskInfo.Item2.ColumnName, taskInfo.Item2.RowKey);
                    taskMinimalStartTicksIndex.UnindexMeta(taskInfo.Item1, taskInfo.Item2);
                }
                return LocalTaskProcessingResult.Undefined;
            }
            if(meta.MinimalStartTicks > TicksNameHelper.GetTicksFromColumnName(taskInfo.Item2.ColumnName))
            {
                logger.InfoFormat("Удаляем зависшую запись индекса (TaskId = {0}, ColumnName = {1}, RowKey = {2})", taskInfo.Item1, taskInfo.Item2.ColumnName, taskInfo.Item2.RowKey);
                taskMinimalStartTicksIndex.UnindexMeta(taskInfo.Item1, taskInfo.Item2);
            }
            if(meta.State == TaskState.Finished || meta.State == TaskState.Fatal || meta.State == TaskState.Canceled)
            {
                logger.InfoFormat("Даже не пытаемся обработать таску '{0}', потому что она уже находится в состоянии '{1}'", taskId, meta.State);
                taskMinimalStartTicksIndex.UnindexMeta(taskInfo.Item1, taskInfo.Item2);
                return LocalTaskProcessingResult.Undefined;
            }
            if(!taskHandlerCollection.ContainsHandlerFor(meta.Name))
                return LocalTaskProcessingResult.Undefined;
            var startTicks = Math.Max(startProcessingTicks, DateTime.UtcNow.Ticks);
            if(meta.MinimalStartTicks != 0 && (meta.MinimalStartTicks > startTicks))
            {
                logger.InfoFormat("MinimalStartTicks ({0}) задачи '{1}' больше, чем  startTicks ({2}), поэтому не берем задачу в обработку, ждем.",
                                  meta.MinimalStartTicks, meta.Id, startTicks);
                return LocalTaskProcessingResult.Undefined;
            }
            IRemoteLock taskGroupRemoteLock = null;
            if(!string.IsNullOrEmpty(meta.TaskGroupLock) && !remoteLockCreator.TryGetLock(meta.TaskGroupLock, out taskGroupRemoteLock))
            {
                logger.InfoFormat("Не смогли взять блокировку на задачу '{0}', так как выполняется другая задача из группы {1}.", taskId, meta.TaskGroupLock);
                return LocalTaskProcessingResult.Undefined;
            }
            try
            {
                if(!taskCounter.TryIncrement(reason))
                    return LocalTaskProcessingResult.Undefined;
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
                    taskCounter.Decrement(reason);
                }
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

            var startTicks = Math.Max(startProcessingTicks, DateTime.UtcNow.Ticks);
            if(task.Meta.MinimalStartTicks != 0 && (task.Meta.MinimalStartTicks > startTicks))
            {
                logger.InfoFormat("MinimalStartTicks ({0}) задачи '{1}' больше, чем  startTicks ({2}), поэтому не берем задачу в обработку, ждем.",
                                  task.Meta.MinimalStartTicks, task.Meta.Id, startTicks);
                return LocalTaskProcessingResult.Undefined;
            }

            if(task.Meta.State == TaskState.Finished || task.Meta.State == TaskState.Fatal || task.Meta.State == TaskState.Canceled)
            {
                logger.InfoFormat("Другая очередь успела обработать задачу '{0}'", taskId);
                taskMinimalStartTicksIndex.UnindexMeta(taskInfo.Item1, taskInfo.Item2);
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

        private static readonly ISerializer allFieldsSerializer = new Serializer(new AllFieldsExtractor());
        private static readonly ILog logger = LogManager.GetLogger(typeof(HandlerTask));
        private readonly IHandleTaskCollection handleTaskCollection;
        private readonly IRemoteLockCreator remoteLockCreator;
        private readonly IHandleTaskExceptionInfoStorage handleTaskExceptionInfoStorage;
        private readonly string taskId;
        private readonly TaskQueueReason reason;
        private readonly Tuple<string, ColumnInfo> taskInfo;
        private readonly TaskMetaInformation taskMetaInformation;
        private readonly long startProcessingTicks;
        private readonly ITaskCounter taskCounter;
        private readonly ISerializer serializer;
        private readonly IRemoteTaskQueue remoteTaskQueue;
        private readonly ITaskHandlerCollection taskHandlerCollection;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ITaskMinimalStartTicksIndex taskMinimalStartTicksIndex;
        private readonly IRemoteTaskQueueProfiler remoteTaskQueueProfiler;
    }
}