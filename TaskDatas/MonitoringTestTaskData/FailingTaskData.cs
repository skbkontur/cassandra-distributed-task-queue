using System;

using RemoteQueue.Configuration;
using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData
{
    [TaskName("FailingTaskData")]
    public class FailingTaskData : ITaskData
    {
        public Guid UniqueData { get; set; }

        public int RetryCount { get; set; }

        public override string ToString()
        {
            return string.Format("UniqueData: {0}", UniqueData);
        }
    }
}