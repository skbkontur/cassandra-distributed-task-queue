using System;
using System.Threading;

using GroBuf;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.BlobStorages;
using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService.UserClasses.MonitoringTestTaskData
{
    public class BetaTaskHandler : RtqTaskHandler<BetaTaskData>
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