using System;
using System.Threading;

using GroBuf;

using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Handling;

using RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class BetaTaskHandler : TaskHandler<BetaTaskData>
    {
        public BetaTaskHandler(ISerializer serializer, ITaskDataStorage taskDataStorage)
        {
            this.serializer = serializer;
            this.taskDataStorage = taskDataStorage;
        }

        protected override HandleResult HandleTask(BetaTaskData taskData)
        {
            while (taskData.IsProcess)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(50));
                var taskDataBytes = taskDataStorage.Read(Context);
                taskData = serializer.Deserialize<BetaTaskData>(taskDataBytes);
            }
            return Finish();
        }

        private readonly ISerializer serializer;
        private readonly ITaskDataStorage taskDataStorage;
    }
}