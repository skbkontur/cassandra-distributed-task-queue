using System;

using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.RemoteLock;
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
            ITaskCounter taskCounter,
            ISerializer serializer,
            IRemoteTaskQueue remoteTaskQueue,
            IHandleTaskCollection handleTaskCollection,
            IRemoteLockCreator remoteLockCreator,
            IHandleTaskExceptionInfoStorage handleTaskExceptionInfoStorage,
            ITaskHandlerCollection taskHandlerCollection,
            IHandleTasksMetaStorage handleTasksMetaStorage,
            IIndexRecordsCleaner indexRecordsCleaner)
            : base(taskInfo.Item1)
        {
            this.taskInfo = taskInfo;
            this.taskCounter = taskCounter;
            this.serializer = serializer;
            this.remoteTaskQueue = remoteTaskQueue;
            this.handleTaskCollection = handleTaskCollection;
            this.remoteLockCreator = remoteLockCreator;
            this.handleTaskExceptionInfoStorage = handleTaskExceptionInfoStorage;
            this.taskHandlerCollection = taskHandlerCollection;
            this.handleTasksMetaStorage = handleTasksMetaStorage;
            this.indexRecordsCleaner = indexRecordsCleaner;
        }

        public override void Run()
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

        private bool TryUpdateTaskState(Task task, long? minimalStartTicks, long? startExecutingTicks, int attempts, TaskState state)
        {
            var metaForWrite = serializer.Copy(task.Meta);
            var ticks = minimalStartTicks != null ? minimalStartTicks.Value : 0;
            metaForWrite.MinimalStartTicks = Math.Max(metaForWrite.MinimalStartTicks, ticks) + 1;
            metaForWrite.StartExecutingTicks = startExecutingTicks;
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
                indexRecordsCleaner.RemoveMeta(task.Meta, taskInfo.Item2);
                return;
            }

            if(task.Meta.MinimalStartTicks != 0 && (task.Meta.MinimalStartTicks > DateTime.UtcNow.Ticks))
            {
                logger.InfoFormat("Другая очередь успела обработать задачу '{0}'", Id);
                return;
            }

            logger.InfoFormat("Начинаем обрабатывать задачу [{0}]", task.Meta);

            if(!TryUpdateTaskState(task, null, DateTime.UtcNow.Ticks, task.Meta.Attempts + 1, TaskState.InProcess))
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
                TryUpdateTaskState(task, null, task.Meta.StartExecutingTicks, task.Meta.Attempts, TaskState.Fatal);
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
                TryUpdateTaskState(task, null, task.Meta.StartExecutingTicks, task.Meta.Attempts, TaskState.Fatal);
            }
        }

        private void UpdateTaskMetaByHandleResult(Task task, HandleResult handleResult)
        {
            switch(handleResult.FinishAction)
            {
            case FinishAction.Finish:
                TryUpdateTaskState(task, null, task.Meta.StartExecutingTicks, task.Meta.Attempts, TaskState.Finished);
                break;
            case FinishAction.Fatal:
                TryUpdateTaskState(task, null, task.Meta.StartExecutingTicks, task.Meta.Attempts, TaskState.Fatal);
                LogError(handleResult.Error, task.Meta);
                break;
            case FinishAction.RerunAfterError:
                TryUpdateTaskState(task, handleResult.RerunDelay.Ticks + DateTime.UtcNow.Ticks,
                                   task.Meta.StartExecutingTicks, task.Meta.Attempts,
                                   TaskState.WaitingForRerunAfterError);
                LogError(handleResult.Error, task.Meta);
                break;
            case FinishAction.Rerun:
                TryUpdateTaskState(task, handleResult.RerunDelay.Ticks + DateTime.UtcNow.Ticks,
                                   task.Meta.StartExecutingTicks, task.Meta.Attempts, TaskState.WaitingForRerun);
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

        private static readonly ILog logger = LogManager.GetLogger(typeof(HandlerTask));
        private readonly IHandleTaskCollection handleTaskCollection;
        private readonly IRemoteLockCreator remoteLockCreator;
        private readonly IHandleTaskExceptionInfoStorage handleTaskExceptionInfoStorage;
        private readonly Tuple<string, ColumnInfo> taskInfo;
        private readonly ITaskCounter taskCounter;
        private readonly ISerializer serializer;
        private readonly IRemoteTaskQueue remoteTaskQueue;
        private readonly ITaskHandlerCollection taskHandlerCollection;
        private readonly IHandleTasksMetaStorage handleTasksMetaStorage;
        private readonly IIndexRecordsCleaner indexRecordsCleaner;
    }
}