using System;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;

using log4net;

namespace RemoteQueue.Cassandra.Repositories
{
    public class HandleTaskExceptionInfoStorage : IHandleTaskExceptionInfoStorage
    {
        public HandleTaskExceptionInfoStorage(ITaskExceptionInfoBlobStorage storage)
        {
            this.storage = storage;
            logger = LogManager.GetLogger(typeof(HandleTaskExceptionInfoStorage));
        }

        public void TryAddExceptionInfo(string taskId, Exception e)
        {
            try
            {
                storage.Write(taskId, new TaskExceptionInfo
                    {
                        ExceptionMessageInfo = e.ToString()
                    });
            }
            catch
            {
                logger.ErrorFormat("Не смогли записать ошибку для задачи '{0}'. {1}", taskId, e);
            }
        }

        public bool TryGetExceptionInfo(string taskId, out TaskExceptionInfo exceptionInfo)
        {
            exceptionInfo = storage.Read(taskId);
            return exceptionInfo != null;
        }

        public TaskExceptionInfo[] ReadExceptionInfosQuiet(string[] taskIds)
        {
            return storage.ReadQuiet(taskIds);
        }

        private readonly ITaskExceptionInfoBlobStorage storage;
        private readonly ILog logger;
    }
}