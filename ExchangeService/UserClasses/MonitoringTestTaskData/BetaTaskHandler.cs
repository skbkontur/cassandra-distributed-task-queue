using System;
using System.Threading;

using GroBuf;

using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class BetaTaskHandler : TaskHandler<BetaTaskData>
    {
        public BetaTaskHandler(ISerializer serializer, ITaskDataBlobStorage taskDataStorage)
        {
            this.serializer = serializer;
            this.taskDataStorage = taskDataStorage;
        }

        protected override HandleResult HandleTask(BetaTaskData taskData)
        {
            while(taskData.IsProcess)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(50));
                taskData = serializer.Deserialize<BetaTaskData>(taskDataStorage.Read(Context.TaskDataId));
            }
            return Finish();
        }

        private readonly ISerializer serializer;
        private readonly ITaskDataBlobStorage taskDataStorage;
    }
}