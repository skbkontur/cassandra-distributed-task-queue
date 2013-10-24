using System;

using GroBuf;
using GroBuf.DataMembersExtracters;

using RemoteLock;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Handling.HandlerResults;
using RemoteQueue.LocalTasks.TaskQueue;

using log4net;

namespace RemoteQueue.Handling
{
    public class HandlerTask : SimpleTask
    {
        public HandlerTask(
            Tuple<string, ColumnInfo> taskInfo,
            long startProcessingTicks,
            ITaskCounter taskCounter,
            ISerializer serializer,
            IRemoteTaskQueue remoteTaskQueue,
            IHandleTaskCollection handleTaskCollection,
            IRemoteLockCreator remoteLockCreator,
            IHandleTaskExceptionInfoStorage handleTaskExceptionInfoStorage,
            ITaskHandlerCollection taskHandlerCollection,
            IHandleTasksMetaStorage handleTasksMetaStorage,
            ITaskMinimalStartTicksIndex taskMinimalStartTicksIndex)
            : base(taskInfo.Item1)
        {
            this.taskInfo = taskInfo;
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
        }

        public override void Run()
        {
            var meta = handleTasksMetaStorage.GetMeta(Id);
            if(meta.MinimalStartTicks > TicksNameHelper.GetTicksFromColumnName(taskInfo.Item2.ColumnName))
            {
                logger.InfoFormat("Удаляем зависшую запись индекса (TaskId = {0}, ColumnName = {1}, RowKey = {2})", taskInfo.Item1, taskInfo.Item2.ColumnName, taskInfo.Item2.RowKey);
                taskMinimalStartTicksIndex.UnindexMeta(taskInfo.Item2);
            }
            if(!taskHandlerCollection.ContainsHandlerFor(meta.Name))
                return;
            IRemoteLock taskGroupRemoteLock = null;
            if(!string.IsNullOrEmpty(meta.TaskGroupLock) && !remoteLockCreator.TryGetLock(meta.TaskGroupLock, out taskGroupRemoteLock))
            {
                logger.InfoFormat("Не смогли взять блокировку на задачу '{0}', так как выполняется другая задача из группы {1}.", Id, meta.TaskGroupLock);
                return;
            }
            try
            {
                if(!taskCounter.TryIncrement()) return;
                try
                {
                    IRemoteLock remoteLock;
                    if(!remoteLockCreator.TryGetLock(Id, out remoteLock))
                    {
                        logger.InfoFormat("Не смогли взять блокировку на задачу '{0}', пропускаем её.", Id);
                        return;
                    }
                    using(remoteLock)
                        ProcessTask();
                }
                finally
                {
                    taskCounter.Decrement();
                }
            }
            finally
            {
                if(taskGroupRemoteLock != null) taskGroupRemoteLock.Dispose();
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
                task.Meta = metaForWrite;
                return true;
            }
            catch(Exception e)
            {
                logger.Error("Can't update task state " + e);
                return false;
            }
        }

        private void ProcessTask()
        {
            Task task;
            try
            {
                //note тут обязательно надо перечитывать мету
                task = handleTaskCollection.GetTask(Id);
                if(task.Meta == null)
                {
                    logger.Warn(string.Format("Ошибка во время чтения задачи '{0}'. Отсутствует метаинформация.", Id));
                    return;
                }
            }
            catch(Exception e)
            {
                logger.Error(string.Format("Ошибка во время чтения задачи '{0}'", Id), e);
                return;
            }

            if(task.Meta.State == TaskState.Finished || task.Meta.State == TaskState.Fatal ||
               task.Meta.State == TaskState.Canceled)
            {
                logger.InfoFormat("Другая очередь успела обработать задачу '{0}'", Id);
                taskMinimalStartTicksIndex.UnindexMeta(taskInfo.Item2);
                return;
            }

            if(task.Meta.MinimalStartTicks != 0 && (task.Meta.MinimalStartTicks > Math.Max(startProcessingTicks, DateTime.UtcNow.Ticks)))
            {
                logger.InfoFormat("Другая очередь успела обработать задачу '{0}'", Id);
                taskMinimalStartTicksIndex.UnindexMeta(taskInfo.Item2);
                return;
            }

            logger.InfoFormat("Начинаем обрабатывать задачу [{0}]", task.Meta);

            if(!TryUpdateTaskState(task, null, DateTime.UtcNow.Ticks, null, task.Meta.Attempts + 1, TaskState.InProcess))
            {
                logger.ErrorFormat("Не удалось обновить метаинформацию у задачи '{0}'.", task.Meta);
                return;
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
                return;
            }

            try
            {
                task.Meta = task.Meta;
                var handleResult = taskHandler.HandleTask(remoteTaskQueue, serializer, remoteLockCreator, task);
                UpdateTaskMetaByHandleResult(task, handleResult);
            }
            catch(Exception e)
            {
                LogError(e, task.Meta);
                TryUpdateTaskState(task, null, task.Meta.StartExecutingTicks, DateTime.UtcNow.Ticks, task.Meta.Attempts, TaskState.Fatal);
            }
        }

        private void UpdateTaskMetaByHandleResult(Task task, HandleResult handleResult)
        {
            var nowTicks = DateTime.UtcNow.Ticks;
            switch(handleResult.FinishAction)
            {
            case FinishAction.Finish:
                TryUpdateTaskState(task, null, task.Meta.StartExecutingTicks, nowTicks, task.Meta.Attempts, TaskState.Finished);
                break;
            case FinishAction.Fatal:
                TryUpdateTaskState(task, null, task.Meta.StartExecutingTicks, nowTicks, task.Meta.Attempts, TaskState.Fatal);
                LogError(handleResult.Error, task.Meta);
                break;
            case FinishAction.RerunAfterError:
                TryUpdateTaskState(task, handleResult.RerunDelay.Ticks + nowTicks,
                                   task.Meta.StartExecutingTicks, nowTicks, task.Meta.Attempts,
                                   TaskState.WaitingForRerunAfterError);
                LogError(handleResult.Error, task.Meta);
                break;
            case FinishAction.Rerun:
                TryUpdateTaskState(task, handleResult.RerunDelay.Ticks + nowTicks,
                                   task.Meta.StartExecutingTicks, nowTicks, task.Meta.Attempts, TaskState.WaitingForRerun);
                break;
            }
        }

        private void LogError(Exception e, TaskMetaInformation meta)
        {
            TaskExceptionInfo previousExceptionInfo;
            if(!handleTaskExceptionInfoStorage.TryGetExceptionInfo(Id, out previousExceptionInfo))
                previousExceptionInfo = new TaskExceptionInfo();
            if(!previousExceptionInfo.EqualsToException(e))
                logger.Error(string.Format("Ошибка во время обработки задачи '{0}'.", meta), e);
            handleTaskExceptionInfoStorage.TryAddExceptionInfo(Id, e);
        }

        private static readonly SerializerImpl allFieldsSerializer = new SerializerImpl(new AllFieldsExtractor());

        private static readonly ILog logger = LogManager.GetLogger(typeof(HandlerTask));
        private readonly IHandleTaskCollection handleTaskCollection;
        private readonly IRemoteLockCreator remoteLockCreator;
        private readonly IHandleTaskExceptionInfoStorage handleTaskExceptionInfoStorage;
        private readonly Tuple<string, ColumnInfo> taskInfo;
        private readonly long startProcessingTicks;
        private readonly ITaskCounter taskCounter;
        private readonly ISerializer serializer;
        private readonly IRemoteTaskQueue remoteTaskQueue;
        private readonly ITaskHandlerCollection taskHandlerCollection;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly ITaskMinimalStartTicksIndex taskMinimalStartTicksIndex;
    }
}